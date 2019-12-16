using System.Collections.Generic;
using KSP.Localization;
namespace USITools
{
    [KSPModule("Power Coupler")]
    public class ModulePowerCoupler : PartModule
    {
        [KSPField(guiActive = true, guiName = "#LOC_USI_Tools_PC")]//PowerCoupler
        public string gui_powerCoupler;

        public int numPowerSources { get; set; }

        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            //Surface only
            if (vessel == null || !vessel.LandedOrSplashed)
            {
                gui_powerCoupler = Localizer.Format("#LOC_USI_Tools_PC_Info1");//"Not landed!"
            }
            else if (numPowerSources > 0)
            {
                gui_powerCoupler = numPowerSources + " PDU" + (numPowerSources == 1 ? "" : "s");//
            }
            else
            {
                gui_powerCoupler = Localizer.Format("#LOC_USI_Tools_PC_Info2");//"No PDUs in Range"
            }
        }

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return  Localizer.Format("#LOC_USI_Tools_PC_Info3") +//"Receives power from nearby Power Distribution Units (PDUs)\n\n"
                Localizer.Format("#LOC_USI_Tools_PC_Info4");//"Required: landed/splashed down"
        }
    }
}