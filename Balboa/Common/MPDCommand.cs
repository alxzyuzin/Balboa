/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс инкапсулирует команды MPD.
 *
 --------------------------------------------------------------------------*/
using System.Globalization;

namespace Balboa.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mpd")]
    public class MpdCommand
    {
        public MpdCommand(string op)
        {
            Op = op;
            Argument1 = null;
            Argument2 = null;
            Argument3 = null;
            FullSyntax = op;
        }

        public MpdCommand(string op, string argument1, string argument2, string argument3)
        {
            Op = op;
            Argument1 = argument1;
            Argument2 = argument2;
            Argument2 = argument3;
            FullSyntax = op + " " + Quote(argument1) + " " + Quote(argument2) + " " + Quote(argument3);
        }

        public MpdCommand(string op, string argument1)
        {
            Op = op;
            Argument1 = argument1;
            Argument2 = null;
            FullSyntax = op + " " + Quote(argument1);
        }

        public MpdCommand(string op, int argument1)
        {
            Op = op;
            Argument1 = argument1.ToString(CultureInfo.InvariantCulture);
            Argument2 = null;
            FullSyntax = op + " " + Quote(argument1);
        }

        public MpdCommand(string op, bool argument1)
        {
            Op = op;
            Argument1 = argument1 ? "1" : "0";
            Argument2 = null;
            FullSyntax = op + " " + Quote(Argument1);
        }

        public MpdCommand(string op, string argument1, string argument2)
        {
            Op = op;
            Argument1 = argument1;
            Argument2 = argument2;
            FullSyntax = op + " " + Quote(argument1) + " " + Quote(argument2);
        }

        public MpdCommand(string op, string argument1, int argument2)
        {
            Op = op;
            Argument1 = argument1;
            Argument2 = argument2.ToString(CultureInfo.InvariantCulture);
            FullSyntax = op + " " + Quote(argument1) + " " + Quote(argument2);
        }

        public MpdCommand(string op, int argument1, int argument2)
        {
            Op = op;
            Argument1 = argument1.ToString(CultureInfo.InvariantCulture);
            Argument2 = argument2.ToString(CultureInfo.InvariantCulture);
            FullSyntax = op + " " + Quote(argument1) + " " + Quote(argument2);
        }

        public string Op        { get; set; }

        public string Argument1  { get; set; }

        public string Argument2  { get; set; }

        public string Argument3  { get; set; }

        public string FullSyntax { get; set; }


        private static string Quote(int i)
        {
            return "\"" + i.ToString(CultureInfo.InvariantCulture) + "\"";
        }

        private static string Quote(string s)
        {
            string intermediate = s.Replace("\\", "\\\\");
            string result = intermediate.Replace("\"", "\\\"");
            return "\"" + result + "\"";
        }
    }
}
