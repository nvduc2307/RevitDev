using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using RevitDevelop.Utils.NumberUtils;

namespace RevitDevelop.DaitoTest.Utils
{
    public static class ConnectorUtils
    {
        public static bool IsElementConnect(this Element elementsSt, Element elementE)
        {
            var connector1s = elementsSt.GetConnectors();
            var connector2s = elementE.GetConnectors();
            if (!connector1s.Any()) return false;
            if (!connector2s.Any()) return false;
            foreach (var connector in connector1s)
            {
                var isSame = connector2s.Any(x => x.Origin.DistanceTo(connector.Origin).FootToMm() <= 1);
                if (isSame)
                    return true;
            }
            return false;
        }
        public static List<Connector> GetConnectors(this Element element)
        {
            var results = new List<Connector>();
            if (element is Pipe pipe)
            {
                results.AddRange(GetConnectors(pipe));
                return results;
            }
            if (element is FlexPipe flexPipe)
            {
                results.AddRange(GetConnectors(flexPipe));
                return results;
            }
            if (element is Duct duct)
            {
                results.AddRange(GetConnectors(duct));
                return results;
            }
            if (element is FlexDuct flexDuct)
            {
                results.AddRange(GetConnectors(flexDuct));
                return results;
            }
            if (element is FamilyInstance familyInstance)
            {
                results.AddRange(GetConnectors(familyInstance));
                return results;
            }
            return results;
        }
        public static List<Connector> GetConnectors(this Pipe pipe)
        {
            var results = new List<Connector>();
            var connectorM = pipe.ConnectorManager;
            foreach (var item in connectorM.Connectors)
            {
                if (item is Connector connector)
                    results.Add(connector);
            }
            return results;
        }
        public static List<Connector> GetConnectors(this FlexPipe flexPipe)
        {
            var results = new List<Connector>();
            var connectorM = flexPipe.ConnectorManager;
            foreach (var item in connectorM.Connectors)
            {
                if (item is Connector connector)
                    results.Add(connector);
            }
            return results;
        }
        public static List<Connector> GetConnectors(this Duct duct)
        {
            var results = new List<Connector>();
            var connectorM = duct.ConnectorManager;
            foreach (var item in connectorM.Connectors)
            {
                if (item is Connector connector)
                    results.Add(connector);
            }
            return results;
        }
        public static List<Connector> GetConnectors(this FlexDuct flexDuct)
        {
            var results = new List<Connector>();
            var connectorM = flexDuct.ConnectorManager;
            if (connectorM == null) return results;
            foreach (var item in connectorM.Connectors)
            {
                if (item is Connector connector)
                    results.Add(connector);
            }
            return results;
        }
        public static List<Connector> GetConnectors(
            this FamilyInstance familyInstance,
            List<BuiltInCategory> cateFittings = null)
        {
            var results = new List<Connector>();
            if (cateFittings == null)
            {
                cateFittings = new List<BuiltInCategory>() {
                    BuiltInCategory.OST_DuctFitting,
                    BuiltInCategory.OST_DuctAccessory,
                    BuiltInCategory.OST_PipeFitting,
                    BuiltInCategory.OST_PipeAccessory,
                };
            }
            if (familyInstance == null) return results;
            if (!cateFittings.Any(x => x == familyInstance.Category.BuiltInCategory))
                return results;
            var mep = familyInstance.MEPModel;
            if (mep == null) return results;
            var connectorM = mep.ConnectorManager;
            if (connectorM == null) return results;
            foreach (var item in connectorM.Connectors)
            {
                if (item is Connector connector)
                    results.Add(connector);
            }
            return results;
        }
    }
}
