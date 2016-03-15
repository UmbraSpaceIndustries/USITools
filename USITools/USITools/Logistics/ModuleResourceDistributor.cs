using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace KolonyTools
{
    public class ModuleResourceDistributor : PartModule
    {
        [KSPField]
        public int ResourceDistributionRange = 2000;
    }
}