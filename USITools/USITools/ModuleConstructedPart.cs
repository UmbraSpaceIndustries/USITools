using UnityEngine;

namespace USITools
{
    public class ModuleConstructedPart : PartModule
    {
        private bool isActive;

        public void StartConstruction()
        {
            isActive = true;
        }

        public void StopConstruction()
        {
            isActive = false;
        }


        public void FixedUpdate()
        {
            if(isActive)
                FXMonger.Explode(part,part.transform.position,0f);
        }
    }
}