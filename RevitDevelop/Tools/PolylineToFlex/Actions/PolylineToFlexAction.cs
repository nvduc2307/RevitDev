using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using RevitDevelop.Tools.PolylineToFlex.IActions;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevDuct;
using RevitDevelop.Utils.RevEvents;
using RevitDevelop.Utils.SkipWarning;

namespace RevitDevelop.Tools.PolylineToFlex.Actions
{
    public class PolylineToFlexAction : IPolylineToFlexAction
    {
        private PolylineToFlexCommand _cmd;
        private UIApplication _app;
        private UIDocument _uiDocument;
        private Document _document;
        private RevitCommandEndedMonitor _escEvent;
        private List<ModelCurve> _modelLines;
        private FlexDuctType _flexDuctType;
        private MechanicalSystemType _systemType;
        private double _flexDuctDiamterMm;
        private Level _level;
        public PolylineToFlexAction(PolylineToFlexCommand cmd)
        {
            _cmd = cmd;
            _uiDocument = cmd.UiDocument;
            _document = cmd.Document;
            _app = cmd.UiApplication;
            _modelLines = new List<ModelCurve>();
        }

        public void Excute(
            FlexDuctType flexDuctType, 
            MechanicalSystemType systemType, 
            double flexDuctDiamterMm,
            Level level)
        {
            _flexDuctType = flexDuctType;
            _systemType = systemType;
            _flexDuctDiamterMm = flexDuctDiamterMm;
            _level = level;
            _escEvent = new RevitCommandEndedMonitor(_app);
            _escEvent.CommandEnded += EscEvent_CommandEnded;
            _app.Application.DocumentChanged += Application_DocumentChanged;
            var commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.ModelLine);
            var canExecute = _app.CanPostCommand(commandId);
            if (canExecute) _app.PostCommand(commandId);
        }

        private void EscEvent_CommandEnded(object sender, EventArgs e)
        {
            _app.Application.DocumentChanged -= Application_DocumentChanged;
            _escEvent.CommandEnded -= EscEvent_CommandEnded;
            //get points of polylines
            var psGr = _modelLines
                .Select(x => x.GetPoints())
                .Aggregate((a,b)=>a.Concat(b).ToList())
                .GroupBy(x=>x, new ComparePoint())
                .Where(x=>x.Count() == 1)
                .Select(x=>x.FirstOrDefault())
                .ToList();

            using (var ts = new Transaction(_document, "new transaction"))
            {
                ts.SkipAllWarnings();
                ts.Start();
                var flexDuct = _document.CreateFlexDuct(_flexDuctType, _systemType, _level, psGr);
                flexDuct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).Set(_flexDuctDiamterMm.MmToFoot());
                ts.Commit();
            }

        }

        private void Application_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            var eles = e.GetAddedElementIds()
                .Select(x=>_document.GetElement(x))
                .Where(x=>x is ModelCurve)
                .Cast<ModelCurve>()
                .ToList();
            if (eles.Any())
                _modelLines.AddRange(eles);
        }
    }
}
