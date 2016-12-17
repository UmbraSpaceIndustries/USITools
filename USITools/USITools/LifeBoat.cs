using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    if (part.CrewCapacity == 1 && !part.protoModuleCrew.Any())
                    {
                        print("Evacuating!");
                        var source = vessel.Parts.First(x => x != part
                                                             && !x.Modules.Contains("LifeBoat")
                                                             && x.protoModuleCrew.Count > 0);

                        if (source != null)
                        {
                            print("Attempting to evacuate...");
                            var k = source.protoModuleCrew.First();
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
                                if (p.Modules.OfType<ModuleDecouple>().Any())
                                {
                                    var dpart = p.Modules.OfType<ModuleDecouple>().First();
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


        [KSPEvent(guiName = "Inflate Lifeboat", guiActive = true, externalToEVAOnly = true, guiActiveEditor = false, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
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

            foreach (var p in part.FindModulesImplementing<ModuleStoredResource>())
            {
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
                //print("Dampening...");
                foreach (var p in vessel.parts)
                {
                    p.Rigidbody.angularVelocity *= dampenFactor;
                    p.Rigidbody.velocity *= dampenFactor;
                }
            }
        }
    }
}

