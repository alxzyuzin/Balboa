/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс со служебными функциями
 *
 --------------------------------------------------------------------------*/

using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Balboa.Common
{
    internal static class Utilities
    {
        private const string _modName = "Utils.cs";
        /// <summary>
        /// Извлекает имя файла из строки передаваемой во входном параметре
        /// Строка должна содержать корректное имя файла и путь
        /// </summary>
        /// <param name="s">
        /// Строка для разбора
        /// </param>
        /// <returns></returns>
        public static string ExtractFileName(string path, bool removeFileExtension)
        {
            if (path == null)
                throw new BalboaNullValueException(_modName, "ExtractFileName", "36", "path");

            string[] fileparts = path.Split('/');
            string res = fileparts[fileparts.Length - 1];
            if (removeFileExtension)
            {
                if (res.Contains("."))
                { 
                    string[] filenameparts = res.Split('.');
                    res = filenameparts[filenameparts.Length - 2];
                }
            }
            return res.Trim();
        }

        /// <summary>
        /// Извлекает имя файла из строки передаваемой во входном параметре
        /// Строка должна содержать корректное имя файла и путь
        /// </summary>
        /// <param name="s">
        /// Строка для разбора
        /// </param>
        /// <returns></returns>
        public static string ExtractFilePath(string path)
        {
            if (path == null)
                throw new BalboaNullValueException(_modName, "ExtractFilePath", "62", "path");

            if (path.Length == 0)
                return path;

            string[] fileparts = path.Split('/');
            StringBuilder filepath = new StringBuilder(fileparts[0]);
            for (int i=1; i< fileparts.Length-1;i++)
            {
                filepath.Append('\\');
                filepath.Append(fileparts[i]);
            }
             return filepath.ToString();
        }

        /// <summary>
        /// Строит путь к файлам из элементов списка 
        /// </summary>
        /// <param name="pathitems">
        /// Список элементов пути
        /// </param>
        /// <returns></returns>
        /// 
        public static string BuildFilePath(List<string> pathItems)
        {
            if (pathItems == null)
                throw new BalboaNullValueException(_modName, "BuildFilePath", "88", "pathItems");

            string filepath = string.Empty;

            if (pathItems.Count > 0)
//            { 
                filepath += pathItems[0];
//            }
            int i = 1;
            while  (i < pathItems.Count)
//            {
                filepath += ("/"+ pathItems[i++]);
//            }
            return filepath;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "HHMMSS")]
        public static string SecToHHMMSS(object value)
        {
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
                 
                int m = (int)((ts /60) % 60);
                ms = m.ToString(CultureInfo.InvariantCulture);
                if (m < 10)
                    ms = "0" + ms;

                int h = (int)(ts / 3600);

                res = ms+ " : " + ss;
                if (h > 0)
                    res = h.ToString(CultureInfo.InvariantCulture) + " : " + res;

            }
            catch(Exception  e)
            {
                throw new BalboaException(_modName, "SecToHHMMSS", "136", e.Message);
            }

            return res;
        }

        public static async Task<BitmapImage> GetFolderImage(string musicCollectionFolder, string folderName, string albumCoverFileNames)
        { 
            StorageFile file = null;
            while (musicCollectionFolder.EndsWith("\\", StringComparison.Ordinal))
                musicCollectionFolder = musicCollectionFolder.Substring(0, musicCollectionFolder.Length - 1);

            StringBuilder sb = new StringBuilder(musicCollectionFolder);
            
            sb.Append('\\');
            sb.Append(folderName);
            sb.Replace('/', '\\');
            sb.Append('\\');

            int pathlength = sb.Length;

            string[] CoverFileNames = albumCoverFileNames.Split(';');

            foreach (string albumCoverFileName in CoverFileNames)
            {
                try
                {
                    sb.Append(albumCoverFileName);
                    file = await StorageFile.GetFileFromPathAsync(sb.ToString());
                    break;
                }
                catch (FileNotFoundException )
                {
                    sb.Remove(pathlength, albumCoverFileName.Length);
                }
            }
            if (file == null)
                return null;
            using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                return bitmapImage;
            }
        }

        public static async Task<IRandomAccessStream> GetFolderImageStream(string musicCollectionFolder, string folderName, string albumCoverFileNames)
        {
            StorageFile file = null;
            StringBuilder sb = new StringBuilder(musicCollectionFolder);
            sb.Append('\\');
            sb.Append(folderName);
            sb.Replace('/', '\\');
            sb.Append('\\');

            int pathlength = sb.Length;

            string[] CoverFileNames = albumCoverFileNames.Split(';');

            foreach (string albumcoverfilename in CoverFileNames)
            {
                try
                {
                    sb.Append(albumcoverfilename);
                    file = await StorageFile.GetFileFromPathAsync(sb.ToString());
                    break;
                }
                catch (FileNotFoundException )
                {
                    sb.Remove(pathlength, albumcoverfilename.Length);
                }
            }

            if (file == null)
                return null;
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            if (fileStream.Size == 0)
                return null;
            return fileStream;
        }

        public static string GetExceptionMsg(Exception e)
        {
            if (e.Message.Contains("\r\n"))
                return e.Message.Substring(0, e.Message.IndexOf("\r\n"));
            else
                return e.Message;
        }

    }
}
