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

        public List<ResourceRatio> ResourceCostRatios = new List<ResourceRatio>();
        public List<AbstractSwapOption> Loadouts;

        private List<ISwappableConverter> _converters;
        private double lastCheck;
        private double checkInterval = 5d;

        public override void OnStart(StartState state)
        {
            Loadouts = part.FindModulesImplementing<AbstractSwapOption>();
            _converters = part.FindModulesImplementing<ISwappableConverter>();
            SetupResourceCosts();
        }

        private void SetupResourceCosts()
        {
            ResourceCostRatios.Clear();

            if (string.IsNullOrEmpty(ResourceCosts))
                return;

            var resources = ResourceCosts.Split(',');
            for (int i = 0; i < resources.Length; i += 2)
            {
                ResourceCostRatios.Add(new ResourceRatio
                {
                    ResourceName = resources[i],
                    Ratio = double.Parse(resources[i + 1])
                });
            }
        }

        public override string GetInfo()
        {
            if (string.IsNullOrEmpty(ResourceCosts))
                return string.Empty;

            var output = new StringBuilder("Resource Cost:\n\n");
            var resources = ResourceCosts.Split(',');
            for (int i = 0; i < resources.Length; i += 2)
            {
                output.Append(string.Format("{0} {1}\n", double.Parse(resources[i + 1]), resources[i]));
            }
            return output.ToString();
        }

        public void ApplyLoadout(int loadoutIndex, int converterIndex)
        {
            var loadout = Loadouts[loadoutIndex];
            var converter = _converters[converterIndex];

            converter.Swap(loadout);
        }
    }
}
