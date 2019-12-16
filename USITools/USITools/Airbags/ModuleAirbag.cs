using System;
using UnityEngine;

namespace AirbagTools
{
    public class ModuleAirbag : PartModule
    {
        [KSPField]
        public float dampenFactor = .75f;
        [KSPField]
        public float dampenSpeed = 15f;
        [KSPField]
        public String deployAnimationName = "Deploy";

        [KSPField(isPersistant = true)]
        public bool isDeployed = false;


        [KSPAction("Inflate")]
        public void InflateAction(KSPActionParam param)
        {
            InflateAirbags();
        }

        [KSPAction("Deflate")]
        public void DeflateAction(KSPActionParam param)
        {
            DeflateAirbags();
        }

        [KSPAction("Disconnect")]
        public void DisconnectAction(KSPActionParam param)
        {
            DisconnectAirbags();
        }
        public void DisconnectAirbags()
        {
            part.decouple(200f);
        }


        [KSPEvent(guiName = "#LOC_USI_InflateAirbags", guiActiveEditor = true)]//Inflate Airbags
        public void InflateAirbags()
        {
            if (!isDeployed)
            {
                isDeployed = true;
                PlayDeployAnimation(1);
                ToggleEvent("DeflateAirbags", true);
                ToggleEvent("InflateAirbags", false);
            }
        }

        [KSPEvent(guiName = "#LOC_USI_DeflateAirbags", guiActiveEditor = false)]//Deflate Airbags
        public void DeflateAirbags()
        {
            if (isDeployed)
            {
                isDeployed = false;
                PlayDeployAnimation(-.25f);
                ToggleEvent("DeflateAirbags", false);
                ToggleEvent("InflateAirbags", true);
            }
        }

        private void ToggleEvent(string eventName, bool state)
        {
            Events[eventName].active = state;
            Events[eventName].externalToEVAOnly = state;
            Events[eventName].guiActiveEditor = state;
            Events[eventName].guiActive = state;
        }

        public Animation DeployAnimation
        {
            get
            {
                return part.FindModelAnimators(deployAnimationName)[0];
            }
        }


        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            try
            {
                if (part.checkLanded() && isDeployed)
                {
                    Dampen();
                }
                if (part.Landed && isDeployed)
                {
                    Dampen();
                }
            }
            catch (Exception ex)
            {
                print("[AB] Error in OnFixedUpdate - " + ex.Message);
            }
        }

        private void Dampen()
        {
            if (vessel.srfSpeed > dampenSpeed
                || vessel.horizontalSrfSpeed > dampenSpeed)
            {
                var count = vessel.parts.Count;
                for(int i = 0; i < count; ++i)
                {
                    var p = vessel.parts[i];
                    p.Rigidbody.angularVelocity *= dampenFactor;
                    p.Rigidbody.velocity *= dampenFactor;
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            Setup();
        }

        public override void OnStart(StartState state)
        {
            Setup();
        }

        private void Setup()
        {
            print("Deployed: " + isDeployed);
            if (vessel != null)
            {
                part.force_activate();
                if (isDeployed)
                {
                    ToggleEvent("DeflateAirbags", true);
                    ToggleEvent("InflateAirbags", false);
                    PlayDeployAnimation(1000);
                }
                else
                {
                    ToggleEvent("DeflateAirbags", false);
                    ToggleEvent("InflateAirbags", true);
                    PlayDeployAnimation(-1000);
                }
            }
        }

        private void PlayDeployAnimation(float speed)
        {
            DeployAnimation[deployAnimationName].speed = speed;
            if (speed < 0)
            {
                DeployAnimation[deployAnimationName].time = DeployAnimation[deployAnimationName].length;
            }
            DeployAnimation.Play(deployAnimationName);
        }
    }
}
