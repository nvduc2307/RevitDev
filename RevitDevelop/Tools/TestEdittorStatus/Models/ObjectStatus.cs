namespace RevitDevelop.Tools.TestEdittorStatus.Models
{
    public partial class ObjectStatus : ObservableObject
    {
        public int Index { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; }
        [ObservableProperty]
        public string _editor;
    }
}
