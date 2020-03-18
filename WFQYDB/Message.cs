using System;
using System.Collections.Generic;
using System.Text;

namespace WFQYDB
{
    public enum Command : byte
    {
        BroadcastQuery = 0xa0,
        IndividualQuery = 0xa1,
        AutoRun = 0xa2,
        Shutdown = 0xa3,
        RealtimeData = 0xa4,
    }

    public class Message
    {
        public Message() { }

        public Message(byte[] buffer)
        {
            this.buffer = buffer;
            //Build();
        }

        const byte Sync = 0xFA;
        protected byte[] buffer = new byte[18];

        public byte[] Buffer
        {
            get
            {
                Build();
                return buffer;
            }
            set
            {
                buffer = value;

            }
        }

        public byte[] Destination
        {
            get => Buffer.Take(1, 4);
            set
            {
                FitBuffer(18);
                value.CopyTo(buffer, 1);
            }
        }

        public byte[] Source
        {
            get => Buffer.Take(5, 4);
            set
            {
                FitBuffer(18);
                value.CopyTo(buffer, 5);
            }
        }

        public Command Command
        {
            get => (Command)buffer[9];
            set {
                FitBuffer(18);
                buffer[9] = (byte)value;
            }
        }

        public int DataLength
        {
            get => (buffer[10] << 8) + buffer[11];
            set
            {
                FitBuffer(FullLengthWithCrc);
                buffer[11] = (byte)(value & 0xFF);
                buffer[10] = (byte)(value >> 8);
            }
        }

        public byte[] Data {
            get => buffer.Take(12, DataLength);
            set {
                DataLength = value.Length;
                value.CopyTo(buffer, 12);
            }
        }

        public byte Crc => buffer[FullLengthWithCrc - 1];

        public int FullLengthWithCrc => 12 + DataLength + 1;

        public bool IsHeaderValid =>
            (buffer.Length > 10)&&
            (buffer[0] == Sync) &&
            ((byte)Command >= 0xA0) &&
            ((byte)Command <= 0xA4) &&
            (Data.Length >= 0)&&
            (Data.Length < 510);

        public bool Valid => IsHeaderValid && (buffer[FullLengthWithCrc - 1] == GetCrc());

        void Build()
        {
            FitBuffer(FullLengthWithCrc);
            buffer[0] = Sync;
            buffer[FullLengthWithCrc - 1] = GetCrc();
        }

        public void AutoFit() => FitBuffer(FullLengthWithCrc);

        protected void FitBuffer(int len)
        {
            if(buffer.Length < len)
            {
                var minlen = len > 18 ? len : 18;
                var newBuffer = new byte[minlen];
                buffer.CopyTo(newBuffer, 0);
                buffer = newBuffer;
            }
        }

        byte GetCrc()
        {
            var len = FullLengthWithCrc - 1;
            byte crc = 0;
            for (var i = 1; i < len; i++)
                crc ^= buffer[i];
            return crc;
        }

    }
}
