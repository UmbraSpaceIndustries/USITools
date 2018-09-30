using USITools.KolonyTools;

namespace USITools
{
    public class ModuleConverterSwapOption
        : AbstractSwapOption<ModuleResourceConverter_USI>
    {
        public override void ApplyConverterChanges(ModuleResourceConverter_USI converter)
        {
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
                for (int o = recipe.Outputs.Count; o-- > 0;)
                {
                    var output = recipe.Outputs[o];
                    if (output.ResourceName == "Recyclables")
                        recipe.Inputs.Remove(output);
                }
            }

            return recipe;
        }
    }
}
