/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс для хранения настроек приложения
 *
  --------------------------------------------------------------------------*/


using System;
using Windows.Storage;

namespace Balboa
{
    public class AppSettings
    {
        public bool   InitialSetupDone { get; set; }
        
        public string ServerName { get; set; } = "localhost";
        public string Port { get; set; } = "6600";
        public string MusicCollectionFolder { get; set; } 
        public string MusicCollectionFolderToken { get; set; } 
        public string ViewUpdateInterval { get; set; } = "500";
        public string Password { get; set; }
        public bool?  DisplayFolderPictures { get; set; } = false;
        public string AlbumCoverFileNames { get; set; } = "folder.jpg;cover.jpg";
               
        public void Restore()
        {
            Object value;

            ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

            value = LocalSettings.Values["InitialSetupDone"];
            InitialSetupDone = (value != null ) ? (bool)value : false;

            if (!InitialSetupDone)
                return;

            value = LocalSettings.Values["ServerName"];
            if (value != null) ServerName = (string)value;

            value = LocalSettings.Values["Port"];
            if (value != null) Port = (string)value;

            value = LocalSettings.Values["ViewUpdateInterval"];
            if (value != null) ViewUpdateInterval = (string)value.ToString();

            value = LocalSettings.Values["Password"];
            if (value != null) Password = (string)value;

            value = LocalSettings.Values["MusicCollectionFolder"];
            if (value != null)     MusicCollectionFolder = (string)value;

            value = LocalSettings.Values["MusicCollectionFolderToken"];
            if (value != null)     MusicCollectionFolderToken = (string)value;

            value = LocalSettings.Values["AlbumCoverFileNames"];
            if (value != null)     AlbumCoverFileNames = (string)value;

            value = LocalSettings.Values["DisplayFolderPictures"];
            if (value != null) DisplayFolderPictures = (bool)value;
        }

        public void Save()
        {
            InitialSetupDone = false;
            ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
                
            LocalSettings.Values["ServerName"] = ServerName;
            LocalSettings.Values["Port"] = Port;
            LocalSettings.Values["ViewUpdateInterval"] = ViewUpdateInterval;
            LocalSettings.Values["Password"] = Password;
            LocalSettings.Values["MusicCollectionFolder"]= MusicCollectionFolder;
            LocalSettings.Values["MusicCollectionFolderToken"] = MusicCollectionFolderToken;
            LocalSettings.Values["AlbumCoverFileNames"] = AlbumCoverFileNames;
            LocalSettings.Values["DisplayFolderPictures"] = DisplayFolderPictures;

            if (ServerName != null & Port != null & ViewUpdateInterval!=null)
                LocalSettings.Values["InitialSetupDone"] = InitialSetupDone = true;
        }

        public void SetDefault()
        {
            InitialSetupDone = false;
            ServerName = "localhost";
            Port = "6600";
            ViewUpdateInterval = "500" ;
            AlbumCoverFileNames = "folder.jpg;cover.jpg";
            Password = "";
            DisplayFolderPictures = false;
        }

    } // class AppSettings
}
