using Autodesk.Revit.UI;
using Newtonsoft.Json;
using RevitDevelop.testHtml.views;

namespace RevitDevelop.testHtml.cores
{
    public class WebBridge : IExternalEventHandler
    {
        public UIApplication UiApp;
        public string Payload; // JSON

        public void Execute(UIApplication app)
        {
            //var cmd = JsonSerializer.Deserialize<Cmd>(Payload ?? "{}");
            var cmd = JsonConvert.DeserializeObject<Cmd>(Payload ?? "{}");
            var uidoc = app.ActiveUIDocument;
            if (uidoc == null || cmd == null) return;

            switch (cmd.Type)
            {
                case "countWalls":
                    int n = new FilteredElementCollector(uidoc.Document).OfClass(typeof(Wall)).ToElements().Count;
                    HtmlWindow.Current?.SendToJsAsync(new { type = "countWalls", count = n });
                    break;

                case "zoomTo":
                    using (var t = new Transaction(uidoc.Document, "ZoomTo"))
                    {
                        t.Start();
                        uidoc.ShowElements(new ElementId(cmd.ElementId));
                        t.Commit();
                    }
                    HtmlWindow.Current?.SendToJsAsync(new { type = "zoomTo", ok = true });
                    break;
            }
        }
        public string GetName() => "WebView2 Bridge";
        private class Cmd
        {
            public string Type;
            public int ElementId;
        };
    }
}
