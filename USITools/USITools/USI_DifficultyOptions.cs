using KSP.Localization;
namespace USITools
{
    namespace KolonyTools
    {
        public class USI_DifficultyOptions : GameParameters.CustomParameterNode
        {
            [GameParameters.CustomParameterUI("#LOC_USI_ConsumeMachinery", toolTip = "#LOC_USI_ConsumeMachinery_desc", autoPersistance = true)]//Consume Machinery (MKS)If enabled, machinery will be consumed as part of a converter's processing.
            public bool ConsumeMachinery = true;

            public static bool ConsumeMachineryEnabled
            {
                get
                {
                    USI_DifficultyOptions options = HighLogic.CurrentGame.Parameters.CustomParams<USI_DifficultyOptions>();
                    return options.ConsumeMachinery;
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
                    return Localizer.Format("#LOC_USI_OptionsTitle");//"Difficulty"
                }
            }

            public override int SectionOrder
            {
                get
                {
                    return 0;
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
}
