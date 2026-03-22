using Autodesk.Revit.UI;
using Firebase.Database;
using Nice3point.Revit.Toolkit.External;
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
            //_actionRemoveFileBak();
            //_initPannelPrimaDemo();
            //_intHtml();
            _Init();
        }
        public void _Init()
        {
            var panel = Application.CreatePanel("Tools", "Demo");
            
        }
        public override void OnShutdown()
        {
            _disposeUpdater();
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
                    .Where(x => x.Contains(fileName))
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
    public static class AppEntry { public static UIApplication UiApp; }
}