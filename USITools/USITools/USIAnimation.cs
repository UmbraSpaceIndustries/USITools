using System;
using System.Linq;
using UnityEngine;

namespace USITools
{
    public class USIAnimation : PartModule, IPartCostModifier
    {
        [KSPField]
        public string deployAnimationName = "Deploy";

        [KSPField]
        public string secondaryAnimationName = "";

        [KSPField(isPersistant = true)]
        public bool isDeployed = false;

        [KSPField(isPersistant = true)]
        public float inflatedCost = 0;

        [KSPField]
        public bool inflatable = false;

        [KSPField] 
        public float inflatedMultiplier = -1;

        [KSPAction("Deploy Module")]
        public void DeployAction(KSPActionParam param)
        {
            DeployModule();
        }


        [KSPAction("Retract Module")]
        public void RetractAction(KSPActionParam param)
        {
            RetractModule();
        }


        [KSPAction("Toggle Module")]
        public void ToggleScoopAction(KSPActionParam param)
        {
            if (isDeployed)
            {
                RetractModule();
            }
            else
            {
                DeployModule();
            }
        }

        public Animation DeployAnimation
        {
            get
            {
                return part.FindModelAnimators(deployAnimationName)[0];
            }
        }

        public Animation SecondaryAnimation
        {
            get
            {
                try
                {
                    return part.FindModelAnimators(secondaryAnimationName)[0]; 
                }
                catch (Exception ex)
                {
                    print("[OKS] Could not find secondary animation - " + secondaryAnimationName);
                    return null;
                }
            }
        }

        [KSPEvent(guiName = "Deploy", guiActive = true, externalToEVAOnly = true, guiActiveEditor = true, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void DeployModule()
        {
            if (!isDeployed)
            {
                PlayDeployAnimation();
                ToggleEvent("DeployModule", false);
                ToggleEvent("RetractModule", true);
                if (inflatable && inflatedMultiplier > 0)
                {
                    ExpandResourceCapacity();
                }
                isDeployed = true;
            }
        }

        [KSPEvent(guiName = "Retract", guiActive = true, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void RetractModule()
        {
            if (isDeployed)
            {
                isDeployed = false;
                ReverseDeployAnimation();
                ToggleEvent("DeployModule", true);
                ToggleEvent("RetractModule", false);
                if (inflatable && inflatedMultiplier > 0)
                {
                    CompressResourceCapacity();
                }
            }
        }

        private void PlayDeployAnimation(int speed = 1)
        {
            DeployAnimation[deployAnimationName].speed = speed;
            DeployAnimation.Play(deployAnimationName);
        }

        public void ReverseDeployAnimation(int speed = -1)
        {
            if (secondaryAnimationName != "")
            {
                SecondaryAnimation.Stop(secondaryAnimationName);
            }
            DeployAnimation[deployAnimationName].time = DeployAnimation[deployAnimationName].length;
            DeployAnimation[deployAnimationName].speed = speed;
            DeployAnimation.Play(deployAnimationName);
        }

        private void ToggleEvent(string eventName, bool state)
        {
            Events[eventName].active = state;
            Events[eventName].externalToEVAOnly = state;
            Events[eventName].guiActive = state;
            Events[eventName].guiActiveEditor = state;
            if (inflatedMultiplier > 0)
            {
                Events[eventName].guiActiveEditor = false;
            }

        }

        public override void OnStart(StartState state)
        {
            try
            {
                DeployAnimation[deployAnimationName].layer = 2;
                if (secondaryAnimationName != "")
                {
                    SecondaryAnimation[secondaryAnimationName].layer = 3;
                }
                CheckAnimationState();
                base.OnStart(state);
            }
            catch (Exception ex)
            {
                print("ERROR IN USIAnimationOnStart - " + ex.Message);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            try
            {
                CheckAnimationState();
            }
            catch (Exception ex)
            {
                print("ERROR IN USIAnimationOnLoad - " + ex.Message);
            }
        }

        private void CheckAnimationState()
        {
            if (isDeployed)
            {
                ToggleEvent("DeployModule", false);
                ToggleEvent("RetractModule", true);
                PlayDeployAnimation(1000);
            }
            else
            {
                ToggleEvent("DeployModule", true);
                ToggleEvent("RetractModule", false); 
                ReverseDeployAnimation(-1000);
            }
        }



        private void ExpandResourceCapacity()
        {
            try
            {
                foreach (var res in part.Resources.list)
                {
                    double oldMaxAmount = res.maxAmount;
                    res.maxAmount *= inflatedMultiplier;
                    inflatedCost += (float)((res.maxAmount - oldMaxAmount) * res.info.unitCost);
                }
            }
            catch (Exception ex)
            {
                print("Error in ExpandResourceCapacity - " + ex.Message);
            }
        }

        private void CompressResourceCapacity()
        {
            try
            {
                foreach (var res in part.Resources.list)
                {
                    res.maxAmount /= inflatedMultiplier;
                    if (res.amount > res.maxAmount)
                        res.amount = res.maxAmount;
                }
                inflatedCost = 0.0f;
            }
            catch (Exception ex)
            {
                print("Error in CompressResourceCapacity - " + ex.Message);
            }
        }


        public override void OnUpdate()
        {
            if (vessel != null)
            {
                if (isDeployed && secondaryAnimationName != "")
                {
                    try
                    {
                        if (!SecondaryAnimation.isPlaying && !DeployAnimation.isPlaying)
                        {
                            SecondaryAnimation[secondaryAnimationName].speed = 1;
                            SecondaryAnimation.Play(secondaryAnimationName);
                        }
                    }
                    catch (Exception ex)
                    {
                        print("Error in OnUpdate - USI Animation - " + ex.Message);
                    }
                }
            }
            base.OnUpdate();
        }

        float IPartCostModifier.GetModuleCost(float defaultCost)
        {
            return inflatedCost;
        }
    }
}
