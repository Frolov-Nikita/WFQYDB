using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WFQYDB.Communication
{
    public static class StreamSourceFactory
    {
        // TODO: Уже созданные ресурсы потока не пересоздавать.
        //static Dictionary<string, IStreamSource> serials = new Dictionary<string, IStreamSource>();

        public static IStreamSourceConfig GetStreamSourceConfig(string cfg)
        {
            var cfgParts = cfg.Split(new char[] { ';' }, 2);
            var className = cfgParts[0].Trim();
            var cfgString = cfgParts[1].Trim();

            switch (className)
            {
                case "SerialPortStreamSource":
                    return new SerialPortStreamSourceConfig().FromCfgString(cfgString);

                case "TcpClientStreamSource":
                    return new TcpStreamSourceConfig().FromCfgString(cfgString);

                default:
                    throw new ArgumentException($"Unknown StreamSource: {className}");
            }
        }

        public static IStreamSource GetStreamSource(string cfg) => 
            GetStreamSourceConfig(cfg).Get();
        
    }
}
