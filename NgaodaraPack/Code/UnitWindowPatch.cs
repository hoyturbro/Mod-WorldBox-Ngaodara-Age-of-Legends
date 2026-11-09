using System;
using HarmonyLib;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ผสานกับหน้าต่าง Info ของยูนิตในเกม:
    /// - เพิ่มแถว แรงค์ · อาชีพ · CP · ค่าสถานะ ลงในรายการค่าของเกม
    /// - เพิ่มปุ่มรูปดวงตาที่มุมหน้าต่าง กดแล้วเปิดเมนูเทพของตัวนั้นทันที (ไม่ต้องเลือกใหม่)
    /// </summary>
    [HarmonyPatch(typeof(UnitWindow), "showStatsRows")]
    public static class Patch_UnitWindowRows
    {
        public static void Postfix(UnitWindow __instance)
        {
            try
            {
                var a = __instance.actor;
                if (a == null || !a.isAlive()) return;
                GodMenu.LastSelected = a;
                var content = __instance.transform.Find("Background/Scroll View/Viewport/Content");
                if (content == null) content = __instance.GetComponentInChildren<UnityEngine.UI.ScrollRect>(true)?.content;
                if (content == null) NgdMod.LogOnce("unit window: content not found");
                EditorTabs.Inject(__instance, a);   // แท็บข้างแบบเกม (แทนแผงยาวที่ต้องเลื่อนลง)
                NgdData.EnsureStats(a);

                int r = NgdData.GetRank(a);
                if (r >= 0)
                {
                    __instance.showStatRow("ngd_row_rank", NgdData.RankNames[r] + "  " + NgdData.GetStar(a) + "★",
                        NgdData.RankColors[r], MetaType.None, -1, true, "ngd/r_" + NgdData.RankNames[r], null, null, true);
                    var c = Classes.Get(NgdData.GetClass(a));
                    if (c != null)
                        __instance.showStatRow("ngd_row_class", Classes.TierName(c, r), "#ffd76a", MetaType.None, -1, true,
                            "ngd/c_" + c.Id, null, null, true);
                }
                else
                {
                    __instance.showStatRow("ngd_row_rank", LM.Get("ngd_no_rank"), "#aaaaaa", MetaType.None, -1, true,
                        "ngd/star16", null, null, true);
                    var prof = Professions.Get(Professions.Of(a));
                    if (prof != null)
                        __instance.showStatRow("ngd_row_prof", LM.Get(Professions.TierKey(Professions.Level(a))) + " " + LM.Get("ngd_prof_" + prof.Id),
                            Professions.Level(a) >= 3 ? "#ffd76a" : "#ffffff", MetaType.None, -1, true, "ngd/" + prof.Icon, null, null, false);
                }

                var title = Legends.Title(a);
                if (!string.IsNullOrEmpty(title))
                    __instance.showStatRow("ngd_row_title", title + "  (" + Legends.Deeds(a) + ")", "#ffd76a", MetaType.None, -1, true, "ngd/star16", null, null, true);
                if (Cult.IsExposed(a))
                    __instance.showStatRow("ngd_row_cult", LM.Get("ngd_cultist_exposed"), "#c070f0", MetaType.None, -1, true, "ngd/p_rift", null, null, true);
                int bounty = Rifts.Bounty(a);
                if (bounty > 0)
                    __instance.showStatRow("ngd_row_bounty", bounty, "#ff9a9a", MetaType.None, -1, true, "ngd/pts", null, null, true);
                __instance.showStatRow("ngd_row_cp", Num.Short(NgdData.CombatPower(a)), "#ffd76a", MetaType.None, -1, true, "ngd/pts", null, null, true);
                for (int i = 0; i < 7; i++)
                    __instance.showStatRow("ngd_stat_" + NgdData.StatKeys[i], NgdData.GetStat(a, i), "#ffffff", MetaType.None, -1, false,
                        "ngd/s_" + NgdData.StatKeys[i], null, null, true);
            }
            catch (Exception e) { NgdMod.LogOnce("unit window rows: " + e); }
        }
    }

}
