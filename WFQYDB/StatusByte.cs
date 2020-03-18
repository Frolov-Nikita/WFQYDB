namespace WFQYDB
{
    public class StatusByte
    {
        public byte Byte { get; set; }

        public bool OverLoad { 
            get => GetBit(3);
            set => SetBit(value, 3);
        }

        public bool OverTemperature { 
            get => GetBit(2);
            set => SetBit(value, 2);
        }

        public bool ShortCircuit { 
            get => GetBit(1);
            set => SetBit(value, 1);
        }

        public bool Start { 
            get => GetBit(0);
            set => SetBit(value, 0);
        }

        bool GetBit(int num) => (Byte & (1 << num)) > 0;

        void SetBit(bool value, int num)
        {
            var mask = 1 << num;
            Byte = (byte)(value ? Byte | mask : Byte & (~mask));
        }
    }
}
