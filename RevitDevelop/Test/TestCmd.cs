using Autodesk.AutoCAD.Geometry;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.BrowserNodes;
using RevitDevelop.Utils.Compares;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevDuct;
using RevitDevelop.Utils.RevParameters;
using RevitDevelop.Utils.RevPipes;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.SkipWarning;
using System.Diagnostics;
using System.IO;
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
                    var fitting1 = UiDocument.Selection.PickElement(Document, BuiltInCategory.OST_DuctFitting);
                    var duct = UiDocument.Selection.PickElement(Document, BuiltInCategory.OST_DuctCurves);
                    var eles = fitting1.GetGroupDuct(duct, 1000);
                    UiDocument.Selection.SetElementIds(eles.Select(x=>x.Id).ToList());
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
