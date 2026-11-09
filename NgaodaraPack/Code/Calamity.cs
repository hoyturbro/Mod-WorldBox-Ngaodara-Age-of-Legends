using System;
using System.Collections.Generic;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// มหันตภัยระดับโลก (เรด): ยุคดาราเท่านั้น หายากมาก
    /// - มังกรแรงค์ SSS ฉายา "มหันตภัย" ปรากฏใกล้เมืองใหญ่ที่สุด เลือดและพลังมหาศาล
    /// - ทุกกิลด์ในรัศมี 150 ช่องส่งยอดฝีมือแรงค์ A ขึ้นไป เมืองละ 2 คน มาร่วมเรด
    /// - ปราบได้: ทุกคนที่ร่วมได้ EXP ก้อนใหญ่ · ผู้ปิดฉากได้ฉายาและเซ็ตดารา · จารึกในพงศาวดาร
    /// </summary>
    public static class Calamity
    {
        public static Actor Current;
        static readonly List<Actor> _raid = new List<Actor>();

        public static void MonthlyTick(List<City> cities)
        {
            if (Current != null && !Current.isAlive()) { Finish(); return; }
            if (Current != null) { CallRaid(cities); return; }
            if (Eras.WorldEra() < Eras.Star || UnityEngine.Random.value > 0.012f) return;
            City biggest = null;
            foreach (var c in cities) if (biggest == null || c.getPopulationPeople() > biggest.getPopulationPeople()) biggest = c;
            if (biggest != null) Spawn(biggest);
        }

        public static bool Spawn(City near)
        {
            var ct = near?.getTile(false);
            if (ct == null) return false;
            for (int i = 0; i < 30; i++)
            {
                float ang = UnityEngine.Random.value * Mathf.PI * 2f;
                var t = World.world.GetTile(ct.x + (int)(Mathf.Cos(ang) * 30), ct.y + (int)(Mathf.Sin(ang) * 30));
                if (t == null || t.Type == null || !t.Type.ground || t.Type.liquid) continue;
                var id = AssetManager.actor_library.has("dragon") ? "dragon" : "demon";
                var b = World.world.units.spawnNewUnit(id, t, true, true, 3f, null, false, true);
                if (b == null) return false;
                b.data.set("ngd_boss", true);
                b.data.set("ngd_calamity", true);
                NgdData.EnsureStats(b);
                NgdData.SetRank(b, 7);
                NgdData.SetStar(b, 12);
                for (int s = 0; s < 7; s++) NgdData.SetStat(b, s, NgdData.GetStatL(b, s) + 60);
                NgdData.SetClass(b, "boss");
                b.data.set("ngd_title", "Calamity " + Legends.MakeTitle(true));
                b.setStatsDirty();
                Rifts.PostBounty(near, b);
                Current = b;
                try { MapBox.spawnLightningBig(t, 1f, null); } catch { }
                string text = string.Format(NeoModLoader.General.LM.Get("ngd_news_calamity"), Legends.FullName(b), near.name);
                GuildSystem.Announce(text);
                Legends.Record(text, b);
                CallRaid(new List<City>(World.world.cities));
                return true;
            }
            return false;
        }

        /// <summary>เรียกยอดฝีมือจากทุกกิลด์ใกล้ ๆ มาร่วมเรด</summary>
        static void CallRaid(List<City> cities)
        {
            if (Current == null || !Current.isAlive()) return;
            var bt = Current.current_tile;
            foreach (var c in cities)
            {
                if (c == null || !c.isAlive() || !GuildSystem.HasGuild(c)) continue;
                var ct = c.getTile(false);
                if (ct == null || Vector2.Distance(new Vector2(ct.x, ct.y), new Vector2(bt.x, bt.y)) > 150f) continue;
                if (!Economy.ByCity.TryGetValue(c.getID(), out var mem)) continue;
                var best = mem.FindAll(a => a.isAlive() && NgdData.GetRank(a) >= 4 && !Cult.IsCultist(a));
                best.Sort((x, y) => NgdData.CombatPower(y).CompareTo(NgdData.CombatPower(x)));
                for (int i = 0; i < best.Count && i < 2; i++)
                {
                    var a = best[i];
                    if (!_raid.Contains(a)) _raid.Add(a);
                    try { a.setAttackTarget(Current); a.setTask("fighting", true, false, true); } catch { }
                }
            }
        }

        static void Finish()
        {
            foreach (var a in _raid)
                if (a != null && a.isAlive()) { Progression.AddExp(a, 800); Legends.AddDeed(a, 8); }
            string text = string.Format(NeoModLoader.General.LM.Get("ngd_news_calamity_end"), _raid.Count);
            GuildSystem.Announce(text);
            Legends.Record(text);
            _raid.Clear();
            Current = null;
        }

        /// <summary>ผู้ปิดฉากมหันตภัย (เรียกจาก Rifts.OnKill)</summary>
        public static void OnSlain(Actor killer)
        {
            if (killer == null) return;
            Legends.GrantTitle(killer);
            Gear.EquipSet(killer, "star");
        }
    }
}
