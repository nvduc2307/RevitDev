using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCategories;
using RevitDevelop.Utils.RevFaces;
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
            List<XYZ> ps,
            MechanicalSystemType systemType,
            List<FlexDuctType> flexDucTypes)
        {
            if (!flexDucTypes.Any()) return null;
            if (systemType == null) return null;
            var extent = 50.0.MmToFoot();
            var document = duct.Document;
            var location = duct.Location as LocationCurve;
            if (location == null) return null;
            var ductType = duct.DuctType.Shape;
            var flexDucType = flexDucTypes.FirstOrDefault(x => x.Shape == ductType);
            if (flexDucType == null) return null;
            var level = duct.LevelId;
            var qPs = ps.Count;
            var pStart = ps.FirstOrDefault();
            var pEnd = ps.LastOrDefault();
            var vtStart = (ps[1] - ps[0]).Normalize();
            var vtEnd = (ps[qPs - 1] - ps[qPs - 2]).Normalize();
            var fd = FlexDuct.Create(document, systemType.Id, flexDucType.Id, level, ps);
            fd.StartTangent = vtStart * extent * 2;
            fd.EndTangent = vtEnd * extent * 2;
            switch (ductType)
            {
                case ConnectorProfileType.Invalid:
                    break;
                case ConnectorProfileType.Round:
                    fd.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).Set(duct.Diameter);
                    break;
                case ConnectorProfileType.Rectangular:
                    fd.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).Set(duct.Height);
                    fd.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).Set(duct.Width);
                    break;
                case ConnectorProfileType.Oval:
                    break;
            }
            return fd;
        }
        public static List<XYZ> ConvertConnectorToPoint(
            this List<Connector> connectors, 
            int numDot, 
            double spacingMm)
        {
            var result = new List<XYZ>();
            if (connectors.Count % 2 != 0)
                return result;
            if (connectors.GroupBy(x => x.Owner.Id.ToString()).Any(x => x.Count() != 2))
                return result;
            var qConnectors = connectors.Count;
            for (int i = 0; i < qConnectors; i += 2)
            {
                if (connectors[i].Owner.Id.ToString() != connectors[i + 1].Owner.Id.ToString())
                    return result;
                var distance = connectors[i].Origin.DistanceTo(connectors[i + 1].Origin).FootToMm();
                var vt = (connectors[i + 1].Origin - connectors[i].Origin).Normalize();
                result.Add(connectors[i].Origin);
                if (distance >= numDot * 2 * spacingMm)
                {
                    if (connectors[i].Owner is Duct)
                    {
                        for (int j = 0; j < numDot; j++)
                        {
                            result.Add(connectors[i].Origin + (j + 1) * vt * spacingMm.MmToFoot());
                        }
                        for (int j = numDot - 1; j >= 0; j--)
                        {
                            result.Add(connectors[i + 1].Origin - (j + 1) * vt * spacingMm.MmToFoot());
                        }
                    }
                }
                result.Add(connectors[i + 1].Origin);
            }
            return result.Distinct(new ComparePoint()).ToList();
        }
        public static List<Connector> SortConnector(this List<Element> elements)
        {
            var connectors =
                elements.Select(x => x.GetConnectors())
                .Aggregate((a, b) => a.Concat(b).ToList())
                .ToList();
            var qConnector = connectors.Count;
            Connector connectorStart = null;
            Connector connectorEnd = null;
            for (int i = 0; i < qConnector; i++)
            {
                var connect = connectors.Where(x => x.Owner.Id.ToString() != connectors[i].Owner.Id.ToString())
                    .FirstOrDefault(x => x.Origin.IsSame(connectors[i].Origin));
                if (connect == null)
                {
                    connectorStart = connectors[i];
                    connectorEnd = connectors.Where(x => x.Owner.Id.ToString() == connectorStart.Owner.Id.ToString())
                        .FirstOrDefault(x => x.Id != connectorStart.Id);
                    break;
                }
            }
            if (connectorStart == null)
                return new List<Connector>();
            if (connectorEnd == null)
                return new List<Connector>();
            var results = new List<Connector>();
            var elementStart = connectorStart.Owner;
            results.Add(connectorStart);
            results.Add(connectorEnd);
            var isDo = true;
            do
            {
                try
                {
                    var connectorsStart = connectors.Where(x => x.Owner.Id.ToString() != connectorStart.Owner.Id.ToString());
                    if (!connectorsStart.Any())
                        throw new Exception();
                    var connectorStartAdd = connectorsStart
                        .FirstOrDefault(x => x.Origin.IsSame(connectorEnd.Origin));
                    if (connectorStartAdd == null)
                        throw new Exception();
                    var connectorsEnd = connectors.Where(x => x.Owner.Id.ToString() == connectorStartAdd.Owner.Id.ToString());
                    if (!connectorsEnd.Any())
                        throw new Exception();
                    var connectorEndAdd = connectorsEnd
                        .FirstOrDefault(x => x.Id != connectorStartAdd.Id);
                    results.Add(connectorStartAdd);
                    results.Add(connectorEndAdd);
                    connectorStart = connectorStartAdd;
                    connectorEnd = connectorEndAdd;
                }
                catch (Exception)
                {
                    isDo = false;
                }
            } while (isDo);
            return results;
        }
        public static List<Element> SortElement(this List<Element> elements, Element elementstart)
        {
            var connectorsCheck = elementstart.GetConnectors();
            var connectors = elements.SortConnector();
            var results = connectors
                .Select(x => x.Owner)
                .GroupBy(x => x.Id.ToString())
                .Select(x => x.FirstOrDefault())
                .ToList();
            if (connectorsCheck.Any())
            {
                var connectorStart = connectors.FirstOrDefault();
                var connectorEnd = connectors.LastOrDefault();
                var distanceStart = connectorsCheck.Min(x => x.Origin.DistanceTo(connectorStart.Origin));
                var distanceEnd = connectorsCheck.Min(x => x.Origin.DistanceTo(connectorEnd.Origin));
                if (distanceEnd < distanceStart)
                    results.Reverse();
            }
            return results;
        }
        public static List<Element> GetElementsBySolid(
            this Element element,
            List<BuiltInCategory> builtInCategoriesValid = null,
            double extentMm = 40,
            Element elementIgnore = null)
        {
            var document = element.Document;
            var result = new List<Element>() { element };
            var q = result.Count;
            builtInCategoriesValid = builtInCategoriesValid == null
            ? new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_DuctAccessory,
            }
            : builtInCategoriesValid;
            var extent = extentMm.MmToFoot();
            for (int i = 0; i < q; i++)
            {
                var connectors = result[i].GetConnectors();
                if (!connectors.Any()) continue;
                foreach (var connector in connectors)
                {
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
                    var elements = new FilteredElementCollector(document)
                    .WhereElementIsNotElementType()
                    .WherePasses(bbFilter)
                    .Where(x =>
                    {
                        var cate = x.Category.BuiltInCategory;
                        if (builtInCategoriesValid.Any(y => y == x.Category.BuiltInCategory))
                            return true;
                        return false;
                    })
                    .Where(x => x.GetConnectors().Any(y => y.Origin.IsSame(connector.Origin)))
                    .Where(x=> result.Any(y =>y.Id.ToString() != x.Id.ToString()))
                    .ToList();
                    if (elements.Any())
                    {
                        result.AddRange(elements);
                        q += q + elements.Count;
                    }
                }
            }
            return result;
        }
        public static List<Element> GetElementsByConnector(
            this Element element,
            List<BuiltInCategory> builtInCategoriesValid = null,
            Element elementIgnore = null)
        {
            var result = new List<Element>() { element };
            var q = result.Count;
            builtInCategoriesValid = builtInCategoriesValid == null
            ? new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_DuctAccessory,
            }
            : builtInCategoriesValid;
            for (int i = 0; i < q; i++)
            {
                var connectors = result[i].GetConnectors();
                if (!connectors.Any()) continue;
                foreach (var connector in connectors)
                {
                    foreach (var obj in connector.AllRefs)
                    {
                        if (!(obj is Connector conRef))
                            continue;
                        var buildCat = conRef.Owner.Category.BuiltInCategory;
                        if (!builtInCategoriesValid.Any(x => x == buildCat))
                            continue;
                        if (result.Any(x => x.Id.ToString() == conRef.Owner.Id.ToString()))
                            continue;
                        if (elementIgnore != null)
                        {
                            if (conRef.Owner.Id.ToString() == elementIgnore.Id.ToString())
                                continue;
                        }
                        result.Add(conRef.Owner);
                        q++;
                    }
                }
            }
            return result;
        }
        public static List<Connector> GetConnectors(this Element element)
        {
            var result = new List<Connector>();
            if (element == null) return result;
            var cate = element.Category.ToBuiltinCategory();
            if (cate == BuiltInCategory.OST_FlexDuctCurves)
                result = GetConnectorsByFlexDuct(element as FlexDuct);
            else if (cate == BuiltInCategory.OST_DuctCurves)
                result = GetConnectorsByDuct(element as Duct);
            else if (cate == BuiltInCategory.OST_DuctFitting)
                result = GetConnectorsByDuctFitting(element as FamilyInstance);
            else if (cate == BuiltInCategory.OST_DuctAccessory)
                result = GetConnectorsByDuctAccessories(element as FamilyInstance);
            else if (cate == BuiltInCategory.OST_MechanicalEquipment)
                result = GetConnectorsByEquipment(element as FamilyInstance);
            return result;
        }
        public static List<Connector> GetConnectorsByFlexDuct(this FlexDuct flexDuct)
        {
            var result = new List<Connector>();
            var connectorManager = flexDuct.ConnectorManager.Connectors;
            foreach (var item in flexDuct.ConnectorManager.Connectors)
            {
                if (item is Connector connector)
                    result.Add(connector);
            }
            return result;
        }
        public static List<Connector> GetConnectorsByDuct(this Duct duct)
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
        public static List<Connector> GetConnectorsByDuctFitting(this FamilyInstance ductFitting)
        {
            var result = new List<Connector>();
            var buildCate = ductFitting.Category.BuiltInCategory;
            if (buildCate != BuiltInCategory.OST_DuctFitting)
                return result;
            var mepModel = ductFitting.MEPModel;
            foreach (var item in mepModel.ConnectorManager.Connectors)
            {
                if (item is Connector connector)
                    result.Add(connector);
            }
            return result;
        }
        public static List<Connector> GetConnectorsByDuctAccessories(this FamilyInstance ductAccsessories)
        {
            var result = new List<Connector>();
            var buildCate = ductAccsessories.Category.BuiltInCategory;
            if (buildCate != BuiltInCategory.OST_DuctAccessory) return result;
            var mepModel = ductAccsessories.MEPModel;
            foreach (var item in mepModel.ConnectorManager.Connectors)
            {
                if (item is Connector connector)
                    result.Add(connector);
            }
            return result;
        }
        public static List<Connector> GetConnectorsByEquipment(this FamilyInstance equipment)
        {
            var result = new List<Connector>();
            var buildCate = equipment.Category.BuiltInCategory;
            if (buildCate != BuiltInCategory.OST_MechanicalEquipment)
                return result;
            var mepModel = equipment.MEPModel;
            foreach (var item in mepModel.ConnectorManager.Connectors)
            {
                if (item is Connector connector)
                    result.Add(connector);
            }
            return result;
        }
        public static List<XYZ> GetMepGeometry(this Element element, out XYZ midPoint)
        {
            midPoint = null;
            var result = new List<XYZ>();
            var connectors = element.GetConnectors();
            if (connectors.Count != 2)
                return result;
            var cate = element.Category.BuiltInCategory;
            if (cate == BuiltInCategory.OST_DuctCurves)
            {
                var ps = connectors.Select(x => x.Origin).ToList();
                midPoint = ps.CenterPoint();
                return ps;
            }
            if (cate == BuiltInCategory.OST_DuctAccessory)
            {
                var ps = connectors.Select(x => x.Origin).ToList();
                midPoint = ps.CenterPoint();
                return ps;
            }
            if (cate == BuiltInCategory.OST_DuctFitting)
            {
                if (!(element is FamilyInstance fa))
                    return result;
                var mep = fa.MEPModel;
                if (mep == null) return result;
                if (!(mep is MechanicalFitting mechanical))
                    return result;
                if (mechanical.PartType == PartType.Elbow)
                {
                    var con1 = connectors.FirstOrDefault();
                    var con2 = connectors.LastOrDefault();
                    var f = new FaceCustom(con1.CoordinateSystem.BasisX, con1.Origin);
                    var pInterSect = con2.Origin.RayPointToFace(con2.CoordinateSystem.BasisZ, f);
                    if (pInterSect == null)
                        return result;
                    result.Add(con1.Origin);
                    result.Add(pInterSect);
                    result.Add(con2.Origin);
                    midPoint = pInterSect;
                }
                else if (mechanical.PartType == PartType.Offset)
                {
                    var ps = connectors.Select(x => x.Origin).ToList();
                    midPoint = ps.CenterPoint();
                    return ps;
                }
            }
            return result;
        }
        public static void ConnectAction(this FlexDuct flexDuct)
        {
            var document = flexDuct.Document;
            var flexConnectors = GetConnectors(flexDuct);
            foreach (var flexConnector in flexConnectors)
            {
                try
                {
                    if (flexConnector.IsConnected)
                        continue;
                    var extent = (40.0).MmToFoot();
                    var pMin = flexConnector.Origin
                            - XYZ.BasisX * extent / 2
                            - XYZ.BasisY * extent / 2
                            - XYZ.BasisZ * extent / 2;
                    var pMax = flexConnector.Origin
                        + XYZ.BasisX * extent / 2
                        + XYZ.BasisY * extent / 2
                        + XYZ.BasisZ * extent / 2;
                    var outLine = new Outline(pMin, pMax);
                    var bbFilter = new BoundingBoxIntersectsFilter(outLine);
                    var elementConnect = new FilteredElementCollector(document)
                    .WhereElementIsNotElementType()
                    .WherePasses(bbFilter)
                    .FirstOrDefault(x => x.GetConnectors().Any(y => y.Origin.IsSame(flexConnector.Origin)));
                    if (elementConnect == null)
                        continue;
                    var connector = elementConnect
                        .GetConnectors()
                        .FirstOrDefault(x => x.Origin.IsSame(flexConnector.Origin));
                    if (connector == null) continue;
                    flexConnector.ConnectTo(connector);
                }
                catch (System.Exception)
                {
                }
            }
        }
    }
}
