using UnityEngine;
using Pihkura.Camera.Core;

namespace Pihkura.Camera.Behaviour
{
    /// <summary>
    /// Base class for all camera behaviours.
    /// Provides common functionality such as rotation, zoom and smoothing.
    /// Concrete behaviours (e.g., RTS or Following) should override HandleMovement
    /// to implement specific movement logic.
    /// </summary>
    public abstract class BaseCameraBehaviour : ICameraBehaviour
    {
        /// <summary>
        /// Configuration values defining limits, speeds, and input state.
        /// </summary>
        protected CameraConfiguration configuration;

        /// <summary>
        /// Shared runtime state data (yaw, pitch, velocities, etc.).
        /// </summary>
        protected CameraData data;

        /// <summary>
        /// Flag indicating if camera is moving.
        /// </summary>
        protected bool moving;

        /// <summary>
        /// Creates a new base camera behaviour.
        /// </summary>
        /// <param name="transform">The camera transform to control.</param>
        /// <param name="configuration">The configuration containing input and settings.</param>
        /// <param name="data">The runtime state data for this behaviour.</param>
        public BaseCameraBehaviour(CameraConfiguration configuration, CameraData data)
        {
            this.configuration = configuration;
            this.data = data;
        }

        /// <summary>
        /// Initializes yaw and pitch values based on the current camera orientation.
        /// Should be called once before the first update.
        /// </summary>
        /// <param name="target">The target transform (if applicable).</param>
        public virtual bool Initialize(Transform target)
        {
            this.data.target = target;
            this.data.yaw = this.data.targetYaw = this.data.current.eulerAngles.y;
            this.data.pitch = this.data.targetPitch = this.data.current.eulerAngles.x;
            this.data.effectivePitch = this.data.pitch;
            this.data.yawVelocity = 0f;
            this.data.pitchVelocity = 0f;
            return true;
        }

        /// <summary>
        /// Handles camera rotation logic using input and smoothing.
        /// Can be overridden if behaviour needs custom rotation handling.
        /// </summary>
        public virtual void HandleRotation(float dt)
        {
            if (data.rotationInputX != 0f || data.rotationInputY != 0f)
            {
                if (Mathf.Abs(data.rotationInputY) > 0.0001f)
                {
                    data.targetYaw += data.rotationInputY * configuration.yawSpeed * dt;
                    this.moving = true;
                }
                if (Mathf.Abs(data.rotationInputX) > 0.0001f)
                {
                    data.targetPitch -= data.rotationInputX * configuration.pitchSpeed * dt;
                    data.targetPitch = Mathf.Clamp(data.targetPitch, configuration.minPitch, configuration.maxPitch);
                    this.moving = true;
                }
            }

            // Smoothly interpolate yaw and pitch to prevent sudden jumps
            data.yaw = Mathf.SmoothDampAngle(
                data.yaw,
                data.targetYaw,
                ref data.yawVelocity,
                configuration.rotSmoothTime,
                float.PositiveInfinity,
                dt
            );

            data.pitch = Mathf.SmoothDampAngle(
                data.pitch,
                data.targetPitch,
                ref data.pitchVelocity,
                configuration.rotSmoothTime,
                float.PositiveInfinity,
                dt
            );
        }

        /// <summary>
        /// Handles camera zoom using scroll wheel or other configured input.
        /// Adjusts camera distance within min/max bounds.
        /// </summary>
        public virtual void HandleZoom(float dt)
        {
            if (Mathf.Abs(data.zoomInput) > 0.0001f)
            {
                data.distance -= data.zoomInput * configuration.zoomSpeed * data.speedRatio;
                data.distance = Mathf.Clamp(data.distance, configuration.minDistance, configuration.maxDistance);
                this.moving = true;
            }
        }

        /// <summary>
        /// Must be implemented in concrete behaviours.
        /// Defines how the camera moves in the world (e.g., follow target, RTS free move).
        /// </summary>
        public abstract void HandleMovement(float dt);

        /// <summary>
        /// Called at the beginning of the update cycle.
        /// Override to insert behaviour-specific pre-update logic.
        /// </summary>
        public virtual void OnUpdateBegin()
        {
            this.data.speedRatio = this.configuration.GetDistanceRatio(this.data.current.position, this.data.origin);
            this.moving = false;
        }

        /// <summary>
        /// Called at the end of the update cycle.
        /// Override to insert behaviour-specific post-update logic.
        /// </summary>
        public virtual void OnUpdateCompleted() { }

        /// <summary>
        /// Called when the behaviour is released or deactivated.
        /// Use to clean up or reset data.
        /// </summary>
        public virtual void Release() { }
    }
}
