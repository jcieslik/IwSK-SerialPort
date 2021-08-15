using RS232_Model.Enums;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace RS232_UI
{
    public class ViewModel
    {
        public IEnumerable<string> Ports { get; set; }
        public string SelectedPort { get; set; }
        public ParityBitsNumber SelectedParity { get; set; }
        public StopBitsNumber SelectedStopBits { get; set; }
        public DataBitsNumber SelectedDataBits { get; set; }
        public Terminator SelectedTerminator { get; set; }
        public FlowControlType SelectedFlowControl { get; set; }

        public ViewModel()
        {
            Ports = SerialPort.GetPortNames();
            SelectedPort = Ports.First() ?? "";
        }
    }
}
