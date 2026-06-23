using UnityEngine;
using Pihkura.Camera.Utils;
using Pihkura.Camera.Core;

namespace Pihkura.Camera.Behaviour
{

    public class FollowingCameraBehaviour : BaseCameraBehaviour
    {

        public FollowingCameraBehaviour(CameraConfiguration configuration, CameraData data)
            : base(configuration, data) { }

        /// <inheritdoc/>
        public override bool Initialize(Transform target)
        {
            if (target == null)
                return false;            
            configuration.heightOffset = 2f;
            return base.Initialize(target);
        }

        /// <inheritdoc/>
        public override void HandleMovement(float dt)
        {

            data.effectivePitch = data.pitch + configuration.autoPitch;
            data.origin = data.target.position;

            Quaternion rotation = Quaternion.Euler(data.effectivePitch, data.yaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -data.distance);
            Vector3 desiredPosition = data.origin + offset;

            CameraUtils.HandleLOSCorrection(configuration, data, ref desiredPosition, ref offset, ref rotation, moving);
            CameraUtils.HandleCameraCollision(configuration, data, ref desiredPosition, out _);

            if (Vector3.Distance(configuration.forwardRay.Point, data.target.position) > 10f)
            {
                Vector3 position = Vector3.MoveTowards(data.current.position, desiredPosition + (Vector3.up * configuration.heightOffset), dt * configuration.movementSpeed * data.speedRatio);
                data.next.position = position;
            }
            else
            {
                Vector3 position = Vector3.SmoothDamp(data.current.position, desiredPosition + (Vector3.up * configuration.heightOffset), ref data.moveVelocity, configuration.moveSmoothTime, float.PositiveInfinity, dt);
                data.next.position = position;
            }
            float t = 1f - Mathf.Exp(-dt / Mathf.Max(configuration.rotSmoothTime, 0.0001f));
            data.next.rotation = Quaternion.Slerp(data.current.rotation, rotation, t);
        }
    }
}