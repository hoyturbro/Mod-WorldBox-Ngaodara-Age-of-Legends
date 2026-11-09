using System;
using System.Collections.Generic;
using System.Linq;
using NeoModLoader.General;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// โรงตีเหล็กเทพ: เมืองที่มีโรงตีเหล็ก (ngd_forge) ใช้ทรัพยากรมหาศาลหลอมของเทพให้ฮีโร่ที่เก่งที่สุดของเมือง
    ///   ชิ้นเซ็ตเทพ (20 เซ็ต × 6 ชิ้น): ยุคเหล็กขึ้นไป ฮีโร่แรงค์ A ขึ้นไป
    ///   อาวุธเทพ (20 ชิ้น มีชิ้นเดียวในโลก): ยุคอาณาจักรขึ้นไป ฮีโร่แรงค์ S ขึ้นไป
    /// เมืองหลอมเองทุกเดือน (ถ้าของพอ) และกดหลอมเองได้ที่แท็บโรงตีเหล็กในหน้าต่างเมือง
    /// </summary>
    public static class Forge
    {
        public static readonly (string id, int n)[] SetCost =
            { ("gold", 600), ("silver", 40), ("mythril", 30), ("adamantine", 10), ("gems", 20), ("common_metals", 60) };
        public static readonly (string id, int n)[] RelicCost =
            { ("gold", 4000), ("silver", 200), ("mythril", 150), ("adamantine", 60), ("gems", 120), ("common_metals", 300) };
        public const int SetEra = Eras.Iron, RelicEra = Eras.Kingdom;
        public const int SetRank = 4, RelicRank = 5;   // A, S
        public const float MonthlyChance = 0.25f;

        public static int Crafted;   // สถิติ (ทดสอบ)

        public static bool HasBuilding(City c, string id)
        {
            if (c == null || c.buildings == null) return false;
            foreach (var b in c.buildings)
                if (b != null && b.isAlive() && b.asset != null && b.asset.id == id && !b.isUnderConstruction() && !b.isRuin()) return true;
            return false;
        }

        public static bool HasForge(City c) => HasBuilding(c, "ngd_forge");

        static bool Exists(string res) => AssetManager.resources.get(res) != null;

        // ---------- คลังของโรงตีเหล็ก: ทยอยสะสมวัตถุดิบทุกเดือน (โกดังเมืองเก็บของได้จำกัด ของเทพต้องสะสมนาน) ----------
        static string Kind((string id, int n)[] cost) => cost == RelicCost ? "r" : "s";
        public static int Deposit(City c, (string id, int n)[] cost, string res) { c.data.get("ngd_fg_" + Kind(cost) + "_" + res, out int v, 0); return v; }
        static void SetDeposit(City c, (string id, int n)[] cost, string res, int v) => c.data.set("ngd_fg_" + Kind(cost) + "_" + res, v);

        /// <summary>ย้ายวัตถุดิบจากโกดังเมืองเข้าคลังโรงตีเหล็ก (ไม่เกินที่ต้องใช้ และไม่เกิน share ของที่เมืองมี)</summary>
        public static void Stash(City c, (string id, int n)[] cost, float share = 1f)
        {
            foreach (var (id, n) in cost)
            {
                if (!Exists(id)) continue;
                int dep = Deposit(c, cost, id), need = n - dep;
                if (need <= 0) continue;
                int take = Math.Min(need, (int)(c.getResourcesAmount(id) * share));
                if (take <= 0) continue;
                c.takeResource(id, take);
                SetDeposit(c, cost, id, dep + take);
            }
        }

        /// <summary>พลังเทพ: เติมคลังโรงตีเหล็กให้ครบทันที</summary>
        public static void FillDeposit(City c, (string id, int n)[] cost)
        {
            foreach (var (id, n) in cost) if (Exists(id)) SetDeposit(c, cost, id, n);
        }

        public static bool CanAfford(City c, (string id, int n)[] cost)
        {
            foreach (var (id, n) in cost) if (Exists(id) && Deposit(c, cost, id) + c.getResourcesAmount(id) < n) return false;
            return true;
        }

        static void Pay(City c, (string id, int n)[] cost)
        {
            foreach (var (id, n) in cost)
            {
                if (!Exists(id)) continue;
                int dep = Deposit(c, cost, id), fromDep = Math.Min(dep, n);
                SetDeposit(c, cost, id, dep - fromDep);
                int rest = n - fromDep;
                if (rest > 0) c.takeResource(id, Math.Min(rest, c.getResourcesAmount(id)));
            }
        }

        /// <summary>วัตถุดิบ: ทีละบรรทัด "ชื่อ มี/ต้องใช้"</summary>
        public static string CostText(City c, (string id, int n)[] cost)
        {
            var lines = new List<string>();
            foreach (var (id, n) in cost)
            {
                if (!Exists(id)) continue;
                int have = c.getResourcesAmount(id) + Deposit(c, cost, id);
                string name = LocalizedTextManager.stringExists(id) ? LM.Get(id) : id;
                lines.Add((have >= n ? "<color=#8aff8a>" : "<color=#ff8a8a>") + name + " " + Num.Short(have) + "/" + Num.Short(n) + "</color>");
            }
            return string.Join("\n", lines);
        }

        public static IEnumerable<Actor> Residents(City c)
        {
            if (Economy.ByCity.TryGetValue(c.getID(), out var list)) return list.Where(a => a != null && a.isAlive());
            return World.world.units.Where(a => a != null && a.isAlive() && a.getCity() == c);
        }

        public static Actor BestHero(City c, int minRank) =>
            Residents(c).Where(a => NgdData.GetRank(a) >= minRank && a.equipment != null).OrderByDescending(NgdData.CombatPower).FirstOrDefault();

        /// <summary>ชิ้นเซ็ตถัดไปของฮีโร่: เติมเซ็ตเดิมให้ครบก่อน ถ้ายังไม่มีเลือกธาตุตามอาชีพ</summary>
        public static string NextSetPiece(Actor a)
        {
            var (_, el, _) = Gear.Info(a);
            if (el == null)
            {
                int h = Mathf.Abs(((NgdData.GetClass(a) ?? "") + a.data.id).GetHashCode());
                el = Gear.Elements[h % Gear.Elements.Length].Id;
            }
            foreach (var piece in Gear.Pieces)
            {
                string id = Gear.SetId(el, piece);
                if (!Wearing(a, id)) return id;
            }
            return null;   // ครบเซ็ตแล้ว
        }

        static bool Wearing(Actor a, string id)
        {
            foreach (var t in new[] { EquipmentType.Weapon, EquipmentType.Helmet, EquipmentType.Armor, EquipmentType.Boots, EquipmentType.Ring, EquipmentType.Amulet })
            {
                var slot = a.equipment?.getSlot(t);
                if (slot != null && !slot.isEmpty() && slot.getItem()?.getAsset()?.id == id) return true;
            }
            return false;
        }

        /// <summary>อาวุธเทพที่ยังไม่มีใครถือ (หนึ่งชิ้นในโลก)</summary>
        public static string NextRelic()
        {
            var owned = new HashSet<string>();
            foreach (var u in World.world.units)
            {
                if (u == null || !u.isAlive() || u.equipment == null) continue;
                var w = u.equipment.getSlot(EquipmentType.Weapon);
                var id = w != null && !w.isEmpty() ? w.getItem()?.getAsset()?.id : null;
                if (id != null) owned.Add(id);
            }
            var free = Arsenal.Relics.Where(r => !owned.Contains(r.Id)).ToList();
            return free.Count == 0 ? null : free[UnityEngine.Random.Range(0, free.Count)].Id;
        }

        /// <summary>หลอมชิ้นเซ็ตเทพ คืนค่า null = สำเร็จ ไม่งั้นคือเหตุผล (คีย์ภาษา)</summary>
        public static string CraftSet(City c, out Actor hero, out string item, bool free = false, bool skipBuilding = false)
        {
            hero = null; item = null;
            if (!free && !skipBuilding && !HasForge(c)) return "ngd_forge_no_forge";
            if (!free && !skipBuilding && Eras.CityEra(c) < SetEra) return "ngd_forge_need_iron";
            hero = BestHero(c, SetRank);
            if (hero == null) return "ngd_forge_no_hero_a";
            item = NextSetPiece(hero);
            if (item == null) return "ngd_forge_set_full";
            if (!free && !CanAfford(c, SetCost)) return "ngd_forge_no_res";
            if (!free) Pay(c, SetCost);
            if (!Gear.Equip(hero, item)) return "ngd_forge_fail";
            hero.setStatsDirty();
            Crafted++;
            Legends.Record(string.Format(LM.Get("ngd_news_forge_set"), c.name, LM.Get("item_" + item), hero.getName()), hero, c);
            return null;
        }

        public static string CraftRelic(City c, out Actor hero, out string item, bool free = false, bool skipBuilding = false)
        {
            hero = null; item = null;
            if (!free && !skipBuilding && !HasForge(c)) return "ngd_forge_no_forge";
            if (!free && !skipBuilding && Eras.CityEra(c) < RelicEra) return "ngd_forge_need_kingdom";
            hero = BestHero(c, RelicRank);
            if (hero == null) return "ngd_forge_no_hero_s";
            var w = hero.equipment.getSlot(EquipmentType.Weapon);
            var cur = w != null && !w.isEmpty() ? w.getItem()?.getAsset()?.id : null;
            if (cur != null && Arsenal.Relics.Any(r => r.Id == cur)) return "ngd_forge_has_relic";
            item = NextRelic();
            if (item == null) return "ngd_forge_all_relics";
            if (!free && !CanAfford(c, RelicCost)) return "ngd_forge_no_res";
            if (!free) Pay(c, RelicCost);
            if (!Arsenal.Give(hero, item)) return "ngd_forge_fail";
            Crafted++;
            string text = string.Format(LM.Get("ngd_news_forge_relic"), c.name, LM.Get("item_" + item), hero.getName());
            Legends.Record(text, hero, c);
            GuildSystem.Announce(text);
            try { Fx.Ultimate(hero, "star"); } catch { }
            return null;
        }

        /// <summary>รายเดือน: เมืองที่มีโรงตีเหล็กหลอมของให้ฮีโร่เอง</summary>
        public static void MonthlyTick(List<City> cities)
        {
            foreach (var c in cities)
            {
                if (!HasForge(c)) continue;
                int era = Eras.CityEra(c);
                // ทยอยเก็บวัตถุดิบ 30% ของที่เมืองมีต่อเดือน: อาวุธเทพก่อน (ถ้ามีฮีโร่ S) ไม่งั้นชิ้นเซ็ต
                if (era >= RelicEra && BestHero(c, RelicRank) != null) Stash(c, RelicCost, 0.3f);
                else if (era >= SetEra && BestHero(c, SetRank) != null) Stash(c, SetCost, 0.3f);
                if (UnityEngine.Random.value > MonthlyChance) continue;
                if (CraftRelic(c, out _, out _) == null) continue;
                CraftSet(c, out _, out _);
            }
        }
    }

    /// <summary>อาคารใหม่ของมอดทำงานจริงทุกเดือน</summary>
    public static class CityBuildings
    {
        public static int Ticks;

        public static void MonthlyTick(List<City> cities)
        {
            Ticks++;
            foreach (var c in cities)
            {
                if (c.buildings == null) continue;
                var have = new HashSet<string>();
                foreach (var b in c.buildings)
                    if (b != null && b.isAlive() && b.asset != null && b.asset.id.StartsWith("ngd_") && !b.isUnderConstruction() && !b.isRuin()) have.Add(b.asset.id);
                if (have.Count == 0) continue;
                int pop = c.getPopulationPeople();
                if (have.Contains("ngd_bakery")) CityTabs.AddSpread(c, "bread", 10 + pop / 4);
                if (have.Contains("ngd_stall")) CityTabs.AddSpread(c, "gold", 5 + pop / 10);
                if (have.Contains("ngd_bank")) CityTabs.AddSpread(c, "gold", Mathf.Clamp(Economy.Gold(c) / 100, 5, 150));
                if (have.Contains("ngd_school")) Economy.Set(c, "research", Eras.Research(c) + 15);
                if (have.Contains("ngd_alchemist")) Economy.Set(c, "research", Eras.Research(c) + 5);
                if (have.Contains("ngd_guild")) c.data.set("ngd_guild_fame", GuildSystem.Fame(c) + 3);
                if (have.Contains("ngd_tavern")) c.data.set("ngd_guild_fame", GuildSystem.Fame(c) + 1);
                if (have.Contains("ngd_courthouse")) { int cr = Economy.Get(c, "crisis"); if (cr > 0) Economy.Set(c, "crisis", cr - 1); }

                int happy = (have.Contains("ngd_tavern") ? 4 : 0) + (have.Contains("ngd_shrine") ? 3 : 0) + (have.Contains("ngd_fountain") ? 3 : 0)
                            + (have.Contains("ngd_bathhouse") ? 6 : 0) + (have.Contains("ngd_cemetery") ? 2 : 0);
                bool heal = have.Contains("ngd_healer"), mana = have.Contains("ngd_alchemist");
                int train = (have.Contains("ngd_stable") ? 3 : 0) + (have.Contains("ngd_guild") ? 2 : 0);
                if (happy == 0 && !heal && !mana && train == 0) continue;
                int n = 0;
                foreach (var a in Forge.Residents(c))
                {
                    if (++n > 150) break;
                    if (happy > 0) a.data.happiness = Mathf.Min(100, a.data.happiness + happy);
                    if (heal && a.getHealth() < a.getMaxHealth()) a.restoreHealth(Mathf.Max(1, a.getMaxHealth() / 4));
                    if (mana) a.setMana(a.getMaxMana(), true);
                    if (train > 0 && NgdData.GetRank(a) >= 0) Progression.AddExp(a, train);
                }
            }
        }
    }
}
