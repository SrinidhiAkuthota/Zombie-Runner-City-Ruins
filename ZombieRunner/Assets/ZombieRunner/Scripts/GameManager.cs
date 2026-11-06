using UnityEngine;

namespace ZombieRunner
{
    public enum GameState { Title, Playing, Dead }

    /// <summary>
    /// Self-bootstrapping: this component is created automatically when you press Play
    /// in ANY scene (see Create()). It builds the camera, lighting, world, player and UI.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Instance = null; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            if (Instance != null) return;
            new GameObject("ZombieRunner").AddComponent<GameManager>();
        }

        Player player;
        World world;
        Horde horde;
        Camera cam;

        GameState state = GameState.Title;
        bool paused, newBest;
        float speed, timeAlive, hordeDist, invuln, shake, flash, deadTimer, camX;
        int supplies, bonus, best;

        const float MaxHordeDist = 24f;

        int Score { get { return Mathf.FloorToInt(player.z) + supplies * 10 + bonus; } }

        // ------------------------------------------------------------------ setup

        void Awake()
        {
            Instance = this;
            if (Application.isMobilePlatform) Application.targetFrameRate = 60;
            best = PlayerPrefs.GetInt("ZR_Best", 0);

            SetupEnvironment();
            Sfx.Init(gameObject);
            world = new World();
            player = new Player();
            horde = new Horde(14);
            ResetRun();
        }

        void SetupEnvironment()
        {
            Color fog = new Color(0.34f, 0.22f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fog;
            RenderSettings.fogStartDistance = 35f;
            RenderSettings.fogEndDistance = 150f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.50f, 0.40f, 0.40f);
            RenderSettings.skybox = null;

            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) l.enabled = false;

            Light sun = new GameObject("Dusk Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.65f, 0.45f);
            sun.intensity = 1.1f;
            sun.shadows = LightShadows.None;
            sun.transform.rotation = Quaternion.Euler(35f, -30f, 0f);

            cam = Camera.main;
            if (cam == null)
            {
                GameObject go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            if (cam.GetComponent<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = fog;
            cam.fieldOfView = 62f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 400f;
        }

        void ResetRun()
        {
            world.Reset();
            player.Reset();
            horde.Reset();
            speed = 12f; timeAlive = 0f; hordeDist = 22f; invuln = 0f;
            supplies = 0; bonus = 0; shake = 0f; flash = 0f; newBest = false;
            world.Tick(0f, 0f);
            horde.Update(0f, 0f, 0f, hordeDist, 8f);
            UpdateCamera(0f, true);
        }

        // ------------------------------------------------------------------ loop

        void Update()
        {
            GameInput.Poll();
            float dt = Mathf.Min(Time.deltaTime, 0.05f);

            switch (state)
            {
                case GameState.Title:
                    player.Idle(dt);
                    horde.Update(dt, 0f, 0f, hordeDist, 8f);
                    UpdateCamera(dt, true);
                    if (GameInput.Confirm) state = GameState.Playing;
                    break;

                case GameState.Playing:
                    if (GameInput.Pause) paused = !paused;
                    if (!paused) PlayTick(dt);
                    break;

                case GameState.Dead:
                    deadTimer += dt;
                    player.Fall(dt);
                    hordeDist = Mathf.Max(0.8f, hordeDist - 5f * dt);
                    horde.Update(dt, player.z, player.x, hordeDist, 14f);
                    flash = Mathf.Max(0f, flash - dt);
                    UpdateCamera(dt, false);
                    if (deadTimer > 0.8f && GameInput.Confirm)
                    {
                        ResetRun();
                        state = GameState.Playing;
                    }
                    break;
            }
        }

        void PlayTick(float dt)
        {
            timeAlive += dt;
            float target = Mathf.Min(12f + timeAlive * 0.22f, 26f);
            speed = Mathf.MoveTowards(speed, target, 5f * dt);

            bool jump = GameInput.Jump || GameInput.Tap;
            player.Tick(dt, speed, GameInput.Left, GameInput.Right, jump, GameInput.Slide);
            if (player.JumpedThisFrame) Sfx.Play("jump");
            if (player.SlidThisFrame) Sfx.Play("slide");

            world.Tick(player.z, dt);

            invuln = Mathf.Max(0f, invuln - dt);
            player.SetVisible(invuln <= 0f || (((int)(Time.time * 14f)) & 1) == 0);

            CheckCollisions();

            hordeDist = Mathf.Min(hordeDist + 0.5f * dt, MaxHordeDist);
            shake = Mathf.Max(0f, shake - 2.5f * dt);
            flash = Mathf.Max(0f, flash - 1.8f * dt);

            horde.Update(dt, player.z, player.x, hordeDist, 9f + speed * 0.3f);
            UpdateCamera(dt, false);

            if (hordeDist <= 2f) Die();
        }

        void CheckCollisions()
        {
            float px = player.x, pz = player.z, py = player.y, ph = player.Height;
            for (int i = 0; i < world.items.Count; i++)
            {
                Item it = world.items[i];
                if (it.dead || it.hit || it.go == null) continue;
                Vector3 p = it.go.transform.position;
                if (Mathf.Abs(p.z - pz) > it.hz + 0.3f) continue;
                if (Mathf.Abs(p.x - px) > it.hx + 0.35f) continue;
                if (py > it.yMax || py + ph < it.yMin) continue;
                OnTouch(it);
            }
        }

        void OnTouch(Item it)
        {
            switch (it.kind)
            {
                case Kind.Supply:
                    supplies++;
                    Sfx.Play("coin", 0.6f);
                    world.Kill(it);
                    break;
                case Kind.Medkit:
                    hordeDist = Mathf.Min(MaxHordeDist, hordeDist + 7f);
                    bonus += 50;
                    Sfx.Play("med");
                    world.Kill(it);
                    break;
                default:
                    if (invuln > 0f) break;
                    float pen = it.kind == Kind.Car ? 6f : (it.kind == Kind.Zombie ? 7f : 4f);
                    hordeDist -= pen;
                    speed = Mathf.Max(8f, speed * 0.72f);
                    invuln = 1.4f;
                    shake = 0.7f;
                    flash = 1f;
                    Sfx.Play("hit");
                    Sfx.Play("crunch");
                    it.hit = true;
                    if (it.kind != Kind.Car) world.Kill(it);
                    break;
            }
        }

        void Die()
        {
            state = GameState.Dead;
            deadTimer = 0f;
            flash = 1f;
            player.SetVisible(true);
            Sfx.Play("over");
            int s = Score;
            if (s > best)
            {
                best = s; newBest = true;
                PlayerPrefs.SetInt("ZR_Best", best);
                PlayerPrefs.Save();
            }
        }

        void UpdateCamera(float dt, bool snap)
        {
            Transform t = cam.transform;
            if (state == GameState.Title)
            {
                t.position = new Vector3(2.4f, 1.9f, player.z - 4.8f);
                t.LookAt(new Vector3(0f, 1.3f, player.z + 2f));
                return;
            }
            camX = snap ? player.x : Mathf.Lerp(camX, player.x, 1f - Mathf.Exp(-6f * dt));
            Vector3 pos = new Vector3(camX * 0.7f, 4.2f + player.y * 0.35f, player.z - 7.5f);
            if (shake > 0f) pos += Random.insideUnitSphere * shake * 0.25f;
            t.position = pos;
            t.LookAt(new Vector3(camX * 0.9f, 1.4f + player.y * 0.2f, player.z + 9f));
            float fov = 62f + Mathf.Clamp(speed - 12f, 0f, 14f) * 0.6f;
            cam.fieldOfView = snap ? fov : Mathf.Lerp(cam.fieldOfView, fov, 1f - Mathf.Exp(-3f * dt));
        }

        // ------------------------------------------------------------------ HUD

        GUIStyle big, mid, small;
        int styleH = -1;

        void EnsureStyles()
        {
            if (big != null && styleH == Screen.height) return;
            styleH = Screen.height;
            big = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(18, styleH / 8), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            mid = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(14, styleH / 16), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            small = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(11, styleH / 34), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        }

        void Text(Rect r, string t, GUIStyle s, Color c, TextAnchor a = TextAnchor.MiddleCenter)
        {
            s.alignment = a;
            s.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
            GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), t, s);
            s.normal.textColor = c;
            GUI.Label(r, t, s);
        }

        void Fill(Rect r, Color c)
        {
            Color prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = prev;
        }

        void OnGUI()
        {
            if (player == null) return;
            EnsureStyles();
            float W = Screen.width, H = Screen.height;
            Color orange = new Color(1f, 0.55f, 0.15f);
            Color white = new Color(1f, 0.95f, 0.9f);

            if (state != GameState.Title)
            {
                // danger tint + hit flash
                float danger = Mathf.Clamp01((12f - hordeDist) / 12f) * 0.40f;
                float a = Mathf.Max(danger, flash * 0.45f);
                if (a > 0.01f) Fill(new Rect(0, 0, W, H), new Color(0.8f, 0f, 0f, a));

                Text(new Rect(20, 10, W * 0.5f, H * 0.07f), "SCORE  " + Score, mid, white, TextAnchor.MiddleLeft);
                Text(new Rect(20, 10 + H * 0.07f, W * 0.5f, H * 0.05f), "SUPPLIES  " + supplies + "     BEST  " + best, small, orange, TextAnchor.MiddleLeft);

                // horde proximity bar
                float bw = W * 0.34f, bh = H * 0.028f, bx = (W - bw) * 0.5f, by = 14f;
                float safe = Mathf.Clamp01(hordeDist / MaxHordeDist);
                Fill(new Rect(bx - 2, by - 2, bw + 4, bh + 4), new Color(0f, 0f, 0f, 0.6f));
                Fill(new Rect(bx, by, bw * safe, bh), Color.Lerp(new Color(0.9f, 0.1f, 0.1f), new Color(0.3f, 0.85f, 0.3f), safe));
                Text(new Rect(bx, by + bh + 2, bw, H * 0.04f), safe < 0.4f ? "THE HORDE IS CLOSE!" : "HORDE DISTANCE", small, white);
            }

            if (state == GameState.Title)
            {
                Fill(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, 0.45f));
                Text(new Rect(0, H * 0.10f, W, H * 0.16f), "ZOMBIE RUNNER", big, new Color(0.9f, 0.15f, 0.1f));
                Text(new Rect(0, H * 0.26f, W, H * 0.09f), "C I T Y   R U I N S", mid, orange);
                Text(new Rect(0, H * 0.50f, W, H * 0.05f), "A / D  or  LEFT / RIGHT  -  change lane", small, white);
                Text(new Rect(0, H * 0.55f, W, H * 0.05f), "SPACE / W / UP  -  jump over barriers", small, white);
                Text(new Rect(0, H * 0.60f, W, H * 0.05f), "S / DOWN  -  slide under beams", small, white);
                Text(new Rect(0, H * 0.65f, W, H * 0.05f), "Touch: swipe left / right / up / down      P - pause", small, white);
                float blink = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 4f);
                Text(new Rect(0, H * 0.78f, W, H * 0.08f), "PRESS SPACE  /  TAP  TO  RUN", mid, new Color(1f, 1f, 1f, blink));
                Text(new Rect(0, H * 0.88f, W, H * 0.05f), "Grab supplies and medkits. Smash into things and the horde catches up.", small, orange);
            }
            else if (state == GameState.Dead)
            {
                Fill(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, 0.45f));
                Text(new Rect(0, H * 0.18f, W, H * 0.16f), "OVERRUN!", big, new Color(0.9f, 0.15f, 0.1f));
                Text(new Rect(0, H * 0.38f, W, H * 0.09f), "SCORE  " + Score, mid, white);
                Text(new Rect(0, H * 0.47f, W, H * 0.07f), newBest ? "NEW BEST!" : "BEST  " + best, mid, orange);
                Text(new Rect(0, H * 0.56f, W, H * 0.05f), "Distance " + Mathf.FloorToInt(player.z) + " m     Supplies " + supplies, small, white);
                if (deadTimer > 0.8f)
                {
                    float blink = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 4f);
                    Text(new Rect(0, H * 0.72f, W, H * 0.08f), "R / SPACE / TAP  TO  TRY  AGAIN", mid, new Color(1f, 1f, 1f, blink));
                }
            }
            else if (paused)
            {
                Fill(new Rect(0, 0, W, H), new Color(0f, 0f, 0f, 0.5f));
                Text(new Rect(0, H * 0.35f, W, H * 0.16f), "PAUSED", big, white);
                Text(new Rect(0, H * 0.52f, W, H * 0.06f), "Press P to resume", small, orange);
            }
        }
    }
}
