
namespace USITools
{
    public class ModuleAquaticEmitter : PartModule
    {
        //Horrible bloody hack
        [KSPField] 
        private int ParticleDepth = -20;

        private bool splashState = false;


        public void FixedUpdate()
        {
            var eList = part.GetComponentsInChildren<KSPParticleEmitter>();
            //Debug.Log("START:  FOUND " + eList.Count() + " EMMITTERS!");
            
            vessel.checkSplashed();
            var h = vessel.altitude;  //vessel.GetHeightFromTerrain();

            var isSplashed = vessel.Splashed && h < ParticleDepth;
            if (isSplashed != splashState)
            {
                splashState = isSplashed;
                var count = eList.Length;
                for(int i = 0; i < count; ++i)
                {
                    var e = eList[i];
                    e.emit = splashState;
                    e.enabled = splashState;
                }

            }

        }
    }
}