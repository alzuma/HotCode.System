using System;

namespace HotCode.System
{
    public class HotCodeException : Exception
    {
        public string Code { get; }

        public HotCodeException()
        {
        }

        public HotCodeException(string code)
        {
            Code = code;
        }

        public HotCodeException(string message, params object[] args)
            : this(string.Empty, message, args)
        {
        }

        public HotCodeException(string code, string message, params object[] args)
            : this(null, code, message, args)
        {
        }

        public HotCodeException(Exception innerException, string message, params object[] args)
            : this(innerException, string.Empty, message, args)
        {
        }

        public HotCodeException(Exception innerException, string code, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
            Code = code;
        }
    }
}