using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace RevitDevelop.Utils.RevEvents
{
    public class RevitCommandEndedMonitor
    {
        private readonly UIApplication _app;

        public bool _initializingCommandMonitor;

        public event EventHandler CommandEnded;
        public RevitCommandEndedMonitor(UIApplication app)
        {
            _app = app;

            _initializingCommandMonitor = true;
            _app.Idling += OnRevitUiApplicationIdling;
        }
        private void OnRevitUiApplicationIdling(object sender, IdlingEventArgs idlingEventArgs)
        {
            if (_initializingCommandMonitor)
            {
                _initializingCommandMonitor = false;
                return;
            }
            _app.Idling -= OnRevitUiApplicationIdling;
            OnCommandEnded();
        }

        protected virtual void OnCommandEnded() => CommandEnded?.Invoke(this, EventArgs.Empty);
    }
}
