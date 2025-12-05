namespace RevitDevelop.DemoST.CopyFilters.models
{
    public partial class FilterModel : ObservableObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [ObservableProperty]
        private bool _isSelected;
    }
}
