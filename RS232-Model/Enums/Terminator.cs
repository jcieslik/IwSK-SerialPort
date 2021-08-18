using System.ComponentModel;

namespace RS232_Model.Enums
{
    public enum Terminator
    {
        [Description("Brak")]
        None,
        [Description("CR")]
        CR,
        [Description("LF")]
        LF,
        [Description("CRLF")]
        CRLF,
        [Description("Własny")]
        Custom
    }
}
