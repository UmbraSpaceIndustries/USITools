using System;

namespace USITools
{
    [Obsolete("Use a class derived from AbstractSwapOption instead.")]
    public class ModuleSwapOption : USI_HarvesterSwapOption { }

    public class USI_HarvesterSwapOption
        : AbstractSwapOption<USI_ResourceHarvester>
    {
        public override void ApplyConverterChanges(USI_ResourceHarvester converter)
        {
            converter.Efficiency = Efficiency;
            converter.ResourceName = ResourceName;

            base.ApplyConverterChanges(converter);
        }
    }
}
