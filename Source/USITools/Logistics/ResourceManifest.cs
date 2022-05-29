using System.Collections.Generic;

namespace USITools.Logistics
{
    public class ResourceManifest
    {
        public bool AllowWarehouses { get; set; }
        public float WarehouseRange { get; set; }
        public bool IncludeSelf { get; set; }
        public Vessel TargetVessel { get; set; }
        public List<ResourceRatio> Resources{ get; private set; }
        public ResourceManifest()
        {
            Resources = new List<ResourceRatio>();
        }
    }

    public class CrewManifest
    {
        public Vessel TargertVessel { get; set; }
        public List<ProtoCrewMember> Crew { get; private set; }
        public CrewManifest()
        {
            Crew = new List<ProtoCrewMember>();
        }
    }

    public static class ManifestUtilities
    {
        public static bool VesselHasResources(ResourceManifest manifest)
        {
            //Given a vessel, determine if they have the resources on our manifest
            return true;
        }

        public static void ConsumeResources(ResourceManifest manifest)
        {
            //Given a vessel, consume the resources on our manifest
        }

        public static void TransferResources(ResourceManifest sourceManifest, ResourceManifest destinationManifest)
        {
            //Remove the source manifest from the source vessel, and add the target manifest to the destination vessel.
        }

        public static bool VesselCanHoldResources(ResourceManifest manifest)
        {
            //Given a vessel, determine if it can store the manifest resources
            return true;
        }

        public static bool VesselCanHoldCrew(CrewManifest manifest)
        {
            //Given a vessel, determine if it can store the manifest crew
            return true;
        }

        public static void TransferCrew(CrewManifest sourceManifest, CrewManifest destinationManifest)
        {
            //Remove the crew from the source vessel, and add the crew to the destination vessel.
        }
    }
}
