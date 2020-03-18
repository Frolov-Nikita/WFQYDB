using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFQYDB;
using WFQYDB.Communication;

namespace WF_svr
{

    public static class FactorySvr
    {

        public static Svr Get(string cfgFile = "SvrCfg.json")
        {
            try
            {
                if (File.Exists(cfgFile))
                    return Load(cfgFile);
            }catch
            {
            }
            return new Svr();
        }

        public static Svr Load(string cfgFile = "SvrCfg.json")
        {
            var cfgStr = File.ReadAllText(cfgFile);
            var svrJO = ((Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(cfgStr)); //.GetValue("StreamSource")
            var svrO = new Svr();
            svrO.DestenationAddress = svrJO.GetValue("DestenationAddress").ToObject<byte[]>();
            svrO.ServerAddress = svrJO.GetValue("ServerAddress").ToObject<byte[]>();
            var streamSourceClassName = ((Newtonsoft.Json.Linq.JObject)svrJO.GetValue("StreamSource")).GetValue("ClassName").ToObject<string>();

            switch (streamSourceClassName)
            {
                case nameof(SerialPortStreamSourceConfig):
                    svrO.StreamSourceCfg = svrJO.GetValue("StreamSource").ToObject<SerialPortStreamSourceConfig>();
                    break;

                case nameof(TcpStreamSourceConfig):
                    svrO.StreamSourceCfg = svrJO.GetValue("StreamSource").ToObject<TcpStreamSourceConfig>();
                    break;

                case nameof(TcpServerStreamSourceConfig):
                    svrO.StreamSourceCfg = svrJO.GetValue("StreamSource").ToObject<TcpServerStreamSourceConfig>();
                    break;

                default:
                    break;
            }


            return svrO;
            //return JsonConvert.DeserializeObject<Svr>(cfgStr, new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.Objects, TypeNameAssemblyFormat });
        }

        public static void Save(Svr svr, string cfgFile = "SvrCfg.json")
        {
            var cfgStr = JsonConvert.SerializeObject(svr, Formatting.Indented);
            File.WriteAllText(cfgFile, cfgStr);
        }
    }
}
