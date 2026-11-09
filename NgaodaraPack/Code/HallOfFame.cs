using System;
using System.Collections.Generic;
using NeoModLoader.General;

namespace NgaodaraPack
{
    /// <summary>
    /// ดาวเด่น: ทุกเดือนจัดอันดับยอดฝีมือของโลก (CP สูงสุด 10 คน) และ "ดาวเด่นประจำเมือง"
    /// ดาวเด่นของเมืองได้ฉายา และแสดงในหน้าต่างเมือง · ทำเนียบโลกแสดงบนหัวหน้าต่างพงศาวดาร
    /// </summary>
    public static class HallOfFame
    {
        public static readonly List<string> Top = new List<string>();
        static readonly Dictionary<long, string> _cityStar = new Dictionary<long, string>();

        public static string CityStar(City c) => c != null && _cityStar.TryGetValue(c.getID(), out var s) ? s : null;

        public static void MonthlyTick(List<City> cities)
        {
            var all = new List<(Actor a, long cp)>();
            _cityStar.Clear();
            foreach (var c in cities)
            {
                if (!Economy.ByCity.TryGetValue(c.getID(), out var people)) continue;
                Actor best = null; long bestCp = -1;
                foreach (var a in people)
                {
                    if (!a.isAlive() || NgdData.GetRank(a) < 0) continue;
                    long cp = NgdData.CombatPower(a);
                    all.Add((a, cp));
                    if (cp > bestCp) { bestCp = cp; best = a; }
                }
                if (best != null)
                {
                    if (NgdData.GetRank(best) >= 3 && string.IsNullOrEmpty(Legends.Title(best))) Legends.GrantTitle(best);
                    _cityStar[c.getID()] = Legends.FullName(best) + "  " + NgdData.RankNames[NgdData.GetRank(best)];
                }
            }
            // บอสและมอนแรงค์สูงก็ติดทำเนียบได้ (ศัตรูของโลก)
            all.Sort((x, y) => y.cp.CompareTo(x.cp));
            Top.Clear();
            for (int i = 0; i < all.Count && i < 10; i++)
            {
                var a = all[i].a;
                var c = Classes.Get(NgdData.GetClass(a));
                Top.Add($"{i + 1}. {Legends.FullName(a)} — {NgdData.RankNames[NgdData.GetRank(a)]}{NgdData.GetStar(a)}★ " +
                        $"{(c != null ? Classes.TierName(c, NgdData.GetRank(a)) : "")}  CP {all[i].cp}" +
                        (a.getCity() != null ? $"  ({a.getCity().name})" : ""));
            }
        }

        public static string Render()
        {
            if (Top.Count == 0) return "";
            return "<color=#ffd76a><b>" + LM.Get("ngd_hall_title") + "</b></color>\n" + string.Join("\n", Top) + "\n\n";
        }
    }
}
