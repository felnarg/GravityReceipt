using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace GravityReceipt.World
{
    /// <summary>
    /// Carga OBJ/MTL de Kenney (CC0) desde StreamingAssets, sin importar en el Editor.
    /// </summary>
    public static class KenneyProp
    {
        private static readonly Dictionary<string, Mesh> MeshCache = new();
        private static readonly Dictionary<string, Color[]> ColorCache = new();
        private static readonly Dictionary<string, Texture2D[]> TexCache = new();
        private static readonly Dictionary<string, Texture2D> ImageCache = new();

        public static GameObject Place(
            Transform parent,
            string model,
            Vector3 worldPos,
            float yaw,
            float heightMeters,
            bool solid = true)
        {
            if (model is not { Length: > 0 })
            {
                return null;
            }

            if (!TryGetMesh(model, out var mesh, out var colors, out var textures) || mesh == null)
            {
                return null;
            }

            var root = new GameObject("Kenney_" + model);
            root.transform.SetParent(parent, false);
            var size = mesh.bounds.size;
            var scale = size.y > 0.01f ? heightMeters / size.y : 0.15f;
            root.transform.position = worldPos;
            root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            root.transform.localScale = Vector3.one * scale;
            PaintMesh(root, mesh, colors, textures, null);

            if (solid)
            {
                var box = root.AddComponent<BoxCollider>();
                box.center = mesh.bounds.center;
                box.size = mesh.bounds.size;
            }

            return root;
        }

        /// <summary>
        /// Hijo visual sobre un collider existente (valuables / paquete / grises).
        /// Escala uniforme en mundo; no añade collider.
        /// </summary>
        public static GameObject Attach(
            Transform parent,
            string model,
            float heightMeters,
            float yaw = 0f,
            Color? tintMix = null)
        {
            if (parent == null || model is not { Length: > 0 })
            {
                return null;
            }

            var parentScale = parent.localScale;
            if (parentScale.x == 0f || parentScale.y == 0f || parentScale.z == 0f)
            {
                return null;
            }

            if (!TryGetMesh(model, out var mesh, out var colors, out var textures) || mesh == null)
            {
                return null;
            }

            HideOwnRenderer(parent);

            var visual = new GameObject("KenneyVisual_" + model);
            visual.transform.SetParent(parent, false);
            var size = mesh.bounds.size;
            var worldScale = size.y > 0.01f ? heightMeters / size.y : 0.15f;
            visual.transform.localScale = new Vector3(
                worldScale / parentScale.x,
                worldScale / parentScale.y,
                worldScale / parentScale.z);
            visual.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            visual.transform.localPosition = new Vector3(0f, LocalBottom(parent), 0f);
            PaintMesh(visual, mesh, colors, textures, tintMix);
            return visual;
        }

        private static void HideOwnRenderer(Transform parent)
        {
            var rend = parent.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.enabled = false;
            }
        }

        private static float LocalBottom(Transform parent)
        {
            var filter = parent.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
            {
                return filter.sharedMesh.bounds.min.y;
            }

            return -0.5f;
        }

        private static void PaintMesh(
            GameObject root,
            Mesh mesh,
            Color[] colors,
            Texture2D[] textures,
            Color? tintMix)
        {
            var filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var rend = root.AddComponent<MeshRenderer>();
            var mats = new Material[mesh.subMeshCount];
            for (var i = 0; i < mats.Length; i++)
            {
                var tint = i < colors.Length ? colors[i] : Color.white;
                if (tintMix.HasValue)
                {
                    tint = Color.Lerp(tint, tintMix.Value, 0.7f);
                }

                var tex = textures != null && i < textures.Length ? textures[i] : null;
                mats[i] = tex != null
                    ? SurfaceLook.Atlas(tex, tint)
                    : SurfaceLook.Solid(tint, 0.28f, 0.05f);
            }

            rend.sharedMaterials = mats;
        }

        private static bool TryGetMesh(string model, out Mesh mesh, out Color[] colors, out Texture2D[] textures)
        {
            if (MeshCache.TryGetValue(model, out mesh))
            {
                colors = ColorCache[model];
                textures = TexCache[model];
                return true;
            }

            mesh = null;
            colors = Array.Empty<Color>();
            textures = Array.Empty<Texture2D>();
            var dir = Path.Combine(Application.streamingAssetsPath, "Kenney");
            var objPath = Path.Combine(dir, model + ".obj");
            if (!File.Exists(objPath))
            {
                return false;
            }

            var mats = ParseMtl(Path.Combine(dir, model + ".mtl"), dir);
            if (!TryParseObj(File.ReadAllLines(objPath), mats, out mesh, out colors, out var maps))
            {
                return false;
            }

            textures = new Texture2D[maps.Length];
            for (var i = 0; i < maps.Length; i++)
            {
                textures[i] = LoadPng(maps[i]);
            }

            mesh.name = "Kenney_" + model;
            MeshCache[model] = mesh;
            ColorCache[model] = colors;
            TexCache[model] = textures;
            return true;
        }

        private static Dictionary<string, KenneyMat> ParseMtl(string path, string dir)
        {
            var map = new Dictionary<string, KenneyMat>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(path))
            {
                return map;
            }

            var current = string.Empty;
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.StartsWith("newmtl ", StringComparison.OrdinalIgnoreCase))
                {
                    current = line[7..].Trim();
                    if (current.Length > 0 && !map.ContainsKey(current))
                    {
                        map[current] = new KenneyMat(Color.white, string.Empty);
                    }

                    continue;
                }

                if (current.Length == 0)
                {
                    continue;
                }

                if (line.StartsWith("Kd ", StringComparison.OrdinalIgnoreCase))
                {
                    var p = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length >= 4
                        && float.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var r)
                        && float.TryParse(p[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var g)
                        && float.TryParse(p[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
                    {
                        var prev = map.TryGetValue(current, out var existing) ? existing : new KenneyMat(Color.white, string.Empty);
                        map[current] = new KenneyMat(new Color(r, g, b, 1f), prev.MapKd);
                    }

                    continue;
                }

                if (!line.StartsWith("map_Kd ", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var file = line[7..].Trim().Replace('\\', '/');
                var name = Path.GetFileName(file);
                var resolved = Path.Combine(dir, name);
                var prevMap = map.TryGetValue(current, out var mat) ? mat : new KenneyMat(Color.white, string.Empty);
                map[current] = new KenneyMat(prevMap.Color, resolved);
            }

            return map;
        }

        private static bool TryParseObj(
            string[] lines,
            Dictionary<string, KenneyMat> mtl,
            out Mesh mesh,
            out Color[] subColors,
            out string[] subMaps)
        {
            mesh = null;
            subColors = Array.Empty<Color>();
            subMaps = Array.Empty<string>();
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var texcoords = new List<Vector2>();
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
            var groups = new List<(string mat, List<int> tris)>();
            var current = -1;

            void UseMat(string name)
            {
                for (var i = 0; i < groups.Count; i++)
                {
                    if (groups[i].mat == name)
                    {
                        current = i;
                        return;
                    }
                }

                groups.Add((name, new List<int>()));
                current = groups.Count - 1;
            }

            UseMat("default");

            foreach (var raw in lines)
            {
                if (raw is not { Length: > 1 })
                {
                    continue;
                }

                if (raw.StartsWith("v ", StringComparison.Ordinal))
                {
                    if (TryParseVec(raw, 1, out var v))
                    {
                        positions.Add(v);
                    }

                    continue;
                }

                if (raw.StartsWith("vn ", StringComparison.Ordinal))
                {
                    if (TryParseVec(raw, 2, out var n))
                    {
                        normals.Add(n);
                    }

                    continue;
                }

                if (raw.StartsWith("vt ", StringComparison.Ordinal))
                {
                    if (TryParseUv(raw, out var uv))
                    {
                        texcoords.Add(uv);
                    }

                    continue;
                }

                if (raw.StartsWith("usemtl ", StringComparison.Ordinal))
                {
                    UseMat(raw[7..].Trim());
                    continue;
                }

                if (!raw.StartsWith("f ", StringComparison.Ordinal))
                {
                    continue;
                }

                var bits = raw.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (bits.Length < 4)
                {
                    continue;
                }

                var face = new int[bits.Length - 1];
                for (var i = 1; i < bits.Length; i++)
                {
                    face[i - 1] = AddCorner(bits[i], positions, normals, texcoords, verts, norms, uvs);
                }

                var tris = groups[current].tris;
                for (var i = 1; i < face.Length - 1; i++)
                {
                    tris.Add(face[0]);
                    tris.Add(face[i]);
                    tris.Add(face[i + 1]);
                }
            }

            if (verts.Count == 0)
            {
                return false;
            }

            RecenterFloor(verts);
            mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(verts);
            if (norms.Count == verts.Count)
            {
                mesh.SetNormals(norms);
            }

            if (uvs.Count == verts.Count)
            {
                mesh.SetUVs(0, uvs);
            }

            var used = 0;
            for (var i = 0; i < groups.Count; i++)
            {
                if (groups[i].tris.Count > 0)
                {
                    used++;
                }
            }

            mesh.subMeshCount = Mathf.Max(1, used);
            subColors = new Color[mesh.subMeshCount];
            subMaps = new string[mesh.subMeshCount];
            var sub = 0;
            for (var i = 0; i < groups.Count; i++)
            {
                if (groups[i].tris.Count == 0)
                {
                    continue;
                }

                mesh.SetTriangles(groups[i].tris, sub, true);
                if (mtl.TryGetValue(groups[i].mat, out var mat))
                {
                    subColors[sub] = mat.Color;
                    subMaps[sub] = mat.MapKd;
                }
                else
                {
                    subColors[sub] = Color.white;
                    subMaps[sub] = string.Empty;
                }

                sub++;
            }

            if (norms.Count != verts.Count)
            {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
            return true;
        }

        private static int AddCorner(
            string token,
            List<Vector3> positions,
            List<Vector3> normals,
            List<Vector2> texcoords,
            List<Vector3> verts,
            List<Vector3> norms,
            List<Vector2> uvs)
        {
            var slash = token.Split('/');
            var vi = ParseIndex(slash[0], positions.Count);
            verts.Add(vi >= 0 ? positions[vi] : Vector3.zero);
            if (slash.Length >= 2 && slash[1].Length > 0)
            {
                var ti = ParseIndex(slash[1], texcoords.Count);
                uvs.Add(ti >= 0 ? texcoords[ti] : Vector2.zero);
            }
            else
            {
                uvs.Add(Vector2.zero);
            }

            if (slash.Length >= 3 && slash[2].Length > 0)
            {
                var ni = ParseIndex(slash[2], normals.Count);
                norms.Add(ni >= 0 ? normals[ni] : Vector3.up);
            }
            else
            {
                norms.Add(Vector3.up);
            }

            return verts.Count - 1;
        }

        private static bool TryParseUv(string line, out Vector2 uv)
        {
            uv = default;
            var p = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length < 3)
            {
                return false;
            }

            if (!float.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                || !float.TryParse(p[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                return false;
            }

            uv = new Vector2(x, y);
            return true;
        }

        private static Texture2D LoadPng(string path)
        {
            if (path is not { Length: > 0 } || !File.Exists(path))
            {
                return null;
            }

            if (ImageCache.TryGetValue(path, out var cached))
            {
                return cached;
            }

            var data = File.ReadAllBytes(path);
            if (data is not { Length: > 16 })
            {
                return null;
            }

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data))
            {
                return null;
            }

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            tex.anisoLevel = 0;
            tex.name = Path.GetFileName(path);
            ImageCache[path] = tex;
            return tex;
        }

        private readonly struct KenneyMat
        {
            public KenneyMat(Color color, string mapKd)
            {
                Color = color;
                MapKd = mapKd ?? string.Empty;
            }

            public Color Color { get; }
            public string MapKd { get; }
        }

        private static int ParseIndex(string raw, int count)
        {
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
            {
                return -1;
            }

            if (i < 0)
            {
                i = count + i + 1;
            }

            i -= 1;
            return i >= 0 && i < count ? i : -1;
        }

        private static bool TryParseVec(string line, int skipChars, out Vector3 v)
        {
            v = default;
            var p = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length < 4)
            {
                return false;
            }

            if (!float.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                || !float.TryParse(p[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
                || !float.TryParse(p[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            {
                return false;
            }

            _ = skipChars;
            v = new Vector3(x, y, z);
            return true;
        }

        private static void RecenterFloor(List<Vector3> verts)
        {
            var min = verts[0];
            var max = verts[0];
            for (var i = 1; i < verts.Count; i++)
            {
                min = Vector3.Min(min, verts[i]);
                max = Vector3.Max(max, verts[i]);
            }

            var mid = (min + max) * 0.5f;
            var shift = new Vector3(mid.x, min.y, mid.z);
            for (var i = 0; i < verts.Count; i++)
            {
                verts[i] -= shift;
            }
        }
    }
}
