using RS485_Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS485_UI
{
    class ViewModel
    {
        public IEnumerable<string> Ports { get; set; }
        public string SelectedPort { get; set; }
        public TransactionType SelectedTransactionType { get; set; }
    }
}
