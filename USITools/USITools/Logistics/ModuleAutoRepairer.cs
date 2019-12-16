using System;
using KSP.Localization;

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

            var engineerCount = 0;
            var cCount = part.protoModuleCrew.Count;
            for (int i = 0; i < cCount; ++i)
            {
                if (part.protoModuleCrew[i].experienceTrait.TypeName == "Engineer")
                    engineerCount++;
            }

            if (engineerCount == 0)
                return;

            lastCheck = Planetarium.GetUniversalTime();

            var nearbyVesselList = LogisticsTools.GetNearbyVessels(RepairRange, true, vessel, false);
            var vCount = nearbyVesselList.Count;
            for(int i = 0; i < vCount; ++i)
            {
                var v = nearbyVesselList[i];
                var modList = v.FindPartModulesImplementing<USI_ModuleFieldRepair>();
                var mCount = modList.Count;
                for(int x = 0; x < mCount; ++x)
                {
                    var m = modList[x];
                    m.FinalizeMaintenance(Localizer.Format("#LOC_USI_Tools_MAR_msg1", m.part.partInfo.title));//"Automated Maintenance performed on " + 
                }
            }
        }

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return Localizer.Format("#LOC_USI_Tools_MAR_info1") +//"Performs daily maintenance on parts in nearby vessels\n\n"
                Localizer.Format("#LOC_USI_Tools_MAR_info2", (int)RepairRange) +//"Range: " +  + "m\n"
               Localizer.Format("#LOC_USI_Tools_MAR_info3") ;//"Required: Engineer"
        }
    }
}
