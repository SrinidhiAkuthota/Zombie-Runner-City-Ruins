using UnityEngine;

namespace ZombieRunner
{
    /// <summary>A blocky, shambling zombie built from primitives. Faces +Z by default.</summary>
    public class ZombieRig
    {
        public GameObject root;
        readonly Transform model, armL, armR, legL, legR;
        float phase;

        static readonly Color[] shirts =
        {
            new Color(0.35f, 0.25f, 0.20f), new Color(0.20f, 0.30f, 0.40f), new Color(0.40f, 0.40f, 0.35f),
            new Color(0.45f, 0.15f, 0.15f), new Color(0.25f, 0.35f, 0.25f)
        };

        public ZombieRig(Transform parent)
        {
            root = new GameObject("Zombie");
            root.transform.SetParent(parent, false);
            model = new GameObject("Model").transform;
            model.SetParent(root.transform, false);
            model.localScale = Vector3.one * Random.Range(0.92f, 1.1f);

            Color skin = new Color(Random.Range(0.40f, 0.55f), Random.Range(0.55f, 0.70f), Random.Range(0.35f, 0.45f));
            Color shirt = shirts[Random.Range(0, shirts.Length)];
            Color pants = new Color(0.15f, 0.15f, 0.18f);
            Color eye = new Color(1f, 0.15f, 0.05f);

            Transform torso = new GameObject("Torso").transform;
            torso.SetParent(model, false);
            torso.localPosition = new Vector3(0f, 0.75f, 0f);
            torso.localEulerAngles = new Vector3(14f, 0f, 0f);

            Prims.Make(Shape.Cube, torso, new Vector3(0f, 0.35f, 0f), new Vector3(0.6f, 0.75f, 0.35f), shirt);
            Prims.Make(Shape.Sphere, torso, new Vector3(0f, 0.92f, 0.08f), Vector3.one * 0.38f, skin);
            Prims.Make(Shape.Cube, torso, new Vector3(-0.08f, 0.96f, 0.26f), Vector3.one * 0.07f, eye, Vector3.zero, true);
            Prims.Make(Shape.Cube, torso, new Vector3(0.08f, 0.96f, 0.26f), Vector3.one * 0.07f, eye, Vector3.zero, true);
            armL = Prims.Limb(torso, -0.4f, 0.65f, 0.6f, 0.15f, skin);
            armR = Prims.Limb(torso, 0.4f, 0.65f, 0.6f, 0.15f, skin);
            legL = Prims.Limb(model, -0.15f, 0.75f, 0.75f, 0.2f, pants);
            legR = Prims.Limb(model, 0.15f, 0.75f, 0.75f, 0.2f, pants);

            phase = Random.Range(0f, 6.28f);
            Animate(0f, 0f);
        }

        public void Animate(float dt, float rate)
        {
            phase += dt * rate;
            float s = Mathf.Sin(phase);
            legL.localEulerAngles = new Vector3(s * 38f, 0f, 0f);
            legR.localEulerAngles = new Vector3(-s * 38f, 0f, 0f);
            float a = Mathf.Sin(phase * 0.5f) * 10f;
            armL.localEulerAngles = new Vector3(-85f + a, 0f, 0f);
            armR.localEulerAngles = new Vector3(-85f - a, 0f, 0f);
            model.localPosition = new Vector3(Mathf.Sin(phase * 0.5f) * 0.05f, Mathf.Abs(s) * 0.07f, 0f);
        }
    }
}
