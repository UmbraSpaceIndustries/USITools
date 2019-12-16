using KSP.Localization;
namespace USITools
{
    [KSPModule("Resource Distributor")]
    public class ModuleResourceDistributor : PartModule
    {
        [KSPField]
        public int ResourceDistributionRange = 2000;

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return Localizer.Format("#LOC_USI_Tools_MRD_info1", ResourceDistributionRange);//"Distributes resouces to nearby vessels\n\n" + "Range: " +  + "m\n" + "Required: Pilot"
        }
    }
}