using System;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;

using MonoDroid.ColorPickers;
using Android.Appwidget;
using Android.Util;

namespace NextAlarmWidget
{
    public static class PrefsKeys {
        public const string BackgroundColor = "backgroundColor";
        public const string TimeColor = "timeColor";
        public const string DateColor = "dateColor";
        public const string IconColor = "iconColor";
        public const string TimeTextSize = "timeTextSize";
        public const string DateTextSize = "dateTextSize";
        public const string DateUseTodTom = "dateUseTodTom";
        public const string TimeOffset = "timeOffset";

        public const string DefaultValue = "_defaultValue";

        public static ISharedPreferences GetPrefs(Context context) {
            return context.GetSharedPreferences("com.sebcano.NextAlarmWidget.widget_prefs", FileCreationMode.Private);
        }
    };

    [Activity(Label = "@string/settings", Name = "com.sebcano.nextalarmwidget.ConfigurationActivity", Icon = "@drawable/ic_launcher")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_CONFIGURE" })]
    public class ConfigurationActivity : Activity
    {
        int _widgetId;
        ColorSetting[] _tbColorSettings;
        IntSetting[] _tbIntSettings;
        BoolSetting[] _tbBoolSettings;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.configuration_layout);

            var prefs = PrefsKeys.GetPrefs(this);

            _tbColorSettings = new ColorSetting[] {
                new ColorSetting(this, Resource.Id.backround_picker, Resource.Color.background, true, PrefsKeys.BackgroundColor),
                new ColorSetting(this, Resource.Id.time_picker, Resource.Color.time, true, PrefsKeys.TimeColor),
                new ColorSetting(this, Resource.Id.date_picker, Resource.Color.date, true, PrefsKeys.DateColor),
                new ColorSetting(this, Resource.Id.icon_picker, Resource.Color.icon, true, PrefsKeys.IconColor)
            };

            _tbIntSettings = new IntSetting[]
            {
                new IntSetting(this, Resource.Id.time_textsize, PrefsKeys.TimeTextSize, -1),
                new IntSetting(this, Resource.Id.date_textsize, PrefsKeys.DateTextSize, -1),
                new IntSetting(this, Resource.Id.time_offset, PrefsKeys.TimeOffset, 0), 
            };

            _tbBoolSettings = new BoolSetting[]
            {
                new BoolSetting(this, Resource.Id.date_usetodtom, PrefsKeys.DateUseTodTom)
            };

            Bundle extras = Intent.Extras;
            if (extras != null) {
                _widgetId = extras.GetInt( AppWidgetManager.ExtraAppwidgetId, AppWidgetManager.InvalidAppwidgetId);
            }

            Intent resultValue = new Intent();
            resultValue.PutExtra(AppWidgetManager.ExtraAppwidgetId, _widgetId);
            SetResult(Result.Canceled, resultValue);

            FindViewById<Button>(Resource.Id.ok_button).Click += ConfigurationActivity_OkClick;
            FindViewById<Button>(Resource.Id.default_button).Click += ConfigurationActivity_DefaultClick;
        }

        private void ConfigurationActivity_OkClick(object sender, EventArgs e)
        {
            var prefs = PrefsKeys.GetPrefs(this);
            var editor = prefs.Edit();
            foreach (var colorSetting in _tbColorSettings)
                colorSetting.Save(editor, _widgetId);
            foreach (var intSetting in _tbIntSettings)
                intSetting.Save(editor, _widgetId);
            foreach (var boolSetting in _tbBoolSettings)
                boolSetting.Save(editor, _widgetId);

            editor.Commit();

            WidgetsUpdater.Update(this);

            Intent resultValue = new Intent();
            resultValue.PutExtra(AppWidgetManager.ExtraAppwidgetId, _widgetId);
            SetResult(Result.Ok, resultValue);
            Finish();
        }

        private void ConfigurationActivity_DefaultClick(object sender, EventArgs e)
        {
            foreach (var colorSetting in _tbColorSettings)
                colorSetting.ResetDefault();
            foreach (var intSetting in _tbIntSettings)
                intSetting.ResetDefault();
            foreach (var boolSetting in _tbBoolSettings)
                boolSetting.ResetDefault();
        }
    }

    class ColorSetting
    {
        ColorPickerPanelView _colorPicker;
        string _savekey;
        Activity _activity;
        int _idDefaultColor;

        public ColorSetting(Activity activity, int idPicker, int idDefaultColor, bool useAlpha, string saveKey)
        {
            _activity = activity;
            _idDefaultColor = idDefaultColor;
            _savekey = saveKey;
            _colorPicker = activity.FindViewById<ColorPickerPanelView>(idPicker);

            using (var prefs = PrefsKeys.GetPrefs(activity))
            {
                var defaultColor = new Color(prefs.GetInt(_savekey + PrefsKeys.DefaultValue, activity.GetCompatColor(idDefaultColor)));

                _colorPicker.Color = defaultColor;
                _colorPicker.PanelClicked += (sender, e) =>
                {
                    var colorPickerDialog = new ColorPickerDialog(activity, _colorPicker.Color);
                    colorPickerDialog.AlphaSliderVisible = useAlpha;
                    colorPickerDialog.ColorChanged += (o, args) => _colorPicker.Color = args.Color;
                    colorPickerDialog.Show();
                };
            }
        }

        public void Save(ISharedPreferencesEditor editor, int appWidgetId)
        {
            Log.Debug("saving", _savekey + appWidgetId);
            var intColor = _colorPicker.Color.ToArgb();
            editor.PutInt(_savekey + appWidgetId, intColor);
            editor.PutInt(_savekey + PrefsKeys.DefaultValue, intColor);
        }

        public void ResetDefault()
        {
            var defaultColor = new Color(_activity.GetCompatColor(_idDefaultColor));
            _colorPicker.Color = defaultColor;
        }
    }
    
    class IntSetting
    {
        EditText _editText;
        string _savekey;

        public IntSetting(Activity activity, int idEdit, string saveKey, int defaultValue)
        {
            _savekey = saveKey;
            using (var prefs = PrefsKeys.GetPrefs(activity))
            {
                _editText = activity.FindViewById<EditText>(idEdit);
                var savedValue = prefs.GetInt(_savekey + PrefsKeys.DefaultValue, defaultValue);
                if (savedValue != defaultValue)
                    _editText.Text = savedValue.ToString();
            }
        }

        public void Save(ISharedPreferencesEditor editor, int appWidgetId)
        {
            Log.Debug("saving", _savekey + appWidgetId);
            int iNewValue;
            if (int.TryParse(_editText.Text, out iNewValue))
            {
                editor.PutInt(_savekey + appWidgetId, iNewValue);
                editor.PutInt(_savekey + PrefsKeys.DefaultValue, iNewValue);
            }
        }

        public void ResetDefault()
        {
            _editText.Text = "";
        }
    }

    class BoolSetting
    {
        CheckBox _checkBox;
        string _savekey;

        public BoolSetting(Activity activity, int idCheckBox, string saveKey)
        {
            _savekey = saveKey;
            using (var prefs = PrefsKeys.GetPrefs(activity))
            {
                _checkBox = activity.FindViewById<CheckBox>(idCheckBox);
                var defaultValue = prefs.GetBoolean(_savekey + PrefsKeys.DefaultValue, true);
                _checkBox.Checked = defaultValue;                
            }
        }

        public void Save(ISharedPreferencesEditor editor, int appWidgetId)
        {
            Log.Debug("saving", _savekey + appWidgetId);
            var boolValue = _checkBox.Checked;
            editor.PutBoolean(_savekey + appWidgetId, boolValue);
            editor.PutBoolean(_savekey + PrefsKeys.DefaultValue, boolValue);
        }

        public void ResetDefault()
        {
            _checkBox.Checked = true;
        }
    }
}
