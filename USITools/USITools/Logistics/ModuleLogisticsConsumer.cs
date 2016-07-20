using System;
using System.Collections.Generic;
using System.Linq;
using FinePrint.Utilities;
using USITools.Logistics;

namespace KolonyTools
{
    [KSPModule("Logistics Consumer")]
    public class ModuleLogisticsConsumer : PartModule
    {
        private double lastCheck;

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return "Scavanges nearby warehouses or more distant piloted distribution hubs\n\n" +
                "Scavange Range: " + LogisticsSetup.Instance.Config.ScavangeRange + "m";
        }
 
        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (Math.Abs(Planetarium.GetUniversalTime() - lastCheck) < LogisticsSetup.Instance.Config.LogisticsTime)
                return;

            lastCheck = Planetarium.GetUniversalTime();

            foreach (var con in part.FindModulesImplementing<ModuleResourceConverter>())
            {
                if (con.inputList != null)
                    CheckLogistics(con.inputList, false);
                if (con.outputList != null)
                    CheckLogistics(con.outputList, true);
            }

            //Special for USI-LS/TAC-LS
            if (vessel.GetCrewCount() > 0)
            {
                CheckLogistics(new List<ResourceRatio>
                {
                    new ResourceRatio {ResourceName = "Supplies"},
                    new ResourceRatio {ResourceName = "Food"},
                    new ResourceRatio {ResourceName = "Water"},
                    new ResourceRatio {ResourceName = "Oxygen"},
                },false);
            }

            // Special for USI-Config'd Extraplanetary LaunchPads
            if (part.Modules.Contains("ExWorkshop"))
            {
                CheckLogistics(new List<ResourceRatio>
                {
                    new ResourceRatio {ResourceName = "MaterialKits"}
                },false);
            }

            //Always check for power!
            CheckLogistics(new List<ResourceRatio>
            {
                new ResourceRatio {ResourceName = "ElectricCharge"}
            },false);
        }

        private void CheckLogistics(List<ResourceRatio> resList, bool output)
        {
            //Surface only
            if (!vessel.LandedOrSplashed)
                return;

            var resourceSources = GetResourceStockpiles();
            var powerSources = new List<Vessel>();

            var powerCoupler = part.FindModuleImplementing<ModulePowerCoupler>();
            if (powerCoupler != null)
            {
                powerSources.AddRange(GetPowerDistributors());
                powerCoupler.numPowerSources = powerSources.Count;
            }

            var sourceList = new List<Vessel>();

            //The konverter will scan for missing resources and
            //attempt to pull them in from nearby ships.

            //Find what we need!
            foreach (var res in resList)
            {
                //There are certain exeptions - specifically, anything for field repair.
                if (res.ResourceName == "Machinery")
                    continue;

                if (res.ResourceName != "ElectricCharge")
                {
                    //A stockpile must be nearby.
                    if (!resourceSources.Any())
                        continue;
                    sourceList = resourceSources;
                }
                else
                {
                    //A PDU must be nearby
                    if (!powerSources.Any())
                        continue;
                    sourceList = powerSources;
                }
                //How many do we have in our ship
                var pRes = PartResourceLibrary.Instance.GetDefinition(res.ResourceName);
                var maxAmount = 0d;
                var curAmount = 0d;
                foreach (var p in vessel.parts.Where(pr => pr.Resources.Contains(res.ResourceName)))
                {
                    var wh = p.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                    if(wh != null)
                        if (!wh.transferEnabled)
                            continue;

                    var rr = p.Resources[res.ResourceName];
                    maxAmount += rr.maxAmount;
                    curAmount += rr.amount;
                }
                double fillPercent = curAmount / maxAmount; //We use this to equalize things cross-ship as a percentage.
                if (output)
                {
                    if (fillPercent > 0.85d)
                    {
                        //We will not attempt to ship out output goods until we are over 85%
                        {
                            //Keep changes small - 10% per tick.  So we should hover between 75% and 85%
                            var surplus = maxAmount * .1d;
                            double pushed = PushResources(surplus, pRes, sourceList);
                            TakeResources(pushed, pRes);
                        }
                    }
                }
                else
                {
                    if (fillPercent < 0.5d )
                    //We will not attempt a fillup until we're at less than half capacity
                    {
                        //Keep changes small - 10% per tick.  So we should hover between 50% and 60%
                        var deficit = maxAmount * .1d;
                        double receipt = FetchResources(deficit, pRes, fillPercent, maxAmount, sourceList);
                        //Put these in our vessel
                        StoreResources(receipt, pRes);
                    }
                }

            }
        }

        public List<Vessel> GetResourceStockpiles()
        {
            List<Vessel> depots = LogisticsTools.GetNearbyVessels(LogisticsSetup.Instance.Config.ScavangeRange, false, vessel, true)
                .Where(dv => dv.FindPartModulesImplementing<USI_ModuleResourceWarehouse>().Any()).ToList();

            var nearbyVesselList = LogisticsTools.GetNearbyVessels(LogisticsTools.PHYSICS_RANGE, true, vessel, true);
            foreach (var v in nearbyVesselList)
            {
                var range = LogisticsTools.GetRange(vessel, v);
                var parts =
                    v.Parts.Where(
                        p => p.FindModuleImplementing<ModuleResourceDistributor>() != null && LogisticsTools.HasCrew(p, "Pilot"));
                foreach (var p in parts)
                {
                    var m = p.FindModuleImplementing<ModuleResourceDistributor>();
                    if (range <= m.ResourceDistributionRange)
                    {
                        //Now find ones adjacent to our depot.                        
                        List<Vessel> stockpiles = LogisticsTools.GetNearbyVessels(m.ResourceDistributionRange, false, vessel,
                            true).Where(sv=>sv.FindPartModulesImplementing<USI_ModuleResourceWarehouse>().Any()).ToList();
                        foreach (var s in stockpiles)
                        {
                            if (!depots.Contains(s))
                                depots.Add(s);
                        }
                    }
                }
            }
            return depots;
        }
        

        public List<Vessel> GetPowerDistributors()
        {
            var distributors = new List<Vessel>();
            var nearbyVessels = LogisticsTools.GetNearbyVessels(LogisticsTools.PHYSICS_RANGE, false,
                vessel, true);

            foreach (var v in nearbyVessels)
            {
                var range = LogisticsTools.GetRange(vessel, v);

                if (v.parts
                    .Select(p => p.FindModuleImplementing<ModulePowerDistributor>())
                    .Any(m => m != null && range <= m.ActiveDistributionRange))
                {
                    distributors.Add(v);
                }
            }
            return distributors;
        }

        private void StoreResources(double amount, PartResourceDefinition resource)
        {
            try
            {
                var transferAmount = amount;
                var partList = vessel.Parts.Where(
                    p => p.Resources.Contains(resource.name));
                foreach (var p in partList)
                {
                    var pr = p.Resources[resource.name];
                    var storageSpace = pr.maxAmount - pr.amount;
                    if (storageSpace >= transferAmount)
                    {
                        pr.amount += transferAmount;
                        break;
                    }
                    else
                    {
                        transferAmount -= storageSpace;
                        pr.amount = pr.maxAmount;
                    }
                }
            }
            catch (Exception ex)
            {
                print(String.Format("[MKS] - ERROR in StoreResources - {0}", ex.StackTrace));
            }
        }

        private double TakeResources(double amount, PartResourceDefinition resource)
        {
            double taken = 0;
            try
            {
                var transferAmount = amount;
                var partList = vessel.Parts.Where(
                    p => p.Resources.Contains(resource.name));
                foreach (var p in partList)
                {
                    PartResource pr = p.Resources[resource.name];
                    var available = pr.amount;
                    if (available >= transferAmount)
                    {
                        taken += transferAmount;
                        pr.amount -= transferAmount;
                        break;
                    }
                    else
                    {
                        taken += available;
                        transferAmount -= available;
                        pr.amount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                print(String.Format("[MKS] - ERROR in StoreResources - {0}", ex.StackTrace));
            }
            return taken;
        }

        private double FetchResources(double amount, PartResourceDefinition resource, double fillPercent, double targetMaxAmount, List<Vessel> vList )
        {
            double demand = amount;
            double fetched = 0d;
            try
            {
                foreach (var v in vList)
                {
                    if (demand <= ResourceUtilities.FLOAT_TOLERANCE) break;
                    //Is this a valid target?
                    var maxToSpare = GetAmountOfResourcesToSpare(v, resource, fillPercent + fetched / targetMaxAmount, targetMaxAmount);
                    if (maxToSpare < ResourceUtilities.FLOAT_TOLERANCE)
                        continue;
                    //Can we find what we're looking for?
                    var partList = v.Parts.Where(p => p.Resources.Contains(resource.name));
                    foreach (var p in partList)
                    {
                        //Guard clause.
                        if (resource.name != "ElectricCharge")
                        {
                            var wh = p.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                            if(wh == null)
                                continue;
                            if(!wh.transferEnabled)
                                continue;
                        }

                        var pr = p.Resources[resource.name];
                        if (pr.amount >= demand)
                        {
                            if (maxToSpare >= demand)
                            {
                                pr.amount -= demand;
                                fetched += demand;
                                demand = 0;
                                break;
                            }
                            else
                            {
                                pr.amount -= maxToSpare;
                                fetched += maxToSpare;
                                demand -= maxToSpare;
                                break;
                            }
                        }
                        else
                        {
                            if (maxToSpare >= pr.amount)
                            {
                                demand -= pr.amount;
                                fetched += pr.amount;
                                maxToSpare -= pr.amount;
                                pr.amount = 0;
                            }
                            else
                            {
                                demand -= maxToSpare;
                                fetched += maxToSpare;
                                pr.amount -= maxToSpare;
                                break;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                print(String.Format("[MKS] - ERROR in FetchResources - {0}", ex.StackTrace));
            }
            return fetched;
        }

        private double PushResources(double amount, PartResourceDefinition resource, List<Vessel> vList)
        {
            double surplus = amount;
            double pushed = 0d;
            try
            {
                //remove full vessels from our list
                vList.RemoveAll(v => GetAmountOfResourceSpace(v, resource) < ResourceUtilities.FLOAT_TOLERANCE);

                for (int i = 0; i < vList.Count; i++)
                {
                    //Attempt to push the remaining surplus equally to all vessels - this will ensure that nearly full vessels get filled quickly
                    //rather that having smaller and smaller quantities pushed every cycle
                    double vesselAmount = surplus / (vList.Count - i);
                    Vessel v = vList[i];
                    var partList = v.Parts.Where(p => p.Resources.Contains(resource.name));
                    foreach (var p in partList)
                    {
                        //Guard clause.
                        if (vesselAmount < ResourceUtilities.FLOAT_TOLERANCE)
                        {
                            break;
                        }
                        if (resource.name != "ElectricCharge")
                        {
                            var wh = p.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                            if (wh == null)
                                continue;
                            if (!wh.transferEnabled)
                                continue;
                        }
                        PartResource res= p.Resources[resource.name];
                        double space = res.maxAmount - res.amount;
                        double add = Math.Min(space, vesselAmount);
                        res.amount += add;
                        pushed += add;
                        surplus -= add;
                        vesselAmount -= add;
                    }
                }
                
            }
            catch (Exception ex)
            {
                print(String.Format("[MKS] - ERROR in PushResources - {0}", ex.StackTrace));
            }
            return pushed;
        }

        private double GetAmountOfResourcesToSpare(Vessel v, PartResourceDefinition resource, double targetPercent, double targetMaxAmount)
        {
            var maxAmount = 0d;
            var curAmount = 0d;
            foreach (var p in v.parts.Where(pr => pr.Resources.Contains(resource.name)))
            {
                var rr = p.Resources[resource.name];
                maxAmount += rr.maxAmount;
                curAmount += rr.amount;
            }
            double fillPercent = maxAmount < ResourceUtilities.FLOAT_TOLERANCE ? 0 : curAmount / maxAmount;
            if (fillPercent > targetPercent)
            {
                //If we're in better shape, they can take some of our stuff.
                var targetCurrentAmount = targetMaxAmount * targetPercent;
                targetPercent = (curAmount + targetCurrentAmount) / (targetMaxAmount + maxAmount);
                return curAmount - maxAmount * targetPercent;
            }
            else
            {
                return 0;
            }
        }

        private double GetAmountOfResourceSpace(Vessel v, PartResourceDefinition resource)
        {
            var maxAmount = 0d;
            var curAmount = 0d;
            foreach (var p in v.parts.Where(pr => pr.Resources.Contains(resource.name)))
            {
                var rr = p.Resources[resource.name];
                maxAmount += rr.maxAmount;
                curAmount += rr.amount;
            }
            return maxAmount - curAmount;
        }

    }
}