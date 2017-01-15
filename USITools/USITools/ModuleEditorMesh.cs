namespace USITools
{
    public class ModuleEditorMesh : PartModule
    {
        [KSPField]
        public string ObjectName;

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsEditor)
                return;

            var t = part.FindModelTransform(ObjectName);
            if (t != null)
            {
                t.gameObject.SetActive(false);
            }
        }
    }
}
