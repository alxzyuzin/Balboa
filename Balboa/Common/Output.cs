/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс - для хранения данных Output MPD 
 *
 --------------------------------------------------------------------------*/
using System;
using System.Globalization;
using System.ComponentModel;

namespace Balboa.Common
{
    public class Output: INotifyPropertyChanged, IUpdatable
    {
        private const string _modName = "Output.cs";
        //

        public event PropertyChangedEventHandler PropertyChanged;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public int    Id { get; set; }
        public string Name { get; set; }
        public bool   Enabled { get; set; }

        ///<summary>
        ///  Забирает из ответа сервера данные по одному выходу и 
        /// распределяет их по свойствам экземпляра
        /// Обработанные данные удаляются из ответа
        ///</summary>
        public void Update(MpdResponseCollection response)
        {
            if (response == null)
                throw new BalboaNullValueException(_modName, "Update", "40", "responce");

            int i = 0;
            do
            {
                string[] items = response[i].Split(':');
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
                while ((i < response.Count) && (!response[i].StartsWith("outputid", StringComparison.OrdinalIgnoreCase)));
            response.RemoveRange(0, i);
        }
    }
}
