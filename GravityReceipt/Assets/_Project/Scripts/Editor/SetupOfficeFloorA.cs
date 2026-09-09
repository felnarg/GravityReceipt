using GravityReceipt.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GravityReceipt.EditorTools
{
    /// <summary>
    /// Regenera Office_Floor_A (Hub → Archive → Pasillo → Office → Executive).
    /// Menú: GravityReceipt → Setup Office Floor A
    /// Batch: -executeMethod GravityReceipt.EditorTools.SetupOfficeFloorA.SetupFromBatch
    /// </summary>
    public static class SetupOfficeFloorA
    {
        private const string ScenePath = "Assets/_Project/Scenes/Office_Floor_A.unity";

        [MenuItem("GravityReceipt/Setup Office Floor A")]
        public static void SetupFromMenu()
        {
            BuildAndSave(2);
            EditorUtility.DisplayDialog(
                "Gravity Receipt",
                "Escena 2P regenerada (split).\n\nP1 arriba: WASD + ratón, E agarrar, Q ping, Shift sprint, F ancla.\nP2 abajo: flechas + numpad 4/6/8/5, RShift agarrar, / ping.\n\n1) Play\n2) Agarra el paquete NARANJA\n3) Enchúfalo en Archive (zona verde, pared este)\n4) Caja DORADA a una pared → flip de g (~1 s)\n\nR = rematch al ganar/perder.",
                "OK");
        }

        [MenuItem("GravityReceipt/Setup Office Floor A (1 jugador)")]
        public static void SetupOnePlayer()
        {
            BuildAndSave(1);
            EditorUtility.DisplayDialog(
                "Gravity Receipt",
                "Escena 1P regenerada.\n\nWASD + ratón, E agarrar, Q ping, Shift sprint, F ancla.\nPlay → paquete naranja → enchufar en Archive → flip con la caja dorada.",
                "OK");
        }

        public static void SetupFromBatch()
        {
            BuildAndSave(2);
            Debug.Log("[GravityReceipt] Office_Floor_A created at " + ScenePath);
        }

        private static void BuildAndSave(int playerCount)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            OfficeFloorFactory.Build(playerCount);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log($"[GravityReceipt] Saved {ScenePath} ({playerCount}p).");
        }
    }
}
