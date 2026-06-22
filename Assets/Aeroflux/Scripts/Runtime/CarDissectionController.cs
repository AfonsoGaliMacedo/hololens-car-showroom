using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aeroflux
{
    /// <summary>
    /// Handles the three "teardown" interactions used in the demo:
    ///
    ///   * <b>Exploded view</b> – every panel slides outward at once so you can
    ///     see how the whole car fits together (toggle).
    ///   * <b>Dissect one-by-one</b> – each press lifts the next single part out
    ///     to an inspection pose so it can be examined on its own.
    ///   * <b>Reassemble</b> – everything animates smoothly back home.
    ///
    /// The controller discovers its parts automatically from the mesh renderers
    /// under <see cref="carRoot"/>, so it works with any imported car model
    /// without hand-wiring each panel.
    /// </summary>
    [DisallowMultipleComponent]
    public class CarDissectionController : MonoBehaviour
    {
        [Header("Car")]
        [Tooltip("Root of the car model. If left empty the component's own transform is used.")]
        [SerializeField] private Transform carRoot;

        [Header("Exploded view")]
        [Tooltip("How far each part travels outward, scaled by its distance from the car centre.")]
        [SerializeField, Min(0f)] private float explodeStrength = 0.35f;

        [Header("Dissection")]
        [Tooltip("Distance a single part lifts away from the body when dissected.")]
        [SerializeField, Min(0f)] private float dissectLift = 0.25f;

        [Header("Animation")]
        [Tooltip("Seconds for a part to travel between two poses.")]
        [SerializeField, Min(0.01f)] private float animationDuration = 0.6f;
        [Tooltip("Extra delay per part so motion ripples out instead of snapping together.")]
        [SerializeField, Min(0f)] private float perPartStagger = 0.03f;

        private readonly List<CarPart> _parts = new List<CarPart>();
        private readonly List<Coroutine> _running = new List<Coroutine>();

        private bool _initialised;
        private int _dissectCursor;            // how many parts are currently lifted out
        private bool _isExploded;

        /// <summary>True while the exploded view is showing.</summary>
        public bool IsExploded => _isExploded;

        /// <summary>Number of parts the controller found under the car root.</summary>
        public int PartCount => _parts.Count;

        private void Awake()
        {
            if (carRoot == null) carRoot = transform;
            Initialise();
        }

        /// <summary>
        /// Scans the car hierarchy and caches a <see cref="CarPart"/> for every
        /// mesh renderer. Safe to call again if the model is swapped at runtime.
        /// </summary>
        public void Initialise()
        {
            _parts.Clear();
            if (carRoot == null) return;

            var renderers = carRoot.GetComponentsInChildren<MeshRenderer>(includeInactive: true);

            // World-space centre of the whole car, used to work out which way each
            // part should fly when exploded.
            Bounds carBounds = CalculateBounds(renderers);
            Vector3 centreWorld = carBounds.center;

            foreach (var renderer in renderers)
            {
                Transform t = renderer.transform;
                Vector3 partCentreWorld = renderer.bounds.center;
                Vector3 outwardWorld = partCentreWorld - centreWorld;

                // Convert the outward push into the car root's local space so it
                // survives the car being moved, rotated or rescaled.
                Vector3 outwardLocal = carRoot.InverseTransformDirection(outwardWorld);
                float radius = outwardLocal.magnitude;
                if (radius < 0.0001f)
                {
                    // Part sits dead centre – give it a gentle upward nudge so it
                    // still separates from the pack.
                    outwardLocal = Vector3.up;
                    radius = 0.01f;
                }

                _parts.Add(new CarPart(
                    t,
                    t.localPosition,
                    t.localRotation,
                    outwardLocal.normalized,
                    radius));
            }

            // Inspect parts furthest from the centre first – it reads more
            // naturally when you peel the car from the outside in.
            _parts.Sort((a, b) => b.RadiusFromCentre.CompareTo(a.RadiusFromCentre));

            _initialised = true;
            _dissectCursor = 0;
            _isExploded = false;
        }

        // ----------------------------------------------------------------- API

        /// <summary>Toggle the all-at-once exploded view on or off.</summary>
        public void ToggleExplodedView()
        {
            SetExplodedView(!_isExploded);
        }

        /// <summary>Show or hide the all-at-once exploded view.</summary>
        public void SetExplodedView(bool exploded)
        {
            if (!_initialised) Initialise();
            _isExploded = exploded;
            _dissectCursor = 0; // exploding overrides any partial dissection

            StopAllPartAnimations();
            for (int i = 0; i < _parts.Count; i++)
            {
                CarPart part = _parts[i];
                Vector3 target = exploded
                    ? part.HomeLocalPosition + part.OutwardDirection * (part.RadiusFromCentre * explodeStrength + explodeStrength * 0.25f)
                    : part.HomeLocalPosition;

                _running.Add(StartCoroutine(MovePart(part, target, part.HomeLocalRotation, i * perPartStagger)));
            }
        }

        /// <summary>
        /// Lift the next intact part out to an inspection pose. Call once per
        /// button press to walk through the car piece by piece.
        /// </summary>
        public void DissectNext()
        {
            if (!_initialised) Initialise();
            if (_isExploded) SetExplodedView(false);
            if (_dissectCursor >= _parts.Count) return;

            CarPart part = _parts[_dissectCursor];
            Vector3 target = part.HomeLocalPosition
                             + part.OutwardDirection * dissectLift
                             + Vector3.up * (dissectLift * 0.5f);

            _running.Add(StartCoroutine(MovePart(part, target, part.HomeLocalRotation, 0f)));
            _dissectCursor++;
        }

        /// <summary>Return the most recently dissected part to the body.</summary>
        public void DissectPrevious()
        {
            if (!_initialised || _dissectCursor <= 0) return;

            _dissectCursor--;
            CarPart part = _parts[_dissectCursor];
            _running.Add(StartCoroutine(MovePart(part, part.HomeLocalPosition, part.HomeLocalRotation, 0f)));
        }

        /// <summary>Animate every part back to its resting pose.</summary>
        public void ReassembleAll()
        {
            if (!_initialised) Initialise();
            _isExploded = false;
            _dissectCursor = 0;

            StopAllPartAnimations();
            for (int i = 0; i < _parts.Count; i++)
            {
                CarPart part = _parts[i];
                _running.Add(StartCoroutine(MovePart(part, part.HomeLocalPosition, part.HomeLocalRotation, i * perPartStagger)));
            }
        }

        // ------------------------------------------------------------- helpers

        private IEnumerator MovePart(CarPart part, Vector3 targetLocalPos, Quaternion targetLocalRot, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (part.Transform == null) yield break;

            Vector3 startPos = part.Transform.localPosition;
            Quaternion startRot = part.Transform.localRotation;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / animationDuration));
                if (part.Transform == null) yield break;
                part.Transform.localPosition = Vector3.LerpUnclamped(startPos, targetLocalPos, t);
                part.Transform.localRotation = Quaternion.SlerpUnclamped(startRot, targetLocalRot, t);
                yield return null;
            }

            if (part.Transform != null)
            {
                part.Transform.localPosition = targetLocalPos;
                part.Transform.localRotation = targetLocalRot;
            }
        }

        private void StopAllPartAnimations()
        {
            foreach (Coroutine c in _running)
            {
                if (c != null) StopCoroutine(c);
            }
            _running.Clear();
        }

        private static Bounds CalculateBounds(IReadOnlyList<MeshRenderer> renderers)
        {
            if (renderers == null || renderers.Count == 0) return new Bounds(Vector3.zero, Vector3.zero);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Count; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
    }
}
