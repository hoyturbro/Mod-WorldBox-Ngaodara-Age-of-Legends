namespace NgaodaraPack
{
    /// <summary>ข้อมูลระดับโลกของมอด เก็บใน map_stats.custom_data จึงบันทึกไปกับเซฟโลก</summary>
    public static class WorldData
    {
        static BaseSystemData D => World.world?.map_stats?.custom_data;

        public static int GetInt(string k, int def = 0)
        {
            var d = D; if (d == null) return def;
            d.get("ngd_" + k, out int v, def); return v;
        }

        public static void SetInt(string k, int v) { D?.set("ngd_" + k, v); }

        public static string GetString(string k, string def = "")
        {
            var d = D; if (d == null) return def;
            d.get("ngd_" + k, out string v, def); return v;
        }

        public static void SetString(string k, string v) { D?.set("ngd_" + k, v ?? ""); }
    }
}
