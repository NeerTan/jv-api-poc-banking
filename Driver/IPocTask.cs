using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Driver
{
    public interface IPocTask
    {
        Task<PocResult> Execute(CancellationToken cancellationToken = default);
    }

    public class PocContext
    {

    }

    public class PocResult
    {
        public bool Pass { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
