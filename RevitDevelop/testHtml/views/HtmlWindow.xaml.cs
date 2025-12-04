using Autodesk.Revit.UI;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using RevitDevelop.testHtml.cores;
using RevitDevelop.Utils.Paths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static UIFramework.Widget.CustomControls.NativeMethods;

namespace RevitDevelop.testHtml.views
{
    /// <summary>
    /// Interaction logic for HtmlWindow.xaml
    /// </summary>
    public partial class HtmlWindow : Window
    {
        public static HtmlWindow Current;                 // để nơi khác gửi message về JS
        private readonly ExternalEvent _evt;
        private readonly WebBridge _handler;
        public HtmlWindow(UIApplication uiapp)
        {
            InitializeComponent();
            Current = this;

            // đảm bảo window “thuộc” Revit
            new WindowInteropHelper(this) { Owner = uiapp.MainWindowHandle };

            _handler = new WebBridge();
            _evt = ExternalEvent.Create(_handler);

            Loaded += async (_, __) => await InitWebAsync();
            Closed += (_, __) => Current = null;
        }
        private async Task InitWebAsync()
        {
            var baseDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "MyCompany", "MyAddin", "WebView2");
            Directory.CreateDirectory(baseDir);
            var userData = System.IO.Path.Combine(baseDir, Process.GetCurrentProcess().Id.ToString());
            Directory.CreateDirectory(userData);

            Web.CreationProperties = new CoreWebView2CreationProperties { UserDataFolder = userData };
            await Web.EnsureCoreWebView2Async();

            var dir = $"{PathUtils.AssemblyDirectory}\\Resources\\UI";

            if (!Directory.Exists(dir))
            {
                MessageBox.Show($"UI folder NOT found:\n{dir}\n" +
                                "Hãy đảm bảo thư mục này tồn tại và đã copy ra cùng .dll.");
                return;
            }
            string indexPath = System.IO.Path.Combine(dir, "index.html");
            if (!File.Exists(indexPath))
            {
                MessageBox.Show($"index.html NOT found in:\n{indexPath}");
                return;
            }

            // 3) Map Virtual Host và điều hướng
            Web.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app", dir, CoreWebView2HostResourceAccessKind.Allow);

            Web.Source = new Uri("https://app/index.html");

            // nhận message từ JS -> đẩy vào ExternalEvent để thao tác Revit API
            Web.CoreWebView2.WebMessageReceived += (_, e) =>
            {
                _handler.UiApp = AppEntry.UiApp;           // gán UIApplication
                _handler.Payload = e.WebMessageAsJson;      // JSON từ JS
                //_handler.Payload = e.TryGetWebMessageAsString();      // JSON từ JS
                _evt.Raise();                               // gọi API trong thread Revit
            };
        }

        public void SendToJsAsync(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            Web.CoreWebView2.PostWebMessageAsJson(json);
        }
    }
}
