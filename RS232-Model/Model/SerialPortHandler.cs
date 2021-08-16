using RS232_Model.Enums;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS232_Model.Model
{
    public class SerialPortHandler
    {
        public event EventHandler<ByteReceivedEventArgs> ByteRead;

        private SerialPort serialPort;
        private string terminatorString = "";
        
        public DataBitsNumber DataBitsNumber { get; set; }
        public FlowControlType FlowControlType { get; set; }
        public ParityBitsNumber ParityBitsNumber { get; set; }
        public StopBitsNumber StopBitsNumber { get; set; }
        public Terminator Terminator { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }


        public bool RtsEnable
        {
            get => serialPort.RtsEnable;
            set => serialPort.RtsEnable = value;
        }

        public bool DtrEnable
        {
            get => serialPort.DtrEnable;
            set => serialPort.DtrEnable = value;
        }

        public bool CtsHolding
        {
            get => serialPort.CtsHolding;
        }

        public bool DsrHolding
        {
            get => serialPort.DsrHolding;
        }


        public SerialPortHandler()
        {
            serialPort = new SerialPort();
        }

        public void Open()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            ConfigureSerialPort();
            serialPort.Open();
            Task.Run(() => Read());
        }

        public void Close()
        {
            serialPort.Close();
        }

        public void Write(string text)
        {
            serialPort.Write(text);
            if (terminatorString.Length > 0)
            {
                serialPort.Write(terminatorString);
            }
        }

        public void Write(byte[] bytes)
        {
            serialPort.Write(bytes, 0, bytes.Length);
            if (terminatorString.Length > 0)
            {
                serialPort.Write(terminatorString);
            }
        }

        private void ConfigureSerialPort()
        {
            serialPort.DataBits = (int)DataBitsNumber;
            serialPort.Handshake = (Handshake)FlowControlType;
            serialPort.Parity = (Parity)ParityBitsNumber;
            serialPort.StopBits = (StopBits)StopBitsNumber;
            serialPort.PortName = PortName;
            serialPort.BaudRate = BaudRate;
            SetTerminatorString();
        }
        
        private void SetTerminatorString()
        {
            switch (Terminator)
            {
                case Terminator.None:
                    terminatorString = "";
                    break;
                case Terminator.CR:
                    terminatorString = "\r";
                    break;
                case Terminator.LF:
                    terminatorString = "\n";
                    break;
                case Terminator.CRLF:
                    terminatorString = "\r\n";
                    break;

            }
        }

        private void Read()
        {
            while (serialPort.IsOpen)
            {
                try
                {
                    int receivedByte = serialPort.ReadByte();
                    HandleReceivedByte(receivedByte);
                }
                catch (Exception) { 
                
                }
            }

        }
        
        private void HandleReceivedByte(int receivedByte)
        {
            if (receivedByte != -1)
            {
                ByteReceivedEventArgs eventArgs = new ByteReceivedEventArgs() { ReceivedByte = receivedByte };
                ByteRead?.Invoke(this, eventArgs);
            }
        }
    }
}
