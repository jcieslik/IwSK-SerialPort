using System.ComponentModel;

namespace RS232_Model.Enums
{
    public enum FlowControlType
    {
        [Description("Brak")]
        None,
        [Description("DTR/DSR")]
        DtrDsr,
        [Description("RTS/CTS")]
        RtsCts,
        [Description("XON/XOFF")]
        XOnXOff,

    }
}
