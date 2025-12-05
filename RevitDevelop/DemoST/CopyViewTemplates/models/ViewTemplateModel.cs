namespace RevitDevelop.DemoST.CopyViewTemplates.models
{
    public partial class ViewTemplateModel : ObservableObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [ObservableProperty]
        private bool _isSelected;
    }
}
