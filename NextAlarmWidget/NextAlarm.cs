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

        public string Time { get; private set; }
        public string Day { get; private set; }
        public string DayAbbreviated { get; private set; }
        public PendingIntent RelayIntent { get; private set; }

        private PendingIntent _showAlarmIntent;
        private PendingIntent _showAllAlarmsIntent;


        public static NextAlarm ObtainFromSystem(Context context)
        {
            var nextAlarm = new NextAlarm();

            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            var alarmInfo = alarmManager.NextAlarmClock;
            if (alarmInfo != null)
            {

                long alarmUnixTime = alarmInfo.TriggerTime;
                var alarmDateTime = UnixDateToLocalDateTime(alarmUnixTime);
                bool is24hFormat = Android.Text.Format.DateFormat.Is24HourFormat(context);
                if (is24hFormat)
                {
                    nextAlarm.Time = alarmDateTime.ToString("H:mm");
                }
                else
                {
                    nextAlarm.Time = alarmDateTime.ToString("h:mm t");
                }

                var nowDate = DateTime.Now;
                var diffInDays = (alarmDateTime.Date - nowDate.Date).Days;
                if (diffInDays <7)
                {
                    nextAlarm.Day = alarmDateTime.ToString("ddd");
                    if (!nextAlarm.Day.EndsWith("."))
                    {
                        nextAlarm.Day += ".";
                    }
                }
                else
                {
                    nextAlarm.Day = alarmDateTime.ToString("d");
                }
                nextAlarm.DayAbbreviated = nextAlarm.Day;
                if (nowDate.Date == alarmDateTime.Date)
                {
                    nextAlarm.DayAbbreviated = context.GetString(Resource.String.today_abbreviation);
                }
                else if(diffInDays == 1)
                {
                    nextAlarm.DayAbbreviated = context.GetString(Resource.String.tomorrow_abbreviation);
                }                

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

            var relayIntent = new Intent(context, typeof(AlarmRelayService));
            nextAlarm.RelayIntent = PendingIntent.GetService(context, 0, relayIntent, PendingIntentFlags.UpdateCurrent);

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
                    _showAlarmIntent.Send(Result.Ok, finishedHandler, new Handler(looper)); ;
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