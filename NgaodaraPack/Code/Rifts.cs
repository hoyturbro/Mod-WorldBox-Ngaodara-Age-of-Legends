using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// รอยแยกมิติ (ธีม U กฎแห่งมิติ) + บอส + ค่าหัวกิลด์
    /// - ทุกเดือนมีโอกาสเกิดรอยแยกใกล้เมืองที่มีกิลด์ (ยุคสูงยิ่งบ่อยและยิ่งอันตราย)
    /// - รอยแยกปล่อยลูกสมุนออกมาพร้อม "บอส" ที่มีแรงค์ตามยุค
    /// - เมืองตั้งค่าหัวด้วยทองในคลัง กิลด์ส่งปาร์ตี้ไปล่าก่อนเป้าหมายอื่น ค่าหัวแบ่งให้ทุกคนในปาร์ตี้
    /// - บอสตาย: ผู้ปิดฉากได้อาวุธเทพ + ฉายา (ตำนานมีชีวิต)
    /// </summary>
    public static class Rifts
    {
        public static bool Enabled = true;

        // ลูกสมุนตามระดับรอยแยก
        static readonly string[][] Minions =
        {
            new[] { "skeleton", "skeleton", "zombie" },
            new[] { "skeleton", "demon", "evil_mage" },
            new[] { "demon", "fire_elemental", "necromancer" },
            new[] { "demon", "fire_elemental", "cold_one", "necromancer" },
        };
        static readonly string[] Bosses = { "necromancer", "demon", "fire_elemental", "dragon" };

        public static bool IsBoss(Actor a) { if (a == null) return false; a.data.get("ngd_boss", out bool v, false); return v; }
        public static int Bounty(Actor a) { if (a == null) return 0; a.data.get("ngd_bounty", out int v, 0); return v; }
        public static long BountyCity(Actor a) { a.data.get("ngd_bounty_city", out long v, -1L); return v; }

        /// <summary>รายเดือน: สุ่มเปิดรอยแยก</summary>
        public static void MonthlyTick()
        {
            if (!Enabled) return;
            int era = Eras.WorldEra();
            if (era < Eras.Bronze) return;
            float chance = 0.01f + era * 0.008f;
            if (UnityEngine.Random.value > chance) return;
            var cities = new List<City>();
            foreach (var c in World.world.cities) if (c != null && c.isAlive() && GuildSystem.HasGuild(c)) cities.Add(c);
            if (cities.Count == 0) return;
            OpenNear(cities[UnityEngine.Random.Range(0, cities.Count)], -1);
        }

        /// <summary>เปิดรอยแยกใกล้เมือง (tier -1 = ตามยุค)</summary>
        public static bool OpenNear(City city, int tier)
        {
            var ct = city?.getTile(false);
            if (ct == null) return false;
            for (int tries = 0; tries < 30; tries++)
            {
                float ang = UnityEngine.Random.value * Mathf.PI * 2f;
                float dist = UnityEngine.Random.Range(30f, 45f);   // นอกเมือง
                var t = World.world.GetTile(ct.x + (int)(Mathf.Cos(ang) * dist), ct.y + (int)(Mathf.Sin(ang) * dist));
                if (t == null || t.Type == null || !t.Type.ground || t.Type.liquid || t.Type.block || t.hasBuilding()) continue;
                Open(t, tier, city);
                return true;
            }
            return false;
        }

        public static void Open(WorldTile t, int tier, City nearCity)
        {
            if (tier < 0) tier = Mathf.Clamp(Eras.WorldEra() - 1, 0, Minions.Length - 1);
            tier = Mathf.Clamp(tier, 0, Minions.Length - 1);
            try { MapBox.spawnLightningBig(t, 0.4f, null); } catch { }
            try { World.world.drop_manager.spawn(t, "dust_purple", 4f, 2f, -1); } catch { }

            int count = 3 + tier * 2;
            for (int i = 0; i < count; i++)
            {
                var id = Minions[tier][UnityEngine.Random.Range(0, Minions[tier].Length)];
                var m = Spawn(id, Near(t, 3));
                if (m == null) continue;
                m.data.set("ngd_rift", true);
                SetHome(m, t);
                GuildSystem.AssessMonster(m);
            }

            var boss = Spawn(Bosses[tier], t);
            if (boss != null)
            {
                boss.data.set("ngd_boss", true);
                boss.data.set("ngd_rift", true);
                SetHome(boss, t);
                NgdData.EnsureStats(boss);
                // แรงค์บอสตามระดับรอยแยก: B · A · S · SS(ตำนาน ยุคดาราเท่านั้น)
                int rank = Mathf.Min(3 + tier, Eras.WorldEra() >= Eras.Star && UnityEngine.Random.value < 0.3f ? 6 : 5);
                NgdData.SetRank(boss, rank);
                NgdData.SetStar(boss, UnityEngine.Random.Range(4, 13));
                for (int s = 0; s < 7; s++) NgdData.SetStat(boss, s, NgdData.GetStatL(boss, s) + 10 + tier * 8);
                boss.setStatsDirty();
                boss.data.set("ngd_title", Legends.MakeTitle(true));
                NgdData.SetClass(boss, "boss");

                if (nearCity != null) PostBounty(nearCity, boss);
                GuildSystem.Announce(string.Format(NeoModLoader.General.LM.Get("ngd_news_rift"),
                    nearCity != null ? nearCity.name : "?", NgdData.RankNames[rank], boss.getName()));
                Legends.Record(string.Format(NeoModLoader.General.LM.Get("ngd_chron_rift"), nearCity != null ? nearCity.name : "?", NgdData.RankNames[rank]), boss);
            }
        }

        static void SetHome(Actor m, WorldTile t) { m.data.set("ngd_home_x", t.x); m.data.set("ngd_home_y", t.y); }

        public const float LeashRange = 22f;

        /// <summary>
        /// สายจูงรอยแยก: มอนฯ จากรอยแยกเฝ้าอยู่รอบรอยแยก (ให้กิลด์มาปราบ) ไม่เดินไปล้างเมืองทั้งแผนที่
        /// ออกไกลเกิน 25 ช่องและไม่ได้สู้อยู่ = เดินกลับ (มหันตภัยไม่ถูกจูง)
        /// </summary>
        public static void Leash()
        {
            foreach (var m in World.world.units)
            {
                if (m == null || !m.isAlive() || m.current_tile == null) continue;
                m.data.get("ngd_rift", out bool rift, false);
                if (!rift) continue;
                m.data.get("ngd_home_x", out int hx, -1);
                m.data.get("ngd_home_y", out int hy, -1);
                if (hx < 0) continue;
                float d = Vector2.Distance(new Vector2(m.current_tile.x, m.current_tile.y), new Vector2(hx, hy));
                if (d < LeashRange) continue;
                if (m.has_attack_target && d < LeashRange * 1.6f) continue;   // ไล่ตามได้อีกนิด
                var home = World.world.GetTile(hx, hy);
                if (home == null) continue;
                try { m.clearAttackTarget(); m.cancelAllBeh(); m.goTo(home, false, false, false, 0); } catch { }
            }
        }

        static WorldTile Near(WorldTile t, int r)
        {
            return World.world.GetTile(t.x + UnityEngine.Random.Range(-r, r + 1), t.y + UnityEngine.Random.Range(-r, r + 1)) ?? t;
        }

        static Actor Spawn(string id, WorldTile t)
        {
            try
            {
                if (!AssetManager.actor_library.has(id)) return null;
                return World.world.units.spawnNewUnit(id, t, true, true, 3f, null, false, true);
            }
            catch (Exception e) { NgdMod.LogOnce("rift spawn " + id + ": " + e.Message); return null; }
        }

        /// <summary>เมืองตั้งค่าหัวจากคลังทอง (ไม่เกิน 40% ของทองที่มี)</summary>
        public static void PostBounty(City c, Actor target)
        {
            int rank = Math.Max(0, NgdData.GetRank(target));
            int want = 40 + rank * 45;
            int gold = Economy.Gold(c);
            int amount = Math.Min(want, gold * 4 / 10);
            if (amount < 10) amount = 10;   // กิลด์ออกค่าหัวขั้นต่ำเอง
            else c.takeResource("gold", amount);
            target.data.set("ngd_bounty", amount);
            target.data.set("ngd_bounty_city", c.getID());
        }

        /// <summary>ปิดฉากเป้าหมาย: แบ่งค่าหัว · บอสดรอปอาวุธเทพ · บันทึกตำนาน</summary>
        public static void OnKill(Actor killer, Actor dead)
        {
            if (killer == null || dead == null) return;
            // รางวัลเฉพาะคน (อารยธรรม) หรือนักผจญภัยที่มีแรงค์ สัตว์ป่า/มอนฯ ฆ่ากันเองไม่นับ
            bool hero = (killer.kingdom != null && killer.kingdom.isCiv()) || NgdData.GetRank(killer) >= 0;
            if (!hero || Rifts.IsBoss(killer)) return;
            int bounty = Bounty(dead);
            if (bounty > 0)
            {
                var party = GuildSystem.PartyOf(killer);
                if (party.Count == 0) party.Add(killer);
                int share = Math.Max(1, bounty / party.Count);
                foreach (var p in party) p.data.money += share;
                dead.data.set("ngd_bounty", 0);
                Legends.AddDeed(killer, 2);
            }
            if (IsBoss(dead))
            {
                dead.data.set("ngd_boss", false);
                dead.data.get("ngd_calamity", out bool calamity, false);
                if (calamity) Calamity.OnSlain(killer);          // มหันตภัย: เซ็ตดาราครบชุด
                else { Arsenal.Give(killer); Gear.Equip(killer, Gear.RandomArmorPiece()); }   // บอส: อาวุธเทพ + ชิ้นเกราะ (ไม่ทับอาวุธ)
                Progression.AddExp(killer, 200 + NgdData.GetRank(dead) * 120);
                Legends.AddDeed(killer, 5);
                Legends.GrantTitle(killer);
                string text = string.Format(NeoModLoader.General.LM.Get("ngd_news_boss_slain"), Legends.FullName(killer), dead.getName());
                GuildSystem.Announce(text);
                Legends.Record(text, killer);
            }
        }
    }

    [HarmonyPatch(typeof(Actor), "newKillAction")]
    public static class Patch_KillRift
    {
        public static void Postfix(Actor __instance, Actor pDeadUnit)
        {
            try { if (SelfTest.Active) SelfTest.Tally(__instance, pDeadUnit); Rifts.OnKill(__instance, pDeadUnit); }
            catch (Exception e) { NgdMod.LogOnce("rift kill: " + e); }
        }
    }
}
