using System.Collections.Generic;

namespace KolonyTools
{
    [KSPModule("Power Coupler")]
    public class ModulePowerCoupler : PartModule
    {
        [KSPField(guiActive = true, guiName = "PowerCoupler")]
        public string gui_powerCoupler;

        public int numPowerSources { get; set; }

        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            //Surface only
            if (vessel == null || !vessel.LandedOrSplashed)
            {
                gui_powerCoupler = "Not landed!";
            }
            else if (numPowerSources > 0)
            {
                gui_powerCoupler = numPowerSources + " PDU" + (numPowerSources == 1 ? "" : "s");
            }
            else
            {
                gui_powerCoupler = "No PDUs in Range";
            }
        }

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return "Receives power from nearby Power Distribution Units (PDUs)\n\n" +
                "Required: landed/splashed down";
        }
    }
}