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
using Windows.UI.Xaml.Data;

namespace Balboa.Common

{
    public class TimeConverter: IValueConverter
    {
        public object Convert(object value, System.Type type, object parameter, string language)
        {
            string s = Utils.SecTo_hh_mm_ss(value);
            return s;
        }

        public object ConvertBack(object value, System.Type type, object parameter, string language)
        {
            throw new NotImplementedException();
        }
   }
}
