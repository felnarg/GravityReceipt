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

            if (!TryGetMesh(model, out var mesh, out var colors) || mesh == null)
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

            var filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var rend = root.AddComponent<MeshRenderer>();
            var mats = new Material[mesh.subMeshCount];
            for (var i = 0; i < mats.Length; i++)
            {
                var tint = i < colors.Length ? colors[i] : Color.white;
                mats[i] = SurfaceLook.Solid(tint, 0.28f, 0.05f);
            }

            rend.sharedMaterials = mats;

            if (solid)
            {
                var box = root.AddComponent<BoxCollider>();
                box.center = mesh.bounds.center;
                box.size = mesh.bounds.size;
            }

            return root;
        }

        private static bool TryGetMesh(string model, out Mesh mesh, out Color[] colors)
        {
            if (MeshCache.TryGetValue(model, out mesh))
            {
                colors = ColorCache[model];
                return true;
            }

            mesh = null;
            colors = Array.Empty<Color>();
            var dir = Path.Combine(Application.streamingAssetsPath, "Kenney");
            var objPath = Path.Combine(dir, model + ".obj");
            if (!File.Exists(objPath))
            {
                return false;
            }

            var mtlColors = ParseMtl(Path.Combine(dir, model + ".mtl"));
            if (!TryParseObj(File.ReadAllLines(objPath), mtlColors, out mesh, out colors))
            {
                return false;
            }

            mesh.name = "Kenney_" + model;
            MeshCache[model] = mesh;
            ColorCache[model] = colors;
            return true;
        }

        private static Dictionary<string, Color> ParseMtl(string path)
        {
            var map = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
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
                    continue;
                }

                if (current.Length == 0 || !line.StartsWith("Kd ", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var p = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length >= 4
                    && float.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var r)
                    && float.TryParse(p[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var g)
                    && float.TryParse(p[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
                {
                    map[current] = new Color(r, g, b, 1f);
                }
            }

            return map;
        }

        private static bool TryParseObj(
            string[] lines,
            Dictionary<string, Color> mtl,
            out Mesh mesh,
            out Color[] subColors)
        {
            mesh = null;
            subColors = Array.Empty<Color>();
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var groups = new List<(string mat, List<int> tris)>();
            var current = -1;
            var matName = "default";

            void UseMat(string name)
            {
                matName = name;
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
                    face[i - 1] = AddCorner(bits[i], positions, normals, verts, norms);
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
            var sub = 0;
            for (var i = 0; i < groups.Count; i++)
            {
                if (groups[i].tris.Count == 0)
                {
                    continue;
                }

                mesh.SetTriangles(groups[i].tris, sub, true);
                subColors[sub] = mtl.TryGetValue(groups[i].mat, out var c) ? c : Color.white;
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
            List<Vector3> verts,
            List<Vector3> norms)
        {
            var slash = token.Split('/');
            var vi = ParseIndex(slash[0], positions.Count);
            verts.Add(vi >= 0 ? positions[vi] : Vector3.zero);
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
