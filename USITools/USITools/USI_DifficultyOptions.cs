using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USITools
{
    namespace KolonyTools
    {
        public class USI_DifficultyOptions : GameParameters.CustomParameterNode
        {
            [GameParameters.CustomParameterUI("Consume Machinery (MKS)", toolTip = "If enabled, machienry will be consumed as part of a converter's processing.", autoPersistance = true)]
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
                    return "Difficulty";
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
