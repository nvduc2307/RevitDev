using System.IO;

namespace RevitDevelop.Utils.BrowserNodes
{
    public static class BrowserNodeUtils
    {
        public static BrowserNode BuildViewsTree(Document doc, EventHandler eventHandler = null, string wrap = "Views")
        {
            var dirBase = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ShinryoTher";
            if (Directory.Exists(dirBase))
                Directory.Delete(dirBase, true);
            Directory.CreateDirectory(dirBase);
            var node = new BrowserNode(wrap);
            var boViews = BrowserOrganization.GetCurrentBrowserOrganizationForViews(doc);
            if (boViews == null) return node;
            var views = new FilteredElementCollector(doc)
            .OfClass(typeof(View)).Cast<View>()
            .Where(v => !v.IsTemplate)
            .Where(v => v.ViewType != ViewType.SystemBrowser && v.ViewType != ViewType.ProjectBrowser && v.ViewType != ViewType.Legend)
            .Where(x => !(x is ViewSheet) && !(x is ViewSchedule))
            .Where(v => boViews.AreFiltersSatisfied(v.Id))
            .ToList();
            if (!views.Any()) return node;
            var viewSheets = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .ToList();
            var viewPorts = viewSheets.Any()
                ? viewSheets
                .Select(x => x.GetAllViewports())
                .Aggregate((a, b) => a.Concat(b).ToList())
                .GroupBy(x => x.ToString())
                .Select(x => doc.GetElement(x.FirstOrDefault()) as Viewport)
                .ToList()
                : new List<Viewport>();
            var viewOnSheetIds = viewPorts
                .Select(x => x.ViewId)
                .ToList();
            var paths = views
                .Where(x => boViews.GetFolderItems(x.Id).Any())
                .Select(x => $"{dirBase}\\{wrap}\\{boViews.GetFolderItems(x.Id).Select(y => y.Name).Aggregate((a, b) => $"{a}\\{b}")}\\{x.Name}.txt")
                .Distinct()
                .ToList();
            if (!paths.Any())
                return node;
            foreach (var path in paths)
            {
                var pathValid = path.Replace('?', '&');
                var dirCreate = pathValid.Split('\\').Where(x => !x.Contains('.'))
                    .Aggregate((a, b) => $"{a}\\{b}");
                Directory.CreateDirectory(dirCreate);
            }
            var dirs = Directory.GetDirectories(dirBase).First();
            node = BuildViewsTree(dirBase, dirs, views, boViews, viewPorts, eventHandler);
            node.SelectAction = eventHandler;
            Directory.Delete(dirBase, true);
            return node;
        }
        public static BrowserNode BuildViewSchedulesTree(Document doc, EventHandler eventHandler = null, string wrap = "Schedules")
        {
            var dirBase = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ShinryoTher";
            if (Directory.Exists(dirBase))
                Directory.Delete(dirBase, true);
            Directory.CreateDirectory(dirBase);
            var node = new BrowserNode(wrap);
            node.SelectAction = eventHandler;
            var boViews = BrowserOrganization.GetCurrentBrowserOrganizationForSchedules(doc);
            if (boViews == null) return node;
            var views = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSchedule)).Cast<ViewSchedule>()
            .Where(v => !v.IsTemplate)
            .Where(v => boViews.AreFiltersSatisfied(v.Id))
            .ToList();
            if (!views.Any()) return node;
            var viewSheets = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .ToList();
            var viewPorts = viewSheets.Any()
                ? viewSheets
                .Select(x => x.GetAllViewports())
                .Aggregate((a, b) => a.Concat(b).ToList())
                .GroupBy(x => x.ToString())
                .Select(x => doc.GetElement(x.FirstOrDefault()) as Viewport)
                .ToList()
                : new List<Viewport>();
            var viewOnSheetIds = viewPorts
                .Select(x => x.ViewId)
                .ToList();
            var paths = views
                .Where(x => boViews.GetFolderItems(x.Id).Any())
                .Select(x => $"{dirBase}\\{wrap}\\{boViews.GetFolderItems(x.Id).Select(y => y.Name).Aggregate((a, b) => $"{a}\\{b}")}\\{x.Name}.txt")
                .Distinct()
                .ToList();
            if (!paths.Any())
                return node;
            foreach (var path in paths)
            {
                var pathValid = path.Replace('?', '&');
                var dirCreate = pathValid.Split('\\').Where(x => !x.Contains('.'))
                    .Aggregate((a, b) => $"{a}\\{b}");
                Directory.CreateDirectory(dirCreate);
            }
            var dirs = Directory.GetDirectories(dirBase).First();
            node = BuildViewSchedulesTree(dirBase, dirs, views, boViews, viewPorts, eventHandler);
            node.SelectAction = eventHandler;
            Directory.Delete(dirBase, true);
            return node;
        }
        public static BrowserNode BuildViewSheetTree(Document doc, EventHandler eventHandler = null, string wrap = "Sheets")
        {
            var node = new BrowserNode("Sheets");
            node.SelectAction = eventHandler;
            var boViews = BrowserOrganization.GetCurrentBrowserOrganizationForSheets(doc);
            if (boViews == null) return node;
            var views = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSheet)).Cast<ViewSheet>()
            .Where(v => !v.IsTemplate)
            .Where(v => boViews.AreFiltersSatisfied(v.Id));
            var viewSheets = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .ToList();
            var viewPorts = viewSheets.Any()
                ? viewSheets
                .Select(x => x.GetAllViewports())
                .Aggregate((a, b) => a.Concat(b).ToList())
                .GroupBy(x => x.ToString())
                .Select(x => doc.GetElement(x.FirstOrDefault()) as Viewport)
                .ToList()
                : new List<Viewport>();
            var viewOnSheetIds = viewPorts
                .Select(x => x.ViewId)
                .ToList();
            if (!views.Any()) return node;
            foreach (var v in views)
            {
                var noteView = new BrowserNode(v.Name);
                noteView.ViewId = long.Parse(v.Id.ToString());
                noteView.Parent = node;
                var viewOnSheetId = viewOnSheetIds.FirstOrDefault(x => x.ToString() == v.Id.ToString());
                if (viewOnSheetId != null)
                {
                    var viewPort = viewPorts.FirstOrDefault(x => x.ViewId.ToString() == viewOnSheetId.ToString());
                    if (viewPort != null)
                    {
                        noteView.Name = $"{noteView.Name} (On Sheet)";
                        noteView.ViewSheetId = long.Parse(viewPort.SheetId.ToString());
                    }
                }
                node.Children.Add(noteView);
            }
            return node;
        }
        public static List<BrowserNode> GetBrowserNodeIsView(BrowserNode browserNode)
        {
            var result = new List<BrowserNode>();
            if (browserNode.ViewId != -1)
                result.Add(browserNode);
            if (!browserNode.Children.Any())
                return result;
            var child = browserNode.Children;
            foreach (var item in child)
            {
                if (item.ViewId != -1)
                    result.Add(item);
                if (item.Children.Any())
                {
                    result.AddRange(GetBrowserNodeIsView(item));
                }
            }
            return result;
        }

        private static BrowserNode BuildViewsTree(
            string dirBase,
            string dir,
            List<View> views,
            BrowserOrganization boViews,
            List<Viewport> viewPorts,
            EventHandler eventHandler)
        {
            var lastIndex = dirBase.Split('\\').Count() - 1;
            var folderItems = dir.Split('\\').Select(x => x.Replace('&', '?')).ToList();
            var dirViews = folderItems.Where(x => folderItems.IndexOf(x) > lastIndex);
            var dirView = dirViews.Count() == 1
                ? ""
                : dirViews.Where((x, index) => index > 0).Aggregate((a, b) => $"{a}\\{b}");
            var name = folderItems.LastOrDefault();
            var node = new BrowserNode(name)
            {
                SelectAction = eventHandler
            };
            var viewsTarget = views
                .Where(v => boViews.GetFolderItems(v.Id).Select(x => x.Name).Aggregate((a, b) => $"{a}\\{b}") == dirView);
            if (viewsTarget.Any())
            {
                foreach (var item in viewsTarget)
                {
                    var noteView = new BrowserNode(item.Name);
                    noteView.Parent = node;
                    noteView.ViewId = long.Parse(item.Id.ToString());
                    var viewPort = viewPorts.FirstOrDefault(x => x.ViewId.ToString() == item.Id.ToString());
                    if (viewPort != null)
                    {
                        noteView.Name = $"{noteView.Name} (On Sheet)";
                        noteView.ViewSheetId = long.Parse(viewPort.SheetId.ToString());
                        noteView.SelectAction = eventHandler;
                    }
                    node.Children.Add(noteView);
                }
            }
            var folders = Directory.GetDirectories(dir).ToList();
            if (!folders.Any())
                return node;
            foreach (var folder in folders)
            {
                var noteNth = BuildViewsTree(dirBase, folder, views, boViews, viewPorts, eventHandler);
                noteNth.SelectAction = eventHandler;
                noteNth.Parent = node;
                node.Children.Add(noteNth);
            }
            return node;
        }
        private static BrowserNode BuildViewSchedulesTree(
            string dirBase,
            string dir,
            List<ViewSchedule> views,
            BrowserOrganization boViews,
            List<Viewport> viewPorts,
            EventHandler eventHandler)
        {
            var lastIndex = dirBase.Split('\\').Count() - 1;
            var folderItems = dir.Split('\\').Select(x => x.Replace('&', '?')).ToList();
            var dirViews = folderItems.Where(x => folderItems.IndexOf(x) > lastIndex);
            var dirView = dirViews.Count() == 1
                ? ""
                : dirViews.Where((x, index) => index > 0).Aggregate((a, b) => $"{a}\\{b}");
            var name = folderItems.LastOrDefault();
            var node = new BrowserNode(name)
            {
                SelectAction = eventHandler
            };
            var viewsTarget = views
                .Where(v => boViews.GetFolderItems(v.Id).Select(x => x.Name).Aggregate((a, b) => $"{a}\\{b}") == dirView);
            if (viewsTarget.Any())
            {
                foreach (var item in viewsTarget)
                {
                    var noteView = new BrowserNode(item.Name);
                    noteView.Parent = node;
                    noteView.ViewId = long.Parse(item.Id.ToString());
                    var viewPort = viewPorts.FirstOrDefault(x => x.ViewId.ToString() == item.Id.ToString());
                    if (viewPort != null)
                    {
                        noteView.Name = $"{noteView.Name} (On Sheet)";
                        noteView.ViewSheetId = long.Parse(viewPort.SheetId.ToString());
                        noteView.SelectAction = eventHandler;
                    }
                    node.Children.Add(noteView);
                }
            }
            var folders = Directory.GetDirectories(dir).ToList();
            if (!folders.Any())
                return node;
            foreach (var folder in folders)
            {
                var noteNth = BuildViewSchedulesTree(dirBase, folder, views, boViews, viewPorts, eventHandler);
                noteNth.SelectAction = eventHandler;
                noteNth.Parent = node;
                node.Children.Add(noteNth);
            }
            return node;
        }
    }
}
