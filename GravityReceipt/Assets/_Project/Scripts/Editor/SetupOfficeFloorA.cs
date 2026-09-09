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
                "Escena 2P lista.\n\nGanar: lleva el PAQUETE naranja a 3 zonas (verde → azul → dorada).\nTwist: el objeto con más $ voltea esa sala.\n\nP1: WASD + ratón, E agarrar. P2: flechas, RShift agarrar.\nPlay. Sigue la flecha. P pausa (controles). F10 comfort.",
                "OK");
        }

        [MenuItem("GravityReceipt/Setup Office Floor A (1 jugador)")]
        public static void SetupOnePlayer()
        {
            BuildAndSave(1);
            EditorUtility.DisplayDialog(
                "Gravity Receipt",
                "Escena 1P lista.\n\nGanar: paquete naranja a 3 zonas (verde → azul → dorada).\nEl $ más caro voltea esa sala.\nWASD + E. P pausa · F10 comfort.",
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
