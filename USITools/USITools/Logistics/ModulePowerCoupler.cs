namespace KolonyTools
{
	[KSPModule("Power Coupler")]
    public class ModulePowerCoupler : PartModule
    {
        public override string GetInfo()
        {
            return base.GetInfo() + "\nWill consume power from a Power Distributor\n";
        }

    }
}