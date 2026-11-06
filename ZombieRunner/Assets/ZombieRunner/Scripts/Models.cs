using UnityEngine;

namespace ZombieRunner
{
    /// <summary>Prop factories (all built from primitives).</summary>
    public static class Models
    {
        public static readonly Color[] CarColors =
        {
            new Color(0.55f, 0.18f, 0.14f), new Color(0.18f, 0.27f, 0.42f),
            new Color(0.45f, 0.45f, 0.42f), new Color(0.62f, 0.50f, 0.18f), new Color(0.22f, 0.36f, 0.28f)
        };

        static Transform NewRoot(string name, Transform parent, Vector3 pos, float rotY)
        {
            Transform g = new GameObject(name).transform;
            g.SetParent(parent, false);
            g.localPosition = pos;
            g.localEulerAngles = new Vector3(0f, rotY, 0f);
            return g;
        }

        /// <summary>Wrecked car, ~2 wide x 4.2 long x 1.5 tall. Origin on the ground.</summary>
        public static GameObject Car(Transform parent, Vector3 pos, float rotY, Color body, bool burnt)
        {
            Transform g = NewRoot("Car", parent, pos, rotY);
            Color b = burnt ? new Color(0.09f, 0.08f, 0.07f) : body;
            Color dark = new Color(0.05f, 0.06f, 0.08f);
            Prims.Make(Shape.Cube, g, new Vector3(0f, 0.6f, 0f), new Vector3(2f, 0.7f, 4.2f), b);
            Prims.Make(Shape.Cube, g, new Vector3(0f, 1.2f, -0.2f), new Vector3(1.8f, 0.6f, 2.2f), Prims.Shade(b, 0.85f));
            Prims.Make(Shape.Cube, g, new Vector3(0f, 1.22f, -0.2f), new Vector3(1.84f, 0.38f, 1.9f), dark);
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    Prims.Make(Shape.Cylinder, g, new Vector3(sx * 1.0f, 0.35f, sz * 1.4f),
                               new Vector3(0.7f, 0.12f, 0.7f), new Color(0.04f, 0.04f, 0.04f), new Vector3(0f, 0f, 90f));
            Color tail = new Color(0.9f, 0.1f, 0.05f);
            Prims.Make(Shape.Cube, g, new Vector3(-0.7f, 0.7f, -2.11f), new Vector3(0.3f, 0.15f, 0.05f), tail, Vector3.zero, true);
            Prims.Make(Shape.Cube, g, new Vector3(0.7f, 0.7f, -2.11f), new Vector3(0.3f, 0.15f, 0.05f), tail, Vector3.zero, true);
            if (burnt)
                Prims.Make(Shape.Cube, g, new Vector3(0f, 1.7f, 0.3f), new Vector3(1.0f, 0.5f, 1.0f),
                           new Color(1f, 0.45f, 0.1f), new Vector3(0f, 30f, 0f), true);
            return g.gameObject;
        }

        /// <summary>Concrete road barrier with hazard stripes. Must be jumped. ~2.6 wide, 0.9 tall.</summary>
        public static GameObject Barrier(Transform parent, Vector3 pos)
        {
            Transform g = NewRoot("Barrier", parent, pos, 0f);
            Prims.Make(Shape.Cube, g, new Vector3(0f, 0.45f, 0f), new Vector3(2.6f, 0.9f, 0.6f), new Color(0.50f, 0.49f, 0.47f));
            Color orange = new Color(0.95f, 0.45f, 0.05f);
            Prims.Make(Shape.Cube, g, new Vector3(-0.8f, 0.8f, 0f), new Vector3(0.45f, 0.2f, 0.62f), orange);
            Prims.Make(Shape.Cube, g, new Vector3(0f, 0.8f, 0f), new Vector3(0.45f, 0.2f, 0.62f), Color.white);
            Prims.Make(Shape.Cube, g, new Vector3(0.8f, 0.8f, 0f), new Vector3(0.45f, 0.2f, 0.62f), orange);
            return g.gameObject;
        }

        /// <summary>Fallen sign beam across a lane at head height. Must be slid under.</summary>
        public static GameObject Beam(Transform parent, Vector3 pos)
        {
            Transform g = NewRoot("Beam", parent, pos, 0f);
            Color metal = new Color(0.30f, 0.30f, 0.32f);
            Prims.Make(Shape.Cube, g, new Vector3(0f, 1.65f, 0f), new Vector3(2.8f, 0.55f, 0.35f), metal);
            Prims.Make(Shape.Cube, g, new Vector3(-0.9f, 1.65f, -0.19f), new Vector3(0.5f, 0.55f, 0.05f), new Color(0.9f, 0.15f, 0.1f), Vector3.zero, true);
            Prims.Make(Shape.Cube, g, new Vector3(0.9f, 1.65f, -0.19f), new Vector3(0.5f, 0.55f, 0.05f), new Color(0.9f, 0.15f, 0.1f), Vector3.zero, true);
            Prims.Make(Shape.Cylinder, g, new Vector3(-1.3f, 0.75f, 0f), new Vector3(0.18f, 0.8f, 0.18f), metal);
            Prims.Make(Shape.Cylinder, g, new Vector3(1.3f, 0.75f, 0f), new Vector3(0.18f, 0.8f, 0.18f), metal);
            return g.gameObject;
        }

        public static GameObject FireBarrel(Transform parent, Vector3 pos)
        {
            Transform g = NewRoot("FireBarrel", parent, pos, 0f);
            Prims.Make(Shape.Cylinder, g, new Vector3(0f, 0.5f, 0f), new Vector3(0.7f, 0.5f, 0.7f), new Color(0.25f, 0.12f, 0.08f));
            Prims.Make(Shape.Cube, g, new Vector3(0f, 1.15f, 0f), new Vector3(0.5f, 0.45f, 0.5f), new Color(1f, 0.5f, 0.1f), new Vector3(0f, 20f, 0f), true);
            Prims.Make(Shape.Cube, g, new Vector3(0.05f, 1.45f, 0f), new Vector3(0.3f, 0.35f, 0.3f), new Color(1f, 0.75f, 0.2f), new Vector3(0f, 50f, 0f), true);
            return g.gameObject;
        }

        /// <summary>Floating medkit pickup (centered on pos).</summary>
        public static GameObject Medkit(Transform parent, Vector3 pos)
        {
            Transform g = NewRoot("Medkit", parent, pos, 0f);
            Prims.Make(Shape.Cube, g, Vector3.zero, new Vector3(0.6f, 0.45f, 0.3f), new Color(0.95f, 0.95f, 0.95f), Vector3.zero, true);
            Color red = new Color(0.9f, 0.05f, 0.05f);
            for (int s = -1; s <= 1; s += 2)
            {
                Prims.Make(Shape.Cube, g, new Vector3(0f, 0f, s * 0.16f), new Vector3(0.36f, 0.1f, 0.02f), red, Vector3.zero, true);
                Prims.Make(Shape.Cube, g, new Vector3(0f, 0f, s * 0.16f), new Vector3(0.1f, 0.36f, 0.02f), red, Vector3.zero, true);
            }
            return g.gameObject;
        }

        /// <summary>Spinning supply token (centered on pos).</summary>
        public static GameObject Supply(Transform parent, Vector3 pos)
        {
            Transform g = NewRoot("Supply", parent, pos, 0f);
            Prims.Make(Shape.Cylinder, g, Vector3.zero, new Vector3(0.5f, 0.07f, 0.5f), new Color(1f, 0.78f, 0.15f), new Vector3(90f, 0f, 0f), true);
            return g.gameObject;
        }
    }
}
