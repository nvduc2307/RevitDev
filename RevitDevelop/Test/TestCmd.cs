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
                    var systemType = Document.GetElementsFromClass<MechanicalSystemType>(true).FirstOrDefault();
                    var flexDuctTypes = Document.GetElementsFromClass<FlexDuctType>(true);
                    if (systemType == null)
                        throw new Exception();
                    if (!flexDuctTypes.Any())
                        throw new Exception();
                    var ductRef = UiDocument.Selection.PickObject(ObjectType.Element, new GenericSelectionFilter(BuiltInCategory.OST_DuctCurves));
                    var duct = Document.GetElement(ductRef) as Duct;
                    if (duct == null)
                        throw new Exception("obj is not a duct");
                    var eles = duct.GetGroupDuct();
                    var connectors = eles.SortConnector()
                        .ConvertConnectorToPoint(2, 50)
                        .ToList();
                    using (var ts = new Transaction(Document, "new transaction"))
                    {
                        ts.SkipAllWarnings();
                        ts.Start();
                        duct.DuctToFlexDuct(connectors, systemType, flexDuctTypes);
                        Document.Delete(eles.Select(x=>x.Id).ToList());
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
