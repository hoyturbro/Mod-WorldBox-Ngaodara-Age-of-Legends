using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ระบบอุปกรณ์หลากหลาย:
    /// - 8 ธาตุ: ไฟ น้ำแข็ง สายฟ้า ลม ดิน แสง มืด ดารา (มีวงจรแพ้ทาง ดาเมจ ×1.25 / ×0.85)
    /// - เซ็ตธาตุละ 6 ชิ้น (อาวุธ หมวก เกราะ รองเท้า แหวน สร้อย) โบนัส 2/4/6 ชิ้น
    /// - ชุดวัสดุตามยุค 4 ระดับ (หนัง สำริด เหล็ก มิธริล) สำหรับนักผจญภัย
    /// - ตีโดน: ผลของธาตุ (ติดไฟ แช่แข็ง มึน ชะลอ พิษ ดูดเลือด ฟื้นพลัง ดาเมจดารา)
    /// - ได้อุปกรณ์จาก: เลื่อนแรงค์ · บอสรอยแยก · ช่างตีเหล็ก · เมนูเทพ
    /// </summary>
    public static class Gear
    {
        public class Element
        {
            public string Id, Color, Beats, Weapon, Projectile, Proc, Full;   // Full = เอฟเฟกต์เต็มเซ็ต 6 ชิ้น เช่น "damage*1.2,armor+25"
            public float ProcTime;
            public Dictionary<string, float> Bias;
        }

        public static readonly Element[] Elements =
        {
            new Element { Id = "fire", Full = "damage*1.25,critical_damage_multiplier+0.3", Color = "#ff6a3a", Beats = "ice", Weapon = "$sword", Proc = "burning", ProcTime = 3f,
                Bias = new Dictionary<string, float> { { "damage", 8 }, { "critical_damage_multiplier", 0.15f } } },
            new Element { Id = "ice", Full = "armor+25,health*1.15", Color = "#8fd8ff", Beats = "wind", Weapon = "$range", Projectile = "freeze_orb", Proc = "frozen", ProcTime = 1.2f,
                Bias = new Dictionary<string, float> { { "armor", 6 }, { "health", 30 } } },
            new Element { Id = "thunder", Full = "attack_speed+40,critical_chance+0.08", Color = "#ffe45a", Beats = "earth", Weapon = "$spear", Proc = "stunned", ProcTime = 0.8f,
                Bias = new Dictionary<string, float> { { "attack_speed", 15 }, { "critical_chance", 0.05f } } },
            new Element { Id = "wind", Full = "speed+12,range+3", Color = "#9affc8", Beats = "thunder", Weapon = "$bow", Projectile = "arrow", Proc = "slowness", ProcTime = 3f,
                Bias = new Dictionary<string, float> { { "speed", 8 }, { "range", 2 } } },
            new Element { Id = "earth", Full = "health*1.3,armor+15", Color = "#c8964a", Beats = "fire", Weapon = "$hammer", Proc = "stunned", ProcTime = 0.6f,
                Bias = new Dictionary<string, float> { { "health", 60 }, { "armor", 4 } } },
            new Element { Id = "light", Full = "health*1.2,mana*1.3", Color = "#fff2c0", Beats = "dark", Weapon = "$sword", Proc = "heal", ProcTime = 0f,
                Bias = new Dictionary<string, float> { { "health", 40 }, { "mana", 30 } } },
            new Element { Id = "dark", Full = "damage*1.2,critical_damage_multiplier+0.5", Color = "#9a5aff", Beats = "light", Weapon = "$axe", Proc = "drain", ProcTime = 0f,
                Bias = new Dictionary<string, float> { { "damage", 10 }, { "critical_chance", 0.06f } } },
            new Element { Id = "star", Full = "damage*1.12,health*1.12,armor*1.12,speed+4", Color = "#7fa8ff", Beats = "", Weapon = "$range", Projectile = "plasma_ball", Proc = "star", ProcTime = 0f,
                Bias = new Dictionary<string, float> { { "damage", 6 }, { "health", 25 }, { "speed", 3 }, { "mana", 20 } } },
            // ---- ธาตุใหม่ 8 ชุด ----
            new Element { Id = "blood", Full = "critical_chance+0.12,damage*1.15", Color = "#ff3a4a", Beats = "sakura", Weapon = "$sword", Proc = "drain", ProcTime = 0f,
                Bias = new Dictionary<string, float> { { "damage", 9 }, { "critical_chance", 0.04f } } },
            new Element { Id = "plague", Full = "attack_speed+25,critical_chance+0.06", Color = "#9aff4a", Beats = "blood", Weapon = "$sword", Proc = "poisoned", ProcTime = 4f,
                Bias = new Dictionary<string, float> { { "attack_speed", 10 }, { "critical_chance", 0.05f } } },
            new Element { Id = "abyss", Full = "health*1.2,armor+12", Color = "#4affe8", Beats = "fire", Weapon = "$spear", Proc = "slowness", ProcTime = 3f,
                Bias = new Dictionary<string, float> { { "health", 40 }, { "armor", 3 } } },
            new Element { Id = "clockwork", Full = "armor+20,attack_speed+20", Color = "#5af0ff", Beats = "nature", Weapon = "$sword", Proc = "stunned", ProcTime = 0.6f,
                Bias = new Dictionary<string, float> { { "armor", 8 }, { "attack_speed", 8 } } },
            new Element { Id = "dragon", Full = "damage*1.25,health*1.1", Color = "#ffd24a", Beats = "clockwork", Weapon = "$axe", Proc = "burning", ProcTime = 3f,
                Bias = new Dictionary<string, float> { { "damage", 12 }, { "health", 30 } } },
            new Element { Id = "nature", Full = "health*1.25,speed+4", Color = "#c8ff6a", Beats = "abyss", Weapon = "$hammer", Proc = "heal", ProcTime = 0f,
                Bias = new Dictionary<string, float> { { "health", 50 }, { "speed", 2 } } },
            new Element { Id = "sakura", Full = "critical_chance+0.15,speed+6", Color = "#ff7aa8", Beats = "plague", Weapon = "$sword", Proc = "self:dodge", ProcTime = 2f,
                Bias = new Dictionary<string, float> { { "speed", 6 }, { "critical_chance", 0.08f } } },
            new Element { Id = "moon", Full = "critical_damage_multiplier+0.6,range+3", Color = "#e8eeff", Beats = "dragon", Weapon = "$bow", Projectile = "arrow", Proc = "frozen", ProcTime = 0.8f,
                Bias = new Dictionary<string, float> { { "critical_damage_multiplier", 0.2f }, { "range", 2 } } },
            // ---- เซ็ตเทพชุดสุดท้าย 4 ชุด (รวม 20) ----
            new Element { Id = "void", Full = "damage*1.3,critical_chance+0.05", Color = "#ff4aff", Beats = "star", Weapon = "$axe", Proc = "drain", ProcTime = 0f,
                Bias = new Dictionary<string, float> { { "damage", 14 }, { "critical_chance", 0.05f } } },
            new Element { Id = "phoenix", Full = "health*1.15,damage*1.1", Color = "#ff8a3a", Beats = "plague", Weapon = "$sword", Proc = "burning", ProcTime = 3f,
                Bias = new Dictionary<string, float> { { "health", 50 }, { "damage", 8 } } },
            new Element { Id = "crystal", Full = "mana*1.5,armor+18", Color = "#ff9aff", Beats = "dark", Weapon = "$range", Projectile = "freeze_orb", Proc = "frozen", ProcTime = 1f,
                Bias = new Dictionary<string, float> { { "mana", 40 }, { "armor", 6 } } },
            new Element { Id = "titan", Full = "health*1.4,armor+20", Color = "#ff6a2a", Beats = "wind", Weapon = "$hammer", Proc = "stunned", ProcTime = 0.8f,
                Bias = new Dictionary<string, float> { { "health", 80 }, { "armor", 8 } } },
        };

        public static readonly string[] Pieces = { "weapon", "helmet", "armor", "boots", "ring", "amulet" };
        public static readonly string[] Materials = { "leather", "bronze", "iron", "mythril" };
        static readonly string[] MatPieces = { "helmet", "armor", "boots" };

        static readonly Dictionary<string, (string template, EquipmentType slot)> PieceInfo = new Dictionary<string, (string, EquipmentType)>
        {
            { "helmet", ("$helmet", EquipmentType.Helmet) }, { "armor", ("$armor", EquipmentType.Armor) },
            { "boots", ("$boots", EquipmentType.Boots) }, { "ring", ("$ring", EquipmentType.Ring) },
            { "amulet", ("$amulet", EquipmentType.Amulet) },
        };

        static readonly Dictionary<string, Dictionary<string, float>> PieceStats = new Dictionary<string, Dictionary<string, float>>
        {
            { "weapon", new Dictionary<string, float> { { "damage", 30 } } },
            { "helmet", new Dictionary<string, float> { { "armor", 4 }, { "health", 20 } } },
            { "armor", new Dictionary<string, float> { { "armor", 9 }, { "health", 45 } } },
            { "boots", new Dictionary<string, float> { { "armor", 3 }, { "speed", 5 } } },
            { "ring", new Dictionary<string, float> { { "damage", 6 }, { "critical_chance", 0.04f } } },
            { "amulet", new Dictionary<string, float> { { "mana", 25 }, { "health", 20 } } },
        };

        public static string SetId(string el, string piece) => "ngd_set_" + el + "_" + piece;
        public static string MatId(string mat, string piece) => "ngd_mat_" + mat + "_" + piece;
        public static Element GetElement(string id) { foreach (var e in Elements) if (e.Id == id) return e; return null; }

        // ---------- ลงทะเบียน ----------
        public static void Register()
        {
            foreach (var el in Elements)
                foreach (var piece in Pieces)
                {
                    string tpl = piece == "weapon" ? el.Weapon : PieceInfo[piece].template;
                    var a = Make(SetId(el.Id, piece), tpl, piece == "weapon", Rarity.R3_Legendary, 1500);
                    if (a == null) continue;
                    foreach (var kv in PieceStats[piece]) a.base_stats[kv.Key] = kv.Value * 1.3f;
                    foreach (var kv in el.Bias) a.base_stats[kv.Key] += kv.Value * (piece == "weapon" || piece == "armor" ? 1f : 0.5f);
                    if (piece == "weapon" && !string.IsNullOrEmpty(el.Projectile)) a.projectile = el.Projectile;
                    if (piece == "weapon" && el.Weapon == "$range") { a.group_id = "staff"; a.base_stats["range"] = 10; }
                }
            for (int t = 0; t < Materials.Length; t++)
                foreach (var piece in MatPieces)
                {
                    var a = Make(MatId(Materials[t], piece), PieceInfo[piece].template, false, t >= 3 ? Rarity.R1_Rare : Rarity.R0_Normal, 60 + t * 120);
                    if (a == null) continue;
                    foreach (var kv in PieceStats[piece]) a.base_stats[kv.Key] = kv.Value * (0.4f + t * 0.3f);
                }
        }

        /// <summary>ไอเทมที่โคลนจากแม่แบบนามธรรม ($range ฯลฯ) ไม่มีรายชื่อแม่แบบชื่อ เกมสุ่มชื่อแล้วพัง: ยืมจากไอเทมจริงชนิดเดียวกัน</summary>
        /// <summary>
        /// ปรับภาพอาวุธในมือให้สูงเท่าอาวุธของเกมกลุ่มเดียวกัน (NML โหลด PNG ที่ ppu=1 ทำให้ใหญ่เกินตัวคน)
        /// </summary>
        public static void FitWeaponSprites(EquipmentAsset a)
        {
            // ภาพในมือวาดขนาดเท่าอาวุธของเกม (art9_hand.py) → ใช้ 1 พิกเซล = 1 หน่วย แบบเกม และจุดหมุนที่ด้ามตามชนิด
            if (a?.gameplay_sprites == null || a.gameplay_sprites.Length == 0) return;
            a.colored = false;
            var list = new Sprite[a.gameplay_sprites.Length];
            for (int i = 0; i < list.Length; i++)
            {
                var s = a.gameplay_sprites[i];
                if (s.texture != null) s.texture.filterMode = FilterMode.Point;
                float w = s.rect.width, h = s.rect.height;
                float py;
                switch (a.group_id)
                {
                    case "spear": py = 1.8f; break;
                    case "hammer": py = 1.45f; break;
                    case "axe": py = 1.0f; break;
                    case "bow": py = h * 0.5f; break;
                    case "staff": case "range": py = h * 0.47f; break;
                    default: py = 1.3f; break;
                }
                list[i] = Sprite.Create(s.texture, s.rect, new Vector2(0.5f, py / h), 1f);
                list[i].name = s.name;
            }
            a.gameplay_sprites = list;
        }

        public static void FixNames(EquipmentAsset a)
        {
            if (a == null || (a.name_templates != null && a.name_templates.Count > 0)) return;
            foreach (var o in AssetManager.items.list)
                if (o != a && o.equipment_type == a.equipment_type && o.name_templates != null && o.name_templates.Count > 0)
                { a.name_templates = new List<string>(o.name_templates); return; }
            a.name_templates = new List<string>();
        }

        static EquipmentAsset Make(string id, string template, bool weapon, Rarity q, int value)
        {
            try
            {
                var a = AssetManager.items.clone(id, template);
                a.equipment_subtype = id;
                a.has_locales = true;
                a.translation_key = null;
                a.quality = q;
                a.equipment_value = value;
                if (weapon) a.is_pool_weapon = false;
                a.pool_rate = 0;
                a.path_icon = "ui/Icons/items/icon_" + id;
                a.show_in_meta_editor = true;   // แสดงในเมนู Equipment ของเกม
                FixNames(a);
                if (weapon)
                {
                    a.path_gameplay_sprite = "items/weapons/w_" + id;
                    a.gameplay_sprites = SpriteTextureLoader.getSpriteList(a.path_gameplay_sprite);
                    if (a.gameplay_sprites == null || a.gameplay_sprites.Length == 0)
                    {
                        var one = SpriteTextureLoader.getSprite(a.path_gameplay_sprite);
                        if (one != null) a.gameplay_sprites = new[] { one };
                    }
                    FitWeaponSprites(a);
                }
                return a;
            }
            catch (Exception e) { NgdMod.LogOnce("gear " + id + ": " + e.Message); return null; }
        }

        // ---------- มอบอุปกรณ์ ----------
        public static bool Equip(Actor a, string id)
        {
            if (a == null || !a.isAlive() || a.equipment == null) return false;
            try
            {
                var asset = AssetManager.items.get(id);
                if (asset == null) return false;
                var k = a.kingdom != null && a.kingdom.isCiv() ? a.kingdom : null;
                Item item;
                try { item = World.world.items.generateItem(asset, k, a.getName(), 1, a, 0, true); }
                catch (Exception e1)
                {   // บางครั้งการสุ่มม็อดของเกมพัง: สร้างใหม่แบบไม่สุ่มม็อด
                    NgdMod.LogOnce("equip retry " + id + ": " + e1);
                    item = World.world.items.generateItem(asset, null, null, 0, null, 0, true);
                }
                if (item == null) return false;
                var slot = a.equipment.getSlot(asset.equipment_type);
                if (slot == null) return false;
                slot.setItem(item, a);
                a.setStatsDirty();
                return true;
            }
            catch (Exception e) { NgdMod.LogOnce("equip " + id + ": " + e); return false; }
        }

        /// <summary>ถอดอุปกรณ์ทุกช่อง</summary>
        public static void UnequipAll(Actor a)
        {
            if (a?.equipment == null) return;
            foreach (var t in new[] { EquipmentType.Weapon, EquipmentType.Helmet, EquipmentType.Armor, EquipmentType.Boots, EquipmentType.Ring, EquipmentType.Amulet })
                try { var s = a.equipment.getSlot(t); if (s != null && !s.isEmpty()) s.takeAwayItem(); } catch { }
            a.setStatsDirty();
        }

        public static void EquipSet(Actor a, string el) { foreach (var p in Pieces) Equip(a, SetId(el, p)); }
        public static void EquipMaterial(Actor a, int tier) { foreach (var p in MatPieces) Equip(a, MatId(Materials[Mathf.Clamp(tier, 0, 3)], p)); }

        public static string RandomArmorPiece() =>
            SetId(Elements[UnityEngine.Random.Range(0, Elements.Length)].Id, Pieces[UnityEngine.Random.Range(1, Pieces.Length)]);

        public static string RandomSetPiece() =>
            SetId(Elements[UnityEngine.Random.Range(0, Elements.Length)].Id, Pieces[UnityEngine.Random.Range(0, Pieces.Length)]);

        /// <summary>เลื่อนแรงค์: อุปกรณ์ดีขึ้นตามแรงค์ (สไตล์อนิเมะ)</summary>
        public static void OnRankUp(Actor a, int rank)
        {
            if (rank <= 1) EquipMaterial(a, 0);
            else if (rank == 2) EquipMaterial(a, 1);
            else if (rank == 3) EquipMaterial(a, 2);
            else EquipMaterial(a, 3);   // เซ็ตเทพไม่แจกฟรีแล้ว: ต้องหลอมที่โรงตีเหล็ก (Forge) หรือได้จากรอยแยก
        }

        // ---------- นับเซ็ต · ธาตุ ----------
        static readonly Dictionary<long, (string atk, string def, int pieces)> _cache = new Dictionary<long, (string, string, int)>();

        public static (string atk, string def, int pieces) Info(Actor a)
        {
            if (a == null) return (null, null, 0);
            return _cache.TryGetValue(a.data.id, out var v) ? v : (null, null, 0);
        }

        static void Recalc(Actor a)
        {
            string atk = null; var counts = new Dictionary<string, int>();
            foreach (var slotType in new[] { EquipmentType.Weapon, EquipmentType.Helmet, EquipmentType.Armor, EquipmentType.Boots, EquipmentType.Ring, EquipmentType.Amulet })
            {
                var slot = a.equipment?.getSlot(slotType);
                if (slot == null || slot.isEmpty()) continue;
                var id = slot.getItem()?.getAsset()?.id;
                if (id == null || !id.StartsWith("ngd_set_")) continue;
                var el = id.Substring(8, id.LastIndexOf('_') - 8);
                counts[el] = (counts.TryGetValue(el, out var n) ? n : 0) + 1;
                if (slotType == EquipmentType.Weapon) atk = el;
            }
            string best = null; int bestN = 0;
            foreach (var kv in counts) if (kv.Value > bestN) { bestN = kv.Value; best = kv.Key; }
            if (atk == null && bestN >= 6) atk = best;
            if (atk == null && best == null) { _cache.Remove(a.data.id); return; }
            _cache[a.data.id] = (atk, best, bestN);
        }

        /// <summary>โบนัสเซ็ต (เรียกจาก updateStats)</summary>
        public static void ApplySetBonus(Actor a, BaseStats s)
        {
            Recalc(a);
            var (_, el, n) = Info(a);
            var e = GetElement(el);
            if (e == null) return;
            if (n >= 2) foreach (var kv in e.Bias) s[kv.Key] += kv.Value;
            if (n >= 4) { s["damage"] *= 1.15f; s["health"] *= 1.15f; }
            if (n >= 6) { s["armor"] += 10; s["speed"] += 3; ApplyFull(e.Full, s); }
        }

        /// <summary>เอฟเฟกต์เต็มเซ็ต: "stat*1.2" คูณ · "stat+25" บวก</summary>
        public static void ApplyFull(string full, BaseStats s)
        {
            if (string.IsNullOrEmpty(full)) return;
            foreach (var part in full.Split(','))
            {
                int i = part.IndexOfAny(new[] { '*', '+' });
                if (i <= 0) continue;
                string key = part.Substring(0, i);
                if (!float.TryParse(part.Substring(i + 1), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v)) continue;
                if (part[i] == '*') s[key] *= v; else s[key] += v;
            }
        }

        // ---------- ฟีนิกซ์: เต็มเซ็ตฟื้นคืนชีพ 1 ครั้ง ทุก 3 นาที ----------
        static readonly Dictionary<long, float> _rebirth = new Dictionary<long, float>();
        public static bool TryRebirth(Actor a, float dmg)
        {
            var (_, el, n) = Info(a);
            if (el != "phoenix" || n < 6 || dmg < a.getHealth()) return false;
            float now = Time.time;
            if (_rebirth.TryGetValue(a.data.id, out var t) && now - t < 180f) return false;
            _rebirth[a.data.id] = now;
            a.restoreHealth(a.getMaxHealth());
            try { Fx.Ultimate(a, "fire"); } catch { }
            try { Legends.Record(string.Format(NeoModLoader.General.LM.Get("ngd_news_rebirth"), a.getName()), a); } catch { }
            return true;
        }

        public static float ElementFactor(string atk, string def)
        {
            if (string.IsNullOrEmpty(atk)) return 1f;
            if (atk == "star") return 1.1f;
            var ea = GetElement(atk);
            if (ea != null && ea.Beats == def) return 1.25f;
            var ed = GetElement(def);
            if (ed != null && ed.Beats == atk) return 0.85f;
            return 1f;
        }

        /// <summary>ตีโดน: ผลของธาตุ</summary>
        public static void OnHit(Actor attacker, Actor victim, float dmg)
        {
            var (atk, _, n) = Info(attacker);
            var e = GetElement(atk);
            if (e == null) return;
            float chance = 0.15f + (n >= 4 ? 0.15f : 0f) + (n >= 6 ? 0.1f : 0f);
            if (UnityEngine.Random.value > chance) return;
            switch (e.Proc)
            {
                case "heal": attacker.restoreHealth(Mathf.Max(1, (int)(dmg * 0.3f))); break;
                case "drain": attacker.restoreHealth(Mathf.Max(1, (int)(dmg * 0.2f))); break;
                case "star": break;   // ดาเมจเสริมคิดใน prefix แล้ว
                case string self when self.StartsWith("self:"):
                    try { attacker.addStatusEffect(self.Substring(5), e.ProcTime, true); } catch { }
                    break;
                default:
                    try { victim.addStatusEffect(e.Proc, e.ProcTime, true); } catch { }
                    break;
            }
        }
    }

    /// <summary>ดาเมจตามธาตุ (ธาตุอาวุธผู้ตี vs ธาตุชุดผู้โดน) + ผลตีโดน</summary>
    [HarmonyPatch(typeof(Actor), "getHit")]
    public static class Patch_ElementHit
    {
        public static void Prefix(Actor __instance, ref float pDamage, BaseSimObject pAttacker)
        {
            try
            {
                if (Gear.TryRebirth(__instance, pDamage)) { pDamage = 0; return; }
                var atk = pAttacker as Actor;
                if (atk == null) return;
                var (ae, _, _) = Gear.Info(atk);
                if (ae == null) return;
                var (_, de, _) = Gear.Info(__instance);
                pDamage *= Gear.ElementFactor(ae, de);
                Gear.OnHit(atk, __instance, pDamage);
            }
            catch (Exception e) { NgdMod.LogOnce("element hit: " + e.Message); }
        }
    }
}
