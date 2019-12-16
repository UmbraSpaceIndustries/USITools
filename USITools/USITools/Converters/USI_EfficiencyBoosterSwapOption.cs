using KSP.Localization;
namespace USITools
{
    /// <summary>
    /// A converter loadout that boosts the efficiency of other converters.
    /// </summary>
    public class USI_EfficiencyBoosterSwapOption : USI_ConverterSwapOption
    {
        [KSPField]
        public double EfficiencyMultiplier = 1d;

        public override void ApplyConverterChanges(USI_Converter converter)
        {
            // Efficiency boosters can't also be efficiency consumers, for now.
            UseEfficiencyBonus = false;

            converter.Addons.Add(new USI_EfficiencyBoosterAddon(converter)
            {
                Multiplier = EfficiencyMultiplier,
                Tag = EfficiencyTag
            });

            base.ApplyConverterChanges(converter);
        }

        public override string GetInfo()
        {
            if (string.IsNullOrEmpty(EfficiencyTag))
                return base.GetInfo();
            var resourceConsumption = base.GetInfo();
            int index = resourceConsumption.IndexOf("\n"); // Strip the first line containing the etag
            resourceConsumption = resourceConsumption.Substring(index + 1);
            return Localizer.Format("#LOC_USI_Tools_EBSO_Info", EfficiencyTag,EfficiencyMultiplier.ToString() + resourceConsumption);//"Boosts efficiency of converters benefiting from a " +  + "\n\n" + "Boost power: " + 
        }
    }
}
