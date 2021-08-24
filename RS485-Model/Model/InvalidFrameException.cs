using System;

namespace RS485_Model.Model
{
    public class InvalidFrameException : Exception
    {
        public InvalidFrameException()
        {
        }

        public InvalidFrameException(string message)
            : base(message)
        {
        }

        public InvalidFrameException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
