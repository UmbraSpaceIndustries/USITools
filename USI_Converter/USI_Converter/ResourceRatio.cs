namespace USI
{
    public class ResourceRatio
    {
        public bool allowExtra;
        public double ratio;
        public PartResourceDefinition resource;

        public ResourceRatio(PartResourceDefinition resource, double ratio, bool allowExtra = false)
        {
            this.resource = resource;
            this.ratio = ratio;
            this.allowExtra = allowExtra;
        }
    }
}