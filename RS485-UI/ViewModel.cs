using RS485_Model.Enums;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace RS485_UI
{
    class ViewModel
    {
        public IEnumerable<string> Ports { get; set; }
        public string SelectedPortMaster { get; set; }
        public string SelectedPortSlave { get; set; }
        public TransactionType SelectedTransactionType { get; set; }
        public ViewModel()
        {
            Ports = SerialPort.GetPortNames();
            SelectedPortMaster = Ports.FirstOrDefault();
            SelectedPortSlave = Ports.FirstOrDefault();
            SelectedTransactionType = TransactionType.Addressed;
        }
    }
}
