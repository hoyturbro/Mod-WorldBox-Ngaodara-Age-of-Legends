using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.General.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NgaodaraPack
{
    /// <summary>
    /// ตัวสร้างโลกเงาดารา: กดปุ่มเดียวได้โลกใหม่พร้อมอาณาจักร
    ///   1) เลือกรูปแบบโลก (5 แบบของมอด) และขนาด
    ///   2) เลือกจำนวนอาณาจักรของแต่ละเผ่า ประชากรเริ่มต้น ยุคเริ่มต้น และฮีโร่เริ่มต้น
    ///   3) กด "สร้างโลก" → เกมสร้างแผนที่ แล้วมอดวางเผ่าต่าง ๆ ให้ตั้งเมืองทันที (กระจายห่างกัน)
    /// </summary>
    public static class WorldGen
    {
        const string WindowId = "ngd_worldgen";
        const float W = 206f;

        // รูปแบบโลก: id, แม่แบบของเกม, การปรับ
        static readonly (string id, string from, Action<MapGenValues> tweak)[] Templates =
        {
            ("ngd_world_continents", "continent", v => { v.random_shapes_amount = 8; v.ring_effect = false; }),
            ("ngd_world_pangaea", "continent", v => { v.random_shapes_amount = 2; v.add_center_gradient_land = true; v.gradient_round_edges = true; v.ring_effect = false; }),
            ("ngd_world_archipelago", "islands", v => { }),
            ("ngd_world_highlands", "continent", v => { v.high_ground = true; v.add_mountain_edges = true; v.random_shapes_amount = 5; }),
            ("ngd_world_lakeland", "continent", v => { v.add_center_lake = true; v.low_ground = true; v.random_shapes_amount = 6; }),
        };
        static readonly string[] Sizes = { "standard", "large", "huge", "gigantic", "ngd_colossal" };
        public static readonly string[] MainRaces = { "human", "elf", "orc", "dwarf" };
        static readonly int[] PopOptions = { 6, 12, 20 };

        // ตัวเลือกปัจจุบัน
        public static string Template = "ngd_world_continents";
        public static string Size = "huge";
        public static readonly Dictionary<string, int> Kingdoms = new Dictionary<string, int> { { "human", 2 }, { "elf", 1 }, { "orc", 1 }, { "dwarf", 1 }, { "other", 0 } };
        public static int Pop = 12;
        public static int StartEra = 0;
        public static bool Heroes = true;

        static bool _pending, _generated;
        public static string LastReport = "";

        // ---------------- ลงทะเบียนรูปแบบโลก ----------------
        public static void Register()
        {
            var mc = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            int ok = 0;
            foreach (var (id, from, tweak) in Templates)
            {
                try
                {
                    if (AssetManager.map_gen_templates.has(id)) continue;
                    var src = AssetManager.map_gen_templates.get(from);
                    if (src == null) continue;
                    var t = (MapGenTemplate)mc.Invoke(src, null);
                    t.id = id;
                    t.values = (MapGenValues)mc.Invoke(src.values, null);
                    t.values.add_vegetation = true; t.values.add_resources = true;
                    tweak(t.values);
                    t.show_reset_button = false;
                    AssetManager.map_gen_templates.add(t);
                    ok++;
                }
                catch (Exception e) { NgdMod.LogOnce("world template " + id + ": " + e.Message); }
            }
            NgdMod.Log("world templates registered: " + ok);
        }

        public static List<string> OtherRaces() =>
            AssetManager.actor_library.list.Where(a => a != null && a.civ && !MainRaces.Contains(a.id)).Select(a => a.id).ToList();

        // ---------------- หน้าต่าง ----------------
        static ScrollWindow _win;
        static Transform _root;
        static readonly Dictionary<string, SimpleButton> _pick = new Dictionary<string, SimpleButton>();
        static readonly Dictionary<string, Text> _counts = new Dictionary<string, Text>();
        static Text _summary;

        public static void Open()
        {
            Ensure();
            Refresh();
            ScrollWindow.showWindow(WindowId);
        }

        public static void Ensure()
        {
            if (_win != null) return;
            _win = WindowCreator.CreateEmptyWindow(WindowId, WindowId, "ngd/worldgen");
            var content = _win.transform.Find("Background/Scroll View/Viewport/Content");
            var fit = content.gameObject.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = content.gameObject.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter; vlg.spacing = 3; vlg.padding = new RectOffset(4, 4, 4, 8);
            vlg.childControlHeight = false; vlg.childControlWidth = false; vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = false;
            _root = content;
            Build(_root);
        }

        static void Build(Transform p)
        {
            EditorTabs.Title(p, "ngd_wg_title");
            var intro = EditorTabs.Txt(p, W, 24, 8, TextAnchor.UpperLeft);
            intro.text = LM.Get("ngd_wg_intro");

            EditorTabs.Section(p, "ngd_wg_sec_shape");
            foreach (var (id, _, _) in Templates)
            {
                var r = EditorTabs.Row(p, 22);
                string tid = id;
                _pick["tpl_" + id] = Btn(r, LM.Get("template_" + id), 200, () => Template = tid, "template_" + id + "_info");
            }

            EditorTabs.Section(p, "ngd_wg_sec_size");
            var rs = EditorTabs.Row(p, 22);
            foreach (var s in Sizes)
            {
                string sid = s;
                _pick["size_" + s] = Btn(rs, LM.Get("ngd_wg_size_" + s), 38, () => Size = sid, null);
            }

            EditorTabs.Section(p, "ngd_wg_sec_civ");
            foreach (var race in MainRaces.Concat(new[] { "other" }))
            {
                if (race == "other" && OtherRaces().Count == 0) continue;
                var r = EditorTabs.Row(p, 22);
                string rid = race;
                var lbl = EditorTabs.Txt(r, 96, 20, 9, TextAnchor.MiddleLeft);
                lbl.text = LM.Get("ngd_wg_race_" + race);
                Btn(r, "-", 26, () => Kingdoms[rid] = Math.Max(0, Kingdoms[rid] - 1), null);
                _counts[race] = EditorTabs.Txt(r, 30, 20, 10, TextAnchor.MiddleCenter);
                Btn(r, "+", 26, () => Kingdoms[rid] = Math.Min(6, Kingdoms[rid] + 1), null);
            }
            var rp = EditorTabs.Row(p, 22);
            var pl = EditorTabs.Txt(rp, 80, 20, 9, TextAnchor.MiddleLeft);
            pl.text = LM.Get("ngd_wg_pop");
            foreach (var n in PopOptions) { int nn = n; _pick["pop_" + n] = Btn(rp, n.ToString(), 38, () => Pop = nn, null); }

            EditorTabs.Section(p, "ngd_wg_sec_era");
            var re1 = EditorTabs.Row(p, 22); var re2 = EditorTabs.Row(p, 22);
            for (int e = 0; e < 4; e++) { int ee = e; _pick["era_" + e] = Btn(e < 2 ? re1 : re2, Eras.Name(e), 99, () => StartEra = ee, null); }

            EditorTabs.Section(p, "ngd_wg_sec_heroes");
            var rh = EditorTabs.Row(p, 22);
            _pick["heroes_on"] = Btn(rh, LM.Get("ngd_wg_heroes_on"), 99, () => Heroes = true, "ngd_wg_heroes_on_desc");
            _pick["heroes_off"] = Btn(rh, LM.Get("ngd_wg_heroes_off"), 99, () => Heroes = false, null);

            EditorTabs.Spacer(p, 4);
            _summary = EditorTabs.Txt(p, W, 40, 8, TextAnchor.UpperLeft);
            var rg = EditorTabs.Row(p, 30);
            var go = Btn(rg, LM.Get("ngd_wg_generate"), 200, Generate, "ngd_wg_generate_desc");
            go.Background.color = new Color(1f, 0.8f, 0.35f);
            go.Text.fontSize = 11;
            ((RectTransform)go.transform).sizeDelta = new Vector2(200, 28);
        }

        static SimpleButton Btn(Transform parent, string label, float w, Action act, string tipKey)
        {
            var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, parent);
            b.Setup(() => { try { act(); } catch (Exception e) { NgdMod.LogOnce("worldgen ui: " + e.Message); } Refresh(); }, null, label, new Vector2(w, 20));
            b.name = "btn_wg_" + label;
            b.Text.font = LocalizedTextManager.current_font;
            b.Text.resizeTextForBestFit = false;
            b.Text.fontSize = 8;
            b.Text.alignment = TextAnchor.MiddleCenter;
            b.Text.horizontalOverflow = HorizontalWrapMode.Overflow;
            if (tipKey != null) b.TipButton.SetTextTooltip(() => label, () => LocalizedTextManager.stringExists(tipKey) ? LM.Get(tipKey) : "", true);
            return b;
        }

        static void Refresh()
        {
            var on = new Color(1f, 0.85f, 0.4f);
            foreach (var kv in _pick)
            {
                if (kv.Value == null) continue;
                bool sel = kv.Key == "tpl_" + Template || kv.Key == "size_" + Size || kv.Key == "pop_" + Pop || kv.Key == "era_" + StartEra
                           || (kv.Key == "heroes_on" && Heroes) || (kv.Key == "heroes_off" && !Heroes);
                kv.Value.Background.color = sel ? on : Color.white;
            }
            foreach (var kv in _counts) if (kv.Value != null) kv.Value.text = "<b>" + Kingdoms[kv.Key] + "</b>";
            if (_summary != null)
            {
                int total = Kingdoms.Values.Sum();
                _summary.text = string.Format(LM.Get("ngd_wg_summary"), total, total * Pop) + "\n<color=#ff9a6a>" + LM.Get("ngd_wg_warning") + "</color>";
            }
        }

        // ---------------- สร้างโลก ----------------
        public static void Generate()
        {
            if (Kingdoms.Values.Sum() == 0) { EditorTabs.Toast("✖ " + LM.Get("ngd_wg_need_kingdom")); return; }
            Config.current_map_template = AssetManager.map_gen_templates.has(Template) ? Template : "continent";
            Config.customMapSize = AssetManager.map_sizes.has(Size) ? Size : "huge";
            _pending = true; _generated = false;
            ScrollWindow.hideAllEvent(false);
            World.world.clickGenerateNewMap();
            Wait(0);
        }

        static void Wait(int tries)
        {
            NgdTicker.Later(1f, () =>
            {
                bool loading = false;
                try { loading = SmoothLoader.isLoading(); } catch { }
                if ((!_generated || loading) && tries < 180) { Wait(tries + 1); return; }
                if (!_generated) { _pending = false; NgdMod.LogOnce("worldgen: map never finished"); return; }
                NgdTicker.Later(1.5f, Populate);
            });
        }

        /// <summary>เรียกจาก Harmony เมื่อเกมสร้างแผนที่เสร็จ</summary>
        internal static void OnMapMade() { if (_pending) _generated = true; }

        public static void Populate()
        {
            _pending = false;
            var report = new List<string>();
            try
            {
                Eras.YearOffset = Eras.StartYear[Mathf.Clamp(StartEra, 0, Eras.Ids.Length - 1)] - (Eras.WorldYear() - Eras.YearOffset);
                var pool = Candidates();
                var founded = new List<WorldTile>();
                var others = OtherRaces();
                int made = 0;
                foreach (var kv in Kingdoms)
                    for (int k = 0; k < kv.Value; k++)
                    {
                        string race = kv.Key == "other" ? (others.Count > 0 ? others[UnityEngine.Random.Range(0, others.Count)] : "human") : kv.Key;
                        var at = Found(race, pool, founded);
                        if (at != null) { founded.Add(at); made++; }
                    }
                var spots = pool;
                report.Add(string.Format(LM.Get("ngd_wg_done"), made, Eras.Name(StartEra)));
                LastReport = "kingdoms=" + made + " spots=" + spots.Count + " era=" + StartEra;
                NgdMod.Log("worldgen: " + LastReport);
            }
            catch (Exception e) { NgdMod.LogOnce("worldgen populate: " + e); LastReport = "error " + e.Message; }
            if (report.Count > 0) { EditorTabs.Toast(report[0]); GuildSystem.Announce(report[0]); }
        }

        static bool Good(WorldTile t)
        {
            var ty = t?.cur_tile_type;
            return ty != null && ty.ground && ty.can_build_on && !ty.liquid && !ty.mountains && !ty.rocks && !ty.lava && !ty.block;
        }

        /// <summary>จุดที่ตั้งเมืองได้: พื้นดินสร้างได้ และรอบ ๆ เป็นดินเกือบทั้งหมด</summary>
        public static List<WorldTile> Candidates()
        {
            var tiles = World.world.tiles_list;
            var cand = new List<WorldTile>();
            for (int i = 0; i < 12000 && cand.Count < 2500; i++)
            {
                var t = tiles[UnityEngine.Random.Range(0, tiles.Length)];
                if (Good(t) && Neighbours(t, 3).Count(Good) >= 30) cand.Add(t);
            }
            return cand;
        }

        static float MinDist2(WorldTile c, List<WorldTile> founded)
        {
            float d = float.MaxValue;
            foreach (var p in founded) d = Mathf.Min(d, (c.x - p.x) * (c.x - p.x) + (c.y - p.y) * (c.y - p.y));
            return d;
        }

        static IEnumerable<WorldTile> Neighbours(WorldTile t, int r)
        {
            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    var n = World.world.GetTile(t.x + dx, t.y + dy);
                    if (n != null) yield return n;
                }
        }

        /// <summary>
        /// วางเผ่า 1 กลุ่ม: หาจุดที่เผ่านี้ตั้งเมืองได้จริง (บางเผ่าอยู่ได้เฉพาะบางไบโอม)
        /// โดยเลือกจุดที่ไกลจากอาณาจักรอื่นที่สุดก่อน แล้วให้หัวหน้าตั้งอาณาจักรตามเส้นทางของเกม
        /// </summary>
        public static WorldTile Found(string race, List<WorldTile> pool, List<WorldTile> founded)
        {
            if (AssetManager.actor_library.get(race) == null || pool.Count == 0) return null;
            Actor leader = null;
            try { leader = World.world.units.spawnNewUnit(race, pool[0], false, false, 0f, null, true, true); }
            catch (Exception e) { NgdMod.LogOnce("worldgen spawn " + race + ": " + e.Message); }
            if (leader == null) return null;
            WorldTile at = null;
            var ordered = founded.Count == 0 ? pool.OrderBy(_ => UnityEngine.Random.value) : pool.OrderByDescending(t => MinDist2(t, founded));
            int tried = 0;
            foreach (var t in ordered)
            {
                if (++tried > 400) break;
                if (founded.Count > 0 && MinDist2(t, founded) < 400) break;   // ห่างกันอย่างน้อย 20 ช่อง
                try
                {
                    if (t.zone == null || t.zone.hasCity() || !t.zone.isGoodForNewCity(leader)) continue;
                    leader.setCurrentTilePosition(t);
                    if (World.world.cities.canStartNewCityCivilizationHere(leader)) { at = t; break; }
                }
                catch { }
            }
            if (at == null) { NgdMod.Log("worldgen: no settle spot for " + race); try { leader.die(false, AttackType.Divine, false, false); } catch { } return null; }
            var near = Neighbours(at, 3).Where(Good).ToList();
            var people = new List<Actor> { leader };
            for (int i = 1; i < Pop; i++)
            {
                var t = near.Count == 0 ? at : near[UnityEngine.Random.Range(0, near.Count)];
                try
                {
                    var a = World.world.units.spawnNewUnit(race, t, false, false, 0f, null, true, true);
                    if (a != null) people.Add(a);
                }
                catch (Exception e) { NgdMod.LogOnce("worldgen spawn " + race + ": " + e.Message); }
            }
            // ใช้เส้นทางเดียวกับเกม: สร้างอาณาจักร + เมืองแรก + วัฒนธรรม/ภาษา/ตระกูล
            City city = null;
            foreach (var lead in people)
            {
                try { if (lead.isAlive() && lead.buildCityAndStartCivilization()) { city = lead.getCity(); break; } }
                catch (Exception e) { NgdMod.LogOnce("worldgen city " + race + ": " + e.Message); }
            }
            if (city != null)
                foreach (var a in people) if (a.isAlive() && a.getCity() == null) try { a.joinCity(city); } catch { }
            if (Heroes) MakeHeroes(people);
            return city != null ? at : null;
        }

        /// <summary>ฮีโร่เริ่มต้น: หัวหน้าแรงค์ B · ลูกทีม 2 คนแรงค์ D-C (สมดุล: แรงค์ S ต้องอายุ 100+)</summary>
        static void MakeHeroes(List<Actor> people)
        {
            for (int i = 0; i < people.Count && i < 3; i++)
            {
                var a = people[i];
                if (a == null || !a.isAlive()) continue;
                try
                {
                    Progression.Awaken(a, true);
                    NgdData.SetRank(a, i == 0 ? 3 : UnityEngine.Random.Range(1, 3));
                    NgdData.SetStar(a, UnityEngine.Random.Range(1, 6));
                    Gear.OnRankUp(a, NgdData.GetRank(a));
                    a.setStatsDirty();
                }
                catch (Exception e) { NgdMod.LogOnce("worldgen hero: " + e.Message); }
            }
        }
    }

    [HarmonyPatch(typeof(MapBox), "finishMakingWorld")]
    public static class Patch_WorldGenDone
    {
        public static void Postfix() { WorldGen.OnMapMade(); }
    }
}
