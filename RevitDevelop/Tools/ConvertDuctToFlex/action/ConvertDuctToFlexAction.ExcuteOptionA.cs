using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.SelectFilters;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public partial class ConvertDuctToFlexAction : IConvertDuctToFlexAction
    {
        public void ExcuteOptionA()
        {
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
            _uiDocument.Selection.SetElementIds(elementsTarget.Select(x => x.Id).ToList());
        }
    }
}
