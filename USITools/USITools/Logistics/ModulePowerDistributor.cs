namespace KolonyTools
{
    [KSPModule("Power Distributor")]
    public class ModulePowerDistributor : PartModule
    {
        [KSPField]
        public int PowerDistributionRange = 2000;

        [KSPField(guiActive = true, guiName = "PDU Range")]
        public string gui_pduRange;

        public bool isDistributingPower
        {
            get
            {
                return this.part != null && LogisticsTools.HasCrew(this.part, "Engineer");
            }
        }

        public int ActiveDistributionRange
        {
            get
            {
                return isDistributingPower ? PowerDistributionRange : 0;
            }
        }

        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (!isDistributingPower)
            {
                gui_pduRange = "No Engineer!";
            }
            else
            {
                gui_pduRange = ActiveDistributionRange.ToString() + "m";
            }
        }

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return "Distributes power to nearby PowerCouplers\n\n" +
                "Range: " + PowerDistributionRange + "m";
        }
    }
}