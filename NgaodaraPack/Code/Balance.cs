using System;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ตัวเลขบาลานซ์ทั้งหมดอยู่ที่นี่ที่เดียว (ปรับจากผลจำลอง worldbox_mod/balance_sim.py)
    ///
    /// แนวฝึกยุทธ์อายุยืน (ผลจำลอง balance_sim2.py · โลกอายุ 1,500 ปี):
    ///   สัดส่วนที่พบในโลก: E ~20% / D ~25% / C ~25% / B ~20% / A ~6.5% / S ~2–4%
    ///   คนที่ไปถึง S จริงมีแค่ 0.2–0.4% (ส่วนใหญ่เป็นอัจฉริยะ)
    ///   อายุตอนถึง: D 31 / C 44 / B 63 / A 85 / S ~106 / S5★ ~470–500 ปี / SS ขึ้นไปเป็นพันปี
    /// </summary>
    public static class Balance
    {
        // ================= พลัง =================
        /// <summary>พลังรวมเทียบคนธรรมดา ณ ดาว 1 ของแต่ละแรงค์ (E…G) + ช่องสุดท้าย = เพดานหลัง G12</summary>
        public static readonly float[] PowerRatio = { 1.2f, 3f, 5f, 9f, 16f, 35f, 90f, 250f, 700f, 2000f, 6000f, 18000f };
        /// <summary>สัดส่วนพลังที่ไปลงเลือด (ที่เหลือลงดาเมจ): ยอดฝีมืออึดมากกว่าแรง</summary>
        public const float HealthShare = 0.55f;

        /// <summary>พลังรวมตามขั้นพลัง (rank×12 + star) ไล่แบบทวีคูณระหว่างแรงค์</summary>
        public static float Power(int pl)
        {
            if (pl <= 0) return 1f;
            int r = Mathf.Clamp((pl - 1) / 12, 0, PowerRatio.Length - 2);
            float s = ((pl - 1) % 12) / 12f;
            return PowerRatio[r] * Mathf.Pow(PowerRatio[r + 1] / PowerRatio[r], s);
        }

        public static float HealthMul(int pl) => Mathf.Pow(Power(pl), HealthShare);
        public static float DamageMul(int pl) => Mathf.Pow(Power(pl), 1f - HealthShare);

        // ================= EXP / ดาว =================
        /// <summary>EXP ต่อดาวที่ดาว 1 ของแต่ละแรงค์ (ดาว 12 = ×2) · ตั้งแต่ S กระโดดแรง ต้องฝึกเป็นร้อยปี</summary>
        public static readonly int[] ExpPerStar = { 70, 150, 300, 600, 1200, 65000, 160000, 600000, 2400000, 9000000, 36000000 };
        public static int ExpToNext(int pl)
        {
            if (pl <= 0) return ExpPerStar[0];
            int r = Mathf.Clamp((pl - 1) / 12, 0, ExpPerStar.Length - 1), st = (pl - 1) % 12;
            return (int)(ExpPerStar[r] * (1f + st / 11f));
        }

        public const int TrainBase = 4, TrainPerTier = 2;   // EXP ฝึกที่กิลด์ต่อเดือน
        public const int ExpPerKill = 12;                   // + ขั้นพลังผู้ตาย×3 + เลือดสูงสุด/40 (ไม่เกิน 300)

        /// <summary>แต้มสถานะต่อดาว: แรงค์สูงได้มากขึ้น (E–B 3 / A–S 4 / SS+ 5)</summary>
        public static int PointsPerStar(int rank) => rank >= 6 ? 12 : rank >= 5 ? 8 : rank >= 4 ? 4 : 3;   // ดาวขั้น S ได้ยากมาก แต้มจึงมากตาม
        public const int PointsPerBreakthrough = 10;

        // ================= เลเวลเกม =================
        /// <summary>เลเวลสูงสุด (ถึงแล้วแสดงคำว่า MAX)</summary>
        public const int LevelMax = 10000;
        public static string LevelText(int lv) => lv >= LevelMax ? "MAX" : lv.ToString();
        /// <summary>แต้มสถานะต่อเลเวลเกม (แจกอัตโนมัติตามอาชีพ)</summary>
        public const int PointsPerLevel = 2;

        // ================= ทะลวงแรงค์ =================
        /// <summary>โอกาสพื้นฐานทะลวงจากแรงค์นี้ไปแรงค์ถัดไป (E→D … M→G)</summary>
        public static readonly float[] BreakChance = { 0.80f, 0.60f, 0.45f, 0.25f, 0.10f, 0.03f, 0.02f, 0.012f, 0.008f, 0.004f };
        /// <summary>ประตูตำนาน: ทะลวงจาก A ขึ้นไปแล้วล้ม = เสีย 2 ดาว (ไม่ใช่แค่เสีย EXP)</summary>
        public const int LegendGateRank = 4, LegendGateStarLoss = 2;
        public const float SpiBonus = 0.01f;        // +1% (คูณ) ต่อแต้ม SPI
        public const float PreMagicS = 0.5f;        // ทะลวงสู่ S ขึ้นไปก่อนยุคเวทมนตร์: ยากขึ้นครึ่งหนึ่ง
        public const float StarEraLegend = 1.5f;    // ยุคดารา: ตำนาน (SS+) ง่ายขึ้น 1.5 เท่า
        public const float FailExpKeep = 0.7f;      // ทะลวงล้ม: เหลือ EXP 70% และบาดเจ็บ

        // ================= อายุขัย =================
        /// <summary>อายุขัยเพิ่มตามแรงค์ (ปี) แนวฝึกยุทธ์: ยิ่งแกร่งยิ่งอายุยืน</summary>
        public static readonly int[] LifespanBonus = { 0, 5, 12, 25, 70, 650, 1500, 4000, 10000, 30000, 100000 };

        // ================= ยุค =================
        public const float TechPerEra = 0.06f;      // พลังทหารเพิ่มต่อยุค
    }
}
