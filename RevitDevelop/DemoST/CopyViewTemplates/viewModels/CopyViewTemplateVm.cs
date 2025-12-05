using RevitDevelop.DemoST.CopyViewTemplates.actions;
using RevitDevelop.DemoST.CopyViewTemplates.models;
using RevitDevelop.DemoST.CopyViewTemplates.views;

namespace RevitDevelop.DemoST.CopyViewTemplates.viewModels
{
    public class CopyViewTemplateVm
    {
        private CopyViewTemplateCmd _cmd;
        private CopyViewTemplateAction _action;
        public List<ViewTemplateModel> ViewTemplates { get; set; }
        public CopyViewTemplateView MainView { get; set; }
        public CopyViewTemplateVm(
            CopyViewTemplateCmd cmd,
            CopyViewTemplateAction action)
        {
            _cmd = cmd;
            _action = action;
            ViewTemplates = _action.GetViewTemplateModels();
            MainView = new CopyViewTemplateView() { DataContext = this};
        }
    }
}
