using System;

namespace NgaodaraPack
{
    /// <summary>ข้อมูลของมอดที่เก็บในตัว Actor (บันทึกลงเซฟไปกับตัวละคร)</summary>
    public static class NgdData
    {
        public static readonly string[] RankNames = { "E", "D", "C", "B", "A", "S", "SS", "SSS", "U", "M", "G" };
        public const int MaxStar = 12;

        // สีแรงค์ ตามแผ่นออกแบบใน AURA_DESIGN.md (E เทาฟ้า … G ขาว-ทอง)
        public static readonly string[] RankColors =
        {
            "#9fb4c4", "#7fbfb0", "#4fa9d8", "#3f6fd8", "#7a5fe0", "#f0c040",
            "#c070f0", "#8f7cff", "#5a2a7a", "#f08fd0", "#fff2c0"
        };

        public static readonly string[] StatKeys = { "str", "agi", "vit", "int", "spi", "luk", "cha" };
        public static readonly string[] StatNames = { "STR", "AGI", "VIT", "INT", "SPI", "LUK", "CHA" };

        const string P = "ngd_";

        // -1 = คนธรรมดาไร้พลัง (ไม่มีแรงค์ ไม่ใช่ E)
        public static int GetRank(Actor a) { a.data.get(P + "rank", out int v, -1); return v; }
        public static void SetRank(Actor a, int v) { a.data.set(P + "rank", Math.Max(-1, Math.Min(RankNames.Length - 1, v))); a.setStatsDirty(); }

        public static int GetStar(Actor a) { a.data.get(P + "star", out int v, 1); return v; }
        public static void SetStar(Actor a, int v) { a.data.set(P + "star", Math.Max(1, Math.Min(MaxStar, v))); a.setStatsDirty(); }

        public static int GetExp(Actor a) { a.data.get(P + "exp", out int v, 0); return v; }
        public static void SetExp(Actor a, int v) { a.data.set(P + "exp", Math.Max(0, v)); }

        public static int GetPoints(Actor a) { a.data.get(P + "pts", out int v, 0); return v; }
        public static void SetPoints(Actor a, int v) { a.data.set(P + "pts", Math.Max(0, v)); }

        public static bool IsImmortal(Actor a) { a.data.get(P + "immortal", out bool v, false); return v; }
        public static void SetImmortal(Actor a, bool v) { a.data.set(P + "immortal", v); }

        public static string GetClass(Actor a) { a.data.get(P + "class", out string v, ""); return v; }
        public static void SetClass(Actor a, string v) { a.data.set(P + "class", v ?? ""); a.setStatsDirty(); }

        /// <summary>เพดานค่าสถานะ (10 หลัก) — เก็บเป็นเลขแบบยาว</summary>
        public const long StatMax = 9999999999L;

        /// <summary>ค่าสถานะเต็ม (long) · เซฟเก่าที่เก็บเป็น int จะถูกอ่านมาใช้อัตโนมัติ</summary>
        public static long GetStatL(Actor a, int i)
        {
            EnsureStats(a);
            a.data.get(P + StatKeys[i] + "_l", out long v, -1L);
            if (v >= 0) return v;
            a.data.get(P + StatKeys[i], out int old, 5);
            return old;
        }

        /// <summary>ค่าสถานะสำหรับสูตรทั่วไป (ไม่เกิน int)</summary>
        public static int GetStat(Actor a, int i) => (int)Math.Min(GetStatL(a, i), int.MaxValue);

        public static void SetStat(Actor a, int i, long v)
        {
            EnsureStats(a);
            a.data.set(P + StatKeys[i] + "_l", Math.Max(1L, Math.Min(StatMax, v)));
            a.setStatsDirty();
        }

        /// <summary>ขั้นพลังรวม: แรงค์ × 12 + ดาว (ใช้เทียบความต่างแบบเกมหลัก)</summary>
        public static int PowerLevel(Actor a)
        {
            int r = GetRank(a);
            return r < 0 ? 0 : r * MaxStar + GetStar(a);
        }

        /// <summary>สุ่มค่าเริ่มต้นครั้งแรกที่ต้องใช้ บางคนเกิดมาเป็นอัจฉริยะ</summary>
        public static void EnsureStats(Actor a)
        {
            a.data.get(P + "init", out bool init, false);
            if (init) return;
            var rnd = new System.Random(unchecked((int)a.data.id * 7919 + 17));
            bool genius = rnd.NextDouble() < 0.03;
            for (int i = 0; i < StatKeys.Length; i++)
            {
                int v = 3 + rnd.Next(0, 8);
                if (genius) v += 6;
                a.data.set(P + StatKeys[i], v);
            }
            a.data.set(P + "init", true);
            if (genius) a.data.set(P + "genius", true);
        }

        public static bool IsInit(Actor a) { a.data.get(P + "init", out bool v, false); return v; }

        public static bool IsGenius(Actor a) { a.data.get(P + "genius", out bool v, false); return v; }

        public static string RankLabel(Actor a)
        {
            int r = GetRank(a);
            if (r < 0) return "-";
            return RankNames[r] + " " + new string('★', Math.Min(GetStar(a), MaxStar)) + " (" + GetStar(a) + ")";
        }

        /// <summary>CP ค่าพลังรวม คนในโลกไม่เห็น เห็นได้เฉพาะในเมนูเทพ</summary>
        public static long CombatPower(Actor a)
        {
            double sum = 0;
            for (int i = 0; i < StatKeys.Length; i++) sum += GetStatL(a, i);
            double cp = sum * 10.0 * RankMultiplier(a) + a.getMaxHealth() * 0.5;
            return (long)Math.Min(cp, 9e18);
        }

        /// <summary>ตัวคูณตามขั้นพลัง ทุกดาวเพิ่มจริง</summary>
        /// <summary>ตัวคูณดาเมจ (โตช้ากว่าเลือด ไม่ให้ยอดฝีมือ/บอสฆ่าทั้งเมืองในไม่กี่วินาที)</summary>
        public static float DamageMultiplier(Actor a) => Balance.DamageMul(PowerLevel(a));

        /// <summary>ตัวคูณเลือดตามขั้นพลัง (ตาราง Balance.PowerRatio)</summary>
        public static float RankMultiplier(Actor a) => Balance.HealthMul(PowerLevel(a));
    }
}
