using Autodesk.Revit.DB;
using RevitDevelop.Tools.TestEdittorStatus.Models;
using RevitDevelop.Utils.FilterElementsInRevit;

namespace RevitDevelop.Tools.TestEdittorStatus.Actions
{
    public partial class TestEdittorStatusAction
    {
        private TestEdittorStatusCommand _cmd;
        public TestEdittorStatusAction(TestEdittorStatusCommand cmd)
        {
            _cmd = cmd;
        }
        public List<ObjectStatus> GetObjects()
        {
            var results = new List<ObjectStatus>();
            try
            {
                var walls = _cmd.Document.GetElementsFromClass<Wall>(false);
                if (!walls.Any()) throw new Exception();
                foreach (var wall in walls)
                {
                    var tip = WorksharingUtils.GetWorksharingTooltipInfo(_cmd.Document, wall.Id);
                    var obj = new ObjectStatus();
                    obj.Id = long.Parse(wall.Id.ToString());
                    obj.Name= wall.Name;
                    obj.Creator = tip.Creator;
                    obj.Editor = tip.Owner;
                    results.Add(obj);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return results;
        }
        public void UpdateObjectStatus(List<ObjectStatus> objectStatuses)
        {
            foreach (var objectStatus in objectStatuses)
            {
                if (objectStatus.Editor != string.Empty) continue;
                var id = new ElementId(objectStatus.Id);
                var tip = WorksharingUtils.GetWorksharingTooltipInfo(_cmd.Document, id);
                objectStatus.Editor = tip.Owner;
            }
        }
        public void Syn()
        {
            var tso = new TransactWithCentralOptions();
            var rql = new RelinquishOptions(true)
            {
                StandardWorksets = true,
                ViewWorksets = true,
                FamilyWorksets = true,
                UserWorksets = true,
                CheckedOutElements = true
            };
            var sco = new SynchronizeWithCentralOptions()
            {
                Comment = "Auto-sync via API",
                Compact = false,
            };
            _cmd.Document.SynchronizeWithCentral(tso, sco);

        }
        public void Reload()
        {
            var options = new ReloadLatestOptions();
            _cmd.Document.ReloadLatest(options);
        }
    }
}
