using System.Collections.Generic;
using UnityEngine;

namespace ZombieRunner
{
    public enum Kind { Barrier, Beam, Car, Zombie, Medkit, Supply }

    public class Item
    {
        public GameObject go;
        public Kind kind;
        public float hx, hz, yMin, yMax;   // collision box: half-extents in X/Z, absolute Y range
        public float vz, spin;
        public ZombieRig rig;
        public bool dead, hit;
    }

    /// <summary>Endless procedurally generated ruined-city street.</summary>
    public class World
    {
        public const float ChunkLen = 40f;
        const float Ahead = 170f;

        public readonly List<Item> items = new List<Item>();

        readonly Transform root, itemsRoot;
        readonly Queue<GameObject> chunks = new Queue<GameObject>();
        int nextChunk;

        static readonly Color[] concrete =
        {
            new Color(0.42f, 0.41f, 0.40f), new Color(0.35f, 0.34f, 0.34f), new Color(0.48f, 0.44f, 0.40f),
            new Color(0.30f, 0.30f, 0.33f), new Color(0.40f, 0.33f, 0.30f)
        };
        static readonly Color asphalt = new Color(0.15f, 0.15f, 0.16f);
        static readonly Color ground = new Color(0.17f, 0.15f, 0.13f);
        static readonly Color sidewalk = new Color(0.33f, 0.32f, 0.31f);
        static readonly Color windowDark = new Color(0.05f, 0.06f, 0.08f);
        static readonly Color rust = new Color(0.40f, 0.22f, 0.12f);

        public World()
        {
            root = new GameObject("World").transform;
            itemsRoot = new GameObject("Items").transform;
            itemsRoot.SetParent(root, false);
        }

        public void Reset()
        {
            foreach (GameObject g in chunks) if (g != null) Object.Destroy(g);
            chunks.Clear();
            foreach (Item it in items) if (it.go != null) Object.Destroy(it.go);
            items.Clear();
            nextChunk = 0;
        }

        public void Kill(Item it)
        {
            it.dead = true;
            if (it.go != null) Object.Destroy(it.go);
        }

        public void Tick(float playerZ, float dt)
        {
            while (nextChunk * ChunkLen < playerZ + Ahead) SpawnChunk(nextChunk++);

            while (chunks.Count > 0 && chunks.Peek().transform.position.z + ChunkLen < playerZ - 45f)
                Object.Destroy(chunks.Dequeue());

            for (int i = items.Count - 1; i >= 0; i--)
            {
                Item it = items[i];
                if (it.dead || it.go == null) { items.RemoveAt(i); continue; }
                Transform t = it.go.transform;
                if (t.position.z < playerZ - 25f) { Object.Destroy(it.go); items.RemoveAt(i); continue; }
                if (it.vz != 0f)
                {
                    Vector3 p = t.position; p.z += it.vz * dt; t.position = p;
                }
                if (it.rig != null) it.rig.Animate(dt, 10f);
                if (it.spin != 0f) t.Rotate(0f, it.spin * dt, 0f, Space.World);
            }
        }

        // ------------------------------------------------------------------ chunks

        static Color Pick(Color[] a) { return a[Random.Range(0, a.Length)]; }

        void SpawnChunk(int idx)
        {
            float z0 = idx * ChunkLen;
            GameObject cg = new GameObject("Chunk " + idx);
            cg.transform.SetParent(root, false);
            cg.transform.position = new Vector3(0f, 0f, z0);
            chunks.Enqueue(cg);
            Transform c = cg.transform;

            // ground + road
            Prims.Make(Shape.Cube, c, new Vector3(0f, -0.3f, ChunkLen * 0.5f), new Vector3(400f, 0.2f, ChunkLen), ground);
            Prims.Make(Shape.Cube, c, new Vector3(0f, -0.1f, ChunkLen * 0.5f), new Vector3(10f, 0.2f, ChunkLen), asphalt);

            // worn lane lines
            Color line = new Color(0.55f, 0.5f, 0.3f);
            for (int k = 0; k < 6; k++)
            {
                if (Random.value < 0.25f) continue;
                float zz = 3f + k * 6.5f;
                Prims.Make(Shape.Cube, c, new Vector3(-1.5f, 0.01f, zz), new Vector3(0.12f, 0.02f, 2.4f), line);
                Prims.Make(Shape.Cube, c, new Vector3(1.5f, 0.01f, zz), new Vector3(0.12f, 0.02f, 2.4f), line);
            }
            // cracks + blood stains
            for (int k = 0; k < 5; k++)
                Prims.Make(Shape.Cube, c, new Vector3(Random.Range(-4.5f, 4.5f), 0.012f, Random.Range(0f, ChunkLen)),
                           new Vector3(Random.Range(0.06f, 0.12f), 0.02f, Random.Range(2f, 6f)), new Color(0.05f, 0.05f, 0.05f),
                           new Vector3(0f, Random.Range(-25f, 25f), 0f));
            for (int k = 0; k < 2; k++)
                Prims.Make(Shape.Cube, c, new Vector3(Random.Range(-4f, 4f), 0.014f, Random.Range(0f, ChunkLen)),
                           new Vector3(Random.Range(0.8f, 2f), 0.02f, Random.Range(0.8f, 2.2f)), new Color(0.25f, 0.03f, 0.03f),
                           new Vector3(0f, Random.Range(0f, 90f), 0f));

            BuildSide(c, -1);
            BuildSide(c, 1);

            float diff = Mathf.Clamp01(idx / 30f);
            for (int k = 0; k < 4; k++) SpawnRow(z0 + 5f + k * 10f, diff);
        }

        void BuildSide(Transform c, int side)
        {
            // sidewalk
            Prims.Make(Shape.Cube, c, new Vector3(side * 6.5f, 0f, ChunkLen * 0.5f), new Vector3(3f, 0.3f, ChunkLen), sidewalk);

            // distant skyline silhouettes
            for (int k = 0; k < 2; k++)
                Prims.Make(Shape.Cube, c, new Vector3(side * Random.Range(40f, 70f), 0f, Random.Range(0f, ChunkLen)),
                           new Vector3(Random.Range(12f, 25f), Random.Range(60f, 160f), Random.Range(12f, 25f)),
                           new Color(0.17f, 0.15f, 0.17f));

            // buildings / rubble lots
            float z = Random.Range(0f, 2f);
            while (z < ChunkLen - 5f)
            {
                float d = Random.Range(9f, 16f);
                if (z + d > ChunkLen) d = ChunkLen - z;
                if (d < 5f) break;
                float zc = z + d * 0.5f;
                if (Random.value < 0.18f) BuildLot(c, side, zc, d);
                else BuildBuilding(c, side, zc, d, Random.Range(8f, 14f));
                z += d;
            }

            // sidewalk rubble
            for (int k = 0; k < 6; k++)
            {
                float s = Random.Range(0.3f, 0.9f);
                Prims.Make(Shape.Cube, c, new Vector3(side * Random.Range(5.3f, 7.8f), 0.2f, Random.Range(0f, ChunkLen)),
                           new Vector3(s, s * Random.Range(0.5f, 1f), s * Random.Range(0.8f, 1.5f)), Pick(concrete),
                           new Vector3(Random.Range(-20f, 20f), Random.Range(0f, 90f), Random.Range(-20f, 20f)));
            }

            // street lamps
            for (int k = 0; k < 2; k++) BuildLamp(c, side, 8f + k * 20f + Random.Range(-2f, 2f));

            // wrecked car on the sidewalk
            if (Random.value < 0.5f)
                Models.Car(c, new Vector3(side * 6.6f, 0.15f, Random.Range(6f, 34f)), Random.Range(-25f, 25f),
                           Models.CarColors[Random.Range(0, Models.CarColors.Length)], Random.value < 0.5f);

            // burning barrel
            if (Random.value < 0.6f)
                Models.FireBarrel(c, new Vector3(side * Random.Range(5.6f, 7.4f), 0.15f, Random.Range(2f, 38f)));
        }

        void BuildBuilding(Transform c, int side, float zc, float d, float w)
        {
            float h = Random.Range(10f, 34f);
            float x0 = side * (8.4f + w * 0.5f);
            Color col = Pick(concrete);
            Prims.Make(Shape.Cube, c, new Vector3(x0, h * 0.5f, zc), new Vector3(w, h, d), col);

            float face = x0 - side * w * 0.5f;   // road-facing wall
            for (int s = 0; s < 3; s++)
                Prims.Make(Shape.Cube, c, new Vector3(face - side * 0.04f, h * 0.5f + 1f, zc + (s - 1) * d * 0.3f),
                           new Vector3(0.1f, h - 4f, d * 0.14f), windowDark);
            int holes = Random.Range(1, 3);
            for (int k = 0; k < holes; k++)
                Prims.Make(Shape.Cube, c, new Vector3(face - side * 0.05f, Random.Range(3f, h - 3f), zc + Random.Range(-d * 0.3f, d * 0.3f)),
                           new Vector3(0.1f, Random.Range(2f, 4f), Random.Range(2f, 5f)), new Color(0.02f, 0.02f, 0.03f));

            // jagged broken top
            int steps = Random.Range(2, 4);
            for (int k = 0; k < steps; k++)
            {
                float sh = Random.Range(1.5f, 5f);
                Prims.Make(Shape.Cube, c,
                           new Vector3(x0 + Random.Range(-w * 0.25f, w * 0.25f), h + sh * 0.5f, zc + Random.Range(-d * 0.25f, d * 0.25f)),
                           new Vector3(w * Random.Range(0.3f, 0.6f), sh, d * Random.Range(0.3f, 0.6f)), Prims.Shade(col, 0.9f));
            }
            Prims.Make(Shape.Cube, c, new Vector3(x0 - side * w * 0.2f, h + 1f, zc), new Vector3(w * 0.7f, 0.4f, d * 0.7f),
                       Prims.Shade(col, 0.8f), new Vector3(0f, 0f, side * Random.Range(12f, 28f)));
            for (int k = 0; k < 3; k++)
                Prims.Make(Shape.Cylinder, c, new Vector3(x0 + Random.Range(-w / 3f, w / 3f), h + 3f, zc + Random.Range(-d / 3f, d / 3f)),
                           new Vector3(0.08f, 1.5f, 0.08f), rust,
                           new Vector3(Random.Range(-15f, 15f), 0f, Random.Range(-15f, 15f)));
        }

        void BuildLot(Transform c, int side, float zc, float d)
        {
            for (int i = 0; i < 7; i++)
            {
                float s = Random.Range(0.8f, 3.2f);
                Prims.Make(Shape.Cube, c,
                           new Vector3(side * Random.Range(9.5f, 16f), s * 0.35f, zc + Random.Range(-d * 0.45f, d * 0.45f)),
                           new Vector3(s * Random.Range(0.8f, 1.6f), s * 0.7f, s * Random.Range(0.8f, 1.6f)), Pick(concrete),
                           new Vector3(Random.Range(-15f, 15f), Random.Range(0f, 90f), Random.Range(-15f, 15f)));
            }
            Prims.Make(Shape.Cube, c, new Vector3(side * 11f, 2.2f, zc), new Vector3(0.5f, 4.5f, Mathf.Min(d * 0.6f, 7f)),
                       Prims.Shade(Pick(concrete), 0.9f), new Vector3(0f, 0f, -side * 18f));
            Models.FireBarrel(c, new Vector3(side * 9f, 0f, zc + 1f));
        }

        void BuildLamp(Transform c, int side, float z)
        {
            float tilt = Random.Range(-6f, 6f);
            Prims.Make(Shape.Cylinder, c, new Vector3(side * 5.4f, 2.4f, z), new Vector3(0.15f, 2.2f, 0.15f), new Color(0.12f, 0.12f, 0.13f), new Vector3(0f, 0f, tilt));
            Prims.Make(Shape.Cube, c, new Vector3(side * 4.8f, 4.7f, z), new Vector3(1.3f, 0.1f, 0.1f), new Color(0.12f, 0.12f, 0.13f));
            Prims.Make(Shape.Sphere, c, new Vector3(side * 4.2f, 4.55f, z), Vector3.one * 0.3f, new Color(0.9f, 0.7f, 0.3f), Vector3.zero, Random.value < 0.4f);
        }

        // ------------------------------------------------------------------ gameplay items

        Item Add(Kind k, GameObject go, float hx, float hz, float yMin, float yMax)
        {
            Item it = new Item();
            it.go = go; it.kind = k; it.hx = hx; it.hz = hz; it.yMin = yMin; it.yMax = yMax;
            items.Add(it);
            return it;
        }

        static float LaneX(int lane) { return (lane - 1) * Player.LaneW; }

        void SpawnObstacle(Kind k, int lane, float z)
        {
            float x = LaneX(lane);
            switch (k)
            {
                case Kind.Barrier:
                    Add(k, Models.Barrier(itemsRoot, new Vector3(x, 0f, z)), 1.3f, 0.4f, 0f, 0.9f);
                    break;
                case Kind.Beam:
                    Add(k, Models.Beam(itemsRoot, new Vector3(x, 0f, z)), 1.35f, 0.3f, 1.1f, 2.2f);
                    break;
                case Kind.Car:
                    Add(k, Models.Car(itemsRoot, new Vector3(x, 0f, z), Random.Range(-6f, 6f),
                        Models.CarColors[Random.Range(0, Models.CarColors.Length)], Random.value < 0.3f), 1.0f, 2.1f, 0f, 1.6f);
                    break;
                case Kind.Zombie:
                    ZombieRig rig = new ZombieRig(itemsRoot);
                    rig.root.transform.position = new Vector3(x, 0f, z);
                    rig.root.transform.eulerAngles = new Vector3(0f, 180f, 0f);
                    Item it = Add(k, rig.root, 0.4f, 0.4f, 0f, 1.4f);
                    it.vz = -3.5f;
                    it.rig = rig;
                    break;
            }
        }

        void SpawnAllLanes(Kind k, float z)
        {
            for (int lane = 0; lane < 3; lane++) SpawnObstacle(k, lane, z);
        }

        void SpawnSupplies(int lane, float z, int n)
        {
            float x = LaneX(lane);
            bool arc = Random.value < 0.35f;
            for (int k = 0; k < n; k++)
            {
                float y = arc ? 0.9f + 1.0f * Mathf.Sin(Mathf.PI * k / (n - 1f)) : 0.9f;
                GameObject go = Models.Supply(itemsRoot, new Vector3(x, y, z + k * 1.8f));
                Item it = Add(Kind.Supply, go, 0.5f, 0.5f, y - 0.5f, y + 0.5f);
                it.spin = 220f;
            }
        }

        void SpawnMedkit(int lane, float z)
        {
            GameObject go = Models.Medkit(itemsRoot, new Vector3(LaneX(lane), 1.0f, z));
            Item it = Add(Kind.Medkit, go, 0.6f, 0.6f, 0.4f, 1.6f);
            it.spin = 120f;
        }

        static Kind RandomKind(float diff)
        {
            float v = Random.value;
            float zombieShare = 0.25f + 0.2f * diff;
            if (v < zombieShare) return Kind.Zombie;
            v = Random.value;
            if (v < 0.35f) return Kind.Barrier;
            if (v < 0.60f) return Kind.Beam;
            return Kind.Car;
        }

        void SpawnRow(float z, float diff)
        {
            if (z < 50f) return;                 // safe start
            float r = Random.value;
            int a = Random.Range(0, 3);

            if (r < 0.18f)                        // supply trail (+ rare medkit)
            {
                SpawnSupplies(a, z, 6);
                if (Random.value < 0.18f) SpawnMedkit((a + 1) % 3, z + 4f);
            }
            else if (r < 0.42f)                   // single obstacle
            {
                SpawnObstacle(RandomKind(diff), a, z);
            }
            else if (r < 0.60f)                   // two lanes blocked, one free (with reward)
            {
                int b = (a + Random.Range(1, 3)) % 3;
                int free = 3 - a - b;
                SpawnObstacle(RandomKind(diff), a, z);
                SpawnObstacle(RandomKind(diff), b, z + Random.Range(-1.5f, 1.5f));
                SpawnSupplies(free, z - 3f, 4);
            }
            else if (r < 0.70f) SpawnAllLanes(Kind.Barrier, z);   // jump
            else if (r < 0.78f) SpawnAllLanes(Kind.Beam, z);      // slide
            else if (r < 0.90f)                   // zombie pack
            {
                int b = (a + Random.Range(1, 3)) % 3;
                int n = Random.value < 0.5f ? 2 : 3;
                for (int k = 0; k < n; k++)
                {
                    SpawnObstacle(Kind.Zombie, a, z + k * 2.6f);
                    SpawnObstacle(Kind.Zombie, b, z + k * 2.6f + 1.3f);
                }
            }
            else                                  // mixed: barrier + beam + free lane
            {
                SpawnObstacle(Kind.Barrier, a, z);
                SpawnObstacle(Kind.Beam, (a + 1) % 3, z);
                SpawnSupplies((a + 2) % 3, z - 3f, 4);
            }
        }
    }
}
