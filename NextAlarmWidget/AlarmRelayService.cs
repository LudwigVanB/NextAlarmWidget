using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace NextAlarmWidget
{
    [Service]
    class AlarmRelayService : IntentService

    {
        protected override void OnHandleIntent(Intent intent)
        {
            var nextAlarm = NextAlarm.ObtainFromSystem(this);
            nextAlarm.Show(this);
            WidgetsUpdater.Update(this, nextAlarm);
        }
    }
}