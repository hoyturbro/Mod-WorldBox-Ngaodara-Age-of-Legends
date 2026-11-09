using System;
using System.Collections.Generic;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>God power ชุดเงาดารา</summary>
    public static class DivinePowers
    {
        static readonly List<Actor> _tmp = new List<Actor>();

        static List<Actor> UnitsAround(WorldTile t, float radius)
        {
            _tmp.Clear();
            if (t == null) return _tmp;
            foreach (var a in Finder.getUnitsFromChunk(t, 2, radius, false))
                if (a != null && a.isAlive()) _tmp.Add(a);
            return _tmp;
        }

        static WorldTile RandomTileNear(WorldTile c, int r)
        {
            int x = c.x + UnityEngine.Random.Range(-r, r + 1);
            int y = c.y + UnityEngine.Random.Range(-r, r + 1);
            return World.world.GetTile(x, y) ?? c;
        }

        static void Dust(WorldTile t, string id)
        {
            try { World.world.drop_manager.spawn(t, id, 3f, 1f, -1); } catch { }
        }

        /// <summary>ประทานอาวุธเทพ: คลิกคน ได้อาวุธเทพแบบสุ่ม + บัฟพลัง</summary>
        public static bool GiftRelic(WorldTile tile, string power)
        {
            var a = NgdMod.PickActor(tile);
            if (a == null) return false;
            Arsenal.Give(a);
            a.addStatusEffect("powerup", 20f, true);
            Dust(a.current_tile, "dust_gold");
            return true;
        }

        /// <summary>ฝนดาวตก: ฟ้าผ่าลงพื้นที่กว้าง 16 ครั้ง</summary>
        public static bool Starfall(WorldTile tile, string power)
        {
            if (tile == null) return false;
            for (int i = 0; i < 16; i++)
            {
                var t = RandomTileNear(tile, 9);
                if (i % 4 == 0) MapBox.spawnLightningBig(t, 0.6f, null);
                else MapBox.spawnLightningMedium(t, 0.5f, null);
            }
            return true;
        }

        /// <summary>หยุดกาลเวลา (ธีม SS กฎแห่งเวลา): แช่แข็งทุกตัวในรัศมี 12 ช่อง 12 วินาที</summary>
        public static bool TimeStop(WorldTile tile, string power)
        {
            if (tile == null) return false;
            foreach (var a in UnitsAround(tile, 12f))
                if (!NgdData.IsImmortal(a)) a.addStatusEffect("frozen", 12f, true);
            Dust(tile, "dust_blue");
            return true;
        }

        /// <summary>เปิดรอยแยกมิติตรงที่คลิก (ทดสอบระบบบอส/ค่าหัว)</summary>
        public static bool OpenRift(WorldTile tile, string power)
        {
            if (tile == null) return false;
            City near = null; float best = float.MaxValue;
            foreach (var c in World.world.cities)
            {
                var ct = c?.getTile(false);
                if (ct == null) continue;
                float d = Vector2.Distance(new Vector2(ct.x, ct.y), new Vector2(tile.x, tile.y));
                if (d < best) { best = d; near = c; }
            }
            Rifts.Open(tile, -1, best < 80 ? near : null);
            return true;
        }

        /// <summary>อวยพรหมู่: ทุกตัวในรัศมี 8 ช่อง ตื่นพลัง +1 ดาว และฮีลเต็ม</summary>
        public static bool MassAscension(WorldTile tile, string power)
        {
            if (tile == null) return false;
            foreach (var a in UnitsAround(tile, 8f))
            {
                if (!a.asset.can_be_inspected) continue;
                Progression.Awaken(a);
                if (NgdData.GetStar(a) < NgdData.MaxStar) NgdData.SetStar(a, NgdData.GetStar(a) + 1);
                a.setStatsDirty();
                a.restoreHealth(a.getMaxHealth());
            }
            Dust(tile, "dust_white");
            return true;
        }
    }
}
