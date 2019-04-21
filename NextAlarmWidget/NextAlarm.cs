using System;

using Android.App;
using Android.Content;
using Android.Util;
using Android.Provider;
using Android.Widget;
using Android.OS;
using Android.Runtime;

namespace NextAlarmWidget
{
    class NextAlarm
    {
        private const string TAG = "Widget.NextAlarm";

        private DateTime? _alarmDateTime;

        public string Time { get; private set; }
        public string Day { get; private set; }
        public string DayAbbreviated { get; private set; }
        public PendingIntent RelayIntent { get; private set; }

        private PendingIntent _showAlarmIntent;
        private PendingIntent _showAllAlarmsIntent;


        public void RefreshDisplay(Context context, int offsetMinutes)
        {
            if (! _alarmDateTime.HasValue) return;

            var alarmDateTime = _alarmDateTime.Value.AddMinutes(offsetMinutes);

            bool is24hFormat = Android.Text.Format.DateFormat.Is24HourFormat(context);
            if (is24hFormat)
            {
                Time = alarmDateTime.ToString("H:mm");
            }
            else
            {
                Time = alarmDateTime.ToString("h:mm t");
            }

            var nowDate = DateTime.Now;
            var diffInDays = (alarmDateTime.Date - nowDate.Date).Days;
            if (diffInDays < 7)
            {
                Day = alarmDateTime.ToString("ddd");
                if (!Day.EndsWith("."))
                {
                    Day += ".";
                }
            }
            else
            {
                Day = alarmDateTime.ToString("d");
            }
            DayAbbreviated = Day;
            if (nowDate.Date == alarmDateTime.Date)
            {
                DayAbbreviated = context.GetString(Resource.String.today_abbreviation);
            }
            else if (diffInDays == 1)
            {
                DayAbbreviated = context.GetString(Resource.String.tomorrow_abbreviation);
            }
        }

        public static NextAlarm ObtainFromSystem(Context context)
        {
            var nextAlarm = new NextAlarm();

            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            var alarmInfo = alarmManager.NextAlarmClock;
            if (alarmInfo != null)
            {
                long alarmUnixTime = alarmInfo.TriggerTime;
                nextAlarm._alarmDateTime = UnixDateToLocalDateTime(alarmUnixTime);

                if (alarmInfo.ShowIntent != null)
                {
                    nextAlarm._showAlarmIntent = alarmInfo.ShowIntent;
                }
            }
            else
            {
                nextAlarm.Time = "--:--";                
            }
            nextAlarm._showAllAlarmsIntent = BuildShowAlarmsSystemIntent(context);

            var relayIntent = new Intent(context, typeof(TapReceiver));
            nextAlarm.RelayIntent = PendingIntent.GetBroadcast(context, 0, relayIntent, PendingIntentFlags.UpdateCurrent);

            return nextAlarm;
        }

        private static PendingIntent BuildShowAlarmsSystemIntent(Context context)
        {
            Intent openClockIntent = new Intent(AlarmClock.ActionShowAlarms);
            return PendingIntent.GetActivity(context, 0, openClockIntent, PendingIntentFlags.UpdateCurrent);
        }

        private static DateTime UnixDateToLocalDateTime(long utcDate)
        {
            var unixStartDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return unixStartDate.AddMilliseconds(utcDate).ToLocalTime();
        }

        public void Show(Context context)
        {
            if (_showAlarmIntent != null)
            {
                HandlerThread handlerThread = new HandlerThread("Intent sending handler");
                try
                {
                    handlerThread.Start();
                    Looper looper = handlerThread.Looper;
                    var finishedHandler = new OnFinished(context, _showAllAlarmsIntent, handlerThread);
                    _showAlarmIntent.Send(Result.Ok, finishedHandler, new Handler(looper));                    
                }
                catch (Exception)
                {
                    handlerThread.QuitSafely();
                    _showAllAlarmsIntent.Send();
                }
            } else
                _showAllAlarmsIntent.Send();
        }

        class OnFinished : Java.Lang.Object, PendingIntent.IOnFinished
        {
            private Context _context;
            private PendingIntent _fallbackIntent;
            private HandlerThread _handlerThread;

            internal OnFinished(Context context, PendingIntent fallbackIntent, HandlerThread handlerThread)
            {
                _context = context;
                _fallbackIntent = fallbackIntent;
                _handlerThread = handlerThread;
            }

            public void OnSendFinished(PendingIntent pendingIntent, Intent intent, [GeneratedEnum] Result resultCode, string resultData, Bundle resultExtras)
            {
                var packageManager = _context.PackageManager;
                if (intent.ResolveActivity(packageManager) == null)                
                    _fallbackIntent.Send();
                    
                _handlerThread.QuitSafely();                
            }
        }
    }
}