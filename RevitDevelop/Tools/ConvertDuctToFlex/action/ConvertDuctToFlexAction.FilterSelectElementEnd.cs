using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using RevitDevelop.Utils.RevCategories;
using RevitDevelop.Utils.RevDuct;
using RevitDevelop.Utils.SelectFilters;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public partial class ConvertDuctToFlexAction : IConvertDuctToFlexAction
    {
        private bool _filterElementEnd(Element element)
        {
            //validate type
            if (!(element is FamilyInstance familyInstance)) return false;
            //validate category
            var catesValid = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_MechanicalEquipment,
            };
            var cate = element.Category.ToBuiltinCategory();
            if (!catesValid.Any(x => x == cate))
                return false;
            //validate element connection
            //element connect is a round duct
            var connectors = element.GetConnectors();
            var connectorValid = connectors.Any(x =>
            {
                var result = false;
                foreach (var item in x.AllRefs)
                {
                    if (!(item is Connector connector)) continue;
                    if (connector.Owner.Id.ToString() == x.Owner.Id.ToString())
                        continue;
                    var ele = connector.Owner;
                    if (!(ele is Duct duct)) continue;
                    if (duct.DuctType.Shape != ConnectorProfileType.Round) continue;
                    result = true;
                    break;
                }
                return result;
            });
            return connectorValid;
        }
    }
}
