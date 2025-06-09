using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Updaters;

namespace RevitDevelop
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public RibbonPanel PANEL_GENERAL { get; private set; }
        public RibbonPanel PANEL_CONCRETE { get; private set; }
        public RibbonPanel PANEL_STEEL { get; private set; }
        public RibbonPanel PANEL_REBAR { get; private set; }
        public RibbonPanel PANEL_SCHEDULE { get; private set; }
        public override void OnStartup()
        {
            _registerUpdater();
        }
        public override void OnShutdown()
        {
            _disposeUpdater();
        }

        private void _registerUpdater()
        {
            RebarModifyUpdater.Init(Application);
        }
        private void _disposeUpdater()
        {
            RebarModifyUpdater.Dispose(Application);
        }

    }
}