using RevitDevelop.Utils.SelectFilters;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public partial class ConvertDuctToFlexAction : IConvertDuctToFlexAction
    {
        public void ExcuteOptionA()
        {
            var fitting1 = _uiDocument.Selection.PickElement(_document, null, _filterElementEnd) as FamilyInstance;
            var fitting2 = _uiDocument.Selection.PickElement(_document, null, _filterElementEnd) as FamilyInstance;
            var eles = GetElementsByFittingToFitting(fitting1, fitting2);
            _uiDocument.Selection.SetElementIds(eles.Select(x=>x.Id).ToList());
        }
    }
}
