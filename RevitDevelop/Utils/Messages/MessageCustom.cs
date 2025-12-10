using RevitDevelop.Utils.Messages.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitDevelop.Utils.Messages
{
    public class MessageCustom
    {
        public static DialogResult ShowInfo(string text = "Info", string title = "Info")
        {
            var view = new InfoView();
            view.Title = title;
            view.Content.Text = text;
            view.ShowDialog();
            var result = view.DialogResult;
            view.Close();
            return result;
        }
    }
}
