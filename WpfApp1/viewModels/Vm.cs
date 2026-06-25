using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WpfApp1.viewModels
{
    public class Vm : ObservableObject
    {
        public RelayCommand OkCommand { get; set; }
        public Vm()
        {
            OkCommand = new RelayCommand(_OkCommand);
        }

        private void _OkCommand()
        {
            var pathData = "D:\\proj\\me\\RevitDev\\WpfApp1\\datas\\data_construction_address.json";
            var path = @"D:\proj\prima\Yamaguchi_Sangyo_Platform\datas\垂直積雪量　一覧-toc_do_gio_muc_do_tuyet.xlsx";

        }
    }
}
