using UnityEngine;
using Pihkura.Camera.Utils;
using Pihkura.Camera.Core;

namespace Pihkura.Camera.Behaviour
{

    public class RTSCameraBehaviour : BaseCameraBehaviour
    {

        public RTSCameraBehaviour(CameraConfiguration configuration, CameraData data)
            : base(configuration, data) { }


        public override bool Initialize(Transform target)
        {
            this.configuration.heightOffset = 0f;
            this.data.origin = this.configuration.forwardRay.Point;
            return base.Initialize(target);
        }

        public override void HandleMovement(float dt)
        {
            this.data.effectivePitch = this.data.pitch + this.configuration.autoPitch;

            Quaternion rotation = Quaternion.Euler(this.data.effectivePitch, this.data.yaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -this.data.distance);

            if (this.data.movementInputY != 0f)
            {
                this.data.origin += this.configuration.movementSpeed * this.data.speedRatio * this.data.movementInputY * new Vector3(this.data.current.forward.x, 0f, this.data.current.forward.z).normalized * dt;
                this.moving = true;
            }
            if (this.data.movementInputX != 0f)
            {
                this.data.origin += this.configuration.movementSpeed * this.data.speedRatio * this.data.movementInputX * this.data.current.right.normalized * dt;
                this.moving = true;
            }

            // this.data.origin.y = Terrain.activeTerrain.SampleHeight(this.data.origin);
            this.data.origin.y = this.configuration.groundRay.Point.y;
            this.data.origin = this.configuration.GetBoundedPosition(ref this.data.origin);

            Vector3 desiredPosition = this.data.origin + offset;
            CameraUtils.HandleLOSCorrection(configuration, data, ref desiredPosition, ref offset, ref rotation, this.moving);
            CameraUtils.HandleCameraCollision(configuration, data, ref desiredPosition, out bool isCollision);

            // --- Smooth movement & rotation ---
            this.data.next.position = Vector3.SmoothDamp(this.data.current.position, desiredPosition + (Vector3.up * this.configuration.heightOffset), ref this.data.moveVelocity, this.configuration.moveSmoothTime, float.PositiveInfinity, dt);

            float t = 1f - Mathf.Exp(-dt / Mathf.Max(this.configuration.rotSmoothTime, 0.0001f));
            this.data.next.rotation = Quaternion.Slerp(this.data.current.rotation, rotation, t);
        }
    }
}