using UnityEditor;
using UnityEngine;

namespace Aeroflux.EditorTools
{
    /// <summary>
    /// One-click scene wiring for the Aeroflux demo, under the <b>Aeroflux</b>
    /// menu in the Unity Editor. It finds (or imports) the car model, drops an
    /// <see cref="AerofluxAppController"/> into the scene and points it at the
    /// car – so contributors don't have to wire references by hand.
    /// </summary>
    public static class AerofluxSetup
    {
        // GUID of the bundled car FBX (A1+PRO+Changed+wheels.fbx).
        private const string CarFbxGuid = "15b5d3ac072b0c5408c3908c281e3420";

        [MenuItem("Aeroflux/Set Up Demo In Current Scene", priority = 0)]
        public static void SetUpDemo()
        {
            Transform car = FindOrSpawnCar();
            if (car == null)
            {
                EditorUtility.DisplayDialog("Aeroflux",
                    "Couldn't find the car model in the scene or in the project. " +
                    "Drag the car into the scene first, then run this again.", "OK");
                return;
            }

            var existing = Object.FindObjectOfType<AerofluxAppController>();
            if (existing != null)
            {
                Selection.activeObject = existing.gameObject;
                EditorUtility.DisplayDialog("Aeroflux",
                    "An AerofluxAppController is already in the scene. Selecting it for you.", "OK");
                return;
            }

            var rig = new GameObject("Aeroflux Rig");
            Undo.RegisterCreatedObjectUndo(rig, "Create Aeroflux Rig");

            var controller = rig.AddComponent<AerofluxAppController>();
            var so = new SerializedObject(controller);
            so.FindProperty("carRoot").objectReferenceValue = car;
            so.ApplyModifiedProperties();

            Selection.activeObject = rig;
            EditorGUIUtility.PingObject(rig);

            EditorUtility.DisplayDialog("Aeroflux",
                "Demo set up.\n\n" +
                "• Car: " + car.name + "\n" +
                "• Added: AerofluxAppController (+ dissection / summon / environment on play)\n\n" +
                "Press Play to try it. The control panel and racing backdrop build at runtime.",
                "Nice");
        }

        [MenuItem("Aeroflux/Add Car To Scene", priority = 20)]
        public static void AddCarMenu()
        {
            if (FindOrSpawnCar() == null)
            {
                EditorUtility.DisplayDialog("Aeroflux", "Couldn't locate the car FBX in the project.", "OK");
            }
        }

        private static Transform FindOrSpawnCar()
        {
            // Prefer something already in the scene that looks like the car.
            var controller = Object.FindObjectOfType<AerofluxAppController>();
            if (controller != null)
            {
                var so = new SerializedObject(controller);
                var carRef = so.FindProperty("carRoot").objectReferenceValue as Transform;
                if (carRef != null) return carRef;
            }

            // Otherwise instantiate the bundled FBX.
            string path = AssetDatabase.GUIDToAssetPath(CarFbxGuid);
            if (string.IsNullOrEmpty(path)) return null;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return null;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Aeroflux Car";
            instance.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(instance, "Add Aeroflux Car");
            return instance.transform;
        }
    }
}
