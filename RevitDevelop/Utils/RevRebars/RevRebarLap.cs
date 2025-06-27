using Autodesk.Revit.DB.Structure;

namespace RevitDevelop.Utils.RevRebars
{
    public class RevRebarLap
    {
        public int HostStartID { get; set; }
        public int HostEndID { get; set; }
        public bool LapWeldStart { get; set; }
        public bool LapWeldEnd { get; set; }
        public bool LapCouplerStart { get; set; }
        public bool LapCouplerEnd { get; set; }
        public bool LapPlateStart { get; set; }
        public bool LapPlateEnd { get; set; }
        public static RevRebarLap GetRevRebarLap(Rebar rebar)
        {
            try
            {
                var weldStart = rebar.LookupParameter(Properties.ParameterRebarLap.LAP_WELD_START).AsInteger() == 1 ? true : false;
                var weldEnd = rebar.LookupParameter(Properties.ParameterRebarLap.LAP_WELD_END).AsInteger() == 1 ? true : false;
                var couplerStart = rebar.LookupParameter(Properties.ParameterRebarLap.LAP_COUPLER_START).AsInteger() == 1 ? true : false;
                var couplerEnd = rebar.LookupParameter(Properties.ParameterRebarLap.LAP_COUPLER_END).AsInteger() == 1 ? true : false;
                var plateStart = rebar.LookupParameter(Properties.ParameterRebarLap.LAP_PLATE_START).AsInteger() == 1 ? true : false;
                var plateEnd = rebar.LookupParameter(Properties.ParameterRebarLap.LAP_PLATE_END).AsInteger() == 1 ? true : false;

                return new RevRebarLap()
                {
                    LapWeldStart = weldStart,
                    LapWeldEnd = weldEnd,
                    LapCouplerStart = couplerStart,
                    LapCouplerEnd = couplerEnd,
                    LapPlateStart = plateStart,
                    LapPlateEnd = plateEnd,
                };
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
