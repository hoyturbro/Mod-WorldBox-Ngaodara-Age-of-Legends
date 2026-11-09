using System;
using System.Collections.Generic;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ลัทธิดาราดับ (จากเนื้อเรื่องเกมเงาดารา): ลัทธิลับแทรกซึมเข้าเมือง
    /// - ตั้งแต่ยุคอาณาจักร มีโอกาสเกิดสาขาลัทธิในเมืองสุ่ม
    /// - สาวกชักชวนคนเพิ่มทุกเดือน (เมืองที่ความภักดีต่ำ ชักชวนง่ายกว่า) และทำให้เมืองไม่สงบ
    /// - กิลด์สืบสวน: นักผจญภัยที่ INT สูงมีโอกาสเปิดโปงสาวก แล้วกิลด์ส่งปาร์ตี้กวาดล้าง
    /// - ถ้าสาวกครบ 6 คน: "พิธีราตรีไร้ดาว" สาวกสังเวยตัวเองเปิดรอยแยกระดับสูงกลางเมือง
    /// </summary>
    public static class Cult
    {
        public static bool Enabled = true;
        public const int RitualSize = 8;

        public static bool IsCultist(Actor a) { if (a == null || a.data == null) return false; a.data.get("ngd_cult", out bool v, false); return v; }
        public static bool IsExposed(Actor a) { if (a == null || a.data == null) return false; a.data.get("ngd_cult_exposed", out bool v, false); return v; }

        public static int CountIn(City c)
        {
            int n = 0;
            if (Economy.ByCity.TryGetValue(c.getID(), out var l)) foreach (var a in l) if (a != null && a.isAlive() && IsCultist(a)) n++;
            return n;
        }

        public static void MonthlyTick(List<City> cities)
        {
            if (!Enabled || Eras.WorldEra() < Eras.Kingdom) return;

            // สาขาใหม่
            if (UnityEngine.Random.value < 0.015f && cities.Count > 0)
            {
                var c = cities[UnityEngine.Random.Range(0, cities.Count)];
                if (CountIn(c) == 0) Seed(c, UnityEngine.Random.Range(2, 4));
            }

            foreach (var c in cities)
            {
                if (!Economy.ByCity.TryGetValue(c.getID(), out var people)) continue;
                var cultists = people.FindAll(a => a.isAlive() && IsCultist(a));
                if (cultists.Count == 0) continue;

                // ชักชวน
                // ชักชวนได้เดือนละไม่เกิน 1 คน โอกาสตามจำนวนสาวก (เมืองไม่สงบชักชวนง่ายขึ้น)
                float conv = Math.Min(0.5f, cultists.Count * (c.getCachedLoyalty() < 0 ? 0.1f : 0.05f));
                if (UnityEngine.Random.value < conv)
                {
                    var t = people[UnityEngine.Random.Range(0, people.Count)];
                    if (t.isAlive() && !IsCultist(t) && t.isAdult() && NgdData.GetRank(t) < 4) { t.data.set("ngd_cult", true); cultists.Add(t); }
                }

                // กิลด์สืบสวน
                if (GuildSystem.HasGuild(c))
                {
                    int bestInt = 0;
                    foreach (var a in people) if (NgdData.GetRank(a) >= 0 && !IsCultist(a)) bestInt = Math.Max(bestInt, NgdData.GetStat(a, 3));
                    float find = 0.15f + bestInt * 0.015f + GuildSystem.Fame(c) * 0.005f;
                    foreach (var cu in cultists)
                        if (!IsExposed(cu) && UnityEngine.Random.value < find)
                        {
                            cu.data.set("ngd_cult_exposed", true);
                            Rifts.PostBounty(c, cu);
                        }
                }

                // พิธีราตรีไร้ดาว
                if (cultists.Count >= RitualSize) Ritual(c, cultists);
            }
        }

        static void Seed(City c, int n)
        {
            if (!Economy.ByCity.TryGetValue(c.getID(), out var people) || people.Count < 8) return;
            for (int i = 0; i < n; i++)
            {
                var a = people[UnityEngine.Random.Range(0, people.Count)];
                if (a.isAlive() && a.isAdult() && !a.isKing()) a.data.set("ngd_cult", true);
            }
        }

        static void Ritual(City c, List<Actor> cultists)
        {
            var t = c.getTile(false);
            if (t == null) return;
            foreach (var cu in cultists)
            {
                cu.data.set("ngd_cult", false);
                try { cu.getHit(cu.getMaxHealth() * 10f, true, AttackType.Divine, null, false, false, false); } catch { }
            }
            Rifts.Open(t, Mathf.Min(3, Eras.WorldEra() - 1), c);
            Economy.Set(c, "crisis", Math.Max(Economy.Get(c, "crisis"), 6));
            string text = string.Format(NeoModLoader.General.LM.Get("ngd_news_ritual"), c.name);
            GuildSystem.Announce(text);
            Legends.Record(text, null, c);
        }

        public static int LoyaltyPenalty(City c) => Math.Min(12, CountIn(c) * 2);
    }
}
