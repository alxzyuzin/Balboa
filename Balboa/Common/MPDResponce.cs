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
    public enum ResponseKeyword { OK, NotOk, ACK, Empty, InternalError, SocketError, ServerHalted, New, ConnectionError }

    public class MpdResponse : EventArgs
    {
        public ResponseKeyword Keyword { get; private set; }
        public MpdCommand Command { get; private set; }
        private List<string> _content = new List<string>();
        public List<string> Content { get { return _content; }  }
        public string ErrorMessage { get; set; } = string.Empty;

        public MpdResponse()
        {

        }
        public MpdResponse(ResponseKeyword keyword, MpdCommand command, string  content)
        {
            Keyword = keyword;
            Command = command;
            if (keyword == ResponseKeyword.OK)
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
