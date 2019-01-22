/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс - для хранения данных Output MPD 
 *
 --------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Balboa.Common
{
    public class Output //: INotifyPropertyChanged, IUpdatable
    {
        public int    Id { get; set; }
        public string Name { get; set; }
        public bool   Enabled { get; set; }

        ///<summary>
        ///  Забирает из ответа сервера данные по одному выходу и 
        /// распределяет их по свойствам экземпляра
        /// Обработанные данные удаляются из ответа
        ///</summary>
        public void Update(List<string> responseItems)
        {
            int i = 0;
            do
            {
                string[] items = responseItems[i].Split(':');
                string tagname = items[0].ToLower();
                string tagvalue = items[1].Trim();
                switch (tagname)
                {
                    case "outputid":
                        Id = int.Parse(tagvalue, NumberStyles.Integer, CultureInfo.InvariantCulture);
                        break;
                    case "outputname":
                        Name = tagvalue;
                        break;
                    case "outputenabled": Enabled = (tagvalue == "1") ? true : false; break;
                }
                i++;
            }
            while ((i < responseItems.Count) && (!responseItems[i].StartsWith("outputid", StringComparison.OrdinalIgnoreCase)));
            responseItems.RemoveRange(0, i);
        }
    }
}
