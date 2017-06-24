namespace USITools
{
    public class ModuleWeightDistributableCargo : PartModule
    {
        public void Update()
        {
            if (part.parent != null && part.children.Count == 0)
                return;

            part.CoMOffset = Vector3d.zero;
        }
    }
}
