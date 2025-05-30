using Autodesk.Revit.UI;

namespace RevitDevelop.Updaters
{
    public class WallCreationUpdater : IUpdater
    {
        private readonly AddInId _addInId;
        private readonly UpdaterId _updaterId;
        public WallCreationUpdater(AddInId id)
        {
            _addInId = id;
            _updaterId = new UpdaterId(_addInId, new Guid("fe1312b3-a765-4ccd-bbab-4f1aed5b4a61"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            ICollection<ElementId> addedElementIds = data.GetAddedElementIds();

            foreach (ElementId id in addedElementIds)
            {
                Element element = doc.GetElement(id);
                if (element is Wall)
                {
                    TaskDialog.Show("Wall Created", $"A wall with ID {id.ToString()} has been created.");
                }
            }
        }

        public string GetAdditionalInformation() => "Wall creation monitor.";
        public ChangePriority GetChangePriority() => ChangePriority.InteriorWalls;
        public UpdaterId GetUpdaterId() => _updaterId;
        public string GetUpdaterName() => "Wall Creation Updater";
        public static void Init(UIControlledApplication application)
        {
            var updater = new WallCreationUpdater(application.ActiveAddInId);
            UpdaterRegistry.RegisterUpdater(updater);
            ElementClassFilter rebarFilter = new ElementClassFilter(typeof(Wall));
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), rebarFilter,
                                   Element.GetChangeTypeElementAddition());
        }
        public static void Dispose(UIControlledApplication application)
        {
            var updater = new WallCreationUpdater(application.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
        }
    }
}
