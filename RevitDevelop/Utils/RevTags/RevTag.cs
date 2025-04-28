namespace RevitDevelop.Utils.RevTags
{
    public abstract partial class RevTag : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LevelReferenceId { get; set; }
        public double ElevationOffsetFromLevelReferenceMm { get; set; }
        public List<RevTagType> RevTagTypes { get; set; }
        [ObservableProperty]
        private RevTagType _tagType;
        
    }
}
