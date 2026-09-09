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
    [DefaultExecutionOrder(60)]
    public sealed class GravityHud : MonoBehaviour
    {
        [SerializeField] private Text statusText;
        [SerializeField] private Text matchText;
        [SerializeField] private Text helpText;
        [SerializeField] private Text helpP1Text;
        [SerializeField] private Text centerText;
        private Text _crossP1;
        private Text _crossP2;
        private Text _promptP1;
        private Text _promptP2;
        private GameObject _splitBar;
        private Image _centerPanel;
        private Image _statusPanel;
        private Image _matchPanel;
        private Image _vignette;
        private Image _flash;
        private float _flashUntil;
        private bool _wasTelegraphing;
        private Text _gChipP1;
        private Text _gChipP2;
        private bool _hallwayWarned;
        private MatchDirector _boundMatch;
        private string _toast = string.Empty;
        private float _toastUntil;
        private bool _chromeHidden;
        private PlayerMotor _cachedP1;
        private PlayerMotor _cachedP2;
        private MissionPackage _cachedPkg;
        private float _cacheUntil;

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

            if (_promptP2 != null)
            {
                _promptP2.gameObject.SetActive(false);
            }

            if (_crossP1 != null)
            {
                var rt = _crossP1.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
            }

            if (_promptP1 != null)
            {
                var rt = _promptP1.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
            }

            if (_gChipP1 != null)
            {
                var rt = _gChipP1.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.92f);
                rt.anchorMax = new Vector2(0.5f, 0.92f);
            }

            if (_gChipP2 != null)
            {
                _gChipP2.gameObject.SetActive(false);
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

            if (Input.GetKeyDown(KeyCode.F9))
            {
                _chromeHidden = !_chromeHidden;
                FollowBillboard.Hidden = _chromeHidden;
            }

            RefreshActorCache();
            var p1 = _cachedP1;
            var p2 = _cachedP2;
            var match = MatchDirector.Instance;
            if (statusText != null)
            {
                statusText.gameObject.SetActive(!_chromeHidden);
            }

            if (_statusPanel != null)
            {
                _statusPanel.enabled = !_chromeHidden;
            }

            if (helpP1Text != null)
            {
                helpP1Text.gameObject.SetActive(!_chromeHidden);
            }

            if (helpText != null)
            {
                helpText.gameObject.SetActive(!_chromeHidden && p2 != null);
            }

            if (matchText != null)
            {
                matchText.gameObject.SetActive(!_chromeHidden && (match == null || !match.IsInSplash));
            }

            if (_matchPanel != null)
            {
                _matchPanel.enabled = !_chromeHidden && (match == null || !match.IsInSplash);
            }

            if (_promptP1 != null)
            {
                _promptP1.gameObject.SetActive(!_chromeHidden);
            }

            if (_promptP2 != null)
            {
                _promptP2.gameObject.SetActive(!_chromeHidden && p2 != null);
            }

            if (_gChipP1 != null)
            {
                _gChipP1.gameObject.SetActive(!_chromeHidden);
            }

            if (_gChipP2 != null)
            {
                _gChipP2.gameObject.SetActive(!_chromeHidden && p2 != null);
            }
            var g1 = p1 != null ? p1.Gravity : null;
            var g2 = p2 != null ? p2.Gravity : null;
            var gravity = g1 != null ? g1 : FindAnyObjectByType<GravityManager>();
            var room1 = p1 != null ? RoomRegistry.FindRoom(p1.transform.position) : null;
            var room2 = p2 != null ? RoomRegistry.FindRoom(p2.transform.position) : null;
            var roomName = FormatRoom(room1);
            var room2Name = room2 != null ? FormatRoom(room2) : null;
            var flipG = FirstTelegraph(p1, p2);

            statusText.text = p2 != null
                ? $"P1 {roomName} g→{DirName(g1)} {DomShort(g1)}  |  P2 {room2Name} g→{DirName(g2)} {DomShort(g2)}  |  {TelegraphLabel(flipG != null ? flipG : g1)}"
                : $"Sala: {roomName}  |  g → {DirName(gravity)}  |  Dominante: {DomLong(gravity)}  |  {TelegraphLabel(gravity)}";
            statusText.color = flipG != null || (gravity != null && gravity.ShowFlipBanner)
                ? new Color(1f, 0.9f, 0.2f)
                : Color.white;

            var pkg = _cachedPkg;
            var rec = MatchHighlightRecorder.Instance;
            if (matchText != null && match != null)
            {
                var t = Mathf.CeilToInt(match.RemainingSeconds);
                var mm = t / 60;
                var ss = t % 60;
                var hearts = Hearts(pkg);
                matchText.color = t <= 60
                    ? new Color(1f, 0.45f, 0.4f)
                    : Color.white;
                matchText.text =
                    $"⏱ {mm:00}:{ss:00}   Paquete {hearts}{PackageHolderSuffix()}{PackageRangeSuffix(p1)}  dest {(pkg != null ? pkg.Destructions : 0)}/{(pkg != null ? pkg.MaxDestructions : 3)}\n" +
                    $"{ObjMark(match, 0)} Enchufar   {ObjMark(match, 1)} Entregar   {ObjMark(match, 2)} Sellar   ({match.ObjectivesDone}/{match.ObjectivesToWin})" +
                    NextObjectiveHint(match, p1) +
                    ObjectiveProgressSuffix();
            }

            if (helpP1Text != null)
            {
                var r1 = RoleOf(LocalPlayerSlot.One);
                var wind = WindUp(p1);
                helpP1Text.text = $"P1 [{r1}] WASD+ratón  E agarrar  Q ping  1-4 emote  Shift sprint  F ancla  Tab rol  P pausa  F5 restart  F9 HUD{wind}";
            }

            if (helpText != null)
            {
                var r2 = RoleOf(LocalPlayerSlot.Two);
                helpText.text = p2 != null
                    ? $"P2 [{r2}] flechas  J/L+I/K mirar  RShift agarrar  / ping  KP1-3/9 emote  Alt sprint  KP0 ancla  KP7 rol  P pausa"
                    : $"P1 [{RoleOf(LocalPlayerSlot.One)}] WASD+ratón  E agarrar  Q ping  1-4 emote  Shift sprint  F ancla";
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
                else if (match.IsPaused)
                {
                    centerText.text = match.IsInSplash
                        ? $"PAUSA — LEE LA REGLA ({Mathf.CeilToInt(match.SplashSecondsLeft)} s)\nLA GRAVEDAD SIGUE AL OBJETO MÁS CARO\nGrises sin $ no cuentan · taza $15 o caja $80 a una PARED\nP continúa"
                        : "PAUSA\nP continúa · clic para mirar · F5 restart";
                    centerText.color = new Color(0.85f, 0.95f, 1f);
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
                else if (match.IsInSplash)
                {
                    centerText.text = "LA GRAVEDAD SIGUE AL OBJETO MÁS CARO\nGrises sin $ no cuentan · taza $15 o caja $80 a una PARED\nSigue la flecha · P pausa para leer\n" + Mathf.CeilToInt(match.SplashSecondsLeft) + " s";
                    centerText.color = new Color(1f, 0.92f, 0.4f);
                }
                else if (CursorUnlockedHint(p1))
                {
                    centerText.text = "Clic para mirar";
                    centerText.color = new Color(0.85f, 0.9f, 1f);
                }
                else
                {
                    centerText.text = string.Empty;
                }
            }

            if (_centerPanel != null)
            {
                _centerPanel.enabled = !_chromeHidden
                    && centerText != null
                    && centerText.text is { Length: > 0 };
            }
            TintCross(_crossP1, p1);
            TintCross(_crossP2, p2);
            UpdateLookPrompt(_promptP1, p1, "E");
            UpdateLookPrompt(_promptP2, p2, "RShift");
            UpdateGravityChip(_gChipP1, p1);
            UpdateGravityChip(_gChipP2, p2);
            MaybeWarnHallway(p1, p2, match);

            if (Input.GetKeyDown(KeyCode.F8))
            {
                var name = $"GravityReceipt_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
                ScreenCapture.CaptureScreenshot(name);
                Debug.Log("[GravityReceipt] Screenshot: " + name);
            }

            if (_vignette != null)
            {
                var g = FirstTelegraph(p1, p2);
                var telegraphing = g != null && g.IsTelegraphing;
                if (_wasTelegraphing && !telegraphing && g != null && g.ShowFlipBanner)
                {
                    _flashUntil = Time.unscaledTime + 0.14f;
                }

                _wasTelegraphing = telegraphing;
                var a = 0f;
                if (g != null && g.IsTelegraphing)
                {
                    a = 0.28f * g.TelegraphNormalized;
                }
                else if (g != null && g.ShowFlipBanner)
                {
                    a = 0.16f;
                }

                _vignette.color = new Color(0.15f, 0.04f, 0f, a);
            }

            if (_flash != null)
            {
                var flashA = Time.unscaledTime < _flashUntil ? 0.32f : 0f;
                _flash.color = new Color(1f, 0.72f, 0.28f, flashA);
            }
        }

        private void RefreshActorCache()
        {
            if (Time.unscaledTime < _cacheUntil && _cachedP1 != null)
            {
                return;
            }

            _cacheUntil = Time.unscaledTime + 0.2f;
            _cachedP1 = FindPlayer(LocalPlayerSlot.One);
            _cachedP2 = FindPlayer(LocalPlayerSlot.Two);
            _cachedPkg = FindAnyObjectByType<MissionPackage>();
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
            var focus = inter == null
                ? null
                : inter.LookValuable != null
                    ? inter.LookValuable
                    : inter.HeldValuable;
            var dominant = focus != null
                           && focus.Manager != null
                           && focus.Manager.Dominant == focus;
            cross.color = winding
                ? new Color(1f, 0.55f, 0.15f)
                : dominant
                    ? new Color(1f, 0.72f, 0.12f)
                    : looking
                        ? new Color(1f, 0.92f, 0.25f)
                        : new Color(1f, 1f, 1f, 0.85f);
            cross.fontSize = winding ? 28 : dominant ? 26 : 22;
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

        private static string FormatRoom(RoomVolume room)
        {
            if (room == null || room.RoomId.Length == 0)
            {
                return "—";
            }

            return room.HasOwnGravity ? room.RoomId : room.RoomId + " · g hereda";
        }

        private static string DirName(GravityManager g)
        {
            return g == null ? "?" : DirName(g.CurrentDirection);
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

        private static string PackageHolderSuffix()
        {
            var inters = FindObjectsByType<PlayerInteractor>(FindObjectsSortMode.None);
            foreach (var inter in inters)
            {
                if (inter == null || !inter.IsHoldingPackage)
                {
                    continue;
                }

                var input = inter.GetComponent<LocalPlayerInput>();
                var who = input != null && input.Slot == LocalPlayerSlot.Two ? "P2" : "P1";
                return $" · {who}";
            }

            return string.Empty;
        }

        private static string DomShort(GravityManager g)
        {
            return g != null && g.Dominant != null ? $"${g.Dominant.Price}" : "—";
        }

        private static string DomLong(GravityManager g)
        {
            return g != null && g.Dominant != null
                ? $"{g.Dominant.name} (${g.Dominant.Price})"
                : "ninguno";
        }

        private static string TelegraphLabel(GravityManager g)
        {
            if (g == null)
            {
                return "";
            }

            if (g.IsAnchored)
            {
                return "ANCLA";
            }

            if (g.IsTelegraphing)
            {
                return $"FLIP en {1f - g.TelegraphNormalized:0.0}s";
            }

            return g.ShowFlipBanner ? "FLIP" : "estable";
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

        private static string ObjMark(MatchDirector match, int index)
        {
            if (match.IsObjectiveComplete(index))
            {
                return "[x]";
            }

            if (index > 0 && !match.IsObjectiveComplete(index - 1))
            {
                return "[-]";
            }

            return index == match.CurrentObjectiveIndex ? "[>]" : "[ ]";
        }

        private static string NextObjectiveHint(MatchDirector match, PlayerMotor from)
        {
            if (match == null || !match.IsPlaying)
            {
                return string.Empty;
            }

            return match.CurrentObjectiveIndex switch
            {
                0 => "\nSiguiente: paquete a la zona VERDE de Archive" + DistToObjective(0, from),
                1 => "\nSiguiente: suelta el paquete en la losa AZUL" + DistToObjective(1, from),
                2 => "\nSiguiente: paquete + mantén E en la losa DORADA" + DistToObjective(2, from),
                _ => string.Empty
            };
        }

        private static string DistToObjective(int index, PlayerMotor from)
        {
            if (from == null)
            {
                return string.Empty;
            }

            var triggers = FindObjectsByType<ObjectiveTrigger>(FindObjectsSortMode.None);
            foreach (var t in triggers)
            {
                if (t == null || t.Index != index || t.IsDone)
                {
                    continue;
                }

                var m = Vector3.Distance(from.transform.position, t.transform.position);
                return m >= 3.5f ? $" ({m:0} m)" : string.Empty;
            }

            return string.Empty;
        }

        private static string PackageRangeSuffix(PlayerMotor from)
        {
            if (from == null)
            {
                return string.Empty;
            }

            var pkg = FindAnyObjectByType<MissionPackage>();
            if (pkg == null)
            {
                return string.Empty;
            }

            var m = Vector3.Distance(from.transform.position, pkg.transform.position);
            return m >= 4f ? $" · {m:0}m" : string.Empty;
        }

        private static bool CursorUnlockedHint(PlayerMotor p1)
        {
            if (p1 == null)
            {
                return false;
            }

            var input = p1.GetComponent<LocalPlayerInput>();
            return input != null && input.UsesMouseLook && Cursor.lockState != CursorLockMode.Locked;
        }

        private void MaybeWarnHallway(PlayerMotor p1, PlayerMotor p2, MatchDirector match)
        {
            if (match == null || !match.IsPlaying || match.IsInSplash)
            {
                return;
            }

            if (Time.unscaledTime < _toastUntil)
            {
                return;
            }

            if (IsSidewaysHallway(p1) || IsSidewaysHallway(p2))
            {
                if (_hallwayWarned)
                {
                    return;
                }

                _hallwayWarned = true;
                _toast = "Pasillo: g hereda · vacío a los lados";
                _toastUntil = Time.unscaledTime + 2.6f;
                return;
            }

            _hallwayWarned = false;
        }

        private static bool IsSidewaysHallway(PlayerMotor motor)
        {
            if (motor == null)
            {
                return false;
            }

            var room = RoomRegistry.FindRoom(motor.transform.position);
            if (room == null || room.HasOwnGravity)
            {
                return false;
            }

            var g = motor.Gravity;
            return g != null && Vector3.Dot(g.CurrentDirection, Vector3.down) < 0.92f;
        }

        private static void UpdateGravityChip(Text chip, PlayerMotor motor)
        {
            if (chip == null)
            {
                return;
            }

            if (motor == null)
            {
                chip.text = "";
                return;
            }

            var g = motor.Gravity;
            var room = RoomRegistry.FindRoom(motor.transform.position);
            var inherit = room != null && !room.HasOwnGravity;
            if (g == null)
            {
                chip.text = inherit ? "g hereda" : "";
                return;
            }

            var dir = DirName(g);
            if (g.IsTelegraphing)
            {
                chip.text = $"FLIP → {DirName(g.PendingDirection)}";
                chip.color = new Color(1f, 0.82f, 0.2f);
                return;
            }

            if (g.IsAnchored)
            {
                chip.text = "ANCLA";
                chip.color = new Color(0.45f, 0.9f, 1f);
                return;
            }

            chip.text = inherit ? $"g hereda → {dir}" : $"g → {dir}";
            chip.color = Vector3.Dot(g.CurrentDirection, Vector3.down) > 0.92f
                ? new Color(0.75f, 0.9f, 1f)
                : new Color(1f, 0.78f, 0.35f);
        }

        private static void UpdateLookPrompt(Text prompt, PlayerMotor motor, string grabKey)
        {
            if (prompt == null)
            {
                return;
            }

            if (motor == null)
            {
                prompt.text = "";
                return;
            }

            var inter = motor.GetComponent<PlayerInteractor>();
            if (inter == null)
            {
                prompt.text = "";
                return;
            }

            if (inter.IsHolding)
            {
                prompt.text = inter.LookHint is { Length: > 0 }
                    ? $"{grabKey}  {inter.LookHint}"
                    : $"{grabKey}  soltar";
                prompt.color = inter.LookHint.Contains("PARED") || inter.LookHint.Contains("FLIP")
                    ? new Color(1f, 0.85f, 0.3f)
                    : new Color(1f, 0.75f, 0.35f);
                return;
            }

            if (inter.IsWinding || inter.WindUpNormalized > 0.05f)
            {
                prompt.text = $"agarrando… {inter.WindUpNormalized:0.0}";
                prompt.color = new Color(1f, 0.55f, 0.2f);
                return;
            }

            if (inter.LookHint is { Length: > 0 })
            {
                prompt.text = inter.LookHint.Contains("ocupado")
                    ? inter.LookHint
                    : $"{grabKey}  {inter.LookHint}";
                prompt.color = inter.LookHint.Contains("ocupado")
                    ? new Color(1f, 0.45f, 0.4f)
                    : new Color(1f, 0.95f, 0.55f);
                return;
            }

            prompt.text = "";
        }

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

            var dir = DirName(g.BannerDirection);
            var item = g.Dominant != null ? $"${g.Dominant.Price}" : "el objeto caro";
            var room = g.name.StartsWith("Gravity_") ? g.name[8..] : g.name;
            banner = $"¡FLIP {room.ToUpperInvariant()}!\nLa gravedad va hacia {dir}\nSigue a {item}";
            color = Color.Lerp(new Color(1f, 0.92f, 0.25f), new Color(1f, 0.4f, 0.12f), g.TelegraphNormalized);
            return true;
        }

        private static GravityManager FirstTelegraph(PlayerMotor p1, PlayerMotor p2)
        {
            if (p1 != null && p1.Gravity != null && p1.Gravity.ShowFlipBanner)
            {
                return p1.Gravity;
            }

            if (p2 != null && p2.Gravity != null && p2.Gravity.ShowFlipBanner)
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
                _boundMatch.PackageDented += OnPackageDented;
                _boundMatch.PlayerRespawned += OnPlayerRespawned;
                _boundMatch.Hint += OnHint;
            }
        }

        private void UnbindMatch()
        {
            if (_boundMatch != null)
            {
                _boundMatch.ObjectiveCompleted -= OnObjectiveCompleted;
                _boundMatch.PackageDented -= OnPackageDented;
                _boundMatch.PlayerRespawned -= OnPlayerRespawned;
                _boundMatch.Hint -= OnHint;
            }

            _boundMatch = null;
        }

        private void OnPackageDented()
        {
            _toast = "¡PAQUETE ABOLLADO!";
            _toastUntil = Time.unscaledTime + 1.6f;
        }

        private void OnHint(string message)
        {
            if (message is not { Length: > 0 })
            {
                return;
            }

            _toast = message;
            _toastUntil = Time.unscaledTime + 3.2f;
        }

        private void OnPlayerRespawned()
        {
            _toast = "Caíste · checkpoint";
            _toastUntil = Time.unscaledTime + 1.8f;
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

            _statusPanel = MakePanel(canvasGo.transform, "StatusPanel", new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(1680f, 40f));
            _matchPanel = MakePanel(canvasGo.transform, "MatchPanel", new Vector2(0.5f, 0.5f), new Vector2(0f, -28f), new Vector2(1680f, 86f));
            _centerPanel = MakePanel(canvasGo.transform, "CenterPanel", new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(920f, 250f));
            _vignette = MakeVignette(canvasGo.transform);
            _flash = MakeFlash(canvasGo.transform);
            _vignette.transform.SetAsFirstSibling();
            _flash.transform.SetSiblingIndex(1);

            statusText = MakeText(canvasGo.transform, "Status", new Vector2(0f, 18f), new Vector2(0.5f, 0.5f), new Vector2(1600f, 36f), 20, TextAnchor.MiddleCenter);
            matchText = MakeText(canvasGo.transform, "Match", new Vector2(0f, -28f), new Vector2(0.5f, 0.5f), new Vector2(1600f, 78f), 18, TextAnchor.MiddleCenter);
            helpP1Text = MakeText(canvasGo.transform, "HelpP1", new Vector2(16f, -10f), new Vector2(0f, 1f), new Vector2(1600f, 28f), 16, TextAnchor.UpperLeft);
            helpP1Text.color = new Color(0.7f, 0.85f, 1f);
            helpText = MakeText(canvasGo.transform, "Help", new Vector2(16f, 12f), new Vector2(0f, 0f), new Vector2(1600f, 32f), 16, TextAnchor.LowerLeft);
            helpText.color = new Color(0.85f, 0.9f, 1f);
            centerText = MakeText(canvasGo.transform, "Center", new Vector2(0f, 80f), new Vector2(0.5f, 0.5f), new Vector2(900f, 240f), 34, TextAnchor.MiddleCenter);
            centerText.color = Color.white;

            _crossP1 = MakeCross(canvasGo.transform, "CrossP1", new Vector2(0.5f, 0.75f));
            _crossP2 = MakeCross(canvasGo.transform, "CrossP2", new Vector2(0.5f, 0.25f));
            _promptP1 = MakeText(canvasGo.transform, "PromptP1", new Vector2(0f, -36f), new Vector2(0.5f, 0.75f), new Vector2(520f, 32f), 18, TextAnchor.UpperCenter);
            _promptP1.color = new Color(1f, 0.95f, 0.55f);
            _promptP2 = MakeText(canvasGo.transform, "PromptP2", new Vector2(0f, -36f), new Vector2(0.5f, 0.25f), new Vector2(520f, 32f), 18, TextAnchor.UpperCenter);
            _promptP2.color = new Color(1f, 0.95f, 0.55f);
            _gChipP1 = MakeText(canvasGo.transform, "GChipP1", new Vector2(0f, 42f), new Vector2(0.5f, 0.75f), new Vector2(420f, 28f), 18, TextAnchor.LowerCenter);
            _gChipP1.color = new Color(0.7f, 0.92f, 1f);
            _gChipP2 = MakeText(canvasGo.transform, "GChipP2", new Vector2(0f, 42f), new Vector2(0.5f, 0.25f), new Vector2(420f, 28f), 18, TextAnchor.LowerCenter);
            _gChipP2.color = new Color(1f, 0.82f, 0.55f);
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

        private static Image MakePanel(Transform parent, string name, Vector2 anchor, Vector2 anchored, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.03f, 0.04f, 0.07f, 0.72f);
            var rt = img.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchored;
            rt.sizeDelta = size;
            return img;
        }

        private static Image MakeFlash(Transform parent)
        {
            var go = new GameObject("FlipFlash");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 0.72f, 0.28f, 0f);
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return img;
        }

        private static Image MakeVignette(Transform parent)
        {
            var go = new GameObject("Vignette");
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.04f, 0f, 0f);
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return img;
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
