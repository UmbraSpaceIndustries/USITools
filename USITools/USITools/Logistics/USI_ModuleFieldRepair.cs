using System;
using KSP.Localization;

namespace USITools
{
    public class USI_ModuleFieldRepair : PartModule
    {
        //Very simple module.  Just lets you transfer spare parts in via an EVA.
        //super hacky.  Don't judge me.

        [KSPField]
        public float EVARange = 5f;

        [KSPField] 
        public string PullResourceList = "Machinery,EnrichedUranium";

        [KSPField] 
        public string PushResourceList = "DepletedFuel,Recyclables";


        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "#LOC_USI_Performmaintenance",//Perform maintenance
            unfocusedRange = 5f)]
        public void PerformMaintenance()
        {
            var kerbal = FlightGlobals.ActiveVessel.rootPart.protoModuleCrew[0];
            if (!kerbal.HasEffect("RepairSkill"))
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_USI_Tools_msg10"), 5f,//"Only Kerbals with repair skills (engineers, mechanics) can perform EVA Maintenance!"
                    ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            FinalizeMaintenance("You perform routine maintenance...");
        }

        public void FinalizeMaintenance(string msg)
        {
            ScreenMessages.PostScreenMessage(msg, 5f, ScreenMessageStyle.UPPER_CENTER);
            var PullList = PullResourceList.Split(',');
            var PushList = PushResourceList.Split(',');

            var count = PullList.Length;
            for(int i = 0; i < count; ++i)
            {
                var r = PullList[i];
                if (!String.IsNullOrEmpty(r))
                    GrabResources(r);
            }

            count = PushList.Length;
            for (int i = 0; i < count; ++i)
            {
                var r = PushList[i];
                if (!String.IsNullOrEmpty(r))
                    PushResources(r);
            }

            //We have a very special case - MaterialKits => ReplacementParts.
            print("Swapping Resources..");
            SwapResources("MaterialKits", "ReplacementParts");
        }

        public override void OnStart(StartState state)
        {
            Events["PerformMaintenance"].unfocusedRange = EVARange;
            base.OnStart(state);
        }


        private void PushResources(string resourceName)
        {
            if (!part.Resources.Contains(resourceName))
            {
                return;
            }
            var brokRes = part.Resources[resourceName];
            //Put remaining parts in warehouses
            var wh = LogisticsTools.GetRegionalWarehouses(vessel, "USI_ModuleCleaningBin");
            var count = wh.Count;
            for (int i = 0; i < count; ++i)
            {
                var p = wh[i];
                if (p.Resources.Contains(resourceName))
                {
                    var partRes = p.Resources[resourceName];
                    var partNeed = partRes.maxAmount - partRes.amount;
                    if (partNeed > 0 && brokRes.amount > 0)
                    {
                        if (partNeed > brokRes.amount)
                        {
                            partNeed = brokRes.amount;
                        }
                        partRes.amount += partNeed;
                        brokRes.amount -= partNeed;
                    }
                }
            }
        }


        private void SwapResources(string fetchName, string storeName)
        {
            if (!part.Resources.Contains(storeName))
                return;

            var brokRes = part.Resources[storeName];
            var needed = brokRes.maxAmount - brokRes.amount;
            //Pull in from warehouses
            var whpList = LogisticsTools.GetRegionalWarehouses(vessel, "USI_ModuleResourceWarehouse");
            var count = whpList.Count;
            for(int i = 0; i < count; ++i)
            {
                var whp = whpList[i];
                var wh = whp.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                if (wh != null)
                {
                    if (!wh.localTransferEnabled)
                        continue;
                }

                if (whp.Resources.Contains(fetchName))
                {
                    print("Found " + fetchName);
                    var res = whp.Resources[fetchName];
                    if (res.amount >= needed)
                    {
                        brokRes.amount += needed;
                        res.amount -= needed;
                        needed = 0;
                        break;
                    }
                    else
                    {
                        brokRes.amount += res.amount;
                        needed -= res.amount;
                        res.amount = 0;
                    }
                }
            }
        }


        private void GrabResources(string resourceName)
        {
            if (!part.Resources.Contains(resourceName))
                return;

            var brokRes = part.Resources[resourceName];
            var needed = brokRes.maxAmount - brokRes.amount;
            //Pull in from warehouses

            var whpList = LogisticsTools.GetRegionalWarehouses(vessel, "USI_ModuleResourceWarehouse");
            var count = whpList.Count;

            for (int i = 0; i < count; ++i)
            {
                var whp = whpList[i];
                if (whp == part)
                    continue;

                var whc = whp.FindModulesImplementing<BaseConverter>();
                if(whc.Count > 0)
                    continue;

                
                if (whp.Modules.Contains("USI_ModuleResourceWarehouse"))
                {
                    var wh = whp.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                    if (!wh.localTransferEnabled)
                        continue;
                }
                if (whp.Resources.Contains(resourceName))
                {
                    var res = whp.Resources[resourceName];
                    if (res.amount >= needed)
                    {
                        brokRes.amount += needed;
                        res.amount -= needed;
                        needed = 0;
                        break;
                    }
                    else
                    {
                        brokRes.amount += res.amount;
                        needed -= res.amount;
                        res.amount = 0;
                    }
                }
            }
        }
    }
}
