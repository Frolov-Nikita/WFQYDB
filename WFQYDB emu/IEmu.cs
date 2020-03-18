using System.Collections.Generic;
using WFQYDB;
using WFQYDB.Communication;

namespace WFQYDB_emu
{
    public interface IEmu : INamedClass
    {
        byte[] Id { get; set; }

        //IStreamSourceConfig StreamSourceCfg { get; }

        WFQYDBemul ProtoEmu { get; }

        void Start();

        void Stop();

        Term.TermView GetInfo();
    }
}