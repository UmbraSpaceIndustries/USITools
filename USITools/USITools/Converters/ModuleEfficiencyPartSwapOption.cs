namespace USITools
{
    public class ModuleEfficiencyPartSwapOption : ModuleConverterSwapOption
    {
        [KSPField]
        public double eMultiplier = 1d;

        [KSPField]
        public string eTag = "";

        public override void ApplyConverterChanges(ModuleResourceConverter_USI converter)
        {
            converter.eMultiplier = eMultiplier;
            converter.eTag = eTag;

            base.ApplyConverterChanges(converter);
        }
    }
}
