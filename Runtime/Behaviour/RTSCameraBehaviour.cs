using UnityEngine;
using Pihkura.Camera.Utils;
using Pihkura.Camera.Core;

namespace Pihkura.Camera.Behaviour
{

    public class RTSCameraBehaviour : BaseCameraBehaviour
    {

        public RTSCameraBehaviour(CameraConfiguration configuration, CameraData data)
            : base(configuration, data) { }


        /// <inheritdoc/>
        public override bool Initialize(Transform target)
        {
            configuration.heightOffset = 0f;
            data.origin = configuration.forwardRay.Point;
            return base.Initialize(target);
        }

        /// <inheritdoc/>
        public override void HandleMovement(float dt)
        {
            data.effectivePitch = data.pitch + configuration.autoPitch;

            Quaternion rotation = Quaternion.Euler(data.effectivePitch, data.yaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -data.distance);

            if (data.movementInputY != 0f)
            {
                data.origin += configuration.movementSpeed * data.speedRatio * data.movementInputY * new Vector3(data.current.forward.x, 0f, data.current.forward.z).normalized * dt;
                moving = true;
            }
            if (data.movementInputX != 0f)
            {
                data.origin += configuration.movementSpeed * data.speedRatio * data.movementInputX * data.current.right.normalized * dt;
                moving = true;
            }

            // data.origin.y = Terrain.activeTerrain.SampleHeight(data.origin);
            data.origin.y = configuration.groundRay.Point.y;
            data.origin = configuration.GetBoundedPosition(ref data.origin);

            Vector3 desiredPosition = data.origin + offset;
            CameraUtils.HandleLOSCorrection(configuration, data, ref desiredPosition, ref offset, ref rotation, moving);
            CameraUtils.HandleCameraCollision(configuration, data, ref desiredPosition, out _);

            // --- Smooth movement & rotation ---
            data.next.position = Vector3.SmoothDamp(data.current.position, desiredPosition + (Vector3.up * configuration.heightOffset), ref data.moveVelocity, configuration.moveSmoothTime, float.PositiveInfinity, dt);

            float t = 1f - Mathf.Exp(-dt / Mathf.Max(configuration.rotSmoothTime, 0.0001f));
            data.next.rotation = Quaternion.Slerp(data.current.rotation, rotation, t);
        }
    }
}