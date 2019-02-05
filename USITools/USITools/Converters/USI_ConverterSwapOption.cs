using USITools.KolonyTools;

namespace USITools
{
    /// <summary>
    /// A basic converter loadout, with the option to consume efficiency bonuses.
    /// </summary>
    public class USI_ConverterSwapOption
        : AbstractSwapOption<USI_Converter>
    {
        /// <summary>
        /// Set this to <c>false</c> to ignore efficiency bonuses.
        /// </summary>
        [KSPField]
        public bool UseEfficiencyBonus = true;

        [KSPField]
        public string EfficiencyTag;

        public override void ApplyConverterChanges(USI_Converter converter)
        {
            // Setup the conversion recipe
            converter.inputList.Clear();
            converter.outputList.Clear();
            converter.reqList.Clear();

            converter.Recipe.Inputs.Clear();
            converter.Recipe.Outputs.Clear();
            converter.Recipe.Requirements.Clear();

            converter.inputList.AddRange(inputList);
            converter.outputList.AddRange(outputList);
            converter.reqList.AddRange(reqList);

            converter.Recipe.Inputs.AddRange(inputList);
            converter.Recipe.Outputs.AddRange(outputList);
            converter.Recipe.Requirements.AddRange(reqList);

            // Setup efficiency bonus consumption
            if (UseEfficiencyBonus)
            {
                converter.Addons.Add(new USI_EfficiencyConsumerAddonForConverters(converter)
                {
                    Tag = EfficiencyTag
                });
            }

            base.ApplyConverterChanges(converter);
        }

        public override ConversionRecipe PrepareRecipe(ConversionRecipe recipe)
        {
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
    }
}
