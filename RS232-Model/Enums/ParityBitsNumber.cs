using System.ComponentModel;

namespace RS232_Model.Enums
{
    public enum ParityBitsNumber
    {
        [Description("Brak")]
        None,
        [Description("Bit nieparzystości")]
        Odd = 1,
        [Description("Bit parzystości")]
        Even = 2
    }
}
