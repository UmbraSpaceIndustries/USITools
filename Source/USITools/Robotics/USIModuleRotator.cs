using System;
using UnityEngine;

namespace USITools
{
    /// <summary>
    /// Add this PartModule to any part that rotates about a fixed axis.
    /// </summary>
    public class USIModuleRotator : PartModule
    {
        #region Static class variables
        /// <summary>
        /// A fixed base value used to calculate the velocity for <see cref="JointMotor.targetVelocity"/>.
        /// Any tweaking needed for a specific part is done via the <see cref="SpeedMultiplier"/> value.
        /// </summary>
        /// <remarks>
        /// This should be established at a reasonable value and then left alone.
        /// </remarks>
        protected const float BASE_TARGET_VELOCITY = 20.0f;

        /// <summary>
        /// A fixed base value used to calculate the torque for <see cref="JointMotor.force"/>.
        /// Any tweaking needed for a specific part is done via the <see cref="TorqueMultiplier"/> value.
        /// </summary>
        /// <remarks>
        /// This should be established at a reasonable value and then left alone.
        /// </remarks>
        protected const float BASE_MOTOR_TORQUE = 0.1f;

        /// <summary>
        /// A fixed base value used to set the spring force for <see cref="JointSpring.spring"/>.
        /// </summary>
        protected const float JOINT_SPRING_FORCE = 1000.0f;

        /// <summary>
        /// A fixed base value used to set the spring force for <see cref="JointSpring.damper"/>.
        /// </summary>
        protected const float JOINT_DAMPER_FORCE = 1000.0f;
        #endregion

        #region Local class variables
        /// <summary>
        /// This is used by OnUpdate to prevent it from indefinitely searching for a child attach node.
        /// </summary>
        protected int _attachChildTimeout = 120;

        /// <summary>
        /// This is determined in OnUpdate.
        /// </summary>
        protected bool _isChildConnected = false;

        /// <summary>
        /// This is set by the <see cref="VesselOnRails(Vessel)"/> and <see cref="VesselOffRails(Vessel)"/> methods.
        /// </summary>
        protected bool _isVesselOnRails = false;
        #endregion

        #region Cached variables
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
        /// Unity will use this to rotate our <see cref="_movableMesh"/> and any attached children automagically.
        /// </summary>
        /// <remarks>This is instantiated in OnStart.</remarks>
        protected HingeJoint _rotatorJoint;

        /// <summary>
        /// A cached reference to the <see cref="HingeJoint.motor"/> on <see cref="_rotatorJoint"/>.
        /// </summary>
        /// <remarks>
        /// This is cached when the joint is setup in OnStart.
        /// </remarks>
        protected JointMotor _rotatorJointMotor;
        #endregion

        #region Non-Persistent KSPFields // These are part settings that can be loaded via .cfg file
        // The name of the mesh/transform in the part that represents the fixed portion of the model
        [KSPField(isPersistant = false)] public string FixedMeshName = "Fixed";

        // The name of the mesh/transform in the part that represents the movable portion of the model
        [KSPField(isPersistant = false)] public string MovableMeshName = "Movable";

        /// <summary>
        /// Set this in part.cfg to raise or lower the speed value for a part if <see cref="BASE_TARGET_VELOCITY"/> is inadequate.
        /// </summary>
        [KSPField(isPersistant = false)] public float SpeedMultiplier = 1.0f;

        /// <summary>
        /// Set this in part.cfg to raise or lower the torque value for a part if <see cref="BASE_MOTOR_TORQUE"/> is inadequate.
        /// </summary>
        [KSPField(isPersistant = false)] public float TorqueMultiplier = 1.0f;

        // The axis for setting up a HingeJoint that will be used to "lock" the fixed mesh to the root part transform.
        [KSPField(isPersistant = false)] public float FixedJointAxisX = 0;
        [KSPField(isPersistant = false)] public float FixedJointAxisY = 0;
        [KSPField(isPersistant = false)] public float FixedJointAxisZ = 1;

        // The axis for the HingeJoint that the movable mesh will rotate around.
        [KSPField(isPersistant = false)] public float RotationAxisX = 0;
        [KSPField(isPersistant = false)] public float RotationAxisY = 0;
        [KSPField(isPersistant = false)] public float RotationAxisZ = 1;

        // The offset from the origin point of the movable mesh to the location of the attach node.
        // TODO: Determine if these values are actually necessary (i.e. will we be able to just use 0 in every instance?)
        [KSPField(isPersistant = false)] public float MovableMeshAttachNodeOffsetX = 0;
        [KSPField(isPersistant = false)] public float MovableMeshAttachNodeOffsetY = 0;
        [KSPField(isPersistant = false)] public float MovableMeshAttachNodeOffsetZ = 0;
        #endregion

        #region Persistent KSPFields // These are part settings that can be loaded via .cfg file and later from the save file
        /// <summary>
        /// The rotational velocity that the part will attempt to achieve.
        /// </summary>
        /// <remarks>
        /// This is just one of the values used to calculate the actual speed.
        /// See <see cref="UpdateMotor"/> to see the full calculation.
        /// </remarks>
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Speed", guiFormat = "N1")]
        [UI_FloatEdit(minValue = 1, maxValue = 11.0f, incrementSmall = 0.5f, incrementLarge = 1.0f, incrementSlide = 0.5f, scene = UI_Scene.All, sigFigs = 1)]
        public float Speed = 1.0f;

        /// <summary>
        /// The amount of force the joint motor will use to attempt to reach target velocity (aka <see cref="Speed"/>).
        /// </summary>
        /// <remarks>
        /// This is just one of the values used to calculate the actual torque.
        /// See <see cref="UpdateMotor"/> to see the full calculation.
        /// </remarks>
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Torque", guiFormat = "N1")]
        [UI_FloatEdit(minValue = 1, maxValue = 11.0f, incrementSmall = 0.5f, incrementLarge = 1.0f, incrementSlide = 0.5f, scene = UI_Scene.All, sigFigs = 1)]
        public float Torque = 1.0f;

        /// <summary>
        /// Denotes whether the joint is rotating in the forward or reverse direction.
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool IsInverted = false;

        /// <summary>
        /// Denotes whether the joint is/should be rotating or stationary.
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool IsRunning = false;
        #endregion

        #region KSPEvents // These show up in the part right-click menu
        /// <summary>
        /// Sets rotation in the forward or reverse direction from the UI.
        /// </summary>
        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Reverse")]
        public void DirectionToggleEvent()
        {
            DirectionToggle(!IsInverted);
        }

        /// <summary>
        /// Starts and stops rotation.
        /// </summary>
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Start")]
        public void StartStopToggleEvent()
        {
            StartStopToggle(!IsRunning);
        }
        #endregion

        #region KSPActions // These are behaviors that can be assigned to an action group.
        /// <summary>
        /// Sets rotation in the forward direction via Action Groups.
        /// </summary>
        [KSPAction("Forward Direction")]
        public void ForwardDirectionAction(KSPActionParam param)
        {
            DirectionToggle(false);
        }

        /// <summary>
        /// Sets rotation in the reverse direction via Action Groups.
        /// </summary>
        [KSPAction("Reverse Direction")]
        public void ReverseDirectionAction(KSPActionParam param)
        {
            DirectionToggle(true);
        }

        /// <summary>
        /// Toggles rotation direction via Action Groups.
        /// </summary>
        [KSPAction("Toggle Direction")]
        public void ToggleDirectionAction(KSPActionParam param)
        {
            DirectionToggle(!IsInverted);
        }

        /// <summary>
        /// Starts rotation via Action Groups.
        /// </summary>
        [KSPAction("Start Rotation")]
        public void StartRotationAction(KSPActionParam param)
        {
            StartStopToggle(true);
        }

        /// <summary>
        /// Stops rotation via Action Groups.
        /// </summary>
        [KSPAction("Stop Rotation")]
        public void StopRotationAction(KSPActionParam param)
        {
            StartStopToggle(false);
        }

        /// <summary>
        /// Toggles rotation via Action Groups.
        /// </summary>
        [KSPAction("Toggle Rotation")]
        public void ToggleRotationAction(KSPActionParam param)
        {
            StartStopToggle(!IsRunning);
        }
        #endregion

        #region KSPAction/KSPEvent helper methods
        /// <summary>
        /// Called by KSPActions/KSPEvents related to changing the rotation direction.
        /// </summary>
        /// <param name="isInverted">The new value to assign to <see cref="IsInverted"/>.</param>
        protected void DirectionToggle(bool isInverted)
        {
            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Rotator.DirectionToggle: Servo direction is " + (isInverted ? " reversed." : " normal."));

            IsInverted = isInverted;
            Events["DirectionToggleEvent"].guiName = (IsInverted ? "Forward" : "Reverse");
        }

        /// <summary>
        /// Called by KSPActions/KSPEvents related to starting and stopping rotation.
        /// </summary>
        /// <param name="isRunning">The new value to assign to <see cref="IsRunning"/>.</param>
        protected void StartStopToggle(bool isRunning)
        {
            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Rotator.StartStopToggle: Servo is " + (isRunning ? "" : "not ") + "running.");

            IsRunning = isRunning;
            Events["StartStopToggleEvent"].guiName = (IsRunning ? "Stop" : "Start");
        }
        #endregion

        #region Delegates
        /// <summary>
        /// Handles notifications from <see cref="GameEvents.onVesselGoOnRails"/>.
        /// </summary>
        /// <param name="vessel"></param>
        public void VesselOnRails(Vessel v)
        {
            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Rotator.VesselOnRails called.");

            // If for some reason the Vessel param doesn't match this vessel, then return
            if (v != this.vessel)
                return;

            _isVesselOnRails = true;

            // If we have attached children, we need the game to record their current position/rotation
            if (_isChildConnected)
            {
                Part root = vessel.rootPart; 
                Part[] children = part.FindChildParts<Part>(true);
                for (int i = 0; i < children.Length; i++)  // using for instead of foreach is a Unity best practice
                {
                    children[i].UpdateOrgPosAndRot(root);
                }
            }
        }

        /// <summary>
        /// Handles notifications from <see cref="GameEvents.onVesselGoOffRails"/>
        /// </summary>
        /// <param name="vessel"></param>
        public void VesselOffRails(Vessel v)
        {
            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Rotator.VesselOffRails called.");

            // If for some reason the Vessel param doesn't match this vessel, then return
            if (v != this.vessel)
                return;

            _isVesselOnRails = false;
        }
        #endregion

        #region Parent method overrides
        /// <remarks>
        /// This method is called once when the part is loaded into the current scene (e.g. editor, flight, etc.)
        /// </remarks>
        public override void OnAwake()
        {
            base.OnAwake();

            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Rotator.OnAwake called.");

            // Register listeners for OnRails/OffRails events
            GameEvents.onVesselGoOnRails.Add(VesselOnRails);
            GameEvents.onVesselGoOffRails.Add(VesselOffRails);
        }

        /// <remarks>
        /// This method is called once when the part is loaded into the current scene, after OnAwake.
        /// </remarks>
        /// <param name="node"></param>
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Rotator.OnLoad called.");
        }

        /// <summary>
        /// This method is called once when the part is loaded into the current scene, after OnLoad.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (GameSettings.VERBOSE_DEBUG_LOG)
                Debug.Log("[USI Tools] Rotator.OnStart called.");

            // Make sure the text for gui events is set correctly
            DirectionToggle(IsInverted);
            StartStopToggle(IsRunning);

            // Let's see if we can find our fixed mesh and movable mesh
            try
            {
                _fixedMesh = KSPUtil.FindInPartModel(transform, FixedMeshName);
                _movableMesh = KSPUtil.FindInPartModel(transform, MovableMeshName);

                if (_fixedMesh != null && _movableMesh != null)
                {
                    if (GameSettings.VERBOSE_DEBUG_LOG)
                    {
                        Debug.Log("[USI Tools] Rotator.OnStart: Found fixed mesh!");
                        Debug.Log("[USI Tools] Rotator.OnStart: Found movable mesh!");
                    }

                    // Give our FixedMesh its own Rigidbody
                    Rigidbody fixedMeshRigidbody = _fixedMesh.gameObject.AddComponent<Rigidbody>();

                    // Give our MovableMesh its own Rigidbody
                    _movableMesh.gameObject.AddComponent<Rigidbody>();

                    // Setup a Joint for our MovableMesh and cache a reference to the motor for later
                    _rotatorJoint = _movableMesh.gameObject.AddComponent<HingeJoint>();
                    _rotatorJointMotor = _rotatorJoint.motor;

                    // Mate the joint to the FixedMesh
                    _rotatorJoint.connectedBody = fixedMeshRigidbody;

                    // Configure other Joint options
                    _rotatorJoint.anchor = Vector3.zero;
                    _rotatorJoint.axis = new Vector3(RotationAxisX, RotationAxisY, RotationAxisZ);
                    _rotatorJoint.autoConfigureConnectedAnchor = true;
                    _rotatorJoint.useMotor = true;
                    _rotatorJoint.breakForce = float.PositiveInfinity;
                    _rotatorJoint.breakTorque = float.PositiveInfinity;

                    // Setup the motor on the joint
                    UpdateMotor();
                }
                else
                    throw new Exception("Part must contain child GameObjects named " + FixedMeshName + " and " + MovableMeshName + ".");
            }
            catch (Exception ex)
            {
                Debug.LogError("[USI Tools] Rotator.OnStart encountered an error: " + ex.Message);
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
                // We need to anchor our FixedMesh to the root part's Rigidbody with a Joint
                Rigidbody rootRigidbody = part.GetComponent<Rigidbody>();

                if (_fixedMesh != null && rootRigidbody != null)
                {
                    if (GameSettings.VERBOSE_DEBUG_LOG)
                        Debug.Log("[USI Tools] Rotator.OnStartFinished: Found root Rigidbody!");

                    // Create a joint to "lock" our FixedMesh in place
                    // TODO: Change this to FixedJoint?
                    HingeJoint joint = _fixedMesh.gameObject.AddComponent<HingeJoint>();

                    // Mate the joint to the root part's Rigidbody
                    joint.connectedBody = part.GetComponent<Rigidbody>();

                    // Configure other Joint options
                    joint.anchor = Vector3.zero;
                    joint.axis = new Vector3(RotationAxisX, RotationAxisY, RotationAxisZ);
                    joint.autoConfigureConnectedAnchor = true;
                    joint.useSpring = true;
                    JointSpring spring = joint.spring;
                    spring.spring = JOINT_SPRING_FORCE;
                    spring.damper = JOINT_DAMPER_FORCE;
                    spring.targetPosition = 0;
                    joint.spring = spring;
                }
                else
                    throw new Exception("FixedMesh must exist and root part must have a Rigidbody.");

            }
            catch (Exception ex)
            {
                Debug.LogError("[USI Tools] Rotator.OnStartFinished encountered an error: " + ex.Message);
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

            if (!_isVesselOnRails)
            {
                // Update the joint's motor with current speed and torque values
                UpdateMotor();

                // If this part has a child part attached via an AttachNode, it will be connected to this part's root Transform
                // by default. We want it to be attached to our movable mesh instead so that the child part will move with it.
                // We can't do this in OnStart unfortunately because our children likely won't have their attach node/ConfigurableJoint
                // setup yet, so do this now...
                if (!_isChildConnected && _attachChildTimeout > 0 && part.children.Count > 0)
                {
                    if (GameSettings.VERBOSE_DEBUG_LOG)
                        Debug.Log("[USI Tools] Rotator.OnUpdate: Looking for child attach nodes.");

                    _attachChildTimeout--;

                    // See if child has a ConfigurableJoint yet (aka attach node, in KSP-speak) 
                    ConfigurableJoint joint;
                    for (int i = 0; i < part.children.Count; i++)  // using for instead of foreach is a Unity best practice
                    {
                        joint = part.children[i].attachJoint.Joint;

                        if (joint != null)
                        {
                            // Reconfigure child's attach node ConfigurableJoint to be locked to our MovableMesh
                            ReattachChild(joint);

                            // Reset _attachChildTimeout, just because
                            _attachChildTimeout = 120;
                        }
                    }
                }
            }
        }
        #endregion

        #region Class methods
        /// <summary>
        /// Called by OnStart and OnUpdate methods to change the values of the hinge motor
        /// </summary>
        protected void UpdateMotor()
        {
            try
            {
                if (_rotatorJoint != null)
                {
                    // Update motor with current values
                    _rotatorJointMotor.force = BASE_MOTOR_TORQUE * Torque * TorqueMultiplier;
                    _rotatorJointMotor.targetVelocity = IsRunning
                        ? BASE_TARGET_VELOCITY * Speed * SpeedMultiplier * (IsInverted ? -1 : 1)
                        : 0;

                    // Update joint with new motor values
                    _rotatorJoint.motor = _rotatorJointMotor;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[USI Tools] Rotator.UpdateMotor encountered an error: " + ex.Message);
            }
        }

        /// <summary>
        /// This method is called by <see cref="OnUpdate"/> to attach a child part's attach node (<see cref="ConfigurableJoint"/>) to our <see cref="_movableMesh"/>.
        /// </summary>
        /// <param name="joint">The child part's attach node <see cref="ConfigurableJoint"/> created by KSP.</param>
        protected void ReattachChild(ConfigurableJoint joint)
        {
            try
            {
                // Attach the joint to the movable mesh
                joint.connectedBody = _movableMesh.GetComponent<Rigidbody>();

                // Create a new anchor point relative to the movable mesh so that the part is still attached in
                //   the correct position (i.e. at the location of the original attach node).
                joint.anchor = new Vector3(MovableMeshAttachNodeOffsetX, MovableMeshAttachNodeOffsetY, MovableMeshAttachNodeOffsetZ);
                joint.autoConfigureConnectedAnchor = true;

                // Set this flag so that OnUpdate doesn't continually call this method.
                _isChildConnected = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[USI Tools] Rotator.ReattachChild encountered an error: " + ex.Message);
            }
        }
        #endregion
    }
}
