namespace RevitDevelop.Tools.Schedules.model
{
    public class DTScheduleFieldValue
    {
        public string LevelName { get; set; }
        public string ProjectName { get; set; }
        public int IndexRow { get; set; }
        public string IndexColQuantity { get; set; }
        public string IndexColTotal { get; set; }
        public double QuantityValue { get; set; }//数量
        public double TotalValue { get; set; }//合計
    }
}
