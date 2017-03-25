using Android.Content;
using Android.Widget;
using Android.Appwidget;
using Android.Views;
using Android.App;
using Android.Graphics;
using Java.Util;
using Java.Lang;
using Android.Util;
using Android.Graphics.Drawables;
using Android.Runtime;
using System;

namespace NextAlarmWidget
{
    class WidgetsUpdater
    {

        public static void Update(Context context, int[] appWidgetIds = null)
        {
            var nextAlarm = NextAlarm.ObtainFromSystem(context);
            Update(context, nextAlarm, appWidgetIds);
        }

        public static void Update(Context context, NextAlarm nextAlarm, int[] appWidgetIds = null)
        {
            AppWidgetManager appWidgetManager = AppWidgetManager.GetInstance(context);
            ComponentName thisWidget = new ComponentName(context, Java.Lang.Class.FromType(typeof(Widget)).Name);
            if (appWidgetIds == null)
            {
                appWidgetIds = appWidgetManager.GetAppWidgetIds(thisWidget);
            }
            var prefs = PrefsKeys.GetPrefs(context);

            foreach (var appWidgetId in appWidgetIds)
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

                var dateColor = new Color(prefs.GetInt(PrefsKeys.DateColor + appWidgetId, context.GetCompatColor(Resource.Color.date)));
                updateViews.SetTextColor(Resource.Id.alarm_date, dateColor);
                var dateTextSize = prefs.GetInt(PrefsKeys.DateTextSize + appWidgetId, -1);
                if (dateTextSize != - 1)
                    updateViews.SetTextViewTextSize(Resource.Id.alarm_date, (int)ComplexUnitType.Dip, dateTextSize);


                var timeColor = new Color( prefs.GetInt(PrefsKeys.TimeColor + appWidgetId, context.GetCompatColor(Resource.Color.time)) );
                updateViews.SetTextColor(Resource.Id.alarm_time_24, timeColor);
                updateViews.SetTextColor(Resource.Id.alarm_time_12, timeColor);

                var timeTextSize = prefs.GetInt(PrefsKeys.TimeTextSize + appWidgetId, -1);
                if (timeTextSize != -1)
                    updateViews.SetTextViewTextSize(Resource.Id.alarm_time_24, (int)ComplexUnitType.Dip, timeTextSize);
                    updateViews.SetTextViewTextSize(Resource.Id.alarm_time_12, (int)ComplexUnitType.Dip, timeTextSize);

                var iconColor = new Color(prefs.GetInt(PrefsKeys.IconColor + appWidgetId, context.GetCompatColor(Resource.Color.icon)));
                Bitmap sourceBitmap = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.ic_alarm_white_18dp);
                Bitmap resultBitmap = Bitmap.CreateBitmap(sourceBitmap.Width, sourceBitmap.Height, Bitmap.Config.Argb8888);
                Paint p = new Paint();
                float[] matrix = { 0, 0, 0, iconColor.R/255f, 0,
                                   0, 0, 0, iconColor.G/255f, 0,
                                   0, 0, 0, iconColor.B/255f, 0,
                                   0, 0, 0, iconColor.A/255f, 0 };
                ColorFilter filter = new ColorMatrixColorFilter(matrix);
                p.SetColorFilter(filter);
                Canvas canvas = new Canvas(resultBitmap);
                canvas.DrawBitmap(sourceBitmap, 0, 0, p);
                updateViews.SetImageViewBitmap(Resource.Id.alarm_icon, resultBitmap);
                
                var backgroundColor = new Color(prefs.GetInt(PrefsKeys.BackgroundColor + appWidgetId, context.GetCompatColor(Resource.Color.background)));
                try
                {
                    IntPtr cls = JNIEnv.FindClass("android/widget/RemoteViews");
                    //void setDrawableParameters(int viewId, boolean targetBackground, int alpha, int colorFilter, PorterDuff.Mode mode, int level)
                    IntPtr mth = JNIEnv.GetMethodID(cls, "setDrawableParameters", "(IZIILandroid/graphics/PorterDuff$Mode;I)V");
                    JNIEnv.CallVoidMethod(updateViews.Handle, mth, new JValue(Resource.Id.box), new JValue(true),
                        new JValue(backgroundColor.A), new JValue(backgroundColor.ToArgb()), new JValue(PorterDuff.Mode.Src), new JValue(0));
                } catch (System.Exception e)
                {
                    Log.Error("NextAlarmWidget", "Exception when using reflection " + e.ToString());
                }


                updateViews.SetOnClickPendingIntent(Resource.Id.widget_root, nextAlarm.RelayIntent);
                appWidgetManager.UpdateAppWidget(appWidgetId, updateViews);
            }

            ScheduleMidnightUpdate(context);
        }

        private static void ScheduleMidnightUpdate(Context context)
        {
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
 