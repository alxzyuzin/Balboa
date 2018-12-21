/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс - для хранения частично разобранного ответа от сервера: 
 * Уже  выполнен анализ типа ответа (нормальное завершение или ошибка) и ответ 
 * разбит на элементы представленные в  виде списка.
 *
 --------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace Balboa.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mpd")]
    public class MpdResponseCollection : List<string>
    {
        public enum ResponseKeyword { Ok, NotOk, [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ack")] Ack }

        public ResponseKeyword Keyword { get; set;} 
        public MpdCommand      Command { get; set; } 
        public string          Error   { get; set; } = string.Empty;
        public string          Content { get; set; } = string.Empty;
    }

    public enum ResponseKeyword { Ok, Error, ACK }
    public class MpdResponse : EventArgs
    {
        public ResponseKeyword Keyword { get; }
        public MpdCommand Command { get; }
        private List<string> _content = new List<string>();
        public  List<string> Content { get { return _content; }  }
        public string ErrorMessage { get; set; } = string.Empty;

        public MpdResponse(ResponseKeyword keyword, MpdCommand command, string  content)
        {
            Keyword = keyword;
            Command = command;
            if (keyword == ResponseKeyword.Ok)
            {
                _content.AddRange(content.Split('\n'));
                // Уберём из ответа два последних элемента (ОК и \n)
                _content.RemoveRange(_content.Count - 2, 2);
            }
            else
                ErrorMessage = content;


        }
    }
}
