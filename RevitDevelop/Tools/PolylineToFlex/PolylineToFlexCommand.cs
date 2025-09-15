using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Tools.PolylineToFlex.Actions;
using RevitDevelop.Tools.PolylineToFlex.IActions;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Tools.PolylineToFlex
{
    [Transaction(TransactionMode.Manual)]
    public class PolylineToFlexCommand : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var flexDuctType = Document.GetElementsFromClass<FlexDuctType>(true)
                        .FirstOrDefault(x => x.Shape == ConnectorProfileType.Round);
                    var systemType = Document.GetElementsFromClass<MechanicalSystemType>(true)
                        .FirstOrDefault();
                    var levels = Document.GetElementsFromClass<Level>();
                    var service = new ServiceCollection();
                    service.AddSingleton<PolylineToFlexCommand>();
                    service.AddSingleton<IPolylineToFlexAction, PolylineToFlexAction>();
                    var provider = service.BuildServiceProvider();
                    var polylineToFlexAction = provider.GetService<IPolylineToFlexAction>();
                    polylineToFlexAction.Excute(flexDuctType, systemType, 150, levels.FirstOrDefault());
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
