/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс со служебными функциями
 *
 --------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Balboa.Common
{
    public static class Utils
    {
        /// <summary>
        /// Извлекает имя файла из строки передаваемой во входном параметре
        /// Строка должна содержать корректное имя файла и путь
        /// </summary>
        /// <param name="s">
        /// Строка для разбора
        /// </param>
        /// <returns></returns>
        public static string ExtractFileName(string s, bool removefileextention)
        {
            string[] fileparts = s.Split('/');
            string res = fileparts[fileparts.Length - 1];
            if (removefileextention)
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
        public static string ExtractFilePath(string s)
        {
            if (s.Length == 0)
                return s;

            string[] fileparts = s.Split('/');
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
        public static string BuildFilePath(List<string> pathitems)
        {
            string filepath = string.Empty;

            if (pathitems.Count > 0)
            { 
                filepath += pathitems[0];
            }
            int i = 1;
            while  (i < pathitems.Count)
            {
                filepath += ("/"+ pathitems[i++]);
            }
            return filepath;
        }

        public static string SecTo_hh_mm_ss(object value)
        {
            string res = string.Empty;
            try
            {
                string ss = string.Empty;
                string ms = string.Empty;

                float ts = float.Parse(value.ToString());

                int s = (int)(ts % 60);
                ss = s.ToString();
                if (s < 10)
                    ss = "0" + ss;
                 
                int m = (int)((ts /60) % 60);
                ms = m.ToString();
                if (m < 10)
                    ms = "0" + ms;

                int h = (int)(ts / 3600);

                res = ms+ " : " + ss;
                if (h > 0)
                    res = h.ToString() + " : " + res;

            }
            catch(Exception )
            {
                res = "Conversion error";
            }

            return res;
        }

        public static async Task<BitmapImage> GetFolderImage(string musiccollectionfolder, string foldername, string albumcoverfilenames)
        {
            StorageFile file = null;
            StringBuilder sb = new StringBuilder(musiccollectionfolder);
            sb.Append('\\');
            sb.Append(foldername);
            sb.Replace('/', '\\');
            sb.Append('\\');

            int pathlength = sb.Length;

            string[] CoverFileNames = albumcoverfilenames.Split(';');

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
                catch (System.UnauthorizedAccessException )
                {

                }
            }
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);
                    return bitmapImage;
                }
            }
            else
            {
                return null;
            }
        }

        public static async Task<IRandomAccessStream> GetFolderImageStream(string musiccollectionfolder, string foldername, string albumcoverfilenames)
        {
            StorageFile file = null;
            StringBuilder sb = new StringBuilder(musiccollectionfolder);
            sb.Append('\\');
            sb.Append(foldername);
            sb.Replace('/', '\\');
            sb.Append('\\');

            int pathlength = sb.Length;

            string[] CoverFileNames = albumcoverfilenames.Split(';');

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
                catch (System.UnauthorizedAccessException )
                {

                }
            }
            if (file != null)
            {
                IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                return fileStream;
            }
            else
            {
                return null;
            }
        }
    }
}
