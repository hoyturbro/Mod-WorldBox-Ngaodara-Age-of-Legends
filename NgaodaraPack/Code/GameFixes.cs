using System;
using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>ตัวเลขย่อ: 16,717,201 → 16.7M (ให้พอดีช่องของเกมที่ไม่ได้ทำมารองรับตัวเลขใหญ่จากมอด)</summary>
    public static class Num
    {
        public static string Short(double v)
        {
            double a = Math.Abs(v);
            string sign = v < 0 ? "-" : "";
            if (a < 10000) return sign + ((long)a).ToString("N0");
            string[] suf = { "k", "M", "B", "T", "Q" };
            int i = -1;
            while (a >= 1000 && i < suf.Length - 1) { a /= 1000; i++; }
            return sign + (a < 100 ? a.ToString("0.#") : a.ToString("0")) + suf[i];
        }
    }

    /// <summary>แถบของเกม (เลือด/มานา/สตามินา ฯลฯ): ตัวเลขใหญ่ย่อให้พอดีกล่อง</summary>
    [HarmonyPatch(typeof(StatBar), "setBar")]
    public static class Patch_StatBarShort
    {
        public static void Postfix(StatBar __instance, float pVal, float pMax, string pEnding, bool pUpdateText)
        {
            try
            {
                if (!pUpdateText || __instance.textField == null) return;
                if (Mathf.Max(Mathf.Abs(pVal), Mathf.Abs(pMax)) < 10000f) return;
                if (__instance._text_tween != null) __instance._text_tween.Kill();
                string end = !string.IsNullOrEmpty(pEnding) && pEnding.StartsWith("/") ? "/" + Num.Short(pMax) : (pEnding ?? "");
                __instance.textField.text = Num.Short(pVal) + end;
            }
            catch (Exception e) { NgdMod.LogOnce("statbar short: " + e.Message); }
        }
    }

    /// <summary>ปลดล็อกสิ่งมีชีวิตทั้งหมด (เฉพาะตอนเปิดมอด ไม่เขียนลงโปรไฟล์ผู้เล่น)</summary>
    [HarmonyPatch(typeof(ActorAsset), "isDebugUnlockedAll")]
    public static class Patch_UnlockAllCreatures
    {
        public static bool Enabled = true;
        public static void Postfix(ref bool __result) { if (Enabled) __result = true; }
    }

    /// <summary>ปลดล็อกอุปกรณ์ทั้งหมดในเมนู Equipment ของเกม (รวมเซ็ตเทพ/อาวุธเทพของมอด) ไม่เขียนลงโปรไฟล์</summary>
    [HarmonyPatch(typeof(ItemAsset), "isDebugUnlockedAll")]
    public static class Patch_UnlockAllItems
    {
        public static void Postfix(ref bool __result) { if (Patch_UnlockAllCreatures.Enabled) __result = true; }
    }
}
