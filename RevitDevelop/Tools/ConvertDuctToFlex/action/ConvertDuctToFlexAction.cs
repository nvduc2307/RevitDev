using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public partial class ConvertDuctToFlexAction : IConvertDuctToFlexAction
    {
        private UIDocument _uiDocument;
        private Document _document;
        public ConvertDuctToFlexAction(UIDocument uiDocument)
        {
            _uiDocument = uiDocument;
            _document = _uiDocument.Document;
        }
        public void Excute()
        {
        }
    }
}
