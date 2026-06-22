using System.Collections;
using UnityEngine;

namespace Aeroflux
{
    /// <summary>
    /// Brings the car to the user. On a press it places the car a comfortable
    /// distance in front of the headset, at a sensible height, turned to face
    /// the viewer. Handy on HoloLens where the car can otherwise end up behind
    /// you or across the room.
    /// </summary>
    [DisallowMultipleComponent]
    public class CarSummoner : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The car (or its container) to reposition. Defaults to this transform.")]
        [SerializeField] private Transform target;

        [Tooltip("Camera the car is summoned in front of. Defaults to Camera.main.")]
        [SerializeField] private Transform viewer;

        [Header("Placement")]
        [Tooltip("Metres in front of the viewer to place the car.")]
        [SerializeField, Min(0.2f)] private float distance = 1.2f;

        [Tooltip("Vertical offset from eye level (negative drops it toward a tabletop height).")]
        [SerializeField] private float heightOffset = -0.35f;

        [Tooltip("If true the car only yaws to face you and stays level (no tilting up/down).")]
        [SerializeField] private bool keepUpright = true;

        [Header("Animation")]
        [Tooltip("Seconds to glide into place. 0 snaps instantly.")]
        [SerializeField, Min(0f)] private float travelTime = 0.5f;

        private Coroutine _travel;

        private void Awake()
        {
            if (target == null) target = transform;
        }

        /// <summary>Place the car in front of the viewer, animating into position.</summary>
        public void SummonInFront()
        {
            Transform cam = ResolveViewer();
            if (cam == null || target == null) return;

            Vector3 forward = cam.forward;
            if (keepUpright)
            {
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
                forward.Normalize();
            }

            Vector3 destination = cam.position + forward * distance + Vector3.up * heightOffset;

            // Face the viewer: look back along the approach direction.
            Vector3 lookDir = keepUpright ? -forward : (cam.position - destination).normalized;
            Quaternion destRotation = Quaternion.LookRotation(lookDir, Vector3.up);

            if (_travel != null) StopCoroutine(_travel);
            if (travelTime <= 0f)
            {
                target.SetPositionAndRotation(destination, destRotation);
            }
            else
            {
                _travel = StartCoroutine(Glide(destination, destRotation));
            }
        }

        private IEnumerator Glide(Vector3 destination, Quaternion destRotation)
        {
            Vector3 startPos = target.position;
            Quaternion startRot = target.rotation;
            float elapsed = 0f;

            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / travelTime));
                target.SetPositionAndRotation(
                    Vector3.Lerp(startPos, destination, t),
                    Quaternion.Slerp(startRot, destRotation, t));
                yield return null;
            }

            target.SetPositionAndRotation(destination, destRotation);
            _travel = null;
        }

        private Transform ResolveViewer()
        {
            if (viewer != null) return viewer;
            if (Camera.main != null) return Camera.main.transform;
            return null;
        }
    }
}
