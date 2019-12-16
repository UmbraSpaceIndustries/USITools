using USITools.Logistics;
using KSP.Localization;

namespace USITools
{
    [KSPModule("Power Distributor")]
    public class ModulePowerDistributor : PartModule
    {
        [KSPField]
        public int PowerDistributionRange = 2000;

        [KSPField(guiActive = true, guiName = "#LOC_USI_Tools_PC_Label")]//PDU Range
        public string gui_pduRange;
 
        [KSPField]
        public string RequiredSkill = "ConverterSkill";

        public bool isDistributingPower
        {
            get
            {
                if (this.part == null)
                    return false;
                else if (string.IsNullOrEmpty(RequiredSkill))
                    return true;
                else
                    return LogisticsTools.NearbyCrew(this.vessel, LogisticsSetup.Instance.Config.ScavangeRange, RequiredSkill);
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
                gui_pduRange = Localizer.Format("#LOC_USI_Tools_PC_Info5");//"No Engineer!"
            }
            else
            {
                gui_pduRange = ActiveDistributionRange.ToString() + "m";
            }
        }

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return Localizer.Format("#LOC_USI_Tools_PC_Info6") +//"Distributes power to nearby PowerCouplers\n\n"
                Localizer.Format("#LOC_USI_Tools_PC_Info7", PowerDistributionRange);//"Range: " +  + "m"
        }
    }
}
