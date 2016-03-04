using System.Collections.Generic;

namespace KolonyTools
{
    public class ModulePowerCoupler : PartModule
    {
        [KSPField(guiActive = true, guiName = "PowerCoupler")]
        public string gui_powerCoupler;

        public List<Vessel> PowerSources { get; set; }

        public void Update()
        {
            //Surface only
            if (vessel == null || !vessel.LandedOrSplashed)
            {
                gui_powerCoupler = "Not landed!";
            }
            else if (PowerSources != null && PowerSources.Count > 0)
            {
                gui_powerCoupler = PowerSources.Count + " PDU" + (PowerSources.Count == 1 ? "" : "s");
            }
            else
            {
                gui_powerCoupler = "No PDUs in Range";
            }
        }
    }
}