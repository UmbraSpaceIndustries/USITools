using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace USITools
{
    public class ModuleResourceHarvester_USI : ModuleResourceHarvester, IEfficiencyBonusConsumer
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
            var finBonus = 1f; //Harvesters already have a Crew Bonus
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