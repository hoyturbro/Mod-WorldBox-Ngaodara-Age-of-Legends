using System;

namespace NgaodaraPack
{
    /// <summary>EXP · ขึ้นดาว · ทะลวงแรงค์ (คอนเซปต์เดียวกับเกมเงาดารา)</summary>
    public static class Progression
    {
        public static int ExpToNext(Actor a)
        {
            return Balance.ExpToNext(NgdData.PowerLevel(a));
        }

        /// <summary>
        /// การวัดแรงค์ของกิลด์ตอนตื่นพลัง: ไม่มีใครเกิดมาแรงค์สูงเอง ทุกคนเริ่ม E
        /// ยกเว้นคนที่ค่าพลังติดตัวสูงมาก (เช่นอัจฉริยะ) กิลด์จะวัดให้แรงค์สูงขึ้นได้ แต่สูงสุดแค่ S
        /// SS ขึ้นไปคือตำนาน ได้จากการทะลวงเท่านั้น
        /// </summary>
        public const int AssessCap = 5; // S

        public static int AssessRank(Actor a)
        {
            int sum = 0;
            for (int i = 0; i < 7; i++) sum += NgdData.GetStat(a, i);
            int r = sum < 60 ? 0 : sum < 72 ? 1 : sum < 86 ? 2 : sum < 100 ? 3 : sum < 115 ? 4 : 5;
            return Math.Min(r, AssessCap);
        }

        /// <summary>ทำให้กลายเป็นสายต่อสู้ (ตื่นพลัง) และให้กิลด์วัดแรงค์</summary>
        /// <summary>
        /// ตื่นพลัง: viaGuild = ลงทะเบียนที่กิลด์ (กิลด์วัดแรงค์และจับคู่อาชีพจาก 29 อาชีพ)
        /// ไม่ผ่านกิลด์ (ตื่นพลังจากการต่อสู้): ครึ่งหนึ่งเป็นนักผจญภัยอิสระ อีกครึ่งไม่มีอาชีพ
        /// </summary>
        public static void Awaken(Actor a) => Awaken(a, true);

        public static void Awaken(Actor a, bool viaGuild)
        {
            if (NgdData.GetRank(a) >= 0)
            {   // นักผจญภัยอิสระ/ไม่มีอาชีพ มาเข้ากิลด์ทีหลัง: ได้อาชีพกิลด์
                if (viaGuild && (string.IsNullOrEmpty(NgdData.GetClass(a)) || NgdData.GetClass(a) == "freelancer"))
                { NgdData.SetClass(a, Classes.Pick(a).Id); a.data.set("ngd_guild", true); }
                return;
            }
            NgdData.EnsureStats(a);
            NgdData.SetRank(a, AssessRank(a));
            NgdData.SetStar(a, 1);
            NgdData.SetExp(a, 0);
            if (string.IsNullOrEmpty(NgdData.GetClass(a)))
            {
                if (viaGuild) { NgdData.SetClass(a, Classes.Pick(a).Id); a.data.set("ngd_guild", true); }
                else if (a.kingdom != null && a.kingdom.isCiv() && UnityEngine.Random.value < 0.5f) NgdData.SetClass(a, "freelancer");
            }
        }

        public static bool InGuild(Actor a) { a.data.get("ngd_guild", out bool v, false); return v; }

        public static void AddExp(Actor a, int amount)
        {
            if (a == null || !a.isAlive()) return;
            if (NgdData.GetRank(a) < 0) Awaken(a, false);   // ตื่นพลังจากการต่อสู้ (ไม่ผ่านกิลด์)
            if (NgdData.IsGenius(a)) amount *= 2;
            int exp = NgdData.GetExp(a) + amount;
            // ขึ้นดาวได้หลายดาวในครั้งเดียวถ้า EXP เยอะ แต่หยุดที่ดาว 12 (ต้องทะลวง)
            while (exp >= ExpToNext(a) && NgdData.GetStar(a) < NgdData.MaxStar)
            {
                exp -= ExpToNext(a);
                NgdData.SetStar(a, NgdData.GetStar(a) + 1);
                NgdData.SetPoints(a, NgdData.GetPoints(a) + Balance.PointsPerStar(NgdData.GetRank(a)));
                SkillTree.AddPoints(a, 1);                         // ขึ้นดาว: แต้มสกิล +1
                try { a.data.level = Math.Min(Balance.LevelMax, a.data.level + 1); } catch { }   // ขึ้นดาว: เลเวลเกม +1
            }
            int cap = ExpToNext(a);
            NgdData.SetExp(a, Math.Min(exp, cap));
            Classes.AutoAllocate(a);
            SkillTree.AutoLearn(a);
            if (NgdData.GetStar(a) >= NgdData.MaxStar && NgdData.GetExp(a) >= cap
                && NgdData.GetRank(a) < NgdData.RankNames.Length - 1 && NgdConfig.AutoBreakthrough)
                TryBreakthrough(a, false);
        }

        public static bool CanBreakthrough(Actor a)
        {
            return NgdData.GetRank(a) >= 0 && NgdData.GetRank(a) < NgdData.RankNames.Length - 1
                   && NgdData.GetStar(a) >= NgdData.MaxStar && NgdData.GetExp(a) >= ExpToNext(a);
        }

        public static float BreakthroughChance(Actor a)
        {
            int r = NgdData.GetRank(a);
            if (r < 0 || r >= Balance.BreakChance.Length) return 0f;
            float c = Balance.BreakChance[r] * (1f + NgdData.GetStat(a, 4) * Balance.SpiBonus + NgdData.GetStat(a, 5) * Balance.SpiBonus * 0.5f);
            c *= Eras.BreakthroughEraFactor(a, r + 1);
            return Math.Min(0.95f, c);
        }

        /// <summary>ทะลวงแรงค์ สำเร็จ: แรงค์ใหม่ดาว 1 · แต้ม +10 · เลือดเต็ม / ล้ม: เสีย EXP 30% และบาดเจ็บ</summary>
        public static bool TryBreakthrough(Actor a, bool forced)
        {
            if (!forced && !CanBreakthrough(a)) return false;
            if (NgdData.GetRank(a) >= NgdData.RankNames.Length - 1) return false;
            bool ok = forced || UnityEngine.Random.value < BreakthroughChance(a);
            if (ok)
            {
                NgdData.SetRank(a, NgdData.GetRank(a) + 1);
                NgdData.SetStar(a, 1);
                NgdData.SetExp(a, 0);
                NgdData.SetPoints(a, NgdData.GetPoints(a) + Balance.PointsPerBreakthrough);
                if (!forced) try { a.data.level = Math.Min(Balance.LevelMax, a.data.level + 5); } catch { }   // ทะลวงสำเร็จ = เลเวลเกม +5
                SkillTree.AddPoints(a, 3);                         // ทะลวงแรงค์: แต้มสกิล +3
                SkillTree.AutoLearn(a);
                Classes.AutoAllocate(a);
                a.setStatsDirty();
                a.restoreHealth(a.getMaxHealth());
                NgdMod.Log($"{a.getName()} broke through to rank {NgdData.RankNames[NgdData.GetRank(a)]}");
                if (!forced) Legends.OnRankUp(a, NgdData.GetRank(a));
                if (!forced) Gear.OnRankUp(a, NgdData.GetRank(a));
            }
            else
            {
                if (NgdData.GetRank(a) >= Balance.LegendGateRank)
                {   // ประตูตำนาน: ล้มแล้วเสีย 2 ดาว
                    NgdData.SetStar(a, Math.Max(1, NgdData.GetStar(a) - Balance.LegendGateStarLoss));
                    NgdData.SetExp(a, 0);
                }
                else NgdData.SetExp(a, (int)(NgdData.GetExp(a) * Balance.FailExpKeep));
                if (!NgdData.IsImmortal(a)) a.setHealth(Math.Max(1, a.getHealth() / 2), true);
            }
            return ok;
        }
    }

    public static class NgdConfig
    {
        public static bool AutoBreakthrough = true;
        public static int ExpPerKill = 12;
    }
}
