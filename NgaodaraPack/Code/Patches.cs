using System;
using HarmonyLib;

namespace NgaodaraPack
{
    /// <summary>
    /// ค่าหลัก 7 ตัว + ตัวคูณแรงค์ แปลงลงค่าเดิมของเกม หลังเกมคำนวณค่าเสร็จ
    /// สำคัญ: Actor.updateStats ถูกเรียกแทบทุกเฟรมแต่คำนวณใหม่เฉพาะตอน stats dirty
    /// จึงต้องเพิ่มโบนัสเฉพาะรอบที่เกมล้างค่าแล้วคำนวณใหม่จริง ไม่งั้นค่าจะทบไม่หยุด
    /// </summary>
    [HarmonyPatch(typeof(Actor), "updateStats")]
    public static class Patch_UpdateStats
    {
        static void Cap(BaseStats s)
        {
            foreach (var (k, max) in StatCaps.Caps)
            {
                float v = s[k];
                if (float.IsNaN(v) || float.IsInfinity(v) || v > max) s[k] = max;
            }
        }

        public static void Prefix(Actor __instance, out bool __state)
        {
            __state = __instance != null && __instance.isStatsDirty();
        }

        public static void Postfix(Actor __instance, bool __state)
        {
            if (!__state) return;
            try
            {
                var a = __instance;
                if (a.data == null || a.stats == null || a.asset == null || !a.asset.can_be_inspected) return;
                // ยังไม่เคยถูกแตะ (ไม่มีค่าสถานะ ไม่มีแรงค์) = ไม่เขียนข้อมูลลงเซฟ ไม่ให้โบนัส
                if (!NgdData.IsInit(a) && NgdData.GetRank(a) < 0) return;
                Classes.SyncLevel(a);   // เลเวลเกมสูงขึ้น = ค่าสถานะเพิ่มตาม
                var s = a.stats;

                float str = NgdData.GetStatL(a, 0), agi = NgdData.GetStatL(a, 1), vit = NgdData.GetStatL(a, 2),
                      intel = NgdData.GetStatL(a, 3), spi = NgdData.GetStatL(a, 4), luk = NgdData.GetStatL(a, 5),
                      cha = NgdData.GetStatL(a, 6);
                float m = NgdData.RankMultiplier(a);
                int pl = NgdData.PowerLevel(a);

                s["health"] = (s["health"] + vit * 12f) * m;
                float dm = NgdData.DamageMultiplier(a);
                a.data.get("ngd_rift", out bool riftMob, false);
                if (riftMob) dm *= 0.6f;   // มอนฯ รอยแยก: อึดแต่ตีเบากว่า ให้ปาร์ตี้มีโอกาสสู้
                s["damage"] = (s["damage"] + str * 2f + intel * 1f) * dm;
                s["armor"] = s["armor"] + vit * 0.5f + pl * 0.5f;
                s["speed"] = s["speed"] + agi * 0.4f;
                s["attack_speed"] = s["attack_speed"] + agi * 1f;
                s["critical_chance"] = s["critical_chance"] + luk * 0.01f;
                s["mana"] = (s["mana"] + spi * 8f) * m;
                s["diplomacy"] = s["diplomacy"] + cha * 0.15f;
                s["intelligence"] = s["intelligence"] + intel * 0.15f;
                s["warfare"] = s["warfare"] + str * 0.05f + pl * 0.05f;

                Gear.ApplySetBonus(a, s);
                SkillTree.ApplyPassives(a, s);
                int rk = NgdData.GetRank(a);
                if (rk >= 0) s["lifespan"] += Balance.LifespanBonus[Math.Min(rk, Balance.LifespanBonus.Length - 1)];   // ยิ่งแกร่งยิ่งอายุยืน
                // ขนาดตัวโตตามขั้นพลัง (G12 ≈ +20%) อาชีพสายร่างใหญ่ +5% บอส +30%
                float scale = 1f + Math.Min(pl, 132) * 0.0015f;
                var cls = NgdData.GetClass(a);
                if (cls == "knight" || cls == "monk" || cls == "berserker") scale += 0.05f;
                if (Rifts.IsBoss(a)) scale += 0.3f;
                if (s["scale"] > 0) s["scale"] *= scale;
                Cap(s);

            }
            catch (Exception e) { NgdMod.LogOnce("updateStats: " + e); }
        }
    }

    /// <summary>
    /// เกมตัดเลือดปัจจุบันให้ไม่เกิน "ค่าสูงสุดก่อนโบนัสมอด" ตอนท้าย updateStats (เช่นคนแรงค์ S เลือด 5000 ถูกตัดเหลือ 110)
    /// แพตช์นี้รันหลังสุด: คืนเลือดเดิมก่อนคำนวณ (ไม่เกินค่าสูงสุดใหม่) · แก้จากเมนูเทพ = เลือดเต็ม
    /// </summary>
    [HarmonyPatch(typeof(Actor), "updateStats")]
    [HarmonyPriority(Priority.Last)]
    public static class Patch_KeepHealth
    {
        public static void Prefix(Actor __instance, out int __state)
        {
            __state = __instance != null && __instance.isStatsDirty() && __instance.isAlive() ? __instance.getHealth() : -1;
        }

        public static void Postfix(Actor __instance, int __state)
        {
            if (__state <= 0) return;
            try
            {
                var a = __instance;
                if (!a.isAlive() || a.getHealth() <= 0) return;   // กำลังตาย: ห้ามแตะ (setHealth จะดันขึ้นเป็น 1 = ชุบชีวิต)
                int max = a.getMaxHealth();
                a.data.get("ngd_heal_after", out bool heal, false);
                if (heal) { a.data.set("ngd_heal_after", false); a.setHealth(max, true); }
                else a.setHealth(Math.Min(Math.Max(__state, a.getHealth()), max), true);
            }
            catch (Exception e) { NgdMod.LogOnce("keep health: " + e.Message); }
        }
    }

    /// <summary>เพดานค่าในเกม: ค่าสถานะ 10 หลักต้องไม่ทำให้เลือด/ดาเมจล้น int (ล้น = ติดลบ = ตายทันที)</summary>
    public static class StatCaps
    {
        public const float Big = 1e9f;
        public static readonly (string key, float max)[] Caps =
        {
            ("health", Big), ("damage", Big), ("mana", Big), ("armor", 1e6f), ("speed", 150f), ("attack_speed", 500f),
            ("critical_chance", 1f), ("diplomacy", 1e6f), ("intelligence", 1e6f), ("warfare", 1e6f), ("stewardship", 1e6f),
            ("lifespan", 2e6f), ("range", 60f), ("targets", 30f), ("knockback", 20f), ("critical_damage_multiplier", 50f),
        };
    }

    /// <summary>ได้ EXP จากการฆ่า</summary>
    [HarmonyPatch(typeof(Actor), "newKillAction")]
    public static class Patch_Kill
    {
        public static void Postfix(Actor __instance, Actor pDeadUnit)
        {
            try
            {
                var a = __instance;
                if (a == null) return;
                bool civ = a.kingdom != null && a.kingdom.isCiv();
                if (civ || NgdData.GetRank(a) >= 0)
                {   // EXP ตามความแกร่งของผู้ตาย
                    int exp = NgdConfig.ExpPerKill;
                    if (pDeadUnit != null) exp += Math.Min(300, NgdData.PowerLevel(pDeadUnit) * 3 + pDeadUnit.getMaxHealth() / 40);
                    Progression.AddExp(a, exp);
                }
            }
            catch (Exception e) { NgdMod.LogOnce("kill: " + e); }
        }
    }

    /// <summary>อมตะ: ไม่รับดาเมจ ไม่ตาย ไม่แก่ตาย</summary>
    [HarmonyPatch(typeof(Actor), "getHit")]
    public static class Patch_GetHit
    {
        public static bool Prefix(Actor __instance) => !NgdData.IsImmortal(__instance);
    }

    [HarmonyPatch(typeof(Actor), "die")]
    public static class Patch_Die
    {
        public static bool Prefix(Actor __instance)
        {
            if (!NgdData.IsImmortal(__instance)) return true;
            __instance.restoreHealth(__instance.getMaxHealth());
            return false;
        }
    }

    [HarmonyPatch(typeof(Actor), "checkNaturalDeath")]
    public static class Patch_NaturalDeath
    {
        public static bool Prefix(Actor __instance) => !NgdData.IsImmortal(__instance);
    }

    /// <summary>จำตัวที่ผู้เล่นเลือกล่าสุด ให้ปุ่มเมนูเทพใช้ได้แม้เกมล้างการเลือกไปแล้ว</summary>
    [HarmonyPatch(typeof(SelectedUnit), "select")]
    public static class Patch_RememberSelected
    {
        public static void Postfix(Actor pActor)
        {
            if (pActor != null) GodMenu.LastSelected = pActor;
        }
    }
}
