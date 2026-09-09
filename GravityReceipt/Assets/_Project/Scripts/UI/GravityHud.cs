using GravityReceipt.Gravity;
using GravityReceipt.Interaction;
using GravityReceipt.Mission;
using GravityReceipt.Player;
using GravityReceipt.World;
using UnityEngine;
using UnityEngine.UI;

namespace GravityReceipt.UI
{
    /// <summary>
    /// HUD: gravedad, telegráfo, objetivos, paquete, timer, roles, rematch.
    /// </summary>
    public sealed class GravityHud : MonoBehaviour
    {
        [SerializeField] private Text statusText;
        [SerializeField] private Text matchText;
        [SerializeField] private Text helpText;
        [SerializeField] private Text helpP1Text;
        [SerializeField] private Text centerText;
        private Text _crossP1;
        private Text _crossP2;
        private GameObject _splitBar;

        private void Awake()
        {
            if (statusText is null)
            {
                EnsureCanvas();
            }
        }

        private void Start()
        {
            var playerCount = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None).Length;
            if (playerCount >= 2)
            {
                return;
            }

            if (_crossP2 is not null)
            {
                _crossP2.gameObject.SetActive(false);
            }

            if (_crossP1 is not null)
            {
                var rt = _crossP1.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
            }

            if (helpText is not null)
            {
                helpText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (statusText is null)
            {
                return;
            }

            var p1 = FindPlayer(LocalPlayerSlot.One);
            var p2 = FindPlayer(LocalPlayerSlot.Two);
            var gravity = p1 is { Gravity: { } g } ? g : FindAnyObjectByType<GravityManager>();
            var room1 = p1 is not null ? RoomRegistry.FindRoom(p1.transform.position) : null;
            var room2 = p2 is not null ? RoomRegistry.FindRoom(p2.transform.position) : null;
            var roomName = room1 is { RoomId: { Length: > 0 } id } ? id : "—";
            var room2Name = room2 is { RoomId: { Length: > 0 } id2 } ? id2 : null;

            var dominant = gravity is { Dominant: { } d } ? $"{d.name} (${d.Price})" : "ninguno";
            var telegraph = gravity is null
                ? ""
                : gravity.IsAnchored
                    ? "ANCLA"
                    : gravity.IsTelegraphing
                        ? $"FLIP en {1f - gravity.TelegraphNormalized:0.0}s"
                        : "estable";
            var gDir = gravity is not null ? DirName(gravity.CurrentDirection) : "?";
            statusText.text = room2Name is { Length: > 0 }
                ? $"P1 {roomName}  |  P2 {room2Name}  |  g → {gDir}  |  Dom: {dominant}  |  {telegraph}"
                : $"Sala: {roomName}  |  g → {gDir}  |  Dominante: {dominant}  |  {telegraph}";
            statusText.color = gravity is { IsTelegraphing: true } ? new Color(1f, 0.9f, 0.2f) : Color.white;

            var match = MatchDirector.Instance;
            var pkg = FindAnyObjectByType<MissionPackage>();
            var rec = MatchHighlightRecorder.Instance;
            if (matchText is not null && match is not null)
            {
                var t = Mathf.CeilToInt(match.RemainingSeconds);
                var mm = t / 60;
                var ss = t % 60;
                var hearts = Hearts(pkg);
                var o1 = Mark(match.IsObjectiveComplete(0));
                var o2 = Mark(match.IsObjectiveComplete(1));
                var o3 = Mark(match.IsObjectiveComplete(2));
                matchText.text =
                    $"⏱ {mm:00}:{ss:00}   Paquete {hearts}  dest {pkg?.Destructions ?? 0}/{pkg?.MaxDestructions ?? 3}\n" +
                    $"{o1} Enchufar   {o2} Entregar   {o3} Sellar   ({match.ObjectivesDone}/{match.ObjectivesToWin})";
            }

            if (helpP1Text is not null)
            {
                var r1 = RoleOf(LocalPlayerSlot.One);
                var wind = WindUp(p1);
                helpP1Text.text = $"P1 [{r1}] WASD+ratón  E agarrar  Q ping  Shift sprint  F ancla  Tab rol{wind}";
            }

            if (helpText is not null)
            {
                var r2 = RoleOf(LocalPlayerSlot.Two);
                helpText.text = p2 is not null
                    ? $"P2 [{r2}] flechas+numpad 8465  RShift agarrar  / ping  Alt sprint  KP0 ancla  KP7 rol"
                    : $"P1 [{RoleOf(LocalPlayerSlot.One)}] WASD+ratón  E agarrar  Q ping  Shift sprint  F ancla";
            }

            if (centerText is not null && match is not null)
            {
                if (match.Phase == MatchPhase.Playing)
                {
                    centerText.text = string.Empty;
                }
                else
                {
                    var title = match.Phase == MatchPhase.Won ? "GANASTE" : "PERDISTE";
                    var moment = rec is not null ? rec.Summary : "—";
                    centerText.text = $"{title}\n{match.EndReason}\nMomento del partido: {moment}\nPulsa R para rematch";
                    centerText.color = match.Phase == MatchPhase.Won
                        ? new Color(0.45f, 1f, 0.55f)
                        : new Color(1f, 0.45f, 0.4f);
                }
            }
        }

        private static string DirName(Vector3 d)
        {
            if (Vector3.Dot(d, Vector3.down) > 0.9f) return "abajo";
            if (Vector3.Dot(d, Vector3.up) > 0.9f) return "arriba";
            if (Vector3.Dot(d, Vector3.right) > 0.9f) return "este";
            if (Vector3.Dot(d, Vector3.left) > 0.9f) return "oeste";
            if (Vector3.Dot(d, Vector3.forward) > 0.9f) return "norte";
            if (Vector3.Dot(d, Vector3.back) > 0.9f) return "sur";
            return d.ToString();
        }

        private static string Hearts(MissionPackage pkg)
        {
            if (pkg is null)
            {
                return "—";
            }

            var s = string.Empty;
            for (var i = 0; i < pkg.LivesMax; i++)
            {
                s += i < pkg.Lives ? "♥" : "♡";
            }

            return s;
        }

        private static string Mark(bool done) => done ? "[x]" : "[ ]";

        private static string RoleOf(LocalPlayerSlot slot)
        {
            var players = FindObjectsByType<PlayerRole>(FindObjectsSortMode.None);
            foreach (var r in players)
            {
                var input = r.GetComponent<LocalPlayerInput>();
                if (input is not null && input.Slot == slot)
                {
                    return r.RoleLabel;
                }
            }

            return "—";
        }

        private static string WindUp(PlayerMotor motor)
        {
            if (motor is null)
            {
                return string.Empty;
            }

            var inter = motor.GetComponent<PlayerInteractor>();
            if (inter is not { WindUpNormalized: > 0.05f and < 1f })
            {
                return string.Empty;
            }

            return $"  agarrando {inter.WindUpNormalized:0.0}";
        }

        private static PlayerMotor FindPlayer(LocalPlayerSlot slot)
        {
            var motors = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None);
            foreach (var m in motors)
            {
                var input = m.GetComponent<LocalPlayerInput>();
                if (input is not null && input.Slot == slot)
                {
                    return m;
                }
            }

            return motors.Length > 0 ? motors[0] : null;
        }

        private void EnsureCanvas()
        {
            var canvasGo = new GameObject("GravityHUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            statusText = MakeText(canvasGo.transform, "Status", new Vector2(0f, 18f), new Vector2(0.5f, 0.5f), new Vector2(1600f, 36f), 20, TextAnchor.MiddleCenter);
            matchText = MakeText(canvasGo.transform, "Match", new Vector2(0f, -18f), new Vector2(0.5f, 0.5f), new Vector2(1600f, 52f), 18, TextAnchor.MiddleCenter);
            helpP1Text = MakeText(canvasGo.transform, "HelpP1", new Vector2(16f, -10f), new Vector2(0f, 1f), new Vector2(1600f, 28f), 16, TextAnchor.UpperLeft);
            helpP1Text.color = new Color(0.7f, 0.85f, 1f);
            helpText = MakeText(canvasGo.transform, "Help", new Vector2(16f, 12f), new Vector2(0f, 0f), new Vector2(1600f, 32f), 16, TextAnchor.LowerLeft);
            helpText.color = new Color(0.85f, 0.9f, 1f);
            centerText = MakeText(canvasGo.transform, "Center", new Vector2(0f, 80f), new Vector2(0.5f, 0.5f), new Vector2(900f, 240f), 34, TextAnchor.MiddleCenter);
            centerText.color = Color.white;

            _crossP1 = MakeCross(canvasGo.transform, "CrossP1", new Vector2(0.5f, 0.75f));
            _crossP2 = MakeCross(canvasGo.transform, "CrossP2", new Vector2(0.5f, 0.25f));
        }

        private static Text MakeCross(Transform parent, string name, Vector2 anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.color = new Color(1f, 1f, 1f, 0.85f);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = "+";
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);
            var rt = text.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(32f, 32f);
            return text;
        }

        private static Text MakeText(Transform parent, string name, Vector2 anchored, Vector2 anchor, Vector2 size, int fontSize, TextAnchor align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = align;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);
            var rt = text.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchored;
            rt.sizeDelta = size;
            return text;
        }
    }
}
