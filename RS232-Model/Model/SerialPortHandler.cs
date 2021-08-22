using RS232_Model.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace RS232_Model.Model
{
    public class SerialPortHandler
    {
        //public event EventHandler<ByteReceivedEventArgs> ByteRead;
        public event EventHandler<TextReceivedEventArgs> TextReceived;
        public event EventHandler<bool> ConnectionClosed;

        private SerialPort serialPort = new SerialPort();
        private string terminatorString = "";
        private Queue<byte> lastReceivedBytes = new Queue<byte>();
        private readonly object writeLock = new object();
        private bool pingReplyReceived = false;
        
        //private bool 

        public string PingRequest { get; set; } = "PING";
        public string PingReply { get; set; } = "OK";
        public DataBitsNumber DataBitsNumber { get; set; }
        public FlowControlType FlowControlType { get; set; }
        public ParityBitsNumber ParityBitsNumber { get; set; }
        public StopBitsNumber StopBitsNumber { get; set; }
        public Terminator Terminator { get; set; }
        public string CustomTerminator { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int PingTimeout { get; set; } = 10000;
        public string ReceivedMessage { get; set; }


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

        public bool IsOpen
        {
            get => serialPort.IsOpen;
        }


        public void Open()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
            ConfigureSerialPort();
            serialPort.Open();
            if (FlowControlType == FlowControlType.DtrDsr)
            {
                serialPort.DtrEnable = true;
            }
            if(Terminator == Terminator.None)
            {
                lastReceivedBytes.Clear();
                Task.Run(() => ReadNoTerminator());
            }
            else
            {
                Task.Run(() => ReadWithTerminator());
            }
        }

        public void Close()
        {
            serialPort.Close();
        }

        public async Task WriteAsync(string text)
        {
            if (FlowControlType == FlowControlType.DtrDsr)
            {
                while (!serialPort.DsrHolding)
                {
                    await Task.Delay(1);
                }
            }
            //Lock jest potrzebny, bo odpowiedź na ping jest wysyłana z innego wątku niż normalne wysyłanie.
            lock (writeLock)
            {
                serialPort.Write(text);
                if (terminatorString.Length > 0)
                {
                    serialPort.Write(terminatorString);
                }
            }
        }

        public async void WriteAsync(byte[] bytes)
        {
            if (FlowControlType == FlowControlType.DtrDsr)
            {
                while (!serialPort.DsrHolding)
                {
                    await Task.Delay(1);
                }
            }
            lock (writeLock)
            {
                try
                {
                    serialPort.Write(bytes, 0, bytes.Length);
                    if (terminatorString.Length > 0)
                    {
                        serialPort.Write(terminatorString);
                    }
                }
                catch(InvalidOperationException)
                {
                    throw new Exception("Port jest zamkniety!");
                }
            }
        }

        public async Task<long> PingAsync()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                Task pingTask = PingTransactionAsync();
                Task firstTask = await Task.WhenAny(pingTask, Task.Delay(PingTimeout));
                if (!pingTask.IsCompletedSuccessfully)
                {
                    throw new Exception("Połączenie jest zamknięte!");
                }
                if (firstTask != pingTask)
                {
                    throw new Exception("Przekroczono czas oczekiwania na odpowiedź.");
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return stopWatch.ElapsedMilliseconds;
        }

        public async Task PingTransactionAsync()
        {
            try
            {
                pingReplyReceived = false;
                await WriteAsync(PingRequest);
                await Task.Run(() => {
                    while (!pingReplyReceived) { }
                });
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private void ConfigureSerialPort()
        {
            serialPort.DataBits = (int)DataBitsNumber;
            if(FlowControlType != FlowControlType.DtrDsr)
            {
                serialPort.Handshake = (Handshake)FlowControlType;
            }
            else
            {
                serialPort.Handshake = Handshake.None;
            }
            
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
                case Terminator.Custom:
                    terminatorString = CustomTerminator;
                    break;
                default:
                    break;
            }
        }

        private void ReadNoTerminator()
        {
            while (true)
            {
                try
                {
                    int receivedByte = serialPort.ReadByte();
                    HandleReceivedByte(receivedByte);
                }
                catch (OperationCanceledException)
                {
                    break;//Port został zamknięty
                }
                catch (InvalidOperationException)
                {
                    ConnectionClosed?.Invoke(this, true);
                    break;
                }
                catch (Exception) { 
                
                }
            }
            Debug.WriteLine("ReadNoTerminator return");

        }
        
        private void HandleReceivedByte(int receivedByte)
        {
            if (receivedByte != -1)
            {
                TextReceivedEventArgs eventArgs = new TextReceivedEventArgs();
                eventArgs.ReceivedText = Encoding.ASCII.GetString(new byte[] { (byte)receivedByte });
                TextReceived?.Invoke(this, eventArgs);
                CheckForPingNoTerminator((byte)receivedByte);
            }
        }

        private void CheckForPingNoTerminator(byte receivedByte)
        {
            lastReceivedBytes.Enqueue(receivedByte);
            byte[] lastBytesArray = lastReceivedBytes.ToArray();
            string lastBytesString = Encoding.ASCII.GetString(lastBytesArray);
            if (lastBytesString.EndsWith(PingRequest))
            {
                SendPingReply();
            }
            else if (lastBytesString.EndsWith(PingReply))
            {
                pingReplyReceived = true;
            }
            while(lastReceivedBytes.Count >= Math.Max(PingRequest.Length, PingReply.Length))
            {
                lastReceivedBytes.Dequeue();
            }
        }

        private void ReadWithTerminator()
        {
            while (true)
            {
                try
                {
                    string receivedText = serialPort.ReadTo(terminatorString);
                    Debug.WriteLine(receivedText);
                    HandleReceivedText(receivedText);
                }
                catch (OperationCanceledException)
                {
                    break;//Port został zamknięty
                }
                catch (InvalidOperationException)
                {
                    ConnectionClosed?.Invoke(this, true);
                    break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            Debug.WriteLine("ReadWithTerminator return");
        }

        private void HandleReceivedText(string receivedText)
        {
            bool isPingRequest = CheckForPingRequestWithTerminator(receivedText);
            bool isPingReply = CheckForPingReplyWithTerminator(receivedText);
            if (!isPingRequest && !isPingReply)
            {
                TextReceivedEventArgs eventArgs = new TextReceivedEventArgs() { ReceivedText = receivedText };
                TextReceived?.Invoke(this, eventArgs);
            }
        }

        private bool CheckForPingRequestWithTerminator(string receivedText)
        {
            if(receivedText == PingRequest)
            {
                SendPingReply();
                return true;
            }
            return false;
        }

        private bool CheckForPingReplyWithTerminator(string receivedText)
        {
            if (receivedText == PingReply)
            {
                pingReplyReceived = true;
                return true;
            }
            return false;
        }

        private void SendPingReply()
        {
            try
            {
                Task writeTask = WriteAsync(PingReply);
                writeTask.Wait();
            }
            catch (Exception)
            {

            }
        }
    }
}
