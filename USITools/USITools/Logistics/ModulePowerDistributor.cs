namespace KolonyTools
{
    [KSPModule("Power Distributor")]
    public class ModulePowerDistributor : PartModule
    {
        [KSPField]
        public int PowerDistributionRange = 2000;

        public override string GetInfo()
        {
            return base.GetInfo() + "\nWill distribute power to Power Couplers\n";
        }
    }
}