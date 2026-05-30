using UnityEngine;
using Pihkura.Camera.Utils;
using Pihkura.Camera.Core;

namespace Pihkura.Camera.Behaviour
{
    /// <summary>
    /// A free-flying camera behaviour.
    /// Allows full 3D movement and rotation without being locked to a target.
    /// Useful for debugging, cinematic shots, or level inspection.
    /// </summary>
    public class FreeCameraBehaviour : BaseCameraBehaviour
    {
        public FreeCameraBehaviour(CameraConfiguration configuration, CameraData data)
            : base(configuration, data) { }

        public override bool Initialize(Transform target)
        {
            // Initialize rotation values based on current camera transform
            data.yaw = data.current.eulerAngles.y;
            data.pitch = data.current.eulerAngles.x;
            data.targetYaw = data.yaw;
            data.targetPitch = data.pitch;
            return base.Initialize(target);
        }

        public override void HandleMovement(float dt)
        {
            Vector3 move = Vector3.zero;

            if (Mathf.Abs(data.movementInputY) > 0.0001f)
                move += this.configuration.movementSpeed * this.data.speedRatio * this.data.movementInputY * this.data.current.forward.normalized * dt;
            if (Mathf.Abs(data.movementInputX) > 0.0001f)
                move += this.configuration.movementSpeed * this.data.speedRatio * this.data.movementInputX * this.data.current.right.normalized * dt;

            Vector3 desiredPosition = data.current.position + move;

            // Optional: clamp to area bounds if defined
            desiredPosition = configuration.GetBoundedPosition(ref desiredPosition);
            if (desiredPosition.y - this.configuration.groundRay.Point.y > this.configuration.maxDistance)
                desiredPosition.y = this.configuration.groundRay.Point.y + this.configuration.maxDistance;

            // Smooth movement
            data.next.position = desiredPosition; // Vector3.SmoothDamp(data.current.position, desiredPosition, ref data.moveVelocity, configuration.moveSmoothTime, float.PositiveInfinity, dt);
            CameraUtils.HandleCameraCollision(configuration, data, ref data.next.position, out bool isCollision);
        }

        public override void HandleRotation(float dt)
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
                    // data.targetPitch = Mathf.Clamp(data.targetPitch, configuration.minPitch, configuration.maxPitch); // We wont be clamping in free camera mode.
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
            data.next.rotation = Quaternion.Euler(data.pitch, data.yaw, 0f);
        }

        public override void HandleZoom(float dt)
        {
        }

        public override void OnUpdateBegin()
        {
            this.data.origin = this.configuration.downRay.Point;
            this.data.speedRatio = this.configuration.GetDistanceRatio(this.data.current.position, this.data.origin);
        }

        public override void OnUpdateCompleted() { }

        public override void Release() { }
    }
}
