namespace USITools
{
    public class USI_EfficiencyBoosterAddon : AbstractConverterAddon<USI_Converter>
    {
        public double Multiplier = 1d;
        public string Tag = "";

        private double _efficiencyMultiplier;
        public double EfficiencyMultiplier
        {
            get
            {
                if (HighLogic.LoadedSceneIsEditor)
                    return _efficiencyMultiplier;

                if (!IsActive)
                    _efficiencyMultiplier = 0d;

                return _efficiencyMultiplier;
            }
            set
            {
                _efficiencyMultiplier = value;
            }
        }

        public USI_EfficiencyBoosterAddon(USI_Converter converter) : base(converter) { }

        public override void PostProcess(ConverterResults result, double deltaTime)
        {
            base.PostProcess(result, deltaTime);
            EfficiencyMultiplier = result.TimeFactor / deltaTime;
        }
    }
}
