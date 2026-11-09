using System;
using System.Collections.Generic;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// อาชีพของคนธรรมดา (ไม่มีแรงค์ แต่มีขั้นฝีมือ): ฝึกหัด → ชำนาญ → เชี่ยวชาญ → ปรมาจารย์ → ยอดฝีมือ
    /// ได้อาชีพจากงานในเมือง (ชาวนา พราน ช่างไม้ คนงานเหมือง ช่างก่อสร้าง) หรือจากค่าสถานะที่เด่น
    /// (พ่อค้า นักปราชญ์ ช่างตีเหล็ก หมอยา ขุนนาง) ขั้นฝีมือขึ้นทุกปีตามอายุงานและค่าสถานะ
    /// ปรมาจารย์ขึ้นไปช่วยเมือง: อาหาร · วิจัย · การค้า · รักษาคน · อาวุธ · ความภักดี
    /// </summary>
    public static class Professions
    {
        public class Prof { public string Id; public int Stat; public string Icon; }

        public static readonly Prof[] All =
        {
            new Prof { Id = "farmer", Stat = 2, Icon = "pr_farmer" },
            new Prof { Id = "hunter", Stat = 1, Icon = "pr_hunter" },
            new Prof { Id = "woodcutter", Stat = 0, Icon = "pr_woodcutter" },
            new Prof { Id = "miner", Stat = 0, Icon = "pr_miner" },
            new Prof { Id = "builder", Stat = 2, Icon = "pr_builder" },
            new Prof { Id = "merchant", Stat = 6, Icon = "pr_merchant" },
            new Prof { Id = "scholar", Stat = 3, Icon = "pr_scholar" },
            new Prof { Id = "smith", Stat = 0, Icon = "pr_smith" },
            new Prof { Id = "healer", Stat = 4, Icon = "pr_healer" },
            new Prof { Id = "noble", Stat = 6, Icon = "pr_noble" },
        };

        public static Prof Get(string id) { foreach (var p in All) if (p.Id == id) return p; return null; }
        public static string Of(Actor a) { a.data.get("ngd_prof", out string v, ""); return v; }
        public static int Level(Actor a) { a.data.get("ngd_prof_lv", out int v, 0); return v; }   // 0..4

        static string FromJob(Actor a)
        {
            var job = a.citizen_job?.id;
            switch (job)
            {
                case "farmer": return "farmer";
                case "hunter": return "hunter";
                case "woodcutter": return "woodcutter";
                case "miner": case "miner_deposit": return "miner";
                case "builder": case "road_builder": return "builder";
            }
            return null;
        }

        static string FromStats(Actor a)
        {
            int best = -1; string id = "farmer";
            foreach (var p in new[] { "merchant", "scholar", "smith", "healer", "noble" })
            {
                int v = NgdData.GetStat(a, Get(p).Stat) + UnityEngine.Random.Range(0, 4);
                if (v > best) { best = v; id = p; }
            }
            return id;
        }

        /// <summary>รายเดือน (เรียกจาก Economy): ให้อาชีพ · ฝึกฝีมือ · ผลต่อเมือง</summary>
        public static void MonthlyTick(List<City> cities)
        {
            bool newYear = Date.getCurrentMonth() == 0;
            foreach (var c in cities)
            {
                if (!Economy.ByCity.TryGetValue(c.getID(), out var people)) continue;
                int masters = 0;
                var counts = new Dictionary<string, int>();
                foreach (var a in people)
                {
                    if (!a.isAlive() || !a.isAdult() || NgdData.GetRank(a) >= 0) continue;   // นักผจญภัยใช้แรงค์แทน
                    if (string.IsNullOrEmpty(Of(a)))
                    {
                        if (UnityEngine.Random.value > 0.15f) continue;   // ทยอยให้อาชีพ ไม่ทำทั้งเมืองพร้อมกัน
                        NgdData.EnsureStats(a);
                        a.data.set("ngd_prof", FromJob(a) ?? FromStats(a));
                    }
                    if (newYear)
                    {
                        var p = Get(Of(a));
                        int lv = Level(a);
                        float chance = 0.15f + (p != null ? NgdData.GetStat(a, p.Stat) * 0.015f : 0f) - lv * 0.03f;
                        if (lv < 4 && UnityEngine.Random.value < chance)
                        {
                            a.data.set("ngd_prof_lv", lv + 1);
                            if (lv + 1 == 4)
                            {
                                string text = string.Format(NeoModLoader.General.LM.Get("ngd_news_grandmaster"), a.getName(),
                                    NeoModLoader.General.LM.Get("ngd_prof_" + Of(a)), c.name);
                                GuildSystem.Announce(text);
                                Legends.Record(text, a);
                            }
                        }
                    }
                    if (Level(a) >= 3)
                    {
                        masters++;
                        counts[Of(a)] = (counts.TryGetValue(Of(a), out var n) ? n : 0) + 1;
                    }
                }
                ApplyMasters(c, counts, people);
                Economy.Set(c, "masters", masters);
            }
        }

        static int Count(Dictionary<string, int> d, string k) => d.TryGetValue(k, out var v) ? v : 0;

        static void ApplyMasters(City c, Dictionary<string, int> m, List<Actor> people)
        {
            int farm = Count(m, "farmer") + Count(m, "hunter");
            if (farm > 0) c.addResourcesToRandomStockpile("wheat", farm * 3);
            int wood = Count(m, "woodcutter"); if (wood > 0) c.addResourcesToRandomStockpile("wood", wood * 3);
            int mine = Count(m, "miner"); if (mine > 0) c.addResourcesToRandomStockpile("stone", mine * 2);
            int sch = Count(m, "scholar"); if (sch > 0) Economy.Set(c, "research", Economy.Get(c, "research") + sch * 4);
            int mer = Count(m, "merchant"); if (mer > 0) c.addResourcesToRandomStockpile("gold", mer * 2);
            int heal = Count(m, "healer");
            if (heal > 0)
                foreach (var a in people)
                    if (a.isAlive() && a.getHealthRatio() < 0.9f) a.restoreHealth((int)(a.getMaxHealth() * 0.1f * heal));
            int smith = Count(m, "smith");
            if (smith > 0 && UnityEngine.Random.value < 0.1f * smith)
            {   // ช่างตีเหล็กระดับปรมาจารย์: ตีอาวุธเข้าคลังเมือง
                try
                {
                    var asset = AssetManager.items.get(Eras.CityEra(c) >= Eras.Kingdom ? "sword_iron" : "sword_bronze")
                                ?? AssetManager.items.get("sword_wood");
                    if (asset != null)
                    {
                        var item = World.world.items.generateItem(asset, c.kingdom, c.name, 1, null, 0, false);
                        if (item != null && c.data.equipment != null) c.data.equipment.addItem(c, item, c.data.equipment.item_storage_weapons);
                    }
                }
                catch (Exception e) { NgdMod.LogOnce("smith: " + e.Message); }
            }
            Economy.Set(c, "nobles", Count(m, "noble"));
        }

        public static string TierKey(int lv) => "ngd_prof_tier_" + Mathf.Clamp(lv, 0, 4);
    }
}
