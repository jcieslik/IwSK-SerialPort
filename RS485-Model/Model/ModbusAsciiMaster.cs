using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;

namespace RS485_Model.Model
{

    enum AsciiMasterState
    {
        Idle,
        WaitingForResponse,
        Reception,
        WaitingForEof,
        ResponseReceived,
        TimedOut
    }
    public class ModbusAsciiMaster
    {
        public event EventHandler<ModbusFrameEventArgs> FrameReceived;
        public event EventHandler<ModbusFrameEventArgs> FrameSent;

        private SerialPort serialPort = new SerialPort();
        private List<byte> receivedBytes = new List<byte>();
        private AsciiMasterState state = AsciiMasterState.Idle;
        Stopwatch transactionStopwatch = new Stopwatch();

        public string PortName { get; set; }
        public int TransactionTimeout { get; set; }
        public int TransactionRetry { get; set; }
        public int MaxCharInterval { get; set; }
        public int WriteTimeout { get; set; } = 500;

        public void Open()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            serialPort.PortName = PortName;
            serialPort.ReadTimeout = MaxCharInterval;
            serialPort.WriteTimeout = WriteTimeout;
            serialPort.Open();
        }

        public async Task<ModbusFrame> MakeRequest(ModbusFrame requestFrame)
        {
            if (requestFrame.Address == 0)
            {
                MakeBroadcast(requestFrame);
                return null;
            }
            else
            {
                return await MakeTransaction(requestFrame);
            }
        }

        private void MakeBroadcast(ModbusFrame requestFrame)
        {
            SendFrame(requestFrame);
        }

        private async Task<ModbusFrame> MakeTransaction(ModbusFrame requestFrame)
        {
            ModbusFrame responseFrame = null;
            int tries = TransactionRetry + 1;
            while (responseFrame == null && tries > 0)
            {
                responseFrame = await MakeSingleTransaction(requestFrame);
                tries--;
            }
            if (responseFrame == null)
            {
                throw new Exception("Nie uzyskano odpowiedzi dla żadnej transakcji.");
            }
            return responseFrame;
        }


        private async Task<ModbusFrame> MakeSingleTransaction(ModbusFrame requestFrame)
        {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
            SendFrame(requestFrame);
            state = AsciiMasterState.WaitingForResponse;
            return await Task.Run(() => ReadResponse());
        }

        private void SendFrame(ModbusFrame requestFrame)
        {
            byte[] frameBytes = requestFrame.ToAscii();
            FrameSent?.Invoke(this, new ModbusFrameEventArgs() { FrameBytes = frameBytes, FrameObject = requestFrame });
            serialPort.Write(frameBytes, 0, frameBytes.Length);
        }

        private ModbusFrame ReadResponse()
        {
            transactionStopwatch.Restart();
            ModbusFrame responseFrame = null;
            while (TransactionInProgress())
            {
                try
                {
                    int receivedByte = serialPort.ReadByte();
                    if (receivedByte >= 0 && receivedByte <= 255)
                        HandleReceivedByte((byte)receivedByte);
                    if (state == AsciiMasterState.ResponseReceived)
                    {
                        responseFrame = ProcessResponseFrame();
                    }
                }
                catch (TimeoutException)
                {
                    state = AsciiMasterState.WaitingForResponse;
                    HandleTimeout();
                }
                catch (Exception)
                {
                    state = AsciiMasterState.WaitingForResponse;
                }
            }
            return responseFrame;
        }

        private bool TransactionInProgress()
        {
            return state == AsciiMasterState.WaitingForResponse || state == AsciiMasterState.Reception || state == AsciiMasterState.WaitingForEof;
        }

        private void HandleReceivedByte(byte receivedByte)
        {
            switch (state)
            {
                case AsciiMasterState.WaitingForResponse:
                    state = HandleReceivedByteStateWaitingForResponse(receivedByte);
                    break;
                case AsciiMasterState.Reception:
                    state = HandleReceivedByteStateReception(receivedByte);
                    break;
                case AsciiMasterState.WaitingForEof:
                    state = HandleReceivedByteStateWaitingEof(receivedByte);
                    break;
            }
        }

        private void HandleTimeout()
        {
            if (transactionStopwatch.ElapsedMilliseconds > TransactionTimeout)
            {
                state = AsciiMasterState.TimedOut;
            }
        }

        private AsciiMasterState HandleReceivedByteStateWaitingForResponse(byte receivedByte)
        {
            if (receivedByte == Convert.ToByte(':'))
            {
                InitializeNewFrame();
                AddByteToBuffer(receivedByte);
                return AsciiMasterState.Reception;
            }
            else
            {
                return AsciiMasterState.WaitingForResponse;
            }
        }

        private AsciiMasterState HandleReceivedByteStateReception(byte receivedByte)
        {
            if (receivedByte == Convert.ToByte(':'))
            {
                InitializeNewFrame();
                AddByteToBuffer(receivedByte);
                return AsciiMasterState.Reception;
            }
            else if (receivedByte == Convert.ToByte('\r'))
            {
                AddByteToBuffer(receivedByte);
                return AsciiMasterState.WaitingForEof;
            }
            else
            {
                AddByteToBuffer(receivedByte);
                return AsciiMasterState.Reception;
            }
        }

        private AsciiMasterState HandleReceivedByteStateWaitingEof(byte receivedByte)
        {
            if (receivedByte == Convert.ToByte(':'))
            {
                InitializeNewFrame();
                AddByteToBuffer(receivedByte);
                return AsciiMasterState.Reception;
            }
            else if (receivedByte == Convert.ToByte('\n'))
            {
                AddByteToBuffer(receivedByte);
                return AsciiMasterState.ResponseReceived;
            }
            else
            {
                return AsciiMasterState.Reception;
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

        private ModbusFrame ProcessResponseFrame()
        {
            byte[] frameBytes = receivedBytes.ToArray();
            ModbusFrame frame = ModbusFrame.FromAscii(frameBytes);
            FrameReceived?.Invoke(this, new ModbusFrameEventArgs() { FrameBytes = frameBytes, FrameObject = frame });
            return frame;
        }
    }
}
