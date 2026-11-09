using System;

namespace NgaodaraPack
{
    /// <summary>
    /// แผนที่ใหญ่ขึ้นสำหรับเมืองแบบมีผังและโซน (ใจกลาง/บ้าน/อุตสาหกรรม/เกษตร ต้องใช้พื้นที่มาก)
    /// เพิ่มขนาดแผนที่ใหม่ในหน้าสร้างโลก: Colossal (10) · Continent (12) · Mega World (14)
    /// ขนาดของเกมเดิม: tiny 2 … titanic 8 · iceberg 9
    /// </summary>
    public static class WorldSize
    {
        public static void Register()
        {
            try
            {
                Add("ngd_colossal", 10, "ngd/map_colossal");
                Add("ngd_continent", 12, "ngd/map_continent");
                Add("ngd_mega", 14, "ngd/map_mega");
                AssetManager.map_sizes.convertToOldFormat();
                Config.maxMapSize = "ngd_mega";
            }
            catch (Exception e) { NgdMod.LogOnce("world size: " + e); }
        }

        static void Add(string id, int size, string icon)
        {
            if (AssetManager.map_sizes.has(id)) return;
            var a = new MapSizeAsset { id = id, size = size, path_icon = icon, show_warning = true };
            AssetManager.map_sizes.add(a);
        }
    }
}
