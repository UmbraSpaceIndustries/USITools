namespace USITools
{
    public class USI_ConverterOptions : GameParameters.CustomParameterNode
    {
        [GameParameters.CustomParameterUI("EVA Required", toolTip = "If enabled, a Kerbal must be on EVA to swap converter recipes.", autoPersistance = true)]
        public bool ConverterSwapRequiresEVA = true;

        [GameParameters.CustomParameterUI("Repair Skill Required", toolTip = "If enabled, a Kerbal with the Repair skill (engineers, mechanics) must be present to swap converter recipes.", autoPersistance = true)]
        public bool ConverterSwapRequiresRepairSkill = true;

        [GameParameters.CustomFloatParameterUI("Cost Multiplier", toolTip = "Set to zero to disable converter swap costs.", autoPersistance = true, minValue = 0f, maxValue = 5f, stepCount = 10)]
        public float ConverterSwapCostMultiplier = 1f;

        public static bool ConverterSwapRequiresRepairSkillEnabled
        {
            get
            {
                var options = HighLogic.CurrentGame.Parameters.CustomParams<USI_ConverterOptions>();

                return options.ConverterSwapRequiresRepairSkill;
            }
        }

        public static bool ConverterSwapRequiresEVAEnabled
        {
            get
            {
                var options = HighLogic.CurrentGame.Parameters.CustomParams<USI_ConverterOptions>();

                return options.ConverterSwapRequiresEVA;
            }
        }

        public static float ConverterSwapCostMultiplierValue
        {
            get
            {
                var options = HighLogic.CurrentGame.Parameters.CustomParams<USI_ConverterOptions>();

                return options.ConverterSwapCostMultiplier;
            }
        }

        public override string Section
        {
            get
            {
                return "Kolonization";
            }
        }

        public override string DisplaySection
        {
            get
            {
                return "Kolonization";
            }
        }

        public override string Title
        {
            get
            {
                return "Swappable Converters";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 1;
            }
        }

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            base.SetDifficultyPreset(preset);
        }

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return false;
            }
        }
    }
}
