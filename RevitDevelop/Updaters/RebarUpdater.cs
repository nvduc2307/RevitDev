using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.Updaters
{
    public class RebarUpdater : IUpdater
    {
        static AddInId m_appId;
        static UpdaterId m_updaterId;
        public RebarUpdater(AddInId id)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, new Guid("56644677-e4f5-44fd-a89d-ed3e8b372afa"));
        }
        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            //
            foreach (ElementId addedElemId in data.GetAddedElementIds())
            {
                var rb = doc.GetElement(addedElemId) as Rebar;
                if (rb != null)
                    IO.ShowWarning($"{rb.Id} is changed");
            }
        }

        public string GetAdditionalInformation() => "Rebar Updater";
        public ChangePriority GetChangePriority() => ChangePriority.Rebar;
        public UpdaterId GetUpdaterId() => m_updaterId;
        public string GetUpdaterName() => "Rebar Updater";
        public static void Init(UIControlledApplication application)
        {
            var rebarUpdater = new RebarUpdater(application.ActiveAddInId);
            UpdaterRegistry.RegisterUpdater(rebarUpdater);
            ElementClassFilter rebarFilter = new ElementClassFilter(typeof(Rebar));
            UpdaterRegistry.AddTrigger(rebarUpdater.GetUpdaterId(), rebarFilter,
                                   Element.GetChangeTypeElementAddition());
        }
        public static void Dispose(UIControlledApplication application)
        {
            RebarUpdater updater = new RebarUpdater(application.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
        }
    }
}
