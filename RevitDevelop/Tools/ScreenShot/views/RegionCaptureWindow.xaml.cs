using System.Drawing;                 // System.Drawing.Common (Windows)
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace RevitDevelop.Tools.ScreenShot.views
{
    public partial class RegionCaptureWindow : Window
    {
        // Win32 để lấy toạ độ con trỏ theo pixel thiết bị (tránh sai lệch DPI)
        [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT pt);
        [StructLayout(LayoutKind.Sequential)] struct POINT { public int X, Y; }

        private System.Windows.Point _startDip;   // điểm bắt đầu (DIPs) để vẽ khung
        private System.Windows.Point _endDip;
        private bool _dragging;

        public string SavePath { get; set; } = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "capture.png");

        public RegionCaptureWindow()
        {
            InitializeComponent();

            // phủ toàn bộ virtual screen để hỗ trợ đa màn hình (cùng DPI)
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            Cursor = Cursors.Cross;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close(); // hủy
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragging = true;
            _startDip = e.GetPosition(this);
            Sel.Visibility = System.Windows.Visibility.Visible;
            CaptureMouse();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            _endDip = e.GetPosition(this);

            // vẽ khung chọn
            var x = Math.Min(_startDip.X, _endDip.X);
            var y = Math.Min(_startDip.Y, _endDip.Y);
            var w = Math.Abs(_endDip.X - _startDip.X);
            var h = Math.Abs(_endDip.Y - _startDip.Y);

            Canvas.SetLeft(Sel, x);
            Canvas.SetTop(Sel, y);
            Sel.Width = w;
            Sel.Height = h;
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragging) return;
            _dragging = false;
            ReleaseMouseCapture();

            // Lấy rect chọn theo DEVICE PIXELS để chụp chính xác
            GetCursorPos(out var endPx);
            // điểm bắt theo pixel (dùng _startDip + chuyển DIP->px theo DPI cửa sổ)
            var dpi = VisualTreeHelper.GetDpi(this); // scale cho cửa sổ hiện tại
            var startPx = DipToPx(_startDip, dpi);

            // lưu ý: vì cửa sổ phủ virtual screen, phải cộng offset virtual
            int vx = (int)Math.Round(SystemParameters.VirtualScreenLeft * dpi.DpiScaleX);
            int vy = (int)Math.Round(SystemParameters.VirtualScreenTop * dpi.DpiScaleY);

            var x = Math.Min(startPx.X, endPx.X) + vx;
            var y = Math.Min(startPx.Y, endPx.Y) + vy;
            var w = Math.Abs(endPx.X - startPx.X);
            var h = Math.Abs(endPx.Y - startPx.Y);

            if (w > 0 && h > 0)
            {
                try
                {
                    CaptureRectangle(new System.Drawing.Rectangle(x, y, w, h), SavePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Capture error: " + ex.Message);
                }
            }

            //Close();
        }

        private static System.Drawing.Point DipToPx(System.Windows.Point dip, DpiScale dpi)
        {
            return new System.Drawing.Point(
                (int)Math.Round(dip.X * dpi.DpiScaleX),
                (int)Math.Round(dip.Y * dpi.DpiScaleY));
        }

        private static void CaptureRectangle(System.Drawing.Rectangle r, string path)
        {
            using var bmp = new Bitmap(r.Width, r.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(r.Location, System.Drawing.Point.Empty, r.Size);
            }
            bmp.Save(path, ImageFormat.Png);
        }
    }
}
