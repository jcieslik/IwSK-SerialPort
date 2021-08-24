using System;

namespace RS485_Model.Model
{
    public class ModbusFrameEventArgs : EventArgs
    {
        public byte[] FrameBytes { get; set; }
        public ModbusFrame FrameObject { get; set; }
    }
}
