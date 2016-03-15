using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace KolonyTools
{
    [KSPModule("Resource Distributor")]
    public class ModuleResourceDistributor : PartModule
    {
        [KSPField]
        public int ResourceDistributionRange = 2000;

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return "Distributes resouces to nearby vessels\n\n" +
                "Range: " + ResourceDistributionRange + "m\n" +
                "Required: Pilot";
        }
    }
}