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
            UseBonus = false;  // efficiency parts should not use bonuses from other efficiency parts!
            converter.eMultiplier = eMultiplier;
            converter.eTag = eTag;

            base.ApplyConverterChanges(converter);
        }

        public override void PostProcess(ModuleResourceConverter_USI converter, ConverterResults result, double deltaTime)
        {
            base.PostProcess(result, deltaTime);
            converter.EfficiencyMultiplier = result.TimeFactor / deltaTime;
        }
    }
}
