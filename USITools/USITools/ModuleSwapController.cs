using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace USITools
{
    public class ModuleSwapController : PartModule
    {
        [KSPField]
        public string ResourceCosts = "";

        [KSPField]
        public string typeName = "Loadout";

        public List<ResourceRatio> ResCosts;
        public List<LoadoutInfo> Loadouts;
        private List<ModuleSwappableConverter> _bays;
        private List<BaseConverter> _modules;
        private IEnumerator _setupLoadouts;

        public override void OnStart(StartState state)
        {
            _modules = part.FindModulesImplementing<BaseConverter>();
            _bays = part.FindModulesImplementing<ModuleSwappableConverter>();
            SetupLoadouts();
            SetupResourceCosts();
            SetModuleStates();
            MonoUtilities.RefreshContextWindows(part);
        }

        public int GetModuleCount()
        {
            return _modules.Count;
        }

        public bool EnabledByAnyModule(int moduleId)
        {
            for (int i = 0; i < _bays.Count; ++i)
            {
                if (_bays[i].currentLoadout == moduleId)
                    return true;
            }
            return false;
        }

        public BaseConverter GetConverter(int i)
        {
            return _modules[i];
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

        public void SetupLoadouts()
        {
            if(_setupLoadouts != null)
                StopCoroutine(_setupLoadouts);

            _setupLoadouts = RunSetupLoadouts();
            StartCoroutine(_setupLoadouts);
        }


        private IEnumerator RunSetupLoadouts()
        {
            //Get our Module List
            Loadouts = new List<LoadoutInfo>();
            int id = 0;
            var loadoutNames = new List<string>();
            var count = _modules.Count;
            for (int i = 0; i < count; ++i)
            {
                var con = _modules[i];
                var loadout = new LoadoutInfo();
                loadout.LoadoutName = con.ConverterName;
                loadout.ModuleId = id;
                loadoutNames.Add(con.ConverterName);
                Loadouts.Add(loadout);
                if (!con.IsActivated || HighLogic.LoadedSceneIsEditor)
                    con.DisableModule();
                id++;
                yield return (null);
            }
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

        private IEnumerator _SetModuleStates;

        public IEnumerator RunModuleStates()
        {
            var count = GetModuleCount();
            for (int i = 0; i < count; ++i)
            {
                var con = GetConverter(i);
                if (HighLogic.LoadedSceneIsEditor && con.enabled)
                    con.DisableModule();
                else if (!con.enabled && EnabledByAnyModule(i))
                    con.EnableModule();
                else if (con.enabled)
                    con.DisableModule();
                yield return (null);
            }
        }

        public void SetModuleStates()
        {
            if(_SetModuleStates != null)
                StopCoroutine(_SetModuleStates);

            _SetModuleStates = RunModuleStates();
            StartCoroutine(_setupLoadouts);
        }
    }
}