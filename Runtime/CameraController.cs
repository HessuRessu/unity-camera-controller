using UnityEngine;
using Pihkura.Camera.Behaviour;
using Pihkura.Camera.Control;
using Pihkura.Camera.Core;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Pihkura.Camera
{

    /// <summary>
    /// Camera input gate, will control if camera is reactive to input or not.
    /// </summary>
    public static class InputGate
    {
        private static readonly HashSet<object> _owners = new();

        public static bool HasFocus => _owners.Count > 0;

        public static bool Push(object owner)
        {
            return _owners.Add(owner);
        }

        public static bool Pop(object owner)
        {
            return _owners.Remove(owner);
        }
    }

    /// <summary>
    /// Central camera controller that manages multiple camera behaviours,
    /// handles input, zoom, rotation, movement, and updates rays for collision and LOS.
    /// </summary>
    public class CameraController : MonoBehaviour
    {

        public const int BEHAVIOUR_RTS = 0;
        public const int BEHAVIOUR_FOLLOW = 1;
        public const int BEHAVIOUR_FREE = 2;
        public const int BEHAVIOUR_CINEMATIC = 3;

        /// <summary>
        /// CameraController instance for singleton object.
        /// </summary>
        public static CameraController Instance { get; set; }

        /// <summary>
        /// Flag controlling if creating singleton object from CameraController.
        /// </summary>
        public bool useSingleton;

        /// <summary>
        /// Current active behaviour index.
        /// </summary>
        public int behaviourIndex = 0;

        /// <summary>
        /// Should this controller use input based behaviour rotation with configured rotation key?
        /// </summary>
        public bool useInputBehaviourRotation = false;

        /// <summary>
        /// Should we use use CameraInputGate for controlling camera reactiveness.
        /// </summary>
        public bool useCameraInputGate = true;

        /// <summary>
        /// The target Transform the camera follows or looks at.
        /// </summary>
        public Transform target;

        /// <summary>
        /// Camera configuration parameters.
        /// </summary>
        public CameraConfiguration configuration;

        /// <summary>
        /// Camera runtime state data.
        /// </summary>
        public CameraData data;

        /// <summary>
        /// Array of available camera behaviours (e.g., RTS, Targeted).
        /// </summary>
        private ICameraBehaviour[] availabeBehaviours;

        /// <summary>
        /// Input controller for the camera.
        /// </summary>
        private IInputControlManager inputControlManager;

        /// <summary>
        /// Time Delta.
        /// </summary>
        [System.NonSerialized] public float deltaTime;

        /// <summary>
        /// Ensures all required variables are set.
        /// </summary>
        private void EnsureInternalAssets()
        {
#if (ENABLE_INPUT_SYSTEM)
            if (configuration == null)
                configuration = new();
            if (configuration.inputMap == null)
                configuration.inputMap = AssetResolver.LoadAsset<InputActionAsset>("CameraInputAsset");
#endif
        }

        /// <summary>
        /// Unity On Validate callback: ensures all internal variables are set.
        /// </summary>
        void OnValidate()
            => EnsureInternalAssets();


        /// <summary>
        /// Initializes the camera controller instance for singleton object.
        /// </summary>
        private void Awake()
        {
            inputControlManager = BaseInputControlManager.Initialize(configuration);
            EnsureInternalAssets();

            if (!useSingleton)
                return;
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initializes the camera controller and sets up behaviours and rays.
        /// </summary>
        void Start()
        {
            data = new CameraData(0, 30);
            availabeBehaviours = new ICameraBehaviour[4] {
                    new RTSCameraBehaviour(configuration, data),
                    new FollowingCameraBehaviour(configuration, data),
                    new FreeCameraBehaviour(configuration, data),
                    new CinematicCameraBehaviour(configuration, data)
                };
            data.Update(transform);
            UpdateRays();
            SetBehaviour(behaviourIndex, null);
        }

        /// <summary>
        /// Set camera position and updates rays.
        /// </summary>
        public void SetPosition(Vector3 position, float distance = 10f)
        {
            transform.position = position;
            data.distance = distance;
            SynchronizeDataWithTransform();
            UpdateRays();
            availabeBehaviours[behaviourIndex].Initialize(target);
            UpdateRays();

            Vector3 offsetPosition = transform.position;
            offsetPosition.y = configuration.groundRay.Point.y + configuration.minDistance;
            transform.position = offsetPosition;
            SynchronizeDataWithTransform();
            UpdateRays();
        }

        /// <summary>
        /// Set camera position, rotation and updates rays.
        /// </summary>
        public void SetPosition(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            SynchronizeDataWithTransform();
            UpdateRays();
            data.distance = Vector3.Distance(position, configuration.forwardRay.Point);
            availabeBehaviours[behaviourIndex].Initialize(target);
            SynchronizeDataWithTransform();
            UpdateRays();
        }

        /// <summary>
        /// Synchronize CameraData with current transform.
        /// </summary>
        private void SynchronizeDataWithTransform()
        {
            data.current.Update(transform);
            data.next.Update(transform);
            data.yaw = data.targetYaw = data.current.eulerAngles.y;
            data.pitch = data.targetPitch = data.current.eulerAngles.x;
            data.effectivePitch = data.pitch;
            data.yawVelocity = 0f;
            data.pitchVelocity = 0f;
            data.moveVelocity = Vector3.zero;
            configuration.autoPitch = 0f;
            data.autoPitch = 0f;
            data.losTimer = 0f;
        }

        /// <summary>
        /// Updates the camera each frame after all other updates.
        /// Handles input, zoom, rotation, movement, and behaviour updates.
        /// </summary>
        void LateUpdate()
        {
            deltaTime = Time.unscaledDeltaTime;
            OnUpdateBegin();
            UpdateRays();
            if (useCameraInputGate && InputGate.HasFocus)
                return;

            HandleInput();

            if (!EventSystem.current.IsPointerOverGameObject())
                availabeBehaviours[behaviourIndex].HandleZoom(deltaTime);
                
            availabeBehaviours[behaviourIndex].HandleRotation(deltaTime);
            availabeBehaviours[behaviourIndex].HandleMovement(deltaTime);

            OnUpdateCompleted();
        }

        /// <summary>
        /// Handles camera controller enable event.
        /// </summary>
        void OnEnable() => inputControlManager.OnEnable();

        /// <summary>
        /// Handles camera controller disable event.
        /// </summary>
        void OnDisable() => inputControlManager.OnDisable();

        /// <summary>
        /// Switches to camera behaviour in the list defined by index.
        /// Releases the current behaviour if necessary and initializes the new one.
        /// <param name="index">Index of desired behaviour.</param>
        /// </summary>
        public void SetBehaviour(int index, Transform target)
        {
            if (index >= availabeBehaviours.Length || index < 0)
                return;
            if (target != null)
                this.target = target;
            behaviourIndex = index;
            data.current.Update(transform);
            availabeBehaviours[behaviourIndex].Initialize(target);
        }

        /// <summary>
        /// Rotates to the next camera behaviour in the list.
        /// Releases the current behaviour if necessary and initializes the new one.
        /// </summary>
        public void RotateBehaviour()
        {
            if (behaviourIndex != -1)
                availabeBehaviours[behaviourIndex].Release();

            behaviourIndex++;
            if (behaviourIndex >= availabeBehaviours.Length)
                behaviourIndex = 0;

            data.current.Update(transform);
            if (!availabeBehaviours[behaviourIndex].Initialize(target))
                RotateBehaviour();
        }

        /// <summary>
        /// Called at the end of LateUpdate.
        /// Handles behaviour switching via Tab key and triggers behaviour completion callbacks.
        /// </summary>
        private void OnUpdateCompleted()
        {
            availabeBehaviours[behaviourIndex].OnUpdateCompleted();
            transform.position = data.next.position;
            transform.rotation = data.next.rotation;
        }

        /// <summary>
        /// Called at the beginning of LateUpdate to allow behaviour-specific pre-update logic.
        /// </summary>
        private void OnUpdateBegin()
        {
            data.Update(transform);
            availabeBehaviours[behaviourIndex].OnUpdateBegin();
        }

        /// <summary>
        /// Updates forward and down rays for LOS and collision detection.
        /// Clamps the down ray height between reasonable bounds.
        /// </summary>
        private void UpdateRays()
        {
            configuration.forwardRay.Cast(transform.position, transform.forward);
            configuration.downRay.Cast(transform.position, -Vector3.up);
            configuration.downRay.ClampHeight(0f, 1000f);
            configuration.groundRay.Cast(data.origin, -Vector3.up);
        }

        /// <summary>
        /// Reads player input for movement, zoom, and rotation.
        /// Mouse and keyboard inputs are handled and stored in CameraData.
        /// </summary>
        private void HandleInput()
        {
            inputControlManager.Move(data);
            inputControlManager.Zoom(data);
            inputControlManager.Rotate(data);
            if (useInputBehaviourRotation)
                inputControlManager.Control(this);
        }
    }
}
