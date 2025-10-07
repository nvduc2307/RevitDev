using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.Idlings
{
    public static class IdlingLoop
    {
        private static EventHandler<IdlingEventArgs> _handler;
        private static bool _running;
        public static Action ExternalAction;

        public static void Start(UIApplication uiapp)
        {
            if (_running) return;
            _handler = OnIdling;
            if (_handler != null)
            {
                uiapp.Idling += _handler;
                _running = true;
            }
        }

        public static void Stop(UIApplication uiapp)
        {
            if (!_running) return;
            if (_handler != null)
            {
                uiapp.Idling -= _handler;
                _running = false;
                _handler = null;
            }
        }

        private static DateTime _last = DateTime.MinValue;
        private static bool _inHandler = false;

        private static void OnIdling(object sender, IdlingEventArgs e)
        {
            if (_inHandler) return;
            _inHandler = true;

            try
            {
                if ((DateTime.UtcNow - _last).TotalMilliseconds < 33)
                {
                    e.SetRaiseWithoutDelay();
                    return;
                }
                _last = DateTime.UtcNow;
                ExternalAction?.Invoke();
                e.SetRaiseWithoutDelay();
            }
            finally { _inHandler = false; }
        }
    }
}
