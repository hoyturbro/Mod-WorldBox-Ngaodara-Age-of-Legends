# อาวุธเทพชุดที่ 2 (13 ชิ้น) — สไตล์เดียวกับเซ็ตเทพ (art2) แต่มีออร่า ตรา และประกายของเทพ
# ไอคอน: GameResources/ui/Icons/items/icon_{id}.png · ภาพในมือ: GameResources/items/weapons/w_{id}.png
import os, sys
from art2_core import Canvas, sheet
from art2_gear import weapon_icon, weapon_hand, emblem, ICON_DIR, HAND_DIR

# id: (โลหะ, ขอบ, ประกาย, ชนิดอาวุธ, ตกแต่ง, ตรา, สีออร่า)
RELICS = {
    "ngd_phoenix_blade":     ("#c8361a", "#ffd24a", "#ff8a3a", "flame_sword", {"wings"}, "flame", "#ff9a3a"),
    "ngd_dragonfang_katana": ("#2e6a46", "#e6b440", "#ffd24a", "katana", set(), "claw", "#ffd24a"),
    "ngd_soul_reaper":       ("#1c1428", "#7affd8", "#7affd8", "void_scythe", set(), "skull", "#7affd8"),
    "ngd_titan_maul":        ("#4a4e5a", "#d8a040", "#ff6a2a", "rock_hammer", set(), "mountain", "#ff6a2a"),
    "ngd_worldtree_staff":   ("#3a6a2a", "#c8ff6a", "#9aff7a", "star_staff", set(), "leaf", "#9aff7a"),
    "ngd_abyss_trident":     ("#246a8a", "#80e0d8", "#4affe8", "trident", set(), "wave", "#4affe8"),
    "ngd_storm_bow":         ("#3a4a6a", "#c8f0ff", "#fff27a", "wind_bow", set(), "bolt", "#fff27a"),
    "ngd_bloodmoon_dagger":  ("#6a121c", "#2a1a20", "#ff3a4a", "blood_sword", {"spikes"}, "moon", "#ff3a4a"),
    "ngd_sun_lance":         ("#d8a020", "#fff0b0", "#ffe27a", "bolt_spear", set(), "sun", "#ffe27a"),
    "ngd_frost_crown_staff": ("#6aa8e0", "#eaf8ff", "#9af0ff", "ice_staff", set(), "snow", "#9af0ff"),
    "ngd_clockwork_saw":     ("#b07e34", "#6a4426", "#5af0ff", "chainsword", {"gears"}, "gear", "#5af0ff"),
    "ngd_dragon_cleaver":    ("#7a2a2a", "#e6b440", "#ff8a4a", "dragon_axe", set(), "claw", "#ff8a4a"),
    "ngd_sakura_bow":        ("#f2b4c6", "#2a2030", "#ff7aa8", "moon_bow", set(), "flower", "#ff7aa8"),
}


# อาวุธเทพรุ่นแรก 7 ชิ้น (วาดใหม่ให้สไตล์เดียวกับชุดที่ 2)
RELICS_OLD = {
    "ngd_starfall_blade":  ("#2a3478", "#ffd24a", "#8fb4ff", "holy_sword", {"wings"}, "star", "#8fb4ff"),
    "ngd_chrono_blade":    ("#5af0ff", "#d0dcff", "#7af0ff", "katana", set(), "eye", "#7af0ff"),
    "ngd_thunder_spear":   ("#46508e", "#ffd84a", "#fff27a", "bolt_spear", set(), "bolt", "#fff27a"),
    "ngd_rift_scythe":     ("#3a2a5a", "#7a4aa0", "#ff5aff", "void_scythe", set(), "swirl", "#ff5aff"),
    "ngd_cosmos_hammer":   ("#3a3a7a", "#d0dcff", "#8fb4ff", "rock_hammer", set(), "star", "#8fb4ff"),
    "ngd_crescent_bow":    ("#d8e8ff", "#9aa6c8", "#8ad8ff", "moon_bow", set(), "moon", "#8ad8ff"),
    "ngd_galaxy_scepter":  ("#2a3478", "#d0dcff", "#b080ff", "star_staff", set(), "star", "#b080ff"),
}


def icon(rid, R):
    metal, trim, acc, kind, deco, emb, aura = R
    cv = Canvas(28, 28)
    weapon_icon(cv, rid, (metal, trim, acc, kind, deco, emb))
    emblem(cv, emb, 23, 5, 3.4, acc)
    cv.glow(aura, 75, 1)
    cv.sparkle(4, 4)
    return cv


def hand(rid, R):
    metal, trim, acc, kind, deco, emb, aura = R
    cv = weapon_hand(rid, (metal, trim, acc, kind, deco, emb))
    return cv


def build(write=True):
    os.makedirs(ICON_DIR, exist_ok=True); os.makedirs(HAND_DIR, exist_ok=True)
    prev = []
    for rid, R in list(RELICS.items()) + list(RELICS_OLD.items()):
        ic = icon(rid, R); prev.append(ic.img)
        if write: ic.save(os.path.join(ICON_DIR, "icon_%s.png" % rid))   # ภาพในมือ: art9_hand.py
    return prev


if __name__ == "__main__":
    prev = build(write="preview" not in sys.argv)
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(os.environ.get("TEMP", "."), "relics2.png")
    sheet(prev, out, scale=5, cols=7)
    print("ok", len(prev), out)
