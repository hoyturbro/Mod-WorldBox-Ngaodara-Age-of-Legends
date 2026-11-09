using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ระบบบ้านเมืองตามยุค:
    ///   1) อาคารเดิมของเกม (ทุกเผ่า) แสดงเป็นอาคารของมอดตาม ยุคของเมือง × ชนิดอาคาร × ระดับอัปเกรด
    ///      เมืองเข้ายุคใหม่ = ทุกหลังเปลี่ยนรูป · บ้านอัปเกรดตามขั้นของเกม · เมืองหลวงมีปราสาทราชา
    ///   2) อาคารใหม่ 15 แบบ ที่เมืองสร้างเองผ่านระบบก่อสร้างของเกม (ลำดับการสร้าง + สถาปัตยกรรมทุกเผ่า)
    /// ภาพ: GameResources/buildings/ngd/&lt;ยุค&gt;_&lt;ชนิด&gt;/{main_0, construction_0, ruin_0}
    /// </summary>
    public static class CityLook
    {
        public static bool Enabled = true;
        public static int ForceEra = -1;   // ทดสอบ/ภาพตัวอย่าง: บังคับยุค
        static readonly Dictionary<string, Sprite[]> _cache = new Dictionary<string, Sprite[]>();   // main, construction, ruin
        static readonly Dictionary<long, (int t, int era)> _eraCache = new Dictionary<long, (int, int)>();

        /// <summary>ภาพอาคาร [ปกติ, กำลังสร้าง, ซาก]</summary>
        static bool _preloaded;

        /// <summary>โหลดภาพทั้งหมดล่วงหน้าบนเธรดหลัก (เกมคำนวณภาพอาคารแบบหลายเธรด)</summary>
        public static bool Preloaded => _preloaded;
        public static void Preload()
        {
            if (_preloaded) return;
            var root = System.IO.Path.Combine(NgdMod.ModFolder, "GameResources", "buildings", "ngd");
            int n = 0;
            try
            {
                foreach (var dir in System.IO.Directory.GetDirectories(root))
                    if (Load(System.IO.Path.GetFileName(dir)) != null) n++;
            }
            catch (Exception e) { NgdMod.LogOnce("building preload: " + e.Message); }
            _preloaded = true;
            NgdMod.Log("building looks preloaded: " + n);
        }

        public static Sprite[] Frames(string id)
        {
            lock (_cache) { if (_cache.TryGetValue(id, out var f)) return f; }
            return null;
        }

        static Sprite[] Load(string id)
        {
            Sprite[] f = null;
            Sprite[] list = null;
            try { list = SpriteTextureLoader.getSpriteList("buildings/ngd/" + id, false); } catch { }
            if (list != null && list.Length > 0)
            {
                Sprite main = null, con = null, ruin = null;
                foreach (var s in list)
                {
                    if (s.name.StartsWith("main")) main = s;
                    else if (s.name.StartsWith("construction")) con = s;
                    else if (s.name.StartsWith("ruin")) ruin = s;
                }
                f = main != null ? new[] { main, con ?? main, ruin ?? main } : null;
            }
            lock (_cache) _cache[id] = f;
            return f;
        }

        static int EraOf(Building b)
        {
            if (ForceEra >= 0) return ForceEra;
            City c = null;
            try { c = b.getCity(); } catch { }
            if (c == null || !c.isAlive()) return Eras.WorldEra();
            long id = c.getID();
            int now = Environment.TickCount;
            lock (_eraCache)
                if (_eraCache.TryGetValue(id, out var e) && now - e.t < 2000) return e.era;
            int era = Eras.CityEra(c);
            lock (_eraCache) _eraCache[id] = (now, era);
            return era;
        }

        /// <summary>ชนิดอาคารของมอดที่ใช้แทนอาคารของเกม ("@" = ใช้ชื่อโฟลเดอร์ตรง ๆ เช่นอาคารเทคประจำยุค)</summary>
        public static string KindFor(Building b, int era)
        {
            var a = b.asset;
            if (a == null) return null;
            if (a.id.StartsWith("ngd_") && NewBuildings.KindOf.TryGetValue(a.id, out var nk)) return nk;
            string e = Eras.Ids[Mathf.Clamp(era, 0, Eras.Ids.Length - 1)];
            City c = null; try { c = b.getCity(); } catch { }
            bool capital = c != null && c.isAlive() && c.isCapitalCity();
            int lv = a.upgrade_level;
            switch (a.type)
            {
                case "type_house":
                    if (lv <= 1) return "house0";
                    if (lv == 2) return "house1";
                    if (lv == 3) return "house2";
                    if (lv == 4) return era >= Eras.Kingdom ? "rowhouse" : "house3";
                    return "mansion";
                case "type_tent": return "shack";
                case "type_hall": return capital ? "castle" : "hall";
                case "type_temple": return capital && era >= Eras.Iron ? "cathedral" : "temple";
                case "type_barracks": return "barracks";
                case "type_market": return "market";
                case "type_watch_tower": return "tower";
                case "type_well": return "well";
                case "type_stockpile": return "granary";
                case "type_library":
                    switch (e) { case "bronze": return "@bronze_scribe"; case "kingdom": return "@kingdom_university"; case "magic": return "@magic_arcane_library"; case "star": return "@star_observatory"; default: return "academy"; }
                case "type_windmill":
                    switch (e) { case "bronze": return "@bronze_waterwheel"; case "iron": return "@iron_watermill"; default: return "farm"; }
                case "type_docks":
                    if (a.id.Contains("fishing")) return "fishery";
                    return era >= Eras.Kingdom ? "@kingdom_harbor" : era == Eras.Iron ? "@iron_shipyard" : "fishery";
                case "type_mine": return era == Eras.Iron ? "@iron_coal_mine" : "mine";
            }
            return null;   // กองไฟ รูปปั้น หุ่นซ้อม: ใช้ภาพเดิมของเกม
        }

        public static Sprite Pick(Building b)
        {
            int era = EraOf(b);
            string kind = KindFor(b, era);
            if (kind == null) return null;
            string id = kind[0] == '@' ? kind.Substring(1) : Eras.Ids[Mathf.Clamp(era, 0, Eras.Ids.Length - 1)] + "_" + kind;
            var f = Frames(id);
            if (f == null) return null;
            if (b.isRuin()) return f[2];
            if (b.isUnderConstruction()) return f[1];
            return f[0];
        }
    }

    [HarmonyPatch(typeof(Building), "calculateMainSprite")]
    public static class Patch_BuildingLook
    {
        public static void Postfix(Building __instance, ref Sprite __result)
        {
            if (!CityLook.Enabled || __instance == null) return;
            try
            {
                if (!__instance.isCiv()) return;
                var s = CityLook.Pick(__instance);
                if (s != null) __result = s;
            }
            catch (Exception e) { NgdMod.LogOnce("building look: " + e.Message); }
        }
    }

    /// <summary>อาคารใหม่ของมอด: เมืองสร้างเองผ่านระบบก่อสร้างของเกม (ใช้ได้ทุกเผ่า)</summary>
    public static class NewBuildings
    {
        public static readonly Dictionary<string, string> KindOf = new Dictionary<string, string>();
        public static IEnumerable<string> Ids { get { foreach (var d in Defs) yield return d.id; } }

        // id, ชนิดภาพ, ประชากรขั้นต่ำ, จำนวนอาคารขั้นต่ำ, ค่าก่อสร้าง (ไม้ หิน โลหะ ทอง), ฐาน (ซ้าย ขวา บน ล่าง)
        static readonly (string id, string kind, int pop, int bld, int[] cost, int[] fund)[] Defs =
        {
            ("ngd_guild", "guild", 10, 6, new[] { 30, 20, 5, 20 }, new[] { 3, 3, 4, 0 }),
            ("ngd_tavern", "tavern", 15, 5, new[] { 25, 10, 0, 5 }, new[] { 2, 2, 3, 0 }),
            ("ngd_stall", "stall", 12, 4, new[] { 10, 0, 0, 0 }, new[] { 1, 1, 1, 0 }),
            ("ngd_shrine", "shrine", 12, 4, new[] { 10, 10, 0, 0 }, new[] { 1, 1, 1, 0 }),
            ("ngd_bakery", "bakery", 20, 8, new[] { 20, 10, 0, 0 }, new[] { 2, 2, 2, 0 }),
            ("ngd_forge", "forge", 20, 8, new[] { 20, 20, 10, 0 }, new[] { 2, 2, 2, 0 }),
            ("ngd_healer", "healer", 25, 10, new[] { 20, 10, 0, 5 }, new[] { 2, 2, 2, 0 }),
            ("ngd_school", "school", 30, 12, new[] { 30, 20, 0, 10 }, new[] { 2, 2, 3, 0 }),
            ("ngd_stable", "stable", 30, 12, new[] { 30, 5, 0, 0 }, new[] { 2, 2, 2, 0 }),
            ("ngd_bank", "bank", 40, 15, new[] { 20, 40, 10, 40 }, new[] { 2, 2, 3, 0 }),
            ("ngd_fountain", "fountain", 40, 15, new[] { 0, 30, 0, 10 }, new[] { 1, 1, 1, 0 }),
            ("ngd_alchemist", "alchemist", 45, 16, new[] { 20, 20, 5, 20 }, new[] { 2, 2, 2, 0 }),
            ("ngd_cemetery", "cemetery", 50, 18, new[] { 10, 30, 0, 0 }, new[] { 2, 2, 2, 0 }),
            ("ngd_bathhouse", "bathhouse", 60, 20, new[] { 30, 40, 0, 15 }, new[] { 2, 2, 3, 0 }),
            ("ngd_courthouse", "courthouse", 80, 25, new[] { 30, 60, 10, 40 }, new[] { 3, 3, 3, 0 }),
        };

        public static void Register()
        {
            int ok = 0;
            foreach (var d in Defs)
            {
                try
                {
                    var tpl = AssetManager.buildings.get("library_human") != null ? "library_human" : "temple_human";
                    var a = AssetManager.buildings.clone(d.id, tpl);
                    a.type = "type_" + d.id;
                    a.main_path = "buildings/ngd/";
                    a.sprite_path = "buildings/ngd/iron_" + d.kind;     // ภาพฐาน (ระหว่างเกมจะเปลี่ยนตามยุคของเมือง)
                    a.building_sprites = null;
                    a.sprites_are_initiated = false;
                    a.has_ruins_graphics = true;
                    a.can_be_upgraded = false; a.upgrade_to = null; a.upgrade_level = 0;
                    a.cost = new ConstructionCost(d.cost[0], d.cost[1], d.cost[2], d.cost[3]);
                    a.fundament = new BuildingFundament(d.fund[0], d.fund[1], d.fund[2], d.fund[3]);
                    a.housing_slots = 0; a.storage = false; a.tower = false;
                    KindOf[d.id] = d.kind;
                    string order = "order_" + d.id;
                    foreach (var arch in AssetManager.architecture_library.list)
                        try { arch.addBuildingOrderKey(order, d.id); } catch { }
                    foreach (var bo in AssetManager.city_build_orders.list)
                        try
                        {
                            var o = bo.addBuilding(order, 1, d.pop, d.bld, false, false, 0);
                            if (o != null) o.requirements_types = new[] { "type_hall" };
                        }
                        catch { }
                    ok++;
                }
                catch (Exception e) { NgdMod.LogOnce("new building " + d.id + ": " + e.Message); }
            }
            foreach (var bo in AssetManager.city_build_orders.list)
                try { bo.prepareForAssetGeneration(); } catch { }
            NgdMod.Log("new buildings registered: " + ok);
        }
    }
}
