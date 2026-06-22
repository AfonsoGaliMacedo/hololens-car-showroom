using System.Collections.Generic;
using UnityEngine;

namespace Aeroflux
{
    /// <summary>
    /// Procedurally builds a stylised racing scene to surround the car: an
    /// asphalt pad, red/white curbing, a checkered start–finish line and a pair
    /// of grandstands. Everything is generated in code (meshes + textures), so
    /// there are no extra assets to import and nothing to break on a fresh clone.
    ///
    /// On HoloLens you'll typically leave this hidden and toggle it on for a
    /// "showroom" backdrop; in the Editor or in VR it gives the scene a track feel.
    /// </summary>
    [DisallowMultipleComponent]
    public class RacingEnvironment : MonoBehaviour
    {
        [Header("Layout")]
        [Tooltip("Side length, in metres, of the square asphalt pad.")]
        [SerializeField, Min(2f)] private float padSize = 8f;

        [Tooltip("Width of the red/white curb ring around the pad.")]
        [SerializeField, Min(0.05f)] private float curbWidth = 0.4f;

        [Tooltip("Build grandstands along two sides of the pad.")]
        [SerializeField] private bool buildGrandstands = true;

        [Header("Mood")]
        [Tooltip("Tint applied to the ambient light when the environment is shown.")]
        [SerializeField] private Color ambientTint = new Color(0.55f, 0.6f, 0.7f);

        [Tooltip("Start the environment hidden (recommended for see-through AR).")]
        [SerializeField] private bool hiddenOnStart = true;

        private GameObject _root;
        private bool _built;
        private bool _visible;

        // Cached so we can restore the user's lighting when the backdrop is hidden.
        private Color _ambientBackup;
        private bool _ambientBackupTaken;

        public bool IsVisible => _visible;

        private void Start()
        {
            if (!hiddenOnStart)
            {
                SetVisible(true);
            }
        }

        /// <summary>Flip the racing backdrop on or off.</summary>
        public void Toggle() => SetVisible(!_visible);

        /// <summary>Show or hide the racing backdrop, building it on first use.</summary>
        public void SetVisible(bool visible)
        {
            if (visible && !_built) Build();
            if (_root != null) _root.SetActive(visible);
            _visible = visible;
            ApplyAmbient(visible);
        }

        // ---------------------------------------------------------- generation

        private void Build()
        {
            _root = new GameObject("Racing Environment (generated)");
            _root.transform.SetParent(transform, worldPositionStays: false);

            Material asphalt = MakeMaterial("Aeroflux/Asphalt", BuildAsphaltTexture(), new Color(0.16f, 0.16f, 0.17f), glossiness: 0.15f);
            Material lightCurb = MakeMaterial("Aeroflux/Curb", null, Color.white, glossiness: 0.2f);
            Material darkCurb = MakeMaterial("Aeroflux/CurbRed", null, new Color(0.7f, 0.08f, 0.08f), glossiness: 0.2f);
            Material standMat = MakeMaterial("Aeroflux/Stand", null, new Color(0.32f, 0.34f, 0.4f), glossiness: 0.05f);
            Material lineMat = MakeMaterial("Aeroflux/Checker", BuildCheckerTexture(), Color.white, glossiness: 0.1f);

            float half = padSize * 0.5f;

            // Asphalt pad.
            var pad = CreateBox("Asphalt Pad", _root.transform, new Vector3(padSize, 0.02f, padSize), new Vector3(0f, -0.01f, 0f), asphalt);
            TileTexture(pad, padSize * 0.5f);

            // Checkered start–finish strip across the front edge.
            CreateBox("Start-Finish Line", _root.transform,
                new Vector3(padSize, 0.022f, 0.6f),
                new Vector3(0f, 0.001f, -half + 1.2f), lineMat);

            // Curb ring (four striped rails framing the pad).
            BuildCurbRail("Curb North", new Vector3(0f, 0f, half), new Vector3(padSize, curbWidth), true, lightCurb, darkCurb);
            BuildCurbRail("Curb South", new Vector3(0f, 0f, -half), new Vector3(padSize, curbWidth), true, lightCurb, darkCurb);
            BuildCurbRail("Curb East", new Vector3(half, 0f, 0f), new Vector3(curbWidth, padSize), false, lightCurb, darkCurb);
            BuildCurbRail("Curb West", new Vector3(-half, 0f, 0f), new Vector3(curbWidth, padSize), false, lightCurb, darkCurb);

            if (buildGrandstands)
            {
                BuildGrandstand("Grandstand East", new Vector3(half + 1.4f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), standMat);
                BuildGrandstand("Grandstand West", new Vector3(-half - 1.4f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), standMat);
            }

            _built = true;
        }

        private void BuildCurbRail(string name, Vector3 centre, Vector2 size, bool stripeAlongX,
            Material light, Material dark)
        {
            var rail = new GameObject(name);
            rail.transform.SetParent(_root.transform, false);
            rail.transform.localPosition = centre;

            float length = stripeAlongX ? size.x : size.y;
            float depth = stripeAlongX ? size.y : size.x;
            const float stripe = 0.5f;
            int count = Mathf.Max(2, Mathf.RoundToInt(length / stripe));
            float actualStripe = length / count;

            for (int i = 0; i < count; i++)
            {
                Material mat = (i % 2 == 0) ? light : dark;
                float offset = -length * 0.5f + actualStripe * (i + 0.5f);
                Vector3 localPos = stripeAlongX ? new Vector3(offset, 0.02f, 0f) : new Vector3(0f, 0.02f, offset);
                Vector3 boxSize = stripeAlongX
                    ? new Vector3(actualStripe, 0.06f, depth)
                    : new Vector3(depth, 0.06f, actualStripe);
                CreateBox($"Stripe {i}", rail.transform, boxSize, localPos, mat);
            }
        }

        private void BuildGrandstand(string name, Vector3 localPos, Quaternion localRot, Material mat)
        {
            var stand = new GameObject(name);
            stand.transform.SetParent(_root.transform, false);
            stand.transform.localPosition = localPos;
            stand.transform.localRotation = localRot;

            const int tiers = 4;
            for (int i = 0; i < tiers; i++)
            {
                float h = 0.4f + i * 0.35f;
                Vector3 size = new Vector3(padSize * 0.9f, 0.35f, 0.6f);
                Vector3 pos = new Vector3(0f, h * 0.5f, i * 0.6f);
                CreateBox($"Tier {i}", stand.transform, size, pos, mat);
            }
        }

        // ------------------------------------------------------------- helpers

        private static GameObject CreateBox(string name, Transform parent, Vector3 size, Vector3 localPos, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;

            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col); // backdrop is decorative; keep it out of the way of interactions

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        private static void TileTexture(GameObject box, float tiling)
        {
            var r = box.GetComponent<MeshRenderer>();
            if (r != null && r.material != null && r.material.mainTexture != null)
            {
                r.material.mainTextureScale = new Vector2(tiling, tiling);
            }
        }

        private static Material MakeMaterial(string name, Texture2D tex, Color color, float glossiness)
        {
            // Use whichever default shader the active render pipeline provides so
            // this works under Built-in and URP without edits.
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            var mat = new Material(shader) { name = name };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                mat.mainTexture = tex;
            }
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", glossiness);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", glossiness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            return mat;
        }

        private static Texture2D BuildAsphaltTexture()
        {
            const int size = 64;
            var tex = new Texture2D(size, size) { name = "AsphaltTex", wrapMode = TextureWrapMode.Repeat };
            var rng = new System.Random(1);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                float n = 0.13f + (float)rng.NextDouble() * 0.06f; // speckled dark grey
                pixels[i] = new Color(n, n, n + 0.01f);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D BuildCheckerTexture()
        {
            const int size = 16;
            var tex = new Texture2D(size, size) { name = "CheckerTex", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool on = ((x / 2) + (y / 2)) % 2 == 0;
                    tex.SetPixel(x, y, on ? Color.white : Color.black);
                }
            }
            tex.Apply();
            return tex;
        }

        private void ApplyAmbient(bool on)
        {
            if (on)
            {
                if (!_ambientBackupTaken)
                {
                    _ambientBackup = RenderSettings.ambientLight;
                    _ambientBackupTaken = true;
                }
                RenderSettings.ambientLight = ambientTint;
            }
            else if (_ambientBackupTaken)
            {
                RenderSettings.ambientLight = _ambientBackup;
                _ambientBackupTaken = false;
            }
        }
    }
}
