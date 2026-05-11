using UnityEngine;
using Pihkura.Camera.Utils;
using Pihkura.Camera.Core;

namespace Pihkura.Camera.Behaviour
{
    /// <summary>
    /// A cinematic camera behaviour that smoothly orbits around the target
    /// with automatic position and rotation interpolation. Intended for cutscenes
    /// or special moments where manual player input is disabled.
    /// </summary>
    public class CinematicCameraBehaviour : BaseCameraBehaviour
    {
        private Vector3 desiredPosition;
        private Quaternion desiredRotation;
        private float orbitAngle = 0f;

        public CinematicCameraBehaviour(CameraConfiguration configuration, CameraData data)
            : base(configuration, data)
        {
        }

        public override bool Initialize(Transform target)
        {
            if (target == null)
                return false;

            // Start aligned with current camera position
            desiredPosition = target.position + new Vector3(0f, data.distance, -data.distance);
            desiredRotation = Quaternion.LookRotation(target.position - desiredPosition);
            return base.Initialize(target);
        }

        public override void HandleMovement(float dt)
        {
            // Slowly orbit around target
            orbitAngle += dt * 10f; // degrees per second
            float radians = orbitAngle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(radians) * data.distance,
                data.distance * 0.5f, // keep some height
                Mathf.Sin(radians) * data.distance
            );

            desiredPosition = data.target.position + offset;
            desiredRotation = Quaternion.LookRotation(data.target.position - desiredPosition);

            // Smooth interpolation
            Vector3 smoothedPos = Vector3.Lerp(data.current.position, desiredPosition, dt);
            Quaternion smoothedRot = Quaternion.Slerp(data.current.rotation, desiredRotation, dt);
            CameraUtils.HandleCameraCollision(configuration, data, ref smoothedPos);

            // Instead of writing to transform directly, update CameraData
            data.next.position = smoothedPos;
            data.next.rotation = smoothedRot;
            data.next.forward = smoothedRot * Vector3.forward;
            data.next.right = smoothedRot * Vector3.right;
        }

        public override void HandleRotation(float dt)
        {
            // Rotation handled in HandleMovement
        }
    }
}
