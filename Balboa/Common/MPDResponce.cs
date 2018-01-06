/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс - для хранения частично разобранного ответа от сервера: 
 * выполнен анализ типа ответа (нормальное завершение или ошибка) и ответ 
 * разбит на элементы представленные в  виде списка.
 *
 --------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace Balboa.Common
{
    public class MPDResponce : List<string>
    {
        public enum ResponceKeyword { OK, NotOK, ACK }

        public ResponceKeyword Keyword { get; set;}
        public MPDCommand      Command { get; set; }
        public string          Error   { get; set; } = "";
    }
}
