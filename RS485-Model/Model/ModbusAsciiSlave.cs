using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;

namespace RS485_Model.Model
{
    enum AsciiSlaveState
    {
        Idle,
        Reception,
        WaitingForEof
    }

    public class ModbusAsciiSlave
    {
        public event EventHandler<ModbusFrameEventArgs> FrameReceived;
        public event EventHandler<ModbusFrameEventArgs> FrameSent;

        private AsciiSlaveState state = AsciiSlaveState.Idle;
        private SerialPort serialPort = new SerialPort();
        private List<byte> receivedBytes = new List<byte>();


        private byte address;

        public string PortName { get; set; }

        public byte Address
        {
            get => address;
            set
            {
                if (value == 0 || value > 247)
                    throw new ArgumentOutOfRangeException("Nieprawidłowy adres stacji slave");
                address = value;
            }
        }

        public int MaxCharInterval { get; set; }

        public int WriteTimeout { get; set; } = 500;

        public ModbusSlaveRequestHandler RequestHandler { get; set; }

        public void Run()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            if (RequestHandler == null)
            {
                RequestHandler = new ModbusSlaveRequestHandler();
            }
            state = AsciiSlaveState.Idle;
            serialPort.PortName = PortName;
            serialPort.ReadTimeout = MaxCharInterval;
            serialPort.WriteTimeout = WriteTimeout;
            serialPort.Open();
            Task.Run(() => Read());
        }

        private void Read()
        {
            while (serialPort.IsOpen)
            {
                try
                {
                    int receivedByte = serialPort.ReadByte();
                    if (receivedByte >= 0 && receivedByte <= 255)
                        HandleReceivedByte((byte)receivedByte);
                }
                catch (TimeoutException)
                {
                    HandleTimeout();
                }
                catch (Exception)
                {
                    HandleOtherException();
                }
            }
        }

        private void HandleReceivedByte(byte receivedByte)
        {
            switch (state)
            {
                case AsciiSlaveState.Idle:
                    state = HandleReceivedByteStateIdle(receivedByte);
                    break;
                case AsciiSlaveState.Reception:
                    state = HandleReceivedByteStateReception(receivedByte);
                    break;
                case AsciiSlaveState.WaitingForEof:
                    state = HandleReceivedByteStateWaiting(receivedByte);
                    break;
            }
        }

        private void HandleTimeout()
        {
            state = AsciiSlaveState.Idle;
            receivedBytes.Clear();
        }

        private void HandleOtherException()
        {
            state = AsciiSlaveState.Idle;
        }

        private AsciiSlaveState HandleReceivedByteStateIdle(byte receivedByte)
        {
            if (receivedByte == Convert.ToByte(':'))
            {
                InitializeNewFrame();
                AddByteToBuffer(receivedByte);
                return AsciiSlaveState.Reception;
            }
            else
            {
                return AsciiSlaveState.Idle;
            }
        }

        private AsciiSlaveState HandleReceivedByteStateReception(byte receivedByte)
        {
            if (receivedByte == Convert.ToByte(':'))
            {
                InitializeNewFrame();
                AddByteToBuffer(receivedByte);
                return AsciiSlaveState.Reception;
            }
            else if (receivedByte == Convert.ToByte('\r'))
            {
                AddByteToBuffer(receivedByte);
                return AsciiSlaveState.WaitingForEof;
            }
            else
            {
                AddByteToBuffer(receivedByte);
                return AsciiSlaveState.Reception;
            }
        }

        private AsciiSlaveState HandleReceivedByteStateWaiting(byte receivedByte)
        {
            if (receivedByte == Convert.ToByte(':'))
            {
                InitializeNewFrame();
                AddByteToBuffer(receivedByte);
                return AsciiSlaveState.Reception;
            }
            else if (receivedByte == Convert.ToByte('\n'))
            {
                AddByteToBuffer(receivedByte);
                ProcessFrame();
                return AsciiSlaveState.Idle;
            }
            else
            {
                return AsciiSlaveState.Reception;
            }
        }


        private void InitializeNewFrame()
        {
            receivedBytes.Clear();
        }

        private void AddByteToBuffer(byte newByte)
        {
            receivedBytes.Add(newByte);
        }

        private void ProcessFrame()
        {
            byte[] frameBytes = receivedBytes.ToArray();
            try
            {
                ModbusFrame frame = ModbusFrame.FromAscii(frameBytes);
                if (frame.Address == Address || frame.Address == 0)
                {
                    FrameReceived?.Invoke(this, new ModbusFrameEventArgs() { FrameBytes = frameBytes, FrameObject = frame });
                    ProcessRequest(frame);
                }
            }
            catch (InvalidFrameException)
            {

            }
        }

        private void ProcessRequest(ModbusFrame frame)
        {
            ModbusFrame responseFrame = RequestHandler.Handle(frame);
            if (responseFrame != null)
            {
                SendResponse(responseFrame);
            }
        }

        private void SendResponse(ModbusFrame responseFrame)
        {
            byte[] frameBytes = responseFrame.ToAscii();
            FrameSent?.Invoke(this, new ModbusFrameEventArgs() { FrameBytes = frameBytes, FrameObject = responseFrame });
            serialPort.Write(frameBytes, 0, frameBytes.Length);
        }
    }
}
