using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Events;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Tools.Syns.view;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.Tools.Syns
{
    [Transaction(TransactionMode.Manual)]
    public class SynCommand : ExternalCommand
    {
        private SynsView _view;
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    Application.WorksharedOperationProgressChanged += Application_WorksharedOperationProgressChanged;
                    Application.DocumentSaving += Application_DocumentSaving;
                    Application.DocumentClosing += Application_DocumentClosing;
                    Application.DocumentSynchronizingWithCentral += Application_DocumentSynchronizingWithCentral;
                    _view = new SynsView();
                    _view.ShowActivated.ToString();
                    _view.Show();
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

        private void Application_WorksharedOperationProgressChanged(object sender, Autodesk.Revit.DB.Events.WorksharedOperationProgressChangedEventArgs e)
        {
            IO.ShowWarning("Application_WorksharedOperationProgressChanged");
        }

        private void Application_DocumentSaving(object sender, Autodesk.Revit.DB.Events.DocumentSavingEventArgs e)
        {
            _saveData();
        }

        private void Application_DocumentSynchronizingWithCentral(object sender, Autodesk.Revit.DB.Events.DocumentSynchronizingWithCentralEventArgs e)
        {
            _saveData();
        }
        private void _saveData()
        {
            if (_view == null) return;
            if (!_view.ShowActivated) return;
            IO.ShowInfo(_view.ShowActivated.ToString());
            IO.ShowInfo("Lưu tạm thời");
        }

        private void Application_DocumentClosing(object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs e)
        {
            _view?.Close();
            Application.DocumentSaving -= Application_DocumentSaving;
            Application.DocumentClosing -= Application_DocumentClosing;
            Application.DocumentSynchronizingWithCentral -= Application_DocumentSynchronizingWithCentral;
        }
    }
}
