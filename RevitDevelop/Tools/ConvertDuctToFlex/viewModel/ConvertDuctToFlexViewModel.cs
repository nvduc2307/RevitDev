using RevitDevelop.Tools.ConvertDuctToFlex.model;

namespace RevitDevelop.Tools.ConvertDuctToFlex.viewModel
{
    public partial class ConvertDuctToFlexViewModel : ObservableObject
    {
        public List<ConvertDuctToFlexOptionConvert> OptionConverts { get; set; }
        [ObservableProperty]
        private ConvertDuctToFlexOptionConvert _optionConvert;
        [ObservableProperty]
        private double _length;
    }
}
