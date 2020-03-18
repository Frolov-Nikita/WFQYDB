using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RJCP.IO.Ports;

namespace WFQYDB.Communication
{
    public interface IStreamSource
    {
        int Aviable { get; }
        
        bool IsOpen { get; }

        /// <summary>
        ///     Gets or sets the number of milliseconds before a timeout occurs when a read operation does not finish.
        /// </summary>
        int ReadTimeout { get; set; }

        /// <summary>
        ///     Gets or sets the number of milliseconds before a timeout occurs when a write operation does not finish.
        /// </summary>
        int WriteTimeout { get; set; }

        /// <summary>
        /// Get opened and cached stream
        /// </summary>
        /// <returns></returns>
        Task<Stream> GetStreamAsync();



    }


}