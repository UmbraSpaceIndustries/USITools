using System;
using System.Collections.Generic;
using USITools.KolonyTools;

namespace USITools
{
    public class ModuleResourceConverter_USI : ModuleResourceConverter, IEfficiencyBonusConsumer
    {
        private Dictionary<string, float> _bonusList;

        [KSPField]
        public bool UseBonus = true;

        public Dictionary<string, float> BonusList
        {
            get
            {
                if (_bonusList == null)
                    _bonusList = new Dictionary<string, float>();
                return _bonusList;
            }
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
                var iCount = recipe.Inputs.Count;
                var oCount = recipe.Outputs.Count;
                for (int i = iCount; i-- > 0;)
                {
                    var ip = recipe.Inputs[i];
                    if (ip.ResourceName == "Machinery")
                        recipe.Inputs.Remove(ip);
                }
                for (int o = oCount; o-- > 0;)
                {
                    var op = recipe.Outputs[o];
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
        }
        public float GetEfficiencyBonus()
        {
            var finBonus = 1f;
            foreach (var b in BonusList)
            {
                finBonus *= b.Value;
            }
            return finBonus;
        }

        public void SetEfficiencyBonus(string bonName, float bonVal)
        {
            if (!BonusList.ContainsKey(bonName))
                BonusList.Add(bonName, bonVal);
            else
                BonusList[bonName] = bonVal;
        }

        public bool useEfficiencyBonus => UseBonus;

        public override string GetModuleDisplayName()
        {
            return GetType().Name;
        }

    }
}