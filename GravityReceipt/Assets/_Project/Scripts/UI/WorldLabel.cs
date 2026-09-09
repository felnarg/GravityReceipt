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

            var shadowGo = new GameObject(name + "_Shadow");
            shadowGo.transform.SetParent(go.transform, false);
            shadowGo.transform.localPosition = new Vector3(0.025f, -0.025f, 0.02f);
            var shadow = shadowGo.AddComponent<TextMesh>();
            shadow.text = text;
            shadow.characterSize = characterSize;
            shadow.fontSize = 48;
            shadow.anchor = TextAnchor.MiddleCenter;
            shadow.alignment = TextAlignment.Center;
            shadow.color = new Color(0f, 0f, 0f, 0.85f);
            shadow.fontStyle = FontStyle.Bold;
            return tm;
        }
    }
}
