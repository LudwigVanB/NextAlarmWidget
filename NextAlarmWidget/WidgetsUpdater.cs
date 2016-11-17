using Android.Content;
using Android.Widget;
using Android.Appwidget;
using Android.Views;

namespace NextAlarmWidget
{
    class WidgetsUpdater
    {

        public static void Update(Context context)
        {
            var nextAlarm = NextAlarm.ObtainFromSystem(context);

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
            
            updateViews.SetOnClickPendingIntent(Resource.Id.widget_root, nextAlarm.Intent);

            ComponentName thisWidget = new ComponentName(context, Java.Lang.Class.FromType(typeof(Widget)).Name);
            AppWidgetManager manager = AppWidgetManager.GetInstance(context);
            manager.UpdateAppWidget(thisWidget, updateViews);
        }
    }
}