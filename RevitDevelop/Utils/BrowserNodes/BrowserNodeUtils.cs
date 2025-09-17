namespace RevitDevelop.Utils.BrowserNodes
{
    public static class BrowserNodeUtils
    {
        public static BrowserNode BuildViewsTree(Document doc, EventHandler eventHandler = null, string wrap = "Views")
        {
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
                .Select(x => $"{wrap}\\{boViews.GetFolderItems(x.Id).Select(y => y.Name).Aggregate((a, b) => $"{a}\\{b}")}\\{x.Name}.view")
                .Distinct()
                .ToList();
            if (!paths.Any())
                return node;
            node = BuildViewsTree(paths, views, viewPorts, eventHandler)
                .FirstOrDefault();
            return node;
        }
        public static BrowserNode BuildViewSchedulesTree(Document doc, EventHandler eventHandler = null, string wrap = "Schedules")
        {
            var node = new BrowserNode(wrap);
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
                .Select(x => $"{wrap}\\{boViews.GetFolderItems(x.Id).Select(y => y.Name).Aggregate((a, b) => $"{a}\\{b}")}\\{x.Name}.view")
                .Distinct()
                .ToList();
            if (!paths.Any())
                return node;
            node = BuildViewSchedulesTree(paths, views, viewPorts, eventHandler)
                .FirstOrDefault();
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

        public static List<BrowserNode> BuildViewsTree(
            this List<string> paths,
            List<View> views,
            List<Viewport> viewPorts,
            EventHandler eventHandler)
        {
            var results = new List<BrowserNode>();
            var root = new Dictionary<string, BrowserNode>();
            foreach (var path in paths)
            {
                string[] parts = path.Split('\\');
                Dictionary<string, BrowserNode> currentLevel = root;
                BrowserNode parentNode = null;
                foreach (var part in parts)
                {
                    if (!currentLevel.ContainsKey(part))
                    {
                        var isView = part.Contains(".view");
                        var nameView = part.Split('.').FirstOrDefault();
                        var view = isView ? views.FirstOrDefault(x => x.Name == nameView) : null;
                        var newNode = view != null
                            ? new BrowserNode(nameView)
                            : new BrowserNode(part);
                        newNode.SelectAction = eventHandler;
                        if (view != null)
                        {
                            var viewPort = viewPorts.FirstOrDefault(x => x.ViewId.ToString() == view.Id.ToString());
                            newNode.Name = viewPort != null
                                ? $"{newNode.Name} (On Sheet)"
                                : newNode.Name;
                            newNode.ViewId = long.Parse(view.Id.ToString());
                            newNode.ViewSheetId = viewPort != null
                                ? long.Parse(viewPort.SheetId.ToString())
                                : -1;
                        }
                        if (parentNode != null)
                        {
                            newNode.Parent = parentNode;
                            parentNode.Children.Add(newNode);
                        }

                        currentLevel[part] = newNode;
                    }
                    parentNode = currentLevel[part];
                    currentLevel = parentNode.Children
                        .OfType<BrowserNode>()
                        .ToDictionary(i => i.Name.ToString(), i => i);
                }
            }
            foreach (var key in root.Keys)
            {
                results.Add(root[key]);
            }
            return results;
        }

        public static List<BrowserNode> BuildViewSchedulesTree(
            this List<string> paths,
            List<ViewSchedule> views,
            List<Viewport> viewPorts,
            EventHandler eventHandler)
        {
            var results = new List<BrowserNode>();
            var root = new Dictionary<string, BrowserNode>();
            foreach (var path in paths)
            {
                string[] parts = path.Split('\\');
                Dictionary<string, BrowserNode> currentLevel = root;
                BrowserNode parentNode = null;
                foreach (var part in parts)
                {
                    if (!currentLevel.ContainsKey(part))
                    {
                        var isView = part.Contains(".view");
                        var nameView = part.Split('.').FirstOrDefault();
                        var view = isView ? views.FirstOrDefault(x => x.Name == nameView) : null;
                        var newNode = view != null
                            ? new BrowserNode(nameView)
                            : new BrowserNode(part);
                        newNode.SelectAction = eventHandler;
                        if (view != null)
                        {
                            var viewPort = viewPorts.FirstOrDefault(x => x.ViewId.ToString() == view.Id.ToString());
                            newNode.Name = viewPort != null
                                ? $"{newNode.Name} (On Sheet)"
                                : newNode.Name;
                            newNode.ViewId = long.Parse(view.Id.ToString());
                            newNode.ViewSheetId = viewPort != null
                                ? long.Parse(viewPort.SheetId.ToString())
                                : -1;
                        }
                        if (parentNode != null)
                        {
                            newNode.Parent = parentNode;
                            parentNode.Children.Add(newNode);
                        }

                        currentLevel[part] = newNode;
                    }
                    parentNode = currentLevel[part];
                    currentLevel = parentNode.Children
                        .OfType<BrowserNode>()
                        .ToDictionary(i => i.Name.ToString(), i => i);
                }
            }
            foreach (var key in root.Keys)
            {
                results.Add(root[key]);
            }
            return results;
        }


        public static List<BrowserNode> BuildItemTree(
            this List<string> paths)
        {
            var results = new List<BrowserNode>();
            foreach (var path in paths)
            {
                string[] parts = path.Split('\\');
                var noteCurrent = results.FirstOrDefault(x => x.Name == parts.FirstOrDefault());
                if (noteCurrent == null)
                {
                    noteCurrent = new BrowserNode(parts.FirstOrDefault());
                    results.Add(noteCurrent);
                }
                var c = 0;
                foreach (var part in parts)
                {
                    //skip first items
                    if (c == 0)
                    {
                        c++;
                        continue;
                    }
                    //get childrent
                    //check childent is exist
                    var childrents = noteCurrent.Children;
                    var child = childrents.FirstOrDefault(x => x.Name == part);
                    if (child == null)
                    {
                        child = new BrowserNode(part);
                        child.Parent = noteCurrent;
                        childrents.Add(child);
                    }
                    noteCurrent = child;
                    c++;
                }
            }
            return results;
        }
    }
}
