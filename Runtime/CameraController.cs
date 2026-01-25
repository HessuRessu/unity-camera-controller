using UnityEngine;
using Pihkura.Camera.Behaviour;
using Pihkura.Camera.Control;
using Pihkura.Camera.Core;
using UnityEngine.InputSystem;

namespace Pihkura.Camera
{
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
        [System.NonSerialized] public float dt;



        /// <summary>
        /// Ensures all required variables are set.
        /// </summary>
        private void EnsureInternalAssets()
        {
#if (ENABLE_INPUT_SYSTEM)
            if (this.configuration == null)
                this.configuration = new();
            if (this.configuration.inputMap == null)
                this.configuration.inputMap = AssetResolver.LoadAsset<InputActionAsset>("CameraInputAsset");
#endif
        }

        /// <summary>
        /// Unity On Validate callback: ensures all internal variables are set.
        /// </summary>
        void OnValidate()
            => this.EnsureInternalAssets();


        /// <summary>
        /// Initializes the camera controller instance for singleton object.
        /// </summary>
        private void Awake()
        {
            this.inputControlManager = BaseInputControlManager.Initialize(this.configuration);
            this.EnsureInternalAssets();

            if (!useSingleton)
                return;
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        /// <summary>
        /// Initializes the camera controller and sets up behaviours and rays.
        /// </summary>
        void Start()
        {
            this.data = new CameraData(0, 30);
            this.availabeBehaviours = new ICameraBehaviour[4] {
                new RTSCameraBehaviour(this.configuration, this.data),
                new FollowingCameraBehaviour(this.configuration, this.data),
                new FreeCameraBehaviour(this.configuration, this.data),
                new CinematicCameraBehaviour(this.configuration, this.data)
            };
            this.data.Update(this.transform);
            this.UpdateRays();
            this.SetBehaviour(this.behaviourIndex, null);
        }

        /// <summary>
        /// Set camera position and updates rays.
        /// </summary>
        public void SetPosition(Vector3 position, float distance = 10f)
        {
            this.transform.position = position;
            this.UpdateRays();
            this.data.distance = distance;
            Vector3 offsetPosition = this.transform.position;
            this.availabeBehaviours[this.behaviourIndex].Initialize(this.target);
            offsetPosition.y = this.configuration.groundRay.Point.y + this.configuration.minDistance;
            this.transform.position = offsetPosition;
        }

        /// <summary>
        /// Updates the camera each frame after all other updates.
        /// Handles input, zoom, rotation, movement, and behaviour updates.
        /// </summary>
        void LateUpdate()
        {
            this.dt = Time.unscaledDeltaTime;
            this.OnUpdateBegin();
            this.UpdateRays();
            this.HandleInput();

            this.availabeBehaviours[this.behaviourIndex].HandleZoom(this.dt);
            this.availabeBehaviours[this.behaviourIndex].HandleRotation(this.dt);
            this.availabeBehaviours[this.behaviourIndex].HandleMovement(this.dt);

            this.OnUpdateCompleted();
        }

        /// <summary>
        /// Handles camera controller enable event.
        /// </summary>
        void OnEnable() => this.inputControlManager.OnEnable();

        /// <summary>
        /// Handles camera controller disable event.
        /// </summary>
        void OnDisable() => this.inputControlManager.OnDisable();

        /// <summary>
        /// Switches to camera behaviour in the list defined by index.
        /// Releases the current behaviour if necessary and initializes the new one.
        /// <param name="index">Index of desired behaviour.</param>
        /// </summary>
        public void SetBehaviour(int index, Transform target)
        {
            if (index >= this.availabeBehaviours.Length || index < 0)
                return;
            if (target != null)
                this.target = target;
            this.behaviourIndex = index;
            this.data.current.Update(this.transform);
            this.availabeBehaviours[this.behaviourIndex].Initialize(this.target);
        }

        /// <summary>
        /// Rotates to the next camera behaviour in the list.
        /// Releases the current behaviour if necessary and initializes the new one.
        /// </summary>
        public void RotateBehaviour()
        {
            if (this.behaviourIndex != -1)
                this.availabeBehaviours[this.behaviourIndex].Release();

            this.behaviourIndex++;
            if (this.behaviourIndex >= this.availabeBehaviours.Length)
                this.behaviourIndex = 0;

            this.data.current.Update(this.transform);
            if (!this.availabeBehaviours[this.behaviourIndex].Initialize(this.target))
                this.RotateBehaviour();
        }

        /// <summary>
        /// Called at the end of LateUpdate.
        /// Handles behaviour switching via Tab key and triggers behaviour completion callbacks.
        /// </summary>
        private void OnUpdateCompleted()
        {
            this.availabeBehaviours[this.behaviourIndex].OnUpdateCompleted();
            this.transform.position = this.data.next.position;
            this.transform.rotation = this.data.next.rotation;
        }

        /// <summary>
        /// Called at the beginning of LateUpdate to allow behaviour-specific pre-update logic.
        /// </summary>
        private void OnUpdateBegin()
        {
            this.data.Update(this.transform);
            this.availabeBehaviours[this.behaviourIndex].OnUpdateBegin();
        }

        /// <summary>
        /// Updates forward and down rays for LOS and collision detection.
        /// Clamps the down ray height between reasonable bounds.
        /// </summary>
        private void UpdateRays()
        {
            this.configuration.forwardRay.Cast(this.transform.position, this.transform.forward);
            this.configuration.downRay.Cast(this.transform.position, -Vector3.up);
            this.configuration.downRay.ClampHeight(0f, 1000f);
            this.configuration.groundRay.Cast(this.data.origin, -Vector3.up);
        }

        /// <summary>
        /// Reads player input for movement, zoom, and rotation.
        /// Mouse and keyboard inputs are handled and stored in CameraData.
        /// </summary>
        private void HandleInput()
        {

            this.inputControlManager.Move(this.data);
            this.inputControlManager.Zoom(this.data);
            this.inputControlManager.Rotate(this.data);
            if (this.useInputBehaviourRotation)
                this.inputControlManager.Control(this);
        }
    }
}