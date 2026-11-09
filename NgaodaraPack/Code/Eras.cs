using System;
using HarmonyLib;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ยุคแห่งอารยธรรม:
    /// - ยุคโลก: เดินตามปีในเกม (หิน → สำริด → เหล็ก → อาณาจักร → เวทมนตร์ → ดารา)
    /// - ยุคเมือง: แต่ละเมืองสะสม "แต้มวิจัย" ทุกเดือน (ประชากร การค้า กิลด์ ธนาคาร)
    ///   เมืองก้าวหน้าไปก่อนยุคโลกได้ 1 ยุค เมืองล้าหลังอยู่ยุคเก่าได้
    /// - ผล: เทคแต่ละยุคเพิ่มพลังทหาร (เลือด/ดาเมจ/เกราะ) · กิลด์เปิดยุคเหล็ก · ธนาคารเปิดยุคอาณาจักร
    ///   ทะลวงสู่ S ง่ายขึ้นในยุคเวทมนตร์ · ตำนาน SS+ เกิดง่ายขึ้นในยุคดารา
    /// </summary>
    public static class Eras
    {
        public static readonly string[] Ids = { "stone", "bronze", "iron", "kingdom", "magic", "star" };
        public static readonly int[] StartYear = { 0, 100, 250, 450, 700, 1000 };
        public static readonly int[] ResearchNeeded = { 0, 300, 1000, 2500, 5000, 9000 };
        public const int Bronze = 1, Iron = 2, Kingdom = 3, Magic = 4, Star = 5;

        /// <summary>god power เร่ง/ย้อนยุค (ปี) — บันทึกไปกับเซฟโลก</summary>
        public static int YearOffset
        {
            get => WorldData.GetInt("year_offset");
            set => WorldData.SetInt("year_offset", value);
        }
        static int _lastWorldEra = -1;

        public static int WorldYear()
        {
            try { return Date.getCurrentYear() + YearOffset; } catch { return 0; }
        }

        public static int WorldEra()
        {
            int y = WorldYear(), e = 0;
            for (int i = 0; i < StartYear.Length; i++) if (y >= StartYear[i]) e = i;
            return e;
        }

        public static int Research(City c) => Economy.Get(c, "research");

        public static int CityEra(City c)
        {
            if (c == null) return WorldEra();
            int r = Research(c), e = 0;
            for (int i = 0; i < ResearchNeeded.Length; i++) if (r >= ResearchNeeded[i]) e = i;
            // ล้าหลังยุคโลกได้ไม่เกิน 1 ยุค และก้าวหน้าเกินได้ 1 ยุค
            int w = WorldEra();
            return Mathf.Clamp(e, Mathf.Max(0, w - 1), Mathf.Min(w + 1, Ids.Length - 1));
        }

        public static int ActorEra(Actor a)
        {
            var c = a?.getCity();
            return c != null ? CityEra(c) : WorldEra();
        }

        public static string Name(int era) => NeoModLoader.General.LM.Get("ngd_era_" + Ids[Mathf.Clamp(era, 0, Ids.Length - 1)]);

        /// <summary>รายเดือน: เมืองสะสมแต้มวิจัย · ประกาศเมื่อโลกเข้ายุคใหม่</summary>
        public static void MonthlyTick()
        {
            int we = WorldEra();
            if (_lastWorldEra >= 0 && we != _lastWorldEra)
                GuildSystem.Announce(string.Format(NeoModLoader.General.LM.Get("ngd_news_era"), Name(we)));
            _lastWorldEra = we;

            foreach (var c in World.world.cities)
            {
                if (c == null || !c.isAlive()) continue;
                int before = CityEra(c);
                int pts = 1 + c.getPopulationPeople() / 8 + Economy.Get(c, "trade_in") / 25;
                if (GuildSystem.HasGuild(c)) pts += 2;
                if (Economy.HasBank(c)) pts += 2;
                if (Economy.Get(c, "crisis") > 0) pts /= 2;
                // ยุคโลกยิ่งสูง ความรู้ยิ่งแพร่เร็ว
                pts += we;
                Economy.Set(c, "research", Research(c) + pts);
                int after = CityEra(c);
                if (after > before && after >= Kingdom)
                    GuildSystem.Announce(string.Format(NeoModLoader.General.LM.Get("ngd_news_city_era"), c.name, Name(after)));
            }
        }

        /// <summary>ตัวคูณเทคโนโลยีทางทหารของยุค (+6% ต่อยุค)</summary>
        public static float TechMultiplier(Actor a) => 1f + ActorEra(a) * Balance.TechPerEra;

        /// <summary>ตัวปรับโอกาสทะลวงตามยุค</summary>
        public static float BreakthroughEraFactor(Actor a, int targetRank)
        {
            int e = ActorEra(a);
            if (targetRank >= 5 && e < Magic) return Balance.PreMagicS;          // S ก่อนยุคเวทมนตร์: ยาก
            if (targetRank >= 6 && WorldEra() >= Star) return Balance.StarEraLegend;  // ยุคดารา: ตำนานเกิดง่ายขึ้น
            return 1f;
        }
    }

    /// <summary>เทคยุคเพิ่มพลัง (ทำหลัง Patch_UpdateStats ของค่าสถานะ)</summary>
    [HarmonyPatch(typeof(Actor), "updateStats")]
    [HarmonyPriority(Priority.Low)]
    public static class Patch_EraStats
    {
        public static void Prefix(Actor __instance, out bool __state) { __state = __instance != null && __instance.isStatsDirty(); }

        public static void Postfix(Actor __instance, bool __state)
        {
            if (!__state) return;
            try
            {
                var a = __instance;
                if (a.stats == null || a.kingdom == null || !a.kingdom.isCiv()) return;
                float m = Eras.TechMultiplier(a);
                if (m <= 1f) return;
                a.stats["health"] *= m;
                a.stats["damage"] *= m;
                a.stats["armor"] += (m - 1f) * 20f;
                if (a.stats["health"] > StatCaps.Big) a.stats["health"] = StatCaps.Big;
                if (a.stats["damage"] > StatCaps.Big) a.stats["damage"] = StatCaps.Big;
            }
            catch (Exception e) { NgdMod.LogOnce("era stats: " + e.Message); }
        }
    }

    /// <summary>แถวยุค/วิจัยในหน้าต่างเมือง</summary>
    [HarmonyPatch(typeof(CityWindow), "showStatsRows")]
    public static class Patch_CityWindowEra
    {
        public static void Postfix(CityWindow __instance)
        {
            try
            {
                var c = __instance.meta_object;
                if (c == null || !c.isAlive()) return;
                int e = Eras.CityEra(c);
                int next = Mathf.Min(e + 1, Eras.ResearchNeeded.Length - 1);
                __instance.showStatRow("ngd_row_era", Eras.Name(e), "#7fd8ff", MetaType.None, -1, true, "ngd/era_" + Eras.Ids[e], null, null, true);
                __instance.showStatRow("ngd_row_research", Eras.Research(c) + " / " + Eras.ResearchNeeded[next], "#a8e8ff", MetaType.None, -1, true, "ngd/s_int", null, null, true);
            }
            catch (Exception ex) { NgdMod.LogOnce("city era rows: " + ex.Message); }
        }
    }
}
