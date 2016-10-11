namespace USITools
{
    public class ModuleAnimationExtended : ModuleAnimateGeneric
    {
        [KSPField]
        public float clampIncrement = 2.5f;

        [KSPField]
        public string menuName = "Clamp";

        [KSPField(isPersistant = true)]
        public bool initialClamp = false;


        [KSPAction("Increase Clamp")]
        public void IncreaseAction(KSPActionParam param)
        {
            deployPercent += clampIncrement;
            if (deployPercent > 100)
                deployPercent = 100;
        }


        [KSPAction("Decrease Clamp")]
        public void DecreaseAction(KSPActionParam param)
        {
            deployPercent -= clampIncrement;
            if (deployPercent < 0)
                deployPercent = 0;
        }

        public override void OnStart(StartState state)
        {
            Actions["IncreaseAction"].guiName = "Increase " + menuName;
            Actions["DecreaseAction"].guiName = "Decrease " + menuName;
            if (initialClamp)
            {
                initialClamp = false;
                deployPercent = 50;
                animTime = .5f;
            }
            base.OnStart(state);
        }
    }
}