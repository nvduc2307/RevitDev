using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RevitDevelop.Utils.RevIcons
{
    public static class RevIconUtils
    {
        public static ImageSource LoadPng(string path)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.Absolute);   // hoặc Relative nếu bạn đã tính sẵn
            bi.CacheOption = BitmapCacheOption.OnLoad;         // mở xong đóng file ngay
            bi.EndInit();
            bi.Freeze();
            return bi;
        }
        public static ImageSource LoadPack(string uri)
        {
            var bi = new BitmapImage(new Uri(uri, UriKind.Absolute));
            bi.Freeze();
            return bi;
        }

        public static ImageSource LoadEmbeddedPng(Assembly asm, string resName)
        {
            //var asm = Assembly.GetExecutingAssembly();
            // resName: ví dụ "MyAddin.icons.my_32.png"
            using var s = asm.GetManifestResourceStream(resName);
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = s;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }
    }
}
