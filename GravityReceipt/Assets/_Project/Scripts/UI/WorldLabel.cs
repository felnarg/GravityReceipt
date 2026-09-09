using UnityEngine;

namespace GravityReceipt.UI
{
    /// <summary>
    /// Texto 3D de alto contraste para tutorial y objetivos.
    /// </summary>
    public static class WorldLabel
    {
        public static TextMesh Create(Transform parent, string name, string text, Vector3 localPos, Color color, float characterSize = 0.12f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.characterSize = characterSize;
            tm.fontSize = 48;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.fontStyle = FontStyle.Bold;
            return tm;
        }
    }
}
