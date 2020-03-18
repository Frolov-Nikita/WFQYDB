using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WFQYDB.Communication
{
    public interface IStreamSourceConfig: INotifyPropertyChanged, INamedClass
    {

        // internal ???
        IStreamSource Get();

        string Cfg { get; set; }

        IStreamSourceConfig FromCfgString(string connString);

        string ToCfgString();

        //TODO: bool IsValid {get;}

    }
}
