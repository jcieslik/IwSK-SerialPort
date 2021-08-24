using System.ComponentModel;

namespace RS485_Model.Enums
{
    public enum TransactionType
    {
        [Description("Adresowana")]
        Addressed = 0,
        [Description("Rozgłoszeniowa")]
        Broadcast = 1
    }
}
