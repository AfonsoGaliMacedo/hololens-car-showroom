using UnityEngine;

namespace Aeroflux
{
    /// <summary>
    /// Lightweight record describing a single dissectable piece of the car.
    /// One <see cref="CarPart"/> is created for every mesh renderer found beneath
    /// the car root when <see cref="CarDissectionController"/> initialises.
    ///
    /// All positions/rotations are stored in the car root's local space so the
    /// effects keep working no matter where the car is summoned or how it is
    /// scaled by the user.
    /// </summary>
    public class CarPart
    {
        /// <summary>The transform of the mesh piece itself.</summary>
        public readonly Transform Transform;

        /// <summary>Resting position (local to the car root) the part returns to.</summary>
        public readonly Vector3 HomeLocalPosition;

        /// <summary>Resting rotation (local to the car root).</summary>
        public readonly Quaternion HomeLocalRotation;

        /// <summary>
        /// Unit direction, in car-root local space, pointing from the car's
        /// centre toward this part. Used to push the part outward for the
        /// exploded and dissected views.
        /// </summary>
        public readonly Vector3 OutwardDirection;

        /// <summary>How far this part sits from the car centre, used to stagger motion.</summary>
        public readonly float RadiusFromCentre;

        public CarPart(Transform transform, Vector3 homeLocalPosition, Quaternion homeLocalRotation,
            Vector3 outwardDirection, float radiusFromCentre)
        {
            Transform = transform;
            HomeLocalPosition = homeLocalPosition;
            HomeLocalRotation = homeLocalRotation;
            OutwardDirection = outwardDirection;
            RadiusFromCentre = radiusFromCentre;
        }
    }
}
