namespace RevitDevelop.DemoST.Command1.models
{
    public partial class ProjectModel : ObservableObject
    {
        public string IdGuid { get; set; }
        public string Name { get; set; }
        public  string FullPath { get; set; }
        [ObservableProperty]
        private bool _isSelected;
    }
}
