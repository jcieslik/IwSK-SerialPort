using System.ComponentModel;

namespace RS232_Model.Enums
{
    public enum FlowControlType
    {
        [Description("Brak")]
        None = 0,
        [Description("DTR/DSR")]
        DtrDsr = 3,
        [Description("RTS/CTS")]
        RtsCts = 2,
        [Description("XON/XOFF")]
        XOnXOff = 1,

    }
}
