using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Driver
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PocTaskAttribute : Attribute
    {
        public int Line { get; set; }
        public string Description { get; set; }
    }
}
