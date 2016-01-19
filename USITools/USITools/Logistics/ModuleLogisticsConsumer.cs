using System;
using System.Collections.Generic;
using System.Linq;
using FinePrint.Utilities;
using USITools.Logistics;

namespace KolonyTools
{
    public class ModuleLogisticsConsumer : PartModule
    {
        private double lastCheck;
 
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
                    CheckLogistics(con.inputList);
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
                });
            }

            // Special for USI-Config'd Extraplanetary LaunchPads
            if (part.Modules.Contains("ExWorkshop"))
            {
                CheckLogistics(new List<ResourceRatio>
                {
                    new ResourceRatio {ResourceName = "MaterialKits"}
                });
            }

            //Always check for power!
            CheckLogistics(new List<ResourceRatio>
            {
                new ResourceRatio {ResourceName = "ElectricCharge"}
            });
        }

        private void CheckLogistics(List<ResourceRatio> resList)
        {
            //Surface only
            if (!vessel.LandedOrSplashed)
                return;

            var resourceSources = GetResourceStockpiles();
            var powerSources = new List<Vessel>();
            
            if(part.FindModulesImplementing<ModulePowerCoupler>().Any())
                powerSources.AddRange(GetPowerDistributors());

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
                    var rr = p.Resources[res.ResourceName];
                    maxAmount += rr.maxAmount;
                    curAmount += rr.amount;
                }
                double fillPercent = curAmount / maxAmount; //We use this to equalize things cross-ship as a percentage.
                if (fillPercent < 0.5d) //We will not attempt a fillup until we're at less than half capacity
                {
                    //Keep changes small - 10% per tick.  So we should hover between 50% and 60%
                    var deficit = maxAmount * .1d;
                    double receipt = FetchResources(deficit, pRes, fillPercent, maxAmount, sourceList);
                    //Put these in our vessel
                    StoreResources(receipt, pRes);
                }
            }
        }

        public List<Vessel> GetResourceStockpiles()
        {
            var depots = LogisticsTools.GetNearbyVessels(LogisticsSetup.Instance.Config.ScavangeRange, false, vessel, true);
            var nearbyVesselList = LogisticsTools.GetNearbyVessels(LogisticsTools.PHYSICS_RANGE, false, vessel, true);
            foreach (var v in nearbyVesselList)
            {
                var range = LogisticsTools.GetRange(vessel, v);
                var parts =
                    v.Parts.Where(
                        p => p.FindModuleImplementing<ModuleResourceDistributor>() != null && HasCrew(p, "Pilot"));
                foreach (var p in parts)
                {
                    var m = p.FindModuleImplementing<ModuleResourceDistributor>();
                    if (range <= m.ResourceDistributionRange)
                    {
                        //Now find ones adjacent to our depot.                        
                        var stockpiles = LogisticsTools.GetNearbyVessels(m.ResourceDistributionRange, false, vessel,
                            true);
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
                var parts =
                    v.Parts.Where(
                        p => p.FindModuleImplementing<ModulePowerDistributor>() != null && HasCrew(p, "Engineer"));
                foreach(var p in parts)
                {
                    var m = p.FindModuleImplementing<ModulePowerDistributor>();
                    if(range <= m.PowerDistributionRange)
                        distributors.Add(v);
                }
            }
            return distributors;
        }

        private bool HasCrew(Part p, string skill)
        {
            if (p.CrewCapacity > 0)
            {
                return (p.protoModuleCrew.Any(c => c.experienceTrait.TypeName == skill));
            }
            else
            {
                return (p.vessel.GetVesselCrew().Any(c => c.experienceTrait.TypeName == skill));
            }
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

    }
}