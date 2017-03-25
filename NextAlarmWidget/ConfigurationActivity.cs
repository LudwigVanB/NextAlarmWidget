using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using MonoDroid.ColorPickers;
using Android.Appwidget;
using Android.Util;

namespace NextAlarmWidget
{
    class ColorSetting
    {
        ColorPickerPanelView _colorPicker;
        string _savekey;
        
        public ColorSetting( Activity activity, int idPicker, int idDefaultColor, bool useAlpha, string saveKey )
        {
            _savekey = saveKey;
            _colorPicker = activity.FindViewById<ColorPickerPanelView>(idPicker);
            _colorPicker.Color = new Color( activity.GetCompatColor( idDefaultColor ) );
            _colorPicker.PanelClicked += (sender, e) =>
            {
                var colorPickerDialog = new ColorPickerDialog(activity, _colorPicker.Color);
                colorPickerDialog.AlphaSliderVisible = useAlpha;
                colorPickerDialog.ColorChanged += (o, args) => _colorPicker.Color = args.Color;
                colorPickerDialog.Show();
            };
        }

        public void Save(ISharedPreferencesEditor editor, int appWidgetId)
        {
            Log.Debug("saving", _savekey + appWidgetId);
            editor.PutInt(_savekey + appWidgetId, _colorPicker.Color.ToArgb());
        }

    }


    [Activity(Label = "@string/settings", Name = "com.sebcano.nextalarmwidget.ConfigurationActivity", Icon = "@drawable/ic_launcher")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_CONFIGURE" })]
    public class ConfigurationActivity : Activity
    {
        int _widgetId;
        ColorSetting[] _tbColorSettings;

        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.configuration_layout);

            _tbColorSettings = new ColorSetting[] {
                new ColorSetting(this, Resource.Id.backround_picker, Resource.Color.background, true, "backgroundColor"),
                new ColorSetting(this, Resource.Id.time_picker, Resource.Color.time, true, "timeColor"),
                new ColorSetting(this, Resource.Id.date_picker, Resource.Color.date, true, "dateColor"),
                new ColorSetting(this, Resource.Id.icon_picker, Resource.Color.icon, true, "iconColor")
            };

            Bundle extras = Intent.Extras;
            if (extras != null) {
                _widgetId = extras.GetInt( AppWidgetManager.ExtraAppwidgetId, AppWidgetManager.InvalidAppwidgetId);
            }

            Intent resultValue = new Intent();
            resultValue.PutExtra(AppWidgetManager.ExtraAppwidgetId, _widgetId);
            SetResult(Result.Canceled, resultValue);

            FindViewById<Button>(Resource.Id.ok_button).Click += ConfigurationActivity_Click;
        }

        private void ConfigurationActivity_Click(object sender, EventArgs e)
        {
            var prefs = GetSharedPreferences("com.sebcano.NextAlarmWidget.widget_prefs", FileCreationMode.Private);
            var editor = prefs.Edit();
            foreach (var colorSetting in _tbColorSettings)
            {
                colorSetting.Save(editor, _widgetId);
            }
            editor.Commit();

            WidgetsUpdater.Update(this);

            Intent resultValue = new Intent();
            resultValue.PutExtra(AppWidgetManager.ExtraAppwidgetId, _widgetId);
            SetResult(Result.Ok, resultValue);
            Finish();
        }
    }


    

}