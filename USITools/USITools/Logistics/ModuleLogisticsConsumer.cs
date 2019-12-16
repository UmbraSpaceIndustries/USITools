using System;
using System.Collections.Generic;
using System.Linq;
using USITools.Logistics;
using KSP.Localization;

namespace USITools
{
    [KSPModule("Logistics Consumer")]
    public class ModuleLogisticsConsumer : PartModule
    {
        [KSPField(isPersistant = true)]
        private double lastCheck = -1d;

        [KSPField]
        public string autoResources = "";

        // Info about the module in the Editor part list
        public override string GetInfo()
        {
            return Localizer.Format("#LOC_USI_Tools_LC_Info") +//"Scavanges nearby warehouses or more distant piloted distribution hubs\n\n"
                Localizer.Format("#LOC_USI_Tools_LC_Info2", LogisticsSetup.Instance.Config.ScavangeRange);//"   "
        }

        public override void OnAwake()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

			_conMods = part.FindModulesImplementing<ModuleResourceConverter>();
            _maxDelta = ResourceUtilities.GetMaxDeltaTime();
        }

        private bool CatchupDone;
        private double _maxDelta;
        private List<ModuleResourceConverter> _conMods;

        private bool InCatchupMode()
        {
            if (CatchupDone)
                return false;

            var count = _conMods.Count;
            for (int i = 0; i < count; ++i)
            {
                var c = _conMods[i];
                var em = c.GetEfficiencyMultiplier();
                if(c.lastTimeFactor / 2 > em)
                    return true;
            }
            CatchupDone = true;
            return false;
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (lastCheck < 0)
                lastCheck = vessel.lastUT;

            if (!HighLogic.LoadedSceneIsFlight)
                return;

            var planTime = Planetarium.GetUniversalTime();

            if (!InCatchupMode())
            {
                if (Math.Abs(planTime - lastCheck) < LogisticsSetup.Instance.Config.LogisticsTime)
                    return;
            }

            lastCheck = Math.Min(lastCheck + _maxDelta, planTime);

            var count = _conMods.Count;
            for (int i = 0; i < count; ++i)
            {
                var con = _conMods[i];
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

            //And any special resources...
            var auto = autoResources.Split(',');
            var aRes = new List<ResourceRatio>();
            var rCount = auto.Length;
            for (int i = 0; i < rCount; ++i)
            {
                var a = auto[i];
                aRes.Add(new ResourceRatio {ResourceName = a.Trim()});
            }
            if(aRes.Count > 0)
                CheckLogistics(aRes,false);

        }

        private List<String> _blackList = new List<string> { "Machinery", "EnrichedUranium", "DepletedFuel", "Construction", "ReplacementParts" };

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
            var rCount = resList.Count;
            for(int i = 0; i < rCount; i++)
            {
                var res = resList[i];
                //There are certain exeptions - specifically, anything for field repair.
                if (_blackList.Contains(res.ResourceName))
                    continue;

                if (res.ResourceName != "ElectricCharge")
                {
                    //A stockpile must be nearby.
                    if (resourceSources.Count == 0)
                        continue;

                    sourceList = resourceSources;
                }
                else
                {
                    //A PDU must be nearby
                    if (powerSources.Count == 0)
                        continue;
                    sourceList = powerSources;
                }
                //How many do we have in our ship
                var pRes = PartResourceLibrary.Instance.GetDefinition(res.ResourceName);
                var maxAmount = 0d;
                var curAmount = 0d;
                var parts = vessel.parts;
                var pCount = parts.Count;
                for (int x = 0; x < pCount; ++x)
                {
                    var p = parts[x];
                    if (!p.Resources.Contains(res.ResourceName))
                        continue;

                    var wh = p.FindModulesImplementing<USI_ModuleResourceWarehouse>();
                    if (wh.Count > 0)
                    {
                        if (!wh[0].localTransferEnabled)
                            continue;
                    }
                    else
                    {
                        if (res.ResourceName != "ElectricCharge")
                            continue;
                    }                    

                    var rr = p.Resources[res.ResourceName];
                    if (rr.flowState)
                    {
                        maxAmount += rr.maxAmount;
                        curAmount += rr.amount;
                    }
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
            var depots = new List<Vessel>();
            var potDeps = LogisticsTools.GetNearbyVessels(LogisticsSetup.Instance.Config.ScavangeRange, false, vessel, true);
            var count = potDeps.Count;
            for (int i = 0; i < count; ++i)
            {
                if(potDeps[i].FindPartModulesImplementing<USI_ModuleResourceWarehouse>().Count > 0)
                    depots.Add(potDeps[i]);
            }

            var nearbyVesselList = LogisticsTools.GetNearbyVessels(LogisticsTools.PHYSICS_RANGE, true, vessel, true);
            count = nearbyVesselList.Count;
            for (int i = 0; i < count; ++i)
            {
                var v = nearbyVesselList[i];
                var range = LogisticsTools.GetRange(vessel, v);
                var pCount = v.parts.Count;

                for (int q = 0; q < pCount; ++q)
                {
                    var p = v.parts[q];
                    if (p.FindModuleImplementing<ModuleResourceDistributor>() == null)
                        continue;
                    if (!LogisticsTools.HasCrew(p.vessel, "Pilot"))
                        continue;

                    var m = p.FindModuleImplementing<ModuleResourceDistributor>();
                    if (range <= m.ResourceDistributionRange)
                    {
                        //Now find ones adjacent to our depot.                        
                        var potStock = LogisticsTools.GetNearbyVessels(m.ResourceDistributionRange, false, vessel,true);
                        var potCount = potStock.Count;

                        for (int z = 0; z < potCount; ++z)
                        {
                            var s = potStock[z];
                            if (s.FindPartModulesImplementing<USI_ModuleResourceWarehouse>().Count == 0)
                                continue;
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

            var count = nearbyVessels.Count;
            for (int i = 0; i < count; ++i)
            {
                var v = nearbyVessels[i];
                var range = LogisticsTools.GetRange(vessel, v);

                var pCount = v.parts.Count;
                for (int x = 0; x < pCount; ++x)
                {
                    var p = v.parts[x];
                    var mod = p.FindModuleImplementing<ModulePowerDistributor>();
                    if (mod == null)
                        continue;

                    if(mod.ActiveDistributionRange >= range)
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
                var count = vessel.Parts.Count;
                for (int i = 0; i < count; ++i)
                {
                    var p = vessel.parts[i];
                    if (!p.Resources.Contains(resource.name))
                        continue;

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
                var count = vessel.parts.Count;
                for (int i = 0; i < count; ++i)
                {
                    var p = vessel.parts[i];
                    if (!p.Resources.Contains(resource.name))
                        continue;

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
                var count = vList.Count;
                for(int i = 0; i < count; ++i)
                {
                    var v = vList[i];
                    if (demand <= ResourceUtilities.FLOAT_TOLERANCE) break;
                    //Is this a valid target?
                    var maxToSpare = GetAmountOfResourcesToSpare(v, resource, fillPercent + fetched / targetMaxAmount, targetMaxAmount);
                    if (maxToSpare < ResourceUtilities.FLOAT_TOLERANCE)
                        continue;
                    //Can we find what we're looking for?
                    var pCount = v.parts.Count;
                    for(int x = 0; x < pCount; x++)
                    {
                        var p = v.parts[x];
                        if (!p.Resources.Contains(resource.name))
                            continue;

                        //Guard clause.
                        if (resource.name != "ElectricCharge")
                        {
                            var wh = p.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                            if(wh == null)
                                continue;
                            if(!wh.localTransferEnabled)
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
                    var count = v.parts.Count;
                    for (int x = 0; x < count; x++)
                    {
                        var p = v.parts[x];
                        if (!p.Resources.Contains(resource.name))
                            continue;

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
                            if (!wh.localTransferEnabled)
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
            var count = v.parts.Count;
            for (int i = 0; i < count; ++i)
            {
                var p = v.parts[i];
                if (!p.Resources.Contains(resource.name))
                    continue;
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
            var count = v.parts.Count;
            for (int i = 0; i < count; ++i)
            {
                var p = v.parts[i];
                if (!p.Resources.Contains(resource.name))
                    continue;

                var rr = p.Resources[resource.name];
                maxAmount += rr.maxAmount;
                curAmount += rr.amount;
            }
            return maxAmount - curAmount;
        }

    }
}