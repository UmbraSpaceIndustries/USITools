using System.Collections.Generic;
using System.Linq;
using System.Text;
using USITools.Helpers;

namespace USITools
{
    /// <summary>
    /// Responsible for tracking available converter loadouts and the
    ///   costs associated with swapping them. Also responsible for instructing
    ///   an <see cref="ISwappableConverter"/> to apply a loadout to itself.
    /// </summary>
    /// <remarks>
    /// See <see cref="USI_SwappableBay"/> and <see cref="AbstractSwapOption"/>
    ///   for additional information.
    /// </remarks>
    public class USI_SwapController : PartModule
    {
        [KSPField]
        public string ResourceCosts = "";

        [KSPField]
        public string typeName = "Bay";

        public List<ResourceRatio> SwapCosts = new List<ResourceRatio>();
        public List<AbstractSwapOption> Loadouts;

        private List<ISwappableConverter> _converters;

        public override void OnStart(StartState state)
        {
            Loadouts = part.FindModulesImplementing<AbstractSwapOption>();
            _converters = part.FindModulesImplementing<ISwappableConverter>()
                .Where(c => !c.IsStandalone)
                .ToList();

            SetupSwapCosts();
        }

        private void SetupSwapCosts()
        {
            SwapCosts.Clear();

            if (string.IsNullOrEmpty(ResourceCosts))
                return;

            SwapCosts = ResourceHelpers.DeserializeResourceRatios(ResourceCosts);
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
