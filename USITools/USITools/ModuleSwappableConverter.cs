using System;
using System.Collections.Generic;
using System.Text;

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
            ScreenMessages.PostScreenMessage("Reconfiguration from " + oldTemplate + " to " + curTemplate + " completed.", 5f,
                ScreenMessageStyle.UPPER_CENTER);
            NextSetup();
        }

        [KSPEvent(active = true, guiActiveUnfocused = true, externalToEVAOnly = true, guiName = "B1: Next Loadout",unfocusedRange = 10f)]
        public void NextSetup()
        {
            if (Loadouts.Count < 2)
                return;

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
            if (Loadouts.Count < 2)
                return;
            
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
            var kerbal = FlightGlobals.ActiveVessel.rootPart.protoModuleCrew[0];
            if (!kerbal.HasEffect("RepairSkill"))
            {
                ScreenMessages.PostScreenMessage("Only Kerbals with repair skills (engineers, mechanics) can reconfigure modules!", 5f,
                    ScreenMessageStyle.UPPER_CENTER);
                return false;
            }

            var allResources = true;
            var missingResources = "";
            //Check that we have everything we need.
            var count = ResCosts.Count;
            for(int i = 0; i < count; ++i)
            {
                var r = ResCosts[i];
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
            for (int i = 0; i < count; ++i)
            {
                var r = ResCosts[i];
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
            var count = whpList.Count;
            for (int i = 0; i < count; ++i)
            {
                var whp = whpList[i];
                if(whp == part)
                    continue;

                if (resInfo.ResourceName != "ElectricCharge")
                {
                    var wh = whp.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                    if (wh != null)
                    {
                        if (!wh.localTransferEnabled)
                            continue;
                    }
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
            var count = whpList.Count;
            for (int i = 0; i < count; ++i)
            {
                var whp = whpList[i];
                if (whp == part)
                    continue;
                var wh = whp.FindModuleImplementing<USI_ModuleResourceWarehouse>();
                if (wh != null)
                {
                    if (!wh.localTransferEnabled)
                        continue;
                }
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
            if (Loadouts == null)
                SetupLoadouts();

            curTemplate = Loadouts[currentLoadout].LoadoutName;
            Events["LoadSetup"].guiName =
                (bayName + " " + curTemplate + "=>" + Loadouts[displayLoadout].LoadoutName).Trim();

            MonoUtilities.RefreshContextWindows(part);
        }

        private int displayLoadout;
        public List<ResourceRatio> ResCosts;
        public List<LoadoutInfo> Loadouts;
        //private IResourceBroker _broker;
        private List<ModuleSwappableConverter> _bays;

        public override void OnStart(StartState state)
        {
            //_broker = new ResourceBroker();
            _bays = part.FindModulesImplementing<ModuleSwappableConverter>();
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
            var count = mods.Count;
            for (int i = 0; i < count; ++i)
            {
                var con = mods[i];
                var loadout = new LoadoutInfo();
                loadout.LoadoutName = con.ConverterName;
                loadout.ModuleId = id;
                loadoutNames.Add(con.ConverterName);
                Loadouts.Add(loadout);
                if(!con.IsActivated)
                    con.DisableModule(); 
                id++;
            }
            MonoUtilities.RefreshContextWindows(part);
        }

        public override string GetInfo()
        {
            if (String.IsNullOrEmpty(ResourceCosts))
                return "";

            var output = new StringBuilder("Resource Cost:\n\n");
            var resources = ResourceCosts.Split(',');
            for (int i = 0; i < resources.Length; i += 2)
            {
                output.Append(string.Format("{0} {1}\n", double.Parse(resources[i + 1]), resources[i]));
            }
            return output.ToString();
        }
    }
}