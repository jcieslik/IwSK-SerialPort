using System;
using System.Text;

namespace RS485_Model.Model
{
    public class ModbusSlaveRequestHandler
    {
        public event EventHandler<string> MessageReceived;

        private const byte illegalFunctionCode = 1;
        private const byte exceptionResponseOffset = 128;

        public string MessageToSend { get; set; } = "";

        public string LastReceivedMessage { get; private set; } = "";

        public ModbusFrame Handle(ModbusFrame requestFrame)
        {
            if (requestFrame.Function == 1)
            {
                return HandleFunctionWrite(requestFrame);
            }
            else if (requestFrame.Function == 2)
            {
                return HandleFunctionRead(requestFrame);
            }
            else
            {
                return HandleUnknownFunction(requestFrame);
            }
        }

        private ModbusFrame HandleFunctionWrite(ModbusFrame requestFrame)
        {
            LastReceivedMessage = Encoding.ASCII.GetString(requestFrame.Data);
            MessageReceived?.Invoke(this, LastReceivedMessage);
            if (requestFrame.Address == 0)
                return null;
            else
                return requestFrame;
        }

        private ModbusFrame HandleFunctionRead(ModbusFrame requestFrame)
        {
            if (requestFrame.Address == 0)
                return null;
            ModbusFrame responseFrame = new ModbusFrame()
            {
                Address = requestFrame.Address,
                Function = requestFrame.Function,
                Data = Encoding.ASCII.GetBytes(MessageToSend)
            };
            return responseFrame;
        }

        private ModbusFrame HandleUnknownFunction(ModbusFrame requestFrame)
        {
            if (requestFrame.Address == 0)
                return null;

            ModbusFrame responseFrame = new ModbusFrame()
            {
                Address = requestFrame.Address,
                Function = (byte)(requestFrame.Function + exceptionResponseOffset),
                Data = new byte[] { illegalFunctionCode }
            };
            return responseFrame;
        }
    }
}
