namespace RevitDevelop.DemoST.Command3.models
{
    public partial class TreeViewModel : ObservableObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                SelectedAction?.Invoke(this, EventArgs.Empty);
            }
        }
        public EventHandler SelectedAction { get; set; }
        public bool IsCategory { get; set; }
        public bool IsFamily { get; set; }
        public bool IsType {  get; set; }
        [ObservableProperty]
        private bool _isOpen;
        public List<TreeViewModel> Childrent { get; set; }
        public TreeViewModel Parent { get; set; }
    }
}
