using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.BrowserNodes;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevPipes;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.SkipWarning;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class TestCmd : ExternalCommand
    {
        public override void Execute()
        {
            using (var tsg = new TransactionGroup(Document, "name transaction group"))
            {
                tsg.Start();
                try
                {
                    //--------
                    //--------
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception)
                {
                    tsg.RollBack();
                }
            }
        }
    }
}
