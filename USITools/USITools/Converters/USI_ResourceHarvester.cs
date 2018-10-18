using System.Collections.Generic;

namespace USITools
{
    public class USI_ResourceHarvester :
        ModuleResourceHarvester,
        IEfficiencyBonusConsumer,
        ISwappableConverter
    {
        #region Fields and properties
        public Dictionary<string, float> BonusList { get; private set; } =
            new Dictionary<string, float>();

        public bool UseEfficiencyBonus
        {
            get
            {
                if (_swapOption != null)
                    return _swapOption.UseBonus;
                else
                    return false;
            }
        }

        private AbstractSwapOption<USI_ResourceHarvester> _swapOption;
        #endregion

        public void Swap(AbstractSwapOption swapOption)
        {
            Swap(swapOption as AbstractSwapOption<USI_ResourceHarvester>);
        }

        public void Swap(AbstractSwapOption<USI_ResourceHarvester> swapOption)
        {
            _swapOption = swapOption;
            _swapOption.ApplyConverterChanges(this);
        }

        public float GetEfficiencyBonus()
        {
            var totalBonus = 1f;
            foreach (var bonus in BonusList)
            {
                totalBonus *= bonus.Value;
            }
            return totalBonus;
        }

        public void SetEfficiencyBonus(string name, float value)
        {
            if (!BonusList.ContainsKey(name))
                BonusList.Add(name, value);
            else
                BonusList[name] = value;
        }

        public override string GetInfo()
        {
            return string.Empty;
        }

        protected override void PreProcessing()
        {
            base.PreProcessing();
            EfficiencyBonus = GetEfficiencyBonus();
        }

        protected override void PostProcess(ConverterResults result, double deltaTime)
        {
            base.PostProcess(result, deltaTime);

            var hasLoad = false;
            if (status != null)
            {
                hasLoad = status.EndsWith("Load");
            }

            if (result.TimeFactor >= ResourceUtilities.FLOAT_TOLERANCE
                && !hasLoad)
            {
                statusPercent = 0d; //Force a reset of the load display.
            }
        }
    }
}
