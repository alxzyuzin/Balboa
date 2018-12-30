/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс выполняет преобразование значение интервала представленное в секундах 
 * в значение в формате HH MM SS 
 *
  --------------------------------------------------------------------------*/

using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace Balboa.Common

{
    
    public class TimeConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string s = SecToHHMMSS(value);
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static string SecToHHMMSS(object value)
        {
            string _modName = "TimeConverter";

            if (value == null)
                throw new BalboaNullValueException(_modName, "SecTo_HH_MM_SS", "107", "value");

            string res = string.Empty;
            string ss = string.Empty;
            string ms = string.Empty;
            try
            {
                float ts = float.Parse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);

                int s = (int)(ts % 60);
                ss = s.ToString(CultureInfo.InvariantCulture);
                if (s < 10)
                    ss = "0" + ss;

                int m = (int)((ts / 60) % 60);
                ms = m.ToString(CultureInfo.InvariantCulture);
                if (m < 10)
                    ms = "0" + ms;

                int h = (int)(ts / 3600);

                res = ms + " : " + ss;
                if (h > 0)
                    res = h.ToString(CultureInfo.InvariantCulture) + " : " + res;

            }
            catch (Exception e)
            {
                throw new BalboaException(_modName, "SecToHHMMSS", "136", e.Message);
            }

            return res;
        }

    }

}
