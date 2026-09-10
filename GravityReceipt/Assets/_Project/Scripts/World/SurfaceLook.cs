using System.IO;
using UnityEngine;

namespace GravityReceipt.World
{
    public enum SurfaceKind
    {
        Paint,
        Wood,
        Carpet,
        Plaster
    }

    /// <summary>
    /// Materiales con textura CC0 (ambientCG) o color sólido. Funciona en runtime.
    /// </summary>
    public static class SurfaceLook
    {
        private static Texture2D _wood;
        private static Texture2D _carpet;
        private static Texture2D _plaster;

        public static Material Solid(Color color, float smooth, float metal)
        {
            return Make(color, smooth, metal, null, Vector2.one);
        }

        public static Material Atlas(Texture2D tex, Color tint)
        {
            return Make(tint, 0.22f, 0.04f, tex, Vector2.one);
        }

        public static void Paint(GameObject go, Color color, SurfaceKind kind = SurfaceKind.Paint, float tile = 1f)
        {
            if (go == null)
            {
                return;
            }

            var rend = go.GetComponent<Renderer>();
            if (rend == null)
            {
                return;
            }

            var tex = kind switch
            {
                SurfaceKind.Wood => Wood(),
                SurfaceKind.Carpet => Carpet(),
                SurfaceKind.Plaster => Plaster(),
                _ => null
            };
            var smooth = kind == SurfaceKind.Wood ? 0.22f : kind == SurfaceKind.Carpet ? 0.08f : 0.18f;
            rend.sharedMaterial = Make(color, smooth, 0.04f, tex, Vector2.one * Mathf.Max(0.25f, tile));
        }

        public static Texture2D Wood() => _wood ??= LoadJpg("wood.jpg");
        public static Texture2D Carpet() => _carpet ??= LoadJpg("carpet.jpg");
        public static Texture2D Plaster() => _plaster ??= LoadJpg("plaster.jpg");

        private static Material Make(Color color, float smooth, float metal, Texture2D tex, Vector2 tile)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard")
                         ?? Shader.Find("Unlit/Color");
            var mat = new Material(shader) { color = color };
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", smooth);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", smooth);
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", metal);
            }

            if (tex != null)
            {
                if (mat.HasProperty("_MainTex"))
                {
                    mat.SetTexture("_MainTex", tex);
                }

                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", tex);
                }

                mat.mainTexture = tex;
                mat.mainTextureScale = tile;
            }

            return mat;
        }

        private static Texture2D LoadJpg(string file)
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Textures", file);
            if (!File.Exists(path))
            {
                return null;
            }

            var data = File.ReadAllBytes(path);
            if (data is not { Length: > 16 })
            {
                return null;
            }

            var tex = new Texture2D(2, 2, TextureFormat.RGB24, true);
            if (!tex.LoadImage(data))
            {
                return null;
            }

            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.anisoLevel = 4;
            tex.name = file;
            return tex;
        }
    }
}
