using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RJCP.IO.Ports;

namespace WFQYDB.Communication
{
    public class SerialPortStreamSourceConfig : IStreamSourceConfig
    {
        private string portName = "COM1";
        private int baudRate = 9600;
        private int dataBits = 8;
        private Parity parity = Parity.None;
        private StopBits stopBits = StopBits.One;

        private int readTimeout = -1;
        private int writeTimeout = -1;

        public string PortName 
        { 
            get => portName;
            set { 
                portName = value; 
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Cfg));
            }
        }

        public int BaudRate 
        { 
            get => baudRate;
            set { 
                baudRate = value; 
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Cfg));
            }
        }

        public int DataBits 
        { 
            get => dataBits;
            set { 
                dataBits = value; 
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Cfg));
            }
        }

        public Parity Parity 
        { 
            get => parity;
            set { 
                parity = value; 
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Cfg));
            }
        }

        public StopBits StopBits 
        { 
            get => stopBits;
            set { 
                stopBits = value; 
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Cfg));
            }
        }

        public string ClassName => nameof(SerialPortStreamSource);

        public int ReadTimeout
        {
            get => readTimeout;
            set { 
                readTimeout = value;
                NotifyPropertyChanged(); 
                NotifyPropertyChanged(nameof(Cfg)); 
            }
        }

        public int WriteTimeout
        {
            get => writeTimeout;
            set { 
                writeTimeout = value; 
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Cfg));
            }
        }

        public string Cfg 
        { 
            get => ToCfgString(); 
            set => FromCfgString(value); 
        }

        static bool ParityTryParse(string s, out Parity parity)
        {
            switch (s.Trim().ToLower())
            {
                case "n":
                case "none":
                    parity = RJCP.IO.Ports.Parity.None;
                    return true;
                
                case "o":
                case "odd":
                    parity = RJCP.IO.Ports.Parity.Odd;
                    return true;

                case "e":
                case "even":
                    parity = RJCP.IO.Ports.Parity.Even;
                    return true;

                case "m":
                case "mark":
                    parity = RJCP.IO.Ports.Parity.Mark;
                    return true;

                case "s":
                case "space":
                    parity = RJCP.IO.Ports.Parity.Space;
                    return true;

                default:
                    parity = default;
                    return false;
            }
        }

        static bool StopBitsTryParse(string s, out StopBits stopBits)
        {
            switch (s.Trim())
            {
                case "1":
                case nameof(RJCP.IO.Ports.StopBits.One):
                    stopBits = RJCP.IO.Ports.StopBits.One;
                    return true;

                case "1.5":
                case nameof(RJCP.IO.Ports.StopBits.One5):
                    stopBits = RJCP.IO.Ports.StopBits.One5;
                    return true;

                case "2":
                case nameof(RJCP.IO.Ports.StopBits.Two):
                    stopBits = RJCP.IO.Ports.StopBits.Two;
                    return true;

                default:
                    stopBits = default;
                    return false;
            }
        }

        public IStreamSourceConfig FromCfgString(string connString)
        {
            // PortName;BaudRate;DataBits;Parity;StopBits
            // COM1;9600;8;none;1
            var csParts = connString.Split(';');
            
            PortName = csParts[0].Trim();

            if (csParts.Length > 1)
            {
                if (int.TryParse(csParts[1], out int b))
                    BaudRate = b;
                else
                    throw new ArgumentException($"BaudRate {csParts[1]} is invalid.");

                if (csParts.Length > 2)
                {
                    if (int.TryParse(csParts[2], out int d))
                        DataBits = d;
                    else
                        throw new ArgumentException($"DataBits {csParts[2]} is invalid.");

                    if (csParts.Length > 3)
                    {
                        if (ParityTryParse(csParts[3], out Parity p))
                            Parity = p;
                        else
                            throw new ArgumentException($"Parity {csParts[3]} is invalid.");

                        if (csParts.Length > 4)
                        {
                            if (StopBitsTryParse(csParts[4], out StopBits s))
                                StopBits = s;
                            else
                                throw new ArgumentException($"StopBits {csParts[4]} is invalid.");
                        }
                    }
                }
            }
            NotifyPropertyChanged(nameof(Cfg));
            return this;
        }

        public string ToCfgString() => $"{PortName}; {BaudRate}; {DataBits}; {Parity}; {StopBits}";

        public override string ToString() => $"\"{ClassName}\"; {ToCfgString()}";
        
        public IStreamSource Get()
        {
            return new SerialPortStreamSource
            {
                BaudRate = baudRate,
                DataBits = dataBits,
                Parity = parity,
                PortName = portName,
                StopBits = stopBits,
                ReadTimeout = ReadTimeout,
                WriteTimeout = WriteTimeout,
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
