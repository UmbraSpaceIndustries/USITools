using System;

namespace USITools
{
    public class USI_HarvesterSwapOption
        : AbstractSwapOption<USI_Harvester>
    {
        [KSPField]
        public string ResourceName = "";

        [KSPField]
        public float Efficiency = 1;

        /// <summary>
        /// Set this to <c>false</c> to ignore efficiency bonuses.
        /// </summary>
        [KSPField]
        public bool UseEfficiencyBonus = true;

        public override void ApplyConverterChanges(USI_Harvester converter)
        {
            converter.Efficiency = Efficiency;
            converter.ResourceName = ResourceName;

            // Setup efficiency bonus consumption
            if (UseEfficiencyBonus)
            {
                converter.Addons.Add(new USI_EfficiencyConsumerAddonForHarvesters(converter));
            }

            base.ApplyConverterChanges(converter);
        }
    }
}
