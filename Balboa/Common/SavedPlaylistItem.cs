/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс для хранения данных общего элемента списка
 * Элемент списка не требующий специального списка полей
 *
  --------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Balboa.Common
{
    public class SavedPlaylistItem
    {
        public string FileName { get; set; }
        public string LastModificationDate { get; set; }
  
        public void Update(List<string> response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            int i =0;

            do
            {
                string[] items = response[i].Split(':');
                string tagname = items[0].ToLower();
                string tagvalue = items[1].Trim();

                switch (tagname)
                {
                    case "playlist":
                        FileName = tagvalue;
                        break;
                    case "last-modified":
                        LastModificationDate = new StringBuilder(tagvalue).Replace('T', ' ').Append(':')
                                                                          .Append(items[2]).Append(':')
                                                                          .Append(items[3]).Replace('Z', ' ').ToString();
                        break;
                }
                i++;
            }
            while ((i < response.Count) && (!response[i].StartsWith("playlist", StringComparison.OrdinalIgnoreCase)));

            response.RemoveRange(0, i);
        }
    }
}

