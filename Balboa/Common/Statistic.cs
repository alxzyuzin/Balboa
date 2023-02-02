/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Contain infomation from "stats" command result 
 *
  --------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Balboa.Common
{

    public class Statistic 
    {
        public int Artists { get; private set; }            // artists: number of artists
        public int Albums { get; private set; }             // albums: number of albums
        public int Songs { get; private set; }              // songs: number of songs
        public int Uptime { get; private set; }             // uptime: daemon uptime in seconds
        public int DbPlaytime { get; private set; }         // db_playtime: sum of all song times in the db
        public int DbUpdate { get; private set; }           // db_update: last db update in UNIX time
        public DateTime DbUpdateDT { get; private set; }
        public int Playtime { get; private set; }           // playtime: time length of music played
        
        public Statistic()
        {
            
        }

        public void Update(List<string> statsInfo)
        {
            if (statsInfo == null)
                throw new ArgumentNullException(nameof(statsInfo));

            foreach (string item in statsInfo)
            {
                string[] statsitem = item.Split(':');
                switch (statsitem[0])
                {
                    case "artists": Artists = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    case "albums": Albums = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    case "songs": Songs = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    case "uptime": Uptime = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    case "db_playtime": DbPlaytime = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    case "db_update":
                        DbUpdate = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
                        // Unix timestamp is seconds past epoch
                        DateTime db_update_dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        DbUpdateDT = db_update_dt.AddSeconds(DbUpdate).ToLocalTime();
                        break;
                    case "playtime": Playtime = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                }
            }
        }

    } // class Statistic 
}
