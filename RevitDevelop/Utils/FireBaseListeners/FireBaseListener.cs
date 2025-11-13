using Firebase.Database;
using RevitDevelop.Utils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.FireBaseListeners
{
    public class FireBaseListener
    {
        public static Action Action { get; set; }
        public static void ListenRequest(FirebaseClient client)
        {
            client
            .Child("syncEvents")
            .AsObservable<SyncEvent>()
            .Subscribe(d =>
            {
                if (d.Object != null)
                {
                    Action?.Invoke();
                    IO.ShowWarning(d.Object.User);
                }
            });
        }
        public static async Task SendRequest(string user, FirebaseClient _client)
        {
            var e = new SyncEvent
            {
                User = user,
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            await _client.Child("syncEvents").PostAsync(e);
        }
        public class SyncEvent
        {
            public string User { get; set; }
            public string Time { get; set; }
        }
    }
}
