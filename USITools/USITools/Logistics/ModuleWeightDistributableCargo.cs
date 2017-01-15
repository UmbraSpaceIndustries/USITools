namespace USITools
{
    public class ModuleWeightDistributableCargo : PartModule
    {
        public void Update()
        {
            if (part.parent != null && part.children.Count == 0)
                return;
            
            if (part.physicalSignificance == Part.PhysicalSignificance.NONE)
                part.physicalSignificance = Part.PhysicalSignificance.FULL;
        }
    }
}