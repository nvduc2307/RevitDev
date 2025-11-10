using Autodesk.Revit.UI;

namespace RevitDevelop.Updaters
{
    public class WallEditByUpdater : IUpdater
    {
        private readonly AddInId _addInId;
        private readonly UpdaterId _updaterId;
        public WallEditByUpdater(AddInId id)
        {
            _addInId = id;
            _updaterId = new UpdaterId(_addInId, new Guid("eb5c2750-f5f2-41d2-b0c6-1d3eb662d964"));
        }
        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            ICollection<ElementId> addedElementIds = data.GetModifiedElementIds();
            foreach (ElementId id in addedElementIds)
            {
                Element element = doc.GetElement(id);
                if (element is Wall)
                {
                    TaskDialog.Show("Wall Created", $"A wall with ID {id.ToString()} has been created.");
                }
            }
        }
        public string GetAdditionalInformation() => "WallEditByUpdater";
        public ChangePriority GetChangePriority() => ChangePriority.InteriorWalls;
        public UpdaterId GetUpdaterId() => _updaterId;
        public string GetUpdaterName() => "WallEditByUpdater";
        public static void Init(UIControlledApplication application)
        {
            var updater = new WallEditByUpdater(application.ActiveAddInId);
            UpdaterRegistry.RegisterUpdater(updater);
            ElementClassFilter wallFilter = new ElementClassFilter(typeof(Wall));
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), wallFilter,
                                   Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.EDITED_BY)));
        }
        public static void Dispose(UIControlledApplication application)
        {
            var updater = new WallEditByUpdater(application.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
        }
    }
}
