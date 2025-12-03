using Autodesk.Revit.UI;
using Firebase.Database;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Updaters;
using RevitDevelop.Utils.FireBaseListeners;
using RevitDevelop.Utils.Messages;
using System.IO;

namespace RevitDevelop
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        private string firebaseUrl = "https://revitsyncnotifier-default-rtdb.asia-southeast1.firebasedatabase.app/";
        public static FirebaseClient Client;
        public RibbonPanel PANEL_GENERAL { get; private set; }
        public RibbonPanel PANEL_CONCRETE { get; private set; }
        public RibbonPanel PANEL_STEEL { get; private set; }
        public RibbonPanel PANEL_REBAR { get; private set; }
        public RibbonPanel PANEL_DRAWING { get; private set; }
        public RibbonPanel PANEL_SCHEDULE { get; private set; }
        public override void OnStartup()
        {
            //Client = new FirebaseClient(firebaseUrl);
            //FireBaseListener.ListenRequest(Client);
            //Application.ControlledApplication.DocumentSynchronizedWithCentral 
            //    += ControlledApplication_DocumentSynchronizedWithCentralAsync;
            //_initPannelGeneral();
            //_initPannelConcrete();
            //_initPannelSteel();
            //_initPannelRebar();
            //_initPannelDrawing();
            //_initPannelSchedule();
            //_registerUpdater();
            _actionRemoveFileBak();
            _initPannelPrimaDemo();
        }

        private void ControlledApplication_DocumentSynchronizedWithCentralAsync(object sender, Autodesk.Revit.DB.Events.DocumentSynchronizedWithCentralEventArgs e)
        {
            Task.Run(async () => { await FireBaseListener.SendRequest("abc", Client); });
        }

        public override void OnShutdown()
        {
            _disposeUpdater();
        }
        private void _initPannelPrimaDemo()
        {
            var panel = Application.CreatePanel("Tools", "Prima Demo");
            panel.AddPushButton<DemoST.Command1.Command1Cmd>("Command1")
            .SetImage("/DPtools;component/Resources/Icons/RibbonIcon16.png")
            .SetLargeImage("/DPtools;component/Resources/Icons/RibbonIcon32.png");

            panel.AddPushButton<DemoST.Command3.Command3Cmd>("Command3")
            .SetImage("/DPtools;component/Resources/Icons/RibbonIcon16.png")
            .SetLargeImage("/DPtools;component/Resources/Icons/RibbonIcon32.png");
        }
        private void _initPannelGeneral()
        {
            PANEL_GENERAL = Application.CreatePanel(Properties.Langs.ApplicationLangs.PANEL_GENERAL, Properties.Langs.ApplicationLangs.TAB);
            PANEL_GENERAL.AddPushButton<Test.EditFlexPipeCommand>("test")
                    .SetImage("/DPtools;component/Resources/Icons/RibbonIcon16.png")
                    .SetLargeImage("/DPtools;component/Resources/Icons/RibbonIcon32.png");
            PANEL_GENERAL.AddPushButton<Tools.Syns.SynCommand>("SynCommand")
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
            PANEL_REBAR.AddPushButton<Test.EditFlexPipeCommand>("Edit By")
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
        private void _actionRemoveFileBak()
        {
            Application.ControlledApplication.DocumentSaved += ControlledApplication_DocumentSaved;
        }

        private void ControlledApplication_DocumentSaved(object sender, Autodesk.Revit.DB.Events.DocumentSavedEventArgs e)
        {
            if (sender is Autodesk.Revit.ApplicationServices.Application app)
            {
                var document = e.Document;
                var path = document.PathName;
                if (path == string.Empty)
                    return;
                var pathFolders = path.Split('\\').ToList();
                var fileName = pathFolders
                    .LastOrDefault().Split('.')
                    .FirstOrDefault();
                pathFolders.RemoveAt(pathFolders.Count - 1);
                var dir = pathFolders
                    .Aggregate((a, b) => $"{a}\\{b}");
                if (!System.IO.Directory.Exists(dir))
                    return;
                var files = System.IO.Directory.GetFiles(dir)
                    .Where(x=>x.Contains(fileName))
                    .Where(x =>
                    {
                        var names = x.Split('\\').LastOrDefault().Split('.').ToList();
                        return names.Count > 2;
                    })
                    .ToList();
                foreach (var item in files)
                {
                    try
                    {
                        if (File.Exists(item))
                            File.Delete(item);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
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