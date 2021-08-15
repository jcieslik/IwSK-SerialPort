using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS232_UI
{
    public class ViewModel
    {
        public List<string> Ports { get; set; }

        public ViewModel()
        {
            Ports = SerialPort.GetPortNames().ToList();
        }
    }
}
