using Android.Content;
using Android.Widget;
using Android.Appwidget;
using Android.Views;
using Android.App;
using Java.Util;
using Java.Lang;

namespace NextAlarmWidget
{
    class WidgetsUpdater
    {

        public static void Update(Context context)
        {
            var nextAlarm = NextAlarm.ObtainFromSystem(context);
            Update(context, nextAlarm);
        }

        public static void Update(Context context, NextAlarm nextAlarm)
        {
            RemoteViews updateViews = new RemoteViews(context.PackageName, Resource.Layout.widget);
            updateViews.SetTextViewText(Resource.Id.alarm_date, nextAlarm.Day);
            if (nextAlarm.Time.Length > 5)
            {
                updateViews.SetTextViewText(Resource.Id.alarm_time_12, nextAlarm.Time);
                updateViews.SetViewVisibility(Resource.Id.alarm_time_12, ViewStates.Visible);
                updateViews.SetViewVisibility(Resource.Id.alarm_time_24, ViewStates.Gone);
            }
            else
            {
                updateViews.SetTextViewText(Resource.Id.alarm_time_24, nextAlarm.Time);
                updateViews.SetViewVisibility(Resource.Id.alarm_time_24, ViewStates.Visible);
                updateViews.SetViewVisibility(Resource.Id.alarm_time_12, ViewStates.Gone);
            }

            updateViews.SetOnClickPendingIntent(Resource.Id.widget_root, nextAlarm.RelayIntent);

            ComponentName thisWidget = new ComponentName(context, Java.Lang.Class.FromType(typeof(Widget)).Name);
            AppWidgetManager manager = AppWidgetManager.GetInstance(context);
            manager.UpdateAppWidget(thisWidget, updateViews);

            var refreshIntent = new Intent(context, typeof(AlarmClockChangedReceiver));
            PendingIntent pendingRefreshIntent = PendingIntent.GetBroadcast(context, 0, refreshIntent, PendingIntentFlags.UpdateCurrent);
            
            Calendar calendar = Calendar.GetInstance(Locale.Default);
            calendar.TimeInMillis = JavaSystem.CurrentTimeMillis();
            calendar.Set(CalendarField.HourOfDay, 0);
            calendar.Set(CalendarField.Minute, 0);
            calendar.Add(CalendarField.DayOfYear, 1);

            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            alarmManager.Set(AlarmType.Rtc, calendar.TimeInMillis, pendingRefreshIntent);
        }
    }
}