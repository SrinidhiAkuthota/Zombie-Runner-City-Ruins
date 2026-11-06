using System.Collections.Generic;
using UnityEngine;

namespace ZombieRunner
{
    public enum Shape { Cube = 0, Sphere = 1, Capsule = 2, Cylinder = 3 }

    /// <summary>
    /// Builds everything from Unity's built-in primitive meshes (no colliders, cached materials).
    /// Materials are cloned from the primitive's default material so it works in Built-in, URP and HDRP.
    /// </summary>
    public static class Prims
    {
        static Mesh[] meshes;
        static Material baseMat;
        static readonly Dictionary<Color, Material> solid = new Dictionary<Color, Material>();
        static readonly Dictionary<Color, Material> glow = new Dictionary<Color, Material>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            meshes = null; baseMat = null; solid.Clear(); glow.Clear();
        }

        static void Init()
        {
            if (meshes != null && baseMat != null) return;
            solid.Clear(); glow.Clear();
            meshes = new Mesh[4];
            PrimitiveType[] types = { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Capsule, PrimitiveType.Cylinder };
            for (int i = 0; i < 4; i++)
            {
                GameObject g = GameObject.CreatePrimitive(types[i]);
                meshes[i] = g.GetComponent<MeshFilter>().sharedMesh;
                if (baseMat == null) baseMat = g.GetComponent<MeshRenderer>().sharedMaterial;
                Object.Destroy(g);
            }
        }

        public static Color Shade(Color c, float f)
        {
            return new Color(c.r * f, c.g * f, c.b * f, 1f);
        }

        public static Material Mat(Color c, bool emissive)
        {
            Init();
            Dictionary<Color, Material> dict = emissive ? glow : solid;
            Material m;
            if (dict.TryGetValue(c, out m) && m != null) return m;
            m = new Material(baseMat);
            m.color = c;
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.05f);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.05f);
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", new Color(c.r * 1.6f, c.g * 1.6f, c.b * 1.6f, 1f));
            }
            dict[c] = m;
            return m;
        }

        public static GameObject Make(Shape s, Transform parent, Vector3 pos, Vector3 scale, Color c,
                                      Vector3 euler = default(Vector3), bool emissive = false)
        {
            Init();
            GameObject go = new GameObject(s.ToString());
            Transform t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = pos;
            t.localEulerAngles = euler;
            t.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = meshes[(int)s];
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = Mat(c, emissive);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return go;
        }

        public static Transform Pivot(Transform parent, Vector3 pos)
        {
            Transform p = new GameObject("Pivot").transform;
            p.SetParent(parent, false);
            p.localPosition = pos;
            return p;
        }

        /// <summary>A box limb hanging down from a pivot at (x, topY).</summary>
        public static Transform Limb(Transform parent, float x, float topY, float len, float thick, Color c)
        {
            Transform p = Pivot(parent, new Vector3(x, topY, 0f));
            Make(Shape.Cube, p, new Vector3(0f, -len * 0.5f, 0f), new Vector3(thick, len, thick), c);
            return p;
        }
    }
}
