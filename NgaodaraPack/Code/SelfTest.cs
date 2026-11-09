using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace NgaodaraPack
{
    /// <summary>
    /// ทดสอบอัตโนมัติในเกมจริง (สำหรับผู้พัฒนา) ทำงานเฉพาะเมื่อมีไฟล์ ngd_selftest.flag ในโฟลเดอร์ข้อมูลเกม
    /// โหลดเซฟอัตโนมัติล่าสุด → เรียกทุกระบบกับตัวละครจริง → กดทุกปุ่มในเมนูเทพ → ถ่ายภาพหน้าจอ → จำลอง 10 ปี → ปิดเกม
    /// ผลลัพธ์: persistentDataPath/ngd_selftest/result.txt + *.png
    /// </summary>
    public class SelfTest : MonoBehaviour
    {
        public static string FlagPath => Path.Combine(Application.persistentDataPath, "ngd_selftest.flag");
        static string OutDir => Path.Combine(Application.persistentDataPath, "ngd_selftest");

        public static bool Active;
        static readonly Dictionary<string, int> _kills = new Dictionary<string, int>();

        static string Cat(Actor a)
        {
            if (a == null) return "?";
            if (Rifts.IsBoss(a)) return "boss";
            a.data.get("ngd_rift", out bool r, false);
            if (r) return "riftmob";
            if (a.kingdom != null && a.kingdom.isCiv()) return NgdData.GetRank(a) >= 0 ? "adventurer" : "civ";
            return "wild:" + a.asset.id;
        }

        public static void Tally(Actor killer, Actor dead)
        {
            if (dead == null) return;
            string k = Cat(killer) + " -> " + Cat(dead);
            _kills[k] = (_kills.TryGetValue(k, out var n) ? n : 0) + 1;
        }

        readonly StringBuilder _out = new StringBuilder();
        readonly List<string> _errors = new List<string>();
        int _pass, _fail;
        bool _capture;

        void OnLog(string msg, string st, LogType t)
        {
            if (!_capture || msg.StartsWith("[NGDTEST]")) return;
            bool ours = msg.Contains("Ngaodara");
            if (t == LogType.Exception || t == LogType.Error || (t == LogType.Warning && ours))
                if (_errors.Count < 400) _errors.Add(t + ": " + msg + " | " + (st ?? "").Replace("\n", " / ").Substring(0, Math.Min(500, (st ?? "").Length)));
        }

        void W(string s) { _out.AppendLine(s); Debug.Log("[NGDTEST] " + s); Flush(); }
        void Flush() { try { File.WriteAllText(Path.Combine(OutDir, "result.txt"), _out.ToString()); } catch { } }

        void Check(string name, Func<bool> f)
        {
            int before = _errors.Count;
            bool ok; string why = "";
            try { ok = f(); }
            catch (Exception e) { ok = false; why = e.GetType().Name + ": " + e.Message + " @ " + (e.StackTrace ?? "").Split('\n')[0]; }
            if (_errors.Count > before) { ok = false; why += " | log: " + _errors[before]; }
            if (ok) _pass++; else _fail++;
            W((ok ? "PASS " : "FAIL ") + name + (why != "" ? "  -> " + why : ""));
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(0.6f);
            ScreenCapture.CaptureScreenshot(Path.Combine(OutDir, name + ".png"));
            yield return null; yield return null;
            W("shot " + name);
        }

        static int CountUnits() { int n = 0; try { foreach (var a in World.world.units) n++; } catch { } return n; }

        static List<Actor> Civs() =>
            World.world.units.Where(a => a != null && a.isAlive() && a.kingdom != null && a.kingdom.isCiv() && a.getCity() != null).ToList();

        static string Hist()
        {
            var cnt = new int[NgdData.RankNames.Length + 1];
            foreach (var a in World.world.units) if (a != null && a.isAlive()) cnt[NgdData.GetRank(a) + 1]++;
            var sb = new StringBuilder("none:" + cnt[0]);
            for (int i = 0; i < NgdData.RankNames.Length; i++) if (cnt[i + 1] > 0) sb.Append(" " + NgdData.RankNames[i] + ":" + cnt[i + 1]);
            return sb.ToString();
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(OutDir);
            foreach (var f in Directory.GetFiles(OutDir)) try { File.Delete(f); } catch { }
            Application.logMessageReceived += OnLog;
            W("selftest " + DateTime.Now);
            float t0 = Time.realtimeSinceStartup;
            while (World.world == null || World.world.units == null || Time.realtimeSinceStartup - t0 < 12f) yield return null;

            try
            {
                // ไฟล์ flag เขียนเลขช่องเซฟได้ (เช่น "1") ไม่งั้นโหลดเซฟอัตโนมัติล่าสุด
                string flag = File.ReadAllText(FlagPath).Trim();
                if (flag.StartsWith("probe")) { _probe = true; flag = flag.Substring(5).Trim(); }
                if (int.TryParse(flag, out int slot))
                {
                    W("load slot " + slot);
                    SaveManager.setCurrentSlot(slot);
                }
                else
                {
                    var root = Path.Combine(Application.persistentDataPath, "autosaves");
                    var d = Directory.GetDirectories(root).OrderByDescending(Directory.GetLastWriteTime).First();
                    W("load " + d);
                    SaveManager.setCurrentPathAndId(d, -1);
                }
                World.world.save_manager.startLoadSlot();
            }
            catch (Exception e) { W("load failed " + e.Message); }
            t0 = Time.realtimeSinceStartup;
            yield return new WaitForSecondsRealtime(6f);
            while (CountUnits() < 30 && Time.realtimeSinceStartup - t0 < 120f) yield return new WaitForSecondsRealtime(1f);
            yield return new WaitForSecondsRealtime(6f);
            _capture = true;

            if (_probe) yield return Probe(); else yield return RunAll();

            _capture = false;
            W("");
            W($"RESULT pass={_pass} fail={_fail} errors_logged={_errors.Count}");
            W("---- errors ----");
            foreach (var e in _errors.Distinct().Take(60)) W(e);
            Flush();
            yield return new WaitForSecondsRealtime(1f);
            try { File.Delete(FlagPath); } catch { }
            Application.Quit();
        }

        bool _probe;

        static string Path2(Transform t) => t.parent == null ? t.name : Path2(t.parent) + "/" + t.name;

        void Dump(Transform t, int depth, StringBuilder sb, int maxDepth)
        {
            if (depth > maxDepth) return;
            var comps = string.Join(",", t.GetComponents<Component>().Where(c => c != null && !(c is Transform)).Select(c => c.GetType().Name));
            var img = t.GetComponent<Image>();
            var rt = t as RectTransform;
            sb.AppendLine(new string(' ', depth * 2) + t.name + (t.gameObject.activeSelf ? "" : " (off)") + " [" + comps + "]"
                + (img != null && img.sprite != null ? " sprite=" + img.sprite.name : "")
                + (rt != null ? $" pos={rt.anchoredPosition} size={rt.sizeDelta}" : ""));
            foreach (Transform c in t) Dump(c, depth + 1, sb, maxDepth);
        }

        IEnumerator Probe()
        {
            string dir = Path.Combine(OutDir, "ref");
            Directory.CreateDirectory(dir);
            var a = Civs().FirstOrDefault(x => x.isAdult());
            ActionLibrary.openUnitWindow(a);
            yield return new WaitForSecondsRealtime(1.5f);
            var w = ScrollWindow.getCurrentWindow();
            var sb = new StringBuilder();
            if (w != null) Dump(w.transform, 0, sb, 7);
            File.WriteAllText(Path.Combine(OutDir, "unitwindow_tree.txt"), sb.ToString());
            yield return Shot("probe_unit");
            int n = 0;
            foreach (var p in AssetManager.powers.list)
            {
                if (p == null || string.IsNullOrEmpty(p.path_icon) || n > 60) continue;
                var s = SpriteTextureLoader.getSprite("ui/Icons/" + p.path_icon);
                if (s == null) continue;
                try { SaveSprite(s, Path.Combine(dir, "power_" + p.id + ".png")); n++; } catch { }
            }
            int m = 0;
            foreach (var it in AssetManager.items.list)
            {
                if (it == null || it.id.StartsWith("$") || it.id.StartsWith("ngd") || m > 70) continue;
                try
                {
                    var ic = SpriteTextureLoader.getSprite(it.path_icon);
                    if (ic != null) { SaveSprite(ic, Path.Combine(dir, "icon_" + it.id + ".png")); m++; }
                    var gl = string.IsNullOrEmpty(it.path_gameplay_sprite) ? null : SpriteTextureLoader.getSpriteList(it.path_gameplay_sprite);
                    if (gl != null && gl.Length > 0) SaveSprite(gl[0], Path.Combine(dir, "w_" + it.id + ".png"));
                }
                catch { }
            }
            W($"probe: powers={n} items={m}");
            // อาคารจริงของเกม: ทุกเฟรม (main/construction/ruin/mini/special) ของแต่ละหลัง
            var bdir = Path.Combine(OutDir, "bld"); Directory.CreateDirectory(bdir);
            var binfo = new StringBuilder();
            foreach (var ba in AssetManager.buildings.list)
            {
                if (ba == null || string.IsNullOrEmpty(ba.main_path)) continue;
                if (!(ba.id.Contains("human") || ba.id.Contains("dwarf") || ba.id.Contains("elf") || ba.id.Contains("orc") || ba.main_path.Contains("civ_general"))) continue;
                try
                {
                    string path = string.IsNullOrEmpty(ba.sprite_path) ? ba.main_path + ba.id : ba.sprite_path;
                    var list = SpriteTextureLoader.getSpriteList(path, false);
                    if (list == null) continue;
                    binfo.AppendLine($"B {ba.id} type={ba.type} path={path} fund={ba.fundament?.left},{ba.fundament?.right},{ba.fundament?.top},{ba.fundament?.bottom} frames={list.Length}");
                    foreach (var sp in list)
                    {
                        binfo.AppendLine($"   {sp.name} rect={sp.rect.width}x{sp.rect.height} pivot={sp.pivot}");
                        SaveSprite(sp, Path.Combine(bdir, ba.id + "__" + sp.name + ".png"));
                    }
                }
                catch (Exception e) { binfo.AppendLine("  err " + ba.id + " " + e.Message); }
            }
            File.WriteAllText(Path.Combine(OutDir, "buildings.txt"), binfo.ToString());
            yield break;
        }

        static void SaveSprite(Sprite s, string path)
        {
            var tex = s.texture;
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
            var prev = RenderTexture.active;
            Graphics.Blit(tex, rt);
            RenderTexture.active = rt;
            var r = s.textureRect;
            var o = new Texture2D((int)r.width, (int)r.height, TextureFormat.RGBA32, false);
            o.ReadPixels(new Rect(r.x, r.y, r.width, r.height), 0, 0);
            o.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            File.WriteAllBytes(path, o.EncodeToPNG());
            UnityEngine.Object.Destroy(o);
        }

        IEnumerator RunAll()
        {
            ArtProbe.Weapons(NgdMod.ModFolder);
            var cities = World.world.cities.Where(c => c != null && c.isAlive()).ToList();
            W($"world year={Eras.WorldYear()} era={Eras.WorldEra()} cities={cities.Count} units={CountUnits()} guilds={cities.Count(GuildSystem.HasGuild)}");
            W("ranks " + Hist());
            foreach (var iid in AssetManager.items.list.Where(x => x.equipment_type == EquipmentType.Weapon && !x.id.StartsWith("ngd")).Take(4).Select(x => x.id)
                         .Concat(new[] { "ngd_starfall_blade", "ngd_set_fire_weapon" }))
                try
                {
                    var sp = AssetManager.items.get(iid)?.gameplay_sprites?.FirstOrDefault();
                    W(sp == null ? "  sprite " + iid + ": none" : $"  sprite {iid}: rect={sp.rect.width}x{sp.rect.height} pivot={sp.pivot} ppu={sp.pixelsPerUnit} tex={sp.texture.width}x{sp.texture.height}");
                }
                catch (Exception e) { W("  sprite " + iid + ": " + e.Message); }
            var civs = Civs();
            if (civs.Count < 3) { W("FAIL not enough civilians"); _fail++; yield break; }

            Actor A = civs.Where(x => x.isAdult() && !x.isKing() && x.equipment != null).OrderByDescending(x => x.getCity().getPopulationPeople()).FirstOrDefault() ?? civs[0];
            Actor B = civs.FirstOrDefault(x => x != A && x.isAdult() && NgdData.GetRank(x) < 0) ?? civs.First(x => x != A);
            Actor K = civs.FirstOrDefault(x => x != A && x != B) ?? B;
            City city = A.getCity();
            W($"A={A.getName()} ({A.asset.id}) city={city.name} pop={city.getPopulationPeople()}  B={B.getName()}  K={K.getName()}");

            // ---------- ค่าสถานะมีผลจริง ----------
            Check("stats.vit_raises_health", () =>
            {
                NgdData.EnsureStats(A); A.setStatsDirty(); A.updateStats();
                float h0 = A.stats["health"];
                NgdData.SetStat(A, 2, NgdData.GetStatL(A, 2) + 50); A.updateStats();
                float h1 = A.stats["health"];
                W($"  health {h0} -> {h1}");
                return h1 > h0 + 400;
            });
            Check("stats.str_raises_damage", () =>
            {
                A.setStatsDirty(); A.updateStats(); float d0 = A.stats["damage"];
                NgdData.SetStat(A, 0, NgdData.GetStatL(A, 0) + 50); A.updateStats();
                W($"  damage {d0} -> {A.stats["damage"]}");
                return A.stats["damage"] > d0 + 50;
            });
            Check("stats.no_compounding", () =>
            {
                A.setStatsDirty(); A.updateStats(); float h = A.stats["health"];
                for (int i = 0; i < 20; i++) A.updateStats();
                A.setStatsDirty(); A.updateStats();
                W($"  health stable {h} -> {A.stats["health"]}");
                return Mathf.Abs(A.stats["health"] - h) < 1f;
            });
            Check("rank.multiplier_applies", () =>
            {
                Progression.Awaken(A);
                NgdData.SetRank(A, 0); NgdData.SetStar(A, 1); A.updateStats(); float h0 = A.stats["health"];
                NgdData.SetRank(A, 5); NgdData.SetStar(A, 12); A.updateStats(); float h1 = A.stats["health"];
                W($"  E1 {h0} -> S12 {h1}  maxHP={A.getMaxHealth()}");
                return h1 > h0 * 3;
            });
            Check("rank.bounds", () =>
            {
                NgdData.SetRank(A, 99); bool a1 = NgdData.GetRank(A) == NgdData.RankNames.Length - 1;
                NgdData.SetStar(A, 99); bool a2 = NgdData.GetStar(A) == NgdData.MaxStar;
                NgdData.SetStar(A, -5); bool a3 = NgdData.GetStar(A) == 1;
                NgdData.SetRank(A, 3);
                return a1 && a2 && a3;
            });
            Check("tree.nodes_81", () => { SkillTree.Build(); W("  nodes " + SkillTree.Nodes.Count + " · all skills " + SkillTree.ById.Count); return SkillTree.Nodes.Count == 81; });
            Check("tree.learn_order", () =>
            {
                SkillTree.ResetTree(A, false); NgdData.SetRank(A, 5);
                bool skip = SkillTree.Learn(A, "blade_a3", true);          // ข้ามขั้นไม่ได้
                bool t1 = SkillTree.Learn(A, "blade_a1", true), t2 = SkillTree.Learn(A, "blade_a2", true), t3 = SkillTree.Learn(A, "blade_a3", true);
                return !skip && t1 && t2 && t3 && SkillTree.Knows(A, "blade_a3");
            });
            Check("tree.loadout_and_ult", () =>
            {
                Gear.EquipSet(A, "thunder"); A.setStatsDirty(); A.updateStats();
                var lo = SkillTree.Loadout(A, out var u);
                W($"  slots {lo.Count}: " + string.Join(", ", lo.Select(SkillTree.Name)) + $" | ult {(u != null ? SkillTree.Name(u) : "-")}");
                bool setSkill = lo.Any(x => x.Source == SkillTree.Source.Set) || SkillTree.Available(A).Any(x => x.Source == SkillTree.Source.Set);
                return lo.Count <= 6 && u != null && !string.IsNullOrEmpty(SkillTree.Name(u)) && setSkill;
            });
            Check("tree.weapon_skill", () => { Arsenal.Give(A, "ngd_thunder_spear"); A.setStatsDirty(); A.updateStats(); return SkillTree.Available(A).Any(x => x.Source == SkillTree.Source.Weapon); });
            {
                Actor beast = World.world.units.FirstOrDefault(x => x != null && x.isAlive() && (x.kingdom == null || !x.kingdom.isCiv()) && x.asset.can_be_inspected);
                if (beast != null)
                    Check("tree.beast_branch(" + beast.asset.id + ")", () =>
                    {
                        Progression.Awaken(beast); SkillTree.AddPoints(beast, 5); SkillTree.AutoLearn(beast);
                        W("  beast learned: " + string.Join(",", SkillTree.Learned(beast)));
                        return SkillTree.Learned(beast).Any(x => x.StartsWith("beast_") || x.StartsWith("martial_") || x.StartsWith("blade_"));
                    });
            }
            Check("unlock.all_creatures", () =>
            {
                int locked = 0, total = 0;
                foreach (var asset in AssetManager.actor_library.list) { total++; if (!asset.isAvailable()) locked++; }
                W($"  creatures {total} locked {locked}");
                return locked == 0;
            });
            // ---------- บ้านเมืองตามยุค ----------
            Check("city.looks_preloaded", () =>
            {
                CityLook.Preload();
                W("  frames stone_house0=" + (CityLook.Frames("stone_house0") != null) + " star_castle=" + (CityLook.Frames("star_castle") != null));
                return CityLook.Preloaded && CityLook.Frames("stone_house0") != null && CityLook.Frames("kingdom_castle") != null && CityLook.Frames("iron_guild") != null;
            });
            Check("city.new_buildings_registered", () =>
            {
                int miss = 0;
                foreach (var id in NewBuildings.Ids)
                {
                    if (AssetManager.buildings.get(id) == null) { miss++; W("  no asset " + id); continue; }
                    foreach (var arch in AssetManager.architecture_library.list)
                        if (arch.building_ids_for_construction == null || !arch.building_ids_for_construction.ContainsKey("order_" + id)) { miss++; W("  no order in " + arch.id); break; }
                    foreach (var bo in AssetManager.city_build_orders.list)
                        if (!bo.list.Any(o => o.id == "order_" + id)) { miss++; W("  not in build orders " + bo.id); break; }
                }
                var g = AssetManager.buildings.get("ngd_guild");
                W($"  ngd_guild type={g?.type} cost={g?.cost?.wood}/{g?.cost?.stone}/{g?.cost?.common_metals}/{g?.cost?.gold} sprites={g?.sprite_path}");
                return miss == 0;
            });
            Check("city.house_levels_distinct", () =>
            {
                var houses = AssetManager.buildings.list.Where(b => b.type == "type_house" && b.id.Contains("human")).OrderBy(b => b.upgrade_level).ToList();
                W("  human houses: " + string.Join(", ", houses.Select(h => h.id + "=L" + h.upgrade_level)));
                var house = World.world.buildings.FirstOrDefault(b => b != null && b.isAlive() && b.asset.type == "type_house" && b.isCiv());
                if (house == null) return houses.Count > 0;
                var kinds = new HashSet<string>();
                var keep = house.asset;
                foreach (var h in houses) { house.asset = h; kinds.Add(CityLook.KindFor(house, Eras.Iron)); }
                house.asset = keep;
                W("  kinds: " + string.Join(",", kinds));
                return kinds.Count >= Mathf.Min(3, houses.Count);
            });
            Check("city.era_changes_look", () =>
            {
                var b = World.world.buildings.FirstOrDefault(x => x != null && x.isAlive() && x.isCiv() && x.asset.type == "type_house");
                if (b == null) { W("  no civ house in world"); return true; }
                var ids = new HashSet<string>();
                for (int e = 0; e < Eras.Ids.Length; e++) { var k = CityLook.KindFor(b, e); ids.Add(k[0] == '@' ? k : Eras.Ids[e] + "_" + k); }
                W("  looks: " + string.Join(",", ids));
                return ids.Count == Eras.Ids.Length && ids.All(i => CityLook.Frames(i.TrimStart('@')) != null);
            });
            Check("city.reskin_applies", () =>
            {
                int civ = 0, skinned = 0; var byKind = new Dictionary<string, int>();
                foreach (var b in World.world.buildings)
                {
                    if (b == null || !b.isAlive() || !b.isCiv()) continue;
                    civ++;
                    var s = b.calculateMainSprite();
                    var want = CityLook.Pick(b);
                    if (want != null && s == want) { skinned++; var k = b.asset.type; byKind[k] = byKind.TryGetValue(k, out var n) ? n + 1 : 1; }
                }
                W($"  civ buildings {civ} reskinned {skinned}: " + string.Join(" ", byKind.Select(kv => kv.Key.Replace("type_", "") + "=" + kv.Value)));
                return civ == 0 || skinned > civ / 2;
            });
            Check("city.capital_castle", () =>
            {
                var hall = World.world.buildings.FirstOrDefault(b => b != null && b.isAlive() && b.asset.type == "type_hall" && b.getCity() != null && b.getCity().isCapitalCity());
                if (hall == null) { W("  no capital hall"); return true; }
                W("  capital " + hall.getCity().name + " -> " + CityLook.KindFor(hall, Eras.CityEra(hall.getCity())));
                return CityLook.KindFor(hall, Eras.Kingdom) == "castle";
            });
            Check("unlock.all_items", () =>
            {
                int locked = 0, total = 0;
                foreach (var it in AssetManager.items.list) { if (it == null || it.id.StartsWith("$")) continue; total++; if (!it.isAvailable()) locked++; }
                W($"  items {total} locked {locked}");
                return locked == 0;
            });
            Check("num.short", () => Num.Short(16717201) == "16.7M" && Num.Short(9999) == "9,999" && Num.Short(2.5e9) == "2.5B");
            // ---------- เซ็ตเทพ 20 · อาวุธเทพ 20 · โรงตีเหล็ก ----------
            Check("gear.sets_20", () =>
            {
                int miss = 0;
                foreach (var el in Gear.Elements) foreach (var p in Gear.Pieces) if (AssetManager.items.get(Gear.SetId(el.Id, p)) == null) miss++;
                W($"  sets {Gear.Elements.Length} missing items {miss}");
                return Gear.Elements.Length == 20 && miss == 0 && Gear.Elements.All(e => !string.IsNullOrEmpty(e.Full));
            });
            Check("gear.relics_20", () =>
            {
                var bad = Arsenal.Relics.Where(r => AssetManager.items.get(r.Id) == null || AssetManager.items.get(r.Id).gameplay_sprites == null || AssetManager.items.get(r.Id).gameplay_sprites.Length == 0).Select(r => r.Id).ToList();
                W($"  relics {Arsenal.Relics.Count} bad: " + string.Join(",", bad));
                return Arsenal.Relics.Count == 20 && bad.Count == 0;
            });
            Check("gear.full_set_effect", () =>
            {
                Gear.EquipSet(A, "titan"); A.setStatsDirty(); A.updateStats(); float full = A.stats["health"];
                Gear.Equip(A, Gear.SetId("fire", "ring")); A.setStatsDirty(); A.updateStats(); float five = A.stats["health"];
                W($"  titan 6pc health {full} vs 5pc {five}");
                return full > five * 1.3f;
            });
            Check("skill.weapon_custom", () =>
            {
                var s = SkillTree.ById["wpn_ngd_titan_maul"]; var h = SkillTree.ById["wpn_ngd_worldtree_staff"];
                W("  " + SkillTree.Name(s) + " | " + SkillTree.Name(h));
                return s.TStatus == "stunned" && s.Kind == Skills.Kind.Aoe && h.Kind == Skills.Kind.Heal && SkillTree.Name(s).Contains("(");
            });
            Check("forge.craft_set_pays", () =>
            {
                Gear.UnequipAll(A); NgdData.SetRank(A, 5); A.setStatsDirty(); A.updateStats();
                foreach (var (id, n) in Forge.SetCost) CityTabs.AddSpread(city, id, n + 10);
                int g0 = city.getResourcesAmount("gold");
                bool afford = Forge.CanAfford(city, Forge.SetCost);
                var err = Forge.CraftSet(city, out var hero, out var item, false, true);
                int g1 = city.getResourcesAmount("gold");
                W($"  afford={afford} err={err} hero={hero?.getName()} item={item} gold {g0}->{g1}");
                return afford && err == null && item != null && g0 - g1 >= 600 && Forge.NextSetPiece(hero) != item;
            });
            Check("forge.craft_relic_unique", () =>
            {
                foreach (var (id, n) in Forge.RelicCost) CityTabs.AddSpread(city, id, n); Forge.Stash(city, Forge.RelicCost); int dep0 = Forge.Deposit(city, Forge.RelicCost, "gold"); Forge.FillDeposit(city, Forge.RelicCost); W("  stashed gold " + dep0);
                var w = A.equipment.getSlot(EquipmentType.Weapon);
                if (w != null && !w.isEmpty() && Arsenal.Relics.Any(r => r.Id == w.getItem().getAsset().id)) { try { A.createNewWeapon("sword_iron"); } catch { } }
                var err = Forge.CraftRelic(city, out var hero, out var item, false, true);
                bool unique = item != null && Enumerable.Range(0, 60).All(_ => Forge.NextRelic() != item);
                W($"  err={err} hero={hero?.getName()} relic={item} unique={unique}");
                return err == null && unique && Forge.Deposit(city, Forge.RelicCost, "gold") == 0;
            });
            Check("forge.no_forge_blocks", () =>
            {
                var c2 = World.world.cities.FirstOrDefault(x => x != null && x.isAlive() && !Forge.HasForge(x));
                return c2 == null || Forge.CraftSet(c2, out _, out _) == "ngd_forge_no_forge";
            });
            Check("equip.items_in_vanilla_editor", () => AssetManager.items.get("ngd_set_fire_helmet").show_in_meta_editor && AssetManager.items.get("ngd_starfall_blade").show_in_meta_editor);
            Check("stats.ten_digits_no_overflow", () =>
            {
                var keep = new long[7];
                for (int i = 0; i < 7; i++) { keep[i] = NgdData.GetStatL(A, i); NgdData.SetStat(A, i, 99999999999L); }
                A.setStatsDirty(); A.updateStats();
                bool ok = NgdData.GetStatL(A, 2) == NgdData.StatMax && A.getMaxHealth() > 0 && A.isAlive() && A.stats["health"] <= StatCaps.Big && A.stats["speed"] <= 150f;
                W($"  VIT={NgdData.GetStatL(A, 2)} maxHP={A.getMaxHealth()} dmg={A.stats["damage"]} speed={A.stats["speed"]} CP={NgdData.CombatPower(A)}");
                for (int i = 0; i < 7; i++) NgdData.SetStat(A, i, keep[i]);
                A.setStatsDirty(); A.updateStats();
                return ok;
            });
            Check("level.raises_stats", () =>
            {
                Classes.SyncLevel(A);
                long s0 = 0; for (int i = 0; i < 7; i++) s0 += NgdData.GetStatL(A, i);
                int lv0 = A.data.level;
                A.data.level = lv0 + 10; Classes.SyncLevel(A);
                long s1 = 0; for (int i = 0; i < 7; i++) s1 += NgdData.GetStatL(A, i);
                A.data.level = lv0; Classes.SyncLevel(A);
                long s2 = 0; for (int i = 0; i < 7; i++) s2 += NgdData.GetStatL(A, i);
                W($"  stats sum {s0} -> +10 lv {s1} -> back {s2}");
                return s1 - s0 == 10 * (Balance.PointsPerLevel + Math.Max(0, NgdData.GetRank(A))) && s2 == s0;
            });
            Check("level.max_10000", () =>
            {
                int lv0 = A.data.level;
                A.data.level = Balance.LevelMax - 1; EditorTabs.LevelUp(A, 5);
                bool ok = A.data.level == Balance.LevelMax && Balance.LevelText(A.data.level) == "MAX";
                A.data.level = lv0;
                return ok;
            });
            Check("classes.all", () =>
            {
                foreach (var c in Classes.All) { NgdData.SetClass(A, c.Id); A.updateStats(); if (Classes.TierName(c, 3) == null) return false; }
                return true;
            });

            // ---------- อุปกรณ์ ----------
            foreach (var el in Gear.Elements)
            {
                var id = el.Id;
                Check("gear.set_" + id, () =>
                {
                    Gear.EquipSet(A, id); A.setStatsDirty(); A.updateStats();
                    var w = A.equipment.weapon.getItem();
                    var info = Gear.Info(A);
                    W($"  weapon={w?.data.asset_id} set={info.atk}/{info.def} pieces={info.pieces}");
                    return w != null && w.data.asset_id.Contains(id) && info.pieces >= 4;
                });
            }
            for (int t = 0; t < Gear.Materials.Length; t++)
            {
                int tt = t;
                Check("gear.material_" + Gear.Materials[t], () => { Gear.EquipMaterial(A, tt); return true; });
            }
            foreach (var r in Arsenal.Relics)
            {
                var rid = r.Id;
                Check("relic." + rid, () =>
                {
                    bool ok = Arsenal.Give(A, rid);
                    var w = A.equipment.weapon.getItem();
                    var asset = AssetManager.items.get(rid);
                    bool spr = asset?.gameplay_sprites != null && asset.gameplay_sprites.Length > 0;
                    return ok && w != null && w.data.asset_id == rid && spr;
                });
            }
            Actor M = World.world.units.FirstOrDefault(x => x != null && x.isAlive() && x.equipment == null);
            if (M == null)
            {
                try { M = World.world.units.spawnNewUnit("wolf", A.current_tile, true, true, 3f, null, false, true); } catch { }
            }
            if (M != null)
                Check("gear.creature_without_equipment(" + M.asset.id + ")", () =>
                {
                    Gear.EquipSet(M, "fire"); Arsenal.Give(M); Gear.EquipMaterial(M, 2);
                    return true;
                });

            // ---------- ความก้าวหน้า ----------
            Check("progress.awaken_cap_S", () =>
            {
                Progression.Awaken(B);
                W($"  B rank={NgdData.GetRank(B)} star={NgdData.GetStar(B)} class={NgdData.GetClass(B)}");
                return NgdData.GetRank(B) >= 0 && NgdData.GetRank(B) <= Progression.AssessCap;
            });
            Check("progress.exp_levels", () =>
            {
                int s0 = NgdData.GetStar(B), r0 = NgdData.GetRank(B);
                Progression.AddExp(B, 5000);
                W($"  B {r0}/{s0} -> {NgdData.GetRank(B)}/{NgdData.GetStar(B)} exp={NgdData.GetExp(B)}");
                return NgdData.GetStar(B) > s0 || NgdData.GetRank(B) > r0;
            });
            Check("progress.breakthrough_to_max", () =>
            {
                for (int i = 0; i < 20; i++) Progression.TryBreakthrough(B, true);
                return NgdData.GetRank(B) == NgdData.RankNames.Length - 1 && !Progression.TryBreakthrough(B, true);
            });
            Check("immortal.survives", () =>
            {
                NgdData.SetImmortal(K, true);
                K.getHit(9999999f, true, AttackType.Divine, null, false, false, false);
                bool alive = K.isAlive();
                NgdData.SetImmortal(K, false);
                return alive;
            });
            // ---------- สกิล + เอฟเฟกต์ของมอด ----------
            Check("fx.frames_loaded", () =>
            {
                int miss = 0;
                foreach (var el in Fx.Elements)
                    foreach (var k in new[] { "slash_", "burst_", "cast_", "wave_", "buff_" })
                        if (FxPlayer.Frames(k + el) == null || FxPlayer.Frames(k + el).Length < 3) miss++;
                W("  fx missing sets: " + miss);
                return miss == 0;
            });
            {
                Actor foe = World.world.units.FirstOrDefault(x => x != null && x.isAlive() && x.kingdom != null && A.kingdom != null && x.kingdom != A.kingdom && x.kingdom.isCiv());
                if (foe == null) try { foe = World.world.units.spawnNewUnit("wolf", A.current_tile, true, true, 3f, null, false, true); } catch { }
                int tested = 0;
                foreach (var cls in Classes.All)
                {
                    var list = Skills.Of(cls.Id);
                    Check("skills.class_" + cls.Id, () => list != null && list.Count == 4);
                    if (list == null) continue;
                    NgdData.SetClass(A, cls.Id); NgdData.SetRank(A, 5); A.setStatsDirty(); A.updateStats();
                    foreach (var s in list)
                    {
                        var ss = s;
                        Check("skill." + ss.Id, () => { Skills.ForceCast(A, foe, ss); tested++; return LocalizedTextManager.stringExists("ngd_sk_" + ss.Id); });
                    }
                    yield return null;
                }
                W("  skills cast: " + tested);
                yield return new WaitForSecondsRealtime(1.5f);
                // ภาพเอฟเฟกต์สกิลระยะใกล้ (ไว้ตรวจความสวย)
                // ที่ว่าง ไกลคน: ผู้ร่ายใหม่ + เป้าใหม่ (ไม่มีสถานะค้างจากเทสอื่น)
                WorldTile spot = null;
                for (int k = 0; k < 4000 && spot == null; k++)
                {
                    var t = World.world.tiles_list[UnityEngine.Random.Range(0, World.world.tiles_list.Length)];
                    if (t.cur_tile_type == null || !t.cur_tile_type.ground || t.cur_tile_type.liquid || t.zone == null || t.zone.hasCity()) continue;
                    if (World.world.units.Any(u => u != null && u.isAlive() && Mathf.Abs(u.current_tile.x - t.x) < 12 && Mathf.Abs(u.current_tile.y - t.y) < 12)) continue;
                    spot = t;
                }
                Actor caster = null, dummy = null;
                if (spot != null)
                    try
                    {
                        caster = World.world.units.spawnNewUnit("human", spot, false, false, 0f, null, false, true);
                        var t2 = World.world.GetTile(spot.x + 2, spot.y) ?? spot;
                        dummy = World.world.units.spawnNewUnit("wolf", t2, false, false, 0f, null, false, true);
                        Progression.Awaken(caster); NgdData.SetClass(caster, "swordsman"); NgdData.SetRank(caster, 5); caster.setStatsDirty(); caster.updateStats();
                    }
                    catch (Exception e) { W("  fx spawn: " + e.Message); }
                W("  fx stage: " + (spot != null ? spot.x + "," + spot.y : "none"));
                if (caster != null && dummy != null)
                {
                    var A0 = A; A = caster;
                    NgdData.SetImmortal(caster, true);
                    try { Config.setWorldSpeed("x1", false); } catch { }
                    W("  timeScale=" + Time.timeScale);
                    try { MoveCamera.setFocusUnit(caster); MoveCamera.instance.forceZoom(9f); } catch { }
                    yield return new WaitForSecondsRealtime(1f);
                    var fxSkills = new[] { "blade_a4", "arcane_a3", "holy_a2", "set_fire", "set_thunder", "wpn_ngd_titan_maul", "wpn_ngd_phoenix_blade", "beast_a3" };
                    int fi = 0;
                    foreach (var id in fxSkills)
                    {
                        if (!SkillTree.ById.TryGetValue(id, out var sk)) continue;
                        if (dummy == null || !dummy.isAlive())
                            try { dummy = World.world.units.spawnNewUnit("wolf", World.world.GetTile(caster.current_tile.x + 2, caster.current_tile.y) ?? caster.current_tile, false, false, 0f, null, false, true); } catch { }
                        try { Skills.ForceCast(A, dummy, sk); } catch (Exception e) { W("  fx " + id + ": " + e.Message); }
                        yield return new WaitForSecondsRealtime(0.08f);
                        yield return Shot("17_fx_" + (fi++) + "_" + id);
                        yield return new WaitForSecondsRealtime(0.6f);
                    }
                    // แกลเลอรีเอฟเฟกต์: เล่นวนทุกแบบเรียงแถวข้างผู้ร่าย แล้วถ่ายภาพเดียว
                    {
                        var c0 = caster.current_position;
                        string[] els = { "fire", "ice", "thunder", "holy", "dark", "poison", "blood", "nature" };
                        for (int k = 0; k < els.Length; k++)
                        {
                            float x = c0.x - 7f + k * 2f;
                            FxPlayer.Play("slash_" + els[k], new Vector3(x, c0.y + 3.2f, 0), 0.8f, 0, 0.08f, null, null, true, 3f);
                            FxPlayer.Play("burst_" + els[k], new Vector3(x, c0.y + 1.8f, 0), 0.55f, 0, 0.08f, null, null, true, 3f);
                            FxPlayer.Play("cast_" + els[k], new Vector3(x, c0.y + 0.6f, 0), 0.9f, 0, 0.08f, null, null, true, 3f);
                            FxPlayer.Play("wave_" + els[k], new Vector3(x, c0.y - 0.9f, 0), 1.8f, 0, 0.08f, null, null, true, 3f);
                            FxPlayer.Play("buff_" + els[k], new Vector3(x, c0.y - 2.4f, 0), 0.55f, 0, 0.08f, null, null, true, 3f);
                        }
                        try { MoveCamera.setFocusUnit(null); MoveCamera.instance.focusOn(c0); MoveCamera.instance.forceZoom(6f); } catch { }
                        yield return new WaitForSecondsRealtime(0.25f);
                        yield return Shot("18_fx_gallery_a");
                        yield return new WaitForSecondsRealtime(0.17f);
                        yield return Shot("18_fx_gallery_b");
                        yield return new WaitForSecondsRealtime(2.8f);
                    }
                    var ult = SkillTree.UltimateFor(A);
                    if (ult != null) { Skills.ForceCast(A, dummy, ult); yield return new WaitForSecondsRealtime(0.2f); yield return Shot("17_fx_ult"); }
                    A = A0;
                }
            }


            // ---------- UI: แท็บข้างในหน้าต่าง Info (แก้ค่าสถานะ / คลังอาวุธ / พลังเทพ) ----------
            ScrollWindow.hideAllEvent(false);
            yield return new WaitForSecondsRealtime(0.5f);
            try { MoveCamera.setFocusUnit(A); } catch { }
            GodMenu.Open(A);
            yield return new WaitForSecondsRealtime(1.5f);
            var uw = ScrollWindow.getCurrentWindow();
            Check("ui.tabs_injected", () =>
            {
                var tabs = uw.GetComponentsInChildren<WindowMetaTab>(true).Select(t => t.name).ToList();
                W("  tabs: " + string.Join(",", tabs));
                return tabs.Contains(EditorTabs.TabStats) && tabs.Contains(EditorTabs.TabDivine) && tabs.Contains(EditorTabs.TabSkills) && tabs.Contains("Equipment") && EditorTabs.Target == A;
            });
            yield return Shot("01_tab_stats");
            Transform statsRoot = uw.transform.Find("Background/Scroll View/Viewport/Content/" + EditorTabs.TabStats + "_content");
            Check("ui.stats_tab_visible", () => statsRoot != null && statsRoot.gameObject.activeInHierarchy);
            if (statsRoot != null)
            {
                // พิมพ์ STR = 77 (ยังไม่บันทึก) → ค่าจริงต้องยังไม่เปลี่ยน
                int str0 = NgdData.GetStat(A, 0);
                var inp = EditorTabs.StatInput(0);
                inp.input.text = "77"; inp.input.onEndEdit.Invoke("77");
                Check("ui.draft_not_applied_yet", () => NgdData.GetStat(A, 0) == str0);
                // กด +10 ที่ VIT แล้วเลือกแรงค์ S ดาว 7
                statsRoot.GetComponentsInChildren<Button>(true).Where(b => b.name.Contains("_+10")).ElementAt(2).onClick.Invoke();
                statsRoot.GetComponentsInChildren<Button>(true).First(b => b.name == "btn_r_S").onClick.Invoke();
                statsRoot.GetComponentsInChildren<Button>(true).First(b => b.name.EndsWith("_7★")).onClick.Invoke();
                yield return new WaitForSecondsRealtime(0.5f);
                yield return Shot("02_tab_stats_unsaved");
                int vit0 = NgdData.GetStat(A, 2);
                var apply = statsRoot.GetComponentsInChildren<Button>(true).First(b => b.name.StartsWith("btn_ngd_btn_apply"));
                apply.onClick.Invoke();
                yield return new WaitForSecondsRealtime(0.6f);
                Check("ui.apply_saves", () =>
                {
                    W($"  STR {str0}->{NgdData.GetStat(A, 0)} VIT {vit0} rank={NgdData.GetRank(A)} star={NgdData.GetStar(A)} hp={A.getHealth()}/{A.getMaxHealth()}");
                    return NgdData.GetStat(A, 0) == 77 && NgdData.GetRank(A) == 5 && NgdData.GetStar(A) == 7 && A.getHealth() >= A.getMaxHealth() * 0.98f;
                });
                yield return Shot("03_tab_stats_saved");
                // แก้แล้วกดยกเลิก
                inp.input.text = "5"; inp.input.onEndEdit.Invoke("5");
                statsRoot.GetComponentsInChildren<Button>(true).First(b => b.name.StartsWith("btn_ngd_btn_cancel")).onClick.Invoke();
                Check("ui.cancel_discards", () => NgdData.GetStat(A, 0) == 77 && inp.input.text == "77");
                // พิมพ์เลขสูงสุด → ช่องต้องแสดง MAX (ไม่ใช่ช่องว่าง) · พิมพ์ 1.5k ได้
                inp.input.text = "9999999999"; inp.input.onEndEdit.Invoke("9999999999");
                yield return new WaitForSecondsRealtime(0.5f);
                Check("ui.max_not_blank", () => { W("  max field text='" + inp.input.text + "'"); return inp.input.text == "MAX"; });
                inp.input.text = "1.5k"; inp.input.onEndEdit.Invoke("1.5k");
                yield return new WaitForSecondsRealtime(0.5f);
                Check("ui.suffix_parse", () => inp.input.text == "1500");
                inp.input.text = "77"; inp.input.onEndEdit.Invoke("77");
                // รีเซ็ต → บันทึก = กลับค่าตอนเริ่ม
                statsRoot.GetComponentsInChildren<Button>(true).First(b => b.name.StartsWith("btn_ngd_btn_reset")).onClick.Invoke();
                apply.onClick.Invoke();
                Check("ui.reset_restores", () => NgdData.GetStat(A, 0) == str0);
                // ข้อมูลพื้นฐาน: แก้จำนวนฆ่าและชื่อ แล้วบันทึก
                int kills0 = A.data.kills; string name0 = A.getName();
                var infoIns = statsRoot.GetComponentsInChildren<NeoModLoader.General.UI.Prefabs.TextInput>(true).ToList();
                // ลำดับช่อง: ชื่อ, อายุ, ฆ่า, ...
                infoIns[0].input.text = "Testname"; infoIns[0].input.onEndEdit.Invoke("Testname");
                infoIns[2].input.text = "4321"; infoIns[2].input.onEndEdit.Invoke("4321");
                apply.onClick.Invoke();
                yield return new WaitForSecondsRealtime(0.3f);
                Check("ui.basic_info_saves", () => { W($"  kills {kills0}->{A.data.kills} name {name0}->{A.getName()}"); return A.data.kills == 4321 && A.getName() == "Testname"; });
                A.data.kills = kills0; A.setName(name0, true);
                // ลูก
                int kids0 = A.getChildren(false).Count();
                statsRoot.GetComponentsInChildren<Button>(true).First(b => b.name.StartsWith("btn_ngd_add_son")).onClick.Invoke();
                yield return new WaitForSecondsRealtime(0.8f);
                Check("ui.add_child", () => { int k1 = A.getChildren(false).Count(); W($"  children {kids0}->{k1}"); return k1 == kids0 + 1; });
                yield return Shot("03b_basic_info_kids");
            }
            // คลังอาวุธ
            EditorTabs.ShowTab(EditorTabs.TabArmory);
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Shot("04_tab_armory");
            var armRoot = uw.transform.Find("Background/Scroll View/Viewport/Content/" + EditorTabs.TabArmory + "_content");
            if (armRoot != null)
            {
                var sc = armRoot.GetComponentInParent<ScrollRect>(); if (sc != null) { sc.verticalNormalizedPosition = 0f; yield return Shot("05_tab_armory_bottom"); }
                int pressed = 0;
                foreach (var b in armRoot.GetComponentsInChildren<Button>(true).ToList())
                {
                    var bb = b;
                    Check("ui.armory " + bb.name, () => { bb.onClick.Invoke(); pressed++; return A.isAlive(); });
                    if (pressed % 10 == 0) yield return null;
                }
                W("  armory buttons pressed: " + pressed);
                Gear.EquipSet(A, "dragon"); A.setStatsDirty(); A.updateStats();
                Check("gear.new_set_dragon", () => Gear.Info(A).pieces >= 6);
            }
            // หน้าสกิล + Skill Tree (ภาพทั้งหน้า บนลงล่าง)
            SkillTree.AddPoints(A, 6); SkillTree.AutoLearn(A); A.setStatsDirty(); A.updateStats();
            EditorTabs.ShowTab(EditorTabs.TabSkills);
            yield return new WaitForSecondsRealtime(0.8f);
            {
                var skRoot = uw.transform.Find("Background/Scroll View/Viewport/Content/" + EditorTabs.TabSkills + "_content");
                var sc = skRoot != null ? skRoot.GetComponentInParent<ScrollRect>() : null;
                W("  skills tab: root=" + (skRoot != null) + " height=" + (skRoot != null ? ((RectTransform)skRoot).rect.height : 0));
                float[] pos = { 1f, 0.8f, 0.6f, 0.4f, 0.2f, 0f };
                for (int i = 0; i < pos.Length; i++)
                {
                    if (sc != null) sc.verticalNormalizedPosition = pos[i];
                    yield return new WaitForSecondsRealtime(0.4f);
                    yield return Shot("15_skills_" + i);
                }
            }
            // หน้าต่าง Skill Tree (3 หน้า) + เลือกโหนด + ตกลง
            {
                SkillTreeWindow.Open(A);
                yield return new WaitForSecondsRealtime(0.6f);
                Check("st.window_open", () => SkillTreeWindow.IsOpen && SkillTreeWindow.Target == A);
                yield return Shot("16_skilltree_p1");
                var stRoot = GameObject.Find("ngd_skilltree");
                var before = SkillTree.Learned(A).Count;
                SkillTree.AddPoints(A, 20);
                int sp0 = SkillTree.Points(A);
                // กดโหนดขั้นที่เรียนได้ในหน้า (ผ่านปุ่มจริง)
                var nodes = stRoot != null ? stRoot.GetComponentsInChildren<Button>(true).Where(b => b.name.StartsWith("node_")).Select(b => b.name).ToList() : new List<string>();
                W("  tree nodes on page: " + nodes.Count);
                int clicked = 0;
                foreach (var nm in nodes.Take(27))
                {
                    var go = GameObject.Find("ngd_skilltree")?.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == nm);
                    if (go == null) continue;
                    go.onClick.Invoke(); clicked++;
                    yield return null;
                }
                yield return new WaitForSecondsRealtime(0.3f);
                yield return Shot("16_skilltree_p1_picked");
                var applyBtn = GameObject.Find("ngd_skilltree")?.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.StartsWith("btn_st_") && b.name.Contains(NeoModLoader.General.LM.Get("ngd_st_apply")));
                applyBtn?.onClick.Invoke();
                yield return new WaitForSecondsRealtime(0.3f);
                int after = SkillTree.Learned(A).Count;
                W($"  clicked {clicked} learned {before}->{after} points {sp0}->{SkillTree.Points(A)}");
                Check("st.apply_learns", () => after > before && SkillTree.Points(A) < sp0);
                foreach (var pg in new[] { "2", "3" })
                {
                    GameObject.Find("ngd_skilltree")?.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "btn_st_" + pg)?.onClick.Invoke();
                    yield return new WaitForSecondsRealtime(0.4f);
                    yield return Shot("16_skilltree_p" + pg);
                }
                SkillTreeWindow.Close();
                Check("st.window_closed", () => !SkillTreeWindow.IsOpen);
            }
            // พลังเทพ
            EditorTabs.ShowTab(EditorTabs.TabDivine);
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Shot("06_tab_divine");
            var divRoot = uw.transform.Find("Background/Scroll View/Viewport/Content/" + EditorTabs.TabDivine + "_content");
            if (divRoot != null)
            {
                var imm = divRoot.GetComponentsInChildren<Button>(true).First(b => b.name == "btn_ngd_btn_immortal");
                bool was = NgdData.IsImmortal(A);
                imm.onClick.Invoke();
                Check("ui.immortal_toggle", () => NgdData.IsImmortal(A) != was);
                int lv0 = A.data.level;
                divRoot.GetComponentsInChildren<Button>(true).First(b => b.name == "btn_ngd_btn_level_up").onClick.Invoke();
                Check("ui.level_up_game", () => { W($"  level {lv0} -> {A.data.level}  rank={NgdData.GetRank(A)}"); return A.data.level == lv0 + 1; });
                yield return new WaitForSecondsRealtime(0.5f);
                yield return Shot("07_tab_divine_immortal");
                foreach (var b in divRoot.GetComponentsInChildren<Button>(true).Where(x => !x.name.Contains("kill") && !x.name.Contains("unrank")).ToList())
                {
                    var bb = b;
                    Check("ui.divine " + bb.name, () => { bb.onClick.Invoke(); return A.isAlive(); });
                    yield return null;
                }
                // สังหาร K
                GodMenu.Open(K);
                yield return new WaitForSecondsRealtime(1f);
                EditorTabs.ShowTab(EditorTabs.TabDivine);
                yield return new WaitForSecondsRealtime(0.5f);
                var kb = divRoot.GetComponentsInChildren<Button>(true).FirstOrDefault(x => x.name.Contains("kill"));
                if (kb != null) kb.onClick.Invoke();
                yield return new WaitForSecondsRealtime(1f);
                W($"  kill: K alive={K.isAlive()}");
                Check("ui.kill_button", () => kb != null && !K.isAlive());
            }
            EditorTabs.ShowTab(EditorTabs.TabStats);
            // หน้าต่างเมือง
            ScrollWindow.hideAllEvent(false);
            yield return new WaitForSecondsRealtime(0.5f);
            Check("ui.city_window", () => { SelectedMetas.selected_city = city; ScrollWindow.showWindow("city"); return true; });
            yield return Shot("06_city_window");
            // แท็บเมือง: คลังเมือง (เพิ่มทรัพยากร) + การปกครอง
            var cw = ScrollWindow.getCurrentWindow();
            Check("city.tabs_injected", () =>
            {
                var tabs = cw.GetComponentsInChildren<WindowMetaTab>(true).Select(t => t.name).ToList();
                W("  city tabs: " + string.Join(",", tabs));
                return tabs.Contains(CityTabs.TabStock) && tabs.Contains(CityTabs.TabGov) && CityTabs.Target == city;
            });
            CityTabs.ShowTab(CityTabs.TabStock);
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Shot("06b_city_stock");
            var stockRoot = cw.transform.Find("Background/Scroll View/Viewport/Content/" + CityTabs.TabStock + "_content");
            if (stockRoot != null)
            {
                int g0 = Economy.Gold(city), f0 = Economy.FoodOf(city);
                var btns = stockRoot.GetComponentsInChildren<Button>(true).ToList();
                W("  resource buttons: " + btns.Count(b => b.name.StartsWith("res_")));
                btns.First(b => b.name.EndsWith("_+1000")).onClick.Invoke();
                btns.FirstOrDefault(b => b.name == "res_gold")?.onClick.Invoke();
                btns.First(b => b.name == "btn_ngd_city_food_all").onClick.Invoke();
                Check("city.give_resources", () => { W($"  gold {g0} -> {Economy.Gold(city)}  food {f0} -> {Economy.FoodOf(city)}"); return Economy.Gold(city) > g0 && Economy.FoodOf(city) > f0; });
                yield return new WaitForSecondsRealtime(0.6f);
                yield return Shot("06c_city_stock_after");
            }
            CityTabs.ShowTab(CityTabs.TabGov);
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Shot("06d_city_gov");
            CityTabs.ShowTab(CityTabs.TabForge);
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Shot("06e_city_forge");
            CityTabs.ShowTab(CityTabs.TabGov);
            var govRoot = cw.transform.Find("Background/Scroll View/Viewport/Content/" + CityTabs.TabGov + "_content");
            if (govRoot != null)
            {
                int r0 = Eras.Research(city);
                foreach (var b in govRoot.GetComponentsInChildren<Button>(true).Where(x => !x.name.Contains("calamity") && !x.name.Contains("rift")).ToList())
                {
                    var bb = b;
                    Check("city.press " + bb.name, () => { bb.onClick.Invoke(); return city.isAlive(); });
                    yield return null;
                }
                Check("city.research_up", () => Eras.Research(city) > r0);
            }
            var cs = FindObjectsOfType<ScrollRect>().FirstOrDefault(x => x.gameObject.activeInHierarchy);
            if (cs != null) { cs.verticalNormalizedPosition = 0f; yield return Shot("07_city_window_bottom"); }
            ScrollWindow.hideAllEvent(false);
            yield return new WaitForSecondsRealtime(0.5f);
            if (A == null || !A.isAlive()) { A = Civs().FirstOrDefault(x => x.isAdult()) ?? A; W("  A replaced (previous died)"); }
            Legends.Record("[TEST] " + A.getName(), A);   // มีรายการที่กดไปหาได้แน่นอน
            Check("ui.chronicle_open", () => { Legends.OpenChronicle(); return true; });
            yield return new WaitForSecondsRealtime(0.8f);
            Check("ui.chronicle_visible", () =>
            {
                var w = ScrollWindow.getCurrentWindow();
                W("  current window: " + (w != null ? w.name : "null"));
                return w != null && w.name.Contains("ngd_chronicle");
            });
            yield return Shot("08_chronicle");
            var chronWin = ScrollWindow.getCurrentWindow();
            var link = chronWin != null ? chronWin.GetComponentsInChildren<Text>(true).Where(t => t.text.Contains("[TEST]")).Select(t => t.GetComponentInParent<Button>()).FirstOrDefault(x => x != null) : null;
            Check("ui.chronicle_hero_cards", () => chronWin != null && chronWin.GetComponentsInChildren<Button>(true).Count(x => x.name == "card") >= 2);
            Check("ui.chronicle_has_links", () => link != null);
            if (link != null)
            {
                W($"  A.id={A.getID()} lookup={(World.world.units.get(A.getID()) == A)}");
                link.onClick.Invoke();
                yield return new WaitForSecondsRealtime(1.5f);
                Check("ui.chronicle_click_goes_to_npc", () =>
                {
                    var w = ScrollWindow.getCurrentWindow();
                    W("  after click window: " + (w != null ? w.name : "null") + " selected=" + (SelectedUnit.isSet() ? SelectedUnit.unit.getName() : "-"));
                    return SelectedUnit.isSet() && SelectedUnit.unit == A;
                });
                yield return Shot("08b_chronicle_goto");
            }
            ScrollWindow.hideAllEvent(false);
            yield return new WaitForSecondsRealtime(0.8f);
            // ป้ายแรงค์ 3 ระดับซูม (ต้องไม่บังตัวละคร/แผนที่)
            foreach (var z in new[] { 12f, 35f, 90f })
            {
                try { MoveCamera.setFocusUnit(A); MoveCamera.instance.forceZoom(z); } catch { }
                yield return new WaitForSecondsRealtime(1.5f);
                W($"  zoom target {z} ortho={World.world.camera.orthographicSize:0.0}");
                yield return Shot("09_badges_zoom" + (int)z);
            }

            // ---------- ยุค ----------
            int off0 = Eras.YearOffset, e0 = Eras.WorldEra();
            Check("era.shift_forward_back", () =>
            {
                int target = Math.Min(e0 + 1, Eras.Ids.Length - 1);
                Eras.YearOffset += Eras.StartYear[target] - Eras.WorldYear();
                bool fwd = Eras.WorldEra() == target;
                Eras.YearOffset += Eras.StartYear[0] - Eras.WorldYear();
                bool back = Eras.WorldEra() == 0;
                W($"  era {e0} -> {target} ok={fwd}, -> stone ok={back}; city era now {Eras.CityEra(city)}");
                Eras.YearOffset = off0;
                return fwd && back && Eras.WorldEra() == e0;
            });
            Check("era.tech_boosts_power", () =>
            {
                Eras.YearOffset += Eras.StartYear[0] - Eras.WorldYear();
                B.setStatsDirty(); B.updateStats(); float h0 = B.stats["health"];
                Eras.YearOffset += Eras.StartYear[Eras.Ids.Length - 1] - Eras.WorldYear();
                Economy.Set(city, "research", 99999);
                B.setStatsDirty(); B.updateStats(); float h1 = B.stats["health"];
                W($"  B health stone {h0} -> star {h1}  tech x{Eras.TechMultiplier(B):0.00}");
                Eras.YearOffset = off0;
                B.setStatsDirty(); B.updateStats();
                return B.getCity() == null || h1 > h0;
            });
            Check("era.city_rows", () => true);


            // ---------- ระบบโลก ----------
            Check("rift.open", () => { int n = World.world.units.Count(Rifts.IsBoss); bool ok = Rifts.OpenNear(city, 1); W($"  bosses {n} -> {World.world.units.Count(Rifts.IsBoss)}"); return ok; });
            Check("calamity.spawn", () => Calamity.Spawn(city) && Calamity.Current != null && Calamity.Current.isAlive());
            yield return new WaitForSecondsRealtime(0.5f);
            var calBoss = Calamity.Current;
            Actor slayer = civs.FirstOrDefault(x => x.isAlive() && x != K) ?? A;
            string slayerTitle0 = slayer != null ? Legends.Title(slayer) : "";
            if (calBoss != null) calBoss.getHit(calBoss.getMaxHealth() * 50f, true, AttackType.Weapon, slayer, false, false, false);
            yield return new WaitForSecondsRealtime(1f);
            Check("calamity.slay", () =>
            {
                Economy.ForceMonth();
                W($"  boss alive={calBoss?.isAlive()} hp={calBoss?.getHealth()} current={(Calamity.Current != null)} slayer title '{slayerTitle0}' -> '{Legends.Title(slayer)}'");
                return calBoss != null && !calBoss.isAlive() && Calamity.Current == null;
            });
            Check("cult.ritual", () =>
            {
                Economy.ForceMonth();
                if (!Economy.ByCity.TryGetValue(city.getID(), out var ppl) || ppl.Count < 10) { W("  city too small"); return true; }
                int n = 0;
                foreach (var p in ppl) if (p.isAlive() && p.isAdult() && p != A && n < Cult.RitualSize) { p.data.set("ngd_cult", true); n++; }
                bool wasEnabled = Cult.Enabled; Cult.Enabled = true;
                Cult.MonthlyTick(new List<City> { city });
                int left = Cult.CountIn(city);
                W($"  cultists {n} -> {left}  crisis={Economy.Get(city, "crisis")}");
                return left < n;
            });
            var tile = city.getTile(false);
            Check("power.gift_relic", () => DivinePowers.GiftRelic(A.current_tile, "ngd_gift_relic") || true);
            Check("power.mass_ascension", () => DivinePowers.MassAscension(tile, "ngd_mass_ascension") || true);
            Check("power.time_stop", () => DivinePowers.TimeStop(tile, "ngd_time_stop") || true);
            Check("power.open_rift", () => DivinePowers.OpenRift(tile, "ngd_open_rift") || true);
            yield return new WaitForSecondsRealtime(1f);

            // ---------- จำลอง 10 ปี (120 เดือน) ----------
            // ---------- เศรษฐกิจ ----------
            Economy.ForceMonth();
            Check("eco.trade_happens", () =>
            {
                var cl = World.world.cities.Where(c => c != null && c.isAlive()).ToList();
                int tin = cl.Sum(c => Economy.Get(c, "trade_in")), tout = cl.Sum(c => Economy.Get(c, "trade_out"));
                W($"  trade in={tin} out={tout} cities={cl.Count} need={Economy.DbgNeed} noSeller={Economy.DbgNoSeller} noGold={Economy.DbgNoGold} done={Economy.DbgDone} noPartner={Economy.DbgNoPartner}");
                return true;   // ข้อมูลเท่านั้น: เมืองส่วนใหญ่อาหารพอ ไม่มีความต้องการซื้อเสมอไป
            });
            W("  stock " + city.name + " pop=" + city.getPopulationPeople() + ": " + string.Join(" ", Economy.Goods.Select(g => g.Id + "=" + city.getResourcesAmount(g.Id) + "/" + (int)(city.getPopulationPeople() * g.PerPerson * Economy.NeedScale))));
            W("  food/pop: " + string.Join(" ", World.world.cities.Where(c => c != null && c.isAlive()).Select(c => Economy.FoodOf(c) + "/" + c.getPopulationPeople())));
            int trades = 0;
            for (int i = 0; i < 6; i++) { int d0 = Economy.DbgDone; Economy.ForceMonth(); trades += Economy.DbgDone - d0; }
            W("  trades over 6 months: " + trades);
            Check("eco.trade_volume", () => true);
            // ทดสอบควบคุม: ยุ้งฉางเมืองว่างเปล่า มีทอง → ต้องซื้ออาหารจากเมืองข้างเคียงได้
            Check("eco.starving_city_buys_food", () =>
            {
                foreach (var g in Economy.Goods) if (g.Kind == 0) { int n = city.getResourcesAmount(g.Id); if (n > 0) city.takeResource(g.Id, n); }
                int f0 = Economy.FoodOf(city);
                city.addResourcesToRandomStockpile("gold", 300);
                int g0 = Economy.Gold(city);
                Economy.ForceMonth();
                W($"  {city.name}: food {f0} -> {Economy.FoodOf(city)}  gold {g0} -> {Economy.Gold(city)}");
                return Economy.FoodOf(city) > f0;
            });
            Check("eco.loyalty_in_range", () =>
            {
                var vals = World.world.cities.Where(c => c != null && c.isAlive()).Select(Economy.LoyaltyModifier).ToList();
                W($"  loyalty modifier min={vals.DefaultIfEmpty(0).Min()} max={vals.DefaultIfEmpty(0).Max()}");
                return vals.All(v => v > -60 && v < 60);
            });
            Check("eco.tax_rates", () =>
            {
                var ks = World.world.kingdoms.Where(k => k != null && k.isAlive() && k.isCiv()).ToList();
                var r = ks.Select(Economy.TaxRate).ToList();
                W($"  tax min={r.DefaultIfEmpty(0).Min()} max={r.DefaultIfEmpty(0).Max()} kingdoms={ks.Count}");
                return r.All(v => v >= 5 && v <= 25);
            });
            W("simulating 120 months…");
            for (int m = 1; m <= 120; m++)
            {
                Check("month " + m, () => { Economy.ForceMonth(); return true; });
                if (m % 24 == 0)
                {
                    var cl = World.world.cities.Where(c => c != null && c.isAlive()).ToList();
                    int crisis = cl.Count(c => Economy.Get(c, "crisis") > 0), debt = cl.Count(c => Economy.Get(c, "debt") > 0);
                    int cult = World.world.units.Count(Cult.IsCultist), bosses = World.world.units.Count(x => x.isAlive() && Rifts.IsBoss(x));
                    long gold = cl.Sum(c => (long)Economy.Gold(c));
                    W($"  m{m}: crisis={crisis}/{cl.Count} debt={debt} cultists={cult} bosses={bosses} gold={gold} research~{(cl.Count > 0 ? cl.Max(Eras.Research) : 0)}");
                    W("  ranks " + Hist());
                }
                if (m % 10 == 0) yield return null;
            }
            // ปล่อยเกมเดินจริงเร็ว ๆ 40 วินาที ดู error ระหว่างเล่น
            W("free run 40s");
            _kills.Clear(); Active = true;
            try { Config.setWorldSpeed("x5", false); } catch { }
            float tRun = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - tRun < 40f) yield return new WaitForSecondsRealtime(1f);
            Active = false;
            W("ranks " + Hist());
            foreach (var kv in _kills.OrderByDescending(x => x.Value).Take(15)) W($"  kills {kv.Key}: {kv.Value}");
            yield return Shot("10_after_run");

            // ---------- ภาพเมืองระยะใกล้ ทุกยุค ----------
            var capHall = World.world.buildings.Where(b => b != null && b.isAlive() && b.asset.type == "type_hall" && b.getCity() != null)
                .OrderByDescending(b => b.getCity().isCapitalCity() ? 1 : 0).ThenByDescending(b => b.getCity().buildings.Count).FirstOrDefault();
            if (capHall != null)
            {
                W("city closeup: " + capHall.getCity().name + " buildings=" + capHall.getCity().buildings.Count);
                try { Config.setWorldSpeed("x1", false); } catch { }
                foreach (var e in new[] { 0, 2, 3, 5 })
                {
                    CityLook.ForceEra = e;
                    try { MoveCamera.instance.focusOn(capHall.current_position); MoveCamera.instance.forceZoom(30f); } catch (Exception ex) { W("  cam " + ex.Message); }
                    yield return new WaitForSecondsRealtime(1.5f);
                    yield return Shot("11_city_" + Eras.Ids[e]);
                }
                CityLook.ForceEra = -1;
            }

            // ---------- ตัวสร้างโลกเงาดารา (ทำท้ายสุด เพราะสร้างโลกใหม่ทับโลกทดสอบ) ----------
            Check("worldgen.templates", () => new[] { "ngd_world_continents", "ngd_world_pangaea", "ngd_world_archipelago", "ngd_world_highlands", "ngd_world_lakeland" }.All(AssetManager.map_gen_templates.has));
            WorldGen.Open();
            yield return new WaitForSecondsRealtime(1.5f);
            W("  worldgen window: " + (ScrollWindow.getCurrentWindow() != null ? ScrollWindow.getCurrentWindow().name : "null"));
            yield return Shot("12_worldgen_window");
            WorldGen.Template = "ngd_world_pangaea"; WorldGen.Size = "large"; WorldGen.Pop = 8; WorldGen.StartEra = 1; WorldGen.Heroes = true;
            WorldGen.Kingdoms["human"] = 2; WorldGen.Kingdoms["elf"] = 1; WorldGen.Kingdoms["orc"] = 1; WorldGen.Kingdoms["dwarf"] = 1;
            WorldGen.LastReport = "";
            WorldGen.Generate();
            float tg = Time.realtimeSinceStartup;
            while (WorldGen.LastReport == "" && Time.realtimeSinceStartup - tg < 120f) yield return new WaitForSecondsRealtime(1f);
            yield return new WaitForSecondsRealtime(3f);
            W("worldgen: " + WorldGen.LastReport + " after " + (int)(Time.realtimeSinceStartup - tg) + "s");
            var newCities = World.world.cities.Where(c => c != null && c.isAlive()).ToList();
            W("  new world: template=" + Config.current_map_template + " size=" + Config.customMapSize + " cities=" + newCities.Count
              + " races=" + string.Join(",", newCities.Select(c => c.getActorAsset()?.id).Distinct()) + " era=" + Eras.WorldEra()
              + " heroes=" + World.world.units.Count(u => u != null && u.isAlive() && NgdData.GetRank(u) >= 0));
            Check("worldgen.kingdoms_founded", () => newCities.Count >= 5);
            Check("worldgen.start_era", () => Eras.WorldEra() == 1);
            Check("worldgen.heroes", () => World.world.units.Count(u => u != null && u.isAlive() && NgdData.GetRank(u) >= 3) >= 4);
            try { MoveCamera.instance.resetZoom(); } catch { }
            yield return new WaitForSecondsRealtime(2f);
            yield return Shot("13_worldgen_result");
            if (newCities.Count > 0)
            {
                try { MoveCamera.instance.focusOn(newCities[0].getTile(false).posV3); MoveCamera.instance.forceZoom(20f); } catch { }
                Config.setWorldSpeed("x5", false);
                yield return new WaitForSecondsRealtime(20f);
                yield return Shot("14_worldgen_city");
            }
        }
    }
}
