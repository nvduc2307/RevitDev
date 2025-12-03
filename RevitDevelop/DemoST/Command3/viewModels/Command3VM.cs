using Autodesk.Revit.DB;
using RevitDevelop.DemoST.Command3.actions;
using RevitDevelop.DemoST.Command3.models;
using RevitDevelop.DemoST.Command3.views;

namespace RevitDevelop.DemoST.Command3.viewModels
{
    public partial class Command3VM
    {
        private Command3Cmd _cmd;
        private Command3Action _action;
        public Command3V MainView { get; set; }
        public List<TreeViewModel> TreeViewModels { get; set; }
        public List<LevelModel> Levels { get; set; }
        public Command3VM(Command3Cmd cmd, Command3Action action)
        {
            _cmd = cmd;
            _action = action;
            var cates = _action.GetCategories(out List<Level> levels);
            Levels = _action.GetLevelModels(levels);
            TreeViewModels = _action.GetTreeViewModels(cates);
            MainView = new Command3V() { DataContext = this};
        }

        [RelayCommand]
        private void CommandChoice()
        {
            var typeRules = TreeViewModels
                .Select(x=>x.Childrent)
                .Aggregate((a,b) => a.Concat(b).ToList())
                .Where(x=>x.IsType)
                .Where(x=>x.IsSelected)
                .ToList();
            var levelRules = Levels
                .Where(x=>x.IsSelected)
                .ToList();

            var eles = new FilteredElementCollector(_cmd.Document)
                .WhereElementIsNotElementType()
                .Where(x=> x.Category != null)
                .Where(x=> typeRules.Any(y=>y.Parent.Id.ToString() == x.Category.Id.ToString()))
                .Where(x=>typeRules.Any(y=>y.Id.ToString() == x.GetTypeId().ToString()))
                .Where(x =>
                {
                    var level = _action.GetLevel(x);
                    if (level == null) return false;
                    return levelRules.Any(y=>y.Id.ToString() == level.Id.ToString());
                }).ToList();
            if (!eles.Any())
                return;
            _cmd.UiDocument.Selection.SetElementIds(eles.Select(x=>x.Id).ToList());
        }
        [RelayCommand]
        private void CommandCancel()
        {
            MainView.Close();
        }
        [RelayCommand] 
        private void TabGrElementSelectAll()
        {
            foreach (var item in TreeViewModels)
            {
                if (!item.IsSelected)
                    item.IsSelected = true;
                if (item.Childrent == null)
                    continue;
                if (!item.Childrent.Any())
                    continue;
                foreach (var child in item.Childrent)
                {
                    if (!child.IsSelected)
                        child.IsSelected = true;
                }
            }
        }
        [RelayCommand]
        private void TabGrElementUnSelectAll()
        {
            foreach (var item in TreeViewModels)
            {
                if (item.IsSelected)
                    item.IsSelected = false;
                if (item.Childrent == null)
                    continue;
                if (!item.Childrent.Any())
                    continue;
                foreach (var child in item.Childrent)
                {
                    if (child.IsSelected)
                        child.IsSelected = false;
                }
            }
        }
        [RelayCommand]
        private void TabGrElementExpand()
        {
            foreach (var item in TreeViewModels)
            {
                if (item.IsOpen)
                    continue;
                item.IsOpen = true;
            }
        }
        [RelayCommand]
        private void TabGrElementUnExpand()
        {
            foreach (var item in TreeViewModels)
            {
                if (!item.IsOpen)
                    continue;
                item.IsOpen = false;
            }
        }

        [RelayCommand]
        private void TabGrLevelSelectAll()
        {
            foreach(var level in Levels)
            {
                if(!level.IsSelected)
                    level.IsSelected = true;
            }
        }
        [RelayCommand]
        private void TabGrLevelUnSelectAll()
        {
            foreach (var level in Levels)
            {
                if (level.IsSelected)
                    level.IsSelected = false;
            }
        }
    }
}
