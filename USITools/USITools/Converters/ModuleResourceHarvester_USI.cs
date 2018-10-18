using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace USITools
{
    [Obsolete("Use USI_ResourceHarvester instead.")]
    public class ModuleResourceHarvester_USI : PartModule
    {
        [KSPField(isPersistant = true)]
        public bool IsActivated;

        [KSPField(isPersistant = true)]
        public bool hasBeenUpdated;

        private ModuleResourceHarvester_USI _presidingInstance;

        public List<bool> PreviouslyActiveList { get; private set; } = new List<bool>();

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            // Determine which recipe each harvester was previously resposible for,
            //  find which bay(s) are currently responsible for that recipe and
            //  activate the corresponding harvester(s) if they were previously active.
            if (_presidingInstance == this && !hasBeenUpdated)
            {
                var swapOptionCount = part.FindModulesImplementing<AbstractSwapOption>().Count;
                var harvesters = part.FindModulesImplementing<USI_ResourceHarvester>();
                var bays = part.FindModulesImplementing<USI_SwappableBay>();
                var controller = part.FindModuleImplementing<USI_SwapController>();

                if (controller == null)
                    Debug.LogError(string.Format("[USI] {0}: Trying to import old loadout(s) into a part with no USI_SwapController. Check the part config file.", GetType().Name));
                else if (swapOptionCount != PreviouslyActiveList.Count)
                    Debug.LogError(string.Format("[USI] {0}: Trying to import {1} old loadout(s) into a part with {2} swap option(s). Check the part config file.", GetType().Name, PreviouslyActiveList.Count, swapOptionCount));
                else
                {
                    // i will correspond to the loadout index
                    for (int i = 0; i < PreviouslyActiveList.Count; i++)
                    {
                        bool wasActive = PreviouslyActiveList[i];
                        if (wasActive)
                        {
                            // Determine which bays (if any) are currently configured for this loadout
                            var configuredBays = bays.Where(b => b.currentLoadout == i);
                            if (configuredBays.Any())
                            {
                                var bayIndexes = configuredBays.Select(b => b.moduleIndex);
                                foreach (var index in bayIndexes)
                                {
                                    var harvester = harvesters[index];
                                    harvester.isEnabled = true;
                                    harvester.IsActivated = true;
                                }
                            }
                        }
                    }

                    hasBeenUpdated = true;
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            // The order these nodes are loaded by the game should match the loadout order
            //  of the swap options. So we should be able to use this to determine
            //  which recipes were active previously and thus which converters to re-activate.
            if (_presidingInstance == null)
            {
                // If there are other ModuleResourceHarvester_USI instances on this part,
                //  see if any of them have a PreviouslyActiveList started yet. If so, then
                //  let that instance handle the remainder of this process. Otherwise, make this instance
                //  the presiding instance.
                var otherInstances = part.FindModulesImplementing<ModuleResourceHarvester_USI>();
                if (otherInstances != null)
                {
                    var candidate = otherInstances.Where(i => i.PreviouslyActiveList.Count > 0).FirstOrDefault();
                    _presidingInstance = candidate ?? this;
                }
                else
                    _presidingInstance = this;
            }

            _presidingInstance.PreviouslyActiveList.Add(IsActivated && isEnabled);
        }

        public override void OnSave(ConfigNode node)
        {
            if (_presidingInstance != null)
                hasBeenUpdated = _presidingInstance.hasBeenUpdated;

            base.OnSave(node);
        }
    }
}
