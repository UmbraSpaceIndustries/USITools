using System;
using System.Collections.Generic;

namespace USITools
{
    public class ModuleResourceConverter_USI : ModuleResourceConverter, IEfficiencyBonusConsumer
    {
        private Dictionary<string, float> _bonusList;

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

        protected override void PostProcess(ConverterResults result, double deltaTime)
        {
            base.PostProcess(result, deltaTime);
            if (result.TimeFactor >= ResourceUtilities.FLOAT_TOLERANCE
                && !status.EndsWith("load"))
            {
                statusPercent = 0d; //Force a reset of the load display.
            }
        }
        public float GetEfficiencyBonus()
        {
            var finBonus = 1f;

            if (HighLogic.LoadedSceneIsFlight)
                finBonus = GetCrewBonus();

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
    }
}