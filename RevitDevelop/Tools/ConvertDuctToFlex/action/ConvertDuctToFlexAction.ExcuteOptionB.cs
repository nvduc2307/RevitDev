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
        public void ExcuteOptionB()
        {
            var mechanicalSystemTypes = _document.GetElementsFromClass<MechanicalSystemType>(true);
            var flexDuctTypes = _document.GetElementsFromClass<FlexDuctType>(true);
            var fitting1 = _uiDocument.Selection.PickElement(_document, null, _filterElementEnd) as FamilyInstance;
            var duct = _uiDocument.Selection.PickElement(_document, null, _filterRoundDuct) as Duct;
            var elements = duct.GetElementsByConnector(null, fitting1);
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
            elementsTarget = elementsTarget
                .SortElement(fitting1)
                .ToList();
            var elementsConvert = GetElementsByFittingToDistance(
                fitting1,
                elementsTarget,
                1000,
                out Duct ductlast,
                out XYZ ductLastPointStart,
                out XYZ ductLastPointIntersect,
                out XYZ ductLastPointEnd);
            var ductTarget = elementsConvert.FirstOrDefault(x => x is Duct);
            if (ductTarget != null)
            {
                using (var ts = new Transaction(_document, "new transaction"))
                {
                    ts.SkipAllWarnings();
                    ts.Start();
                    if (!elementsConvert.Any()) return;
                    if (!(elementsConvert.FirstOrDefault() is Duct))
                        elementsConvert.Remove(elementsConvert.FirstOrDefault());
                    if (!elementsConvert.Any()) return;
                    if (!(elementsConvert.LastOrDefault() is Duct))
                        elementsConvert.Remove(elementsConvert.LastOrDefault());
                    if (!elementsConvert.Any()) return;
                    var connectors = elementsConvert.SortConnector();
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
                    _document.Delete(elementsConvert.Select(x => x.Id).ToList());
                    ts.Commit();
                }
            }
        }
    }
}
