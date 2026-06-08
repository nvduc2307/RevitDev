using RevitDevelop.Tools.Schedules.model;
using System.Collections.ObjectModel;

namespace RevitDevelop.Tools.Schedules.utils
{
    public class ScheduleDocumentUtils
    {
        public static ObservableCollection<ScheduleDocument> GetDefault(Document document)
        {
            var result = new List<ScheduleDocument>();
            if (document == null) return new ObservableCollection<ScheduleDocument>(result);
            result.Add(new ScheduleDocument() { Document = document, Name = document.Title, Path = document.PathName });
            return new ObservableCollection<ScheduleDocument>(result);
        }
    }
}
