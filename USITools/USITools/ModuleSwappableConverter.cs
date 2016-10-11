using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using KolonyTools;
using TestScripts;

namespace USITools
{
    public class ModuleSwappableConverter : PartModule
    {
        [KSPField]
        public bool autoActivate = true;

        [KSPField(isPersistant = true)]
        public int currentLoadout = 0;

        [KSPField]
        public string ResourceCosts = "";

        [KSPField]
        public string bayName = "";

        [KSPField]
        public string typeName = "Loadout";

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Active: ")]
        public string curTemplate = "???";

        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "B1: Install [None]",unfocusedRange = 10f)]
        public void LoadSetup()
        {
            if (!CheckResources())
                return;
            var oldTemplate = curTemplate;
            currentLoadout = displayLoadout;
            SetupMenus();
            AdjustEfficiency();
            ScreenMessages.PostScreenMessage("Reconfiguration from " + oldTemplate + " to " + curTemplate + " completed.", 5f,
                ScreenMessageStyle.UPPER_CENTER);
            NextSetup();
        }

        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "B1: Next Loadout",unfocusedRange = 10f)]
        public void NextSetup()
        {
            displayLoadout++;
            if (displayLoadout >= Loadouts.Count)
            {
                displayLoadout = 0;
            }
            if(displayLoadout == currentLoadout)
                NextSetup();
            ChangeMenu();
        }

        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "B1:  Prev. Loadout", unfocusedRange = 10f)]
        public void PrevSetup()
        {
            displayLoadout--;
            if (displayLoadout < 0)
            {
                displayLoadout = Loadouts.Count-1;
            }
            if (displayLoadout == currentLoadout)
                PrevSetup();
            ChangeMenu();
        }

        private bool CheckResources()
        {
            if (HighLogic.LoadedSceneIsEditor)
                return true;
            //Check for an engineer
            var kerbal = FlightGlobals.ActiveVessel.rootPart.protoModuleCrew[0];
            if (kerbal.experienceTrait.Title != "Engineer")
            {
                ScreenMessages.PostScreenMessage("Only Engineers can reconfigure modules!", 5f,
                    ScreenMessageStyle.UPPER_CENTER);
                return false;
            }

            var allResources = true;
            var missingResources = "";
            //Check that we have everything we need.
            foreach (var r in ResCosts)
            {
                if (!HasResource(r))
                {
                    allResources = false;
                    missingResources += "\n" + r.Ratio + " " + r.ResourceName;
                }
            }
            if (!allResources)
            {
                ScreenMessages.PostScreenMessage("Missing resources to change module:" + missingResources, 5f,
                    ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            //Since everything is here...
            foreach (var r in ResCosts)
            {
                TakeResources(r);
            }


            return true;
        }

        private bool HasResource(ResourceRatio resInfo)
        {
            var resourceName = resInfo.ResourceName;
            var needed = resInfo.Ratio;
            var whpList = LogisticsTools.GetRegionalWarehouses(vessel, "USI_ModuleResourceWarehouse");
            //EC we're a lot less picky...
            if (resInfo.ResourceName == "ElectricCharge")
            {
                whpList.AddRange(part.vessel.parts);
            }
            foreach (var whp in whpList.Where(w => w != part))
            {
                if (resInfo.ResourceName != "ElectricCharge")
                {
                    var wh = whp.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                    if (!wh.transferEnabled)
                        continue;
                }
                if (whp.Resources.Contains(resourceName))
                {
                    var res = whp.Resources[resourceName];
                    if (res.amount >= needed)
                    {
                        needed = 0;
                        break;
                    }
                    else
                    {
                        needed -= res.amount;
                        res.amount = 0;
                    }
                }
            }
            return (needed < ResourceUtilities.FLOAT_TOLERANCE);
        }

        private void TakeResources(ResourceRatio resInfo)
        {
            var resourceName = resInfo.ResourceName;
            var needed = resInfo.Ratio;
            //Pull in from warehouses

            var whpList = LogisticsTools.GetRegionalWarehouses(vessel, "USI_ModuleResourceWarehouse");
            foreach (var whp in whpList.Where(w => w != part))
            {
                var wh = whp.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                if (!wh.transferEnabled)
                    continue;
                if (whp.Resources.Contains(resourceName))
                {
                    var res = whp.Resources[resourceName];
                    if (res.amount >= needed)
                    {
                        res.amount -= needed;
                        needed = 0;
                        break;
                    }
                    else
                    {
                        needed -= res.amount;
                        res.amount = 0;
                    }
                }
            }
        }

        private void ChangeMenu()
        {
            Events["NextSetup"].guiName = (bayName + " Next " + typeName).Trim();
            Events["PrevSetup"].guiName = (bayName + " Prev. " + typeName).Trim();
            Fields["curTemplate"].guiName = (bayName + " Active " + typeName).Trim();
            curTemplate = Loadouts[currentLoadout].LoadoutName;
            Events["LoadSetup"].guiName =
                (bayName + " " + curTemplate + "=>" + Loadouts[displayLoadout].LoadoutName).Trim();

            MonoUtilities.RefreshContextWindows(part);
        }

        private int displayLoadout;
        public List<ResourceRatio> ResCosts;
        public List<LoadoutInfo> Loadouts;
        //private IResourceBroker _broker;

        public override void OnStart(StartState state)
        {
            //_broker = new ResourceBroker();
            if(autoActivate || HighLogic.LoadedSceneIsEditor)
                SetModuleState(null,true);
            GameEvents.OnAnimationGroupStateChanged.Add(SetModuleState);
            MonoUtilities.RefreshContextWindows(part);
        }

        public void OnDestroy()
        {
            GameEvents.OnAnimationGroupStateChanged.Remove(SetModuleState);
        }

        private void SetModuleState(ModuleAnimationGroup module, bool enable)
        {
            if (module != null && module.part != part)
                return;

            if (enable)
            {
                EnableMenus(HighLogic.LoadedSceneIsEditor);
                SetupResourceCosts();
                SetupLoadouts();
                displayLoadout = currentLoadout;
                SetupMenus();
                AdjustEfficiency();
                NextSetup();
            }
            else
            {
                EnableMenus(false);
            }
        }

        private void EnableMenus(bool enable)
        {
            Events["NextSetup"].guiActiveEditor = enable;
            Events["PrevSetup"].guiActiveEditor = enable;
            Events["LoadSetup"].guiActiveEditor = enable;
            Events["NextSetup"].externalToEVAOnly = !enable;
            Events["PrevSetup"].externalToEVAOnly = !enable;
            Events["LoadSetup"].externalToEVAOnly = !enable;
            MonoUtilities.RefreshContextWindows(part);
        }

        private void AdjustEfficiency()
        {
            var bays = part.FindModulesImplementing<ModuleSwappableConverter>();
            var modules = part.FindModulesImplementing<BaseConverter>();
            float activeBays = bays.Count(b => b.currentLoadout >= 0);
            float eBonus = bays.Count / activeBays;
            foreach (var bay in bays)
            {
                if (bay.currentLoadout >= 0 && bay.Loadouts != null)
                {
                    modules[bay.currentLoadout].Efficiency
                        = bay.Loadouts[bay.currentLoadout].BaseEfficiency*eBonus;
                }
            }
        }


        public void SetupMenus()
        {
            var modules = part.FindModulesImplementing<BaseConverter>();
            for (int i = 0; i < modules.Count; ++i)
            {
                if(EnabledByAnyModule(i))
                    modules[i].EnableModule();
                else
                    modules[i].DisableModule();
            }
            ChangeMenu();
        }

        private bool EnabledByAnyModule(int moduleId)
        {
            var modules = part.FindModulesImplementing<ModuleSwappableConverter>();
            for (int i = 0; i < modules.Count; ++i)
            {
                if (modules[i].currentLoadout == moduleId)
                    return true;
            }
            return false;
        }


        private void SetupResourceCosts()
        {
            ResCosts = new List<ResourceRatio>();
            if (String.IsNullOrEmpty(ResourceCosts))
                return;

            var resources = ResourceCosts.Split(',');
            for (int i = 0; i < resources.Length; i += 2)
            {
                ResCosts.Add(new ResourceRatio
                {
                    ResourceName = resources[i],
                    Ratio = double.Parse(resources[i + 1])
                });
            }
        }

        private void SetupLoadouts()
        {
            //Get our Module List
            Loadouts = new List<LoadoutInfo>();
            int id = 0;
            var loadoutNames = new List<string>();
            var mods = part.FindModulesImplementing<BaseConverter>();
            foreach (var con in mods)
            {
                var loadout = new LoadoutInfo();
                loadout.BaseEfficiency = con.Efficiency;
                loadout.LoadoutName = con.ConverterName;
                loadout.ModuleId = id;
                loadoutNames.Add(con.ConverterName);
                Loadouts.Add(loadout);
                con.DisableModule(); ;
                id++;
            }
            MonoUtilities.RefreshContextWindows(part);
        }

    }
}