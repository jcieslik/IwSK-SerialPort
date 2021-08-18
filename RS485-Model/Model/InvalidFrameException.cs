using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
