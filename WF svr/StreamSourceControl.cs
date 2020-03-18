using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFQYDB;
using WFQYDB.Communication;

namespace WF_svr
{
    public partial class StreamSourceControl : UserControl
    {
        class StreamSourceInfo
        {
            public string FullName { get; set; }
            public string DisplayName { get; set; }
            public string Tip { get; set; }
            public string Example { get; set; }
        }

        static readonly List<StreamSourceInfo> Sources = new List<StreamSourceInfo>
        {
            new StreamSourceInfo {
                FullName = nameof(SerialPortStreamSource),
                DisplayName = "Serial",
                Tip = "Port; BaudRate; DataBits; Parity; StopBits",
                Example = "COM1; 9600; 8; None; 1"},
            new StreamSourceInfo {
                FullName = nameof(TcpStreamSource),
                DisplayName = "TCP",
                Tip = "host:port",
                Example = "127.0.0.1:6789"},
            //new StreamSourceInfo {
            //    FullName = nameof(SerialPortStreamSource),
            //    DisplayName = "Serial",
            //    Tip = "Parity; BaudRate; DataBits; Parity; StopBits"  },
        };
        
        private IStreamSourceConfig streamSourceCfg;

        public IStreamSourceConfig StreamSourceCfg {
            get 
            {
                if (streamSourceCfg != null)
                    return streamSourceCfg;

                try
                {
                    var si = (StreamSourceInfo)comboBox1.SelectedItem;
                    streamSourceCfg =  StreamSourceFactory.GetStreamSourceConfig($"{si.FullName}; {textBox1.Text}");
                }
                catch
                {
                    ;
                }
                
                return streamSourceCfg;
            }
            set 
            {
                comboBox1.SelectedIndexChanged -= ComboBox1_SelectedIndexChanged;
                textBox1.TextChanged -= TextBox1_TextChanged;

                if (streamSourceCfg != null)
                    streamSourceCfg.PropertyChanged -= StreamSource_PropertyChanged;

                if ((streamSourceCfg?.ToCfgString() != value?.ToCfgString()) && (value != null))
                {
                    var si = Sources.First(s => s.FullName == value.ClassName);
                    comboBox1.SelectedItem = si;
                    textBox1.Text = value.ToCfgString();
                }

                streamSourceCfg = value;

                if (streamSourceCfg != null)
                    streamSourceCfg.PropertyChanged += StreamSource_PropertyChanged;

                comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
                textBox1.TextChanged += TextBox1_TextChanged;
            }
        }

        private void StreamSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var s = streamSourceCfg.ToCfgString();
            if(s != textBox1.Text)
                textBox1.Text = streamSourceCfg.ToCfgString();
        }

        public StreamSourceControl()
        {
            InitializeComponent();
            comboBox1.Items.AddRange(Sources.ToArray());
            comboBox1.DisplayMember = nameof(StreamSourceInfo.DisplayName);
            comboBox1.SelectedIndex = 0;

            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            textBox1.TextChanged += TextBox1_TextChanged;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var si = (StreamSourceInfo)comboBox1.SelectedItem;
            if (StreamSourceCfg.ClassName == si.FullName)
                return;

            StreamSourceCfg = StreamSourceFactory.GetStreamSourceConfig($"{si.FullName}; {si.Example}");
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

            textBox1.TextChanged -= TextBox1_TextChanged;
            try
            {
                if (streamSourceCfg == null)
                {
                    var si = (StreamSourceInfo)comboBox1.SelectedItem;
                    streamSourceCfg = StreamSourceFactory.GetStreamSourceConfig($"{si.FullName}; {textBox1.Text}");
                }
                else
                {
                    var s = streamSourceCfg.ToCfgString();
                    if (s != textBox1.Text)
                    {
                        streamSourceCfg.FromCfgString(textBox1.Text);
                        textBox1.Text = streamSourceCfg.ToCfgString();
                    }
                }
                textBox1.ForeColor = Color.Black;
            }
            catch
            {
                textBox1.ForeColor = Color.Red;
            }

            textBox1.TextChanged += TextBox1_TextChanged;
        }
    }
}
