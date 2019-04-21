using Android.Content;
using Android.Widget;

namespace NextAlarmWidget
{
    [BroadcastReceiver(Enabled = true)]
    public class TapReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
#if DEBUG
            Toast.MakeText(context, "Received tap intent!", ToastLength.Short).Show();
#endif
            var nextAlarm = NextAlarm.ObtainFromSystem(context);
            nextAlarm.Show(context);
            WidgetsUpdater.Update(context, nextAlarm);
        }
    }
}