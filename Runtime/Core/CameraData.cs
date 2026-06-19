using Pihkura.Camera.Utils;
using UnityEngine;

namespace Pihkura.Camera.Core
{

    /// <summary>
    /// Stores the runtime state of the camera such as yaw, pitch,
    /// smoothing velocities, and line-of-sight correction data.
    /// This class can be reused across multiple camera behaviours
    /// to maintain consistent state and reduce code duplication.
    /// </summary>
    public class CameraData
    {

        // --- Rotation state ---

        /// <summary>
        /// The current yaw angle (horizontal rotation around Y axis).
        /// </summary>
        public float yaw;

        /// <summary>
        /// The current pitch angle (vertical rotation).
        /// </summary>
        public float pitch;

        /// <summary>
        /// The target yaw angle to which the camera is interpolating.
        /// </summary>
        public float targetYaw;

        /// <summary>
        /// The target pitch angle to which the camera is interpolating.
        /// </summary>
        public float targetPitch;

        /// <summary>
        /// Velocity reference used internally by SmoothDampAngle for yaw.
        /// </summary>
        public float yawVelocity;

        /// <summary>
        /// Velocity reference used internally by SmoothDampAngle for pitch.
        /// </summary>
        public float pitchVelocity;

        /// <summary>
        /// Effective pitch applied to camera, including user input and LOS correction.
        /// </summary>
        public float effectivePitch;


        // --- Movement smoothing ---

        /// <summary>
        /// Velocity reference used internally by SmoothDamp for movement.
        /// </summary>
        public Vector3 moveVelocity;


        // --- Line of sight (LOS) correction ---

        /// <summary>
        /// The automatically applied extra pitch (in degrees) used to lift
        /// the camera when the line of sight to the target is blocked.
        /// </summary>
        public float autoPitch;

        /// <summary>
        /// A timer used to delay lowering the camera back down
        /// once the line of sight becomes clear again.
        /// </summary>
        public float losTimer;

        // --- Generic variables ---

        /// <summary>
        /// Current runtime camera transform data for behaviour classes.
        /// </summary>
        public CameraTransform current;

        /// <summary>
        /// Desired runtime camera transform data for behaviour classes.
        /// </summary>
        public CameraTransform next;


        /// <summary>
        /// The point that we are looking at.
        /// </summary>
        public Vector3 origin;

        /// <summary>
        /// Ratio which we will be using for controlling distance based speeds.
        /// </summary>
        public float speedRatio = 0f;

        /// <summary>
        /// Transform that we will be targeting.
        /// </summary>
        public Transform target;


        // --- Input handling ---

        /// <summary>
        /// Camera movement input - x axis
        /// </summary>
        public float movementInputX = 0f;

        /// <summary>
        /// Camera movement input - y axis
        /// </summary>
        public float movementInputY = 0f;

        /// <summary>
        /// Camera rotation input - x axis
        /// </summary>
        public float rotationInputX = 0f;

        /// <summary>
        /// Camera rotation input - y axis
        /// </summary>
        public float rotationInputY = 0f;

        /// <summary>
        /// Camera zoom input
        /// </summary>
        public float zoomInput = 0f;
        
        /// <summary>
        /// Current camera distance from the target.
        /// </summary>
        public float distance = 10f;

        // --- Constructors ---

        /// <summary>
        /// Initializes a new <see cref="CameraData"/> instance with given yaw and pitch.
        /// </summary>
        /// <param name="startYaw">Initial yaw angle in degrees.</param>
        /// <param name="startPitch">Initial pitch angle in degrees.</param>
        public CameraData(float startYaw = 0f, float startPitch = 30f)
        {
            yaw = targetYaw = startYaw;
            pitch = targetPitch = startPitch;
            current = new CameraTransform();
            next = new CameraTransform();
        }

        /// <summary>
        /// Updates current camera transform values based on Unity transform object.
        /// </summary>
        /// <param name="transform">The Unity Transform from which to update.</param>
        public void Update(Transform transform)
        {
            current.Update(transform);
        }
    }
}