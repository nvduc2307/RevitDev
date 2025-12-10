using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RevitDevelop.Utils.Messages.UI
{
    /// <summary>
    /// Interaction logic for InfoView.xaml
    /// </summary>
    public partial class InfoView : Window
    {
        public DialogResult DialogResult { get; set; }
        public InfoView()
        {
            InitializeComponent();
            DialogResult = DialogResult.None;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Hide();
        }
    }
}
