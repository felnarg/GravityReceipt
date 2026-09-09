using UnityEngine;

namespace GravityReceipt.Gravity
{
    /// <summary>
    /// Chispas unlit al aplicar un flip (sin sistema de partículas).
    /// </summary>
    public static class GravityFlipBurst
    {
        public static void Spawn(Vector3 origin, Vector3 newDown)
        {
            if (newDown.sqrMagnitude < 0.01f)
            {
                newDown = Vector3.down;
            }

            newDown.Normalize();
            var tangent = Vector3.Cross(newDown, Vector3.up);
            if (tangent.sqrMagnitude < 0.01f)
            {
                tangent = Vector3.Cross(newDown, Vector3.right);
            }

            tangent.Normalize();
            var bitangent = Vector3.Cross(newDown, tangent);
            for (var i = 0; i < 12; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "FlipSpark";
                go.transform.position = origin + Random.insideUnitSphere * 0.35f;
                go.transform.localScale = Vector3.one * Random.Range(0.06f, 0.14f);
                var col = go.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                var rend = go.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
                    if (shader != null)
                    {
                        var t = i / 11f;
                        rend.sharedMaterial = new Material(shader)
                        {
                            color = Color.Lerp(new Color(1f, 0.85f, 0.25f), new Color(1f, 0.35f, 0.08f), t)
                        };
                    }
                }

                var rb = go.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                var spray = tangent * Random.Range(-4.5f, 4.5f) + bitangent * Random.Range(-4.5f, 4.5f);
                rb.linearVelocity = newDown * Random.Range(2.5f, 7f) + spray;
                rb.angularVelocity = Random.insideUnitSphere * 8f;
                Object.Destroy(go, 0.55f);
            }
        }
    }
}
