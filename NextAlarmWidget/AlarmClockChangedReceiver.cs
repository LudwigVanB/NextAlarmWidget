using Android.App;
using Android.Content;
using Android.Widget;

namespace NextAlarmWidget
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { Android.App.AlarmManager.ActionNextAlarmClockChanged })]
    public class AlarmClockChangedReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            WidgetsUpdater.Update(context);            
        }
    }
}