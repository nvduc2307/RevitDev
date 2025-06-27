using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using RevitDevelop.Utils.BoundingBoxs;
using RevitDevelop.Utils.Entities;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.RevParameters;
using RevitDevelop.Utils.RevRebars;

namespace RevitDevelop.Updaters
{
    public class LapElementDeleteUpdater : IUpdater
    {
        static AddInId m_appId;
        static UpdaterId m_updaterId;
        private SchemaInfo m_schemaInfoRebar;
        private SchemaInfo m_schemaInfoLap;
        public LapElementDeleteUpdater(AddInId id)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, new Guid("f7a4d936-dac0-44f1-844e-ba4404fdc636"));
            m_schemaInfoRebar = new SchemaInfo(
                Properties.PropertySchemalInfo.SCHEMAL_REBAR_LAP_GUID,
                Properties.PropertySchemalInfo.SCHEMAL_REBAR_LAP_NAME,
                new SchemaField());
            m_schemaInfoLap = new SchemaInfo(
                Properties.PropertySchemalInfo.SCHEMAL_LAP_GUID,
                Properties.PropertySchemalInfo.SCHEMAL_LAP_NAME,
                new SchemaField());
        }
        public void Execute(UpdaterData data)
        {
            try
            {
                Document doc = data.GetDocument();
                //
                foreach (ElementId addedElemId in data.GetDeletedElementIds())
                {
                    try
                    {
                        var rebar = doc.GetElementsFromClass<Rebar>(false)
                            .Where( x =>
                            {
                                var rebarLapSchemaInfoCheck = SchemaInfo.ReadAll(m_schemaInfoRebar.SchemaBase, m_schemaInfoRebar.SchemaField, x);
                                if (rebarLapSchemaInfoCheck == null)
                                    return false;
                                return rebarLapSchemaInfoCheck.Value.Contains($"{addedElemId}");
                            })
                            .FirstOrDefault();
                        if (rebar == null) continue;
                        var rebarLapSchemaInfo = JsonConvert.DeserializeObject<RevRebarLap>(SchemaInfo.ReadAll(m_schemaInfoRebar.SchemaBase, m_schemaInfoRebar.SchemaField, rebar).Value);
                        if (rebarLapSchemaInfo.HostStartID.ToString() == $"{addedElemId}")
                        {
                            rebarLapSchemaInfo.HostStartID = -1;
                            rebarLapSchemaInfo.LapWeldStart = false;
                            rebarLapSchemaInfo.LapCouplerStart = false;
                            rebarLapSchemaInfo.LapPlateStart = false;
                            rebar.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_START, "0");
                            rebar.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_START, "0");
                            rebar.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_START, "0");
                        }
                        if (rebarLapSchemaInfo.HostEndID.ToString() == $"{addedElemId}")
                        {
                            rebarLapSchemaInfo.HostEndID = -1;
                            rebarLapSchemaInfo.LapWeldEnd = false;
                            rebarLapSchemaInfo.LapCouplerEnd = false;
                            rebarLapSchemaInfo.LapPlateEnd = false;
                            rebar.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_END, "0");
                            rebar.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_END, "0");
                            rebar.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_END, "0");
                        }
                        m_schemaInfoRebar.SchemaField.Value = JsonConvert.SerializeObject(rebarLapSchemaInfo);
                        SchemaInfo.Write(m_schemaInfoRebar.SchemaBase, rebar, m_schemaInfoRebar.SchemaField);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public string GetAdditionalInformation() => "Rebar Lap Delete Updater";
        public ChangePriority GetChangePriority() => ChangePriority.Structure;
        public UpdaterId GetUpdaterId() => m_updaterId;
        public string GetUpdaterName() => "Rebar Lap Delete Updater";
        public static void Init(UIControlledApplication application)
        {
            var updater = new LapElementDeleteUpdater(application.ActiveAddInId);
            UpdaterRegistry.RegisterUpdater(updater);
            ElementClassFilter filter = new ElementClassFilter(typeof(DirectShape));
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter,
                                   Element.GetChangeTypeElementDeletion());
        }
        public static void Dispose(UIControlledApplication application)
        {
            var updater = new LapElementDeleteUpdater(application.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
        }
    }
}
