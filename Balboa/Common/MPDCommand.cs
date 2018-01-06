﻿/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс инкапсулирует команды MPD.
 *
 --------------------------------------------------------------------------*/

namespace Balboa.Common
{
    public class MPDCommand
    {
        public MPDCommand(string op)
        {
            Op = op;
            Argument1 = null;
            Argument2 = null;
            Argument3 = null;
            FullSyntax = op;
        }

        public MPDCommand(string op, string argument1, string argument2, string argument3)
        {
            Op = op;
            Argument1 = argument1;
            Argument2 = argument2;
            Argument2 = argument3;
            FullSyntax = op + " " + Quote(argument1) + " " + Quote(argument2) + " " + Quote(argument3);
        }

        public MPDCommand(string op, string argument1)
        {
            Op = op;
            Argument1 = argument1;
            Argument2 = null;
            FullSyntax = op + " " + Quote(argument1);
        }

        public MPDCommand(string op, int argument1)
        {
            Op = op;
            Argument1 = argument1.ToString();
            Argument2 = null;
            FullSyntax = op + " " + Quote(argument1);
        }

        public MPDCommand(string op, bool argument1)
        {
            Op = op;
            Argument1 = argument1 ? "1" : "0";
            Argument2 = null;
            FullSyntax = op + " " + Quote(Argument1);
        }

        public MPDCommand(string op, string argument1, string argument2)
        {
            Op = op;
            Argument1 = argument1;
            Argument2 = argument2;
            FullSyntax = op + " " + Quote(argument1) + " " + Quote(argument2);
        }

        public MPDCommand(string op, string argument1, int argument2)
        {
            Op = op;
            Argument1 = argument1;
            Argument2 = argument2.ToString();
            FullSyntax = op + " " + Quote(argument1) + " " + Quote(argument2);
        }

        public MPDCommand(string op, int argument1, int argument2)
        {
            Op = op;
            Argument1 = argument1.ToString();
            Argument2 = argument2.ToString();
            FullSyntax = op + " " + Quote(argument1) + " " + Quote(argument2);
        }

        public string Op        { get; private set; }

        public string Argument1  { get; private set; }

        public string Argument2  { get; private set; }

        public string Argument3  { get; private set; }

        public string FullSyntax { get; private set; }


        private static string Quote(int i)
        {
            return "\"" + i.ToString() + "\"";
        }

        private static string Quote(string s)
        {
            string intermediate = s.Replace("\\", "\\\\");
            string result = intermediate.Replace("\"", "\\\"");
            return "\"" + result + "\"";
        }
    }
}
