using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace WFQYDB_emu
{
    public class FactoryEmu
    {
        public static IEmu Get(string cfgFile = "EmuCfg.json") 
        {
            if (File.Exists(cfgFile))
                return Load(cfgFile);
            return new TcpEmu();
        }

        public static IEmu Load(string cfgFile = "EmuCfg.json")
        {
            var cfgStr = File.ReadAllText(cfgFile);
            Newtonsoft.Json.Linq.JObject obj = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(cfgStr);
            string className = (string)obj["ClassName"];
            IEmu result;

            switch (className)
            {
                case "TcpEmu":
                    result = obj.ToObject<TcpEmu>();
                    return result;
                case "SerialEmu":
                    result = obj.ToObject<SerialEmu>();
                    return result;
                default:
                    throw new NotImplementedException(className);
            }
        }

        public static void Save(IEmu emu, string cfgFile = "EmuCfg.json")
        {
            var cfgStr = JsonConvert.SerializeObject(emu, Formatting.Indented);
            File.WriteAllText(cfgFile, cfgStr);
        }
    }
}
