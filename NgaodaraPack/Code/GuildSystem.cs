using System;
using System.Collections.Generic;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// กิลด์นักผจญภัยประจำเมือง (สไตล์อนิเมะต่างโลก) ทำงานเองทั้งหมด:
    /// - ลงทะเบียน: คนหนุ่มสาวในเมืองตื่นพลังและให้กิลด์วัดแรงค์ (สูงสุด S)
    /// - กระดานเควส: กิลด์ตรวจหามอนสเตอร์รอบเมือง แล้วออกเควสล่า
    /// - ปาร์ตี้: เลือกสมาชิก 1–4 คนที่แรงค์เหมาะกับมอน ออกไปล่าเอง
    /// - สำเร็จ: ได้ EXP ตามแรงค์มอน · ชื่อเสียงกิลด์เพิ่ม · ประกาศเมื่อเป็นเควสใหญ่
    /// มอนสเตอร์ก็มีแรงค์ วัดจากพลังชีวิต/ดาเมจ
    /// </summary>
    public static class GuildSystem
    {
        public static bool Enabled = true;
        public const int MinPopulation = 10;     // เมืองต้องมีคนพอถึงจะมีกิลด์
        const float SearchRadius = 45f;

        public class Quest
        {
            public Actor Monster;
            public List<Actor> Party = new List<Actor>();
            public float Started;
            public int MonsterRank;
        }

        static readonly Dictionary<long, Quest> _quests = new Dictionary<long, Quest>();
        static readonly Dictionary<long, List<Actor>> _members = new Dictionary<long, List<Actor>>();
        static float _timer;

        public static bool HasGuild(City c) => c != null && c.isAlive() && c.getPopulationPeople() >= MinPopulation && Eras.CityEra(c) >= 0;

        public static Quest GetQuest(City c) => c != null && _quests.TryGetValue(c.getID(), out var q) ? q : null;

        public static int Fame(City c) { if (c == null) return 0; c.data.get("ngd_guild_fame", out int v, 0); return v; }
        static void AddFame(City c, int v)
        {
            int before = GuildTier(c);
            c.data.set("ngd_guild_fame", Fame(c) + v);
            int after = GuildTier(c);
            if (after > before)
            {
                string text = LMf("ngd_news_guild_tier", c.name, NeoModLoader.General.LM.Get("ngd_guild_tier_" + after));
                Announce(text);
                Legends.Record(text, null, c);
            }
        }

        /// <summary>ระดับกิลด์ตามชื่อเสียง: ทองแดง · เงิน · ทอง · มิธริล · โอริคัลคุม · ดารา</summary>
        public static readonly int[] TierFame = { 0, 15, 45, 100, 200, 400 };
        public static int GuildTier(City c)
        {
            int f = Fame(c), t = 0;
            for (int i = 0; i < TierFame.Length; i++) if (f >= TierFame[i]) t = i;
            return t;
        }

        // ---------- แรงค์มอนสเตอร์ ----------
        public static bool IsMonster(Actor a)
        {
            if (a == null || !a.isAlive() || a.kingdom == null) return false;
            if (Cult.IsExposed(a)) return true;            // สาวกลัทธิที่ถูกเปิดโปง
            if (a.kingdom.isCiv()) return false;
            return a.getMaxHealth() >= 60 || a.stats["damage"] >= 12;   // ไม่นับสัตว์ตัวเล็กไม่มีพิษภัย
        }

        public static void AssessMonster(Actor m)
        {
            if (NgdData.GetRank(m) >= 0) return;
            float p = m.getMaxHealth() + m.stats["damage"] * 8f;
            int r = p < 150 ? 0 : p < 300 ? 1 : p < 600 ? 2 : p < 1100 ? 3 : p < 2000 ? 4 : p < 4000 ? 5 : p < 9000 ? 6 : 7;
            NgdData.EnsureStats(m);
            NgdData.SetRank(m, r);
            NgdData.SetStar(m, 1 + (int)Mathf.Clamp(p / 500f, 0, 11));
        }

        // ---------- รอบการทำงาน ----------
        public static void Tick(float dt)
        {
            if (!Enabled || World.world == null || World.world.units == null) return;
            _timer += dt;
            if (_timer < 4f) return;
            _timer = 0;
            try
            {
                CollectMembers();
                try { Rifts.Leash(); } catch (Exception e) { NgdMod.LogOnce("leash: " + e.Message); }
                foreach (var city in World.world.cities)
                {
                    if (!HasGuild(city)) continue;
                    Register(city);
                    UpdateQuest(city);
                }
            }
            catch (Exception e) { NgdMod.LogOnce("guild tick: " + e); }
        }

        static void CollectMembers()
        {
            foreach (var l in _members.Values) l.Clear();
            foreach (var a in World.world.units)
            {
                if (a == null || !a.isAlive()) continue;
                var c = a.getCity();
                if (c == null) continue;
                if (!_members.TryGetValue(c.getID(), out var l)) _members[c.getID()] = l = new List<Actor>();
                l.Add(a);
            }
        }

        /// <summary>ลงทะเบียน: ผู้ใหญ่ที่ยังไม่มีแรงค์ มีโอกาสมาสมัครกิลด์ทีละน้อย (คนค่าพลังสูงสมัครง่ายกว่า)</summary>
        static void Register(City city)
        {
            if (!_members.TryGetValue(city.getID(), out var people)) return;
            int adventurers = 0;
            foreach (var a in people) if (NgdData.GetRank(a) >= 0) adventurers++;
            // กิลด์ยิ่งดังยิ่งดึงดูดนักผจญภัย: 10% ของเมือง (ทองแดง) → 27% (ดารา)
            int cap = Math.Max(2, people.Count * (3 + GuildTier(city)) / 30);
            if (adventurers >= cap) return;
            foreach (var a in people)
            {
                bool joinable = NgdData.GetRank(a) < 0 || NgdData.GetClass(a) == "freelancer" || string.IsNullOrEmpty(NgdData.GetClass(a));
                if (!joinable || Progression.InGuild(a) || !a.isAdult() || a.isKing() || a.isCityLeader()) continue;
                a.data.get("ngd_guild_choice", out int choice, 0);   // 0 = ยังไม่ตัดสินใจ · 1 = อยากเข้า · 2 = ไม่เข้ากิลด์
                if (choice == 0) { choice = UnityEngine.Random.value < 0.7f ? 1 : 2; a.data.set("ngd_guild_choice", choice); }
                if (choice == 2) continue;
                NgdData.EnsureStats(a);
                float chance = 0.01f + (NgdData.IsGenius(a) ? 0.08f : 0f);
                if (UnityEngine.Random.value > chance) continue;
                Progression.Awaken(a, true);
                int r = NgdData.GetRank(a);
                if (r >= 3) Announce(LMf("ngd_news_register", a.getName(), city.name, NgdData.RankNames[r]));
                if (++adventurers >= cap) break;
            }
        }

        static void UpdateQuest(City city)
        {
            var q = GetQuest(city);
            if (q != null)
            {
                q.Party.RemoveAll(p => p == null || !p.isAlive());
                if (q.Monster == null || !q.Monster.isAlive())
                {
                    Complete(city, q);
                    _quests.Remove(city.getID());
                    return;
                }
                q.Started += 4f;   // นับเวลาเกม (Tick ทุก 4 วินาทีขณะไม่หยุด)
                if (q.Party.Count == 0 || q.Started > 150f)
                {
                    _quests.Remove(city.getID());   // ปาร์ตี้ล่มหรือหมดเวลา
                    return;
                }
                foreach (var p in q.Party) Engage(p, q.Monster);
                // สาวกลัทธิอยู่ฝ่ายเดียวกัน เกมจึงไม่ให้ตีกัน: ปาร์ตี้เข้าถึงตัวแล้ว "จับกุม"
                if (Cult.IsExposed(q.Monster))
                    foreach (var p in q.Party)
                        if (Vector2.Distance(p.current_position, q.Monster.current_position) < 4f)
                        {
                            q.Monster.data.set("ngd_cult", false);
                            q.Monster.getHit(q.Monster.getMaxHealth() * 10f, true, AttackType.Weapon, p, false, false, false);
                            break;
                        }
                return;
            }

            var cityTile = city.getTile(false);
            if (cityTile == null) return;
            Actor target = null;
            float best = float.MaxValue;
            // ค่าหัวและบอสมาก่อน (มองไกลขึ้น 1.5 เท่า) แล้วจึงเป็นมอนใกล้สุด
            foreach (var m in Finder.getUnitsFromChunk(cityTile, 6, SearchRadius * 1.5f, false))
            {
                if (!IsMonster(m)) continue;
                float d = Vector2.Distance(new Vector2(m.current_tile.x, m.current_tile.y), new Vector2(cityTile.x, cityTile.y));
                int bounty = Rifts.Bounty(m);
                if (bounty <= 0 && d > SearchRadius) continue;
                float score = d - bounty * 0.5f - (Rifts.IsBoss(m) ? 40f : 0f);
                if (score < best) { best = score; target = m; }
            }
            if (target == null) return;
            AssessMonster(target);
            int mr = NgdData.GetRank(target);
            // มอนแรงค์ B ขึ้นไปที่ยังไม่มีค่าหัว: เมืองตั้งค่าหัว
            if (mr >= 3 && Rifts.Bounty(target) <= 0) Rifts.PostBounty(city, target);

            if (!_members.TryGetValue(city.getID(), out var people)) return;
            var cands = new List<Actor>();
            foreach (var a in people)
                if (NgdData.GetRank(a) >= 0 && a.isAdult() && !a.isKing() && !Cult.IsCultist(a) && a != target && a.getHealthRatio() > 0.6f) cands.Add(a);
            if (cands.Count == 0) return;
            // ปาร์ตี้: เรียงจากแรงค์ใกล้มอนที่สุด (ไม่ส่งเด็กใหม่ไปตายกับมอนแรงค์สูง)
            cands.Sort((x, y) => Math.Abs(NgdData.PowerLevel(x) - mr * 12).CompareTo(Math.Abs(NgdData.PowerLevel(y) - mr * 12)));
            int size = Math.Min(cands.Count, Mathf.Clamp(1 + mr + (Rifts.IsBoss(target) ? 2 : 0), 1, 6));
            q = new Quest { Monster = target, Started = 0f, MonsterRank = mr };
            for (int i = 0; i < size; i++) q.Party.Add(cands[i]);
            _quests[city.getID()] = q;
            foreach (var p in q.Party) Engage(p, target);
        }

        /// <summary>สมาชิกปาร์ตี้เควสเดียวกับ actor (ใช้แบ่งค่าหัว)</summary>
        public static List<Actor> PartyOf(Actor a)
        {
            foreach (var q in _quests.Values)
                if (q.Party.Contains(a)) return new List<Actor>(q.Party.FindAll(p => p != null && p.isAlive()));
            return new List<Actor>();
        }

        static void Engage(Actor p, Actor m)
        {
            try
            {
                if (Cult.IsExposed(m))
                {   // คนในเมืองเดียวกัน: เดินไปหาแทนการโจมตี
                    p.goTo(m.current_tile, false, false, false, 0);
                    return;
                }
                if (p.has_attack_target && p.attack_target == m) return;
                p.setAttackTarget(m);
                p.setTask("fighting", true, false, true);
            }
            catch (Exception e) { NgdMod.LogOnce("guild engage: " + e.Message); }
        }

        static void Complete(City city, Quest q)
        {
            int mr = Math.Max(0, q.MonsterRank);
            int exp = (30 + mr * 40) * (10 + GuildTier(city)) / 10;   // กิลด์ระดับสูงสอนเก่งกว่า
            foreach (var p in q.Party) Progression.AddExp(p, exp);
            AddFame(city, 1 + mr);
            if (mr >= 3 && q.Party.Count > 0)
                Announce(LMf("ngd_news_quest", city.name, NgdData.RankNames[Math.Min(mr, NgdData.RankNames.Length - 1)], q.Party[0].getName()));
        }

        static string LMf(string key, params object[] args)
        {
            try { return string.Format(NeoModLoader.General.LM.Get(key), args); } catch { return key; }
        }

        public static void Announce(string text)
        {
            try { WorldTip.showNow(text, false, "top", 4f); } catch { }
            NgdMod.Log(text);
        }
    }

    /// <summary>ตัวจับเวลาของมอด (กิลด์ ฯลฯ) ทำงานเฉพาะตอนเกมไม่หยุด</summary>
    public class NgdTicker : MonoBehaviour
    {
        bool _prepared;
        static NgdTicker _inst;
        void Awake() { _inst = this; }

        /// <summary>ทำงานหลังเวลาจริงผ่านไป (ใช้รอหน้าต่างเกมปิดก่อนเปิดหน้าต่างใหม่)</summary>
        public static void Later(float seconds, Action act)
        {
            if (_inst == null) { act(); return; }
            _inst.StartCoroutine(_inst.Run(seconds, act));
        }

        System.Collections.IEnumerator Run(float s, Action act)
        {
            yield return new WaitForSecondsRealtime(s);
            try { act(); } catch (Exception e) { NgdMod.LogOnce("later: " + e.Message); }
        }
        void Update()
        {
            if (World.world == null) return;
            if (!_prepared && Time.realtimeSinceStartup > 5f)
            {
                _prepared = true;
                try { Legends.EnsureWindow(); } catch (Exception e) { NgdMod.LogOnce("prepare windows: " + e.Message); }
                try { CityLook.Preload(); } catch (Exception e) { NgdMod.LogOnce("city look: " + e.Message); }
                try { WorldGen.Ensure(); } catch (Exception e) { NgdMod.LogOnce("worldgen window: " + e.Message); }
            }
            bool paused = false;
            try { paused = World.world.isPaused(); } catch { }
            if (paused) return;
            GuildSystem.Tick(Time.deltaTime);
            Economy.Tick();
            Skills.Tick(Time.deltaTime);
        }
    }
}
