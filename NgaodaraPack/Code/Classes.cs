using System;

namespace NgaodaraPack
{
    public class ClassDef
    {
        public string Id;
        public string Name;
        public string[] Tiers;     // ชื่อขั้นอาชีพ ตามแรงค์ E–C · B–A · S · SS+
        public int[] Weights;      // น้ำหนักการลงแต้ม STR AGI VIT INT SPI LUK CHA
        public bool Combat;
    }

    public static class Classes
    {
        public static readonly ClassDef[] All =
        {
            new ClassDef { Id = "swordsman", Name = "Swordsman", Combat = true,
                Tiers = new[] { "Swordsman", "Blade Adept", "Sword Master", "Sword Sovereign" }, Weights = new[] { 4, 3, 2, 0, 1, 1, 0 } },
            new ClassDef { Id = "knight", Name = "Knight", Combat = true,
                Tiers = new[] { "Knight", "Vanguard", "Paladin Lord", "Aegis Sovereign" }, Weights = new[] { 3, 1, 4, 0, 1, 0, 2 } },
            new ClassDef { Id = "archer", Name = "Archer", Combat = true,
                Tiers = new[] { "Archer", "Sharpshooter", "Bow Saint", "Star Piercer" }, Weights = new[] { 1, 4, 1, 0, 1, 3, 0 } },
            new ClassDef { Id = "mage", Name = "Mage", Combat = true,
                Tiers = new[] { "Mage", "Arcanist", "Archmage", "Star Sage" }, Weights = new[] { 0, 1, 1, 4, 4, 1, 0 } },
            new ClassDef { Id = "priest", Name = "Priest", Combat = true,
                Tiers = new[] { "Priest", "Bishop", "Saint", "Heaven's Voice" }, Weights = new[] { 0, 0, 2, 2, 4, 1, 3 } },
            new ClassDef { Id = "assassin", Name = "Assassin", Combat = true,
                Tiers = new[] { "Assassin", "Shadowblade", "Night Reaper", "Void Phantom" }, Weights = new[] { 2, 4, 0, 1, 0, 4, 0 } },
            new ClassDef { Id = "monk", Name = "Martial Artist", Combat = true,
                Tiers = new[] { "Martial Artist", "Iron Fist", "Grandmaster", "Heaven Breaker" }, Weights = new[] { 4, 2, 3, 0, 2, 0, 0 } },
            new ClassDef { Id = "lancer", Name = "Lancer", Combat = true,
                Tiers = new[] { "Lancer", "Dragoon", "Sky Piercer", "Heaven Lance" }, Weights = new[] { 3, 3, 2, 0, 1, 1, 0 } },
            new ClassDef { Id = "berserker", Name = "Berserker", Combat = true,
                Tiers = new[] { "Berserker", "Bloodrager", "Warlord of Fury", "Calamity" }, Weights = new[] { 5, 1, 3, 0, 0, 1, 0 } },
            new ClassDef { Id = "ranger", Name = "Ranger", Combat = true,
                Tiers = new[] { "Ranger", "Pathfinder", "Wild Warden", "Forest Sovereign" }, Weights = new[] { 1, 4, 2, 1, 1, 2, 0 } },
            new ClassDef { Id = "paladin", Name = "Paladin", Combat = true,
                Tiers = new[] { "Paladin", "Holy Knight", "Crusader Lord", "Radiant Saint" }, Weights = new[] { 3, 0, 3, 1, 3, 0, 2 } },
            new ClassDef { Id = "spellblade", Name = "Spellblade", Combat = true,
                Tiers = new[] { "Spellblade", "Rune Knight", "Arcane Edge", "Astral Blade" }, Weights = new[] { 3, 2, 1, 3, 2, 0, 0 } },
            new ClassDef { Id = "bard", Name = "Bard", Combat = true,
                Tiers = new[] { "Bard", "Skald", "Maestro", "Voice of Stars" }, Weights = new[] { 0, 2, 1, 2, 2, 2, 4 } },
            // ---- อาชีพเพิ่ม (รวม 30) ----
            new ClassDef { Id = "guardian", Name = "Guardian", Combat = true,
                Tiers = new[] { "Guardian", "Ironwall", "Living Bastion", "Aegis of Ages" }, Weights = new[] { 2, 0, 5, 0, 2, 0, 2 } },
            new ClassDef { Id = "engineer", Name = "Engineer", Combat = true,
                Tiers = new[] { "Engineer", "Steamwright", "Gear Master", "Titan Maker" }, Weights = new[] { 1, 2, 1, 4, 0, 2, 1 } },
            new ClassDef { Id = "necromancer", Name = "Necromancer", Combat = true,
                Tiers = new[] { "Necromancer", "Bone Caller", "Lich Lord", "Emperor of Death" }, Weights = new[] { 0, 0, 1, 4, 3, 2, 0 } },
            new ClassDef { Id = "summoner", Name = "Summoner", Combat = true,
                Tiers = new[] { "Summoner", "Pactbinder", "Spirit Lord", "Star Gate" }, Weights = new[] { 0, 0, 1, 3, 4, 1, 2 } },
            new ClassDef { Id = "druid", Name = "Druid", Combat = true,
                Tiers = new[] { "Druid", "Grove Keeper", "Wild Sage", "Heart of Gaia" }, Weights = new[] { 0, 1, 2, 2, 4, 1, 1 } },
            new ClassDef { Id = "shaman", Name = "Shaman", Combat = true,
                Tiers = new[] { "Shaman", "Spirit Talker", "Storm Shaman", "Voice of Ancestors" }, Weights = new[] { 1, 0, 2, 2, 4, 1, 2 } },
            new ClassDef { Id = "elementalist", Name = "Elementalist", Combat = true,
                Tiers = new[] { "Elementalist", "Element Warden", "Element Lord", "Primordial" }, Weights = new[] { 0, 1, 0, 5, 3, 1, 0 } },
            new ClassDef { Id = "samurai", Name = "Samurai", Combat = true,
                Tiers = new[] { "Samurai", "Iaido Master", "Sakura Saint", "Undefeated Blade" }, Weights = new[] { 4, 4, 1, 0, 1, 0, 0 } },
            new ClassDef { Id = "ninja", Name = "Ninja", Combat = true,
                Tiers = new[] { "Ninja", "Chunin", "Jonin", "Nameless Shadow" }, Weights = new[] { 1, 5, 0, 1, 0, 3, 0 } },
            new ClassDef { Id = "gladiator", Name = "Gladiator", Combat = true,
                Tiers = new[] { "Gladiator", "Arena Champion", "Crowd's Darling", "Colosseum King" }, Weights = new[] { 4, 1, 4, 0, 0, 1, 2 } },
            new ClassDef { Id = "dragoon", Name = "Dragoon", Combat = true,
                Tiers = new[] { "Dragoon", "Sky Leaper", "Wyrm Rider", "Dragon Sovereign" }, Weights = new[] { 3, 3, 2, 0, 1, 0, 1 } },
            new ClassDef { Id = "warlock", Name = "Warlock", Combat = true,
                Tiers = new[] { "Warlock", "Pact Keeper", "Hexmaster", "Heir of the Void" }, Weights = new[] { 0, 0, 1, 4, 2, 3, 0 } },
            new ClassDef { Id = "alchemist", Name = "Alchemist", Combat = true,
                Tiers = new[] { "Alchemist", "Brewmaster", "Philosopher", "Universe Decoder" }, Weights = new[] { 0, 2, 1, 4, 1, 3, 0 } },
            new ClassDef { Id = "beast_tamer", Name = "Beast Tamer", Combat = true,
                Tiers = new[] { "Beast Tamer", "Pack Leader", "Alpha", "King of Beasts" }, Weights = new[] { 1, 3, 2, 0, 1, 1, 4 } },
            new ClassDef { Id = "dancer", Name = "Dancer", Combat = true,
                Tiers = new[] { "Dancer", "Blade Dancer", "Moon Dancer", "Star Dancer" }, Weights = new[] { 1, 4, 1, 0, 1, 2, 3 } },
            new ClassDef { Id = "monster_hunter", Name = "Monster Hunter", Combat = true,
                Tiers = new[] { "Monster Hunter", "Veteran Hunter", "Dragonslayer", "Monsters' Nightmare" }, Weights = new[] { 3, 3, 2, 0, 0, 2, 0 } },
            new ClassDef { Id = "freelancer", Name = "Freelancer", Combat = true,
                Tiers = new[] { "Freelancer", "Wanderer", "Unbound Legend", "World Walker" }, Weights = new[] { 2, 2, 2, 1, 1, 2, 1 } },
        };

        /// <summary>อาชีพที่กิลด์มอบให้ (ทุกอาชีพยกเว้นนักผจญภัยอิสระ)</summary>
        public static bool IsGuildClass(string id) => id != "freelancer";

        public static ClassDef Get(string id)
        {
            foreach (var c in All) if (c.Id == id) return c;
            return null;
        }

        public static string TierName(ClassDef c, int rank)
        {
            if (c == null) return "-";
            int t = rank <= 2 ? 0 : rank <= 4 ? 1 : rank == 5 ? 2 : 3;
            if (rank < 0) t = 0;
            string key = "ngd_ct_" + c.Id + "_" + t;
            string loc = NeoModLoader.General.LM.Get(key);
            return loc != key ? loc : c.Tiers[t];
        }

        /// <summary>เลือกอาชีพตามค่าสถานะที่โดดเด่นที่สุด</summary>
        public static ClassDef Pick(Actor a)
        {
            ClassDef best = All[0];
            float bestScore = float.MinValue;
            var rnd = new System.Random(unchecked((int)a.data.id * 31 + 5));
            foreach (var c in All)
            {
                if (!IsGuildClass(c.Id)) continue;
                float s = 0;
                for (int i = 0; i < 7; i++) s += NgdData.GetStat(a, i) * c.Weights[i];
                s += (float)rnd.NextDouble() * 10f;
                if (s > bestScore) { bestScore = s; best = c; }
            }
            return best;
        }

        /// <summary>
        /// เลเวลเกม → ค่าสถานะ: ทุกเลเวลได้ Balance.PointsPerLevel แต้ม แจกตามน้ำหนักอาชีพ (ไม่มีอาชีพ = เท่า ๆ กัน)
        /// ลดเลเวล = หักแต้มคืนจากค่าที่สูงสุดก่อน · บันทึกเลเวลที่แจกแล้วไว้ใน ngd_lv_paid
        /// </summary>
        public static void SyncLevel(Actor a)
        {
            if (a?.data == null) return;
            int lv = Math.Max(1, a.data.level);
            a.data.get("ngd_lv_paid", out int paid, -1);
            if (paid < 0) { a.data.set("ngd_lv_paid", lv); return; }   // ครั้งแรก: ถือว่าเลเวลปัจจุบันจ่ายแล้ว (ไม่ย้อนแจก)
            if (lv == paid) return;
            int pts = (lv - paid) * (Balance.PointsPerLevel + Math.Max(0, NgdData.GetRank(a)));   // แรงค์สูง เลเวลละได้มากขึ้น
            if (lv > paid) SkillTree.AddPoints(a, lv / 10 - paid / 10);                               // ทุก 10 เลเวล แต้มสกิล +1
            a.data.set("ngd_lv_paid", lv);
            NgdData.EnsureStats(a);
            var c = Get(NgdData.GetClass(a));
            if (pts > 0)
            {
                if (c == null)
                {
                    for (int i = 0; i < 7; i++) NgdData.SetStat(a, i, NgdData.GetStatL(a, i) + pts / 7 + (i < pts % 7 ? 1 : 0));
                }
                else
                {
                    int total = 0; foreach (int w in c.Weights) total += w;
                    long given = 0;
                    for (int i = 0; i < 7; i++)
                    {
                        long g = (long)pts * c.Weights[i] / Math.Max(1, total);
                        given += g;
                        NgdData.SetStat(a, i, NgdData.GetStatL(a, i) + g);
                    }
                    if (given < pts) NgdData.SetStat(a, Best(c), NgdData.GetStatL(a, Best(c)) + (pts - given));
                }
            }
            else
            {
                long take = -pts;
                for (int guard = 0; take > 0 && guard < 64; guard++)
                {
                    int hi = 0; for (int i = 1; i < 7; i++) if (NgdData.GetStatL(a, i) > NgdData.GetStatL(a, hi)) hi = i;
                    long can = Math.Min(take, Math.Max(0, NgdData.GetStatL(a, hi) - 1));
                    if (can <= 0) break;
                    long step = Math.Max(1, Math.Min(can, take / 7 + 1));
                    NgdData.SetStat(a, hi, NgdData.GetStatL(a, hi) - step);
                    take -= step;
                }
            }
            a.setStatsDirty();
        }

        static int Best(ClassDef c) { int b = 0; for (int i = 1; i < 7; i++) if (c.Weights[i] > c.Weights[b]) b = i; return b; }

        /// <summary>AI แจกแต้มเองตามน้ำหนักของอาชีพ</summary>
        public static void AutoAllocate(Actor a)
        {
            var c = Get(NgdData.GetClass(a)) ?? All[0];
            int pts = NgdData.GetPoints(a);
            if (pts <= 0) return;
            int total = 0;
            foreach (int w in c.Weights) total += w;
            var rnd = new System.Random();
            for (int p = 0; p < pts; p++)
            {
                int roll = rnd.Next(total), acc = 0;
                for (int i = 0; i < 7; i++)
                {
                    acc += c.Weights[i];
                    if (roll < acc) { NgdData.SetStat(a, i, NgdData.GetStatL(a, i) + 1); break; }
                }
            }
            NgdData.SetPoints(a, 0);
        }
    }
}
