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

namespace RevitDevelop.DemoST.Command3.views
{
    /// <summary>
    /// Interaction logic for Command3V.xaml
    /// </summary>
    public partial class Command3V : Window
    {
        public Command3V()
        {
            InitializeComponent();
            this.Escape();
        }
    }
}
