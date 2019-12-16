using System;
using UnityEngine;

namespace USITools
{
    class USI_BaseAnchor : PartModule
    {
        [KSPField]
        public float anchorWeight = 1000f;
        
        [KSPField]
        public float offset = 10f;

        [KSPField(isPersistant = true)]
        public bool isActive = false;


        [KSPEvent(guiName = "#LOC_USI_DropAnchor", guiActive = true, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]//Drop Anchor
        public void DropAnchor()
        {
            isActive = true;
            ToggleEvent("DropAnchor", false);
            ToggleEvent("RaiseAnchor", true);

            if (part.checkLanded())
            {
                AnchorModule();
            }
            if (part.Landed)
            {
                AnchorModule();
            }
        }

        [KSPEvent(guiName = "#LOC_USI_RaiseAnchor", guiActive = true, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]//Raise Anchor
        public void RaiseAnchor()
        {
            isActive = false;
            ToggleEvent("DropAnchor", true);
            ToggleEvent("RaiseAnchor", false);
            UnanchorModule();
        }

        private void ToggleEvent(string eventName, bool state)
        {
            Events[eventName].active = state;
            Events[eventName].externalToEVAOnly = state;
            Events[eventName].guiActive = state;
        }

        public override void OnStart(StartState state)
        {
            try
            {
                part.force_activate();
                if (isActive)
                {
                    ToggleEvent("DropAnchor", false);
                    ToggleEvent("RaiseAnchor", true);
                }
                else
                {
                    ToggleEvent("DropAnchor", true);
                    ToggleEvent("RaiseAnchor", false);
                }
            }
            catch (Exception ex)
            {
                print("ERROR in Anchor OnStart - " + ex.Message);
            }
        }

        private void UnanchorModule()
        {
            part.mass -= anchorWeight;
            if (part.mass < 0) part.mass = 0.0001f;
            part.CoMOffset = new Vector3(0, 0, 0);
            var count = vessel.parts.Count;
            for (int i = 0; i < count; ++i)
            {
                vessel.parts[i].GetComponent<Rigidbody>().isKinematic = false;
            }

        }

        private void AnchorModule()
        {
            if (part.mass <= anchorWeight)
            {
                part.mass += anchorWeight;
            }
            part.CoMOffset = new Vector3(0,offset,0);
            var count = vessel.parts.Count;
            for (int i = 0; i < count; ++i)
            {
                var p = vessel.parts[i];
                if(p != part)
                    p.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }
}