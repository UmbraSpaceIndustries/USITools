using Experience;
using KSP.Localization;

namespace USITools
{
    public class AgronomySkill : ExperienceEffect
    {
        public AgronomySkill(ExperienceTrait parent) : base(parent)
        {
        }

        public AgronomySkill(ExperienceTrait parent, float[] modifiers) : base(parent, modifiers)
        {
        }

        protected override float GetDefaultValue()
        {
            return 0f;
        }

        protected override string GetDescription()
        {
            return Localizer.Format("#LOC_USI_Tools_Agronomy");//"Experience in advanced farming and crop diversity"
        }
    }
}