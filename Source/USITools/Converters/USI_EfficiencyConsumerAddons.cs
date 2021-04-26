using System.Collections.Generic;

namespace USITools
{
    /// <summary>
    /// A converter addon that allows a converter to receive efficiency bonuses.
    /// </summary>
    public class USI_EfficiencyConsumerAddonForConverters : AbstractConverterAddon<USI_Converter>
    {
        public string Tag { get; set; }

        protected Dictionary<string, float> _bonusList { get; private set; } =
            new Dictionary<string, float>();

        public USI_EfficiencyConsumerAddonForConverters(USI_Converter converter) : base(converter)
        {
        }

        public override void PreProcessing()
        {
            base.PreProcessing();
            Converter.EfficiencyBonus = GetEfficiencyBonus();
        }

        public float GetEfficiencyBonus()
        {
            var totalBonus = 1f;

            foreach (var bonus in _bonusList)
            {
                totalBonus *= bonus.Value;
            }

            return totalBonus;
        }

        public bool InCatchupMode()
        {
            var multiplier = Converter.GetEfficiencyMultiplier();
            if (Converter.lastTimeFactor / 2 > multiplier)
            { 
                return true;
            }

            return false;
        }

        public void SetEfficiencyBonus(string name, float value)
        {
            if (!_bonusList.ContainsKey(name))
                _bonusList.Add(name, value);
            else
                _bonusList[name] = value;
        }
    }

    /// <summary>
    /// A converter addon that allows a harvester to receive efficiency bonuses.
    /// </summary>
    public class USI_EfficiencyConsumerAddonForHarvesters : AbstractConverterAddon<USI_Harvester>
    {
        protected Dictionary<string, float> _bonusList { get; private set; } =
            new Dictionary<string, float>();

        public USI_EfficiencyConsumerAddonForHarvesters(USI_Harvester converter) : base(converter)
        {
        }

        public override void PreProcessing()
        {
            base.PreProcessing();
            Converter.EfficiencyBonus = GetEfficiencyBonus();
        }

        public float GetEfficiencyBonus()
        {
            var totalBonus = 1f;

            foreach (var bonus in _bonusList)
            {
                totalBonus *= bonus.Value;
            }

            return totalBonus;
        }

        public void SetEfficiencyBonus(string name, float value)
        {
            if (!_bonusList.ContainsKey(name))
                _bonusList.Add(name, value);
            else
                _bonusList[name] = value;
        }
    }
}
