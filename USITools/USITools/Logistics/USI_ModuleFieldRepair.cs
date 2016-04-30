using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KolonyTools
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


        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "Perform maintenance",
            unfocusedRange = 5f)]
        public void PerformMaintenance()
        {
            var kerbal = FlightGlobals.ActiveVessel.rootPart.protoModuleCrew[0];
            if (kerbal.experienceTrait.Title != "Engineer")
            {
                ScreenMessages.PostScreenMessage("Only Engineers can perform EVA Maintenance!", 5f,
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

            foreach (var r in PullList)
            {
                if (!String.IsNullOrEmpty(r))
                    GrabResources(r);
            }
            foreach (var r in PushList)
            {
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
            var brokRes = part.Resources[resourceName];
            //Put remaining parts in warehouses
            foreach (var p in LogisticsTools.GetRegionalWarehouses(vessel, "USI_ModuleCleaningBin"))
            {
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
            print("Making sure part contains " + storeName);
            if (!part.Resources.Contains(storeName))
                return;

            print("Resource exists...");
            var brokRes = part.Resources[storeName];
            var needed = brokRes.maxAmount - brokRes.amount;
            print("We need " + needed);
            //Pull in from warehouses

            var whpList = LogisticsTools.GetRegionalWarehouses(vessel, "USI_ModuleResourceWarehouse");
            print("Found " + whpList.Count() + " warehouses...");
            foreach (var whp in whpList)
            {
                var wh = whp.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                if(!wh.transferEnabled)
                    continue;
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
            foreach (var whp in whpList.Where(w=>w != part))
            {
                var wh = whp.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                if (!wh.transferEnabled)
                    continue; 
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
