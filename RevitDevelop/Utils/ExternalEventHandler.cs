using Autodesk.Revit.UI;

namespace RevitDevelop.Utils
{
    public class ExternalEventHandler : IExternalEventHandler
    {
        public Action Action { get; set; }
        public string Name { get; set; }
        public ExternalEventHandler(string name)
        {
            Name = name;
        }
        public void Execute(UIApplication app)
        {
            Action?.Invoke();
        }

        public string GetName() => Name;
    }
}
