using System;
using System.IO;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// เครื่องมือช่วยวาดภาพ: ส่งออกภาพอ้างอิงของเกม (อาวุธ ไอคอน) เป็น PNG พร้อมขนาด/pivot
    /// เพื่อให้งานภาพของมอดเข้ากับสไตล์และสเกลของเกมจริง ทำครั้งเดียวเมื่อยังไม่มีโฟลเดอร์ _art_ref
    /// </summary>
    public static class ArtProbe
    {
        public static void Run(string modFolder)
        {
            try
            {
                // เก็บนอกโฟลเดอร์มอด เพราะ deploy ลบโฟลเดอร์มอดทั้งก้อน
                string dir = Path.Combine(Directory.GetParent(modFolder).FullName, "_ngd_art_ref");
                if (Directory.Exists(dir)) return;
                Directory.CreateDirectory(dir);
                var log = new System.Text.StringBuilder();
                int n = 0;
                foreach (var it in AssetManager.items.list)
                {
                    if (it == null || it.id.StartsWith("$") || it.gameplay_sprites == null || it.gameplay_sprites.Length == 0) continue;
                    if (n++ > 40) break;
                    var s = it.gameplay_sprites[0];
                    log.AppendLine($"{it.id} group={it.group_id} frames={it.gameplay_sprites.Length} rect={s.rect} pivot={s.pivot} ppu={s.pixelsPerUnit} icon={it.path_icon}");
                    Save(s, Path.Combine(dir, "w_" + it.id + ".png"));
                    var ic = SpriteTextureLoader.getSprite(it.path_icon);
                    if (ic != null)
                    {
                        log.AppendLine($"   icon rect={ic.rect} ppu={ic.pixelsPerUnit}");
                        Save(ic, Path.Combine(dir, "icon_" + it.id + ".png"));
                    }
                }
                // อาคารเมือง (เพื่อวาดอาคารกิลด์และบ้านแต่ละยุคให้สเกลตรง)
                int b = 0;
                foreach (var ba in AssetManager.buildings.list)
                {
                    if (ba == null || !ba.city_building || string.IsNullOrEmpty(ba.main_path)) continue;
                    if (b++ > 30) break;
                    log.AppendLine($"B {ba.id} type={ba.type} main_path={ba.main_path} sprite_path={ba.sprite_path} atlas={ba.atlas_id} fund={ba.fundament?.left},{ba.fundament?.right},{ba.fundament?.top},{ba.fundament?.bottom}");
                    try
                    {
                        var list = SpriteTextureLoader.getSpriteList(ba.main_path);
                        if (list == null) continue;
                        for (int i = 0; i < list.Length && i < 3; i++)
                        {
                            var sp = list[i];
                            log.AppendLine($"   [{i}] {sp.name} rect={sp.rect} pivot={sp.pivot} ppu={sp.pixelsPerUnit}");
                            Save(sp, Path.Combine(dir, "b_" + ba.id + "_" + i + ".png"));
                        }
                    }
                    catch (Exception e) { log.AppendLine("   err " + e.Message); }
                }
                File.WriteAllText(Path.Combine(dir, "info.txt"), log.ToString());
                NgdMod.Log("art reference exported to " + dir);
            }
            catch (Exception e) { NgdMod.LogOnce("art probe: " + e); }
        }

        /// <summary>ส่งออกภาพอาวุธในมือของเกม (โหลดตาม path เพราะตอนเปิดมอดยังไม่โหลด)</summary>
        public static void Weapons(string modFolder)
        {
            try
            {
                string dir = Path.Combine(Directory.GetParent(modFolder).FullName, "_ngd_art_ref2");
                Directory.CreateDirectory(dir);
                var log = new System.Text.StringBuilder();
                foreach (var it in AssetManager.items.list)
                {
                    if (it == null || it.id.StartsWith("$") || it.id.StartsWith("ngd_") || it.equipment_type != EquipmentType.Weapon) continue;
                    Sprite[] list = null;
                    try { list = it.getSprites(); } catch { }
                    if ((list == null || list.Length == 0) && !string.IsNullOrEmpty(it.path_gameplay_sprite)) list = SpriteTextureLoader.getSpriteList(it.path_gameplay_sprite);
                    if (list == null || list.Length == 0) continue;
                    var s = list[0];
                    log.AppendLine($"{it.id} group={it.group_id} frames={list.Length} rect={s.rect.width}x{s.rect.height} pivot={s.pivot} ppu={s.pixelsPerUnit} colored={it.colored} path={it.path_gameplay_sprite}");
                    Save(s, Path.Combine(dir, "w_" + it.id + ".png"));
                }
                File.WriteAllText(Path.Combine(dir, "info.txt"), log.ToString());
            }
            catch (Exception e) { NgdMod.LogOnce("weapon probe: " + e); }
        }

        static void Save(Sprite s, string path)
        {
            var tex = s.texture;
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
            var prev = RenderTexture.active;
            Graphics.Blit(tex, rt);
            RenderTexture.active = rt;
            var r = s.textureRect;
            var outTex = new Texture2D((int)r.width, (int)r.height, TextureFormat.RGBA32, false);
            outTex.ReadPixels(new Rect(r.x, r.y, r.width, r.height), 0, 0);
            outTex.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            File.WriteAllBytes(path, outTex.EncodeToPNG());
            UnityEngine.Object.Destroy(outTex);
        }
    }
}
