using Newtonsoft.Json;
using System.IO;

namespace WfqydbModbusGateway.Model
{
    public static class ConverterConfigFactory
    {
        public static ConverterConfig Get(string cfgFile = "Cfg.json")
        {
            if (File.Exists(cfgFile))
                return Load(cfgFile);
            return new ConverterConfig();
        }

        public static ConverterConfig Load(string cfgFile = "Cfg.json")
        {
            var cfgStr = File.ReadAllText(cfgFile);
            return JsonConvert.DeserializeObject<ConverterConfig>(cfgStr);
        }

        public static void Save(ConverterConfig emu, string cfgFile = "Cfg.json")
        {
            var cfgStr = JsonConvert.SerializeObject(emu, Formatting.Indented);
            File.WriteAllText(cfgFile, cfgStr);
        }
    }
}
