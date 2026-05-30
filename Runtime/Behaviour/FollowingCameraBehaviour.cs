using UnityEngine;
using Pihkura.Camera.Utils;
using Pihkura.Camera.Core;

namespace Pihkura.Camera.Behaviour
{

    public class FollowingCameraBehaviour : BaseCameraBehaviour
    {

        public FollowingCameraBehaviour(CameraConfiguration configuration, CameraData data)
            : base(configuration, data) { }

        public override bool Initialize(Transform target)
        {
            if (target == null)
                return false;            
            this.configuration.heightOffset = 2f;
            return base.Initialize(target);
        }

        public override void HandleMovement(float dt)
        {

            data.effectivePitch = this.data.pitch + this.configuration.autoPitch;
            this.data.origin = this.data.target.position;

            Quaternion rotation = Quaternion.Euler(data.effectivePitch, this.data.yaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -this.data.distance);
            Vector3 desiredPosition = this.data.origin + offset;

            CameraUtils.HandleLOSCorrection(configuration, data, ref desiredPosition, ref offset, ref rotation, this.moving);
            CameraUtils.HandleCameraCollision(configuration, data, ref desiredPosition, out bool isCollision);

            if (Vector3.Distance(this.configuration.forwardRay.Point, this.data.target.position) > 10f)
            {
                Vector3 position = Vector3.MoveTowards(this.data.current.position, desiredPosition + (Vector3.up * this.configuration.heightOffset), dt * 400f);
                this.data.next.position = position;
            }
            else
            {
                Vector3 position = Vector3.SmoothDamp(this.data.current.position, desiredPosition + (Vector3.up * this.configuration.heightOffset), ref this.data.moveVelocity, this.configuration.moveSmoothTime, float.PositiveInfinity, dt);
                this.data.next.position = position;
            }
            float t = 1f - Mathf.Exp(-dt / Mathf.Max(this.configuration.rotSmoothTime, 0.0001f));
            this.data.next.rotation = Quaternion.Slerp(this.data.current.rotation, rotation, t);
        }
    }
}