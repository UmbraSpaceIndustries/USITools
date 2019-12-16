using KSP.Localization;
namespace USITools
{
    public class USI_ConverterOptions : GameParameters.CustomParameterNode
    {
        [GameParameters.CustomParameterUI("#LOC_USI_ConverterSwapRequiresEVA", toolTip = "#LOC_USI_ConverterSwapRequiresEVA_desc", autoPersistance = true)]//EVA Required//If enabled, a Kerbal must be on EVA to swap converter recipes.
        public bool ConverterSwapRequiresEVA = true;

        [GameParameters.CustomParameterUI("#LOC_USI_ConverterSwapRequiresRepairSkill", toolTip = "#LOC_USI_ConverterSwapRequiresRepairSkill_desc", autoPersistance = true)]//Repair Skill Required//If enabled, a Kerbal with the Repair skill (engineers, mechanics) must be present to swap converter recipes.
        public bool ConverterSwapRequiresRepairSkill = true;

        [GameParameters.CustomFloatParameterUI("#LOC_USI_ConverterSwapCostMultiplier", toolTip = "#LOC_USI_ConverterSwapCostMultiplier_desc", autoPersistance = true, minValue = 0f, maxValue = 5f, stepCount = 10)]//Cost Multiplier//Set to zero to disable converter swap costs.
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
                return Localizer.Format("#LOC_USI_ConverterSwapTitle");//"Swappable Converters"
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
