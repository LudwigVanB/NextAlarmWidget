using System;

using Android.App;
using Android.Content;
using Android.Util;
using Android.Provider;

namespace NextAlarmWidget
{
    class NextAlarm
    {
        private const string TAG = "Widget.NextAlarm";

        public string Time { get; private set; }
        public string Day { get; private set; }
        public PendingIntent Intent { get; private set;  }

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
                if (nowDate.Date == alarmDateTime.Date)
                {
                    nextAlarm.Day = context.GetString(Resource.String.today_abbreviation);
                }
                else
                {
                    var diffInDays = (alarmDateTime.Date - nowDate.Date).Days;
                    Log.Debug(TAG, "diffInDays= " + diffInDays);
                    if (diffInDays == 1)
                    {
                        nextAlarm.Day = context.GetString(Resource.String.tomorrow_abbreviation);
                    }
                    else if (diffInDays < 7)
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
                }

                if (alarmInfo.ShowIntent != null)
                {
                    nextAlarm.Intent = alarmInfo.ShowIntent;
                }
                else
                {
                    nextAlarm.Intent = BuildShowAlarmsIntent(context);
                }
            }
            else
            {
                nextAlarm.Day = "zzz";
                nextAlarm.Time = "--:--";                
                nextAlarm.Intent = BuildShowAlarmsIntent(context);
            }

            return nextAlarm;
        }

        private static PendingIntent BuildShowAlarmsIntent(Context context)
        {
            Intent openClockIntent = new Intent(AlarmClock.ActionShowAlarms);
            return PendingIntent.GetActivity(context, 0, openClockIntent, PendingIntentFlags.UpdateCurrent);
        }

        private static DateTime UnixDateToLocalDateTime(long utcDate)
        {
            var unixStartDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return unixStartDate.AddMilliseconds(utcDate).ToLocalTime();
        }
    }
}