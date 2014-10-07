namespace USI
{
    public class ResourceRatio
    {
        public PartResourceDefinition resource;
        public double ratio;
        public bool allowExtra;

        public ResourceRatio(PartResourceDefinition resource, double ratio, bool allowExtra = false)
        {
            this.resource = resource;
            this.ratio = ratio;
            this.allowExtra = allowExtra;
        }
    }
}