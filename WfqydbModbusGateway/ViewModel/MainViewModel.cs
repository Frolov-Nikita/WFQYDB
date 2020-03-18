using Binding.Observables;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WFQYDB;
using WFQYDB.Communication;
using WfqydbModbusGateway.Model;

namespace WfqydbModbusGateway.ViewModel
{
    public class ResponseText : INotifyPropertyChanged
    {

        private const string notAviableText = "N/A";

        public event PropertyChangedEventHandler PropertyChanged;

        public string DateTime { get; set; } = notAviableText;

        public string LastCmd { get; set; } = notAviableText;

        public string UpFreq { get; set; } = notAviableText;

        public string DnFreq { get; set; } = notAviableText;

        public string StokeLength { get; set; } = notAviableText;

        public string StokeRate { get; set; } = notAviableText;

        public string StatusByte { get; set; } = notAviableText;

        public string StatusStart { get; set; } = notAviableText;

        public string StatusShortCircuit { get; set; } = notAviableText;

        public string StatusOverTemp { get; set; } = notAviableText;

        public string StatusOverLoad { get; set; } = notAviableText;

        public ResponseText(MessageStoryItem msi = default)
        {
            if (msi == default)
                return;

            this.DateTime = msi.DateTime.ToString();
            LastCmd = msi.Command.ToString();

            if (msi.DataLength > 12)
            {
                UpFreq = msi.Data[0].ToString();
                DnFreq = msi.Data[1].ToString();

                StokeLength = msi.Data[2].ToString();
                StokeRate = msi.Data[3].ToString();

                StatusByte = msi.Data[3].ToString("X2");
                var sb = new StatusByte { Byte = msi.Data[4] };
                StatusStart = sb.Start ? "Started" : "Stopped";
                StatusShortCircuit = sb.ShortCircuit ? "ShortCircuit" : "ok";
                StatusOverTemp = sb.OverTemperature ? "OverHeat" : "ok";
                StatusOverLoad = sb.OverLoad ? "OverLoad" : "ok";
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private const string notAviableText = "N/A";

        private IStreamSourceConfig connnectionWFQYDB;
        private IStreamSourceConfig connnectionModbus;
        private WFQYDBServer wFQYDBServer;
        private ModbusSvr modbusSvr;

        public string ConnnectionWfqydbCfg =>
            ConnnectionWfqydb.ToString();

        public string ConnnectionModbusCfg =>
            $"TCP Server: 127.0.0.1:{ModbusSvr.TcpPort}";

        public string LastResponseDateTime { get; private set; } = notAviableText;
        public string LastResponseLastCmd { get; private set; } = notAviableText;
        public string LastResponseUpFreq { get; private set; } = notAviableText;
        public string LastResponseDnFreq { get; private set; } = notAviableText;
        public string LastResponseStokeLength { get; private set; } = notAviableText; public string LastResponseStokeRate { get; private set; } = notAviableText;
        public string LastResponseStatusByte { get; private set; } = notAviableText;
        public string LastResponseStatusStart { get; private set; } = notAviableText;

        public Color LastResponseStatusStartColor { get; private set; } = Color.DarkGray;
        public string LastResponseStatusShortCircuit { get; private set; } = notAviableText;
        public Color LastResponseStatusShortCircuitColor { get; private set; } = Color.DarkGray;
        public string LastResponseStatusOverTemp { get; private set; } = notAviableText;
        public Color LastResponseStatusOverTempColor { get; private set; } = Color.DarkGray;
        public string LastResponseStatusOverLoad { get; private set; } = notAviableText;
        public Color LastResponseStatusOverLoadColor { get; private set; } = Color.DarkGray;

        public WFQYDBServer WFQYDBServer
        {
            get => wFQYDBServer;
            set
            {
                wFQYDBServer = value;
                NotifyPropertyChanged();
                wFQYDBServer.Story.CollectionChanged += Story_CollectionChanged;
                wFQYDBServer.PropertyChanged += WFQYDBServer_PropertyChanged;
                wFQYDBServer.OnError += WFQYDBServer_OnError;
            }
        }

        private void WFQYDBServer_OnError(object sender, System.IO.ErrorEventArgs e)
        {
            MessageBox.Show("WFQYDB Error", GetInnerMessage(e.GetException()), delegate (MessageBoxResult result) { });
        }

        private string GetInnerMessage(Exception ex)
        {
            if (ex.InnerException != null)
                return GetInnerMessage(ex.InnerException);
            return ex.Message;
        }

        private void WFQYDBServer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WFQYDBServer.LastResponse):
                    var msi = WFQYDBServer.LastResponse;
                    if (msi == default)
                        return;

                    LastResponseDateTime = msi.DateTime.ToString();
                    LastResponseLastCmd = msi.Command.ToString();

                    NotifyPropertyChanged(nameof(LastResponseDateTime));
                    NotifyPropertyChanged(nameof(LastResponseLastCmd));

                    if (msi.DataLength > 4)
                    {
                        LastResponseUpFreq = msi.Data[0].ToString();
                        LastResponseDnFreq = msi.Data[1].ToString();

                        LastResponseStokeLength = msi.Data[2].ToString();
                        LastResponseStokeRate = msi.Data[3].ToString();

                        LastResponseStatusByte = msi.Data[3].ToString("X2");
                        var sb = new StatusByte { Byte = msi.Data[4] };

                        LastResponseStatusStart = sb.Start ? "Started" : "Stopped";
                        LastResponseStatusStartColor = sb.Start ? Color.DarkGreen : Color.DarkRed;

                        LastResponseStatusShortCircuit = sb.ShortCircuit ? "ShortCircuit" : "ok";
                        LastResponseStatusShortCircuitColor = sb.ShortCircuit ? Color.Red : Color.DarkGreen;

                        LastResponseStatusOverTemp = sb.OverTemperature ? "OverHeat" : "ok";
                        LastResponseStatusOverTempColor = sb.OverTemperature ? Color.Red : Color.DarkGreen;

                        LastResponseStatusOverLoad = sb.OverLoad ? "OverLoad" : "ok";
                        LastResponseStatusOverLoadColor = sb.OverLoad ? Color.Red : Color.DarkGreen;

                        NotifyPropertyChanged(nameof(LastResponseUpFreq));
                        NotifyPropertyChanged(nameof(LastResponseDnFreq));
                        NotifyPropertyChanged(nameof(LastResponseStokeLength));
                        NotifyPropertyChanged(nameof(LastResponseStokeRate));
                        NotifyPropertyChanged(nameof(LastResponseStatusByte));

                        NotifyPropertyChanged(nameof(LastResponseStatusStart));
                        NotifyPropertyChanged(nameof(LastResponseStatusShortCircuit));
                        NotifyPropertyChanged(nameof(LastResponseStatusOverTemp));
                        NotifyPropertyChanged(nameof(LastResponseStatusOverLoad));

                        NotifyPropertyChanged(nameof(LastResponseStatusStartColor));
                        NotifyPropertyChanged(nameof(LastResponseStatusShortCircuitColor));
                        NotifyPropertyChanged(nameof(LastResponseStatusOverTempColor));
                        NotifyPropertyChanged(nameof(LastResponseStatusOverLoadColor));
                    }
                    break;
                default:
                    break;
            }

        }

        private void Story_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var i in e.NewItems)
                        Story.Add(i.ToString() ?? "");
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var i in e.OldItems)
                        Story.Remove(i.ToString() ?? "");
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }

        ObservableList<string> Story { get; } = new ObservableList<string>(new List<string>());

        public ModbusSvr ModbusSvr
        {
            get => modbusSvr;
            set
            {
                modbusSvr = value;
                NotifyPropertyChanged();
                modbusSvr.OnError += ModbusSvr_OnError;
            }
        }

        private void ModbusSvr_OnError(object sender, System.IO.ErrorEventArgs e)
        {
            MessageBox.Show("Modbus svr error", GetInnerMessage(e.GetException()), delegate (MessageBoxResult result) { });
        }

        public IStreamSourceConfig ConnnectionWfqydb
        {
            get => connnectionWFQYDB;
            set
            {
                connnectionWFQYDB = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ConnnectionWfqydbCfg));
            }
        }

        public IStreamSourceConfig ConnnectionModbus
        {
            get => connnectionModbus;
            set
            {
                connnectionModbus = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ConnnectionModbusCfg));
            }
        }

        public MainViewModel()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
