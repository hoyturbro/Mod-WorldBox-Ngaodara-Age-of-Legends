using System;
using System.Collections.Generic;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// อาวุธเทพชุด "โบราณวัตถุเงาดารา" ภาพ PNG ของเราเอง
    /// ภาพในมือ: GameResources/items/weapons/w_{id}.png · ไอคอน: GameResources/ui/Icons/items/icon_{id}.png
    /// ชื่อ: item_{id} · คำอธิบาย: {id}_description (ใน Locales)
    /// </summary>
    public static class Arsenal
    {
        public class Relic
        {
            public string Id, Template, Group, Projectile, Slash;
            // สกิลประจำอาวุธ (ถ้าว่าง ใช้ค่ามาตรฐานตามกลุ่มอาวุธ)
            public string SkFx, SkStatus, SkSelf;
            public float SkTime, SkMult, SkRadius;
            public int SkHits;
            public Skills.Kind? SkKind;
            public Dictionary<string, float> Stats;
            public string[] Modifiers;
        }

        public static readonly List<Relic> Relics = new List<Relic>
        {
            new Relic { Id = "ngd_starfall_blade", Template = "$sword", Group = "sword",
                Stats = new Dictionary<string, float> { { "damage", 60 }, { "critical_chance", 0.15f }, { "targets", 3 }, { "attack_speed", 15 } } },
            new Relic { Id = "ngd_chrono_blade", Template = "$sword", Group = "sword",
                Stats = new Dictionary<string, float> { { "damage", 55 }, { "attack_speed", 40 }, { "speed", 8 }, { "critical_chance", 0.1f } } },
            new Relic { Id = "ngd_thunder_spear", Template = "$spear", Group = "spear",
                Stats = new Dictionary<string, float> { { "damage", 58 }, { "range", 3 }, { "targets", 2 }, { "knockback", 0.6f } } },
            new Relic { Id = "ngd_rift_scythe", Template = "$axe", Group = "axe",
                Stats = new Dictionary<string, float> { { "damage", 72 }, { "targets", 4 }, { "critical_damage_multiplier", 0.5f } } },
            new Relic { Id = "ngd_cosmos_hammer", Template = "$hammer", Group = "hammer",
                Stats = new Dictionary<string, float> { { "damage", 85 }, { "knockback", 1.2f }, { "targets", 3 }, { "armor", 10 } } },
            new Relic { Id = "ngd_crescent_bow", Template = "$bow", Group = "bow", Projectile = "arrow",
                Stats = new Dictionary<string, float> { { "damage", 48 }, { "range", 8 }, { "critical_chance", 0.25f }, { "accuracy", 20 } } },
            new Relic { Id = "ngd_galaxy_scepter", Template = "$range", Group = "staff", Projectile = "freeze_orb",
                Stats = new Dictionary<string, float> { { "damage", 52 }, { "range", 10 }, { "mana", 80 }, { "targets", 2 } } },
            // ---- อาวุธเทพชุดที่ 2 (รวม 20 ชิ้น) ----
            new Relic { Id = "ngd_phoenix_blade", Template = "$sword", Group = "sword", SkFx = "fire", SkStatus = "burning", SkTime = 4f, SkKind = Skills.Kind.Around, SkRadius = 3.5f, SkMult = 2.4f,
                Stats = new Dictionary<string, float> { { "damage", 64 }, { "health", 120 }, { "critical_chance", 0.1f } } },
            new Relic { Id = "ngd_dragonfang_katana", Template = "$sword", Group = "sword", SkFx = "fire", SkKind = Skills.Kind.Dash, SkMult = 3.2f,
                Stats = new Dictionary<string, float> { { "damage", 70 }, { "attack_speed", 30 }, { "critical_chance", 0.2f } } },
            new Relic { Id = "ngd_soul_reaper", Template = "$axe", Group = "axe", SkFx = "dark", SkSelf = "drain", SkKind = Skills.Kind.Around, SkRadius = 4f, SkMult = 2.6f,
                Stats = new Dictionary<string, float> { { "damage", 78 }, { "targets", 4 }, { "critical_damage_multiplier", 0.6f } } },
            new Relic { Id = "ngd_titan_maul", Template = "$hammer", Group = "hammer", SkFx = "nature", SkStatus = "stunned", SkTime = 2.5f, SkKind = Skills.Kind.Aoe, SkRadius = 4.5f, SkMult = 2.8f,
                Stats = new Dictionary<string, float> { { "damage", 92 }, { "health", 200 }, { "armor", 18 }, { "knockback", 1.5f } } },
            new Relic { Id = "ngd_worldtree_staff", Template = "$range", Group = "staff", Projectile = "freeze_orb", SkFx = "nature", SkKind = Skills.Kind.Heal, SkRadius = 7f,
                Stats = new Dictionary<string, float> { { "damage", 45 }, { "range", 10 }, { "mana", 120 }, { "health", 150 } } },
            new Relic { Id = "ngd_abyss_trident", Template = "$spear", Group = "spear", SkFx = "ice", SkStatus = "slowness", SkTime = 4f, SkKind = Skills.Kind.Aoe, SkRadius = 4f, SkMult = 2.5f,
                Stats = new Dictionary<string, float> { { "damage", 66 }, { "range", 3 }, { "targets", 3 }, { "health", 80 } } },
            new Relic { Id = "ngd_storm_bow", Template = "$bow", Group = "bow", Projectile = "arrow", SkFx = "thunder", SkStatus = "stunned", SkTime = 0.8f, SkHits = 6, SkMult = 0.9f,
                Stats = new Dictionary<string, float> { { "damage", 54 }, { "range", 9 }, { "attack_speed", 35 }, { "critical_chance", 0.15f } } },
            new Relic { Id = "ngd_bloodmoon_dagger", Template = "$sword", Group = "sword", SkFx = "blood", SkSelf = "drain", SkKind = Skills.Kind.Dash, SkMult = 2.8f,
                Stats = new Dictionary<string, float> { { "damage", 58 }, { "attack_speed", 45 }, { "critical_chance", 0.25f }, { "speed", 6 } } },
            new Relic { Id = "ngd_sun_lance", Template = "$spear", Group = "spear", SkFx = "holy", SkStatus = "burning", SkTime = 3f, SkKind = Skills.Kind.Dash, SkMult = 3.0f,
                Stats = new Dictionary<string, float> { { "damage", 74 }, { "range", 3 }, { "health", 100 }, { "targets", 2 } } },
            new Relic { Id = "ngd_frost_crown_staff", Template = "$range", Group = "staff", Projectile = "freeze_orb", SkFx = "ice", SkStatus = "frozen", SkTime = 2.5f, SkKind = Skills.Kind.Aoe, SkRadius = 5f, SkMult = 2.4f,
                Stats = new Dictionary<string, float> { { "damage", 56 }, { "range", 11 }, { "mana", 100 }, { "armor", 10 } } },
            new Relic { Id = "ngd_clockwork_saw", Template = "$sword", Group = "sword", SkFx = "thunder", SkHits = 6, SkMult = 0.9f,
                Stats = new Dictionary<string, float> { { "damage", 62 }, { "attack_speed", 50 }, { "armor", 12 } } },
            new Relic { Id = "ngd_dragon_cleaver", Template = "$axe", Group = "axe", SkFx = "fire", SkStatus = "burning", SkTime = 4f, SkKind = Skills.Kind.Around, SkRadius = 4f, SkMult = 2.9f,
                Stats = new Dictionary<string, float> { { "damage", 88 }, { "targets", 4 }, { "health", 150 } } },
            new Relic { Id = "ngd_sakura_bow", Template = "$bow", Group = "bow", Projectile = "arrow", SkFx = "wind", SkSelf = "dodge", SkHits = 5, SkMult = 1.1f,
                Stats = new Dictionary<string, float> { { "damage", 50 }, { "range", 9 }, { "critical_chance", 0.3f }, { "speed", 8 } } },
        };

        public static void Register()
        {
            foreach (var r in Relics)
            {
                try
                {
                    var a = AssetManager.items.clone(r.Id, r.Template);
                    a.equipment_subtype = r.Id;
                    a.group_id = r.Group;
                    a.has_locales = true;
                    a.translation_key = null;
                    a.is_pool_weapon = false;
                    a.pool_rate = 0;
                    a.equipment_value = 2000;
                    a.rigidity_rating = 10;
                    a.quality = Rarity.R3_Legendary;
                    Gear.FixNames(a);
                    a.show_in_meta_editor = true;   // แสดงในเมนู Equipment ของเกม
                    if (!string.IsNullOrEmpty(r.Projectile)) a.projectile = r.Projectile;
                    foreach (var kv in r.Stats) a.base_stats[kv.Key] = kv.Value;

                    a.path_gameplay_sprite = "items/weapons/w_" + r.Id;
                    a.path_icon = "ui/Icons/items/icon_" + r.Id;
                    a.gameplay_sprites = SpriteTextureLoader.getSpriteList(a.path_gameplay_sprite);
                    if (a.gameplay_sprites == null || a.gameplay_sprites.Length == 0)
                    {
                        var one = SpriteTextureLoader.getSprite(a.path_gameplay_sprite);
                        if (one != null) a.gameplay_sprites = new[] { one };
                    }
                    if (a.gameplay_sprites == null || a.gameplay_sprites.Length == 0)
                        NgdMod.LogOnce("relic sprite missing: " + r.Id);
                    else Gear.FitWeaponSprites(a);
                }
                catch (Exception e) { NgdMod.LogOnce("relic " + r.Id + ": " + e); }
            }
        }

        public static string RandomRelicId() => Relics[UnityEngine.Random.Range(0, Relics.Count)].Id;

        /// <summary>มอบอาวุธเทพให้ (แทนอาวุธเดิม)</summary>
        public static bool Give(Actor a, string id = null)
        {
            if (a == null || !a.isAlive() || a.equipment == null || a.equipment.weapon == null) return false;
            try
            {
                a.createNewWeapon(id ?? RandomRelicId());
                a.setStatsDirty();
                return true;
            }
            catch (Exception e) { NgdMod.LogOnce("give relic: " + e); return false; }
        }
    }
}
