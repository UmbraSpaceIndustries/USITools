using Experience;
using KSP.Localization;

namespace USITools
{
    public class BotanySkill : ExperienceEffect
    {
        public BotanySkill(ExperienceTrait parent) : base(parent)
        {
        }

        public BotanySkill(ExperienceTrait parent, float[] modifiers) : base(parent, modifiers)
        {
        }

        protected override float GetDefaultValue()
        {
            return 0f;
        }

        protected override string GetDescription()
        {
            return Localizer.Format("#LOC_USI_Tools_Botany");//"Experience managing basic greenhouses"
        }
    }
}