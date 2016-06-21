using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using UnityEngine;

namespace USITools
{
    public class USIAnimation : PartModule
    {
        [KSPField] 
        public int CrewCapacity = 0;

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
        public int PrimaryLayer = 2;

        [KSPField]
        public int SecondaryLayer = 3;

        [KSPField] 
        public float inflatedMultiplier = -1;

        [KSPField]
        public bool shedOnInflate = false;

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
                catch (Exception)
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
                if (CheckDeployConditions())
                {
                    PlayDeployAnimation();
                    ToggleEvent("DeployModule", false);
                    ToggleEvent("RetractModule", true);
                    CheckDeployConditions();
                    isDeployed = true;
                }
            }
        }

        private bool CheckDeployConditions()
        {
            if (inflatable)
            {
                if (shedOnInflate && !HighLogic.LoadedSceneIsEditor)
                {
                    for (int i = part.children.Count - 1; i >= 0; i--)
                    {
                        var p = part.children[i];
                        var pNode = p.srfAttachNode;
                        if (pNode.attachedPart == part)
                        {
                            p.decouple(0f);
                        }
                    }
                }

                if (inflatedMultiplier > 0)
                    ExpandResourceCapacity();
                if (CrewCapacity > 0)
                {
                    part.CrewCapacity = CrewCapacity;
                    if (CrewCapacity > 0 & !part.Modules.Contains("TransferDialogSpawner"))
                        part.AddModule("TransferDialogSpawner");
                }
                foreach (var m in part.FindModulesImplementing<ModuleResourceConverter>())
                {
                    m.EnableModule();
                }
                MonoUtilities.RefreshContextWindows(part);
            }
            return true;
        }

        [KSPEvent(guiName = "Retract", guiActive = true, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void RetractModule()
        {
            if (isDeployed)
            {
                if(CheckRetractConditions())
                {
                    isDeployed = false;
                    ReverseDeployAnimation();
                    ToggleEvent("DeployModule", true);
                    ToggleEvent("RetractModule", false);
                }
            }
        }

        private bool CheckRetractConditions()
        {
            var canRetract = true;
            if (inflatable)
            {
                if (part.protoModuleCrew.Count > 0)
                {
                    var msg = string.Format("Unable to deflate {0} as it still contains crew members.", part.partInfo.title);
                    ScreenMessages.PostScreenMessage(msg, 5f, ScreenMessageStyle.UPPER_CENTER);
                    canRetract = false;
                }
                if (canRetract)
                {
                    part.CrewCapacity = 0;
                    if (inflatedMultiplier > 0)
                        CompressResourceCapacity();
                    var modList = GetAffectedMods();
                    foreach (var m in modList)
                    {
                        m.DisableModule();
                    }
                    MonoUtilities.RefreshContextWindows(part);
                }
            }
            return canRetract;
        }

        public List<ModuleResourceConverter> GetAffectedMods()
        {
            var modList = new List<ModuleResourceConverter>();
            var modNames = new List<string> 
                {"ModuleResourceConverter", "ModuleLifeSupportRecycler"};

            for(int i = 0; i < part.Modules.Count; i++)
            {
                if(modNames.Contains(part.Modules[i].moduleName))
                    modList.Add((ModuleResourceConverter)part.Modules[i]);
            }
            return modList;
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
                DeployAnimation[deployAnimationName].layer = PrimaryLayer;
                if (secondaryAnimationName != "")
                {
                    SecondaryAnimation[secondaryAnimationName].layer = SecondaryLayer;
                }
                CheckAnimationState();
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
                CheckDeployConditions();
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
                    if (res.maxAmount < inflatedMultiplier)
                    {
                        double oldMaxAmount = res.maxAmount;
                        res.maxAmount *= inflatedMultiplier;
                        inflatedCost += (float) ((res.maxAmount - oldMaxAmount)*res.info.unitCost);
                    }
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
                    if (res.maxAmount > inflatedMultiplier)
                    {
                        res.maxAmount /= inflatedMultiplier;
                        if (res.amount > res.maxAmount)
                            res.amount = res.maxAmount;
                    }
                }
                inflatedCost = 0.0f;
            }
            catch (Exception ex)
            {
                print("Error in CompressResourceCapacity - " + ex.Message);
            }
        }


        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

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
    }

    public class ModuleAnimationExtended : ModuleAnimateGeneric
    {
        [KSPField]
        public float clampIncrement = 2.5f;

        [KSPField]
        public string menuName = "Clamp";

        [KSPField(isPersistant = true)]
        public bool initialClamp = false;


        [KSPAction("Increase Clamp")]
        public void IncreaseAction(KSPActionParam param)
        {
            deployPercent += clampIncrement;
            if (deployPercent > 100)
                deployPercent = 100;
        }


        [KSPAction("Decrease Clamp")]
        public void DecreaseAction(KSPActionParam param)
        {
            deployPercent -= clampIncrement;
            if (deployPercent < 0)
                deployPercent = 0;
        }

        public override void OnStart(StartState state)
        {
            Actions["IncreaseAction"].guiName = "Increase " + menuName;
            Actions["DecreaseAction"].guiName = "Decrease " + menuName;
            if (initialClamp)
            {
                initialClamp = false;
                deployPercent = 50;
                animTime = .5f;
            }
            base.OnStart(state);
        }
    }
}



