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

namespace RevitDevelop.TestDamper.views
{
    /// <summary>
    /// Interaction logic for TestDamperView.xaml
    /// </summary>
    public partial class TestDamperView : Window
    {
        public TestDamperView()
        {
            InitializeComponent();
            this.Escape();
        }
    }
}
