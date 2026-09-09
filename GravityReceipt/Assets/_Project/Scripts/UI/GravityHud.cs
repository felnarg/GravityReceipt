using GravityReceipt.Gravity;
using UnityEngine;
using UnityEngine.UI;

namespace GravityReceipt.UI
{
    /// <summary>
    /// HUD mínimo: dirección de g + telegráfo + valuable dominante.
    /// </summary>
    public sealed class GravityHud : MonoBehaviour
    {
        [SerializeField] private GravityManager gravityManager;
        [SerializeField] private Text statusText;

        private void Awake()
        {
            if (gravityManager is null)
            {
                gravityManager = FindAnyObjectByType<GravityManager>();
            }

            if (statusText is null)
            {
                EnsureCanvas();
            }
        }

        private void Update()
        {
            if (gravityManager is null || statusText is null)
            {
                return;
            }

            var dominant = gravityManager.Dominant is { } d ? $"{d.name} (${d.Price})" : "ninguno";
            var telegraph = gravityManager.IsTelegraphing
                ? $"FLIP en {1f - gravityManager.TelegraphNormalized:0.0}s"
                : "estable";
            statusText.text = $"Gravedad: {gravityManager.CurrentDirection} | Dominante: {dominant} | {telegraph}";
        }

        private void EnsureCanvas()
        {
            var canvasGo = new GameObject("GravityHUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var textGo = new GameObject("Status");
            textGo.transform.SetParent(canvasGo.transform, false);
            statusText = textGo.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 18;
            statusText.color = Color.white;
            statusText.alignment = TextAnchor.UpperLeft;

            var rt = statusText.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(12f, -12f);
            rt.sizeDelta = new Vector2(900f, 60f);
        }
    }
}
