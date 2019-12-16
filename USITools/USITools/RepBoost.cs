using Experience;
using KSP.Localization;

namespace USITools
{
    public class RepBoost : ExperienceEffect
    {
        public RepBoost(ExperienceTrait parent) : base(parent)
        {
        }

        public RepBoost(ExperienceTrait parent, float[] modifiers) : base(parent, modifiers)
        {
        }

        protected override float GetDefaultValue()
        {
            return 0f;
        }

        protected override string GetDescription()
        {
            return Localizer.Format("#LOC_USI_Tools_Rep");//"A Kolonist that increases reputation"
        }
    }
}