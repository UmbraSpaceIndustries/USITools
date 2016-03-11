using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KolonyTools;

namespace USITools.Logistics
{
    [KSPModule("Auto-Repairer")]
    public class ModuleAutoRepairer : PartModule
    {
        [KSPField]
        public float RepairRange = 2000f;
        [KSPField]
        public double RepairFrequency = 21600; //Daily
        //Look for all parts in nearby vessels and perform a maintenance action.

        private double lastCheck;

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (Math.Abs(Planetarium.GetUniversalTime() - lastCheck) < RepairFrequency)
                return;

            if (part.protoModuleCrew.All(c => c.experienceTrait.TypeName != "Engineer"))
                return;

            lastCheck = Planetarium.GetUniversalTime();

            var nearbyVesselList = LogisticsTools.GetNearbyVessels(RepairRange, true, vessel, false);
            foreach (var v in nearbyVesselList)
            {
                var modList = v.FindPartModulesImplementing<USI_ModuleFieldRepair>();
                foreach (var m in modList)
                {
                    m.FinalizeMaintenance("Automated Maintenance performed on " + m.part.partInfo.title);
                }
            }
        }

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return "Performs daily maintenance on parts in nearby vessels\n\n" +
                "Range: " + (int)RepairRange + "m\n" +
                "Required: Engineer";
        }
    }
}
