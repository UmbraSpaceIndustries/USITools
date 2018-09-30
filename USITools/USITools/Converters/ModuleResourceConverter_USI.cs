using System.Collections.Generic;
using USITools.KolonyTools;

namespace USITools
{
    public class ModuleResourceConverter_USI :
        ModuleResourceConverter,
        IEfficiencyBonusConsumer,
        ISwappableConverter
    {
        #region Fields and properties
        [KSPField]
        public double eMultiplier = 1d;

        [KSPField]
        public string eTag = "";

        public Dictionary<string, float> BonusList { get; private set; } =
            new Dictionary<string, float>();

        public float Governor = 1.0f;
        private double _efficiencyMultiplier;
        public double EfficiencyMultiplier
        {
            get
            {
                if (HighLogic.LoadedSceneIsEditor)
                    return _efficiencyMultiplier * Governor;
                if (!IsActivated)
                    _efficiencyMultiplier = 0d;
                return _efficiencyMultiplier * Governor;
            }
            set
            {
                _efficiencyMultiplier = value;
            }
        }

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

        private AbstractSwapOption<ModuleResourceConverter_USI> _swapOption;
        #endregion

        public void Swap(AbstractSwapOption swapOption)
        {
            Swap(swapOption as AbstractSwapOption<ModuleResourceConverter_USI>);
        }

        public void Swap(AbstractSwapOption<ModuleResourceConverter_USI> swapOption)
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

        protected override void PreProcessing()
        {
            base.PreProcessing();
            EfficiencyBonus = GetEfficiencyBonus();
        }

        protected override ConversionRecipe PrepareRecipe(double deltatime)
        {
            var recipe = base.PrepareRecipe(deltatime);
            if (!USI_DifficultyOptions.ConsumeMachineryEnabled && recipe != null)
            {
                for (int i = recipe.Inputs.Count; i-- > 0;)
                {
                    var input = recipe.Inputs[i];
                    if (input.ResourceName == "Machinery")
                        recipe.Inputs.Remove(input);
                }
                for (int output = recipe.Outputs.Count; output-- > 0;)
                {
                    var op = recipe.Outputs[output];
                    if (op.ResourceName == "Recyclables")
                        recipe.Inputs.Remove(op);
                }
            }
            return recipe;
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

            if (_swapOption != null)
            {
                _swapOption.PostProcess(this, result, deltaTime);
            }
        }

        public override string GetInfo()
        {
            if (string.IsNullOrEmpty(eTag))
                return base.GetInfo();
            var resourceConsumption = base.GetInfo();
            int index = resourceConsumption.IndexOf("\n"); // Strip the first line containing the etag
            resourceConsumption = resourceConsumption.Substring(index + 1);
            return "Boosts efficiency of converters benefiting from a " + eTag + "\n\n" +
                "Boost power: " + eMultiplier.ToString() + resourceConsumption;
        }

        public override string GetModuleDisplayName()
        {
            return GetType().Name;
        }
    }
}
