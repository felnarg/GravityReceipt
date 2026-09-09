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
                "Escena 2P regenerada (split).\n\nP1 arriba: WASD + ratón, E agarrar, Q ping, 1-4 emote, Shift sprint, F ancla.\nP2 abajo: flechas + J/L e I/K (o numpad 4/6/8/5), RShift agarrar, / ping, KP1/2/3/9 emote.\n\n1) Play — splash 9 s (P pausa). En Hub la flecha apunta a la TAZA $15.\n2) Agarra la taza o el paquete NARANJA (orbe + franja verde)\n3) Enchúfalo en Archive (zona verde, pared este)\n4) Caja DORADA a una pared → flip (~1 s; cian = nueva abajo)\n\nR = rematch. P = pausa. F5 = restart. F3 = unstuck. F7 = paquete. F8 = PNG. F9 = HUD. F6 = skip.",
                "OK");
        }

        [MenuItem("GravityReceipt/Setup Office Floor A (1 jugador)")]
        public static void SetupOnePlayer()
        {
            BuildAndSave(1);
            EditorUtility.DisplayDialog(
                "Gravity Receipt",
                "Escena 1P regenerada.\n\nWASD + ratón, E agarrar, Q ping, 1-4 emote, Shift sprint, F ancla.\nPlay → paquete naranja → enchufar en Archive → flip con la caja dorada.\nP pausa · F5 restart · F3 unstuck · F7 paquete · F8 PNG · F9 HUD · F6 skip.",
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
