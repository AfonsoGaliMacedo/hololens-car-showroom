using UnityEngine;

namespace Aeroflux
{
    /// <summary>
    /// Single entry point for the Aeroflux demo. Drop this on a GameObject,
    /// point <see cref="carRoot"/> at the car model, and press Play – it wires up
    /// the dissection, summon, environment and control-panel pieces for you.
    ///
    /// Every feature is also exposed as a public method, so the buttons can be
    /// re-bound to MRTK PressableButtons later if you prefer those over the
    /// generated panel.
    /// </summary>
    [DisallowMultipleComponent]
    public class AerofluxAppController : MonoBehaviour
    {
        [Header("Scene references")]
        [Tooltip("Root transform of the car model. Required.")]
        [SerializeField] private Transform carRoot;

        [Tooltip("Headset / main camera. Defaults to Camera.main when empty.")]
        [SerializeField] private Transform viewer;

        [Header("Generated control panel")]
        [Tooltip("Build the floating button panel automatically on start.")]
        [SerializeField] private bool buildControlPanel = true;

        [Tooltip("Where the panel floats relative to the car (metres, car-local-ish).")]
        [SerializeField] private Vector3 panelOffset = new Vector3(0.45f, 0.1f, 0f);

        [Header("Racing environment")]
        [Tooltip("Add the procedural racing backdrop component.")]
        [SerializeField] private bool enableEnvironment = true;

        private CarDissectionController _dissection;
        private CarSummoner _summoner;
        private RacingEnvironment _environment;
        private AerofluxControlPanel _panel;

        private void Start()
        {
            if (carRoot == null)
            {
                Debug.LogError("[Aeroflux] No car root assigned. Assign the car model to AerofluxAppController.", this);
                return;
            }

            if (viewer == null && Camera.main != null) viewer = Camera.main.transform;

            SetupComponents();
            if (buildControlPanel) BuildPanel();
        }

        private void SetupComponents()
        {
            _dissection = carRoot.GetComponent<CarDissectionController>();
            if (_dissection == null) _dissection = carRoot.gameObject.AddComponent<CarDissectionController>();

            _summoner = carRoot.GetComponent<CarSummoner>();
            if (_summoner == null) _summoner = carRoot.gameObject.AddComponent<CarSummoner>();

            if (enableEnvironment)
            {
                _environment = GetComponent<RacingEnvironment>();
                if (_environment == null) _environment = gameObject.AddComponent<RacingEnvironment>();
            }
        }

        private void BuildPanel()
        {
            _panel = AerofluxControlPanel.Create(transform, "AEROFLUX");

            _panel.AddButton("Summon Car", SummonCar);
            _panel.AddButton("Exploded View", ToggleExplodedView);
            _panel.AddButton("Dissect Next", DissectNext);
            _panel.AddButton("Reassemble", Reassemble);
            if (_environment != null) _panel.AddButton("Racing Mode", ToggleEnvironment);

            PositionPanel();
        }

        private void PositionPanel()
        {
            if (_panel == null || carRoot == null) return;
            var t = _panel.transform;
            t.position = carRoot.position + carRoot.right * panelOffset.x
                                          + Vector3.up * panelOffset.y
                                          + carRoot.forward * panelOffset.z;
            // Face the panel toward the viewer if we have one, else match the car.
            if (viewer != null)
            {
                Vector3 look = t.position - viewer.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.0001f) t.rotation = Quaternion.LookRotation(look, Vector3.up);
            }
            else
            {
                t.rotation = carRoot.rotation;
            }
        }

        // ----------------------------------------------- public button actions

        /// <summary>Bring the car in front of the user.</summary>
        public void SummonCar()
        {
            if (_summoner != null) _summoner.SummonInFront();
            PositionPanel();
        }

        /// <summary>Toggle the all-at-once exploded view.</summary>
        public void ToggleExplodedView()
        {
            if (_dissection != null) _dissection.ToggleExplodedView();
        }

        /// <summary>Lift the next single part out for inspection.</summary>
        public void DissectNext()
        {
            if (_dissection != null) _dissection.DissectNext();
        }

        /// <summary>Put every part back together.</summary>
        public void Reassemble()
        {
            if (_dissection != null) _dissection.ReassembleAll();
        }

        /// <summary>Show/hide the racing backdrop.</summary>
        public void ToggleEnvironment()
        {
            if (_environment != null) _environment.Toggle();
        }
    }
}
