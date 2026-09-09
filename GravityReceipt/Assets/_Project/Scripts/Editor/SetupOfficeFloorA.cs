using GravityReceipt.Gravity;
using GravityReceipt.Interaction;
using GravityReceipt.Mission;
using GravityReceipt.Player;
using GravityReceipt.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GravityReceipt.EditorTools
{
    /// <summary>
    /// Genera la escena mínima jugable Archive (Sala A) para el vertical slice.
    /// Menú: GravityReceipt → Setup Office Floor A
    /// Batch: -executeMethod GravityReceipt.EditorTools.SetupOfficeFloorA.SetupFromBatch
    /// </summary>
    public static class SetupOfficeFloorA
    {
        private const string ScenePath = "Assets/_Project/Scenes/Office_Floor_A.unity";

        [MenuItem("GravityReceipt/Setup Office Floor A")]
        public static void SetupFromMenu()
        {
            BuildScene();
            EditorUtility.DisplayDialog(
                "Gravity Receipt",
                "Escena regenerada.\n\n1) Play\n2) Agarra la caja DORADA (E)\n3) Llévada CERCA DE UNA PARED y suéltala\n4) Tras ~1s la gravedad tira hacia esa pared\n\nSi sales de la sala, respawneas solo.",
                "OK");
        }

        public static void SetupFromBatch()
        {
            BuildScene();
            Debug.Log("[GravityReceipt] Office_Floor_A created at " + ScenePath);
        }

        private static void BuildScene()
        {
            EnsureFolders();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            camGo.AddComponent<AudioListener>();

            var roomRoot = new GameObject("RoomCenter");
            roomRoot.transform.position = Vector3.zero;

            var gravityGo = new GameObject("GravityManager");
            var gravity = gravityGo.AddComponent<GravityManager>();
            var gravitySo = new SerializedObject(gravity);
            gravitySo.FindProperty("roomCenter").objectReferenceValue = roomRoot.transform;
            gravitySo.FindProperty("telegraphSeconds").floatValue = 1f;
            gravitySo.ApplyModifiedPropertiesWithoutUndo();
            gravityGo.AddComponent<DominantValuableOutline>();

            // Caja sellada (suelo + 4 paredes + techo, con solape para evitar fugas)
            var room = CreateSealedRoom("Room_Archive", new Vector3(14f, 5f, 10f));

            var cheap = CreateValuable(
                "Valuable_Archivador_40",
                new Vector3(-3.5f, -1.7f, 2f),
                new Vector3(0.8f, 1.6f, 0.5f),
                40,
                gravity,
                new Color(0.55f, 0.4f, 0.25f));

            var expensive = CreateValuable(
                "Valuable_CajaFuerte_80",
                new Vector3(3f, -1.9f, -2f),
                new Vector3(1.1f, 1.1f, 1.1f),
                80,
                gravity,
                new Color(0.85f, 0.7f, 0.2f));

            var player = CreatePlayer(new Vector3(0f, -1.2f, -3.5f), camGo.transform, gravity);

            var hudGo = new GameObject("GravityHUD");
            var hud = hudGo.AddComponent<GravityHud>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("gravityManager").objectReferenceValue = gravity;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "TutorialSign";
            sign.transform.position = new Vector3(0f, 0.2f, 4.7f);
            sign.transform.localScale = new Vector3(5f, 1.4f, 0.15f);
            SetColor(sign, new Color(0.12f, 0.14f, 0.18f));

            var pit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pit.name = "VoidPit";
            pit.transform.position = new Vector3(0f, -8f, 0f);
            pit.transform.localScale = new Vector3(60f, 1f, 60f);
            pit.GetComponent<Collider>().isTrigger = true;
            SetColor(pit, new Color(0.05f, 0.05f, 0.08f));
            var voidZone = pit.AddComponent<VoidKillZone>();
            var voidSo = new SerializedObject(voidZone);
            voidSo.FindProperty("respawnPoint").vector3Value = new Vector3(0f, -1.2f, -3.5f);
            voidSo.FindProperty("player").objectReferenceValue = player.transform;
            voidSo.FindProperty("maxDistanceFromOrigin").floatValue = 12f;
            voidSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            Selection.activeGameObject = player;
            Debug.Log(
                $"[GravityReceipt] Saved {ScenePath}. Dominante: {expensive.name}. " +
                "Coloca la caja cara a una pared y suéltala para voltear g.");
            _ = cheap;
            _ = room;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            }
        }

        private static GameObject CreateSealedRoom(string name, Vector3 size)
        {
            var root = new GameObject(name);
            const float t = 0.4f; // grosor

            // Interior útil ≈ size; placas con solape
            CreateWall(root.transform, "Floor", new Vector3(0f, -size.y * 0.5f, 0f), new Vector3(size.x + t, t, size.z + t), new Color(0.35f, 0.38f, 0.42f));
            CreateWall(root.transform, "Ceiling", new Vector3(0f, size.y * 0.5f, 0f), new Vector3(size.x + t, t, size.z + t), new Color(0.5f, 0.52f, 0.55f));
            CreateWall(root.transform, "Wall_N", new Vector3(0f, 0f, size.z * 0.5f), new Vector3(size.x + t, size.y + t, t), new Color(0.55f, 0.58f, 0.62f));
            CreateWall(root.transform, "Wall_S", new Vector3(0f, 0f, -size.z * 0.5f), new Vector3(size.x + t, size.y + t, t), new Color(0.55f, 0.58f, 0.62f));
            CreateWall(root.transform, "Wall_E", new Vector3(size.x * 0.5f, 0f, 0f), new Vector3(t, size.y + t, size.z + t), new Color(0.52f, 0.55f, 0.6f));
            CreateWall(root.transform, "Wall_W", new Vector3(-size.x * 0.5f, 0f, 0f), new Vector3(t, size.y + t, size.z + t), new Color(0.52f, 0.55f, 0.6f));

            return root;
        }

        private static void CreateWall(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = localPos;
            wall.transform.localScale = scale;
            SetColor(wall, color);
        }

        private static GameObject CreateRoom(string name, Vector3 position, Vector3 size)
        {
            // Compat: redirige al sellado centrado en origen.
            _ = position;
            return CreateSealedRoom(name, size);
        }

        private static void CreateWall(Transform parent, string name, Vector3 localPos, Vector3 scale)
        {
            CreateWall(parent, name, localPos, scale, new Color(0.55f, 0.58f, 0.62f));
        }

        private static GameObject CreateValuable(
            string name,
            Vector3 position,
            Vector3 scale,
            int price,
            GravityManager gravity,
            Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            SetColor(go, color);

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 8f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var valuable = go.AddComponent<ValuableItem>();
            var so = new SerializedObject(valuable);
            so.FindProperty("price").intValue = price;
            so.FindProperty("gravityManager").objectReferenceValue = gravity;
            so.FindProperty("isActiveValuable").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            return go;
        }

        private static GameObject CreatePlayer(Vector3 position, Transform cameraTransform, GravityManager gravity)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = position;
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.skinWidth = 0.08f;
            cc.minMoveDistance = 0f;

            var hold = new GameObject("HoldPoint");
            hold.transform.SetParent(player.transform, false);
            hold.transform.localPosition = new Vector3(0.4f, 1.2f, 1.0f);

            cameraTransform.SetParent(player.transform, false);
            cameraTransform.localPosition = new Vector3(0f, 1.6f, 0f);
            cameraTransform.localRotation = Quaternion.identity;

            var motor = player.AddComponent<PlayerMotor>();
            var motorSo = new SerializedObject(motor);
            motorSo.FindProperty("cameraPivot").objectReferenceValue = cameraTransform;
            motorSo.FindProperty("gravityManager").objectReferenceValue = gravity;
            motorSo.ApplyModifiedPropertiesWithoutUndo();

            var interactor = player.AddComponent<PlayerInteractor>();
            var intSo = new SerializedObject(interactor);
            intSo.FindProperty("holdPoint").objectReferenceValue = hold.transform;
            intSo.ApplyModifiedPropertiesWithoutUndo();

            return player;
        }

        private static void SetColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer is null)
            {
                return;
            }

            var shader = Shader.Find("Standard");
            if (shader is null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader is null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            var mat = new Material(shader) { color = color };
            renderer.sharedMaterial = mat;
        }
    }
}
