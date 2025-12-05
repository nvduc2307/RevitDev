using RevitDevelop.DemoST.CopyViewTemplates.models;

namespace RevitDevelop.DemoST.CopyViewTemplates.actions
{
    public class CopyViewTemplateAction
    {
        private CopyViewTemplateCmd _cmd;
        public CopyViewTemplateAction(CopyViewTemplateCmd cmd)
        {
            _cmd = cmd;
        }
        public List<ViewTemplateModel> GetViewTemplateModels()
        {
            var results = new List<ViewTemplateModel>();
            var viewTemplates = new FilteredElementCollector(_cmd.Document)
                .WhereElementIsElementType()
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(x => x.IsTemplate)
                .OrderBy(x => x.Name)
                .ToList();
            if (!viewTemplates.Any())
                return results;
            foreach (var viewTemplate in viewTemplates)
            {
                var viewTemplateModel = new ViewTemplateModel()
                {
                    Id = viewTemplate.UniqueId,
                    Name = viewTemplate.Name,
                    IsSelected = false
                };
                results.Add(viewTemplateModel);
            }
            return results;
        }
    }
}
