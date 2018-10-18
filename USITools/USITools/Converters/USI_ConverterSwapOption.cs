namespace USITools
{
    public class USI_ConverterSwapOption
        : AbstractSwapOption<USI_ResourceConverter>
    {
        public override void ApplyConverterChanges(USI_ResourceConverter converter)
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
