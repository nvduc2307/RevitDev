namespace RevitDevelop.Tools.SettingRebars.SettingDiameters.models
{
    public partial class RebarDatabaseInfo : ObservableObject
    {
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _nameStyle;
        [ObservableProperty]
        private double _modelBarDiameter;
        [ObservableProperty]
        private double _barDiameter;
        [ObservableProperty]
        private double _barDiameterReal;
        [ObservableProperty]
        private double _standardBendDiameter;
        [ObservableProperty]
        private double _standardHookBendDiameter;
        [ObservableProperty]
        private double _stirrupOrTieBendDiameter;
        [ObservableProperty]
        private double _maximumBendRadius;
    }
}
