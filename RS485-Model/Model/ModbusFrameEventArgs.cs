using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS485_Model.Model
{
    public class ModbusFrameEventArgs : EventArgs
    {
        public byte[] FrameBytes { get; set; }
        public ModbusFrame FrameObject { get; set; }
    }
}
