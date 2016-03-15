using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace KolonyTools
{
    [KSPModule("Logistics Distributor")]
    public class ModuleResourceDistributor : PartModule
    {
        [KSPField]
        public int ResourceDistributionRange = 2000;

        public override string GetInfo()
        {
            return base.GetInfo() + "\nWill distribute resources to nearby Logistics Consumers\n";
        }

    }
}