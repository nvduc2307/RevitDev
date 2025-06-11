using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using RevitDevelop.Utils.BoundingBoxs;
using RevitDevelop.Utils.Entities;
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
                        var lap = doc.GetElement(addedElemId);
                        if (lap == null)
                            continue;
                        var lapInfoSchema = SchemaInfo.ReadAll(m_schemaInfoLap.SchemaBase, m_schemaInfoLap.SchemaField, lap);
                        if (lapInfoSchema == null) continue;
                        var lapInfo = JsonConvert.DeserializeObject<RevLap>(lapInfoSchema.Value);
#if REVIT2024 || REVIT2025
                        var rb = doc.GetElement(new ElementId(long.Parse(lapInfo.Hostid.ToString()))) as Rebar;
#else
                        var rb = doc.GetElement(new ElementId(int.Parse(lapInfo.Hostid.ToString()))) as Rebar;
#endif
                        var cs = rb.GetLinesOrigin();
                        var sp = cs.FirstOrDefault().GetEndPoint(0);
                        var ep = cs.LastOrDefault().GetEndPoint(1);
                        var originBox = new BoxElement(lap).LineBox.Mid();
                        var d1 = originBox.DistanceTo(sp);
                        var d2 = originBox.DistanceTo(ep);
                        var lapPos = d1 > d2 ? RevLapPositionType.Start : RevLapPositionType.End;
                        var lapType = (RevRebarLapType)lapInfo.LapType;
                        var rbLapInfo = RevRebarLap.GetRevRebarLap(rb);
                        switch (lapPos)
                        {
                            case RevLapPositionType.Start:
                                switch (lapType)
                                {
                                    case RevRebarLapType.Weld:
                                        rbLapInfo.LapWeldStart = false;
                                        rb.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_START, "0");
                                        break;
                                    case RevRebarLapType.Coupler:
                                        rbLapInfo.LapCouplerStart = false;
                                        rb.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_START, "0");
                                        break;
                                    case RevRebarLapType.PlateStop:
                                        rbLapInfo.LapPlateStart = false;
                                        rb.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_START, "0");
                                        break;
                                }
                                break;
                            case RevLapPositionType.End:
                                switch (lapType)
                                {
                                    case RevRebarLapType.Weld:
                                        rbLapInfo.LapWeldEnd = false;
                                        rb.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_END, "0");
                                        break;
                                    case RevRebarLapType.Coupler:
                                        rbLapInfo.LapCouplerEnd = false;
                                        rb.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_END, "0");
                                        break;
                                    case RevRebarLapType.PlateStop:
                                        rbLapInfo.LapPlateEnd = false;
                                        rb.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_END, "0");
                                        break;
                                }
                                break;
                        }
                        m_schemaInfoRebar.SchemaField.Value = JsonConvert.SerializeObject(rbLapInfo);
                        SchemaInfo.Write(m_schemaInfoRebar.SchemaBase, rb, m_schemaInfoRebar.SchemaField);
                        //RevLap.CreateLap(rb, rbLapInfo);
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
