using System;
using UnityEngine;

namespace LifeBoat
{
    public class LifeBoat : PartModule
    {
        [KSPField]
        public float dampenFactor = .75f;
        [KSPField]
        public float dampenSpeed = 15f;

        [KSPField]
        public string deployAnimationName = "InflatePod";

        [KSPField(isPersistant = true)]
        public bool isDeployed = false;

        [KSPField(isPersistant = true)]
        public bool isUnsealed = false;

        public Animation DeployAnimation
        {
            get
            {
                return part.FindModelAnimators(deployAnimationName)[0];
            }
        }

        [KSPAction("Evacuate")]
        public void EvacuateShip(KSPActionParam param)
        {
            TryToEvacuate(true);
        }

        private void TryToEvacuate(bool firstTry)
        {
            try
            {
                if (vessel.GetCrewCount() > 0)
                {
                    if (!isDeployed)
                    {
                        InflateLifeboat();
                    }
                    if (part.CrewCapacity == 1 && !(part.protoModuleCrew.Count > 0))
                    {
                        print("Evacuating!");
                        Part source = null;
                        var count = vessel.parts.Count;
                        for (int i = 0; i < count; ++i)
                        {
                            var p = vessel.parts[i];
							if(p != part)
								continue;
								
                            if (!p.Modules.Contains("LifeBoat") && p.protoModuleCrew.Count > 0)
                            {
                                source = p;
                                break;
                            }
                        }

                        if (source != null)
                        {
                            print("Attempting to evacuate...");
                            var k = source.protoModuleCrew[0];
                            if (k != null)
                            {
                                print("Evacuating " + k.name);
                                source.RemoveCrewmember(k);
                                k.seat = null;
                                k.rosterStatus = ProtoCrewMember.RosterStatus.Available;

                                //Add Crewmember
                                part.AddCrewmember(k);
                                k.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
                                if (k.seat != null)
                                    k.seat.SpawnCrew();

                                //Decouple!
                                var p = part.parent;
                                var dpart = part.FindModuleImplementing<ModuleDecouple>();
                                if (dpart != null)
                                {
                                    dpart.Decouple();
                                }
                            }
                            else
                            {
                                print("Problem evacuating crewmember...");
                                if (firstTry)
                                    TryToEvacuate(false);
                            }
                        }
                        else
                        {
                            print("Could not find any crew to move");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                print("ERROR: " + ex.Message);
                if (firstTry)
                    TryToEvacuate(false);
            }

        }


        [KSPEvent(guiName = "#LOC_USI_InflateLifeboat", guiActive = true, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]//Inflate Lifeboat
        public void InflateLifeboat()
        {
            try
            {
                if (!isDeployed)
                {
                    PlayDeployAnimation();
                }
            }
            catch (Exception ex)
            {
                print("ERR in InflateLifeboat: " + ex.StackTrace);
            }
        }

        private void PlayDeployAnimation(int speed = 1)
        {
            print("Inflating");
            DeployAnimation[deployAnimationName].speed = speed;
            DeployAnimation.Play(deployAnimationName);
            isDeployed = true;
            part.CrewCapacity = 1;
            ToggleEvent("InflateLifeboat", false);
            if (!isUnsealed)
            {
                AddResources();
                isUnsealed = true;
            }
        }

        public void DeflateLifeboat(int speed)
        {
            print("Deflating");
            DeployAnimation[deployAnimationName].time = DeployAnimation[deployAnimationName].length;
            DeployAnimation[deployAnimationName].speed = speed;
            DeployAnimation.Play(deployAnimationName);
            part.CrewCapacity = 0;
        }

        private void ToggleEvent(string eventName, bool state)
        {
            Events[eventName].active = state;
            Events[eventName].externalToEVAOnly = state;
            Events[eventName].guiActive = state;
        }

        public override void OnStart(StartState state)
        {
            DeployAnimation[deployAnimationName].layer = 2;
            if (part.protoModuleCrew.Count > 0)
            {
                //They got in here somehow...
                isDeployed = true;
            }
            if (!isDeployed) DeflateLifeboat(-10);
            else PlayDeployAnimation(10);
            base.OnStart(state);
        }


        private void AddResources()
        {
            var massToLose = 0f;
            var mods = part.FindModulesImplementing<ModuleStoredResource>();
            var count = mods.Count;
            for(int i = 0; i <count; ++i)
            {
                var p = mods[i];
                var resInfo = PartResourceLibrary.Instance.GetDefinition(p.ResourceName);
                var resNode = new ConfigNode("RESOURCE");
                resNode.AddValue("name", p.ResourceName);
                resNode.AddValue("amount", p.Amount);
                resNode.AddValue("maxAmount", p.Amount);
                part.Resources.Add(resNode);
                massToLose += resInfo.density * p.Amount;
            }
            part.mass -= massToLose;
            if (part.mass < 0.05f) part.mass = 0.05f;
        }


        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            try
            {
                if (part.checkLanded())
                {
                    Dampen();
                }
                if (part.Landed)
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
            if (!isDeployed)
                return;
            if (vessel.srfSpeed > dampenSpeed
                || vessel.horizontalSrfSpeed > dampenSpeed)
            {
                var count = vessel.parts.Count;
                for (int i = 0; i < count; ++i)
                {
                    var p = vessel.parts[i];
                    p.Rigidbody.angularVelocity *= dampenFactor;
                    p.Rigidbody.velocity *= dampenFactor;
                }
            }
        }
    }
}

