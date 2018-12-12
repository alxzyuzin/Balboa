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

using System.Collections.Generic;

namespace Balboa.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mpd")]
    public class MpdResponseCollection : List<string>
    {
        public enum ResponseKeyword { Ok, NotOk, [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ack")] Ack }

        public ResponseKeyword Keyword { get; set;}
        public MpdCommand      Command { get; set; }
        public string          Error   { get; set; } = "";
    }
}
