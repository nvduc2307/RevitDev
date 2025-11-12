using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Updaters;
using RevitDevelop.Utils.Messages;

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
        public RibbonPanel PANEL_DRAWING { get; private set; }
        public RibbonPanel PANEL_SCHEDULE { get; private set; }
        public override void OnStartup()
        {
            _initPannelGeneral();
            _initPannelConcrete();
            _initPannelSteel();
            _initPannelRebar();
            _initPannelDrawing();
            _initPannelSchedule();
            _registerUpdater();
        }

        public override void OnShutdown()
        {
            _disposeUpdater();
        }
        private void _initPannelGeneral()
        {
            PANEL_GENERAL = Application.CreatePanel(Properties.Langs.ApplicationLangs.PANEL_GENERAL, Properties.Langs.ApplicationLangs.TAB);
            PANEL_GENERAL.AddPushButton<Test.TestCmd>("test")
                    .SetImage("/DPtools;component/Resources/Icons/RibbonIcon16.png")
                    .SetLargeImage("/DPtools;component/Resources/Icons/RibbonIcon32.png");
        }
        private void _initPannelConcrete()
        {
            PANEL_CONCRETE = Application.CreatePanel(Properties.Langs.ApplicationLangs.PANEL_CONCRETE, Properties.Langs.ApplicationLangs.TAB);
        }
        private void _initPannelSteel()
        {
            PANEL_STEEL = Application.CreatePanel(Properties.Langs.ApplicationLangs.PANEL_STEEL, Properties.Langs.ApplicationLangs.TAB);
        }
        private void _initPannelRebar()
        {
            PANEL_REBAR = Application.CreatePanel(Properties.Langs.ApplicationLangs.PANEL_REBAR, Properties.Langs.ApplicationLangs.TAB);

            PANEL_REBAR.AddPushButton<Tools.SettingRebars.SettingDiameters.SettingDiametersCmd>(Properties.Langs.ApplicationLangs.CMD_REBAR_SETTING_DIAMETER)
                .SetImage("/DPtools;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/DPtools;component/Resources/Icons/RibbonIcon32.png");
            PANEL_REBAR.AddPushButton<Test.TestCmd>("Edit By")
                .SetImage("/DPtools;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/DPtools;component/Resources/Icons/RibbonIcon32.png");
        }
        private void _initPannelDrawing()
        {
            PANEL_DRAWING = Application.CreatePanel(Properties.Langs.ApplicationLangs.DRAWING, Properties.Langs.ApplicationLangs.TAB);
        }
        private void _initPannelSchedule()
        {
            PANEL_SCHEDULE = Application.CreatePanel(Properties.Langs.ApplicationLangs.PANEL_SCHEDULE, Properties.Langs.ApplicationLangs.TAB);
        }

        private void _registerUpdater()
        {
            //WallEditByUpdater.Init(Application);
            //RebarModifyUpdater.Init(Application);
            //LapElementDeleteUpdater.Init(Application);
        }

        private void _disposeUpdater()
        {
            //WallEditByUpdater.Init(Application);
            //WallEditByUpdater.Dispose(Application);
            //RebarModifyUpdater.Dispose(Application);
            //LapElementDeleteUpdater.Dispose(Application);
        }

    }
}