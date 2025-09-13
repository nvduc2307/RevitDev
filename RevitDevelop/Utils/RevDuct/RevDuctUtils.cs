using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevParameters;
using RevitDevelop.Utils.SkipWarning;
using RevitDevelop.Utils.Solids;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace RevitDevelop.Utils.RevDuct
{
    public static class RevDuctUtils
    {
        public static FlexDuct DuctToFlexDuct(
            this Duct duct,
            MechanicalSystemType systemType,
            List<FlexDuctType> flexDucTypes)
        {
            if (!flexDucTypes.Any()) return null;
            if (systemType == null) return null;
            var extent = 50.MmToFoot();
            var document = duct.Document;
            var location = duct.Location as LocationCurve;
            if (location == null) return null;
            var ductType = duct.DuctType.Shape;
            var curve = location.Curve as Curve;
            var p1 = curve.GetEndPoint(0);
            var p2 = curve.GetEndPoint(1);
            var dir = (p2 - p1).Normalize();
            var ps = new List<XYZ>() { p1, p2 };
            var h1 = p1 + dir * extent;
            var h2 = p2 - dir * extent;
            var flexDucType = flexDucTypes.FirstOrDefault(x => x.Shape == ductType);
            if (flexDucType == null) return null;
            var level = duct.LevelId;
            var fd = FlexDuct.Create(document, systemType.Id, flexDucType.Id, level, ps);
            fd.StartTangent = h1 - p1;
            fd.EndTangent = p2 - h2;
            switch (ductType)
            {
                case ConnectorProfileType.Invalid:
                    break;
                case ConnectorProfileType.Round:
                    fd.SetParameterValue(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM, $"{duct.Diameter}");
                    break;
                case ConnectorProfileType.Rectangular:
                    fd.SetParameterValue(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM, $"{duct.Height}");
                    fd.SetParameterValue(BuiltInParameter.RBS_CURVE_WIDTH_PARAM, $"{duct.Width}");
                    break;
                case ConnectorProfileType.Oval:
                    break;
            }
            return fd;
        }
        public static List<Element> GetGroupDuct(this Element ele)
        {
            var result = new List<Element>() { ele };
            var elementsTarget = new List<Element>() { ele };
            var qelementsTarget = elementsTarget.Count;
            for (int i = 0; i < qelementsTarget; i++)
            {
                if (elementsTarget[i].GetType() != typeof(Duct) && elementsTarget[i].GetType() != typeof(FamilyInstance))
                    continue;
                var document = elementsTarget[i].Document;
                var connectors = elementsTarget[i].GetType() == typeof(Duct)
                    ? (elementsTarget[i] as Duct).GetConnectors()
                    : (elementsTarget[i] as FamilyInstance).GetConnectors();
                if (!connectors.Any()) continue;
                var elesAdd = new List<Element>();
                foreach (var connector in connectors)
                {
                    var els = GetGroupDuct(document, connector)
                        .Where(x => !elementsTarget.Any(y => y.Id.ToString() == x.Id.ToString()));
                    if (els.Any())
                        elesAdd.AddRange(els);
                }
                if (elesAdd.Any())
                {
                    elementsTarget.AddRange(elesAdd);
                    qelementsTarget = elementsTarget.Count;
                }
            }
            result = elementsTarget.ToList();
            return result;
        }
        public static List<Element> GetGroupDuct(this Document document, Connector connector)
        {
            var result = new List<Element>();
            var extent = 40.MmToFoot();
            var pMin = connector.Origin
                    - XYZ.BasisX * extent / 2
                    - XYZ.BasisY * extent / 2
                    - XYZ.BasisZ * extent / 2;
            var pMax = connector.Origin
                + XYZ.BasisX * extent / 2
                + XYZ.BasisY * extent / 2
                + XYZ.BasisZ * extent / 2;
            var outLine = new Outline(pMin, pMax);
            var bbFilter = new BoundingBoxIntersectsFilter(outLine);
            result = new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .WherePasses(bbFilter)
            .Where(x => x.Category.BuiltInCategory == BuiltInCategory.OST_DuctCurves || x.Category.BuiltInCategory == BuiltInCategory.OST_DuctFitting)
            .Where(x=>x.GetConnectors().Any(y=>y.Origin.IsSame(connector.Origin)))
            .ToList();
            return result;
        }
        public static List<Connector> GetConnectors(this Element element)
        {
            var result = new List<Connector>();
            var dk1 = element.Category.BuiltInCategory == BuiltInCategory.OST_DuctCurves;
            var dk2 = element.Category.BuiltInCategory == BuiltInCategory.OST_DuctFitting;
            var dk3 = element.Category.BuiltInCategory == BuiltInCategory.OST_DuctAccessory;
            if (!dk1 && !dk2 && !dk3)
                return result;
            result = element is Duct
                ? (element as Duct).GetConnectors()
                : (element as FamilyInstance).GetConnectors();
            return result;
        }
        public static List<Connector> GetConnectors(this Duct duct)
        {
            var result = new List<Connector>();
            var connectorManager = duct.ConnectorManager.Connectors;
            foreach (var item in duct.ConnectorManager.Connectors)
            {
                if (item is Connector connector)
                    result.Add(connector);
            }
            return result;
        }
        public static List<Connector> GetConnectors(this FamilyInstance ductAccessories)
        {
            var result = new List<Connector>();
            var buildCate = ductAccessories.Category.BuiltInCategory;
            if (buildCate == BuiltInCategory.OST_DuctAccessory || buildCate == BuiltInCategory.OST_DuctFitting)
            {
                var mepModel = ductAccessories.MEPModel;
                foreach (var item in mepModel.ConnectorManager.Connectors)
                {
                    if (item is Connector connector)
                        result.Add(connector);
                }
                return result;
            }
            return result;
        }
    }
}
