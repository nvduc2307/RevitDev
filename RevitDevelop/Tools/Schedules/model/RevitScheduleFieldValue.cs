namespace RevitDevelop.Tools.Schedules.model
{
    public class RevitScheduleFieldValue
    {
        public string ScheduleName { get; set; }
        public string FamilyName { get; set; }
        public string TypeName { get; set; }
        public string SystemName { get; set; }
        public double Length { get; set; } //mm
        public int Quantity { get; set; }
        public string Size { get; set; }
        public bool IsKyusuiChecked { get; set; } // 設備積算_給水設備工事
        public bool IsKyutouChecked { get; set; } //設備積算_給湯設備工事
        public bool IsDrainageSystemChecked { get; set; } //設備積算_排水設備工事
    }
}
