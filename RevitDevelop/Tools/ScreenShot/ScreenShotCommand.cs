using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Tools.ScreenShot.views;
using RevitDevelop.Utils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Tools.ScreenShot
{
    [Transaction(TransactionMode.Manual)]
    public class ScreenShotCommand : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var view = new RegionCaptureWindow() {
                        SavePath = System.IO.Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.png")
                    };
                    view.ShowDialog();
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
    }
}
