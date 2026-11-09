using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// เศรษฐกิจ + การเมือง ทำงานทุกเดือนในเกม:
    /// - การค้าระหว่างเมือง: อาหาร วัตถุดิบ ของฟุ่มเฟือย และอาวุธ ราคาขึ้นลงตามความขาดแคลน
    ///   ค้าข้ามอาณาจักรได้ถ้าไม่ได้ทำสงครามกัน (เสียภาษีศุลกากร)
    /// - ภาษี: เมืองส่งภาษีเข้าเมืองหลวง อัตราขึ้นกับบารมี (CHA) ของกษัตริย์
    /// - ธนาคาร: ประชาชนฝากเงินได้ดอกเบี้ย ธนาคารปล่อยกู้ให้เมืองที่ขาดทอง ผิดนัดชำระ = วิกฤตการเงิน
    /// - การเมือง: ความมั่งคั่ง ภาษี วิกฤต และบารมีกษัตริย์ มีผลต่อความภักดีของเมือง (ระบบกบฏของเกมใช้ค่านี้)
    /// ทุกค่าเก็บใน data ของเมือง จึงบันทึกไปกับเซฟ
    /// </summary>
    public static class Economy
    {
        public static bool Enabled = true;
        public const float TradeRange = 140f;
        public const int BankMinPopulation = 20;
        /// <summary>ความต้องการต่อหัวจริง (ปรับจากคลังเมืองในเกมจริง: เมืองส่วนใหญ่มีของราว 1/3 ของค่า PerPerson)</summary>
        public const float NeedScale = 0.35f;

        public class Good { public string Id; public float Price, PerPerson; public int Kind; } // 0 อาหาร 1 วัตถุดิบ 2 ฟุ่มเฟือย

        public static readonly Good[] Goods =
        {
            new Good { Id = "wheat", Price = 1, PerPerson = 1.2f }, new Good { Id = "bread", Price = 2, PerPerson = 0.8f },
            new Good { Id = "fish", Price = 2, PerPerson = 0.6f }, new Good { Id = "meat", Price = 2, PerPerson = 0.6f },
            new Good { Id = "berries", Price = 1, PerPerson = 0.5f },
            new Good { Id = "wood", Price = 1, PerPerson = 0.8f, Kind = 1 }, new Good { Id = "stone", Price = 1, PerPerson = 0.6f, Kind = 1 },
            new Good { Id = "common_metals", Price = 3, PerPerson = 0.3f, Kind = 1 }, new Good { Id = "silver", Price = 5, PerPerson = 0.1f, Kind = 1 },
            new Good { Id = "leather", Price = 2, PerPerson = 0.2f, Kind = 1 }, new Good { Id = "wool", Price = 2, PerPerson = 0.2f, Kind = 1 },
            new Good { Id = "gems", Price = 8, PerPerson = 0.05f, Kind = 2 }, new Good { Id = "ale", Price = 3, PerPerson = 0.15f, Kind = 2 },
            new Good { Id = "cider", Price = 3, PerPerson = 0.15f, Kind = 2 }, new Good { Id = "honey", Price = 3, PerPerson = 0.1f, Kind = 2 },
        };

        static int _lastMonth = -1;
        static readonly Dictionary<long, List<Actor>> _byCity = new Dictionary<long, List<Actor>>();
        public static Dictionary<long, List<Actor>> ByCity => _byCity;
        static readonly Dictionary<long, WorldTile> _tiles = new Dictionary<long, WorldTile>();

        // ---------- ค่าที่เก็บในเมือง ----------
        public static int Get(City c, string k, int def = 0) { c.data.get("ngd_" + k, out int v, def); return v; }
        public static void Set(City c, string k, int v) { c.data.set("ngd_" + k, v); }
        static void Add(City c, string k, int v) { Set(c, k, Get(c, k) + v); }

        public static int Gold(City c) => c.getResourcesAmount("gold");
        static int TakeGold(City c, int n) { n = Math.Min(n, Gold(c)); if (n > 0) c.takeResource("gold", n); return n; }
        static int GiveGold(City c, int n) => n > 0 ? c.addResourcesToRandomStockpile("gold", n) : 0;

        public static bool HasBank(City c) => c.getPopulationPeople() >= BankMinPopulation && Eras.CityEra(c) >= Eras.Bronze;

        /// <summary>อัตราภาษี (%) ของอาณาจักร: กษัตริย์บารมีสูงเก็บภาษีต่ำ</summary>
        public static int TaxRate(Kingdom k)
        {
            if (k == null || !k.hasKing()) return 10;
            int cha = NgdData.GetStat(k.king, 6);
            return Mathf.Clamp(22 - cha / 2, 5, 25);
        }

        // ---------- รอบรายเดือน ----------
        /// <summary>บังคับรอบรายเดือนทันที (ใช้ทดสอบ)</summary>
        public static void ForceMonth() { _lastMonth = int.MinValue; Tick(); }

        static void Safe(string name, Action f)
        {
            try { f(); } catch (Exception e) { NgdMod.LogOnce("monthly " + name + ": " + e); }
        }

        public static void Tick()
        {
            if (!Enabled || World.world == null || World.world.cities == null) return;
            int month = Date.getCurrentMonth();
            if (month == _lastMonth) return;
            _lastMonth = month;
            try
            {
                var cities = new List<City>();
                foreach (var c in World.world.cities) if (c != null && c.isAlive() && c.kingdom != null) cities.Add(c);
                foreach (var c in cities) { Set(c, "trade_in", 0); Set(c, "trade_out", 0); }
                _byCity.Clear();
                foreach (var a in World.world.units)
                {
                    if (a == null || !a.isAlive()) continue;
                    var ac = a.getCity();
                    if (ac == null) continue;
                    if (!_byCity.TryGetValue(ac.getID(), out var l)) _byCity[ac.getID()] = l = new List<Actor>();
                    l.Add(a);
                }
                _tiles.Clear();
                foreach (var c in cities) _tiles[c.getID()] = c.getTile(false);
                foreach (var c in cities)
                {
                    var partners = new List<City>();
                    foreach (var s in cities) if (CanTrade(c, s)) partners.Add(s);
                    if (partners.Count > 0) TradeFor(c, partners); else DbgNoPartner++;
                }
                Safe("taxes", () => { foreach (var c in cities) Taxes(c); });
                Safe("bank", () => { foreach (var c in cities) if (HasBank(c)) Bank(c); });
                foreach (var c in cities) { int cr = Get(c, "crisis"); if (cr > 0) Set(c, "crisis", cr - 1); }
                Safe("eras", Eras.MonthlyTick);
                Safe("rifts", Rifts.MonthlyTick);
                Safe("cult", () => Cult.MonthlyTick(cities));
                Safe("professions", () => Professions.MonthlyTick(cities));
                Safe("halloffame", () => HallOfFame.MonthlyTick(cities));
                Safe("calamity", () => Calamity.MonthlyTick(cities));
                Safe("buildings", () => CityBuildings.MonthlyTick(cities));
                Safe("forge", () => Forge.MonthlyTick(cities));
                Safe("training", () =>
                {
                    foreach (var c in cities)
                    {
                        if (!GuildSystem.HasGuild(c) || !_byCity.TryGetValue(c.getID(), out var mem)) continue;
                        int train = Balance.TrainBase + GuildSystem.GuildTier(c) * Balance.TrainPerTier;   // ฝึกที่กิลด์ทุกเดือน
                        foreach (var a in mem) if (a.isAlive() && NgdData.GetRank(a) >= 0) Progression.AddExp(a, train);
                    }
                });
            }
            catch (Exception e) { NgdMod.LogOnce("economy: " + e); }
        }

        static float Dist(City a, City b)
        {
            if (!_tiles.TryGetValue(a.getID(), out var ta)) ta = a.getTile(false);
            if (!_tiles.TryGetValue(b.getID(), out var tb)) tb = b.getTile(false);
            if (ta == null || tb == null) return float.MaxValue;
            return Vector2.Distance(new Vector2(ta.x, ta.y), new Vector2(tb.x, tb.y));
        }

        static bool CanTrade(City a, City b)
        {
            if (a == b || a.kingdom == null || b.kingdom == null) return false;
            if (a.kingdom != b.kingdom && a.kingdom.isEnemy(b.kingdom)) return false;
            return Dist(a, b) <= TradeRange;
        }

        /// <summary>ราคาต่อหน่วยตามความขาดแคลนของผู้ซื้อ (ขาดมาก ราคาสูงสุด 3 เท่า)</summary>
        public static float PriceFor(Good g, City buyer)
        {
            float target = Math.Max(1f, buyer.getPopulationPeople() * g.PerPerson * NeedScale);
            float ratio = buyer.getResourcesAmount(g.Id) / target;
            return g.Price * Mathf.Clamp(2f - ratio, 0.5f, 3f);
        }

        public static int DbgNeed, DbgNoSeller, DbgNoGold, DbgDone, DbgNoPartner;

        public const float FoodPerPerson = 1.5f;

        public static int FoodOf(City c) { int n = 0; foreach (var g in Goods) if (g.Kind == 0) n += c.getResourcesAmount(g.Id); return n; }

        /// <summary>อาหารรวมทุกชนิด: เมืองขาดอาหารซื้อจากเมืองที่เหลือ (ขายชนิดที่มีมากที่สุด)</summary>
        static void TradeFood(City buyer, List<City> cities)
        {
            int pop = Math.Max(1, buyer.getPopulationPeople());
            float need = pop * FoodPerPerson;
            int have = FoodOf(buyer);
            if (have >= need * 0.7f) return;
            float buyerPer = have / (float)pop;
            DbgNeed++;
            City seller = null; float best = float.MaxValue;
            foreach (var s in cities)
            {
                float sn = Math.Max(1, s.getPopulationPeople()) * FoodPerPerson;
                int sf = FoodOf(s);
                if (sf < sn * 1.1f || sf / Math.Max(1f, s.getPopulationPeople()) < buyerPer * 2f + 0.3f) continue;
                float d = Dist(buyer, s);
                if (d < best) { best = d; seller = s; }
            }
            if (seller == null) { DbgNoSeller++; return; }
            Good g = null; int most = 0;
            foreach (var x in Goods) if (x.Kind == 0 && seller.getResourcesAmount(x.Id) > most) { most = seller.getResourcesAmount(x.Id); g = x; }
            if (g == null) return;
            float sNeed = Math.Max(1, seller.getPopulationPeople()) * FoodPerPerson;
            int qty = (int)Math.Min(Math.Min(need * 0.8f - have, FoodOf(seller) - sNeed * 0.9f), Math.Min(most, 60));
            Deal(buyer, seller, g, qty);
        }

        static void Deal(City buyer, City seller, Good g, int qty)
        {
            if (qty <= 0) { DbgNoGold++; return; }
            float unit = PriceFor(g, buyer);
            bool foreign = buyer.kingdom != seller.kingdom;
            int cost = Mathf.CeilToInt(qty * unit);
            int duty = foreign ? Mathf.CeilToInt(cost * 0.1f) : 0;
            int paid;
            if (Gold(buyer) < cost + duty && !foreign && g.Kind == 0)
            {   // ยุ้งฉางหลวง: เมืองในอาณาจักรเดียวกันซื้ออาหารไม่ไหว เมืองหลวงจ่ายส่วนที่ขาด ที่เหลือส่งเป็นความช่วยเหลือ
                paid = TakeGold(buyer, cost);
                var cap = buyer.kingdom.capital;
                if (paid < cost && cap != null && cap != buyer && cap != seller) paid += TakeGold(cap, cost - paid);
                Add(buyer, "aid", qty);
            }
            else
            {
                if (Gold(buyer) < cost + duty) { qty = (int)((Gold(buyer) - duty) / Math.Max(0.1f, unit)); cost = Mathf.CeilToInt(qty * unit); }
                if (qty <= 0) { DbgNoGold++; return; }
                paid = TakeGold(buyer, cost + duty) - duty;
            }
            DbgDone++;
            seller.takeResource(g.Id, qty);
            buyer.addResourcesToRandomStockpile(g.Id, qty);
            cost = Math.Max(0, paid);
            GiveGold(seller, cost);
            if (duty > 0 && buyer.kingdom.capital != null) GiveGold(buyer.kingdom.capital, duty);
            Add(buyer, "trade_out", cost + duty);
            Add(seller, "trade_in", cost);
            Add(buyer, "trade_total", cost);
            Add(seller, "trade_total", cost);
        }

        static void TradeFor(City buyer, List<City> cities)
        {
            int pop = Math.Max(1, buyer.getPopulationPeople());
            TradeFood(buyer, cities);
            foreach (var g in Goods)
            {
                if (g.Kind == 0) continue;
                float target = pop * g.PerPerson * NeedScale;
                int have = buyer.getResourcesAmount(g.Id);
                if (have >= target * 0.5f) continue;
                DbgNeed++;
                City seller = null; float best = float.MaxValue;
                foreach (var s in cities)
                {
                    float st = Math.Max(1, s.getPopulationPeople()) * g.PerPerson * NeedScale;
                    if (s.getResourcesAmount(g.Id) < st * 0.9f + 5) continue;   // มีพอตัวเองแล้วจึงขายส่วนเกิน
                    float d = Dist(buyer, s);
                    if (d < best) { best = d; seller = s; }
                }
                if (seller == null) { DbgNoSeller++; continue; }
                float sTarget = Math.Max(1, seller.getPopulationPeople()) * g.PerPerson * NeedScale;
                int qty = (int)Math.Min(Math.Min(target - have, seller.getResourcesAmount(g.Id) - sTarget * 0.6f), 30);
                Deal(buyer, seller, g, qty);
            }
            TradeWeapons(buyer, cities);
        }

        /// <summary>ค้าอาวุธ: เมืองที่คลังอาวุธน้อยซื้อจากเมืองที่มีเหลือ</summary>
        static void TradeWeapons(City buyer, List<City> cities)
        {
            var bl = buyer.data.equipment?.item_storage_weapons;
            if (bl == null) return;
            int want = Math.Max(1, buyer.getPopulationPeople() / 6);
            if (bl.Count >= want) return;
            foreach (var s in cities)
            {
                if (!CanTrade(buyer, s)) continue;
                var sl = s.data.equipment?.item_storage_weapons;
                if (sl == null || sl.Count <= Math.Max(2, s.getPopulationPeople() / 4)) continue;
                long id = sl[sl.Count - 1];
                var item = World.world.items.get(id);
                if (item == null) { sl.RemoveAt(sl.Count - 1); continue; }
                int price = Math.Max(5, item.getValue() / 10);
                if (Gold(buyer) < price) return;
                sl.RemoveAt(sl.Count - 1);
                item.clearCity();
                buyer.data.equipment.addItem(buyer, item, bl);
                TakeGold(buyer, price);
                GiveGold(s, price);
                Add(buyer, "trade_out", price);
                Add(s, "trade_in", price);
                Add(buyer, "arms_bought", 1);
                return;
            }
        }

        static void Taxes(City c)
        {
            var cap = c.kingdom.capital;
            if (cap == null || cap == c) return;
            int rate = TaxRate(c.kingdom);
            int tax = Gold(c) * rate / 100;
            if (tax <= 0) return;
            TakeGold(c, tax);
            GiveGold(cap, tax);
            Set(c, "tax_paid", tax);
            Add(cap, "tax_income", tax);
        }

        /// <summary>ธนาคารประจำเมือง</summary>
        static void Bank(City c)
        {
            int reserve = Get(c, "bank_reserve");
            int debt = Get(c, "debt");
            int n = 0;
            if (!_byCity.TryGetValue(c.getID(), out var citizens)) citizens = new List<Actor>();
            foreach (var a in citizens)
            {
                if (++n > 60) break;
                int money = a.data.money;
                a.data.get("ngd_deposit", out int dep, 0);
                if (money > 10)
                {   // ฝากเงิน 25%
                    int d = money / 4;
                    a.data.money = money - d;
                    dep += d; reserve += d;
                }
                else if (money < 3 && dep > 0)
                {   // ถอนเงินยามขัดสน
                    int w = Math.Min(dep, Math.Max(1, dep / 5));
                    if (w <= reserve) { a.data.money = money + w; dep -= w; reserve -= w; }
                }
                // ดอกเบี้ยเงินฝาก 0.5%/เดือน
                int interest = dep / 200;
                if (interest > 0 && reserve > interest) { dep += interest; reserve -= interest; }
                a.data.set("ngd_deposit", dep);
            }

            // ปล่อยกู้ให้เมืองที่ทองไม่พอ
            if (Gold(c) < 20 && reserve > 60 && Get(c, "crisis") == 0 && debt == 0)
            {
                int loan = Math.Min(reserve / 2, 150);
                GiveGold(c, loan);
                reserve -= loan; debt += loan;
            }
            // ชำระหนี้: ต้น 10% + ดอก 2% ต่อเดือน
            if (debt > 0)
            {
                int due = Mathf.CeilToInt(debt * 0.12f);
                int paid = TakeGold(c, Math.Min(due, Math.Max(1, Gold(c) * 3 / 10)));
                reserve += paid;
                debt = Math.Max(0, debt - Mathf.FloorToInt(paid / 1.2f));
                int late = paid < due / 3 ? Get(c, "debt_late") + 1 : 0;
                Set(c, "debt_late", late);
                if (late >= 6)
                {   // ค้างชำระ 6 เดือนติด: วิกฤตการเงิน
                    Set(c, "debt_late", 0);
                    reserve = Math.Max(0, reserve - debt / 2);
                    debt = 0;
                    Set(c, "crisis", 12);
                    GuildSystem.Announce(string.Format(NeoModLoader.General.LM.Get("ngd_news_crisis"), c.name));
                }
            }
            Set(c, "bank_reserve", reserve);
            Set(c, "debt", debt);
        }

        /// <summary>ผลทางการเมืองต่อความภักดีของเมือง</summary>
        public static int LoyaltyModifier(City c)
        {
            if (c == null || c.kingdom == null) return 0;
            int m = 0;
            m += Mathf.Clamp(Gold(c) / 40, 0, 8);                     // มั่งคั่ง
            m += Mathf.Clamp(Get(c, "trade_in") / 20, 0, 5);           // การค้าคึกคัก
            m -= (TaxRate(c.kingdom) - 10) / 2;                        // ภาษีหนัก
            if (Get(c, "crisis") > 0) m -= 15;                         // วิกฤตการเงิน
            if (Get(c, "debt") > 300) m -= 5;
            m -= Cult.LoyaltyPenalty(c);                               // ลัทธิแทรกซึม
            m += Math.Min(5, Get(c, "nobles"));                        // ขุนนางระดับปรมาจารย์ช่วยปกครอง
            if (c.kingdom.hasKing()) m += Mathf.Clamp((NgdData.GetStat(c.kingdom.king, 6) - 10) / 3, -5, 8); // บารมีกษัตริย์
            return m;
        }
    }

    [HarmonyPatch(typeof(LoyaltyCalculator), "calculate")]
    public static class Patch_Loyalty
    {
        public static void Postfix(City pCity, ref int __result)
        {
            try { if (Economy.Enabled && pCity != null && pCity.isAlive()) __result += Economy.LoyaltyModifier(pCity); }
            catch (Exception e) { NgdMod.LogOnce("loyalty: " + e); }
        }
    }

    /// <summary>แถวข้อมูลเศรษฐกิจในหน้าต่างเมืองของเกม</summary>
    [HarmonyPatch(typeof(CityWindow), "showStatsRows")]
    public static class Patch_CityWindowRows
    {
        public static void Postfix(CityWindow __instance)
        {
            try
            {
                var c = __instance.meta_object;
                if (c == null || !c.isAlive()) return;
                CityTabs.Inject(__instance, c);   // แท็บข้าง: คลังเมือง / การปกครอง
                var gold = "#ffd76a";
                __instance.showStatRow("ngd_row_tax", Economy.TaxRate(c.kingdom) + "%", gold, MetaType.None, -1, true, "ngd/pts", null, null, true);
                __instance.showStatRow("ngd_row_trade", "+" + Economy.Get(c, "trade_in") + " / -" + Economy.Get(c, "trade_out"), gold, MetaType.None, -1, true, "ngd/exp", null, null, true);
                if (Economy.HasBank(c))
                {
                    __instance.showStatRow("ngd_row_bank", Economy.Get(c, "bank_reserve"), gold, MetaType.None, -1, true, "ngd/pts", null, null, true);
                    __instance.showStatRow("ngd_row_debt", Economy.Get(c, "debt"), "#ff9a9a", MetaType.None, -1, true, "ngd/minus", null, null, true);
                }
                if (Economy.Get(c, "crisis") > 0)
                    __instance.showStatRow("ngd_row_crisis", Economy.Get(c, "crisis"), "#ff6060", MetaType.None, -1, true, "ngd/kill", null, null, true);
                int lm = Economy.LoyaltyModifier(c);
                __instance.showStatRow("ngd_row_politics", (lm >= 0 ? "+" : "") + lm, lm >= 0 ? "#9aff9a" : "#ff9a9a", MetaType.None, -1, true, "s_cha".Insert(0, "ngd/"), null, null, true);
                var star = HallOfFame.CityStar(c);
                if (star != null)
                    __instance.showStatRow("ngd_row_star", star, "#ffd76a", MetaType.None, -1, true, "ngd/star16", null, null, false);
                if (Economy.Get(c, "masters") > 0)
                    __instance.showStatRow("ngd_row_masters", Economy.Get(c, "masters"), "#9aff9a", MetaType.None, -1, true, "ngd/pr_smith", null, null, true);
                if (GuildSystem.HasGuild(c))
                    __instance.showStatRow("ngd_row_guild", NeoModLoader.General.LM.Get("ngd_guild_tier_" + GuildSystem.GuildTier(c)) + " (" + GuildSystem.Fame(c) + ")",
                        "#c8a8ff", MetaType.None, -1, true, "ngd/tab", null, null, true);
            }
            catch (Exception e) { NgdMod.LogOnce("city window rows: " + e); }
        }
    }
}
