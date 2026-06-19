using UnityEngine;

namespace Pihkura.Camera.Utils
{
    /// <summary>
    /// Utility class for casting rays from the camera and storing the hit point.
    /// Supports maximum distance, layer mask filtering, and clamping the resulting point's height.
    /// </summary>
    [System.Serializable]
    public class CameraRay
    {
        private Ray ray;
        private RaycastHit hit;

        /// <summary>
        /// Layer mask used when performing the raycast.
        /// </summary>
        public LayerMask mask;

        /// <summary>
        /// Maximum distance for the raycast.
        /// </summary>
        public float maxDistance;

        /// <summary>
        /// Offset that will be applied before ray is cast.
        /// </summary>
        public Vector3 offset;

        /// <summary>
        /// The point where the ray hits a collider, or the point at maxDistance if no hit occurs.
        /// </summary>
        public Vector3 Point { get; set; }

        /// <summary>
        /// True if the ray hit a collider, false otherwise.
        /// </summary>
        public bool IsHit { get; set; }

        /// <summary>
        /// Constructs a CameraRay with a given origin, direction, and maximum distance.
        /// </summary>
        /// <param name="origin">Origin of the ray.</param>
        /// <param name="direction">Direction of the ray.</param>
        /// <param name="maxDistance">Maximum distance to check for collisions.</param>
        public CameraRay(Vector3 origin, Vector3 direction, float maxDistance)
        {
            this.maxDistance = maxDistance;
            this.ray = new Ray(origin + offset, direction);
        }

        /// <summary>
        /// Casts the ray from the specified origin in the given direction.
        /// Updates the <see cref="Point"/> and <see cref="IsHit"/> properties.
        /// </summary>
        /// <param name="origin">Ray origin.</param>
        /// <param name="direction">Ray direction.</param>
        /// <returns>The hit point or the point at max distance if no collision occurs.</returns>
        public Vector3 Cast(Vector3 origin, Vector3 direction)
        {
            ray.origin = origin + offset;
            ray.direction = direction;

            if (Physics.Raycast(ray, out hit, maxDistance, mask))
            {
                Point = hit.point;
                IsHit = true;
            }
            else
            {
                Point = ray.GetPoint(maxDistance);
                IsHit = false;
            }

            return Point;
        }

        /// <summary>
        /// Clamps the Y-coordinate of the ray's hit point between the given minimum and maximum values.
        /// </summary>
        /// <param name="min">Minimum Y value.</param>
        /// <param name="max">Maximum Y value.</param>
        public void ClampHeight(float min, float max)
        {
            Vector3 clamped = Point;
            clamped.y = Mathf.Clamp(clamped.y, min, max);
            Point = clamped;
        }
    }
}
