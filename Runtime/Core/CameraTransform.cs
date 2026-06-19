using UnityEngine;

namespace Pihkura.Camera.Core
{
    /// <summary>
    /// Serializable container for camera transform data. 
    /// Can be saved/loaded (e.g., to JSON) instead of using Unity's Transform directly.
    /// </summary>
    [System.Serializable]
    public class CameraTransform
    {
        /// <summary>
        /// Camera world position.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Camera world rotation (Quaternion is serializable by Unity).
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        /// Camera euler angles.
        /// </summary>
        public Vector3 eulerAngles;

        /// <summary>
        /// Forward vector in world space.
        /// </summary>
        public Vector3 forward;

        /// <summary>
        /// Right vector in world space.
        /// </summary>
        public Vector3 right;

        /// <summary>
        /// Updates this data snapshot from a Unity Transform.
        /// </summary>
        /// <param name="transform">The Unity Transform to read from.</param>
        public void Update(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            eulerAngles = transform.eulerAngles;
            forward = transform.forward;
            right = transform.right;
        }

        /// <summary>
        /// Applies this stored transform data to a Unity Transform.
        /// </summary>
        /// <param name="transform">The Unity Transform to update.</param>
        public void ApplyTo(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
