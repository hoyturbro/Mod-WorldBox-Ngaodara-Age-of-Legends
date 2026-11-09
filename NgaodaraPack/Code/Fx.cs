using System;
using System.Collections.Generic;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ระบบเอฟเฟกต์ของมอดเอง (ไม่ใช้เอฟเฟกต์ของเกม): เล่นภาพทีละเฟรมจาก GameResources/effects/ngd/&lt;ชื่อ&gt;/
    /// ชุดภาพต่อธาตุ: slash_* (รอยฟัน) · burst_* (ระเบิดตอนโดน) · cast_* (วงร่ายเวท) · wave_* (คลื่นไม้ตาย) · buff_* (ประกายบัฟ)
    /// และ bolt (สายฟ้า) · pillar (เสาแสง) · teleport (ควันพริบตา)
    /// </summary>
    public class FxPlayer : MonoBehaviour
    {
        public static FxPlayer Inst;
        class Anim
        {
            public SpriteRenderer R; public Sprite[] Frames; public float Start, FrameTime;
            public Actor Follow; public Vector3 Offset; public bool Loop; public float Life;
        }
        readonly List<Anim> _live = new List<Anim>();
        readonly Stack<SpriteRenderer> _pool = new Stack<SpriteRenderer>();
        static readonly Dictionary<string, Sprite[]> _cache = new Dictionary<string, Sprite[]>();
        int _layer; bool _layerSet;

        void Awake() { Inst = this; }

        public static string DebugInfo()
        {
            if (Inst == null) return "no player";
            var sb = new System.Text.StringBuilder();
            sb.Append("layers=" + string.Join(",", Array.ConvertAll(SortingLayer.layers, l => l.name + ":" + l.value)));
            sb.Append(" live=" + Inst._live.Count);
            if (Inst._live.Count > 0)
            {
                var r = Inst._live[Inst._live.Count - 1].R;
                sb.Append($" r={r.sprite?.name} pos={r.transform.position} scale={r.transform.localScale} layer={SortingLayer.IDToName(r.sortingLayerID)} order={r.sortingOrder} active={r.gameObject.activeInHierarchy} bounds={r.bounds.size} col={r.color} goLayer={r.gameObject.layer}");
            }
            var cam = Camera.main;
            if (cam != null) sb.Append($" cam={cam.transform.position} ortho={cam.orthographicSize} mask={cam.cullingMask}");
            return sb.ToString();
        }

        public static Sprite[] Frames(string name)
        {
            if (_cache.TryGetValue(name, out var f)) return f;
            Sprite[] list = null;
            try { list = SpriteTextureLoader.getSpriteList("effects/ngd/" + name); } catch { }
            if (list != null)
            {
                Array.Sort(list, (a, b) => FrameNo(a.name).CompareTo(FrameNo(b.name)));
                foreach (var sp in list) if (sp != null && sp.texture != null) sp.texture.filterMode = FilterMode.Point;   // พิกเซลคม ไม่เบลอ
            }
            _cache[name] = list;
            if (list == null || list.Length == 0) NgdMod.LogOnce("fx missing: " + name);
            return list;
        }

        static int FrameNo(string n) { int i = n.LastIndexOf('_'); return i >= 0 && int.TryParse(n.Substring(i + 1), out int v) ? v : 0; }

        SpriteRenderer Get()
        {
            if (!_layerSet && SortingLayer.layers.Length > 0)
            {   // ชั้นเอฟเฟกต์บนสุดของเกม (เหนือตัวละคร) — ไม่ใช่ชั้น Debug
                int id = SortingLayer.NameToID("EffectsTop");
                _layer = SortingLayer.IsValid(id) ? id : SortingLayer.layers[SortingLayer.layers.Length - 1].id; _layerSet = true;
            }
            SpriteRenderer r;
            if (_pool.Count > 0) r = _pool.Pop();
            else
            {
                var go = new GameObject("ngd_fx"); go.transform.SetParent(transform, false);
                r = go.AddComponent<SpriteRenderer>(); r.sortingOrder = 31000;
            }
            if (_layerSet) r.sortingLayerID = _layer;
            r.gameObject.SetActive(true);
            return r;
        }

        /// <summary>เล่นเอฟเฟกต์ที่ตำแหน่งโลก · size = ความกว้างเป็นหน่วยโลก · angle องศา</summary>
        public static void Play(string name, Vector3 pos, float size, float angle = 0f, float frameTime = 0.05f, Actor follow = null, Color? tint = null, bool loop = false, float life = 0f)
        {
            if (Inst == null) return;
            var fr = Frames(name);
            if (fr == null || fr.Length == 0) return;
            var r = Inst.Get();
            r.sprite = fr[0];
            r.color = tint ?? Color.white;
            float sc = size / Mathf.Max(0.01f, fr[0].bounds.size.x);
            r.transform.localScale = new Vector3(sc, sc, 1f);
            r.transform.rotation = Quaternion.Euler(0, 0, angle);
            r.transform.position = new Vector3(pos.x, pos.y, 0f);
            Inst._live.Add(new Anim { R = r, Frames = fr, Start = Time.time, FrameTime = frameTime, Follow = follow,
                Offset = follow != null ? pos - (Vector3)follow.current_position : Vector3.zero, Loop = loop, Life = life });
        }

        void LateUpdate()
        {
            float now = Time.time;
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var a = _live[i];
                int f = (int)((now - a.Start) / a.FrameTime);
                bool done = a.Loop ? now - a.Start > a.Life : f >= a.Frames.Length;
                if (done || a.R == null)
                {
                    if (a.R != null) { a.R.gameObject.SetActive(false); _pool.Push(a.R); }
                    _live.RemoveAt(i); continue;
                }
                a.R.sprite = a.Frames[a.Loop ? f % a.Frames.Length : f];
                if (a.Follow != null && a.Follow.isAlive())
                    a.R.transform.position = new Vector3(a.Follow.current_position.x + a.Offset.x, a.Follow.current_position.y + a.Offset.y, 0f);
            }
        }
    }

    /// <summary>ชุดเอฟเฟกต์ของสกิล (เรียกจาก Skills)</summary>
    public static class Fx
    {
        public static readonly string[] Elements = { "phys", "fire", "ice", "thunder", "holy", "dark", "poison", "wind", "star", "blood", "nature" };

        /// <summary>สกิลกายภาพ + ใส่เซ็ตธาตุ = เอฟเฟกต์เปลี่ยนเป็นธาตุนั้น</summary>
        public static string ElementOf(Actor a, Skills.Skill s)
        {
            if (s.Fx != "phys") return s.Fx;
            var atk = Gear.Info(a).atk;
            switch (atk)
            {
                case "fire": case "dragon": return "fire";
                case "ice": case "moon": return "ice";
                case "thunder": case "clockwork": return "thunder";
                case "light": return "holy";
                case "dark": return "dark";
                case "plague": return "poison";
                case "wind": case "sakura": return "wind";
                case "star": return "star";
                case "blood": return "blood";
                case "earth": case "nature": case "abyss": return "nature";
            }
            return "phys";
        }

        static Vector3 P(Actor a, float up = 0.2f) => new Vector3(a.current_position.x, a.current_position.y + up, 0);

        public static void Slash(Actor from, Actor to, string el, int idx)
        {
            Vector2 d = to.current_position - from.current_position;
            float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + (idx % 2 == 0 ? 0 : 180) + UnityEngine.Random.Range(-25f, 25f);
            FxPlayer.Play("slash_" + el, P(to), 0.8f, ang, 0.05f);
        }

        public static void Impact(Actor t, string el, float scale) => FxPlayer.Play("burst_" + el, P(t), 0.55f * scale, UnityEngine.Random.Range(0, 360), 0.06f);

        public static void Cast(Actor a, Skills.Skill s)
        {
            string el = ElementOf(a, s);
            FxPlayer.Play("cast_" + el, P(a, -0.05f), s.Ult ? 1.4f : 0.9f, 0, 0.06f, a, null, true, s.Cast);
            if (s.Ult) FxPlayer.Play("pillar", P(a, 0.6f), 0.4f, 0, 0.06f, a, ElementColor(el), true, s.Cast);
        }

        public static void Area(Actor center, Skills.Skill s, string el) => FxPlayer.Play("wave_" + el, P(center, 0f), Mathf.Min(4f, s.Radius * 1.3f), 0, 0.06f);

        public static void Ultimate(Actor center, string el)
        {
            FxPlayer.Play("wave_" + el, P(center, 0f), 5f, 0, 0.07f);
            if (el == "thunder" || el == "holy" || el == "star")
                for (int i = 0; i < 3; i++)
                {
                    var off = UnityEngine.Random.insideUnitCircle * 1.4f;
                    FxPlayer.Play(el == "thunder" ? "bolt" : "pillar", new Vector3(center.current_position.x + off.x, center.current_position.y + off.y + 0.9f, 0), 0.5f, 0, 0.05f, null, ElementColor(el));
                }
        }

        public static void Buff(Actor a, string el) => FxPlayer.Play("buff_" + el, P(a, 0.3f), 0.85f, 0, 0.08f, a);
        public static void Teleport(Actor a) => FxPlayer.Play("teleport", P(a), 0.7f, 0, 0.05f);

        public static Color ElementColor(string el)
        {
            switch (el)
            {
                case "fire": return new Color(1f, 0.55f, 0.2f);
                case "ice": return new Color(0.6f, 0.9f, 1f);
                case "thunder": return new Color(1f, 0.95f, 0.4f);
                case "holy": return new Color(1f, 0.92f, 0.6f);
                case "dark": return new Color(0.75f, 0.45f, 1f);
                case "poison": return new Color(0.6f, 1f, 0.3f);
                case "wind": return new Color(0.6f, 1f, 0.85f);
                case "star": return new Color(0.6f, 0.75f, 1f);
                case "blood": return new Color(1f, 0.25f, 0.3f);
                case "nature": return new Color(0.55f, 0.9f, 0.4f);
            }
            return Color.white;
        }
    }

    /// <summary>ตัวเลขดาเมจ/ฮีลลอยขึ้น (ฟอนต์พิกเซล 3×5 สร้างในโค้ด)</summary>
    public class DamageNumbers : MonoBehaviour
    {
        public enum Style { Normal, Crit, Heal }
        public static DamageNumbers Inst;
        public static bool Enabled = true;
        static Sprite[] _digits;
        class Num { public List<SpriteRenderer> Parts = new List<SpriteRenderer>(); public float Start; public Vector3 Pos; public Color Col; public float Size; }
        readonly List<Num> _live = new List<Num>();
        readonly Stack<SpriteRenderer> _pool = new Stack<SpriteRenderer>();
        int _layer; bool _layerSet;

        static readonly string[] Font = {   // 0-9 + "+" (3x5 แถวบนลงล่าง)
            "111101101101111", "010110010010111", "111001111100111", "111001111001111", "101101111001001",
            "111100111001111", "111100111101111", "111001010010010", "111101111101111", "111101111001111", "000010111010000" };

        void Awake() { Inst = this; Build(); }

        static void Build()
        {
            _digits = new Sprite[Font.Length];
            for (int i = 0; i < Font.Length; i++)
            {
                var tex = new Texture2D(5, 7, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                var px = new Color32[35];
                for (int y = 0; y < 7; y++)
                    for (int x = 0; x < 5; x++)
                    {
                        bool on = x >= 1 && x <= 3 && y >= 1 && y <= 5 && Font[i][(5 - y) * 3 + (x - 1)] == '1';
                        bool edge = false;
                        if (!on)
                            for (int dy = -1; dy <= 1 && !edge; dy++)
                                for (int dx = -1; dx <= 1 && !edge; dx++)
                                {
                                    int xx = x + dx, yy = y + dy;
                                    edge = xx >= 1 && xx <= 3 && yy >= 1 && yy <= 5 && Font[i][(5 - yy) * 3 + (xx - 1)] == '1';
                                }
                        px[y * 5 + x] = on ? new Color32(255, 255, 255, 255) : edge ? new Color32(20, 10, 10, 255) : new Color32(0, 0, 0, 0);
                    }
                tex.SetPixels32(px); tex.Apply();
                _digits[i] = Sprite.Create(tex, new Rect(0, 0, 5, 7), new Vector2(0.5f, 0.5f), 1f);
            }
        }

        public static void Show(Actor a, int value, Style st)
        {
            if (!Enabled || Inst == null || a == null || value <= 0) return;
            var cam = World.world?.camera;
            if (cam == null || cam.orthographicSize > 60f) return;   // ซูมไกลไม่แสดง
            Inst.Spawn(new Vector3(a.current_position.x + UnityEngine.Random.Range(-0.2f, 0.2f), a.current_position.y + 0.8f, -7f), value, st);
        }

        void Spawn(Vector3 pos, int value, Style st)
        {
            if (!_layerSet && SortingLayer.layers.Length > 0) { _layer = SortingLayer.layers[SortingLayer.layers.Length - 1].id; _layerSet = true; }
            string s = (st == Style.Heal ? "+" : "") + value;
            var n = new Num { Start = Time.time, Pos = pos, Size = st == Style.Crit ? 0.34f : 0.24f,
                Col = st == Style.Crit ? new Color(1f, 0.85f, 0.2f) : st == Style.Heal ? new Color(0.45f, 1f, 0.45f) : Color.white };
            for (int i = 0; i < s.Length; i++)
            {
                int d = s[i] == '+' ? 10 : s[i] - '0';
                SpriteRenderer r;
                if (_pool.Count > 0) r = _pool.Pop();
                else { var go = new GameObject("ngd_num"); go.transform.SetParent(transform, false); r = go.AddComponent<SpriteRenderer>(); r.sortingOrder = 32500; }
                if (_layerSet) r.sortingLayerID = _layer;
                r.gameObject.SetActive(true);
                r.sprite = _digits[d];
                n.Parts.Add(r);
            }
            _live.Add(n);
            if (_live.Count > 120) Recycle(_live[0]);
        }

        void Recycle(Num n)
        {
            foreach (var r in n.Parts) { r.gameObject.SetActive(false); _pool.Push(r); }
            _live.Remove(n);
        }

        void LateUpdate()
        {
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var n = _live[i];
                float t = (Time.time - n.Start) / 0.9f;
                if (t >= 1f) { Recycle(n); continue; }
                float sc = n.Size / 7f * (t < 0.12f ? 1f + (0.12f - t) * 4f : 1f);   // เด้งตอนเกิด
                float w = 4f * sc * 7f / 7f;
                float x0 = n.Pos.x - (n.Parts.Count - 1) * w * 0.5f;
                var c = n.Col; c.a = t > 0.6f ? 1f - (t - 0.6f) / 0.4f : 1f;
                for (int k = 0; k < n.Parts.Count; k++)
                {
                    var r = n.Parts[k];
                    r.transform.position = new Vector3(x0 + k * w, n.Pos.y + t * 0.6f, -7f);
                    r.transform.localScale = new Vector3(sc, sc, 1f);
                    r.color = c;
                }
            }
        }
    }
}
