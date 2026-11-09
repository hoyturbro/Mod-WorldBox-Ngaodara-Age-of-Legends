using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ระบบสกิลรุ่นใหม่: NPC ใช้ได้ 7 ช่อง (6 สกิล + 1 อัลติเมท) จาก 4 แหล่ง
    ///   1) สกิลอาชีพ (Skills.All)
    ///   2) สกิลเซ็ต: ใส่ครบ 6 ชิ้นได้สกิลของเซ็ตนั้น
    ///   3) สกิลอาวุธเทพ: แต่ละชิ้นมีสกิลประจำตัว
    ///   4) Skill Tree 9 สาย × 5 ขั้น (active + passive · ขั้น 5 = ปรมาจารย์)
    /// อัลติเมท = อาชีพ × ธาตุเซ็ต × ชนิดอาวุธ (สร้างชื่อ/ผลเฉพาะตัว หลายพันแบบ) · ปลดเมื่อแรงค์ S หรือเป็นปรมาจารย์สายใดสายหนึ่ง
    /// ใช้ได้ทุกเผ่า: สิ่งมีชีวิตที่ไม่มีอาชีพ (สัตว์/มอนฯ) ใช้สายอสูร
    /// </summary>
    public static class SkillTree
    {
        public enum Source { Class, Set, Weapon, Tree, Ultimate }

        public class Branch
        {
            public string Id, Th, En, Fx; public int Stat;
            public string[] ActTh, ActEn, PasTh, PasEn; public string MasTh, MasEn;
        }

        public static readonly Branch[] Branches =
        {
            B("blade", "วิถีดาบ", "Way of the Blade", "phys", 0, new[] { "ฟันพริบตา", "ระบำดาบ", "ดาบคลื่นลม", "ดาบพิฆาตดารา" }, new[] { "Flash Cut", "Blade Dance", "Wind Blade Wave", "Star-Slaying Edge" },
              new[] { "จับดาบมั่น", "คมดาบ", "ลมหายใจดาบ", "ใจดาบ" }, new[] { "Firm Grip", "Keen Edge", "Blade Breath", "Sword Heart" }, "เซียนดาบ", "Sword Saint"),
            B("martial", "วิถีหมัด", "Martial Arts", "wind", 0, new[] { "หมัดเหล็ก", "เตะสายฟ้า", "หมัดคลื่นปราณ", "ฝ่ามือทลายภูผา" }, new[] { "Iron Fist", "Lightning Kick", "Chi Wave", "Mountain-Breaking Palm" },
              new[] { "กายแกร่ง", "เท้าเบา", "ลมปราณหมุนเวียน", "กายวัชระ" }, new[] { "Hardened Body", "Light Feet", "Flowing Chi", "Diamond Body" }, "ปรมาจารย์หมัด", "Fist Grandmaster"),
            B("archery", "วิถีศร", "Archery", "wind", 1, new[] { "ยิงแม่น", "ธนูสามดอก", "ฝนศร", "ศรทะลวงฟ้า" }, new[] { "Precise Shot", "Triple Shot", "Arrow Rain", "Sky-Piercer" },
              new[] { "ตาเหยี่ยว", "มือไว", "ลมนำทาง", "จิตศร" }, new[] { "Hawk Eye", "Quick Draw", "Wind Guide", "Arrow Soul" }, "เทพธนู", "Bow God"),
            B("arcane", "วิถีอาคม", "Arcane", "star", 3, new[] { "ลูกพลังเวท", "กระจายเวท", "พายุอาคม", "ทลายมิติ" }, new[] { "Arcane Bolt", "Arcane Scatter", "Arcane Storm", "Rift Collapse" },
              new[] { "สมาธิ", "คลังมานา", "ร่ายเร็ว", "จิตเวทนิรันดร์" }, new[] { "Focus", "Mana Well", "Quick Cast", "Eternal Mind" }, "จอมปราชญ์", "Archmage"),
            B("holy", "วิถีศักดิ์สิทธิ์", "Holy", "holy", 4, new[] { "แสงชำระ", "พรรักษา", "วงแหวนศักดิ์สิทธิ์", "ทัณฑ์สวรรค์" }, new[] { "Purge Light", "Healing Grace", "Holy Ring", "Heaven's Judgment" },
              new[] { "ศรัทธา", "รัศมีพิทักษ์", "ใจบริสุทธิ์", "กายทิพย์" }, new[] { "Faith", "Guardian Aura", "Pure Heart", "Divine Body" }, "นักบุญ", "Saint"),
            B("shadow", "วิถีเงา", "Shadow", "dark", 1, new[] { "แทงเงา", "พริบตาเงา", "ม่านพิษ", "เงามรณะ" }, new[] { "Shade Stab", "Shadow Step", "Venom Veil", "Death Shade" },
              new[] { "ซ่อนตัว", "จุดตาย", "เงาพลิ้ว", "ใจไร้เงา" }, new[] { "Stealth", "Vital Spot", "Fleeting Shade", "Shadowless Heart" }, "ราชันเงา", "Shadow Lord"),
            B("nature", "วิถีพฤกษา", "Nature", "nature", 4, new[] { "หนามพฤกษา", "รากพันธนาการ", "ป่าคืนชีพ", "พิโรธพงไพร" }, new[] { "Thorn Lash", "Binding Roots", "Living Forest", "Wrath of the Wild" },
              new[] { "ผิวเปลือกไม้", "ฟื้นฟูธรรมชาติ", "ลมหายใจป่า", "หัวใจพงไพร" }, new[] { "Barkskin", "Regrowth", "Forest Breath", "Heart of the Wild" }, "ผู้พิทักษ์พงไพร", "Wild Warden"),
            B("command", "วิถีจอมทัพ", "Command", "holy", 6, new[] { "ตะโกนปลุกใจ", "ทัพประสาน", "ธงสงคราม", "ทัพสวรรค์" }, new[] { "Rally Cry", "Formation", "War Banner", "Heavenly Legion" },
              new[] { "บารมี", "ผู้นำ", "ยุทธศาสตร์", "ราชันย์" }, new[] { "Presence", "Leadership", "Strategy", "Sovereign" }, "จอมทัพ", "Grand Marshal"),
            B("beast", "วิถีอสูร", "Beast", "blood", 2, new[] { "กรงเล็บ", "ขย้ำ", "คำรามสะท้านป่า", "พิโรธอสูร" }, new[] { "Rending Claw", "Maul", "Terror Roar", "Primal Fury" },
              new[] { "หนังหนา", "สัญชาตญาณ", "เลือดร้อน", "อสูรบรรพกาล" }, new[] { "Thick Hide", "Instinct", "Hot Blood", "Ancient Beast" }, "ราชาอสูร", "Beast King"),
        };

        static Branch B(string id, string th, string en, string fx, int stat, string[] at, string[] ae, string[] pt, string[] pe, string mt, string me) =>
            new Branch { Id = id, Th = th, En = en, Fx = fx, Stat = stat, ActTh = at, ActEn = ae, PasTh = pt, PasEn = pe, MasTh = mt, MasEn = me };

        /// <summary>สายที่เข้ากับอาชีพ (AI เรียนตามนี้)</summary>
        public static readonly Dictionary<string, string[]> ClassBranches = new Dictionary<string, string[]>
        {
            { "swordsman", new[] { "blade", "martial" } }, { "knight", new[] { "command", "holy" } }, { "archer", new[] { "archery", "nature" } },
            { "mage", new[] { "arcane", "holy" } }, { "priest", new[] { "holy", "nature" } }, { "assassin", new[] { "shadow", "blade" } },
            { "monk", new[] { "martial", "holy" } }, { "lancer", new[] { "blade", "command" } }, { "berserker", new[] { "beast", "martial" } },
            { "ranger", new[] { "archery", "nature" } }, { "paladin", new[] { "holy", "command" } }, { "spellblade", new[] { "arcane", "blade" } },
            { "bard", new[] { "command", "arcane" } }, { "boss", new[] { "beast", "shadow" } },
            { "guardian", new[] { "command", "holy" } },
            { "engineer", new[] { "arcane", "command" } },
            { "necromancer", new[] { "shadow", "arcane" } },
            { "summoner", new[] { "arcane", "holy" } },
            { "druid", new[] { "nature", "holy" } },
            { "shaman", new[] { "nature", "arcane" } },
            { "elementalist", new[] { "arcane", "nature" } },
            { "samurai", new[] { "blade", "martial" } },
            { "ninja", new[] { "shadow", "martial" } },
            { "gladiator", new[] { "martial", "beast" } },
            { "dragoon", new[] { "blade", "beast" } },
            { "warlock", new[] { "shadow", "arcane" } },
            { "alchemist", new[] { "arcane", "nature" } },
            { "beast_tamer", new[] { "beast", "nature" } },
            { "dancer", new[] { "martial", "command" } },
            { "monster_hunter", new[] { "archery", "blade" } },
            { "freelancer", new[] { "blade", "archery" } },
        };

        public static string[] PreferredBranches(Actor a)
        {
            var c = NgdData.GetClass(a);
            if (!string.IsNullOrEmpty(c) && ClassBranches.TryGetValue(c, out var b)) return b;
            if (!string.IsNullOrEmpty(c) && Classes.Get(c) != null) return new[] { "blade", "martial" };
            return new[] { "beast", "martial" };   // สัตว์/มอนฯ/ทุกเผ่าที่ไม่มีอาชีพ
        }

        // ---------------- โหนด ----------------
        public static readonly Dictionary<string, Skills.Skill> Nodes = new Dictionary<string, Skills.Skill>();
        public static readonly Dictionary<string, Skills.Skill> ById = new Dictionary<string, Skills.Skill>();
        static bool _built;

        public static void Build()
        {
            if (_built) return; _built = true;
            foreach (var s in Skills.All) { s.Source = Source.Class; ById[s.Id] = s; }
            foreach (var br in Branches)
                for (int t = 1; t <= 5; t++)
                {
                    if (t <= 4)
                    {
                        var act = MakeActive(br, t);
                        Nodes[act.Id] = act; ById[act.Id] = act;
                        var pas = new Skills.Skill { Id = br.Id + "_p" + t, Cls = br.Id, Source = Source.Tree, Branch = br.Id, Tier = t, Passive = true,
                            NameTh = br.PasTh[t - 1], NameEn = br.PasEn[t - 1], PassKey = PassiveKey(br.Id, t), PassVal = PassiveVal(t), Fx = br.Fx };
                        Nodes[pas.Id] = pas; ById[pas.Id] = pas;
                    }
                    else
                    {
                        var mas = new Skills.Skill { Id = br.Id + "_m", Cls = br.Id, Source = Source.Tree, Branch = br.Id, Tier = 5, Passive = true,
                            NameTh = br.MasTh, NameEn = br.MasEn, PassKey = "damage_pct", PassVal = 0.2f, Fx = br.Fx };
                        Nodes[mas.Id] = mas; ById[mas.Id] = mas;
                    }
                }
            foreach (var el in Gear.Elements)
            {
                var s = SetSkill(el); ById[s.Id] = s;
            }
            foreach (var r in Arsenal.Relics)
            {
                var s = WeaponSkill(r.Id, r.Group);
                if (r.SkKind.HasValue) s.Kind = r.SkKind.Value;
                if (r.SkFx != null) s.Fx = r.SkFx;
                if (r.SkStatus != null) { s.TStatus = r.SkStatus; s.TTime = r.SkTime; }
                if (r.SkSelf != null) { s.SStatus = r.SkSelf == "drain" ? null : r.SkSelf; s.STime = 3f; if (r.SkSelf == "drain") s.HealPct = 0.15f; }
                if (r.SkMult > 0) s.Mult = r.SkMult;
                if (r.SkRadius > 0) s.Radius = r.SkRadius;
                if (r.SkHits > 0) { s.Hits = r.SkHits; if (!r.SkKind.HasValue) s.Kind = Skills.Kind.Multi; }
                if (s.Kind == Skills.Kind.Heal) { s.HealPct = 0.3f; s.Mult = 0; }
                ById[s.Id] = s;
            }
        }

        static Skills.Skill MakeActive(Branch br, int t)
        {
            var k = t == 1 ? Skills.Kind.Strike : t == 2 ? Skills.Kind.Multi : t == 3 ? Skills.Kind.Aoe : Skills.Kind.Strike;
            float range = br.Id == "archery" || br.Id == "arcane" ? 14f : br.Id == "holy" || br.Id == "nature" ? 10f : 3.5f;
            var s = new Skills.Skill { Id = br.Id + "_a" + t, Cls = br.Id, Source = Source.Tree, Branch = br.Id, Tier = t, NameTh = br.ActTh[t - 1], NameEn = br.ActEn[t - 1],
                Kind = k, Cooldown = new[] { 0, 6f, 9f, 14f, 20f }[t], Cast = new[] { 0, 0f, 0.2f, 0.4f, 0.6f }[t], Range = range, Radius = t == 3 ? 3f : 0,
                Mult = new[] { 0, 1.6f, 1.2f, 2.0f, 3.4f }[t], Hits = t == 2 ? 3 : 1, Mana = 6 + t * 5, Fx = br.Fx, Stat = br.Stat, Coef = 1f + t * 0.5f };
            switch (br.Id)
            {
                case "holy": if (t == 2) { s.Kind = Skills.Kind.Heal; s.Radius = 6f; s.HealPct = 0.2f; s.Mult = 0; } break;
                case "command": if (t <= 2) { s.Kind = Skills.Kind.BuffAllies; s.Radius = 6f; s.SStatus = t == 1 ? "motivated" : "inspired"; s.STime = 8f; s.Mult = 0; } break;
                case "shadow": if (t == 2) { s.Kind = Skills.Kind.Dash; s.Hits = 1; s.Mult = 1.9f; s.SStatus = "dodge"; s.STime = 2f; } if (t == 3) { s.TStatus = "poisoned"; s.TTime = 5f; } break;
                case "nature": if (t == 2) { s.TStatus = "frozen"; s.TTime = 1.2f; s.Kind = Skills.Kind.Strike; s.Hits = 1; } break;
                case "martial": if (t == 1) { s.TStatus = "slowness"; s.TTime = 1f; } if (t == 4) { s.TStatus = "stunned"; s.TTime = 1.5f; } break;
                case "beast": if (t == 3) { s.Kind = Skills.Kind.Around; s.TStatus = "slowness"; s.TTime = 2f; } if (t == 4) { s.SStatus = "rage"; s.STime = 6f; } break;
                case "blade": if (t == 4) { s.TStatus = "stunned"; s.TTime = 1f; } break;
                case "arcane": if (t == 4) { s.Kind = Skills.Kind.Aoe; s.Radius = 4f; } break;
            }
            return s;
        }

        static string PassiveKey(string br, int t)
        {
            switch (t)
            {
                case 1: return br == "holy" || br == "nature" || br == "beast" ? "health_pct" : "damage_pct";
                case 2: return br == "shadow" || br == "archery" || br == "blade" ? "crit" : "health_pct";
                case 3: return "cooldown";
                default: return "damage_pct";
            }
        }
        static float PassiveVal(int t) => t == 1 ? 0.08f : t == 2 ? 0.10f : t == 3 ? 0.10f : 0.15f;

        static Skills.Skill SetSkill(Gear.Element el)
        {
            string fx = FxOf(el.Id);
            string ts = el.Proc != null && !el.Proc.StartsWith("self:") && el.Proc != "heal" && el.Proc != "drain" && el.Proc != "star" ? el.Proc : null;
            return new Skills.Skill { Id = "set_" + el.Id, Cls = "set", Source = Source.Set, Kind = Skills.Kind.Aoe, Cooldown = 16f, Cast = 0.4f, Range = 8f, Radius = 3.5f,
                Mult = 2.4f, Fx = fx, Stat = 4, Coef = 2f, Mana = 20, TStatus = ts, TTime = Math.Max(1f, el.ProcTime), SetEl = el.Id };
        }

        static Skills.Skill WeaponSkill(string relic, string group)
        {
            var s = new Skills.Skill { Id = "wpn_" + relic, Cls = "weapon", Source = Source.Weapon, Cooldown = 12f, Cast = 0.3f, Mana = 15, Stat = 0, Coef = 2f, Relic = relic, Fx = "star" };
            switch (group)
            {
                case "sword": s.Kind = Skills.Kind.Multi; s.Hits = 3; s.Mult = 1.4f; s.Range = 4f; break;
                case "spear": s.Kind = Skills.Kind.Dash; s.Mult = 2.6f; s.Range = 8f; s.TStatus = "stunned"; s.TTime = 1f; break;
                case "axe": s.Kind = Skills.Kind.Around; s.Radius = 3.5f; s.Mult = 2.2f; s.Range = 4f; s.Fx = "blood"; break;
                case "hammer": s.Kind = Skills.Kind.Aoe; s.Radius = 3f; s.Mult = 2.4f; s.Range = 4f; s.TStatus = "stunned"; s.TTime = 1.5f; break;
                case "bow": s.Kind = Skills.Kind.Multi; s.Hits = 4; s.Mult = 1.0f; s.Range = 16f; s.Fx = "wind"; break;
                default: s.Kind = Skills.Kind.Aoe; s.Radius = 4f; s.Mult = 2.6f; s.Range = 14f; s.Stat = 3; s.Fx = "ice"; break;
            }
            return s;
        }

        public static string FxOf(string el)
        {
            switch (el)
            {
                case "fire": case "dragon": case "phoenix": return "fire";
                case "ice": case "moon": case "crystal": return "ice";
                case "void": return "dark";
                case "titan": return "nature";
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

        // ---------------- อัลติเมท: อาชีพ × ธาตุเซ็ต × อาวุธ ----------------
        static readonly Dictionary<string, (string th, string en)> ElPrefix = new Dictionary<string, (string, string)>
        {
            { "phys", ("", "") }, { "fire", ("เพลิง", "Inferno ") }, { "ice", ("เหมันต์", "Frost ") }, { "thunder", ("อัสนี", "Thunder ") },
            { "holy", ("สวรรค์", "Heavenly ") }, { "dark", ("ทมิฬ", "Abyssal ") }, { "poison", ("พิษ", "Venom ") }, { "wind", ("วายุ", "Gale ") },
            { "star", ("ดารา", "Star ") }, { "blood", ("โลหิต", "Crimson ") }, { "nature", ("พงไพร", "Verdant ") },
        };
        static readonly Dictionary<string, (string th, string en)> Core = new Dictionary<string, (string, string)>
        {
            { "swordsman", ("ดาบ", "Blade") }, { "knight", ("ป้อม", "Bastion") }, { "archer", ("ศร", "Arrow") }, { "mage", ("อาคม", "Spell") },
            { "priest", ("ปาฏิหาริย์", "Miracle") }, { "assassin", ("เงา", "Shadow") }, { "monk", ("ฝ่ามือ", "Palm") }, { "lancer", ("หอก", "Lance") },
            { "berserker", ("พิโรธ", "Fury") }, { "ranger", ("พราน", "Hunt") }, { "paladin", ("ทัณฑ์", "Judgment") }, { "spellblade", ("รูน", "Rune") },
            { "bard", ("บทเพลง", "Song") }, { "boss", ("หายนะ", "Doom") }, { "", ("อสูร", "Beast") },
            { "guardian", ("ปราการ", "Aegis") },
            { "engineer", ("จักรกล", "Machina") },
            { "necromancer", ("มรณะ", "Death") },
            { "summoner", ("ภูต", "Spirit") },
            { "druid", ("พฤกษ์", "Grove") },
            { "shaman", ("วิญญาณ", "Ancestor") },
            { "elementalist", ("ธาตุ", "Element") },
            { "samurai", ("ซามูไร", "Iaido") },
            { "ninja", ("นินจุทสึ", "Jutsu") },
            { "gladiator", ("ลานประลอง", "Arena") },
            { "dragoon", ("มังกร", "Dragon") },
            { "warlock", ("คำสาป", "Hex") },
            { "alchemist", ("แปรธาตุ", "Alchemy") },
            { "beast_tamer", ("ฝูงสัตว์", "Pack") },
            { "dancer", ("ระบำ", "Dance") },
            { "monster_hunter", ("นักล่า", "Slayer") },
            { "freelancer", ("อิสระ", "Wanderer") },
        };
        static readonly Dictionary<string, (string th, string en)> Suffix = new Dictionary<string, (string, string)>
        {
            { "sword", ("ผ่าฟ้า", "Sky Cleaver") }, { "axe", ("ทลายภูผา", "Mountain Breaker") }, { "spear", ("ทะลวงสวรรค์", "Heaven Piercer") },
            { "hammer", ("ถล่มพสุธา", "Earthshaker") }, { "bow", ("พันดาว", "Thousand Stars") }, { "staff", ("พันธนาการโลก", "World Binder") },
            { "range", ("พันธนาการโลก", "World Binder") }, { "", ("นิรันดร์", "Eternal") },
        };
        static readonly Dictionary<string, Skills.Skill> _ultCache = new Dictionary<string, Skills.Skill>();

        public static Skills.Skill UltimateFor(Actor a)
        {
            string cls = NgdData.GetClass(a) ?? "";
            if (!Core.ContainsKey(cls)) cls = "";
            var info = Gear.Info(a);
            string el = info.pieces >= 6 && info.def != null ? FxOf(info.def) : null;
            string wg = "";
            try { var w = a.equipment?.weapon?.getItem()?.getAsset(); if (w != null) wg = w.group_id ?? ""; } catch { }
            if (!Suffix.ContainsKey(wg)) wg = "";
            if (el == null) { var br = TopBranch(a); el = br != null ? Branches.First(b => b.Id == br).Fx : "phys"; }
            string key = cls + "|" + el + "|" + wg;
            if (_ultCache.TryGetValue(key, out var u)) return u;
            bool ranged = wg == "bow" || wg == "staff" || wg == "range" || cls == "mage" || cls == "archer" || cls == "ranger" || cls == "bard";
            var hash = Math.Abs(key.GetHashCode());
            string[] statuses = { "stunned", "burning", "frozen", "slowness", "poisoned" };
            u = new Skills.Skill
            {
                Id = "ult_" + key.Replace("|", "_"), Cls = cls, Source = Source.Ultimate, Ult = true, Slot = 7,
                NameTh = ElPrefix[el].th + Core[cls].th + Suffix[wg].th, NameEn = (ElPrefix[el].en + Core[cls].en + ": " + Suffix[wg].en).Trim(),
                Kind = ranged ? Skills.Kind.Aoe : (hash % 3 == 0 ? Skills.Kind.Multi : Skills.Kind.Aoe), Hits = 5,
                Cooldown = 40f, Cast = 1.2f, Range = ranged ? 16f : 6f, Radius = 4f + hash % 3, Mult = ranged ? 5f : 4.5f,
                Fx = el, Stat = ranged ? 3 : 0, Coef = 3f, Mana = 40, TStatus = statuses[hash % statuses.Length], TTime = 2f,
            };
            if (u.Kind == Skills.Kind.Multi) u.Mult = 1.4f;
            _ultCache[key] = u;
            return u;
        }

        public static bool UltUnlocked(Actor a) => NgdData.GetRank(a) >= 5 || Branches.Any(b => Knows(a, b.Id + "_m"));

        // ---------------- ข้อมูลรายตัว: แต้ม · ที่เรียนแล้ว · สล็อต ----------------
        public static int Points(Actor a) { a.data.get("ngd_sp", out int v, 0); return v; }
        public static void SetPoints(Actor a, int v) { a.data.set("ngd_sp", Math.Max(0, v)); }
        public static void AddPoints(Actor a, int v) { if (a?.data != null) SetPoints(a, Points(a) + v); }

        static HashSet<string> List(Actor a, string key)
        {
            a.data.get(key, out string s, "");
            return new HashSet<string>(string.IsNullOrEmpty(s) ? new string[0] : s.Split(','));
        }
        static void Save(Actor a, string key, IEnumerable<string> l) { a.data.set(key, string.Join(",", l)); }

        public static bool Knows(Actor a, string id) => List(a, "ngd_tree").Contains(id);
        public static HashSet<string> Learned(Actor a) => List(a, "ngd_tree");

        public static bool CanLearn(Actor a, Skills.Skill n, out string why)
        {
            why = null;
            if (Knows(a, n.Id)) { why = "known"; return false; }
            if (n.Tier > 1)
            {
                bool prev = Knows(a, n.Branch + "_a" + (n.Tier - 1)) || Knows(a, n.Branch + "_p" + (n.Tier - 1));
                if (n.Tier == 5) prev = Knows(a, n.Branch + "_a4") && Knows(a, n.Branch + "_p4");
                if (!prev) { why = "prev"; return false; }
            }
            int needRank = n.Tier - 1;   // ขั้น 1 = E · 2 = D · 3 = C · 4 = B · 5 = A
            if (NgdData.GetRank(a) < needRank) { why = "rank"; return false; }
            return true;
        }

        public static int Cost(Skills.Skill n) => n.Tier;

        /// <summary>เรียนโหนด (free = เทพประทาน ไม่ใช้แต้ม)</summary>
        public static bool Learn(Actor a, string id, bool free)
        {
            Build();
            if (!Nodes.TryGetValue(id, out var n)) return false;
            if (!CanLearn(a, n, out _)) return false;
            if (!free) { if (Points(a) < Cost(n)) return false; SetPoints(a, Points(a) - Cost(n)); }
            var l = Learned(a); l.Add(id); Save(a, "ngd_tree", l);
            a.data.set("ngd_loadout_dirty", true);
            if (n.Passive) a.setStatsDirty();
            return true;
        }

        public static void ResetTree(Actor a, bool refund)
        {
            Build();
            int back = 0;
            foreach (var id in Learned(a)) if (Nodes.TryGetValue(id, out var n)) back += Cost(n);
            a.data.set("ngd_tree", "");
            if (refund) AddPoints(a, back);
            a.data.set("ngd_loadout_dirty", true);
            a.setStatsDirty();
        }

        public static string TopBranch(Actor a)
        {
            var l = Learned(a);
            string best = null; int bestN = 0;
            foreach (var b in Branches) { int n = l.Count(x => x.StartsWith(b.Id + "_")); if (n > bestN) { bestN = n; best = b.Id; } }
            return best;
        }

        /// <summary>AI: ใช้แต้มเรียนโหนดในสายที่เข้ากับอาชีพ (active ก่อน passive ในขั้นเดียวกัน)</summary>
        public static void AutoLearn(Actor a)
        {
            Build();
            var pref = PreferredBranches(a);
            for (int guard = 0; guard < 20 && Points(a) > 0; guard++)
            {
                Skills.Skill pick = null;
                foreach (var br in pref)
                    for (int t = 1; t <= 5 && pick == null; t++)
                        foreach (var id in t < 5 ? new[] { br + "_a" + t, br + "_p" + t } : new[] { br + "_m" })
                            if (Nodes.TryGetValue(id, out var n) && CanLearn(a, n, out _) && Points(a) >= Cost(n)) { pick = n; break; }
                if (pick == null) break;
                Learn(a, pick.Id, false);
            }
        }

        // ---------------- สกิลที่ใช้ได้ทั้งหมด + สล็อต 7 ช่อง ----------------
        public static List<Skills.Skill> Available(Actor a)
        {
            Build();
            var r = new List<Skills.Skill>();
            int rank = NgdData.GetRank(a);
            var cls = Skills.Of(NgdData.GetClass(a));
            if (cls != null) r.AddRange(cls.Where(s => !s.Ult && rank >= s.Unlock));
            var info = Gear.Info(a);
            if (info.pieces >= 6 && info.def != null && ById.TryGetValue("set_" + info.def, out var ss)) r.Add(ss);
            try
            {
                var w = a.equipment?.weapon?.getItem()?.getAsset();
                if (w != null && ById.TryGetValue("wpn_" + w.id, out var ws)) r.Add(ws);
            }
            catch { }
            foreach (var id in Learned(a)) if (Nodes.TryGetValue(id, out var n) && !n.Passive) r.Add(n);
            return r;
        }

        public const int Slots = 6;

        /// <summary>สล็อต: ผู้เล่นจัดเอง (ngd_slots_manual) หรือ AI เลือก 6 สกิลที่แรงที่สุด + อัลติเมท</summary>
        public static List<Skills.Skill> Loadout(Actor a, out Skills.Skill ult)
        {
            var avail = Available(a);
            ult = null;
            if (UltUnlocked(a))
            {
                var cu = Skills.Of(NgdData.GetClass(a))?.FirstOrDefault(s => s.Ult);
                a.data.get("ngd_ult_pick", out string pick, "");
                ult = pick == "class" && cu != null ? cu : UltimateFor(a);
            }
            a.data.get("ngd_slots_manual", out bool manual, false);
            if (manual)
            {
                var chosen = List(a, "ngd_slots").Select(id => avail.FirstOrDefault(s => s.Id == id)).Where(s => s != null).Take(Slots).ToList();
                if (chosen.Count > 0) return chosen;
            }
            return avail.OrderByDescending(s => (s.Source == Source.Weapon ? 50 : s.Source == Source.Set ? 40 : 0) + s.Mult * 10 + s.Tier * 3 + s.Slot * 2)
                        .Take(Slots).ToList();
        }

        public static void ToggleSlot(Actor a, string id)
        {
            var cur = Loadout(a, out _).Select(s => s.Id).ToList();
            if (cur.Contains(id)) cur.Remove(id);
            else { if (cur.Count >= Slots) cur.RemoveAt(cur.Count - 1); cur.Add(id); }
            Save(a, "ngd_slots", cur);
            a.data.set("ngd_slots_manual", true);
        }

        public static void AutoSlots(Actor a) { a.data.set("ngd_slots_manual", false); }

        // ---------------- passive ----------------
        public static void ApplyPassives(Actor a, BaseStats s)
        {
            Build();
            float dmg = 0, hp = 0, crit = 0;
            foreach (var id in Learned(a))
                if (Nodes.TryGetValue(id, out var n) && n.Passive)
                    switch (n.PassKey)
                    {
                        case "damage_pct": dmg += n.PassVal; break;
                        case "health_pct": hp += n.PassVal; break;
                        case "crit": crit += n.PassVal * 0.5f; break;
                    }
            if (dmg > 0) s["damage"] *= 1f + dmg;
            if (hp > 0) s["health"] *= 1f + hp;
            if (crit > 0) s["critical_chance"] += crit;
        }

        public static float CooldownReduction(Actor a)
        {
            float r = 0;
            foreach (var id in Learned(a)) if (Nodes.TryGetValue(id, out var n) && n.Passive && n.PassKey == "cooldown") r += n.PassVal;
            return Math.Min(0.4f, r);
        }

        // ---------------- ชื่อสกิล ----------------
        public static bool Thai => NeoModLoader.General.LM.Get("ngd_lang") == "th";

        public static string Name(Skills.Skill s)
        {
            if (s.NameTh != null) return Thai ? s.NameTh : s.NameEn;
            if (s.Source == Source.Set)
            {
                string el = NeoModLoader.General.LM.Get("ngd_el_" + s.SetEl);
                return Thai ? "ศิลป์เซ็ต" + el : el + " Set Art";
            }
            if (s.Source == Source.Weapon)
            {
                string w = NeoModLoader.General.LM.Get("item_" + s.Relic);
                if (LocalizedTextManager.stringExists("ngd_wsk_" + s.Relic)) return NeoModLoader.General.LM.Get("ngd_wsk_" + s.Relic) + " (" + w + ")";
                return Thai ? w + ": ปลดผนึก" : w + ": Unleash";
            }
            return NeoModLoader.General.LM.Get("ngd_sk_" + s.Id);
        }

        public static string SourceName(Skills.Skill s) => NeoModLoader.General.LM.Get("ngd_src_" + s.Source);
    }
}
