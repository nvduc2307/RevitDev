using System.Windows;
using WpfApp1.viewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new Vm();
            this.DataContext = vm;
        }
    }
}