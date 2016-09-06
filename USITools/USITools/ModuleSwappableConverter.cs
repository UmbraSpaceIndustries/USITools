using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using KolonyTools;

namespace USITools
{
    public class ModuleSwappableConverter : PartModule
    {
        [KSPField(isPersistant = true)]
        public int currentLoadout = 0;

        [KSPField]
        public string ResourceCosts = "";

        [KSPField]
        public string DecalTextures =
            "0,Tex1,1,Tex2";

        [KSPField]
        public string DecalObjects =
            "Object1,Object2";

        [KSPField]
        public string bayName = "";

        [KSPField]
        public string typeName = "Loadout";

        [KSPField(guiActiveEditor = true, guiName = "Active: ")]
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
                (bayName + " Install [" + Loadouts[displayLoadout].LoadoutName + "]").Trim();

            MonoUtilities.RefreshContextWindows(part);
        }

        private int displayLoadout;
        public List<ResourceRatio> ResCosts;
        public List<LoadoutInfo> Loadouts;
        public Dictionary<int, string> _decals;
        private IResourceBroker _broker;

        public override void OnStart(StartState state)
        {
            _broker = new ResourceBroker();
            EnableMenus();
            SetupResourceCosts();
            SetupDecals();
            SetupLoadouts();
            displayLoadout = currentLoadout;
            SetupMenus();
            AdjustEfficiency();
            NextSetup();
        }

        private void EnableMenus()
        {
            var isEditor = HighLogic.LoadedSceneIsEditor;
            Events["NextSetup"].guiActiveEditor = isEditor;
            Events["PrevSetup"].guiActiveEditor = isEditor;
            Events["LoadSetup"].guiActiveEditor = isEditor;
            Events["NextSetup"].externalToEVAOnly = !isEditor;
            Events["PrevSetup"].externalToEVAOnly = !isEditor;
            Events["LoadSetup"].externalToEVAOnly = !isEditor;
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
                if (bay.currentLoadout >= 0)
                {
                    modules[bay.currentLoadout].Efficiency
                        = bay.Loadouts[bay.currentLoadout].BaseEfficiency*eBonus;
                }
            }
        }


        private void SetupMenus()
        {
            var modules = part.FindModulesImplementing<BaseConverter>();
            for (int i = 0; i < modules.Count; ++i)
            {
                if(i == currentLoadout)
                    modules[i].EnableModule();
                else
                    modules[i].DisableModule();
            }
            ChangeMenu();
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
                if (_decals.ContainsKey(id))
                    loadout.DecalTexture = _decals[id];
                loadout.LoadoutName = con.ConverterName;
                loadout.ModuleId = id;
                loadoutNames.Add(con.ConverterName);
                Loadouts.Add(loadout);
                con.DisableModule(); ;
                id++;
            }
            MonoUtilities.RefreshContextWindows(part);
        }

        private void SetupDecals()
        {
            _decals = new Dictionary<int, string>();
            var decals = DecalTextures.Split(',');
            for (int i = 0; i < decals.Length; i += 2)
            {
                _decals.Add(int.Parse(decals[i]), decals[i + 1]);
            }
        }

        private void SetupResourceCosts()
        {
            ResCosts = new List<ResourceRatio>();
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
    }
}