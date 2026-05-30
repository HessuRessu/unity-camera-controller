using UnityEngine;
using Pihkura.Camera.Core;

namespace Pihkura.Camera.Utils
{
    /// <summary>
    /// Collection of utility methods for camera control, including input handling,
    /// line-of-sight (LOS) correction, collision adjustment, and helper extensions.
    /// </summary>
    public static class CameraUtils
    {
        /// <summary>
        /// Determines whether a float value is within a specified inclusive range.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="min">Minimum bound.</param>
        /// <param name="max">Maximum bound.</param>
        /// <returns>True if value is between min and max, inclusive; otherwise false.</returns>
        public static bool Between(this float value, float min, float max) => value >= min && value <= max;

        /// <summary>
        /// Input raw axis value and applies smoothing with sensitivity and gravity.
        /// If the input is zero, the value decays towards zero based on gravity.
        /// </summary>
        /// <param name="rawValue">The raw value of input.</param>
        /// <param name="value">Reference to the current axis value to update.</param>
        /// <param name="sensitivity">Multiplier applied to input when active.</param>
        /// <param name="gravity">Rate at which the value decays to zero when no input is present.</param>
        public static void SmoothInput(float rawValue, ref float value, float sensitivity = 1f, float gravity = 6f)
        {
            if (rawValue == 0)
            {
                value = Mathf.Clamp01(Mathf.Abs(value) - gravity * Time.unscaledDeltaTime) * Mathf.Sign(value);
            }
            else
            {
                value = rawValue * sensitivity;
            }
        }

        /// <summary>
        /// Performs Line-of-Sight (LOS) correction for the camera.
        /// If the direct line from the camera origin to the desired position
        /// is blocked (e.g., by terrain), the camera's pitch is automatically
        /// raised to maintain visibility of the target. Once LOS is clear,
        /// the pitch gradually returns to its original value after a configurable delay.
        /// </summary>
        /// <param name="configuration">Camera configuration containing speeds, limits, and collision mask.</param>
        /// <param name="data">Camera runtime state containing pitch, yaw, LOS timer, etc.</param>
        /// <param name="desiredPosition">Reference to the desired camera position to be adjusted.</param>
        /// <param name="offset">Reference to the camera offset vector (from target) to be updated.</param>
        /// <param name="rotation">Reference to the camera rotation quaternion to be updated.</param>
        /// <param name="losRelaxDelay">Time in seconds to wait before lowering the camera after LOS is cleared.</param>
        public static void HandleLOSCorrection(
            CameraConfiguration configuration,
            CameraData data,
            ref Vector3 desiredPosition,
            ref Vector3 offset,
            ref Quaternion rotation,
            bool moving,
            float losRelaxDelay = 0.3f)
        {
            // --- Check Line-of-Sight ---
            if (Physics.Linecast(data.origin, desiredPosition, out RaycastHit hit, configuration.collisionMask))
            {
                // LOS blocked → raise autoPitch
                configuration.autoPitch = Mathf.MoveTowards(
                    configuration.autoPitch,
                    configuration.maxAutoPitch,
                    configuration.autoPitchSpeed * Time.unscaledDeltaTime
                );

                // Reset timer while LOS is blocked
                data.losTimer = 0f;

                // Update effective pitch, rotation, and offset
                data.effectivePitch = data.pitch + configuration.autoPitch;
                rotation = Quaternion.Euler(data.effectivePitch, data.yaw, 0f);
                offset = rotation * new Vector3(0f, 0f, -data.distance);
                desiredPosition = data.origin + offset;
            }
            else if (moving)
            {
                // LOS clear → increase timer
                data.losTimer += Time.unscaledDeltaTime;

                // If timer exceeds the relax delay, lower autoPitch gradually
                if (data.losTimer >= losRelaxDelay)
                {
                    configuration.autoPitch = Mathf.MoveTowards(
                        configuration.autoPitch,
                        0f,
                        configuration.autoPitchSpeed * Time.unscaledDeltaTime
                    );

                    // Update effective pitch, rotation, and offset
                    data.effectivePitch = data.pitch + configuration.autoPitch;
                    rotation = Quaternion.Euler(data.effectivePitch, data.yaw, 0f);
                    offset = rotation * new Vector3(0f, 0f, -data.distance);
                    desiredPosition = data.origin + offset;
                }
            }
        }

        /// <summary>
        /// Adjusts the camera position to prevent it from going below the terrain or collider.
        /// Adds a vertical offset above the ground based on the configuration.
        /// </summary>
        /// <param name="configuration">Camera configuration containing collision offset and downRay.</param>
        /// <param name="position">Reference to the camera position to adjust.</param>
        public static void HandleCameraCollision(CameraConfiguration configuration, CameraData data, ref Vector3 position, out bool isCollision)
        {
            isCollision = position.y < configuration.downRay.Point.y;
            if (position.y < configuration.downRay.Point.y + configuration.collisionOffset)
            {
                position.y = configuration.downRay.Point.y + configuration.collisionOffset;
            }
        }
    }
}
