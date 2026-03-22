using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using RevitDevelop.Utils;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.Geometries;

namespace RevitDevelop.Utils
{
    public static class PipeUtils
    {
        public static FlexPipe DuctToFlexPipe(
            this Pipe pipe,
            List<XYZ> ps,
            List<FlexPipeType> flexDucTypes)
        {
            if (!flexDucTypes.Any()) return null;
            var extent = 50.0.MmToFoot();
            var document = pipe.Document;
            var location = pipe.Location as LocationCurve;
            if (location == null) return null;
            var ductType = pipe.PipeType.Shape;
            var flexDucType = flexDucTypes.FirstOrDefault(x => x.Shape == ductType);
            if (flexDucType == null) return null;
            var level = pipe.LevelId;
            var qPs = ps.Count;
            var pStart = ps.FirstOrDefault();
            var pEnd = ps.LastOrDefault();
            var vtStart = (ps[1] - ps[0]).Normalize();
            var vtEnd = (ps[qPs - 1] - ps[qPs - 2]).Normalize();
            var systemTypeId = pipe.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM).AsElementId();
            var fd = FlexPipe.Create(document, systemTypeId, flexDucType.Id, level, ps);
            fd.StartTangent = vtStart * extent * 2;
            fd.EndTangent = vtEnd * extent * 2;
            switch (ductType)
            {
                case ConnectorProfileType.Invalid:
                    break;
                case ConnectorProfileType.Round:
                    fd.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(pipe.Diameter);
                    break;
                case ConnectorProfileType.Rectangular:
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
                    if (connectors[i].Owner is Pipe)
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
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_PipeFitting,
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
        
        public static List<Connector> GetConnectorsByFlexPipe(this FlexPipe flexDuct)
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
        public static List<Connector> GetConnectorsByPipe(this Pipe pipe)
        {
            var result = new List<Connector>();
            var connectorManager = pipe.ConnectorManager.Connectors;
            foreach (var item in pipe.ConnectorManager.Connectors)
            {
                if (item is Connector connector)
                    result.Add(connector);
            }
            return result;
        }
        public static List<Connector> GetConnectorsByPipeFitting(this FamilyInstance pipeFitting)
        {
            var result = new List<Connector>();
            var buildCate = pipeFitting.Category.BuiltInCategory;
            if (buildCate != BuiltInCategory.OST_PipeFitting)
                return result;
            var mepModel = pipeFitting.MEPModel;
            foreach (var item in mepModel.ConnectorManager.Connectors)
            {
                if (item is Connector connector)
                    result.Add(connector);
            }
            return result;
        }
        public static XYZ GetDirection(this Pipe pipe)
        {
            if (pipe == null) return null;
            var location = pipe.Location as LocationCurve;
            if (location == null) return null;
            var dir = (location.Curve.GetEndPoint(1) - location.Curve.GetEndPoint(0)).Normalize();
            return dir;
        }
        public static XYZ GetStart(this Pipe pipe)
        {
            if (pipe == null) return null;
            var location = pipe.Location as LocationCurve;
            if (location == null) return null;
            return location.Curve.GetEndPoint(0);
        }
        public static XYZ GetEnd(this Pipe pipe)
        {
            if (pipe == null) return null;
            var location = pipe.Location as LocationCurve;
            if (location == null) return null;
            return location.Curve.GetEndPoint(1);
        }
    }
}
