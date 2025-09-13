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
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevDuct;
using RevitDevelop.Utils.RevParameters;
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
                    //var ductAccessoryRef = UiDocument.Selection.PickObject(ObjectType.Element, new GenericSelectionFilter(BuiltInCategory.OST_DuctAccessory));
                    //var ductAccessory = Document.GetElement(ductAccessoryRef) as FamilyInstance;
                    //if (ductAccessory == null)
                    //    throw new Exception("obj is not a ductAccessory");
                    var ductRef = UiDocument.Selection.PickObject(ObjectType.Element, new GenericSelectionFilter(BuiltInCategory.OST_DuctCurves));
                    var duct = Document.GetElement(ductRef) as Duct;
                    if (duct == null)
                        throw new Exception("obj is not a duct");

                    using (var ts = new Transaction(Document, "new transaction"))
                    {
                        ts.SkipAllWarnings();
                        ts.Start();
                        var eles = duct.GetGroupDuct();
                        UiDocument.Selection.SetElementIds(eles.Select(x=>x.Id).ToList());
                        ts.Commit();
                    }

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
