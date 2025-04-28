using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;

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
            InitPanel();
            CreateRibbon();
        }
        private void InitPanel()
        {

        }

        private void CreateRibbon()
        {
            //PANEL_GENERAL.AddPushButton<StartupCommand>("Execute")
            //    .SetImage("/RevitDevelop;component/Resources/Icons/PluginIcon16.png")
            //    .SetLargeImage("/RevitDevelop;component/Resources/Icons/PluginIcon32.png");
        }
    }
}