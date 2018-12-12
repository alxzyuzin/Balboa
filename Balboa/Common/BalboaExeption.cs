using System;

namespace Balboa.Common
{
    public class BalboaException : Exception
    {
        public BalboaException()
        { }


        public BalboaException(string description)
        {
            Description = description;
        }


        public BalboaException(string description, Exception exception)
        {
            Description = description;
            PreviousException = exception;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fname">
        /// Имя файла в котором возникло исключение
        /// </param>
        /// <param name="cname">
        /// /// Имя класса в котором возникло исключение
        /// </param>
        /// <param name="mname">
        /// /// Имя метода в котором возникло исключение
        /// </param>
        /// <param name="lnumber">
        /// Номер строки в файле в которой создаётся объект исключения
        /// </param>
        /// <param name="message">
        /// Описание причины исключения
        /// </param>
        public BalboaException(string fileName, string className, string methodName, string lineNumber, string description)
        {
            FileName = fileName;
            ClassName = className;
            MethodName = methodName;
            LineNumber = lineNumber;
            Description = description;
        }

        public BalboaException(string fileName, string methodName, string lineNumber, string description)
        {
            FileName = fileName;
            MethodName = methodName;
            LineNumber = lineNumber;
            Description = description;
        }

        public string FileName {get;set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public string LineNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Exception PreviousException { get; set; } = null;


    }

    public class BalboaNullValueException: BalboaException
    {

        public string VariableName { get; set; } = string.Empty;

        public BalboaNullValueException():base()
        { }

        public BalboaNullValueException(string fileName, string methodName, string lineNumber, string variableName)
            :base(fileName, methodName, lineNumber, string.Empty)
        {
            VariableName = variableName;
        }
   
        public BalboaNullValueException(string description): base(description)
        { }

        public BalboaNullValueException(string description, Exception exception): base (description, exception)
        { }

    }
}
