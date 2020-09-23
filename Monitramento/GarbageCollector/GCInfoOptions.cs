using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monitramento.GarbageCollector
{
    public class GCInfoOptions
    {
        // The failure threshold (in bytes)
        public long Threshold { get; set; } = 1024L * 1024L * 1024L;
    }
}
