using Autodesk.Revit.UI;

namespace RevitDevelop.Utils.RevExternalEvents
{
    public class ExternalEventHandler : IExternalEventHandler
    {
        private static ExternalEventHandler _instance;

        private static ExternalEvent _externalEvent;

        private static Action _action;

        public static ExternalEventHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ExternalEventHandler();
                }

                return _instance;
            }
        }

        public async void Run(int millisecondsDelay = 20)
        {
            _externalEvent.Raise();
            while (_externalEvent.IsPending)
            {
                await Task.Delay(millisecondsDelay);
            }
        }

        public ExternalEvent Create()
        {
            if (_externalEvent == null)
            {
                _externalEvent = ExternalEvent.Create(Instance);
            }

            return _externalEvent;
        }

        public void SetAction(Action action)
        {
            _action = action;
        }

        public void Execute(UIApplication app)
        {
            if (app.ActiveUIDocument == null)
            {
                TaskDialog.Show("Notification", "No document");
            }
            else
            {
                _action();
            }
        }

        public string GetName()
        {
            return "ExternalEventHandler";
        }
    }
}
