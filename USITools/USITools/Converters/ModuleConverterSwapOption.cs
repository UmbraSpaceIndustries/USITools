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
    }
}
