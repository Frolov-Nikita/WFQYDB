using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFQYDB;

namespace WF_svr
{
    public partial class FormSvr : Form
    {
        Svr svr;
        StreamSourceControl streamSourceControl;
        private string cfgFile;

        public Svr Svr 
        { 
            get => svr;
            set 
            {
                svr = value;

                //Binding getIsEnabledConBtnBinding() => new Binding("Enabled", Svr, "IsConnected", false, DataSourceUpdateMode.OnPropertyChanged);

                //buttonA0.DataBindings.Clear();
                //buttonA1.DataBindings.Clear();
                //buttonA2.DataBindings.Clear(); 
                //buttonA3.DataBindings.Clear();

                //buttonA0.DataBindings.Add(getIsEnabledConBtnBinding());
                //buttonA1.DataBindings.Add(getIsEnabledConBtnBinding());
                //buttonA2.DataBindings.Add(getIsEnabledConBtnBinding());
                //buttonA3.DataBindings.Add(getIsEnabledConBtnBinding());

                toolStripLabelConnect.Text = "disconnected";
                toolStripLabelConnect.ForeColor = System.Drawing.Color.Red;

                numericUpDownControlId0.Value = Svr.ServerAddress[0];
                numericUpDownControlId1.Value = Svr.ServerAddress[1];
                numericUpDownControlId2.Value = Svr.ServerAddress[2];
                numericUpDownControlId3.Value = Svr.ServerAddress[3];

                numericUpDownSlaveId0.Value = Svr.DestenationAddress[0];
                numericUpDownSlaveId1.Value = Svr.DestenationAddress[1];
                numericUpDownSlaveId2.Value = Svr.DestenationAddress[2];
                numericUpDownSlaveId3.Value = Svr.DestenationAddress[3];
            }
        }

        public FormSvr(string cfgFile = "SvrCfg.json")
        {
            InitializeComponent();
            Svr = FactorySvr.Get(cfgFile);
            FactorySvr.Save(Svr, cfgFile);


            DrawConnectControl();
            this.cfgFile = cfgFile;
        }

        void DrawConnectControl() {

            streamSourceControl = new StreamSourceControl();
            streamSourceControl.Left = 10;
            streamSourceControl.Top = 20;
            streamSourceControl.StreamSourceCfg = Svr.StreamSourceCfg;
            groupBoxConnection.Controls.Add(streamSourceControl);
        }

        private void Server_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WFQYDBServer.LastResponse):
                    DoWorkOnUI(() => { FillLastResponse(Svr.Server.LastResponse); });
                    break;
                default:
                    break;
            }
        }

        private void Story_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DoWorkOnUI(()=> {
                foreach (var i in e.NewItems)
                    StoryUpdate((MessageStoryItem)i);
            });
        }

        void FillLastResponse(MessageStoryItem lr)
        {
            if (lr == null)
                return;
            labelLRDateTime.Text = lr.DateTime.ToString("yyyy.MM.dd HH: mm:ss");

            labelLRDest.Text = lr.Destination.ToMinHexString();
            labelLRSource.Text = lr.Source.ToMinHexString();
            labelLRCmd.Text = ((byte)lr.Command).ToString("X2");

            var colTrue = System.Drawing.Color.Green;
            var colFalse = System.Drawing.Color.Red;
            var colDis = System.Drawing.Color.Gray;
            var filler = "--";

            if (lr.DataLength >= 5)
            {
                labelLRUpFreq.Text = lr.Data[0].ToString();
                labelLRDnFreq.Text = lr.Data[1].ToString();
                labelLRStokeLength.Text = lr.Data[2].ToString();
                labelLRStokeRate.Text = lr.Data[3].ToString();

                labelLRStatusByte.Text = "0x" + lr.Data[4].ToString("X2");
                var sb = new StatusByte { Byte = lr.Data[4] };
                labelLRStatusStart.ForeColor = sb.Start ? colTrue : colFalse;
                labelLRStatusShort.ForeColor = sb.ShortCircuit ? colTrue : colFalse;
                labelLRStatusOverTemp.ForeColor = sb.OverTemperature ? colTrue : colFalse;
                labelLRStatusOverLoad.ForeColor = sb.OverLoad ? colTrue : colFalse;
            }
            else
            {
                labelLRStatusByte.Text = filler;
                labelLRStatusStart.ForeColor = colDis;
                labelLRStatusShort.ForeColor = colDis;
                labelLRStatusOverTemp.ForeColor = colDis;
                labelLRStatusOverLoad.ForeColor = colDis;

                labelLRUpFreq.Text = filler;
                labelLRDnFreq.Text = filler;
                labelLRStokeLength.Text = filler;
                labelLRStokeRate.Text = filler;
            }
        }

        void StoryUpdate(MessageStoryItem msi)
        {
            if (msi == null)
                return;

            var rStory = richTextBoxStory;
            rStory.AppendText(msi.DateTime.ToString("HH:mm:ss.ffff"), System.Drawing.Color.DimGray);

            System.Drawing.Color col1, col2;

            if (msi.Dir == MessageStoryItem.Direction.Rx)
            {
                col1 = System.Drawing.Color.FromArgb(0x00, 0xCC, 0x00);
                col2 = System.Drawing.Color.FromArgb(0x00, 0x70, 0x00); ;
                rStory.AppendText(" Rx:\t", col1);
            }
            else
            {
                col1 = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF);
                col2 = System.Drawing.Color.FromArgb(0x00, 0x00, 0x70); ;
                rStory.AppendText(" Tx:\t", col1);
            }

            rStory.AppendText($"{msi.Buffer[0].ToString("X2")} ", col1);
            rStory.AppendText(msi.Destination.ToMinHexString() + " ", col2);
            rStory.AppendText(msi.Source.ToMinHexString() + " ", col1);
            rStory.AppendText(msi.Buffer.ToMinHexString(9, 1) + " ", col2);
            rStory.AppendText(msi.Buffer.ToMinHexString(10, 2) + " ", col1);
            rStory.AppendText(msi.Data.ToMinHexString() + " ", col2);
            rStory.AppendText($"{msi.Crc.ToString("X2")} ", col1);
            
            rStory.AppendText("\r\n");
            rStory.Select(rStory.Text.Length - 1, 0);
        }

        private void ButtonConnect_Click(object sender, System.EventArgs e)
        {
            Svr.StreamSourceCfg = streamSourceControl.StreamSourceCfg;
            var labelConnect = toolStripLabelConnect;
            labelConnect.BackColor = System.Drawing.Color.Transparent;
            if (!Svr.IsConnected)
            {
                try
                {
                    labelConnect.Text = "connecting..";
                    labelConnect.ForeColor = System.Drawing.Color.Gray;

                    Svr.Connect();
                    labelConnect.Text = "connected";
                    labelConnect.ForeColor = System.Drawing.Color.DarkGreen;
                    Svr.Server.Story.CollectionChanged += Story_CollectionChanged;
                    Svr.Server.PropertyChanged += Server_PropertyChanged;
                }
                catch(Exception ex)
                {
                    labelConnect.Text = "fail";
                    labelConnect.ForeColor = System.Drawing.Color.White;
                    labelConnect.BackColor = System.Drawing.Color.Red;
                    richTextBoxStory.AppendText(ex.Message + "\r\n", System.Drawing.Color.Red);
                }                
            }
            else
            {
                Svr.Server.Story.CollectionChanged -= Story_CollectionChanged;
                Svr.Server.PropertyChanged -= Server_PropertyChanged;
                Svr.Disconnect();
                labelConnect.Text = "disconnected";
                labelConnect.ForeColor = System.Drawing.Color.Red;
            }
        }

        private async void ButtonA0_Click(object sender, System.EventArgs e)
        {
            await Svr.Server?.PullBroadcastQuery();
        }

        private async void ButtonA1_Click(object sender, System.EventArgs e)
        {
            await Task.Run(Svr.Server.PullIndividualQuery);
            //await Svr.Server?.PullIndividualQuery();
        }

        private async void ButtonA2_Click(object sender, System.EventArgs e)
        {
            var upFreq = (byte)numericUpDownUpFreq.Value;
            var dnFreq = (byte)numericUpDownDnFreq.Value;
            var stokeRate = (byte)numericUpDownStokeRate.Value;
            await Svr.Server?.PullAutoRun(upFreq, dnFreq, stokeRate);
        }

        private async void ButtonA3_Click(object sender, System.EventArgs e)
        {
            await Svr.Server?.PullShutdown();
        }

        private void NumericUpDownControlIdChanged(object sender, System.EventArgs e)
        {
            Svr.ServerAddress[0] = (byte)numericUpDownControlId0.Value; 
            Svr.ServerAddress[1] = (byte)numericUpDownControlId1.Value; 
            Svr.ServerAddress[2] = (byte)numericUpDownControlId2.Value; 
            Svr.ServerAddress[3] = (byte)numericUpDownControlId3.Value; 
        }

        private void NumericUpDownSlaveIdChanged(object sender, System.EventArgs e)
        {
            Svr.DestenationAddress[0] = (byte)numericUpDownSlaveId0.Value;
            Svr.DestenationAddress[1] = (byte)numericUpDownSlaveId1.Value;
            Svr.DestenationAddress[2] = (byte)numericUpDownSlaveId2.Value;
            Svr.DestenationAddress[3] = (byte)numericUpDownSlaveId3.Value;
        }

        private void DoWorkOnUI(Action action)
        {
            MethodInvoker methodInvokerDelegate = delegate ()
            { action(); };

            if (InvokeRequired)
                Invoke(methodInvokerDelegate);
            else
                action();
        }
        
        private void OpenCfgToolStripMenuItem_Click(object sender, EventArgs e)
        {

            openFileDialog1.FileName = cfgFile;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                Svr = FactorySvr.Get(openFileDialog1.FileName);

            cfgFile = openFileDialog1.FileName;
            FactorySvr.Save(Svr, openFileDialog1.FileName);
        }

        private void SaveCfgToolStripMenuItem_Click(object sender, EventArgs e)
        {

            saveFileDialog1.FileName = cfgFile;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                FactorySvr.Save(Svr, saveFileDialog1.FileName);

            cfgFile = saveFileDialog1.FileName;
        }

        private void TestToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FactorySvr.Save(Svr, cfgFile);
        }
    }
}
