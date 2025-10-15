using Autodesk.Revit.DB.Mechanical;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.RevDuct;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.SkipWarning;
using RevitDevelop.Utils.Solids;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public partial class ConvertDuctToFlexAction : IConvertDuctToFlexAction
    {
        public void ExcuteOptionA()
        {
            var mechanicalSystemTypes = _document.GetElementsFromClass<MechanicalSystemType>(true);
            var flexDuctTypes = _document.GetElementsFromClass<FlexDuctType>(true);
            var fitting1 = _uiDocument.Selection.PickElement(_document, null, _filterElementEnd) as FamilyInstance;
            var fitting2 = _uiDocument.Selection.PickElement(_document, null, _filterElementEnd) as FamilyInstance;
            var elements = GetElementsByFittingToFitting(fitting1, fitting2);
            _validateElement(elements, out List<Element> elementsInvalid);
            var elementsTarget = new List<Element>();
            if (elementsInvalid.Any())
            {
                var elementsValids = elementsInvalid
                    .Select(x => GetElementsByFittingToFitting(fitting1, x as FamilyInstance))
                    .OrderBy(x => x.Count);
                elementsTarget = elementsValids.FirstOrDefault();
            }
            else
                elementsTarget = elements;
            var ductTarget = elementsTarget.FirstOrDefault(x => x is Duct);
            if (ductTarget != null)
            {
                using (var ts = new Transaction(_document, "new transaction"))
                {
                    ts.SkipAllWarnings();
                    ts.Start();
                    var connectors = elementsTarget.SortConnector();
                    var flexPoints = connectors.ConvertConnectorToPoint(2, 50);
                    var flex = (ductTarget as Duct).DuctToFlexDuct(
                        flexPoints,
                        mechanicalSystemTypes.FirstOrDefault(),
                        flexDuctTypes);
                    var solids = flex.GetSolids();
                    if (!solids.Any())
                    {
                        _document.Delete(flex.Id);
                        flexPoints.Reverse();
                        flex = (ductTarget as Duct).DuctToFlexDuct(
                        flexPoints,
                        mechanicalSystemTypes.FirstOrDefault(),
                        flexDuctTypes);
                    }
                    flex.ConnectAction();
                    _document.Delete(elementsTarget.Select(x => x.Id).ToList());
                    ts.Commit();
                }

            }
        }
    }
}
