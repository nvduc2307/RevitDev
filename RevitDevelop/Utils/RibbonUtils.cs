using Autodesk.Windows;
using System.Windows.Input;

namespace RevitDevelop.Utils
{
    public static class RibbonUtils
    {
        public static RibbonButton CreateRibbonButton(string pathImage16, string pathImage32, ICommand command = null)
        {
            var btn = new RibbonButton
            {
                IsCheckable = true,
                IsEnabled = true,
                Size = RibbonItemSize.Large,
                Image = IConUtils.LoadPack(pathImage16),
                LargeImage = IConUtils.LoadPack(pathImage32),
                ShowImage = true,
                CommandHandler = command
            };
            return btn;
        }
    }
}
