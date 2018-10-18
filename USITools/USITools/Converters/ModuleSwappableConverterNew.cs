using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace USITools
{
    [Obsolete("Use ModuleSwappableBay instead.")]
    public class ModuleSwappableConverterNew : PartModule
    {
        [KSPField]
        public string bayName = "";

        [KSPField(isPersistant = true)]
        public int currentLoadout = 0;

        [KSPField(isPersistant = true)]
        public bool hasBeenUpdated;

        private List<int> oldLoadouts = new List<int>();

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            // Import loadouts from the old ModuleSwappableConverterNew class into the new USI_SwappableBay class.
            if (!hasBeenUpdated)
            {
                var bays = part.FindModulesImplementing<USI_SwappableBay>();
                var controller = part.FindModuleImplementing<USI_SwapController>();

                if (controller == null)
                    Debug.LogError("[USI] ModuleSwappableConverter(New): Trying to import old loadout(s) into a part with no USI_SwapController. Check the part config file.");
                else if (bays.Count < oldLoadouts.Count)
                    Debug.LogError(string.Format("[USI] ModuleSwappableConverter(New): Trying to import {0} old loadout(s) into a part with {1} bay(s). Check the part config file.", oldLoadouts.Count, bays.Count));
                else
                {
                    for (int i = 0; i < oldLoadouts.Count; i++)
                    {
                        var bay = bays.Where(b => b.moduleIndex == i).FirstOrDefault();
                        if (bay != null)
                        {
                            bay.currentLoadout = oldLoadouts[i];
                            controller.ApplyLoadout(bay.currentLoadout, i);
                        }
                    }

                    hasBeenUpdated = true;
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            oldLoadouts.Add(currentLoadout);
        }
    }
}
