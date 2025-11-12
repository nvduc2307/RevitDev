using RevitDevelop.Utils.WPF.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RevitDevelop.Tools.TestEdittorStatus.Views
{
    /// <summary>
    /// Interaction logic for TestEdittorStatusView.xaml
    /// </summary>
    public partial class TestEdittorStatusView : Window
    {
        public TestEdittorStatusView()
        {
            InitializeComponent();
            this.Escape();
        }
    }
}
