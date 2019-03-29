using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace NextAlarmWidget
{
    static class ContextCompat
    {
        public static int GetCompatColor( this Context context, int resId )
        {
            var res = context.Resources;
             if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                return res.GetColor(resId, null);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return res.GetColor(resId);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}