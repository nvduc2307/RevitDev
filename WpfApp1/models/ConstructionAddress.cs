namespace WpfApp1.models
{
    public class ConstructionAddress
    {
        public string? PrefectureName { get; set; } //県名
        public string? Category1 { get; set; } //区分1
        public string? Category2 { get; set; } //区分2
        public string? Category3 { get; set; } //区分3
        public string? Category4 { get; set; } //区分4
        public double WindSpeed { get; set; } //風速 m/s
        public double SnowfallAmount { get; set; } //積雪量 cm
    }
}
