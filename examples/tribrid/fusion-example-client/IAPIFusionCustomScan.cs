using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thermo.Interfaces.FusionAccess_V1.Control.Scans;

namespace FusionExampleClient
{
    //
    // This class provides a wrapper for the FusionCustomScan, allowing access to the FusionCustomScan properties and methods.
    //
    public class IAPIFusionCustomScan : IFusionCustomScan
    {
        public IAPIFusionCustomScan()
        {
            Values = new Dictionary<string, string>();
        }

        public bool IsPAGCScan { get; set; }
        public long PAGCGroupIndex { get; set; }
        public double SingleProcessingDelay { get; set; }
        public IDictionary<string, string> Values { get; set; }
        public long RunningNumber { get; set; }
    }
}
