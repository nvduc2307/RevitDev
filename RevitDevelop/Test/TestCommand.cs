using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.SkipWarning;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    
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
