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
        private MatchDirector _boundMatch;
        private string _toast = string.Empty;
        private float _toastUntil;

        private void Awake()
        {
            if (statusText == null)
            {
                EnsureCanvas();
            }
        }

        private void OnEnable()
        {
            TryBindMatch();
        }

        private void OnDisable()
        {
            UnbindMatch();
        }

        private void Start()
        {
            TryBindMatch();
            var playerCount = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None).Length;
            if (playerCount >= 2)
            {
                return;
            }

            if (_crossP2 != null)
            {
                _crossP2.gameObject.SetActive(false);
            }

            if (_crossP1 != null)
            {
                var rt = _crossP1.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
            }

            if (helpText != null)
            {
                helpText.gameObject.SetActive(false);
            }

            if (_splitBar != null)
            {
                _splitBar.SetActive(false);
            }
        }

        private void Update()
        {
            if (statusText == null)
            {
                return;
            }

            var p1 = FindPlayer(LocalPlayerSlot.One);
            var p2 = FindPlayer(LocalPlayerSlot.Two);
            var gravity = p1 != null && p1.Gravity != null ? p1.Gravity : FindAnyObjectByType<GravityManager>();
            var room1 = p1 != null ? RoomRegistry.FindRoom(p1.transform.position) : null;
            var room2 = p2 != null ? RoomRegistry.FindRoom(p2.transform.position) : null;
            var roomName = room1 != null && room1.RoomId.Length > 0 ? room1.RoomId : "—";
            var room2Name = room2 != null && room2.RoomId.Length > 0 ? room2.RoomId : null;

            var dominant = gravity != null && gravity.Dominant != null
                ? $"{gravity.Dominant.name} (${gravity.Dominant.Price})"
                : "ninguno";
            var telegraph = gravity == null
                ? ""
                : gravity.IsAnchored
                    ? "ANCLA"
                    : gravity.IsTelegraphing
                        ? $"FLIP en {1f - gravity.TelegraphNormalized:0.0}s"
                        : "estable";
            var gDir = gravity != null ? DirName(gravity.CurrentDirection) : "?";
            statusText.text = !string.IsNullOrEmpty(room2Name)
                ? $"P1 {roomName}  |  P2 {room2Name}  |  g → {gDir}  |  Dom: {dominant}  |  {telegraph}"
                : $"Sala: {roomName}  |  g → {gDir}  |  Dominante: {dominant}  |  {telegraph}";
            statusText.color = gravity != null && gravity.IsTelegraphing ? new Color(1f, 0.9f, 0.2f) : Color.white;

            var match = MatchDirector.Instance;
            var pkg = FindAnyObjectByType<MissionPackage>();
            var rec = MatchHighlightRecorder.Instance;
            if (matchText != null && match != null)
            {
                var t = Mathf.CeilToInt(match.RemainingSeconds);
                var mm = t / 60;
                var ss = t % 60;
                var hearts = Hearts(pkg);
                var o1 = Mark(match.IsObjectiveComplete(0));
                var o2 = match.IsObjectiveComplete(0) ? Mark(match.IsObjectiveComplete(1)) : "[-]";
                var o3 = match.IsObjectiveComplete(1) ? Mark(match.IsObjectiveComplete(2)) : "[-]";
                matchText.color = t <= 60
                    ? new Color(1f, 0.45f, 0.4f)
                    : Color.white;
                matchText.text =
                    $"⏱ {mm:00}:{ss:00}   Paquete {hearts}  dest {(pkg != null ? pkg.Destructions : 0)}/{(pkg != null ? pkg.MaxDestructions : 3)}\n" +
                    $"{o1} Enchufar   {o2} Entregar   {o3} Sellar   ({match.ObjectivesDone}/{match.ObjectivesToWin})" +
                    ObjectiveProgressSuffix();
            }

            if (helpP1Text != null)
            {
                var r1 = RoleOf(LocalPlayerSlot.One);
                var wind = WindUp(p1);
                helpP1Text.text = $"P1 [{r1}] WASD+ratón  E agarrar  Q ping  Shift sprint  F ancla  Tab rol  F5 restart{wind}";
            }

            if (helpText != null)
            {
                var r2 = RoleOf(LocalPlayerSlot.Two);
                helpText.text = p2 != null
                    ? $"P2 [{r2}] flechas  J/L+I/K mirar  RShift agarrar  / ping  Alt sprint  KP0 ancla  KP7 rol"
                    : $"P1 [{RoleOf(LocalPlayerSlot.One)}] WASD+ratón  E agarrar  Q ping  Shift sprint  F ancla";
            }

            if (centerText != null && match != null)
            {
                if (match.Phase != MatchPhase.Playing)
                {
                    var title = match.Phase == MatchPhase.Won ? "GANASTE" : "PERDISTE";
                    var moment = rec != null ? rec.Summary : "—";
                    centerText.text = $"{title}\n{match.EndReason}\nMomento del partido: {moment}\nPulsa R para rematch";
                    centerText.color = match.Phase == MatchPhase.Won
                        ? new Color(0.45f, 1f, 0.55f)
                        : new Color(1f, 0.45f, 0.4f);
                }
                else if (TryTelegraphBanner(p1, p2, out var banner, out var bannerColor))
                {
                    centerText.text = banner;
                    centerText.color = bannerColor;
                }
                else if (Time.unscaledTime < _toastUntil && _toast.Length > 0)
                {
                    centerText.text = _toast;
                    centerText.color = new Color(0.55f, 1f, 0.65f);
                }
                else if (Time.timeSinceLevelLoad < 9f)
                {
                    centerText.text = "LA GRAVEDAD SIGUE AL OBJETO MÁS CARO\nLos cubos grises no cuentan · la caja $80 sí";
                    centerText.color = new Color(1f, 0.92f, 0.4f);
                }
                else
                {
                    centerText.text = string.Empty;
                }
            }
            TintCross(_crossP1, p1);
            TintCross(_crossP2, p2);

            if (Input.GetKeyDown(KeyCode.F8))
            {
                var name = $"GravityReceipt_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
                ScreenCapture.CaptureScreenshot(name);
                Debug.Log("[GravityReceipt] Screenshot: " + name);
            }
        }

        private static void TintCross(Text cross, PlayerMotor motor)
        {
            if (cross == null)
            {
                return;
            }

            var inter = motor != null ? motor.GetComponent<PlayerInteractor>() : null;
            var winding = inter != null && inter.WindUpNormalized > 0.05f;
            var looking = inter != null && inter.HasLookTarget;
            cross.color = winding
                ? new Color(1f, 0.55f, 0.15f)
                : looking
                    ? new Color(1f, 0.92f, 0.25f)
                    : new Color(1f, 1f, 1f, 0.85f);
        }

        private static string ObjectiveProgressSuffix()
        {
            var triggers = FindObjectsByType<ObjectiveTrigger>(FindObjectsSortMode.None);
            foreach (var t in triggers)
            {
                if (t == null || t.IsDone || t.ProgressNormalized <= 0.02f)
                {
                    continue;
                }

                return $"\n{t.Label}… {t.ProgressNormalized:0%}";
            }

            return string.Empty;
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
            if (pkg == null)
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
                if (r == null)
                {
                    continue;
                }

                var input = r.GetComponent<LocalPlayerInput>();
                if (input != null && input.Slot == slot)
                {
                    if (r.Role == RoleKind.Anchor && r.AnchorCooldownLeft > 0.2f)
                    {
                        return $"Anchor CD {r.AnchorCooldownLeft:0}";
                    }

                    return r.RoleLabel;
                }
            }

            return "—";
        }

        private static string WindUp(PlayerMotor motor)
        {
            if (motor == null)
            {
                return string.Empty;
            }

            var inter = motor.GetComponent<PlayerInteractor>();
            if (inter == null || inter.WindUpNormalized <= 0.05f || inter.WindUpNormalized >= 1f)
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
                if (m == null)
                {
                    continue;
                }

                var input = m.GetComponent<LocalPlayerInput>();
                if (input != null && input.Slot == slot)
                {
                    return m;
                }
            }

            return null;
        }

        private static bool TryTelegraphBanner(PlayerMotor p1, PlayerMotor p2, out string banner, out Color color)
        {
            banner = string.Empty;
            color = new Color(1f, 0.85f, 0.2f);
            var g = FirstTelegraph(p1, p2);
            if (g == null)
            {
                return false;
            }

            var dir = DirName(g.PendingDirection);
            var item = g.Dominant != null ? $"${g.Dominant.Price}" : "el objeto caro";
            banner = $"¡FLIP!\nLa gravedad va hacia {dir}\nSigue a {item}";
            color = Color.Lerp(new Color(1f, 0.92f, 0.25f), new Color(1f, 0.4f, 0.12f), g.TelegraphNormalized);
            return true;
        }

        private static GravityManager FirstTelegraph(PlayerMotor p1, PlayerMotor p2)
        {
            if (p1 != null && p1.Gravity != null && p1.Gravity.IsTelegraphing)
            {
                return p1.Gravity;
            }

            if (p2 != null && p2.Gravity != null && p2.Gravity.IsTelegraphing)
            {
                return p2.Gravity;
            }

            return null;
        }

        private void TryBindMatch()
        {
            var match = MatchDirector.Instance;
            if (match == _boundMatch)
            {
                return;
            }

            UnbindMatch();
            _boundMatch = match;
            if (_boundMatch != null)
            {
                _boundMatch.ObjectiveCompleted += OnObjectiveCompleted;
            }
        }

        private void UnbindMatch()
        {
            if (_boundMatch != null)
            {
                _boundMatch.ObjectiveCompleted -= OnObjectiveCompleted;
            }

            _boundMatch = null;
        }

        private void OnObjectiveCompleted(int _, string label)
        {
            _toast = label is { Length: > 0 }
                ? $"¡{label.ToUpperInvariant()} COMPLETADO!"
                : "¡OBJETIVO COMPLETADO!";
            _toastUntil = Time.unscaledTime + 2.4f;
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
            _splitBar = MakeSplitBar(canvasGo.transform);
        }

        private static GameObject MakeSplitBar(Transform parent)
        {
            var go = new GameObject("SplitBar");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.04f, 0.05f, 0.08f, 1f);
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(0f, 6f);
            rt.anchoredPosition = Vector2.zero;
            return go;
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
