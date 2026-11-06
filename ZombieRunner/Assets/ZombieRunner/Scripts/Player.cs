using UnityEngine;

namespace ZombieRunner
{
    /// <summary>The survivor: 3-lane movement, jumping and sliding. Runs along +Z.</summary>
    public class Player
    {
        public const float LaneW = 3f;

        public GameObject root;
        public int lane = 1;
        public float x, y, z, vy;
        public bool grounded = true;
        public float slideT;
        public bool JumpedThisFrame, SlidThisFrame;

        public bool Sliding { get { return slideT > 0f; } }
        public float Height { get { return Sliding ? 0.8f : 1.8f; } }

        readonly Transform visual, legL, legR, armL, armR;
        float phase, lean;

        public Player()
        {
            root = new GameObject("Player");
            visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);

            Color jacket = new Color(0.85f, 0.45f, 0.10f);
            Color skin = new Color(0.90f, 0.72f, 0.58f);
            Color pants = new Color(0.15f, 0.20f, 0.35f);
            Color pack = new Color(0.20f, 0.30f, 0.20f);

            Prims.Make(Shape.Cube, visual, new Vector3(0f, 1.1f, 0f), new Vector3(0.6f, 0.7f, 0.35f), jacket);
            Prims.Make(Shape.Sphere, visual, new Vector3(0f, 1.62f, 0f), Vector3.one * 0.36f, skin);
            Prims.Make(Shape.Cube, visual, new Vector3(0f, 1.8f, 0.03f), new Vector3(0.38f, 0.12f, 0.42f), new Color(0.1f, 0.1f, 0.1f));
            Prims.Make(Shape.Cube, visual, new Vector3(0f, 1.42f, 0.02f), new Vector3(0.4f, 0.1f, 0.4f), new Color(0.75f, 0.1f, 0.1f));
            Prims.Make(Shape.Cube, visual, new Vector3(0f, 1.15f, -0.28f), new Vector3(0.45f, 0.55f, 0.22f), pack);
            legL = Prims.Limb(visual, -0.15f, 0.75f, 0.72f, 0.2f, pants);
            legR = Prims.Limb(visual, 0.15f, 0.75f, 0.72f, 0.2f, pants);
            armL = Prims.Limb(visual, -0.4f, 1.4f, 0.6f, 0.15f, jacket);
            armR = Prims.Limb(visual, 0.4f, 1.4f, 0.6f, 0.15f, jacket);

            GameObject lg = new GameObject("Lantern");
            lg.transform.SetParent(root.transform, false);
            lg.transform.localPosition = new Vector3(0f, 2.6f, 1.5f);
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 14f;
            l.intensity = 1.3f;
            l.color = new Color(1f, 0.85f, 0.6f);
            l.shadows = LightShadows.None;

            Reset();
        }

        public void Reset()
        {
            lane = 1; x = 0f; y = 0f; z = 0f; vy = 0f; grounded = true; slideT = 0f; lean = 0f;
            visual.gameObject.SetActive(true);
            Idle(0f);
        }

        public void SetVisible(bool v) { if (visual.gameObject.activeSelf != v) visual.gameObject.SetActive(v); }

        public void Tick(float dt, float speed, bool left, bool right, bool jump, bool slide)
        {
            JumpedThisFrame = false; SlidThisFrame = false;

            if (left && lane > 0) lane--;
            if (right && lane < 2) lane++;
            float tx = (lane - 1) * LaneW;
            x = Mathf.Lerp(x, tx, 1f - Mathf.Exp(-16f * dt));

            if (jump && grounded) { vy = 9.5f; grounded = false; slideT = 0f; JumpedThisFrame = true; }
            if (slide)
            {
                if (grounded) { if (slideT <= 0f) SlidThisFrame = true; slideT = 0.75f; }
                else vy = -18f; // fast-fall
            }
            if (!grounded)
            {
                vy -= 26f * dt;
                y += vy * dt;
                if (y <= 0f) { y = 0f; vy = 0f; grounded = true; }
            }
            if (slideT > 0f) slideT -= dt;

            z += speed * dt;
            Pose(dt, speed);
            Apply();
        }

        public void Idle(float dt)
        {
            legL.localEulerAngles = Vector3.zero; legR.localEulerAngles = Vector3.zero;
            armL.localEulerAngles = Vector3.zero; armR.localEulerAngles = Vector3.zero;
            visual.localEulerAngles = Vector3.zero;
            visual.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 2f) * 0.01f, 0f);
            Apply();
        }

        public void Fall(float dt)
        {
            lean = Mathf.Lerp(lean, -88f, 1f - Mathf.Exp(-5f * dt));
            visual.localEulerAngles = new Vector3(lean, 0f, 0f);
            visual.localPosition = new Vector3(0f, 0.15f, 0f);
        }

        void Pose(float dt, float speed)
        {
            phase += dt * (7f + speed * 0.45f);
            float s = Mathf.Sin(phase);
            if (!grounded)
            {
                legL.localEulerAngles = new Vector3(45f, 0f, 0f);
                legR.localEulerAngles = new Vector3(-25f, 0f, 0f);
                armL.localEulerAngles = new Vector3(-150f, 0f, 0f);
                armR.localEulerAngles = new Vector3(-150f, 0f, 0f);
            }
            else
            {
                legL.localEulerAngles = new Vector3(s * 48f, 0f, 0f);
                legR.localEulerAngles = new Vector3(-s * 48f, 0f, 0f);
                armL.localEulerAngles = new Vector3(-s * 45f, 0f, 0f);
                armR.localEulerAngles = new Vector3(s * 45f, 0f, 0f);
            }
            float target = Sliding ? -72f : 9f;
            lean = Mathf.Lerp(lean, target, 1f - Mathf.Exp(-18f * dt));
            visual.localEulerAngles = new Vector3(lean, 0f, 0f);
            float bob = (grounded && !Sliding) ? Mathf.Abs(s) * 0.07f : 0f;
            visual.localPosition = new Vector3(0f, bob + (Sliding ? 0.2f : 0f), 0f);
        }

        void Apply() { root.transform.position = new Vector3(x, y, z); }
    }
}
