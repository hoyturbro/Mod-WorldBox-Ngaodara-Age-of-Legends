using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ผังเมืองแบบโลกจริง: แบ่งเป็นวงตามระยะจากใจกลางเมือง
    ///   ใจกลาง (ศาลากลาง วิหาร ตลาด หอสมุด รูปปั้น บ่อน้ำ กิลด์)
    ///   → ย่านบ้าน → ย่านช่าง/คลัง/ทหาร → เกษตร (กังหัน ไร่) และหอเฝ้าระวังที่ขอบ
    /// ทำงานโดยกรองโซนที่เมืองจะเลือกสร้างอาคาร ถ้าไม่มีโซนในวงที่ต้องการ จะขยายวงก่อน แล้วค่อยใช้แบบเดิม
    /// ทุกเมืองมีถนนเชื่อมอาคาร
    /// </summary>
    public static class CityPlan
    {
        public static bool Enabled = true;

        // วงของแต่ละประเภทอาคาร (หน่วย: ช่องพื้น) ใจกลางเมืองคือ 0
        static readonly Dictionary<string, (float min, float max)> Bands = new Dictionary<string, (float, float)>
        {
            { "type_hall", (0, 10) }, { "type_temple", (0, 14) }, { "type_market", (0, 14) },
            { "type_library", (0, 14) }, { "type_statue", (0, 12) }, { "type_well", (0, 18) },
            { "type_bonfire", (0, 12) }, { "type_ngd_guild", (0, 14) },
            { "type_house", (6, 30) },
            { "type_stockpile", (10, 30) }, { "type_barracks", (14, 36) }, { "type_training_dummies", (14, 36) },
            { "type_windmill", (22, 60) }, { "type_watch_tower", (24, 70) },
        };

        public static Vector2 Center(City c)
        {
            if (c._city_tile != null) return new Vector2(c._city_tile.x, c._city_tile.y);
            return c.city_center;
        }

        public static void FilterZones(BuildingAsset asset, City city, List<TileZone> zones)
        {
            if (!Enabled || asset == null || city == null || zones == null || zones.Count < 2) return;
            if (asset.type == null || !Bands.TryGetValue(asset.type, out var band)) return;
            var c = Center(city);
            var keep = new List<TileZone>();
            // ลองวงที่ต้องการก่อน ถ้าว่างค่อยขยายวงทีละ 8 ช่อง (สูงสุด 3 รอบ)
            for (int widen = 0; widen <= 3 && keep.Count == 0; widen++)
            {
                float min = Math.Max(0, band.min - widen * 8), max = band.max + widen * 8;
                foreach (var z in zones)
                {
                    var t = z.centerTile;
                    if (t == null) continue;
                    float d = Vector2.Distance(c, new Vector2(t.x, t.y));
                    if (d >= min && d <= max) keep.Add(z);
                }
            }
            if (keep.Count == 0) return;
            zones.Clear();
            zones.AddRange(keep);
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.CityBehBuild), "fillPossibleZones")]
    public static class Patch_CityZones
    {
        public static void Postfix(BuildingAsset pBuildingAsset, City pCity, List<TileZone> pPossibleZones)
        {
            try { CityPlan.FilterZones(pBuildingAsset, pCity, pPossibleZones); }
            catch (Exception e) { NgdMod.LogOnce("city plan: " + e); }
        }
    }

    /// <summary>ทุกวัฒนธรรมใช้ถนนได้ เมืองจึงมีถนนเชื่อมอาคารเป็นระเบียบ</summary>
    [HarmonyPatch(typeof(Culture), "canUseRoads")]
    public static class Patch_Roads
    {
        public static void Postfix(ref bool __result)
        {
            if (CityPlan.Enabled) __result = true;
        }
    }
}
