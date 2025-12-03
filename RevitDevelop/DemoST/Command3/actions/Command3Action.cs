using RevitDevelop.DemoST.Command3.models;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.RevParameters;
using RevitDevelop.Utils.SelectFilters;

namespace RevitDevelop.DemoST.Command3.actions
{
    public class Command3Action
    {
        private Command3Cmd _cmd;
        public Command3Action(Command3Cmd cmd)
        {
            _cmd = cmd;
        }
        public List<Category> GetCategories(out List<Level> levels)
        {
            var objs = _cmd.UiDocument.Selection
                .PickElements(_cmd.Document)
                .Where(x => GetLevel(x) != null)
                .ToList();
            levels = !objs.Any() 
                ? new List<Level>()
                : objs
                .Select(x=> GetLevel(x))
                .Where(x=>x != null)
                .Distinct(new CompareLevelById())
                .ToList();
            var results = !objs.Any() 
                ? new List<Category>() 
                : objs
                .Select(x=>x.Category)
                .Distinct(new CompareElementByCategory())
                .ToList();
            return results;
        }

        public List<TreeViewModel> GetTreeViewModels(List<Category> categories)
        {
            var results = new List<TreeViewModel>();
            foreach (var category in categories)
            {
                var treeViewModel = new TreeViewModel
                {
                    IsCategory = true,
                    IsType = false,
                    Id = long.Parse(category.Id.ToString()),
                    Name = category.Name,
                    IsSelected = true,
                    IsOpen = true,
                    Childrent = new List<TreeViewModel>(),
                };
                treeViewModel.SelectedAction += _selectedAction;
                var eleTypes = new FilteredElementCollector(_cmd.Document)
                    .WhereElementIsNotElementType()
                    .Where(x=>x.Category != null)
                    .Where(x=>x.Category.Id.ToString() == category.Id.ToString())
                    .Select(x=>x.GetTypeId())
                    .GroupBy(x=>x.ToString())
                    .Select(x=>_cmd.Document.GetElement(x.FirstOrDefault()))
                    .ToList();
                foreach (var eleType in eleTypes)
                {
                    var child = new TreeViewModel
                    {
                        IsCategory = false,
                        IsType = true,
                        Id = long.Parse(eleType.Id.ToString()),
                        Name = eleType.Name,
                        IsSelected = true,
                        IsOpen = true,
                        Parent = treeViewModel,
                    };
                    child.SelectedAction += _selectedAction;
                    treeViewModel.Childrent.Add(child);
                }
                results.Add(treeViewModel);
            }
            return results;
        }

        private void _selectedAction(object sender, EventArgs e)
        {
            if (sender is TreeViewModel treeViewModel)
            {
                if (treeViewModel.IsCategory)
                {
                    foreach (var child in treeViewModel.Childrent)
                    {
                        child.SelectedAction -= _selectedAction;
                        child.IsSelected = treeViewModel.IsSelected;
                        child.SelectedAction += _selectedAction;
                    }
                }
                if (treeViewModel.IsType)
                {
                    var parent = treeViewModel.Parent;
                    var childrent = parent.Childrent;
                    parent.SelectedAction -= _selectedAction;
                    parent.IsSelected = childrent.Any(x => !x.IsSelected) ? false : true;
                    parent.SelectedAction += _selectedAction;
                }
            }
        }

        public List<LevelModel> GetLevelModels(List<Level> levels)
        {
            var results = new List<LevelModel>();
            foreach (var level in levels)
            {
                var levelModel = new LevelModel();
                levelModel.Id = long.Parse(level.Id.ToString());
                levelModel.Name = level.Name;
                levelModel.IsSelected = true;
                results.Add (levelModel);
            }
            return results;
        }
        public Level GetLevel(Element element)
        {
            var document = _cmd.Document;
            var levelValue1 = element.GetParameterValue(BuiltInParameter.RBS_START_LEVEL_PARAM);
            var levelValue2 = element.GetParameterValue(BuiltInParameter.SCHEDULE_LEVEL_PARAM);
            var levelValue3 = element.GetParameterValue(BuiltInParameter.FAMILY_LEVEL_PARAM);
            var levelValues = new List<string>() { levelValue1, levelValue2, levelValue3};
            if (!levelValues.Any(x => x != string.Empty)) return null;
            var levelValue = levelValues.FirstOrDefault(x => x != string.Empty);
            var level = document.GetElement(new ElementId(long.Parse(levelValue))) as Level;
            return level;
        }
        public void InitAction(List<TreeViewModel> treeViewModels)
        {
        }
    }
}
