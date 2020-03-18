using System;
using System.ComponentModel.DataAnnotations;
using WFQYDB;

namespace WfqydbModbusGateway.Model
{
    /// <summary>
    /// Архивная запись
    /// </summary>
    public class ArcRecord
    {
        public ArcRecord()
        {
            UnixTimestamp = DateTime.Now.ToUnixTimestamp();
        }

        public ArcRecord(MessageStoryItem msi)
        {
            if (msi == default)
                throw new ArgumentException("MessageStoryItem mast not be null.");
            UnixTimestamp = msi.DateTime.ToUnixTimestamp();
            UpFreq = msi.Data[0];
            DnFreq = msi.Data[1];
            StokeRate = msi.Data[2];
            StokeLength = msi.Data[3];
            Status = msi.Data[4];
        }

        [Key]
        public int Id { get; set; }
        public int UnixTimestamp { get; set; }
        public byte UpFreq { get; set; }
        public byte DnFreq { get; set; }
        public byte StokeRate { get; set; }
        public byte StokeLength { get; set; }
        public byte Status { get; set; }
    }
}
