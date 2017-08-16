using UnityEngine;

using System;
using System.Collections.Generic;

namespace KonstructionRobotics.PartModules
{
    /// <summary>
    /// Add this <see cref="PartModule"/> to any part that behaves as an articulated hitch.
    /// </summary>
    public class USIModuleHitch : PartModule
    {
        #region Static class variables
        /// <summary>
        /// The spring force to use for <see cref="JointSpring.spring"/>.
        /// </summary>
        protected const float JOINT_SPRING_FORCE = 150.0f;

        /// <summary>
        /// The spring damper force to use for <see cref="JointSpring.damper"/>.
        /// </summary>
        protected const float JOINT_DAMPER_FORCE = 15.0f;

        /// <summary>
        /// <see cref="SoftJointLimit.bounciness"/> of the hitch joint.
        /// </summary>
        protected const float JOINT_BOUNCINESS = 0.1f;

        /// <summary>
        /// The angle limit for the hitch joint. See <see cref="SoftJointLimit.limit"/>.
        /// </summary>
        protected const float JOINT_LIMIT = 10.0f;

        /// <summary>
        /// The maximum angle at which <see cref="ConfigurableJointMotion.Locked"/> can be engaged.
        /// </summary>
        /// <remarks>
        /// Unity seems to dramatically increase the spring force in order to "snap" a joint back
        /// to the desired orientation whenever ConfigurableJointMotion.Locked is enaged. We want to wait
        /// for the joint to reach a reasonable angle before enaging the lock for the sake of realism.
        /// </remarks>
        protected const float JOINT_LOCK_EASING = 2.0f;
        #endregion

        #region Local class variables
        /// <summary>
        /// This is used by <see cref="OnUpdate"/> to prevent it from indefinitely searching for a child attach node.
        /// </summary>
        protected int _attachChildTimeout = 120;

        /// <summary>
        /// Denotes whether this part has parts attached to it via attach nodes.
        /// </summary>
        /// <remarks>
        /// This is determined in <see cref="OnUpdate"/>.
        /// </remarks>
        protected bool _isChildConnected = false;

        /// <summary>
        /// Denotes whether the vessel is currently on or off rails.
        /// </summary>
        /// <remarks>
        /// This is set by the <see cref="VesselOnRails(Vessel)"/> and <see cref="VesselOffRails(Vessel)"/> methods.
        /// </remarks>
        protected bool _isVesselOnRails = false;

        /// <summary>
        /// The initial rotation of the <see cref="_movableMesh"/>.
        /// </summary>
        /// <remarks>This is captured in <see cref="OnStartFinished(PartModule.StartState)"/>.</remarks>
        protected Quaternion _initialRotation;

        /// <summary>
        /// Used by <see cref="OnUpdate"/> to determine if it should attempt to lock the hitch joint.
        /// </summary>
        protected bool _isLocking = false;

        /// <summary>
        /// Used as a countdown timer by <see cref="OnUpdate"/> to avoid running expensive operations every frame for non-critical tasks.
        /// </summary>
        protected float _slowUpdate = 1.0f;
        #endregion

        #region Cached object references
        /// <summary>
        /// This should exist in the part model as a GameObject that represents the portion of the part that will remain stationary.
        /// </summary>
        /// <remarks>
        /// This is retrieved in <see cref="OnStart(PartModule.StartState)"/>.
        /// </remarks>
        protected Transform _fixedMesh;

        /// <summary>
        /// This should exist in the part model as a GameObject that represents the portion of the part that will move.
        /// </summary>
        /// <remarks>
        /// This is retrieved in <see cref="OnStart(PartModule.StartState)"/>.
        /// </remarks>
        protected Transform _movableMesh;

        /// <summary>
        /// Unity will use this to allow our <see cref="_movableMesh"/> and any attached children to move automagically.
        /// </summary>
        /// <remarks>This is instantiated in <see cref="OnStart(PartModule.StartState)"/>.</remarks>
        protected ConfigurableJoint _hitchJoint;
        #endregion

        #region Non-Persistent KSPFields // These are part settings that can be loaded via .cfg file
        // The name of the mesh/transform in the part that represents the fixed portion of the model
        [KSPField(isPersistant = false)] public string FixedMeshName = "Fixed";

        // The name of the mesh/transform in the part that represents the movable portion of the model
        [KSPField(isPersistant = false)] public string MovableMeshName = "Movable";

        // The mass for the Rigidbody that is/will be attached to the fixed mesh
        [KSPField(isPersistant = false)] public float FixedMeshMass = 1.0f;

        // The mass for the Rigidbody that is/will be attached to the movable mesh
        [KSPField(isPersistant = false)] public float MovableMeshMass = 0.1f;

        // The axis for setting up a FixedJoint that will be used to "lock" the fixed mesh to the root transform of the part.
        [KSPField(isPersistant = false)] public float FixedJointAxisX = 0;
        [KSPField(isPersistant = false)] public float FixedJointAxisY = 0;
        [KSPField(isPersistant = false)] public float FixedJointAxisZ = 1;

        // The primary axis for the ConfigurableJoint that the movable mesh will move on.
        [KSPField(isPersistant = false)] public float HingeAxisX = 0;
        [KSPField(isPersistant = false)] public float HingeAxisY = 0;
        [KSPField(isPersistant = false)] public float HingeAxisZ = 1;

        // The secondary axis for the ConfigurableJoint that the movable mesh will move on.
        [KSPField(isPersistant = false)] public float HingeSecondaryAxisX = 0;
        [KSPField(isPersistant = false)] public float HingeSecondaryAxisY = 1;
        [KSPField(isPersistant = false)] public float HingeSecondaryAxisZ = 0;

        // The offset from the origin point of the movable mesh to the location of the attach node.
        [KSPField(isPersistant = false)] public float MovableMeshAttachNodeOffsetX = 0;
        [KSPField(isPersistant = false)] public float MovableMeshAttachNodeOffsetY = 0;
        [KSPField(isPersistant = false)] public float MovableMeshAttachNodeOffsetZ = 0;
        #endregion

        #region Persistent KSPFields // These are part settings that can be loaded via .cfg file and later from the save file.
        /// <summary>
        /// Denotes whether the joint is/should be locked or free.
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool IsLocked = true;
        #endregion

        #region KSPEvents // These behaviors show up in the part right-click menu.
        /// <summary>
        /// Locks or unlocks the hinge via the part right-click menu.
        /// </summary>
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Unlock Hinge")]
        public void LockToggleEvent()
        {
            LockToggle(!IsLocked);
        }
        #endregion

        #region KSPActions // These are behaviors that can be assigned to an action group.
        /// <summary>
        /// Locks the hinge via Action Groups.
        /// </summary>
        [KSPAction("Lock Hinge")]
        public void LockHingeAction(KSPActionParam param)
        {
            LockToggle(false);
        }

        /// <summary>
        /// Unlocks the hinge via Action Groups.
        /// </summary>
        [KSPAction("Unlock Hinge")]
        public void UnlockHingeAction(KSPActionParam param)
        {
            LockToggle(true);
        }

        /// <summary>
        /// Toggles hinge lock/unlock via Action Groups.
        /// </summary>
        [KSPAction("Toggle Hinge Lock")]
        public void ToggleDirectionAction(KSPActionParam param)
        {
            LockToggle(!IsLocked);
        }
        #endregion

        #region KSPAction/KSPEvent helper methods
        /// <summary>
        /// Locks or unlocks the hitch hinge.
        /// </summary>
        /// <param name="isLocked">The new value to assign to <see cref="IsLocked"/>.</param>
        protected void LockToggle(bool isLocked)
        {
            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Hitch.LockToggle: Hinge is " + (isLocked ? "" : "not ") + "locked.");

            // Update the right-click menu button text
            Events["LockToggleEvent"].guiName = (isLocked ? "Unlock Hinge" : "Lock Hinge");

            // Lock or unlock the hitch
            if (isLocked)
            {
                TryLock();
            }
            else
            {
                IsLocked = false;

                _hitchJoint.angularXMotion = ConfigurableJointMotion.Limited;
                _hitchJoint.angularYMotion = ConfigurableJointMotion.Limited;
                _hitchJoint.angularZMotion = ConfigurableJointMotion.Limited;
            }
        }
        #endregion

        #region Delegates
        /// <summary>
        /// Handles notifications from <see cref="GameEvents.onVesselGoOnRails"/>.
        /// </summary>
        /// <param name="v"></param>
        public void VesselOnRails(Vessel v)
        {
            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Hitch.VesselOnRails called.");

            if (v == this.vessel)
            {
                _isVesselOnRails = true;

                // If we have attached children, we need the game to record their current position/rotation
                if (part.children.Count > 0)
                {
                    Part root = vessel.rootPart;
                    Part[] children = part.FindChildParts<Part>(true);

                    for (int i = 0; i < children.Length; i++)  // using [for] instead of [foreach] is a Unity best practice
                    {
                        children[i].UpdateOrgPosAndRot(root);
                    }
                }
            }
        }

        /// <summary>
        /// Handles notifications from <see cref="GameEvents.onVesselGoOffRails"/>
        /// </summary>
        /// <param name="v"></param>
        public void VesselOffRails(Vessel v)
        {
            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Hitch.VesselOffRails called.");

            if (v == this.vessel)
            {
                _isVesselOnRails = false;
            }
        }

        /// <summary>
        /// Handles notifications from <see cref="GameEvents.onVesselCreate"/>.
        /// </summary>
        /// <param name="v"></param>
        public void VesselCreated(Vessel v)
        {
            if (GameSettings.VERBOSE_DEBUG_LOG && this.vessel == v)
                Debug.Log("[USI Tools] Hitch.VesselCreated called.");

            // Lock the hitch hinge joint
            if (this.vessel == v && _hitchJoint != null)
                LockToggle(true);
        }

        /// <summary>
        /// Handles notifications from <see cref="GameEvents.onPartJointBreak"/>.
        /// </summary>
        /// <param name="joint">The joint that was broken (via undocking, hopefully).</param>
        /// <param name="something">No idea what this is. Doesn't matter, we don't need it.</param>
        public void PartJointBreak(PartJoint joint, float something)
        {
            bool iAmParent = (joint.Parent != null && joint.Parent == this.part);
            bool iAmTarget = (joint.Target != null && joint.Target == this.part);
            bool iAmHost   = (joint.Host != null && joint.Host == this.part);

            if (iAmParent || iAmTarget || iAmHost)
            {
                if (GameSettings.VERBOSE_DEBUG_LOG)
                {
                    Debug.Log("[USI Tools] Hitch.PartJointBreak called on vessel rooted at: " + vessel.rootPart.name);
                    Debug.Log("[USI Tools] Hitch.PartJointBreak: I am " + (iAmHost ? "host " : "") + (iAmParent ? "parent " : "") + (iAmTarget ? "target " : ""));
                }

                // Lock the hitch hinge joint
                LockToggle(true);
            }
        }

        /// <summary>
        /// Handles notifications from <see cref="GameEvents.onPartCouple"/>
        /// </summary>
        /// <param name="eventData">The <see cref="Part"/>s involved in the docking event.</param>
        public void PartDocked(GameEvents.FromToAction<Part, Part> eventData)
        {
            if (this.part == eventData.from || this.part == eventData.to)
            {
                if (GameSettings.VERBOSE_DEBUG_LOG)
                    Debug.Log("[USI Tools] Hitch.PartDocked called. From: " + eventData.from.name + ", To: " + eventData.to.name);

                // Reattach child parts to our movable mesh
                _isChildConnected = ReattachChildren();
            }
        }
        #endregion

        #region Parent method overrides
        /// <summary>
        /// This method is called once when the part is loaded into the current scene (e.g. editor, flight, etc.)
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Hitch.OnAwake called.");

            // Register listeners for GameEvents
            GameEvents.onVesselGoOnRails.Add(VesselOnRails);
            GameEvents.onVesselGoOffRails.Add(VesselOffRails);
            GameEvents.onVesselCreate.Add(VesselCreated);
            GameEvents.onPartJointBreak.Add(PartJointBreak);
            GameEvents.onPartCouple.Add(PartDocked);
        }

        /// <remarks>
        /// Implementation of <see cref="MonoBehaviour"/> OnDestroy.
        /// </remarks>
        void OnDestroy()
        {
            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Hitch.OnDestroy called.");

            // Un-register listeners for GameEvents
            GameEvents.onVesselGoOnRails.Remove(VesselOnRails);
            GameEvents.onVesselGoOffRails.Remove(VesselOffRails);
            GameEvents.onVesselCreate.Remove(VesselCreated);
            GameEvents.onPartJointBreak.Remove(PartJointBreak);
            GameEvents.onPartCouple.Remove(PartDocked);
        }

        /// <summary>
        /// This method is called once when the part is loaded into the current scene, after OnLoad.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Hitch.OnStart called.");

            // Let's see if we can find our fixed mesh and movable mesh
            try
            {
                _fixedMesh = KSPUtil.FindInPartModel(transform, FixedMeshName);
                _movableMesh = KSPUtil.FindInPartModel(transform, MovableMeshName);

                if (_fixedMesh != null && _movableMesh != null)
                {
                    if (GameSettings.VERBOSE_DEBUG_LOG)
                    {
                        Debug.Log("[USI Tools] Hitch.OnStart: Found fixed mesh!");
                        Debug.Log("[USI Tools] Hitch.OnStart: Found movable mesh!");
                    }

                    // Give our FixedMesh and MovableMesh their own Rigidbody, if they don't already have one
                    Rigidbody fixedRigidbody = _fixedMesh.gameObject.GetComponent<Rigidbody>();
                    Rigidbody movableRigidbody = _movableMesh.gameObject.GetComponent<Rigidbody>();

                    if (fixedRigidbody == null)
                        fixedRigidbody = _fixedMesh.gameObject.AddComponent<Rigidbody>();

                    if (movableRigidbody == null)
                        movableRigidbody = _movableMesh.gameObject.AddComponent<Rigidbody>();

                    // Set the mass of the Rigidbodies from .cfg
                    fixedRigidbody.mass = FixedMeshMass;
                    movableRigidbody.mass = MovableMeshMass;
                }
                else
                    throw new Exception("Part must contain child GameObjects named " + FixedMeshName + " and " + MovableMeshName + ".");
            }
            catch (Exception ex)
            {
                Debug.LogError("[USI Tools] Hitch.OnStart encountered an error: " + ex.Message);
            }
        }

        /// <summary>
        /// This method is called once when the part is loaded into the current scene, after OnStart.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);

            try
            {
                // Change autostrut mode for wheels to prevent the hitch from being "strut locked"
                if (HighLogic.LoadedSceneIsFlight && vessel != null)
                {
                    List<ModuleWheelBase> wheels = vessel.FindPartModulesImplementing<ModuleWheelBase>();
                    for (int i = 0; i < wheels.Count; i++)
                    {
                        wheels[i].part.autoStrutMode = Part.AutoStrutMode.ForceGrandparent;
                    }
                }

                // If our parent part is a hitch, then we need to swap the fixed and movable meshes
                PartJoint attachmentNode = part.attachJoint;
                if (GameSettings.VERBOSE_DEBUG_LOG)
                {
                    Debug.Log("[USI Tools] Hitch.OnStartFinished: We are " + (attachmentNode == null ? "not " : string.Empty) + "attached to another part.");
                    if (attachmentNode != null)
                        Debug.Log("[USI Tools] Hitch.OnStartFinished: We are attached to " + attachmentNode.Parent.name + ".");
                }
                if (attachmentNode != null && attachmentNode.Parent.name.Equals(part.name, StringComparison.Ordinal))  // using String.Equals|StringComparison.Ordinal instead of == is a Unity best practice
                {
                    Transform temp = _fixedMesh;
                    _fixedMesh = _movableMesh;
                    _movableMesh = temp;
                }

                // Cache the current orientation of the movable mesh
                _initialRotation = _movableMesh.transform.localRotation;

                // Setup a Joint for our MovableMesh
                _hitchJoint = _movableMesh.gameObject.AddComponent<ConfigurableJoint>();

                // Mate the joint to the FixedMesh
                _hitchJoint.connectedBody = _fixedMesh.GetComponent<Rigidbody>();

                // Setup joint anchor location and axes
                // Note: Joint axes are relative to the transform they are attached to. They don't have to
                //       (and often shouldn't) match the axes of the transform itself. They should instead
                //       be aligned such that the Joint's primary axis points along the axis of the transform
                //       that needs to have the most customizability.
                _hitchJoint.anchor = Vector3.zero;
                _hitchJoint.axis = new Vector3(HingeAxisX, HingeAxisY, HingeAxisZ);
                _hitchJoint.secondaryAxis = new Vector3(HingeSecondaryAxisX, HingeSecondaryAxisY, HingeSecondaryAxisZ);
                _hitchJoint.autoConfigureConnectedAnchor = true;

                // We don't want this joint to be breakable
                _hitchJoint.breakForce = float.PositiveInfinity;
                _hitchJoint.breakTorque = float.PositiveInfinity;

                // Since this is a hinge, we don't want to allow linear movement
                _hitchJoint.xMotion = ConfigurableJointMotion.Locked;
                _hitchJoint.yMotion = ConfigurableJointMotion.Locked;
                _hitchJoint.zMotion = ConfigurableJointMotion.Locked;
                _hitchJoint.projectionMode = JointProjectionMode.PositionAndRotation;
                _hitchJoint.projectionDistance = 0;
                _hitchJoint.projectionAngle = 0;

                // Configure the angular limits of the hinge
                SoftJointLimit xMinLimit = _hitchJoint.lowAngularXLimit;
                SoftJointLimit xMaxLimit = _hitchJoint.highAngularXLimit;
                SoftJointLimit yLimit = _hitchJoint.angularYLimit;
                SoftJointLimit zLimit = _hitchJoint.angularZLimit;
                xMinLimit.bounciness = JOINT_BOUNCINESS;
                xMaxLimit.bounciness = JOINT_BOUNCINESS;
                yLimit.bounciness = JOINT_BOUNCINESS;
                zLimit.bounciness = JOINT_BOUNCINESS;
                xMinLimit.limit = -1.0f * JOINT_LIMIT;
                xMaxLimit.limit = JOINT_LIMIT;
                yLimit.limit = JOINT_LIMIT;
                zLimit.limit = JOINT_LIMIT;
                _hitchJoint.lowAngularXLimit = xMinLimit;
                _hitchJoint.highAngularXLimit = xMaxLimit;
                _hitchJoint.angularYLimit = yLimit;
                _hitchJoint.angularZLimit = zLimit;

                // We want the hitch to always return to zero, if possible
                _hitchJoint.targetRotation = Quaternion.identity;
                _hitchJoint.rotationDriveMode = RotationDriveMode.XYAndZ;

                JointDrive xDrive = _hitchJoint.angularXDrive;
                JointDrive yzDrive = _hitchJoint.angularYZDrive;
                xDrive.positionDamper = JOINT_DAMPER_FORCE;
                yzDrive.positionDamper = JOINT_DAMPER_FORCE;
                xDrive.positionSpring = JOINT_SPRING_FORCE;
                yzDrive.positionSpring = JOINT_SPRING_FORCE;
                _hitchJoint.angularXDrive = xDrive;
                _hitchJoint.angularYZDrive = yzDrive;

                // We need to anchor our FixedMesh to the root part's Rigidbody with a Joint
                Rigidbody rootRigidbody = part.GetComponent<Rigidbody>();
                if (rootRigidbody != null)
                {
                    if (GameSettings.VERBOSE_DEBUG_LOG)
                        Debug.Log("[USI Tools] Hitch.OnStartFinished: Found root Rigidbody!");

                    // Create a joint to "lock" our FixedMesh in place
                    FixedJoint joint = _fixedMesh.gameObject.AddComponent<FixedJoint>();

                    // Mate the joint to the root part's Rigidbody
                    joint.connectedBody = rootRigidbody;

                    // Configure other Joint options
                    joint.anchor = Vector3.zero;
                    joint.axis = Vector3.forward;
                    joint.autoConfigureConnectedAnchor = true;
                    joint.breakForce = float.PositiveInfinity;
                    joint.breakTorque = float.PositiveInfinity;
                }
                else
                    throw new Exception("Part root Transform must have a Rigidbody.");

                // Lock the hitch joint by default
                LockToggle(IsLocked);
            }
            catch (Exception ex)
            {
                Debug.LogError("[USI Tools] Hitch.OnStartFinished encountered an error: " + ex.Message);
            }
        }

        /// <summary>
        /// This method gets called when MonoBehaviour.Update would normally be called.
        /// </summary>
        /// <remarks>
        /// See <see cref="MonoBehaviour"/> for more information.
        /// </remarks>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (HighLogic.LoadedSceneIsFlight && part.State != PartStates.DEAD && !_isVesselOnRails )
            {
                // Reattach child parts to our movable mesh instead of the root transform of the part
                // We can't do this in OnStart or OnStartFinished unfortunately because our children won't have their
                // attach node/ConfigurableJoint setup yet, so we're forced to do this during Update instead.
                if (_attachChildTimeout > 0 && part.children.Count > 0 && !_isChildConnected)
                {
                    if (GameSettings.VERBOSE_DEBUG_LOG)
                        Debug.Log("[USI Tools] Hitch.OnUpdate: Looking for child attach nodes.");

                    _attachChildTimeout--;

                    _isChildConnected = ReattachChildren();

                    // Reset attach timeout if child successfully connected
                    if (_isChildConnected)
                        _attachChildTimeout = 120;
                }

                // Try to lock the hitch if it has been queued
                if (_isLocking)
                {
                    // There's some overhead involved in determining if it's safe to lock the hitch and it's not
                    // important that we do it every frame. So we'll do it once a second instead.
                    if (_slowUpdate <= 0)
                    {
                        TryLock();
                        _slowUpdate = 1.0f;
                    }
                    else
                        _slowUpdate -= Time.deltaTime;
                }
            }
        }
        #endregion

        #region Class methods
        /// <summary>
        /// Called by <see cref="LockToggle(bool)"/> and <see cref="OnUpdate"/> to attempt locking the hitch.
        /// </summary>
        /// <returns><c>true</c> if locked was engaged, <c>false</c> otherwise.</returns>
        protected bool TryLock()
        {
            if (IsLocked)
                return true;

            _isLocking = true;

            // Lock the hitch joint if the difference between the movable mesh's initial rotation and current rotation
            // is at or below the threshold of the joint lock easing value
            //if (Mathf.Abs(Quaternion.Angle(_initialRotation, _movableMesh.transform.localRotation)) <= JOINT_LOCK_EASING)
            //{
                _hitchJoint.angularXMotion = ConfigurableJointMotion.Locked;
                _hitchJoint.angularYMotion = ConfigurableJointMotion.Locked;
                _hitchJoint.angularZMotion = ConfigurableJointMotion.Locked;

                _isLocking = false;
                IsLocked = true;
            //}

            return IsLocked;
        }

        /// <summary>
        /// This method is called by <see cref="OnStartFinished(PartModule.StartState)"/> and <see cref="PartDocked(GameEvents.FromToAction{Part, Part})"/> to attach
        /// child parts (<see cref="ConfigurableJoint"/>) to our <see cref="_movableMesh"/>.
        /// </summary>
        protected bool ReattachChildren()
        {
            bool success = false;

            if (HighLogic.LoadedSceneIsFlight && part.State != PartStates.DEAD)
            {
                // If this part has children attached via an AttachNode, they will be connected to this part's root Transform
                // by default. We want them to be attached to our movable mesh instead so that they will actually move!
                if (!_isChildConnected && part.children.Count > 0)
                {
                    if (GameSettings.VERBOSE_DEBUG_LOG)
                        Debug.Log("[USI Tools] Hitch.ReattachChildren: Looking for child attach nodes.");

                    // Look for attach nodes in children 
                    ConfigurableJoint joint;
                    for (int i = 0; i < part.children.Count; i++)  // using for instead of foreach is a Unity best practice
                    {
                        joint = part.children[i].attachJoint.Joint;

                        // Reconfigure child's attach node ConfigurableJoint to be locked to our MovableMesh
                        if (joint != null)
                            success |= ReattachChild(joint);
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// This method is called by <see cref="ReattachChildren"/> to attach
        /// a child part's attach node (<see cref="ConfigurableJoint"/>) to our <see cref="_movableMesh"/>.
        /// </summary>
        /// <param name="joint">The child part's attach node <see cref="ConfigurableJoint"/> created by KSP.</param>
        protected bool ReattachChild(ConfigurableJoint joint)
        {
            try
            {
                // Attach the joint to the movable mesh
                joint.connectedBody = _movableMesh.GetComponent<Rigidbody>();

                // Create a new anchor point relative to the movable mesh so that the part is still attached in
                //   the correct position (i.e. at the location of the original attach node).
                joint.anchor = new Vector3(MovableMeshAttachNodeOffsetX, MovableMeshAttachNodeOffsetY, MovableMeshAttachNodeOffsetZ);
                joint.autoConfigureConnectedAnchor = true;

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[USI Tools] Hitch.ReattachChild encountered an error: " + ex.Message);

                return false;
            }
        }
        #endregion
    }
}
