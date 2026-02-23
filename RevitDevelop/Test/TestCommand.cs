using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.SelectFilters;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Revit.IFC.Common.Utility.IFCAnyHandleUtil;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : ExternalCommand
    {
        public const string IMG_COMMAND_FINISH_16 = "pack://application:,,,/UIFrameworkRes;component/Ribbon/ThemedImages/finish/finish_16_light.png";
        public const string IMG_COMMAND_FINISH_32 = "pack://application:,,,/UIFrameworkRes;component/Ribbon/ThemedImages/finish/finish_32_light.png";
        public const string IMG_COMMAND_CANCEL_16 = "pack://application:,,,/UIFrameworkRes;component/Ribbon/ThemedImages/cancel/cancel_16_light.png";
        public const string IMG_COMMAND_CANCEL_32 = "pack://application:,,,/UIFrameworkRes;component/Ribbon/ThemedImages/cancel/cancel_32_light.png";
        public override void Execute()
        {
            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    UiApplication.SelectionChanged += UiApplication_SelectionChanged;
                    //var tab = ComponentManager.Ribbon.Tabs
                    //    .FirstOrDefault(x => x.Id == "Modify");
                    //var lastpannel = tab.Panels.Last();
                    //var btn = CreateRibbonButton(
                    //    IMG_COMMAND_FINISH_16,
                    //    IMG_COMMAND_FINISH_32,
                    //    "Thao Le");
                    //btn.Orientation = System.Windows.Controls.Orientation.Vertical;
                    //lastpannel.Source.Items.Add(btn);
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception ex)
                {
                    IO.ShowWarning(ex.Message);
                    tsg.RollBack();
                }
            }
        }

        private void UiApplication_SelectionChanged(object sender, Autodesk.Revit.UI.Events.SelectionChangedEventArgs e)
        {
            var eles = UiApplication.ActiveUIDocument.Selection.GetElementIds();
            if (eles.Any())
            {
            }
            else
            {
            }
        }

        private void ComponentManager_ItemInitialized1(object sender, RibbonItemEventArgs e)
        {
            IO.ShowInfo("Init UI Element");
        }

        private void ComponentManager_ItemInitialized(object sender, RibbonItemEventArgs e)
        {
            IO.ShowInfo("isActive");
        }

        public static Autodesk.Windows.RibbonButton CreateRibbonButton(
        string name,
        ICommand command = null)
        {
            var btn = new Autodesk.Windows.RibbonButton
            {
                Name = name,
                Text = name,
                ShowText = true,
                IsCheckable = true,
                IsEnabled = true,
                ShowImage = true,
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                CommandHandler = command
            };
            return btn;
        }

        public static Autodesk.Windows.RibbonButton CreateRibbonButton(
            string pathImage16,
            string pathImage32,
            string name = "",
            ICommand command = null)
        {
            var btn = new Autodesk.Windows.RibbonButton
            {
                Text = name,
                ShowText = true,
                IsCheckable = true,
                IsEnabled = true,
                Size = RibbonItemSize.Large,
                Image = LoadPack(pathImage16),
                LargeImage = LoadPng(pathImage32),
                ShowImage = true,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                CommandHandler = command
            };
            return btn;
        }
        public static ImageSource LoadPack(string uri)
        {
            var bi = new BitmapImage(new Uri(uri, UriKind.Absolute));
            bi.Freeze();
            return bi;
        }
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
    }
}
