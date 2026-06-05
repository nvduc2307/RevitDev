namespace RevitDevelop.Tools.Schedules.model
{
    public class MappingRecord
    {
        public string FamilyName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string WorkItem { get; set; } = string.Empty;
        public string MappingTypeName { get; set; } = string.Empty;
        public int RowIndex { get; set; }
    }
}
