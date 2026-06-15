using Nice3point.Revit.Extensions.UI;
using Nice3point.Revit.Toolkit.External;

namespace RevitDevelop
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            _Init();
        }
        public void _Init()
        {
            var panel = Application.CreatePanel("Tools", "Demo");
            panel.AddPushButton<Tools.Schedules.ScheduleCmd>("Schedule")
                .SetImage("/DPtools;component/Resources/Icons/lg16.png")
                .SetLargeImage("/DPtools;component/Resources/Icons/lg32.png");
        }
    }
}