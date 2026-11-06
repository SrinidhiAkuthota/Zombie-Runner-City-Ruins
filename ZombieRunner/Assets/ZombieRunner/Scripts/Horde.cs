using System.Collections.Generic;
using UnityEngine;

namespace ZombieRunner
{
    /// <summary>The pack chasing the player. Its distance is an abstract value owned by GameManager.</summary>
    public class Horde
    {
        class Member { public ZombieRig rig; public float ox, oz; }

        readonly List<Member> members = new List<Member>();
        float x;

        public Horde(int count)
        {
            GameObject root = new GameObject("Horde");
            for (int i = 0; i < count; i++)
            {
                Member m = new Member();
                m.rig = new ZombieRig(root.transform);
                m.ox = Random.Range(-4f, 4f);
                m.oz = -Random.Range(0f, 8f);
                members.Add(m);
            }
        }

        public void Reset() { x = 0f; }

        public void Update(float dt, float playerZ, float playerX, float dist, float rate)
        {
            x = Mathf.Lerp(x, playerX, 1f - Mathf.Exp(-2f * dt));
            for (int i = 0; i < members.Count; i++)
            {
                Member m = members[i];
                float px = Mathf.Clamp(x * 0.6f + m.ox, -4.6f, 4.6f);
                m.rig.root.transform.position = new Vector3(px, 0f, playerZ - dist + m.oz);
                m.rig.Animate(dt, rate);
            }
        }
    }
}
