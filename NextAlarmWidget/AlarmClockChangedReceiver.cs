using Android.App;
using Android.Content;
using Android.Widget;

namespace NextAlarmWidget
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] {
        Android.App.AlarmManager.ActionNextAlarmClockChanged        
    })]
    public class AlarmClockChangedReceiver : BroadcastReceiver
    {
        private static int _nbCalled = 0;

        public override void OnReceive(Context context, Intent intent)
        {            
            WidgetsUpdater.Update(context);
#if DEBUG
            _nbCalled++;
            var toastMessage = string.Format("Received intent! action={0} className={1} nbCalled={2}",
                 intent.Action, intent.Component.ClassName, _nbCalled);
            Toast.MakeText(context, toastMessage, ToastLength.Long).Show();
#endif
        }
    }
}