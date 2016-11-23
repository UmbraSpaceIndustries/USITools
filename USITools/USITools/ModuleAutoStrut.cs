namespace USITools
{
    public class ModuleAutoStrut : PartModule
    {
        //Quick and hacky
        public override void OnStart(StartState state)
        {
            part.autoStrutMode = Part.AutoStrutMode.ForceRoot;
            part.UpdateAutoStrut();
        }
    }
}