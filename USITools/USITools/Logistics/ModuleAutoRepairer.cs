using System;

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
