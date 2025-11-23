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

namespace RevitDevelop.Tools.Syns.view
{
    /// <summary>
    /// Interaction logic for SynsView.xaml
    /// </summary>
    public partial class SynsView : Window
    {
        public SynsView()
        {
            InitializeComponent();
            this.Escape();
        }
    }
}
