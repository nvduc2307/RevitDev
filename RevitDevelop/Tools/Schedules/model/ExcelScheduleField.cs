namespace RevitDevelop.Tools.Schedules.model
{
    public class ExcelScheduleField
    {
        public string Id { set; get; }
        public string SheetName { get; set; }
        public string FamilyName { get; set; } //工　事　項　目
        public string TypeName { get; set; } //備考
        public int UnitScheduleType { get; set; }
        public int IndexRow { get; set; }
        public List<ExcelScheduleFieldValue> ScheduleFieldValues { get; set; }
    }
}
