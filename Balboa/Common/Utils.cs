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

        public static string GetExceptionMsg(Exception e)
        {
            if (e.Message.Contains("\r\n"))
                return e.Message.Substring(0, e.Message.IndexOf("\r\n"));
            else
                return e.Message;
        }

    }
}
