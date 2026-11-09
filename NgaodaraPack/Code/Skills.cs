using System;
using System.Collections.Generic;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ระบบสกิลตามสายอาชีพ: 4 สกิลต่ออาชีพ ปลดล็อกตามแรงค์ (E · C · A · S)
    /// ทุกสกิลมี ดาเมจ (% พลังโจมตี + ค่าสถานะ) · คูลดาวน์ · เวลาร่าย · ระยะ · วงกว้าง · มานา · ผลติดตัว · เอฟเฟกต์
    /// </summary>
    public static class Skills
    {
        public static bool Enabled = true;

        public enum Kind { Strike, Multi, Aoe, Around, BuffSelf, BuffAllies, Heal, Dash }

        public class Skill
        {
            public string Id, Cls;
            public int Slot, Unlock;          // ช่อง 1–4 · แรงค์ที่ปลดล็อก
            public float Cooldown, Cast, Range, Radius, Mult;
            public int Stat = -1; public float Coef;   // โบนัสจากค่าสถานะ (% ต่อแต้ม)
            public int Hits = 1, Mana;
            public Kind Kind;
            public string TStatus; public float TTime;  // ผลต่อเป้า
            public string SStatus; public float STime;  // ผลต่อตัวเอง/พวก
            public float HealPct;
            public string Fx = "phys";                  // ชุดเอฟเฟกต์: phys fire ice thunder holy dark poison wind star blood nature
            public bool Ult;
            // ---- ระบบสกิลรุ่นใหม่ ----
            public SkillTree.Source Source;
            public string Branch, NameTh, NameEn, SetEl, Relic, PassKey;
            public int Tier; public bool Passive; public float PassVal;
        }

        static Skill S(string cls, int slot, string id, Kind k, float cd, float cast, float range, float radius, float mult, string fx,
                       int stat = -1, float coef = 0, int hits = 1, int mana = 0, string ts = null, float tt = 0, string ss = null, float st = 0, float heal = 0)
        {
            return new Skill { Cls = cls, Slot = slot, Unlock = slot == 1 ? 0 : slot == 2 ? 2 : slot == 3 ? 4 : 5, Id = id, Kind = k, Cooldown = cd, Cast = cast,
                Range = range, Radius = radius, Mult = mult, Fx = fx, Stat = stat, Coef = coef, Hits = hits, Mana = mana,
                TStatus = ts, TTime = tt, SStatus = ss, STime = st, HealPct = heal, Ult = slot == 4 };
        }

        const int STR = 0, AGI = 1, VIT = 2, INT = 3, SPI = 4, LUK = 5, CHA = 6;

        public static readonly List<Skill> All = new List<Skill>
        {
            // ---- นักดาบ
            S("swordsman", 1, "starfall_cut", Kind.Strike, 5, 0, 3.5f, 0, 1.8f, "phys", STR, 1.5f, 1, 6, "stunned", 0.5f),
            S("swordsman", 2, "cross_slash", Kind.Multi, 8, 0, 3.5f, 0, 1.2f, "phys", STR, 1f, 2, 10),
            S("swordsman", 3, "blade_storm", Kind.Around, 14, 0.4f, 3.5f, 3f, 1.5f, "wind", AGI, 1f, 1, 16),
            S("swordsman", 4, "heaven_splitter", Kind.Aoe, 30, 1.0f, 5f, 4f, 4.0f, "holy", STR, 2f, 1, 30, "stunned", 1f),
            // ---- อัศวิน
            S("knight", 1, "shield_bash", Kind.Strike, 6, 0, 3f, 0, 1.2f, "phys", VIT, 1f, 1, 6, "stunned", 1f),
            S("knight", 2, "bulwark", Kind.BuffAllies, 14, 0.3f, 0, 4f, 0, "holy", -1, 0, 1, 12, null, 0, "shield", 4f),
            S("knight", 3, "iron_will", Kind.BuffSelf, 16, 0, 0, 0, 0, "phys", -1, 0, 1, 12, null, 0, "invincible", 2f),
            S("knight", 4, "holy_fortress", Kind.BuffAllies, 35, 0.8f, 0, 6f, 0, "holy", -1, 0, 1, 30, null, 0, "shield", 6f, 0.3f),
            // ---- นักธนู
            S("archer", 1, "twin_shot", Kind.Multi, 4, 0, 14f, 0, 0.8f, "phys", AGI, 1f, 2, 6),
            S("archer", 2, "piercing_arrow", Kind.Strike, 8, 0.2f, 14f, 0, 2.2f, "wind", AGI, 1.5f, 1, 10, "slowness", 2f),
            S("archer", 3, "arrow_rain", Kind.Aoe, 15, 0.6f, 14f, 3f, 1.2f, "phys", AGI, 1f, 1, 16),
            S("archer", 4, "star_arrow", Kind.Aoe, 30, 1.0f, 16f, 3f, 5.0f, "star", AGI, 2f, 1, 30),
            // ---- จอมเวท
            S("mage", 1, "fireball", Kind.Strike, 4, 0.3f, 14f, 0, 1.6f, "fire", INT, 3f, 1, 8, "burning", 2f),
            S("mage", 2, "ice_lance", Kind.Strike, 7, 0.4f, 14f, 0, 1.8f, "ice", INT, 3f, 1, 12, "frozen", 1f),
            S("mage", 3, "chain_lightning", Kind.Aoe, 12, 0.5f, 14f, 4f, 1.4f, "thunder", INT, 3f, 1, 18, "stunned", 0.5f),
            S("mage", 4, "meteor", Kind.Aoe, 35, 1.5f, 16f, 5f, 4.5f, "fire", INT, 4f, 1, 40, "burning", 4f),
            // ---- นักบวช
            S("priest", 1, "heal", Kind.Heal, 5, 0.3f, 0, 6f, 0, "holy", SPI, 0, 1, 8, null, 0, null, 0, 0.25f),
            S("priest", 2, "blessing", Kind.BuffAllies, 12, 0.4f, 0, 6f, 0, "holy", -1, 0, 1, 12, null, 0, "enchanted", 8f),
            S("priest", 3, "smite", Kind.Strike, 8, 0.4f, 12f, 0, 1.6f, "holy", SPI, 3f, 1, 14),
            S("priest", 4, "miracle", Kind.Heal, 45, 1.2f, 0, 8f, 0, "holy", SPI, 0, 1, 40, null, 0, "shield", 3f, 0.6f),
            // ---- นักฆ่า
            S("assassin", 1, "shadow_stab", Kind.Strike, 4, 0, 3f, 0, 1.6f, "dark", LUK, 2f, 1, 6),
            S("assassin", 2, "venom_blade", Kind.Strike, 8, 0, 3f, 0, 1.0f, "poison", AGI, 1f, 1, 10, "poisoned", 5f),
            S("assassin", 3, "blink_strike", Kind.Dash, 10, 0, 8f, 0, 2.2f, "dark", AGI, 2f, 1, 14, null, 0, "dodge", 2f),
            S("assassin", 4, "thousand_shadows", Kind.Multi, 30, 0.3f, 4f, 0, 0.9f, "dark", LUK, 1.5f, 6, 30),
            // ---- นักพรต
            S("monk", 1, "chi_palm", Kind.Strike, 4, 0, 3f, 0, 1.5f, "wind", STR, 1f, 1, 6, "slowness", 1.5f),
            S("monk", 2, "iron_body", Kind.BuffSelf, 15, 0, 0, 0, 0, "phys", -1, 0, 1, 12, null, 0, "invincible", 1.5f),
            S("monk", 3, "whirlwind_kick", Kind.Around, 10, 0.2f, 3f, 3f, 1.4f, "wind", AGI, 1f, 1, 14),
            S("monk", 4, "thousand_palms", Kind.Multi, 32, 0.5f, 3f, 0, 0.7f, "holy", STR, 1f, 8, 30, "stunned", 1f),
            // ---- ทหารหอก
            S("lancer", 1, "pierce", Kind.Strike, 5, 0, 4f, 0, 1.7f, "phys", STR, 1.5f, 1, 6),
            S("lancer", 2, "lance_charge", Kind.Dash, 9, 0, 8f, 0, 1.8f, "wind", AGI, 1.5f, 1, 10, "slowness", 2f),
            S("lancer", 3, "sweep", Kind.Around, 11, 0.3f, 4f, 3.5f, 1.3f, "phys", STR, 1f, 1, 14),
            S("lancer", 4, "dragon_lance", Kind.Aoe, 30, 0.8f, 6f, 3f, 4.5f, "thunder", STR, 2f, 1, 30),
            // ---- คนคลั่ง
            S("berserker", 1, "brutal_cleave", Kind.Around, 5, 0, 3f, 2.5f, 1.2f, "blood", STR, 1.5f, 1, 6),
            S("berserker", 2, "bloodlust", Kind.BuffSelf, 14, 0, 0, 0, 0, "blood", -1, 0, 1, 10, null, 0, "rage", 8f),
            S("berserker", 3, "leap_slam", Kind.Aoe, 12, 0.3f, 6f, 3f, 1.8f, "phys", STR, 2f, 1, 14, "stunned", 1f),
            S("berserker", 4, "berserk_fury", Kind.Around, 35, 0.5f, 4f, 4f, 3.0f, "blood", STR, 2f, 1, 30, null, 0, "invincible", 3f),
            // ---- พราน
            S("ranger", 1, "poison_arrow", Kind.Strike, 5, 0, 14f, 0, 1.3f, "poison", AGI, 1f, 1, 6, "poisoned", 5f),
            S("ranger", 2, "snare", Kind.Strike, 10, 0.2f, 12f, 0, 0.8f, "nature", AGI, 1f, 1, 10, "frozen", 1.2f),
            S("ranger", 3, "hunters_mark", Kind.Strike, 12, 0.5f, 16f, 0, 2.5f, "nature", LUK, 2f, 1, 14),
            S("ranger", 4, "storm_of_arrows", Kind.Aoe, 32, 0.8f, 16f, 4f, 3.0f, "wind", AGI, 2f, 1, 30, "slowness", 3f),
            // ---- อัศวินศักดิ์สิทธิ์
            S("paladin", 1, "holy_hammer", Kind.Strike, 5, 0, 3f, 0, 1.5f, "holy", STR, 1f, 1, 6, "stunned", 0.5f),
            S("paladin", 2, "aura_of_guard", Kind.BuffAllies, 14, 0.4f, 0, 4f, 0, "holy", -1, 0, 1, 12, null, 0, "shield", 3f, 0.15f),
            S("paladin", 3, "judgment", Kind.Aoe, 12, 0.5f, 5f, 3f, 2.0f, "holy", SPI, 2f, 1, 16),
            S("paladin", 4, "divine_descent", Kind.Aoe, 40, 1.2f, 6f, 5f, 4.0f, "holy", SPI, 3f, 1, 40, null, 0, null, 0, 0.4f),
            // ---- นักดาบเวท
            S("spellblade", 1, "rune_blade", Kind.Strike, 4, 0, 3.5f, 0, 1.4f, "star", INT, 2f, 1, 6),
            S("spellblade", 2, "rune_burst", Kind.Aoe, 9, 0.3f, 4f, 2.5f, 1.5f, "star", INT, 2f, 1, 12),
            S("spellblade", 3, "arcane_ward", Kind.BuffSelf, 14, 0, 0, 0, 0, "star", -1, 0, 1, 12, null, 0, "spell_boost", 6f),
            S("spellblade", 4, "dimension_slash", Kind.Aoe, 30, 0.6f, 6f, 4f, 4.0f, "dark", INT, 3f, 1, 30),
            // ---- กวี
            S("bard", 1, "war_song", Kind.BuffAllies, 10, 0.3f, 0, 7f, 0, "holy", -1, 0, 1, 8, null, 0, "motivated", 10f),
            S("bard", 2, "healing_melody", Kind.Heal, 12, 0.4f, 0, 7f, 0, "nature", CHA, 0, 1, 12, null, 0, null, 0, 0.15f),
            S("bard", 3, "discord", Kind.Aoe, 10, 0.4f, 12f, 4f, 1.2f, "dark", CHA, 2f, 1, 14, "slowness", 2f),
            S("bard", 4, "starlight_symphony", Kind.Heal, 40, 1.0f, 0, 9f, 0, "star", CHA, 0, 1, 40, null, 0, "powerup", 10f, 0.3f),
            // ---- อาชีพเพิ่ม ----
            S("guardian", 1, "shield_wall", Kind.BuffAllies, 12, 0.3f, 0, 4f, 0, "holy", -1, 0, 1, 10, null, 0, "shield", 3f),
            S("guardian", 2, "taunt_strike", Kind.Strike, 6, 0, 3f, 0, 1.3f, "phys", VIT, 1.5f, 1, 6, "stunned", 1f),
            S("guardian", 3, "fortress_stance", Kind.BuffSelf, 16, 0, 0, 0, 0, "holy", -1, 0, 1, 12, null, 0, "invincible", 2f),
            S("guardian", 4, "aegis_of_ages", Kind.BuffAllies, 38, 1.0f, 0, 7f, 0, "holy", -1, 0, 1, 30, null, 0, "shield", 6f, 0.3f),
            S("engineer", 1, "bomb", Kind.Aoe, 6, 0.3f, 10f, 2f, 1.4f, "fire", INT, 2f, 1, 8, "burning", 2f),
            S("engineer", 2, "gear_trap", Kind.Strike, 9, 0.2f, 8f, 0, 0.8f, "thunder", INT, 1f, 1, 10, "frozen", 1.5f),
            S("engineer", 3, "turret_volley", Kind.Multi, 12, 0.4f, 12f, 0, 0.7f, "thunder", INT, 1.5f, 4, 14),
            S("engineer", 4, "steam_titan", Kind.Aoe, 34, 1.2f, 10f, 5f, 4.0f, "thunder", INT, 3f, 1, 35, "stunned", 1f),
            S("necromancer", 1, "bone_spear", Kind.Strike, 5, 0.3f, 12f, 0, 1.6f, "dark", INT, 2.5f, 1, 8),
            S("necromancer", 2, "life_drain", Kind.Strike, 9, 0.3f, 10f, 0, 1.2f, "dark", SPI, 2f, 1, 12, "slowness", 2f),
            S("necromancer", 3, "plague_cloud", Kind.Aoe, 13, 0.5f, 12f, 3f, 1.2f, "poison", INT, 2f, 1, 16, "poisoned", 5f),
            S("necromancer", 4, "army_of_dead", Kind.Aoe, 36, 1.4f, 12f, 5f, 4.0f, "dark", INT, 3f, 1, 40, "slowness", 3f),
            S("summoner", 1, "spirit_bolt", Kind.Strike, 5, 0.3f, 12f, 0, 1.5f, "star", SPI, 2.5f, 1, 8),
            S("summoner", 2, "guardian_spirit", Kind.BuffAllies, 14, 0.4f, 0, 5f, 0, "holy", -1, 0, 1, 12, null, 0, "shield", 3f),
            S("summoner", 3, "star_beast", Kind.Multi, 12, 0.5f, 12f, 0, 1.0f, "star", SPI, 2f, 3, 16),
            S("summoner", 4, "celestial_gate", Kind.Aoe, 36, 1.4f, 14f, 5f, 4.5f, "star", SPI, 3f, 1, 40),
            S("druid", 1, "thorn", Kind.Strike, 5, 0.2f, 10f, 0, 1.3f, "nature", SPI, 2f, 1, 6, "slowness", 2f),
            S("druid", 2, "rejuvenate", Kind.Heal, 10, 0.4f, 0, 6f, 0, "nature", SPI, 0, 1, 12, null, 0, null, 0, 0.2f),
            S("druid", 3, "entangle", Kind.Aoe, 13, 0.4f, 10f, 3f, 1.0f, "nature", SPI, 2f, 1, 14, "frozen", 1.5f),
            S("druid", 4, "gaia_wrath", Kind.Aoe, 38, 1.2f, 12f, 5f, 4.0f, "nature", SPI, 3f, 1, 40, null, 0, null, 0, 0.3f),
            S("shaman", 1, "lightning_totem", Kind.Strike, 5, 0.2f, 10f, 0, 1.5f, "thunder", SPI, 2f, 1, 8, "stunned", 0.5f),
            S("shaman", 2, "spirit_mend", Kind.Heal, 11, 0.4f, 0, 6f, 0, "holy", SPI, 0, 1, 12, null, 0, null, 0, 0.15f),
            S("shaman", 3, "storm_call", Kind.Aoe, 13, 0.5f, 12f, 3f, 1.6f, "thunder", SPI, 2f, 1, 16),
            S("shaman", 4, "ancestral_storm", Kind.Aoe, 36, 1.3f, 14f, 5f, 4.2f, "thunder", SPI, 3f, 1, 40, "stunned", 1f),
            S("elementalist", 1, "flame_burst", Kind.Strike, 4, 0.3f, 13f, 0, 1.7f, "fire", INT, 3f, 1, 8, "burning", 2f),
            S("elementalist", 2, "frost_nova", Kind.Around, 10, 0.3f, 3f, 3f, 1.2f, "ice", INT, 2.5f, 1, 12, "frozen", 1f),
            S("elementalist", 3, "elemental_storm", Kind.Aoe, 13, 0.5f, 13f, 3.5f, 1.8f, "thunder", INT, 3f, 1, 18),
            S("elementalist", 4, "elemental_cataclysm", Kind.Aoe, 36, 1.5f, 15f, 5f, 4.8f, "fire", INT, 4f, 1, 42, "burning", 3f),
            S("samurai", 1, "iaido", Kind.Strike, 5, 0, 3.5f, 0, 2.0f, "wind", AGI, 1.5f, 1, 6),
            S("samurai", 2, "sakura_dance", Kind.Multi, 9, 0, 3.5f, 0, 1.0f, "wind", AGI, 1.5f, 3, 10, null, 0, "dodge", 2f),
            S("samurai", 3, "moonlit_slash", Kind.Around, 12, 0.3f, 3.5f, 3f, 1.6f, "phys", STR, 2f, 1, 14),
            S("samurai", 4, "thousand_petals", Kind.Multi, 30, 0.6f, 4f, 0, 0.9f, "wind", AGI, 2f, 6, 30),
            S("ninja", 1, "shuriken", Kind.Multi, 4, 0, 10f, 0, 0.6f, "phys", AGI, 1.5f, 3, 6),
            S("ninja", 2, "smoke_bomb", Kind.BuffSelf, 12, 0, 0, 0, 0, "dark", -1, 0, 1, 10, null, 0, "dodge", 3f),
            S("ninja", 3, "shadow_clone", Kind.Multi, 12, 0.3f, 4f, 0, 0.9f, "dark", AGI, 2f, 4, 14),
            S("ninja", 4, "forbidden_jutsu", Kind.Aoe, 30, 0.8f, 6f, 4f, 4.0f, "dark", LUK, 3f, 1, 30, "poisoned", 4f),
            S("gladiator", 1, "crowd_pleaser", Kind.Strike, 5, 0, 3f, 0, 1.6f, "blood", STR, 1.5f, 1, 6),
            S("gladiator", 2, "net_throw", Kind.Strike, 9, 0.2f, 6f, 0, 0.6f, "phys", AGI, 1f, 1, 8, "frozen", 1.5f),
            S("gladiator", 3, "arena_spin", Kind.Around, 11, 0.2f, 3f, 3f, 1.5f, "blood", STR, 1.5f, 1, 12),
            S("gladiator", 4, "colosseum_king", Kind.Around, 34, 0.6f, 4f, 4f, 3.5f, "blood", STR, 2f, 1, 30, null, 0, "rage", 8f),
            S("dragoon", 1, "dragon_jump", Kind.Dash, 6, 0, 8f, 0, 1.8f, "wind", STR, 1.5f, 1, 8),
            S("dragoon", 2, "dragon_breath", Kind.Aoe, 10, 0.4f, 6f, 2.5f, 1.4f, "fire", STR, 1.5f, 1, 12, "burning", 2f),
            S("dragoon", 3, "wyvern_dive", Kind.Dash, 13, 0.2f, 10f, 0, 2.5f, "wind", AGI, 2f, 1, 16, "stunned", 1f),
            S("dragoon", 4, "dragon_descent", Kind.Aoe, 34, 1.0f, 10f, 4f, 4.5f, "fire", STR, 3f, 1, 35, "burning", 3f),
            S("warlock", 1, "curse", Kind.Strike, 6, 0.3f, 12f, 0, 1.0f, "dark", INT, 2f, 1, 8, "slowness", 3f),
            S("warlock", 2, "eldritch_blast", Kind.Strike, 6, 0.3f, 13f, 0, 1.9f, "dark", INT, 3f, 1, 10),
            S("warlock", 3, "hex_field", Kind.Aoe, 13, 0.5f, 12f, 3f, 1.3f, "poison", LUK, 2f, 1, 16, "poisoned", 4f),
            S("warlock", 4, "void_pact", Kind.Aoe, 36, 1.4f, 14f, 5f, 4.6f, "dark", INT, 4f, 1, 42),
            S("alchemist", 1, "acid_flask", Kind.Strike, 5, 0.2f, 10f, 0, 1.3f, "poison", INT, 2f, 1, 8, "poisoned", 4f),
            S("alchemist", 2, "healing_potion", Kind.Heal, 10, 0.3f, 0, 5f, 0, "nature", INT, 0, 1, 10, null, 0, null, 0, 0.2f),
            S("alchemist", 3, "explosive_brew", Kind.Aoe, 13, 0.4f, 10f, 3f, 1.6f, "fire", INT, 2.5f, 1, 16, "burning", 3f),
            S("alchemist", 4, "philosophers_storm", Kind.Aoe, 36, 1.3f, 12f, 5f, 4.0f, "star", INT, 3f, 1, 40, "slowness", 3f),
            S("beast_tamer", 1, "sic_em", Kind.Strike, 5, 0, 6f, 0, 1.4f, "nature", CHA, 2f, 1, 6),
            S("beast_tamer", 2, "pack_howl", Kind.BuffAllies, 14, 0.3f, 0, 6f, 0, "blood", -1, 0, 1, 12, null, 0, "motivated", 8f),
            S("beast_tamer", 3, "stampede", Kind.Aoe, 13, 0.5f, 8f, 3f, 1.6f, "nature", CHA, 2f, 1, 16, "slowness", 2f),
            S("beast_tamer", 4, "king_of_beasts", Kind.Around, 34, 0.8f, 5f, 5f, 3.8f, "blood", CHA, 3f, 1, 35),
            S("dancer", 1, "blade_waltz", Kind.Multi, 5, 0, 3.5f, 0, 1.0f, "wind", AGI, 1.5f, 2, 6),
            S("dancer", 2, "inspiring_dance", Kind.BuffAllies, 14, 0.4f, 0, 6f, 0, "holy", -1, 0, 1, 12, null, 0, "inspired", 8f),
            S("dancer", 3, "whirling_veil", Kind.BuffSelf, 12, 0, 0, 0, 0, "wind", -1, 0, 1, 10, null, 0, "dodge", 3f),
            S("dancer", 4, "dance_of_stars", Kind.Heal, 40, 1.0f, 0, 8f, 0, "star", CHA, 0, 1, 40, null, 0, "powerup", 8f, 0.3f),
            S("monster_hunter", 1, "hunters_strike", Kind.Strike, 5, 0, 3.5f, 0, 1.8f, "phys", STR, 1.5f, 1, 6),
            S("monster_hunter", 2, "trap_net", Kind.Strike, 9, 0.2f, 8f, 0, 0.8f, "nature", AGI, 1f, 1, 8, "frozen", 1.5f),
            S("monster_hunter", 3, "silver_volley", Kind.Multi, 11, 0.3f, 12f, 0, 1.0f, "holy", AGI, 1.5f, 3, 14),
            S("monster_hunter", 4, "slayers_oath", Kind.Strike, 30, 0.8f, 5f, 0, 6.0f, "blood", STR, 3f, 1, 30, "stunned", 1f),
            S("freelancer", 1, "quick_strike", Kind.Strike, 5, 0, 3.5f, 0, 1.5f, "phys", AGI, 1f, 1, 6),
            S("freelancer", 2, "second_wind", Kind.BuffSelf, 14, 0, 0, 0, 0, "wind", -1, 0, 1, 10, null, 0, "powerup", 6f),
            S("freelancer", 3, "improvise", Kind.Around, 11, 0.2f, 3f, 2.5f, 1.4f, "phys", LUK, 1.5f, 1, 12),
            S("freelancer", 4, "jack_of_all_trades", Kind.Aoe, 32, 0.8f, 6f, 4f, 3.8f, "star", LUK, 2f, 1, 30),
            // ---- บอสรอยแยก
            S("boss", 1, "rift_claw", Kind.Strike, 5, 0, 4f, 0, 1.8f, "dark", STR, 1f, 1, 0),
            S("boss", 2, "rift_wave", Kind.Around, 10, 0.6f, 6f, 4f, 1.5f, "dark", STR, 1f, 1, 0, "slowness", 2f),
            S("boss", 3, "void_roar", Kind.BuffSelf, 16, 0.3f, 0, 0, 0, "dark", -1, 0, 1, 0, null, 0, "shield", 3f),
            S("boss", 4, "cataclysm", Kind.Aoe, 30, 1.5f, 8f, 5f, 3.5f, "fire", STR, 2f, 1, 0, "burning", 3f),
        };

        static readonly Dictionary<string, List<Skill>> _byClass = new Dictionary<string, List<Skill>>();
        public static List<Skill> Of(string cls)
        {
            if (string.IsNullOrEmpty(cls)) return null;
            if (_byClass.Count == 0) foreach (var s in All) { if (!_byClass.TryGetValue(s.Cls, out var l)) _byClass[s.Cls] = l = new List<Skill>(); l.Add(s); }
            return _byClass.TryGetValue(cls, out var r) ? r : null;
        }

        // ---------- สถานะรันไทม์ ----------
        static readonly Dictionary<long, Dictionary<string, float>> _ready = new Dictionary<long, Dictionary<string, float>>();   // เวลาที่สกิลพร้อม
        class Pending { public float At; public Actor Caster, Target; public Skill Skill; }
        static readonly List<Pending> _pending = new List<Pending>();
        static readonly List<Actor> _tmp = new List<Actor>();
        static readonly Dictionary<long, (float at, List<Skill> list, Skill ult)> _loadCache = new Dictionary<long, (float, List<Skill>, Skill)>();
        static float _timer;

        public static float ReadyIn(Actor a, Skill s)
        {
            if (a == null || !_ready.TryGetValue(a.data.id, out var r) || !r.TryGetValue(s.Id, out var t)) return 0;
            return Mathf.Max(0, t - Time.time);
        }

        /// <summary>สล็อตปัจจุบัน (แคช 5 วินาที หรือจนกว่าจะเรียน/เปลี่ยนสล็อต)</summary>
        public static (List<Skill> list, Skill ult) LoadoutOf(Actor a)
        {
            a.data.get("ngd_loadout_dirty", out bool dirty, false);
            if (!dirty && _loadCache.TryGetValue(a.data.id, out var c) && Time.time - c.at < 5f) return (c.list, c.ult);
            a.data.set("ngd_loadout_dirty", false);
            var l = SkillTree.Loadout(a, out var u);
            _loadCache[a.data.id] = (Time.time, l, u);
            return (l, u);
        }

        public static void Tick(float dt)
        {
            if (!Enabled || World.world == null) return;
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                var p = _pending[i];
                if (Time.time < p.At) continue;
                _pending.RemoveAt(i);
                try { if (p.Caster != null && p.Caster.isAlive()) Resolve(p.Caster, p.Target, p.Skill); }
                catch (Exception e) { NgdMod.LogOnce("skill resolve: " + e.Message); }
            }
            _timer += dt;
            if (_timer < 0.4f) return;
            _timer = 0;
            try
            {
                foreach (var a in World.world.units)
                {
                    if (a == null || !a.isAlive() || !a.has_attack_target) continue;
                    int r = NgdData.GetRank(a);
                    if (r < 0) continue;   // ทุกเผ่า/ทุกสิ่งมีชีวิตที่มีแรงค์
                    var target = a.attack_target as Actor;
                    if (target == null || !target.isAlive()) continue;
                    if (IsCasting(a)) continue;
                    var (list, ult) = LoadoutOf(a);
                    TryCast(a, target, list, ult, r);
                }
            }
            catch (Exception e) { NgdMod.LogOnce("skills: " + e.Message); }
        }

        /// <summary>ใช้สกิลทันที (ทดสอบ/พลังเทพ)</summary>
        public static void ForceCast(Actor a, Actor target, Skill s) => Resolve(a, target, s);

        static bool IsCasting(Actor a) { foreach (var p in _pending) if (p.Caster == a) return true; return false; }

        static void TryCast(Actor a, Actor target, List<Skill> list, Skill ult, int rank)
        {
            if (!_ready.TryGetValue(a.data.id, out var ready)) _ready[a.data.id] = ready = new Dictionary<string, float>();
            float d = Vector2.Distance(a.current_position, target.current_position);
            var order = new List<Skill>();
            if (ult != null) order.Add(ult);
            for (int i = list.Count - 1; i >= 0; i--) order.Add(list[i]);
            foreach (var s in order)
            {
                if (s.Passive) continue;
                if (ready.TryGetValue(s.Id, out var t) && Time.time < t) continue;
                bool support = s.Kind == Kind.BuffSelf || s.Kind == Kind.BuffAllies || s.Kind == Kind.Heal;
                if (!support && d > s.Range) continue;
                if (s.Kind == Kind.Heal && !AnyHurt(a, s.Radius)) continue;
                if (!PayMana(a, s.Mana)) continue;
                // คูลดาวน์เร็วขึ้นตามแรงค์ · AGI · passive ลดคูลดาวน์ (รวมสูงสุด −60%)
                float cdMul = Mathf.Max(0.4f, 1f - rank * 0.04f - NgdData.GetStat(a, AGI) * 0.002f - SkillTree.CooldownReduction(a));
                ready[s.Id] = Time.time + s.Cooldown * cdMul;
                if (s.Cast > 0)
                {
                    Fx.Cast(a, s);
                    _pending.Add(new Pending { At = Time.time + s.Cast, Caster = a, Target = target, Skill = s });
                }
                else Resolve(a, target, s);
                return;
            }
        }

        static bool AnyHurt(Actor a, float r)
        {
            foreach (var u in Allies(a, r)) if (u.getHealth() < u.getMaxHealth() * 0.8f) return true;
            return false;
        }

        static bool PayMana(Actor a, int cost)
        {
            if (cost <= 0 || a.getMaxMana() <= 0) return true;   // สายไม่มีมานาใช้พลังกาย
            if (a.getMana() < cost) return false;
            a.spendMana(cost);
            return true;
        }

        /// <summary>ดาเมจสกิล = พลังโจมตี × ตัวคูณสกิล × (1 + ค่าสถานะ × %) × ระดับตำนาน</summary>
        public static float Damage(Actor a, Skill s)
        {
            float baseDmg = Mathf.Max(5f, a.stats["damage"]);
            float statBonus = s.Stat >= 0 ? 1f + NgdData.GetStat(a, s.Stat) * s.Coef / 100f : 1f;
            int r = NgdData.GetRank(a);
            float legend = r >= 6 ? 1f + (r - 5) * 0.25f : 1f;
            return baseDmg * s.Mult * statBonus * legend;
        }

        static void Resolve(Actor a, Actor target, Skill s)
        {
            string fx = Fx.ElementOf(a, s);
            switch (s.Kind)
            {
                case Kind.Strike:
                case Kind.Dash:
                    if (target == null || !target.isAlive()) return;
                    if (s.Kind == Kind.Dash) { Status(a, "dash", 1f); Fx.Teleport(a); }
                    HitWithFx(a, target, Damage(a, s), s, fx, 0);
                    break;
                case Kind.Multi:
                    if (target == null || !target.isAlive()) return;
                    for (int i = 0; i < s.Hits; i++)
                    {
                        int k = i;
                        var t = target;
                        NgdTicker.Later(k * 0.12f, () => { if (a.isAlive() && t.isAlive()) HitWithFx(a, t, Damage(a, s), s, fx, k); });
                    }
                    break;
                case Kind.Aoe:
                case Kind.Around:
                    var center = s.Kind == Kind.Around || target == null || !target.isAlive() || target.current_tile == null ? a : target;   // เป้าตายระหว่างร่าย → ระเบิดรอบตัวผู้ร่าย
                    Fx.Area(center, s, fx);
                    foreach (var e in Enemies(a, center, s.Radius)) HitWithFx(a, e, Damage(a, s), s, fx, 0, false);
                    if (s.Ult) Fx.Ultimate(center, fx);
                    break;
                case Kind.BuffSelf:
                    Fx.Buff(a, fx);
                    break;
                case Kind.BuffAllies:
                case Kind.Heal:
                    foreach (var u in Allies(a, s.Radius))
                    {
                        if (s.HealPct > 0)
                        {
                            int amount = (int)(u.getMaxHealth() * s.HealPct * (1f + NgdData.GetStat(a, s.Stat < 0 ? SPI : s.Stat) * 0.01f));
                            u.restoreHealth(amount);
                            DamageNumbers.Show(u, amount, DamageNumbers.Style.Heal);
                        }
                        if (s.SStatus != null) Status(u, s.SStatus, s.STime);
                        Fx.Buff(u, fx);
                    }
                    break;
            }
            if (s.SStatus != null && (s.Kind == Kind.BuffSelf || s.Kind == Kind.Dash || s.Kind == Kind.Around || s.Kind == Kind.Aoe)) Status(a, s.SStatus, s.STime);
            if (s.HealPct > 0 && (s.Kind == Kind.Dash || s.Kind == Kind.Around || s.Kind == Kind.Strike || s.Kind == Kind.Multi))
            {   // ดูดเลือด (อาวุธเทพสายโลหิต/วิญญาณ)
                int h = (int)(a.getMaxHealth() * s.HealPct); a.restoreHealth(h); DamageNumbers.Show(a, h, DamageNumbers.Style.Heal);
            }
            if (s.Kind == Kind.Aoe && s.HealPct > 0)   // ไม้ตายที่ฮีลพวกด้วย (ทวยเทพลงมา)
                foreach (var u in Allies(a, s.Radius)) { int h = (int)(u.getMaxHealth() * s.HealPct); u.restoreHealth(h); DamageNumbers.Show(u, h, DamageNumbers.Style.Heal); }
            if (NgdData.GetRank(a) >= 6 && s.Ult && target != null) MapBox.spawnLightningSmall(target.current_tile, 0.2f, a);
        }

        static void HitWithFx(Actor a, Actor t, float dmg, Skill s, string fx, int idx, bool slash = true)
        {
            if (t == null || !t.isAlive() || NgdData.IsImmortal(t)) return;
            bool crit = UnityEngine.Random.value < 0.05f + NgdData.GetStat(a, LUK) * 0.01f;
            if (crit) dmg *= 1.8f;
            if (slash) Fx.Slash(a, t, fx, idx);
            Fx.Impact(t, fx, s.Ult ? 1.4f : 1f);
            t.getHit(dmg, true, AttackType.Weapon, a, false, false, true);
            DamageNumbers.Show(t, (int)dmg, crit ? DamageNumbers.Style.Crit : DamageNumbers.Style.Normal);
            if (s.TStatus != null && t.isAlive()) Status(t, s.TStatus, s.TTime);
        }

        static void Status(Actor a, string id, float t) { try { a.addStatusEffect(id, t, true); } catch { } }

        static List<Actor> Allies(Actor a, float radius)
        {
            _tmp.Clear();
            if (a == null || !a.isAlive() || a.current_tile == null) return new List<Actor>();
            foreach (var u in Finder.getUnitsFromChunk(a.current_tile, 1, radius, false))
                if (u != null && u.isAlive() && u.kingdom == a.kingdom) _tmp.Add(u);
            return new List<Actor>(_tmp);
        }

        static List<Actor> Enemies(Actor a, Actor center, float radius)
        {
            var l = new List<Actor>();
            var tile = center != null && center.isAlive() ? center.current_tile : a?.current_tile;
            if (tile == null) return l;
            foreach (var u in Finder.getUnitsFromChunk(tile, 1, radius, false))
                if (u != null && u.isAlive() && u != a && u.kingdom != a.kingdom && (a.kingdom == null || u.kingdom == null || a.kingdom.isEnemy(u.kingdom) || !u.kingdom.isCiv())) l.Add(u);
            return l;
        }
    }
}
