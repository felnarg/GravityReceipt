using GravityReceipt.Gravity;
using GravityReceipt.Interaction;
using GravityReceipt.Mission;
using GravityReceipt.Player;
using GravityReceipt.UI;
using UnityEngine;

namespace GravityReceipt.World
{
    /// <summary>
    /// Construye Hub → Archive → Pasillo → Open Office → Executive (offline).
    /// Lo usa el menú Editor y puede llamarse en batch.
    /// </summary>
    public static class OfficeFloorFactory
    {
        public const float MatchSeconds = 600f;
        private const float WallT = 0.4f;
        private const float DoorW = 2.9f;
        private const float DoorH = 3.4f;

        private static readonly Vector3 HubC = new(0f, 2.5f, 0f);
        private static readonly Vector3 HubS = new(10f, 5f, 8f);
        private static readonly Vector3 ArcC = new(0f, 2.5f, 10f);
        private static readonly Vector3 ArcS = new(14f, 5f, 12f);
        private static readonly Vector3 CorC = new(0f, 2.5f, 21.5f);
        private static readonly Vector3 CorS = new(3.6f, 5f, 11f);
        private static readonly Vector3 OffC = new(0f, 2.5f, 35f);
        private static readonly Vector3 OffS = new(16f, 5f, 16f);
        private static readonly Vector3 ExeC = new(0f, 2.5f, 50f);
        private static readonly Vector3 ExeS = new(12f, 5f, 14f);

        public static GameObject Build(int playerCount)
        {
            playerCount = Mathf.Clamp(playerCount, 1, 2);
            RoomRegistry.Clear();
            Physics.gravity = Vector3.zero;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.05f, 0.055f, 0.07f);
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.52f, 0.56f);

            var root = new GameObject("OfficeFloor");

            var lightGo = new GameObject("Directional Light");
            lightGo.transform.SetParent(root.transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.97f, 0.92f);
            lightGo.transform.rotation = Quaternion.Euler(48f, -25f, 0f);

            AddPointLight(root.transform, HubC + Vector3.up, new Color(0.7f, 0.9f, 1f), 1.1f);
            AddPointLight(root.transform, ArcC + Vector3.up, new Color(1f, 0.92f, 0.75f), 1.2f);
            AddPointLight(root.transform, CorC + Vector3.up * 0.2f, new Color(1f, 0.45f, 0.4f), 0.8f);
            AddPointLight(root.transform, OffC + Vector3.up, new Color(0.75f, 0.88f, 1f), 1.2f);
            AddPointLight(root.transform, ExeC + Vector3.up, new Color(1f, 0.82f, 0.55f), 1.15f);
            AddPointLight(root.transform, new Vector3(5.9f, 1.6f, 10f), new Color(0.3f, 1f, 0.55f), 0.7f, 6.5f);
            AddPointLight(root.transform, new Vector3(0f, 1.4f, 40.5f), new Color(0.35f, 0.65f, 1f), 0.65f, 6.5f);
            AddPointLight(root.transform, new Vector3(0f, 1.4f, 54.2f), new Color(1f, 0.82f, 0.3f), 0.65f, 6.5f);

            var hubG = CreateGravity(root.transform, "Gravity_Hub", HubC);
            var arcG = CreateGravity(root.transform, "Gravity_Archive", ArcC);
            var offG = CreateGravity(root.transform, "Gravity_Office", OffC);
            var exeG = CreateGravity(root.transform, "Gravity_Executive", ExeC);

            CreateRoom(root.transform, "Hub", HubC, HubS, new Color(0.16f, 0.28f, 0.3f), new Color(0.32f, 0.48f, 0.5f), southDoor: false, northDoor: true);
            CreateRoom(root.transform, "Archive", ArcC, ArcS, new Color(0.38f, 0.34f, 0.28f), new Color(0.55f, 0.5f, 0.42f), southDoor: true, northDoor: true);
            CreateCatwalk(root.transform, "Pasillo", CorC, CorS, new Color(0.14f, 0.14f, 0.16f));
            CreateRoom(root.transform, "OpenOffice", OffC, OffS, new Color(0.26f, 0.32f, 0.4f), new Color(0.42f, 0.5f, 0.58f), southDoor: true, northDoor: true);
            CreateRoom(root.transform, "Executive", ExeC, ExeS, new Color(0.32f, 0.22f, 0.16f), new Color(0.5f, 0.36f, 0.28f), southDoor: true, northDoor: false);
            DressRooms(root.transform);

            CreateRoomVolume(root.transform, "Hub", HubC, HubS, hubG, inherit: false);
            CreateRoomVolume(root.transform, "Archive", ArcC, ArcS, arcG, inherit: false);
            CreateRoomVolume(root.transform, "Pasillo", CorC, new Vector3(6.4f, CorS.y * 1.1f, CorS.z), null, inherit: true);
            CreateRoomVolume(root.transform, "OpenOffice", OffC, OffS, offG, inherit: false);
            CreateRoomVolume(root.transform, "Executive", ExeC, ExeS, exeG, inherit: false);

            var taza = CreateValuable(root.transform, "Valuable_Taza_15", new Vector3(-2.2f, 0.52f, 1.1f), new Vector3(0.36f, 0.28f, 0.36f), 15, hubG, new Color(0.75f, 0.45f, 0.28f), PrimitiveType.Cylinder);
            AddLocalVisual(taza.transform, PrimitiveType.Cube, new Vector3(0.72f, 0f, 0f), new Vector3(0.28f, 0.85f, 0.18f), new Color(0.7f, 0.42f, 0.25f));
            taza.AddComponent<TutorialItemBeacon>();
            var arch = CreateValuable(root.transform, "Valuable_Archivador_40", new Vector3(-3.6f, 1.0f, 8.5f), new Vector3(0.8f, 1.6f, 0.5f), 40, arcG, new Color(0.55f, 0.38f, 0.22f), PrimitiveType.Cube);
            AddLocalVisual(arch.transform, PrimitiveType.Cube, new Vector3(0f, 0.18f, 0.52f), new Vector3(0.85f, 0.28f, 0.08f), new Color(0.4f, 0.28f, 0.16f));
            AddLocalVisual(arch.transform, PrimitiveType.Cube, new Vector3(0f, -0.18f, 0.52f), new Vector3(0.85f, 0.28f, 0.08f), new Color(0.4f, 0.28f, 0.16f));
            AddLocalVisual(arch.transform, PrimitiveType.Cube, new Vector3(0f, -0.48f, 0.52f), new Vector3(0.85f, 0.22f, 0.08f), new Color(0.35f, 0.24f, 0.14f));
            var caja = CreateValuable(root.transform, "Valuable_CajaFuerte_80", new Vector3(3.4f, 0.75f, 12.2f), new Vector3(1.1f, 1.1f, 1.1f), 80, arcG, new Color(0.9f, 0.72f, 0.18f), PrimitiveType.Cube);
            AddLocalVisual(caja.transform, PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.56f), new Vector3(0.22f, 0.08f, 0.22f), new Color(0.35f, 0.32f, 0.28f));
            var monitor = CreateValuable(root.transform, "Valuable_Monitor_120", new Vector3(4.5f, 0.55f, 32f), new Vector3(1.4f, 0.85f, 0.12f), 120, offG, new Color(0.15f, 0.45f, 0.95f), PrimitiveType.Cube);
            AddLocalVisual(monitor.transform, PrimitiveType.Cube, new Vector3(0f, -0.7f, 0.8f), new Vector3(0.18f, 0.55f, 0.7f), new Color(0.12f, 0.12f, 0.16f));
            AddLocalVisual(monitor.transform, PrimitiveType.Cube, new Vector3(0f, 0.02f, 0.08f), new Vector3(0.82f, 0.72f, 0.2f), new Color(0.35f, 0.75f, 1f));
            var planta = CreateValuable(root.transform, "Valuable_Planta_60", new Vector3(-5.5f, 0.85f, 38f), new Vector3(0.55f, 0.85f, 0.55f), 60, offG, new Color(0.2f, 0.72f, 0.28f), PrimitiveType.Capsule);
            AddLocalVisual(planta.transform, PrimitiveType.Cylinder, new Vector3(0f, -0.55f, 0f), new Vector3(1.15f, 0.28f, 1.15f), new Color(0.45f, 0.28f, 0.16f));
            AddLocalVisual(planta.transform, PrimitiveType.Sphere, new Vector3(0.35f, 0.42f, 0.1f), new Vector3(0.7f, 0.45f, 0.7f), new Color(0.16f, 0.62f, 0.22f));
            var cafe = CreateValuable(root.transform, "Valuable_Cafetera_90", new Vector3(6f, 0.55f, 37.5f), new Vector3(0.55f, 0.55f, 0.55f), 90, offG, new Color(0.82f, 0.18f, 0.14f), PrimitiveType.Cylinder);
            AddLocalVisual(cafe.transform, PrimitiveType.Cube, new Vector3(0.7f, 0.15f, 0f), new Vector3(0.45f, 0.18f, 0.18f), new Color(0.2f, 0.2f, 0.22f));
            AddLocalVisual(cafe.transform, PrimitiveType.Sphere, new Vector3(0f, 0.62f, 0f), new Vector3(0.55f, 0.22f, 0.55f), new Color(0.2f, 0.2f, 0.22f));
            var maletin = CreateValuable(root.transform, "Valuable_Maletin_200", new Vector3(2.2f, 0.32f, 47.5f), new Vector3(1.05f, 0.28f, 0.7f), 200, exeG, new Color(0.12f, 0.08f, 0.06f), PrimitiveType.Cube);
            AddLocalVisual(maletin.transform, PrimitiveType.Cube, new Vector3(0f, 0.7f, 0f), new Vector3(0.28f, 0.55f, 0.16f), new Color(0.78f, 0.64f, 0.18f));
            AddLocalVisual(maletin.transform, PrimitiveType.Cube, new Vector3(0.32f, 0.05f, 0.52f), new Vector3(0.18f, 0.22f, 0.12f), new Color(0.78f, 0.64f, 0.18f));
            AddLocalVisual(maletin.transform, PrimitiveType.Cube, new Vector3(-0.32f, 0.05f, 0.52f), new Vector3(0.18f, 0.22f, 0.12f), new Color(0.78f, 0.64f, 0.18f));
            var trophy = CreateValuable(root.transform, "Valuable_Trofeo_150", new Vector3(-3.2f, 0.85f, 52.5f), new Vector3(0.35f, 0.85f, 0.35f), 150, exeG, new Color(0.98f, 0.82f, 0.18f), PrimitiveType.Capsule);
            AddLocalVisual(trophy.transform, PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), new Vector3(1.15f, 0.42f, 1.15f), new Color(1f, 0.9f, 0.28f));
            AddLocalVisual(trophy.transform, PrimitiveType.Cube, new Vector3(-0.7f, 0.15f, 0f), new Vector3(0.18f, 0.7f, 0.18f), new Color(0.95f, 0.78f, 0.2f));
            AddLocalVisual(trophy.transform, PrimitiveType.Cube, new Vector3(0.7f, 0.15f, 0f), new Vector3(0.18f, 0.7f, 0.18f), new Color(0.95f, 0.78f, 0.2f));
            var server = CreateValuable(root.transform, "Valuable_Server_110", new Vector3(4.2f, 0.95f, 54f), new Vector3(0.7f, 1.7f, 0.55f), 110, exeG, new Color(0.28f, 0.32f, 0.42f), PrimitiveType.Cube);
            var ledOk = AddLocalVisual(server.transform, PrimitiveType.Cube, new Vector3(0.52f, 0.25f, 0f), new Vector3(0.08f, 0.12f, 0.7f), new Color(0.2f, 0.95f, 0.35f));
            ledOk.AddComponent<PulseColor>().Configure(new Color(0.15f, 0.95f, 0.3f), new Color(0.04f, 0.25f, 0.08f), 5.5f);
            var ledErr = AddLocalVisual(server.transform, PrimitiveType.Cube, new Vector3(0.52f, 0.05f, 0f), new Vector3(0.08f, 0.12f, 0.7f), new Color(0.95f, 0.25f, 0.15f));
            ledErr.AddComponent<PulseColor>().Configure(new Color(0.95f, 0.2f, 0.12f), new Color(0.25f, 0.05f, 0.04f), 3.2f);
            var plantaOro = CreateValuable(root.transform, "Valuable_PlantaOro_95", new Vector3(-4.4f, 0.85f, 46.5f), new Vector3(0.6f, 0.9f, 0.6f), 95, exeG, new Color(0.88f, 0.72f, 0.12f), PrimitiveType.Capsule);
            AddLocalVisual(plantaOro.transform, PrimitiveType.Cylinder, new Vector3(0f, -0.55f, 0f), new Vector3(1.15f, 0.28f, 1.15f), new Color(0.55f, 0.42f, 0.12f));
            AddLocalVisual(plantaOro.transform, PrimitiveType.Sphere, new Vector3(-0.32f, 0.4f, 0.12f), new Vector3(0.65f, 0.42f, 0.65f), new Color(0.95f, 0.78f, 0.18f));

            var pkg = CreatePackage(root.transform, new Vector3(0f, 0.45f, 0.6f), hubG);
            pkg.AddComponent<PackageBeacon>();
            AttachPriceTag(pkg.transform, 0, 0.45f, "PAQUETE");
            CreateProp(root.transform, "Prop_CajaGris", new Vector3(2.4f, 0.4f, -1.4f), new Vector3(0.7f, 0.7f, 0.7f), new Color(0.42f, 0.44f, 0.46f), hubG);
            var silla = CreateProp(root.transform, "Prop_Silla", new Vector3(-3.4f, 0.45f, -1.2f), new Vector3(0.45f, 0.85f, 0.45f), new Color(0.32f, 0.3f, 0.28f), hubG);
            AddLocalVisual(silla.transform, PrimitiveType.Cube, new Vector3(0f, 0.55f, -0.42f), new Vector3(1f, 0.95f, 0.16f), new Color(0.28f, 0.26f, 0.24f));
            CreateProp(root.transform, "Prop_Mesa", new Vector3(3.2f, 0.35f, 1.6f), new Vector3(1.4f, 0.12f, 0.8f), new Color(0.38f, 0.28f, 0.2f), hubG);
            CreateProp(root.transform, "Prop_CajaArchive", new Vector3(-5.2f, 0.4f, 11.5f), new Vector3(0.65f, 0.65f, 0.65f), new Color(0.4f, 0.4f, 0.42f), arcG);
            CreateProp(root.transform, "Prop_Libros", new Vector3(5.2f, 0.35f, 13.5f), new Vector3(0.9f, 0.35f, 0.5f), new Color(0.45f, 0.22f, 0.18f), arcG);
            CreateProp(root.transform, "Prop_Sillon", new Vector3(-1.8f, 0.45f, 48.2f), new Vector3(1.1f, 0.7f, 0.7f), new Color(0.28f, 0.2f, 0.16f), exeG);
            var sillaOff = CreateProp(root.transform, "Prop_SillaOffice", new Vector3(-4.2f, 0.45f, 33.5f), new Vector3(0.5f, 0.85f, 0.5f), new Color(0.3f, 0.34f, 0.4f), offG);
            AddLocalVisual(sillaOff.transform, PrimitiveType.Cube, new Vector3(0f, 0.55f, -0.42f), new Vector3(1f, 0.95f, 0.16f), new Color(0.22f, 0.26f, 0.32f));

            CreateStaticCube(root.transform, "Furn_ArchiveShelf", new Vector3(-6.2f, 1.35f, 7.2f), new Vector3(0.35f, 2.6f, 3.4f), new Color(0.42f, 0.32f, 0.22f));
            CreateStaticCube(root.transform, "Furn_OfficeDesk", new Vector3(-6.2f, 0.38f, 34.2f), new Vector3(2.2f, 0.12f, 1.0f), new Color(0.55f, 0.48f, 0.38f));
            CreateStaticCube(root.transform, "Furn_OfficeDeskLegL", new Vector3(-7.05f, 0.18f, 34.2f), new Vector3(0.12f, 0.36f, 0.85f), new Color(0.28f, 0.26f, 0.24f));
            CreateStaticCube(root.transform, "Furn_OfficeDeskLegR", new Vector3(-5.35f, 0.18f, 34.2f), new Vector3(0.12f, 0.36f, 0.85f), new Color(0.28f, 0.26f, 0.24f));
            CreateStaticCube(root.transform, "Furn_ExeTable", new Vector3(0f, 0.38f, 50.2f), new Vector3(3.6f, 0.12f, 1.6f), new Color(0.28f, 0.16f, 0.1f));
            CreateStaticCube(root.transform, "Furn_ExeTableLeg1", new Vector3(-1.5f, 0.18f, 49.6f), new Vector3(0.12f, 0.36f, 0.12f), new Color(0.18f, 0.1f, 0.06f));
            CreateStaticCube(root.transform, "Furn_ExeTableLeg2", new Vector3(1.5f, 0.18f, 49.6f), new Vector3(0.12f, 0.36f, 0.12f), new Color(0.18f, 0.1f, 0.06f));
            CreateStaticCube(root.transform, "Furn_ExeTableLeg3", new Vector3(-1.5f, 0.18f, 50.8f), new Vector3(0.12f, 0.36f, 0.12f), new Color(0.18f, 0.1f, 0.06f));
            CreateStaticCube(root.transform, "Furn_ExeTableLeg4", new Vector3(1.5f, 0.18f, 50.8f), new Vector3(0.12f, 0.36f, 0.12f), new Color(0.18f, 0.1f, 0.06f));
            CreateStaticCube(root.transform, "Furn_HubCounter", new Vector3(0f, 0.42f, -3.45f), new Vector3(3.4f, 0.14f, 0.55f), new Color(0.22f, 0.34f, 0.36f));
            var cooler = CreateStaticCube(root.transform, "Furn_WaterCooler", new Vector3(4.4f, 0.7f, -2.6f), new Vector3(0.45f, 1.2f, 0.45f), new Color(0.75f, 0.82f, 0.88f));
            AddLocalVisual(cooler.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f), new Vector3(0.7f, 0.35f, 0.7f), new Color(0.45f, 0.75f, 0.95f));
            var clock = CreateStaticCube(root.transform, "Furn_HubClock", new Vector3(0f, 3.35f, -3.72f), new Vector3(0.7f, 0.7f, 0.08f), new Color(0.92f, 0.92f, 0.9f));
            DisableCollider(clock);
            var hour = CreateStaticCube(root.transform, "Furn_ClockHour", new Vector3(0f, 3.35f, -3.66f), new Vector3(0.045f, 0.22f, 0.03f), new Color(0.12f, 0.12f, 0.14f));
            DisableCollider(hour);
            hour.AddComponent<SpinInPlace>().Configure(Vector3.forward, 8f);
            var minute = CreateStaticCube(root.transform, "Furn_ClockMinute", new Vector3(0f, 3.35f, -3.64f), new Vector3(0.03f, 0.28f, 0.03f), new Color(0.18f, 0.18f, 0.2f));
            DisableCollider(minute);
            minute.AddComponent<SpinInPlace>().Configure(Vector3.forward, 48f);
            CreateStaticCube(root.transform, "Furn_Printer", new Vector3(6.4f, 0.45f, 38.5f), new Vector3(0.9f, 0.7f, 0.7f), new Color(0.28f, 0.3f, 0.34f));
            var mat = CreateStaticCube(root.transform, "Furn_SpawnMat", new Vector3(0f, 0.03f, -1.8f), new Vector3(3.4f, 0.04f, 1.6f), new Color(0.18f, 0.4f, 0.42f));
            DisableCollider(mat);
            CreateStaticCube(root.transform, "Furn_ArchiveShelfB", new Vector3(6.2f, 1.35f, 12.6f), new Vector3(0.35f, 2.6f, 2.6f), new Color(0.4f, 0.3f, 0.2f));
            CreateStaticCube(root.transform, "Furn_CubicleL", new Vector3(-5.8f, 0.65f, 35.5f), new Vector3(0.1f, 1.3f, 3.6f), new Color(0.38f, 0.46f, 0.55f));
            CreateStaticCube(root.transform, "Furn_CubicleLFront", new Vector3(-4.2f, 0.65f, 33.75f), new Vector3(3.1f, 1.3f, 0.1f), new Color(0.38f, 0.46f, 0.55f));
            CreateStaticCube(root.transform, "Furn_CubicleR", new Vector3(7.15f, 0.65f, 36.8f), new Vector3(0.1f, 1.3f, 3.2f), new Color(0.38f, 0.46f, 0.55f));
            CreateStaticCube(root.transform, "Furn_ExePainting", new Vector3(5.72f, 2.1f, 50f), new Vector3(0.08f, 1.4f, 1.8f), new Color(0.55f, 0.22f, 0.18f));
            var shelfBoxA = CreateStaticCube(root.transform, "Furn_ShelfBoxA", new Vector3(-6.0f, 1.85f, 6.4f), new Vector3(0.28f, 0.22f, 0.45f), new Color(0.55f, 0.38f, 0.22f));
            DisableCollider(shelfBoxA);
            var shelfBoxB = CreateStaticCube(root.transform, "Furn_ShelfBoxB", new Vector3(-6.0f, 1.15f, 7.6f), new Vector3(0.28f, 0.22f, 0.5f), new Color(0.48f, 0.22f, 0.18f));
            DisableCollider(shelfBoxB);
            var keyboard = CreateStaticCube(root.transform, "Furn_Keyboard", new Vector3(-6.2f, 0.48f, 34.2f), new Vector3(0.7f, 0.04f, 0.28f), new Color(0.18f, 0.18f, 0.2f));
            DisableCollider(keyboard);
            var exeLamp = CreateStaticCube(root.transform, "Furn_ExeLamp", new Vector3(1.4f, 0.72f, 50.2f), new Vector3(0.12f, 0.55f, 0.12f), new Color(0.72f, 0.62f, 0.28f));
            DisableCollider(exeLamp);
            AddLocalVisual(exeLamp.transform, PrimitiveType.Sphere, new Vector3(0f, 0.62f, 0f), new Vector3(2.4f, 1.1f, 2.4f), new Color(1f, 0.88f, 0.45f));
            var hubPlaque = CreateStaticCube(root.transform, "Furn_HubPlaque", new Vector3(0f, 0.95f, -3.55f), new Vector3(0.9f, 0.28f, 0.06f), new Color(0.12f, 0.22f, 0.24f));
            DisableCollider(hubPlaque);

            var matchGo = new GameObject("MatchDirector");
            matchGo.transform.SetParent(root.transform, false);
            var match = matchGo.AddComponent<MatchDirector>();
            match.Configure(MatchSeconds, 3);
            matchGo.AddComponent<MatchHighlightRecorder>();
            var checkpoints = matchGo.AddComponent<CheckpointSystem>();
            var p1Spawn = new Vector3(-1.4f, 0.3f, -1.8f);
            var p2Spawn = new Vector3(1.4f, 0.3f, -1.8f);
            checkpoints.Configure(new[] { p1Spawn, p2Spawn }, pkg.transform.position);
            checkpoints.SetStageSpawns(1, new[] { new Vector3(-1.2f, 0.3f, 14.5f), new Vector3(1.2f, 0.3f, 14.5f) }, new Vector3(0f, 0.5f, 14.5f));
            checkpoints.SetStageSpawns(2, new[] { new Vector3(-1.2f, 0.3f, 40f), new Vector3(1.2f, 0.3f, 40f) }, new Vector3(0f, 0.5f, 40f));
            checkpoints.SetStageSpawns(3, new[] { new Vector3(-1.2f, 0.3f, 54f), new Vector3(1.2f, 0.3f, 54f) }, new Vector3(0f, 0.5f, 54f));

            CreateObjective(root.transform, "Obj1_Enchufar", 0, "Enchufar", new Vector3(5.9f, 1.1f, 10f), new Vector3(1.5f, 1.8f, 1.8f), new Vector3(2.2f, 1.4f, 1.4f), 0.45f, package: true, player: false, hold: false, new Color(0.2f, 0.85f, 0.55f));
            CreateObjective(root.transform, "Obj2_Entregar", 1, "Entregar", new Vector3(0f, 0.22f, 40.5f), new Vector3(2.6f, 0.12f, 2.6f), new Vector3(1f, 28f, 1f), 0.5f, package: true, player: false, hold: false, new Color(0.3f, 0.6f, 1f));
            CreateObjective(root.transform, "Obj3_Sellar", 2, "Sellar", new Vector3(0f, 0.22f, 54.2f), new Vector3(2.8f, 0.12f, 2.4f), new Vector3(1f, 32f, 1f), 1.0f, package: true, player: true, hold: true, new Color(0.95f, 0.75f, 0.2f));

            CreateSign(root.transform, "TutorialSign", new Vector3(0f, 3.88f, 3.82f), new Vector3(6.2f, 0.5f, 0.12f),
                "LA GRAVEDAD SIGUE LO MÁS CARO", new Vector3(0f, 3.88f, 3.65f), Quaternion.Euler(0f, 180f, 0f), new Color(0.08f, 0.1f, 0.12f), new Color(1f, 0.92f, 0.35f));
            CreateSign(root.transform, "Sign_HintPared", new Vector3(-4.72f, 1.7f, 0f), new Vector3(0.1f, 1.1f, 3.6f),
                "SIN $ NO TIRA · CAJA $80 SÍ", new Vector3(-4.45f, 1.7f, 0f), Quaternion.Euler(0f, 90f, 0f), new Color(0.12f, 0.1f, 0.04f), new Color(1f, 0.85f, 0.35f));
            CreateSign(root.transform, "Sign_Pasillo", new Vector3(0f, 3.72f, 16.15f), new Vector3(3.2f, 0.4f, 0.1f),
                "CUIDADO: VACÍO", new Vector3(0f, 3.72f, 16.0f), Quaternion.Euler(0f, 180f, 0f), new Color(0.18f, 0.06f, 0.06f), new Color(1f, 0.5f, 0.45f));
            CreateSign(root.transform, "Sign_Socket", new Vector3(6.72f, 2.35f, 10f), new Vector3(0.1f, 0.55f, 2.6f),
                "ENCHUFA EL PAQUETE", new Vector3(6.4f, 2.35f, 10f), Quaternion.Euler(0f, -90f, 0f), new Color(0.08f, 0.2f, 0.14f), new Color(0.55f, 1f, 0.75f));
            CreateSign(root.transform, "Sign_ArchivePared", new Vector3(-6.72f, 2.2f, 10f), new Vector3(0.1f, 0.7f, 3.4f),
                "SUELTA LA CAJA $80 EN UNA PARED", new Vector3(-6.4f, 2.2f, 10f), Quaternion.Euler(0f, 90f, 0f), new Color(0.12f, 0.08f, 0.04f), new Color(1f, 0.88f, 0.4f));
            CreateSign(root.transform, "Sign_Entregar", new Vector3(7.72f, 1.8f, 40.5f), new Vector3(0.1f, 0.7f, 3.2f),
                "SUELTA EL PAQUETE EN LA LOSA AZUL", new Vector3(7.4f, 1.8f, 40.5f), Quaternion.Euler(0f, -90f, 0f), new Color(0.06f, 0.12f, 0.22f), new Color(0.65f, 0.85f, 1f));
            CreateSign(root.transform, "Sign_Sellar", new Vector3(-5.72f, 1.8f, 54.2f), new Vector3(0.1f, 0.7f, 3.0f),
                "PAQUETE + MANTÉN E PARA SELLAR", new Vector3(-5.4f, 1.8f, 54.2f), Quaternion.Euler(0f, 90f, 0f), new Color(0.18f, 0.12f, 0.04f), new Color(1f, 0.85f, 0.4f));
            CreateSign(root.transform, "Door_Archive", new Vector3(0f, 3.62f, 4.05f), new Vector3(2.4f, 0.28f, 0.08f),
                "→ ARCHIVE", new Vector3(0f, 3.62f, 3.92f), Quaternion.Euler(0f, 180f, 0f), new Color(0.08f, 0.08f, 0.1f), new Color(1f, 0.9f, 0.45f));
            CreateSign(root.transform, "Door_Pasillo", new Vector3(0f, 3.62f, 16.05f), new Vector3(2.4f, 0.28f, 0.08f),
                "→ PASILLO", new Vector3(0f, 3.62f, 15.92f), Quaternion.Euler(0f, 180f, 0f), new Color(0.12f, 0.05f, 0.05f), new Color(1f, 0.55f, 0.45f));
            CreateSign(root.transform, "Door_Office", new Vector3(0f, 3.62f, 27.05f), new Vector3(2.5f, 0.28f, 0.08f),
                "→ OPEN OFFICE", new Vector3(0f, 3.62f, 26.92f), Quaternion.Euler(0f, 180f, 0f), new Color(0.06f, 0.1f, 0.16f), new Color(0.7f, 0.88f, 1f));
            CreateSign(root.transform, "Door_Executive", new Vector3(0f, 3.62f, 43.05f), new Vector3(2.5f, 0.28f, 0.08f),
                "→ EXECUTIVE", new Vector3(0f, 3.62f, 42.92f), Quaternion.Euler(0f, 180f, 0f), new Color(0.14f, 0.08f, 0.04f), new Color(1f, 0.82f, 0.45f));

            CreatePathChevrons(root.transform);
            var sillColor = new Color(1f, 0.82f, 0.22f);
            CreateStaticCube(root.transform, "Sill_Archive", new Vector3(0f, 0.05f, 4f), new Vector3(2.8f, 0.08f, 0.32f), sillColor);
            CreateStaticCube(root.transform, "Sill_Pasillo", new Vector3(0f, 0.05f, 16f), new Vector3(2.8f, 0.08f, 0.32f), new Color(0.95f, 0.35f, 0.28f));
            CreateStaticCube(root.transform, "Sill_Office", new Vector3(0f, 0.05f, 27f), new Vector3(2.8f, 0.08f, 0.32f), new Color(0.45f, 0.7f, 1f));
            CreateStaticCube(root.transform, "Sill_Executive", new Vector3(0f, 0.05f, 43f), new Vector3(2.8f, 0.08f, 0.32f), new Color(0.95f, 0.7f, 0.28f));

            var pit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pit.name = "VoidPit";
            pit.transform.SetParent(root.transform, false);
            pit.transform.position = new Vector3(0f, -8f, 28f);
            pit.transform.localScale = new Vector3(80f, 1f, 90f);
            pit.GetComponent<Collider>().isTrigger = true;
            SetColor(pit, new Color(0.04f, 0.04f, 0.06f));
            pit.AddComponent<VoidKillZone>();

            var playersRoot = new GameObject("Players");
            playersRoot.transform.SetParent(root.transform, false);
            CreatePlayer(playersRoot.transform, "Player1", p1Spawn, new Color(0.25f, 0.55f, 0.95f), LocalPlayerSlot.One, true, hubG, playerCount == 1 ? new Rect(0f, 0f, 1f, 1f) : new Rect(0f, 0.5f, 1f, 0.5f), audio: true, RoleKind.Runner);
            if (playerCount > 1)
            {
                CreatePlayer(playersRoot.transform, "Player2", p2Spawn, new Color(0.95f, 0.5f, 0.2f), LocalPlayerSlot.Two, false, hubG, new Rect(0f, 0f, 1f, 0.5f), audio: false, RoleKind.Anchor);
            }

            IgnorePlayerCollisions(playersRoot.transform);
            IgnorePackagePlayerCollisions(pkg, playersRoot.transform);

            var hudGo = new GameObject("GravityHUD");
            hudGo.transform.SetParent(root.transform, false);
            hudGo.AddComponent<GravityHud>();

            return root;
        }

        private static void AddPointLight(Transform parent, Vector3 pos, Color color, float intensity, float range = 16f)
        {
            var go = new GameObject("PointLight");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.color = color;
        }

        private static void DressRooms(Transform parent)
        {
            AddSkirting(parent, HubC, HubS, new Color(0.12f, 0.22f, 0.24f));
            AddSkirting(parent, ArcC, ArcS, new Color(0.28f, 0.22f, 0.16f));
            AddSkirting(parent, OffC, OffS, new Color(0.18f, 0.24f, 0.32f));
            AddSkirting(parent, ExeC, ExeS, new Color(0.22f, 0.14f, 0.1f));

            AddCeilingLamp(parent, HubC + new Vector3(0f, 2.2f, 0f), new Color(0.75f, 0.92f, 1f));
            AddCeilingLamp(parent, ArcC + new Vector3(-2.5f, 2.2f, 0f), new Color(1f, 0.92f, 0.7f));
            AddCeilingLamp(parent, ArcC + new Vector3(2.5f, 2.2f, 0f), new Color(1f, 0.92f, 0.7f));
            AddCeilingLamp(parent, OffC + new Vector3(-3f, 2.2f, -2f), new Color(0.8f, 0.9f, 1f));
            AddCeilingLamp(parent, OffC + new Vector3(3f, 2.2f, 2f), new Color(0.8f, 0.9f, 1f));
            AddCeilingLamp(parent, ExeC + new Vector3(0f, 2.2f, 0f), new Color(1f, 0.82f, 0.55f));
            AddCeilingLamp(parent, new Vector3(0f, 3.15f, 18.2f), new Color(1f, 0.5f, 0.38f));
            AddCeilingLamp(parent, new Vector3(0f, 3.15f, 21.5f), new Color(1f, 0.5f, 0.38f));
            AddCeilingLamp(parent, new Vector3(0f, 3.15f, 24.8f), new Color(1f, 0.5f, 0.38f));

            AddCeilingTiles(parent, HubC, HubS, new Color(0.2f, 0.32f, 0.34f));
            AddCeilingTiles(parent, ArcC, ArcS, new Color(0.4f, 0.36f, 0.3f));
            AddCeilingTiles(parent, OffC, OffS, new Color(0.3f, 0.36f, 0.44f));
            AddCeilingTiles(parent, ExeC, ExeS, new Color(0.36f, 0.26f, 0.2f));

            AddWindow(parent, "Win_HubW", HubC + new Vector3(-HubS.x * 0.5f + 0.22f, 0.35f, 0f), new Vector3(0.08f, 1.7f, 3.4f), new Color(0.45f, 0.75f, 0.95f));
            AddWindow(parent, "Win_ArcE", ArcC + new Vector3(ArcS.x * 0.5f - 0.22f, 0.45f, -2f), new Vector3(0.08f, 1.8f, 3.2f), new Color(0.95f, 0.82f, 0.45f));
            AddWindow(parent, "Win_OffW", OffC + new Vector3(-OffS.x * 0.5f + 0.22f, 0.4f, 0f), new Vector3(0.08f, 1.9f, 4.4f), new Color(0.4f, 0.7f, 1f));
            AddWindow(parent, "Win_ExeE", ExeC + new Vector3(ExeS.x * 0.5f - 0.22f, 0.5f, 0f), new Vector3(0.08f, 1.6f, 3.6f), new Color(0.95f, 0.7f, 0.35f));
        }

        private static void AddCeilingTiles(Transform parent, Vector3 center, Vector3 size, Color color)
        {
            var y = center.y + size.y * 0.5f - 0.22f;
            const float step = 2.4f;
            var x0 = center.x - size.x * 0.5f + 1.2f;
            var z0 = center.z - size.z * 0.5f + 1.2f;
            var x1 = center.x + size.x * 0.5f - 1.2f;
            var z1 = center.z + size.z * 0.5f - 1.2f;
            for (var x = x0; x <= x1 + 0.05f; x += step)
            {
                for (var z = z0; z <= z1 + 0.05f; z += step)
                {
                    var tile = CreateStaticCube(parent, "CeilTile", new Vector3(x, y, z), new Vector3(2.05f, 0.04f, 2.05f), color);
                    DisableCollider(tile);
                }
            }
        }

        private static void AddSkirting(Transform parent, Vector3 center, Vector3 size, Color color)
        {
            var y = center.y - size.y * 0.5f + 0.08f;
            CreateStaticCube(parent, "SkirtE", new Vector3(center.x + size.x * 0.5f - 0.08f, y, center.z), new Vector3(0.12f, 0.16f, size.z), color);
            CreateStaticCube(parent, "SkirtW", new Vector3(center.x - size.x * 0.5f + 0.08f, y, center.z), new Vector3(0.12f, 0.16f, size.z), color);
        }

        private static void AddCeilingLamp(Transform parent, Vector3 pos, Color glow)
        {
            var lamp = CreateStaticCube(parent, "CeilLamp", pos, new Vector3(0.9f, 0.08f, 0.9f), new Color(1f, 0.96f, 0.85f));
            DisableCollider(lamp);
            AddPointLight(parent, pos + Vector3.down * 0.15f, glow, 0.55f);
        }

        private static void AddWindow(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            DisableCollider(go);
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    renderer.sharedMaterial = new Material(shader) { color = color };
                }
            }
        }

        private static GravityManager CreateGravity(Transform parent, string name, Vector3 center)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var gm = go.AddComponent<GravityManager>();
            gm.Configure(go.transform, 1f);
            var outline = go.AddComponent<DominantValuableOutline>();
            outline.Bind(gm);
            go.AddComponent<GravityTelegraphArrow>();
            go.AddComponent<GravityDownPad>();
            return gm;
        }

        private static void CreateRoomVolume(Transform parent, string id, Vector3 center, Vector3 size, GravityManager gravity, bool inherit)
        {
            var go = new GameObject("Room_" + id);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(size.x * 1.04f, size.y * 1.12f, size.z * 1.04f);
            var vol = go.AddComponent<RoomVolume>();
            vol.Configure(id, gravity, inherit);
        }

        private static void CreateRoom(Transform parent, string name, Vector3 center, Vector3 size, Color floor, Color wall, bool southDoor, bool northDoor)
        {
            var root = new GameObject("RoomMesh_" + name);
            root.transform.SetParent(parent, false);
            root.transform.position = center;

            CreateCube(root.transform, "Floor", new Vector3(0f, -size.y * 0.5f, 0f), new Vector3(size.x + WallT, WallT, size.z + WallT), floor);
            var stripe = CreateCube(root.transform, "FloorStripe", new Vector3(0f, -size.y * 0.5f + WallT * 0.5f + 0.02f, 0f), new Vector3(0.28f, 0.03f, size.z * 0.82f), new Color(0.25f, 0.62f, 0.78f));
            DisableCollider(stripe);
            CreateCube(root.transform, "Ceiling", new Vector3(0f, size.y * 0.5f, 0f), new Vector3(size.x + WallT, WallT, size.z + WallT), wall * 1.15f);
            var ceilStripe = CreateCube(root.transform, "CeilingStripe", new Vector3(0f, size.y * 0.5f - WallT * 0.5f - 0.02f, 0f), new Vector3(0.22f, 0.03f, size.z * 0.82f), new Color(0.95f, 0.45f, 0.2f));
            DisableCollider(ceilStripe);
            CreateWallOnZ(root.transform, "Wall_N", new Vector3(0f, 0f, size.z * 0.5f), size, wall, northDoor);
            CreateWallOnZ(root.transform, "Wall_S", new Vector3(0f, 0f, -size.z * 0.5f), size, wall, southDoor);
            CreateCube(root.transform, "Wall_E", new Vector3(size.x * 0.5f, 0f, 0f), new Vector3(WallT, size.y + WallT, size.z + WallT), wall);
            CreateCube(root.transform, "Wall_W", new Vector3(-size.x * 0.5f, 0f, 0f), new Vector3(WallT, size.y + WallT, size.z + WallT), wall);
        }

        private static void CreateCatwalk(Transform parent, string name, Vector3 center, Vector3 size, Color floor)
        {
            var root = new GameObject("RoomMesh_" + name);
            root.transform.SetParent(parent, false);
            root.transform.position = center;
            CreateCube(root.transform, "Floor", new Vector3(0f, -size.y * 0.5f, 0f), new Vector3(size.x, WallT, size.z), floor);
            var stripe = CreateCube(root.transform, "FloorStripe", new Vector3(0f, -size.y * 0.5f + WallT * 0.5f + 0.02f, 0f), new Vector3(0.22f, 0.03f, size.z * 0.9f), new Color(0.25f, 0.62f, 0.78f));
            DisableCollider(stripe);
            var curbH = 0.58f;
            var curbY = -size.y * 0.5f + WallT * 0.5f + curbH * 0.5f;
            CreateCube(root.transform, "Curb_L", new Vector3(-size.x * 0.5f, curbY, 0f), new Vector3(0.16f, curbH, size.z), new Color(0.85f, 0.25f, 0.2f));
            CreateCube(root.transform, "Curb_R", new Vector3(size.x * 0.5f, curbY, 0f), new Vector3(0.16f, curbH, size.z), new Color(0.85f, 0.25f, 0.2f));
            for (var i = -2; i <= 2; i++)
            {
                var z = i * 1.8f;
                var hazard = CreateCube(root.transform, "Hazard", new Vector3(0f, -size.y * 0.5f + WallT * 0.5f + 0.03f, z), new Vector3(size.x * 0.85f, 0.02f, 0.28f), i % 2 == 0 ? new Color(1f, 0.85f, 0.15f) : new Color(0.12f, 0.12f, 0.12f));
                DisableCollider(hazard);
            }
            var postZs = new[] { -size.z * 0.35f, 0f, size.z * 0.35f };
            foreach (var z in postZs)
            {
                CreateCube(root.transform, "Post_L", new Vector3(-size.x * 0.5f, -size.y * 0.15f, z), new Vector3(0.12f, size.y * 0.5f, 0.12f), new Color(0.7f, 0.25f, 0.2f));
                CreateCube(root.transform, "Post_R", new Vector3(size.x * 0.5f, -size.y * 0.15f, z), new Vector3(0.12f, size.y * 0.5f, 0.12f), new Color(0.7f, 0.25f, 0.2f));
            }

            var railColor = new Color(0.85f, 0.28f, 0.22f);
            CreateCube(root.transform, "Rail_L", new Vector3(-size.x * 0.5f, -1.15f, 0f), new Vector3(0.08f, 0.08f, size.z * 0.78f), railColor);
            CreateCube(root.transform, "Rail_R", new Vector3(size.x * 0.5f, -1.15f, 0f), new Vector3(0.08f, 0.08f, size.z * 0.78f), railColor);
            CreateCube(root.transform, "Rail_L2", new Vector3(-size.x * 0.5f, -0.55f, 0f), new Vector3(0.08f, 0.08f, size.z * 0.78f), railColor);
            CreateCube(root.transform, "Rail_R2", new Vector3(size.x * 0.5f, -0.55f, 0f), new Vector3(0.08f, 0.08f, size.z * 0.78f), railColor);
        }

        private static void CreateWallOnZ(Transform parent, string name, Vector3 localPos, Vector3 roomSize, Color color, bool door)
        {
            var scale = new Vector3(roomSize.x + WallT, roomSize.y + WallT, WallT);
            if (!door)
            {
                CreateCube(parent, name, localPos, scale, color);
                return;
            }

            var sideW = (scale.x - DoorW) * 0.5f;
            if (sideW > 0.05f)
            {
                CreateCube(parent, name + "_L", localPos + new Vector3(-DoorW * 0.5f - sideW * 0.5f, 0f, 0f), new Vector3(sideW, scale.y, scale.z), color);
                CreateCube(parent, name + "_R", localPos + new Vector3(DoorW * 0.5f + sideW * 0.5f, 0f, 0f), new Vector3(sideW, scale.y, scale.z), color);
            }

            var doorMinY = -roomSize.y * 0.5f - localPos.y;
            var doorMaxY = doorMinY + DoorH;
            var wallTop = scale.y * 0.5f;
            var lintelH = wallTop - doorMaxY;
            if (lintelH > 0.05f)
            {
                var lintelY = doorMaxY + lintelH * 0.5f;
                CreateCube(parent, name + "_Lint", localPos + new Vector3(0f, lintelY, 0f), new Vector3(DoorW, lintelH, scale.z), color);
            }

            var frame = new Color(1f, 0.82f, 0.2f);
            var doorCenterY = doorMinY + DoorH * 0.5f;
            CreateCube(parent, name + "_FrameL", localPos + new Vector3(-DoorW * 0.5f, doorCenterY, 0f), new Vector3(0.14f, DoorH, scale.z + 0.08f), frame);
            CreateCube(parent, name + "_FrameR", localPos + new Vector3(DoorW * 0.5f, doorCenterY, 0f), new Vector3(0.14f, DoorH, scale.z + 0.08f), frame);
            CreateCube(parent, name + "_FrameT", localPos + new Vector3(0f, doorMaxY, 0f), new Vector3(DoorW + 0.14f, 0.14f, scale.z + 0.08f), frame);
        }

        private static void CreatePathChevrons(Transform parent)
        {
            var zs = new[] { 2.2f, 5.8f, 15.6f, 21.5f, 27.3f, 34.5f, 43.2f, 48.8f, 52.6f };
            foreach (var z in zs)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "PathChevron";
                go.transform.SetParent(parent, false);
                go.transform.position = new Vector3(0f, 0.24f, z);
                go.transform.localScale = new Vector3(0.55f, 0.05f, 0.8f);
                DisableCollider(go);
                SetColor(go, new Color(1f, 0.85f, 0.2f));
            }
        }

        private static GameObject CreateValuable(Transform parent, string name, Vector3 position, Vector3 scale, int price, GravityManager gravity, Color color, PrimitiveType primitive)
        {
            var go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            SetColor(go, color);

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 3.5f + price * 0.045f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.useGravity = false;
            rb.sleepThreshold = 0.12f;
            rb.maxAngularVelocity = 14f;
            rb.linearDamping = 0.25f;
            rb.angularDamping = 0.4f;

            go.AddComponent<Grabbable>();
            var body = go.AddComponent<GravityBody>();
            body.SetManager(gravity);
            var valuable = go.AddComponent<ValuableItem>();
            valuable.Configure(price, gravity);
            var tagHeight = primitive is PrimitiveType.Capsule or PrimitiveType.Cylinder
                ? scale.y * 2f
                : scale.y;
            AttachPriceTag(go.transform, price, tagHeight, "$" + price);
            return go;
        }

        private static void AttachPriceTag(Transform target, int price, float height, string text = null)
        {
            var host = new GameObject(target.name + "_Price");
            host.transform.SetParent(target.root, true);
            var follow = host.AddComponent<FollowBillboard>();
            follow.Configure(target, Vector3.up * (height * 0.5f + 0.28f));
            WorldLabel.Create(host.transform, "Text", text ?? ("$" + price), Vector3.zero, new Color(1f, 0.92f, 0.3f), 0.1f);
        }

        private static GameObject CreateProp(Transform parent, string name, Vector3 position, Vector3 scale, Color color, GravityManager gravity)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            SetColor(go, color);

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 4f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.useGravity = false;
            rb.sleepThreshold = 0.12f;
            rb.maxAngularVelocity = 14f;
            rb.linearDamping = 0.25f;
            rb.angularDamping = 0.4f;

            go.AddComponent<Grabbable>();
            var body = go.AddComponent<GravityBody>();
            body.SetManager(gravity);
            go.AddComponent<SpawnHome>();
            return go;
        }

        private static GameObject AddLocalVisual(Transform parent, PrimitiveType type, Vector3 localPos, Vector3 localScale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = parent.name + "_Part";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            DisableCollider(go);
            SetColor(go, color);
            return go;
        }

        private static GameObject CreateStaticCube(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            SetColor(go, color);
            return go;
        }

        private static GameObject CreatePackage(Transform parent, Vector3 position, GravityManager gravity)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "MissionPackage";
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.7f, 0.45f, 0.55f);
            SetColor(go, new Color(0.95f, 0.55f, 0.12f));
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "PackageStripe";
            stripe.transform.SetParent(go.transform, false);
            stripe.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            stripe.transform.localScale = new Vector3(1.02f, 0.22f, 1.02f);
            DisableCollider(stripe);
            SetColor(stripe, new Color(0.12f, 0.55f, 0.28f));

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.useGravity = false;
            rb.sleepThreshold = 0.12f;
            rb.maxAngularVelocity = 14f;
            rb.linearDamping = 0.25f;
            rb.angularDamping = 0.4f;

            go.AddComponent<Grabbable>();
            var body = go.AddComponent<GravityBody>();
            body.SetManager(gravity);
            go.AddComponent<MissionPackage>();
            return go;
        }

        private static void CreateObjective(
            Transform parent,
            string name,
            int index,
            string label,
            Vector3 pos,
            Vector3 visualScale,
            Vector3 triggerLocalSize,
            float seconds,
            bool package,
            bool player,
            bool hold,
            Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = visualScale;
            var box = go.GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = triggerLocalSize;
            SetColor(go, color * 0.7f);

            var trigger = go.AddComponent<ObjectiveTrigger>();
            trigger.Configure(index, label, seconds, package, player, hold);

            var host = new GameObject(name + "_Label");
            host.transform.SetParent(parent, false);
            host.transform.position = pos + Vector3.up * 1.15f;
            WorldLabel.Create(host.transform, "Text", label.ToUpperInvariant(), Vector3.zero, Color.white, 0.1f);
            var billboard = host.AddComponent<FollowBillboard>();
            billboard.Configure(host.transform, Vector3.zero);
        }

        private static void CreateSign(
            Transform parent,
            string name,
            Vector3 platePos,
            Vector3 plateScale,
            string text,
            Vector3 labelPos,
            Quaternion labelRot,
            Color plate,
            Color letters)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = platePos;
            go.transform.localScale = plateScale;
            SetColor(go, plate);
            DisableCollider(go);

            var host = new GameObject(name + "_Label");
            host.transform.SetParent(parent, false);
            host.transform.position = labelPos;
            host.transform.rotation = labelRot;
            WorldLabel.Create(host.transform, "Text", text, Vector3.zero, letters, 0.11f);
        }

        private static GameObject CreatePlayer(
            Transform parent,
            string name,
            Vector3 position,
            Color color,
            LocalPlayerSlot slot,
            bool mouseLook,
            GravityManager gravity,
            Rect camRect,
            bool audio,
            RoleKind role)
        {
            var player = new GameObject(name);
            player.tag = "Player";
            player.transform.SetParent(parent, false);
            player.transform.position = position;

            var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vis.name = "Mesh";
            vis.transform.SetParent(player.transform, false);
            vis.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            Object.DestroyImmediate(vis.GetComponent<CapsuleCollider>());
            SetColor(vis, color);
            var bodyLayer = slot == LocalPlayerSlot.One ? 8 : 9;
            SetLayerRecursively(vis, bodyLayer);

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.skinWidth = 0.08f;
            cc.minMoveDistance = 0f;
            cc.stepOffset = 0.28f;
            cc.slopeLimit = 50f;

            var camGo = new GameObject(name + "_Camera");
            camGo.transform.SetParent(player.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camGo.transform.localRotation = Quaternion.identity;
            if (slot == LocalPlayerSlot.One)
            {
                camGo.tag = "MainCamera";
            }

            var hold = new GameObject("HoldPoint");
            hold.transform.SetParent(camGo.transform, false);
            hold.transform.localPosition = new Vector3(0.35f, -0.2f, 1.15f);

            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.08f;
            cam.farClipPlane = 90f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.055f, 0.07f);
            cam.rect = camRect;
            cam.depth = slot == LocalPlayerSlot.One ? 0f : 1f;
            cam.cullingMask &= ~(1 << bodyLayer);
            if (audio)
            {
                camGo.AddComponent<AudioListener>();
            }

            var input = player.AddComponent<LocalPlayerInput>();
            input.Configure(slot, mouseLook, cam);

            var motor = player.AddComponent<PlayerMotor>();
            motor.Configure(camGo.transform, gravity);

            var interactor = player.AddComponent<PlayerInteractor>();
            interactor.Configure(hold.transform);
            var roleCmp = player.AddComponent<PlayerRole>();
            roleCmp.Configure(role);

            player.AddComponent<GravityCompass>();
            player.AddComponent<PredictedFlipWall>();
            player.AddComponent<ObjectiveWaypoint>();
            player.AddComponent<PlayerPing>();
            player.AddComponent<PlayerEmote>();

            var tagColor = slot == LocalPlayerSlot.One
                ? new Color(0.55f, 0.8f, 1f)
                : new Color(1f, 0.7f, 0.35f);
            var host = new GameObject(name + "_Tag");
            var follow = host.AddComponent<FollowBillboard>();
            follow.Configure(player.transform, Vector3.up * 2.05f);
            WorldLabel.Create(host.transform, "Text", slot == LocalPlayerSlot.One ? "P1" : "P2", Vector3.zero, tagColor, 0.09f);
            SetLayerRecursively(host, bodyLayer);

            return player;
        }

        private static void IgnorePackagePlayerCollisions(GameObject pkg, Transform playersRoot)
        {
            if (pkg == null || playersRoot == null)
            {
                return;
            }

            var pkgCol = pkg.GetComponent<Collider>();
            if (pkgCol == null)
            {
                return;
            }

            var ccs = playersRoot.GetComponentsInChildren<CharacterController>();
            foreach (var cc in ccs)
            {
                if (cc != null)
                {
                    Physics.IgnoreCollision(pkgCol, cc, true);
                }
            }
        }

        private static void IgnorePlayerCollisions(Transform playersRoot)
        {
            var ccs = playersRoot.GetComponentsInChildren<CharacterController>();
            for (var i = 0; i < ccs.Length; i++)
            {
                for (var j = i + 1; j < ccs.Length; j++)
                {
                    if (ccs[i] != null && ccs[j] != null)
                    {
                        Physics.IgnoreCollision(ccs[i], ccs[j], true);
                    }
                }
            }
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            SetColor(go, color);
            return go;
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            if (go == null)
            {
                return;
            }

            go.layer = layer;
            var t = go.transform;
            for (var i = 0; i < t.childCount; i++)
            {
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
            }
        }

        private static void DisableCollider(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
        }

        private static void SetColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var shader = Shader.Find("Standard")
                         ?? Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Unlit/Color");
            if (shader == null)
            {
                return;
            }

            renderer.sharedMaterial = new Material(shader) { color = color };
        }
    }

    public static class OfficeFloorPlayGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureFloor()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (Object.FindAnyObjectByType<MatchDirector>() != null)
            {
                return;
            }

            Debug.LogWarning(
                "[GravityReceipt] Escena sin MatchDirector. Reconstruyendo Office Floor A (2p) en Play Mode.");

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
            {
                Object.DestroyImmediate(go);
            }

            OfficeFloorFactory.Build(2);
        }
    }
}
