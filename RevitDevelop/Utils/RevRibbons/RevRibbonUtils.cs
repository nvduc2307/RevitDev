using Autodesk.Windows;
using RevitDevelop.Utils.RevIcons;
using System.Windows.Input;

namespace RevitDevelop.Utils.RevRibbons
{
    public static class RevRibbonUtils
    {
        public static RibbonButton CreateRibbonButton(string pathImage16, string pathImage32, ICommand command = null)
        {
            var btn = new RibbonButton
            {
                IsCheckable = true,
                IsEnabled = true,
                Size = RibbonItemSize.Large,
                Image = RevIconUtils.LoadPack(pathImage16),
                LargeImage = RevIconUtils.LoadPack(pathImage32),
                ShowImage = true,
                CommandHandler = command
            };
            return btn;
        }
    }
}
