# อาคารร้าน/บริการ/อุตสาหกรรม แยกสถาปัตยกรรมตามยุค (ต่อจาก art7)
#   โครงตัวอาคาร = ตามยุค (กระท่อม/อิฐโคลน/เรือนยาว/โครงไม้/หอเวท/แคปซูล)
#   จุดจำของแต่ละชนิด = คงไว้ทุกยุค (ป้ายรูปแก้วเบียร์ เตาอบ ทั่ง สระน้ำ หลุมศพ ฯลฯ) ให้ผู้เล่นรู้ว่าตึกอะไร
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from art7_eras import *  # noqa
from art7_eras import ERA_IDS, base_y
from art3_buildings import save_frames
from art2_core import sheet

# ---------------------------------------------------------------- ป้ายรูป 5x5
ICON = {
    "mug":   [".###.", ".#o##", ".#o#.", ".#o##", ".###."],
    "bread": [".....", ".###.", "#ooo#", "#####", "....."],
    "sword": ["....#", "...#.", "#.#..", ".#...", "#.#.."],
    "cross": [".#.", "###", ".#."],
    "book":  ["##.##", "#o#o#", "#o#o#", "#o#o#", "#####"],
    "horse": ["...##", "#####", "####.", "#..#.", "#..#."],
    "coin":  [".###.", "#o#o#", "#ooo#", "#o#o#", ".###."],
    "scale": ["..#..", "#####", "#.#.#", "#.#.#", ".###."],
    "flask": [".###.", "..#..", ".#o#.", "#ooo#", ".###."],
    "fish":  [".....", "#.##.", "#####", "#.##.", "....."],
    "pick":  ["####.", "...#.", "..#..", ".#...", "#...."],
    "herb":  ["..#..", ".###.", "#.#.#", "..#..", "..#.."],
}
ICOL = {"mug": ("#e0b040", "#fff0c0"), "bread": ("#a8641e", "#e0a050"), "sword": ("#c8d0dc", "#ffffff"), "cross": ("#e03a3a", "#ff8a8a"),
        "book": ("#3a5aa0", "#f0e8d0"), "horse": ("#8a5a30", "#c8905a"), "coin": ("#e0b040", "#fff27a"), "scale": ("#c8a040", "#fff0a0"),
        "flask": ("#5aa05a", "#9aff7a"), "fish": ("#7aa8c8", "#c8e8ff"), "pick": ("#a8a8b8", "#6a4a2a"), "herb": ("#4aa04a", "#9aff7a")}


def sign_icon(b, x, y, kind, era):
    pat = ICON[kind]; h = len(pat); w = len(pat[0])
    board = {"stone": "#9a7650", "bronze": "#d8b880", "iron": "#7a5a3a", "kingdom": "#6a4426", "magic": "#3a2a5a", "star": "#2a3a5a"}[era]
    rect(b, x - 1, y - 1, x + w, y + h, board, None)
    c1, c2 = ICOL[kind]
    for j, row in enumerate(pat):
        for i, ch in enumerate(row):
            if ch == "#": b.put(x + i, y + j, R(c1)[3])
            elif ch == "o": b.put(x + i, y + j, R(c2)[3])
    if era == "magic": b.put(x + w, y - 1, R(RUNE)[4])
    if era == "star":
        for i in range(-1, w + 1): b.put(x + i, y + h, R(GLASS)[3])


def hanging_sign(b, x, y, kind, era):
    for i in range(3): b.put(x + i, y - 2, (60, 40, 26, 255))
    b.put(x + 2, y - 1, (60, 40, 26, 255))
    sign_icon(b, x + 1, y, kind, era)


def body(b, era, x0, x1, base, h=8, roofh=10, door_=True):
    """ตัวอาคารตามยุค คืนค่า y ของขอบบนผนัง"""
    cx = (x0 + x1) // 2
    if era == "stone":
        r = (x1 - x0) // 2
        hut(b, cx, base, min(r, 10), h + 1, door_=door_)
        return base - h
    if era == "bronze":
        mudbox(b, x0, x1, base - h, base)
        if door_: rect(b, cx - 1, base - 5, cx + 1, base, "#3a2414", None, outline=False)
        hole_window(b, x0 + 2, base - h + 3); hole_window(b, x1 - 3, base - h + 3)
        return base - h
    if era == "iron":
        longhouse(b, x0, x1, base, max(5, h - 2), roofh, STRAW, shields=False, door_=door_)
        return base - max(5, h - 2)
    if era == "kingdom":
        tudor(b, x0, x1, base, 2 if h >= 9 else 1, roofh, chim=False, dormer=False)
        return base - 7
    if era == "magic":
        spire_house(b, x0, x1, base, h + 1, roofh + 4, 2)
        return base - h - 1
    hover_pad(b, x0 - 1, x1 + 1, base)
    capsule(b, x0, x1, base - 1, h + 1)
    glass_dome(b, cx, base - h - 2, max(3, (x1 - x0) // 4))
    return base - h - 1


# ---------------------------------------------------------------- ชนิดอาคาร
def tavern(b, era):
    base = base_y(b); W = b.w
    top = body(b, era, 3, W - 9, base, 10, 12)
    hanging_sign(b, W - 8, top + 2, "mug", era)
    barrel(b, W - 6, base - 3); barrel(b, W - 3, base - 3)
    if era == "stone": firepit(b, W - 4, base - 5)
    if era in ("kingdom", "iron"): chimney(b, 6, top - 12, "#8a5a48")


def stall(b, era):
    base = base_y(b); W = b.w; x0, x1 = 3, W - 4
    if era == "stone":
        for x in (x0, x1):
            for y in range(base - 9, base + 1): b.put(x, y, (110, 80, 50, 255))
        rect(b, x0, base - 11, x1, base - 9, "#c8a070", "hide"); kc_dots(b, x0 + 1, x1 - 1, base - 10)
    elif era == "bronze":
        rect(b, x0, base - 4, x1, base, MUD, "brick"); awning(b, x0, x1, base - 11, 3)
    elif era == "iron":
        rect(b, x0, base - 4, x1, base, PLANK, "wood"); poly(b, [(x0 - 1, base - 7), (x1 + 1, base - 7), (x1 - 1, base - 12), (x0 + 1, base - 12)], STRAW, "thatch")
    elif era == "kingdom":
        rect(b, x0, base - 4, x1, base, "#a87a44", "wood")
        for j, x in enumerate(range(x0 - 1, x1 + 2, 2)):
            for y in range(base - 13, base - 9): b.put(x, y, KC if j % 2 == 0 else (240, 236, 224, 255)); b.put(x + 1, y, KC if j % 2 == 0 else (240, 236, 224, 255))
        for x in (x0, x1):
            for y in range(base - 9, base - 3): b.put(x, y, (90, 60, 36, 255))
    elif era == "magic":
        rect(b, x0, base - 4, x1, base, MSTONE, "stone"); mushroom(b, (x0 + x1) // 2, base - 4, 8, 3)
    else:
        capsule(b, x0, x1, base, 8, glass_band=False)
        for x in range(x0 + 2, x1 - 1): b.put(x, base - 10, (*R(GLASS)[3][:3], 200))
    for k in range(4): b.put(x0 + 2 + k * 3, base - 5, b.rnd.choice(((230, 80, 60, 255), (240, 200, 60, 255), (100, 190, 80, 255), (220, 150, 230, 255))))


def shrine(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone": menhir(b, cx, base, 15, 3); kc_cloth(b, cx - 2, base - 12, cx + 2, base - 9); firepit(b, cx - 6, base); firepit(b, cx + 6, base)
    elif era == "bronze": ziggurat(b, cx, base, 3, 10, 3, 4); rect(b, cx - 2, base - 15, cx + 2, base - 12, "#e0b040", "gold")
    elif era == "iron": rect(b, cx - 5, base - 7, cx + 5, base, PLANK, "wood"); stave_roof(b, cx, base - 7, 8, 5, 2)
    elif era == "kingdom":
        rect(b, cx - 5, base - 10, cx + 5, base, STONEW, "stone"); roof_gable(b, cx - 5, cx + 5, base - 10, 6, "KC", "slate")
        for y in range(base - 22, base - 15): b.put(cx, y, (220, 200, 120, 255))
        for x in range(cx - 2, cx + 3): b.put(x, base - 20, (220, 200, 120, 255))
        door(b, cx, base - 1, 3, 5, "#4a2c18", "#5a5a64")
    elif era == "magic":
        poly(b, [(cx - 4, base), (cx + 4, base), (cx + 2, base - 14), (cx, base - 17), (cx - 2, base - 14)], MSTONE, "stone")
        for y in (base - 12, base - 7): rune_window(b, cx - 1, y)
        float_crystal(b, cx, base - 19, "#ff9aff", 5)
    else:
        rect(b, cx - 5, base - 3, cx + 5, base, ALLOY, "marble")
        for y in range(base - 18, base - 3): b.put(cx, y, (*R(GLASS)[3][:3], 200)); b.put(cx + 1, y, (*R(GLASS)[2][:3], 160))
        ell(b, cx - 3, base - 22, cx + 4, base - 15, "#e6b440", "gold")
    if era != "kingdom":
        flowers(b, 1, base, 2)


def bakery(b, era):
    base = base_y(b); W = b.w
    top = body(b, era, 3, W - 11, base, 8, 10)
    # เตาอบโดมข้างอาคาร (ทุกยุค) — สตาร์ = เตาไมโครเวฟทรงแคปซูล
    ox = W - 9
    if era == "star":
        capsule(b, ox, ox + 7, base, 7, glass_band=False); rect(b, ox + 2, base - 5, ox + 5, base - 2, "#ff8a3a", None)
    else:
        col = {"stone": "#8a7a68", "bronze": "#c08a5a", "iron": "#8a6a50", "kingdom": "#a85a40", "magic": MSTONE}[era]
        ell(b, ox, base - 8, ox + 8, base + 1, col, "brick")
        rect(b, ox + 3, base - 4, ox + 5, base, "#2a1a10", None)
        b.put(ox + 4, base - 2, (255, 160, 60, 255)); b.put(ox + 3, base - 1, (255, 220, 120, 255))
    sign_icon(b, 5, top + 2 if era != "stone" else base - 14, "bread", era)


def forge(b, era):
    base = base_y(b); W = b.w
    top = body(b, era, 3, W - 13, base, 8, 9)
    # ลานตีเหล็ก: ทั่ง + เตาไฟ + ปล่อง
    ax = W - 11
    rect(b, ax, base - 3, ax + 5, base - 2, "#5a5a64", None); rect(b, ax + 2, base - 1, ax + 3, base, "#4a4a52", None)
    fx = W - 4
    col = {"stone": "#7a7060", "bronze": MUD, "iron": "#6a6a70", "kingdom": "#7a5a48", "magic": MSTONE, "star": ALLOY}[era]
    rect(b, fx - 2, base - 6, fx + 2, base, col, "brick" if era != "star" else "marble")
    b.put(fx, base - 3, (255, 140, 40, 255)); b.put(fx - 1, base - 3, (255, 220, 120, 255)); b.put(fx + 1, base - 2, (255, 90, 30, 255))
    if era == "star": b.put(fx, base - 3, R(GLASS)[4])
    chimney(b, fx - 1, base - 16, "#6a5a50")
    sign_icon(b, ax, base - 12, "sword", era)


def healer(b, era):
    base = base_y(b); W = b.w
    top = body(b, era, 3, W - 8, base, 8, 10)
    hanging_sign(b, W - 7, top + 1, "cross" if era in ("kingdom", "star", "magic") else "herb", era)
    for x in (W - 6, W - 3):   # กระถางสมุนไพร
        ell(b, x - 1, base - 3, x + 2, base, "#a0603a", None); b.put(x, base - 4, (90, 170, 70, 255)); b.put(x + 1, base - 5, (110, 200, 80, 255))


def school(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    top = body(b, era, 3, W - 4, base, 10, 12)
    sign_icon(b, cx - 2, base - 15 if era == "stone" else top - 1, "book", era)
    # ระฆังโรงเรียน
    for y in range(top - 6 if era != "stone" else base - 22, top if era != "stone" else base - 16): b.put(W - 6, y, (90, 60, 36, 255))
    ell(b, W - 8, top - 6 if era != "stone" else base - 22, W - 4, top - 2 if era != "stone" else base - 18, "#e0b040", None)


def stable(b, era):
    base = base_y(b); W = b.w
    body(b, era, 2, 15, base, 7, 8)
    fence(b, 17, W - 2, base)
    hx0 = 21
    for dx, dy in ((0, -4), (1, -4), (2, -4), (3, -4), (4, -5), (5, -6), (5, -5), (0, -3), (4, -3), (0, -2), (4, -2), (0, -1), (4, -1)):
        b.put(hx0 + dx, base + dy, (130, 80, 40, 255))
    b.put(hx0 + 5, base - 7, (60, 40, 20, 255))
    if era == "star":   # ม้าหุ่นยนต์
        for dx in range(0, 6): b.put(hx0 + dx, base - 4, R(ALLOY)[3])
        b.put(hx0 + 5, base - 6, R(GLASS)[4])
    sign_icon(b, W - 8, base - 11, "horse", era)


def bank(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    top = body(b, era, 3, W - 4, base, 11, 11)
    if era in ("bronze", "kingdom"):
        for x in range(6, W - 5, 5): column(b, x, base - 9, base)
    if era == "kingdom": rect(b, 3, base - 12, W - 4, base - 10, "#ece0c0", None)
    sign_icon(b, cx - 2, base - 16 if era == "stone" else top + 1, "coin", era)
    for i, x in enumerate((W - 4, W - 2)):   # กองทอง
        b.put(x, base - 1, (230, 180, 60, 255)); b.put(x, base - 2 - i, (255, 230, 120, 255))
    if era == "star":
        ell(b, cx - 3, base - 8, cx + 3, base - 2, "#8a8aa0", None); b.put(cx, base - 5, R(GLASS)[4])   # ประตูห้องนิรภัย


def fountain(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    col = {"stone": "#8a8478", "bronze": MUD, "iron": "#7a7468", "kingdom": STONEW, "magic": MSTONE, "star": ALLOY}[era]
    ell(b, 2, base - 6, W - 3, base + 1, col, "stone" if era != "star" else "marble")
    ell(b, 4, base - 5, W - 5, base - 1, "#4a8ad0" if era != "magic" else "#7a5ad0", None, outline=False)
    if era == "stone":
        menhir(b, cx, base - 3, 9, 2)
    else:
        rect(b, cx - 1, base - 12, cx + 1, base - 3, col, None)
        ell(b, cx - 4, base - 14, cx + 4, base - 11, col, None)
    water = R("#8ad8ff") if era != "magic" else R(RUNE)
    for dx, dy in ((0, -16), (-2, -15), (2, -15), (-4, -12), (4, -12), (0, -18)):
        b.put(cx + dx, base + dy, (*water[3][:3], 220))
    if era == "kingdom": b.put(cx, base - 19, KC)
    if era == "star": antenna(b, cx, base - 24, 5)


def cemetery(b, era):
    base = base_y(b); W = b.w
    if era == "kingdom":
        for x in range(1, W - 1, 2): b.put(x, base - 6, (60, 60, 70, 255)); b.put(x, base - 5, (60, 60, 70, 255))
    for i, x in enumerate(range(4, W - 3, 6)):
        y = base - (i % 2) * 2
        if era == "stone":   # กองหิน
            ell(b, x - 2, y - 4, x + 2, y, "#8a8478", "stone")
        elif era == "bronze":   # โถดิน
            ell(b, x - 2, y - 5, x + 2, y, "#b0603a", None); b.put(x, y - 6, (90, 50, 30, 255))
        elif era == "iron":   # หินรูน
            rect(b, x - 1, y - 7, x + 1, y, "#8a8478", "stone"); b.put(x, y - 5, KC); b.put(x, y - 3, KC)
        elif era == "kingdom":   # ไม้กางเขน/ป้ายหิน
            if i % 2: rect(b, x - 2, y - 6, x + 2, y, "#a8a49a", "stone")
            else:
                for yy in range(y - 7, y + 1): b.put(x, yy, (120, 110, 100, 255))
                for xx in range(x - 2, x + 3): b.put(xx, y - 5, (120, 110, 100, 255))
        elif era == "magic":   # หลุมศพมีวิญญาณเรืองแสง
            rect(b, x - 2, y - 5, x + 2, y, MSTONE, "stone"); b.put(x, y - 8, (*R(RUNE)[4][:3], 200)); b.put(x + 1, y - 9, (*R(RUNE)[3][:3], 150))
        else:   # เสาอนุสรณ์โฮโลแกรม
            rect(b, x - 1, y - 6, x + 1, y, ALLOY, None); b.put(x, y - 8, R(GLASS)[4]); b.put(x, y - 9, (*R(GLASS)[3][:3], 160))
        for dx in (-1, 0, 1): b.put(x + dx, y + 1, (90, 70, 50, 255))
    if era in ("iron", "kingdom"): bush(b, W - 6, base - 3, "#3a6a3a")


def bathhouse(b, era):
    base = base_y(b); W = b.w
    top = body(b, era, 2, W - 13, base, 9, 10)
    px = W - 12
    col = {"stone": "#8a8478", "bronze": MUD, "iron": PLANK, "kingdom": STONEW, "magic": MSTONE, "star": ALLOY}[era]
    rect(b, px, base - 4, W - 2, base, col, "stone" if era not in ("iron",) else "wood")
    for x in range(px + 1, W - 2): b.put(x, base - 3, R("#6ac0e0")[3]); b.put(x, base - 2, R("#6ac0e0")[2])
    for i, (dx, dy) in enumerate(((2, -6), (3, -8), (5, -7), (6, -10), (8, -9))):   # ไอน้ำ
        b.put(px + dx, base + dy, (235, 235, 240, 180 - i * 25))
    if era == "kingdom": banner_wall(b, 5, top - 3, 4)


def courthouse(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    top = body(b, era, 3, W - 4, base, 12, 13)
    if era in ("bronze", "kingdom", "star"):
        for x in range(6, W - 5, 6): column(b, x, base - 10, base, "#ece0c0" if era != "star" else ALLOY)
    sign_icon(b, cx - 2, base - 18 if era == "stone" else top - 1, "scale", era)
    if era == "kingdom": flag(b, W - 5, top - 18, 7, True)


def alchemist(b, era):
    base = base_y(b); W = b.w
    top = body(b, era, 3, W - 11, base, 9, 12)
    cx = W - 6
    ell(b, cx - 4, base - 6, cx + 4, base, "#3a3a44", None)          # หม้อต้ม
    for dx, dy in ((-1, -7), (1, -8), (0, -10), (2, -11)): b.put(cx + dx, base + dy, (*R("#9aff7a")[3][:3], 210))
    b.put(cx, base - 6, R("#9aff7a")[4]); b.put(cx - 2, base - 6, R("#9aff7a")[3])
    b.put(cx - 3, base + 1, (255, 140, 40, 255)); b.put(cx + 2, base + 1, (255, 200, 80, 255))
    sign_icon(b, 5, base - 15 if era == "stone" else top + 1, "flask", era)


def fishery(b, era):
    base = base_y(b); W = b.w
    for x in range(W // 2, W):   # น้ำ
        for y in range(base - 3, base + 2): b.put(x, y, R("#3a7ac0")[2] if (x + y) % 3 else R("#3a7ac0")[3])
    pier = {"stone": LOG, "bronze": "#a07a4a", "iron": PLANK, "kingdom": "#8a6a44", "magic": MSTONE, "star": ALLOY}[era]
    rect(b, W // 2 - 2, base - 5, W - 2, base - 4, pier, "wood" if era not in ("magic", "star") else None)
    for x in range(W // 2, W - 1, 4):
        for y in range(base - 3, base + 2): b.put(x, y, R(pier)[1])
    body(b, era, 2, W // 2 - 2, base, 7, 8)
    sign_icon(b, W - 9, base - 12, "fish", era)
    # เรือเล็ก
    poly(b, [(W - 9, base - 2), (W - 2, base - 2), (W - 4, base), (W - 8, base)], "#7a5a3a" if era != "star" else ALLOY, None)


def mine(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    poly(b, [(0, base), (W - 1, base), (W - 6, base - 12), (cx + 2, base - 18), (cx - 4, base - 16), (5, base - 10)], "#7a7060", "stone")
    # ทางเข้า (ตามยุค)
    rect(b, cx - 4, base - 9, cx + 4, base, "#1a1410", None, outline=False)
    beam = {"stone": LOG, "bronze": "#a07a4a", "iron": PLANK, "kingdom": "#6a4426", "magic": MSTONE, "star": ALLOY}[era]
    for y in range(base - 10, base + 1): b.put(cx - 5, y, R(beam)[2]); b.put(cx + 5, y, R(beam)[2])
    for x in range(cx - 5, cx + 6): b.put(x, base - 10, R(beam)[3])
    # รางและรถแร่
    for x in range(cx + 5, W - 1): b.put(x, base, (90, 80, 70, 255))
    rect(b, W - 9, base - 4, W - 4, base - 1, "#5a5a64" if era != "star" else ALLOY, None)
    for x in range(W - 8, W - 4): b.put(x, base - 5, (200, 160, 80, 255) if x % 2 else (160, 200, 220, 255))
    if era == "magic": float_crystal(b, cx - 9, base - 14, RUNE, 6)
    if era == "star":
        for y in range(base - 24, base - 10): b.put(cx + 9, y, (150, 150, 170, 255))
        rect(b, cx + 6, base - 26, cx + 12, base - 23, PANEL, None); antenna(b, cx + 9, base - 33, 6)
    if era in ("kingdom", "iron"): sign_icon(b, 2, base - 8, "pick", era)
    if era == "stone": torch(b, cx - 6, base - 6); torch(b, cx + 6, base - 6)


def farm(b, era):
    base = base_y(b); W = b.w
    soil = "#7a5a3a"
    for row in range(4):   # แปลงปลูก
        y = base - row * 3
        for x in range(1, W - 1):
            b.put(x, y, R(soil)[2] if x % 2 else R(soil)[1])
            if x % 2 == 0: b.put(x, y - 1, R("#7ac04a" if era != "magic" else "#c07aff")[3])
    top = base - 12
    if era == "stone":
        hut(b, 7, top, 5, 6, door_=False)
    elif era == "bronze":
        for x in range(1, W - 1): b.put(x, base - 13, R("#4a8ad0")[3])   # คลองชลประทาน
        beehive(b, 7, top - 1, 5)
    elif era == "iron":
        longhouse(b, 2, 16, top, 5, 8, STRAW, shields=False, door_=True)
    elif era == "kingdom":   # กังหันลม
        rect(b, 5, top - 12, 11, top, STONEW, "stone"); roof_cone(b, 8, top - 12, 4, 5, "KC", "shingle")
        hx0, hy0 = 8, top - 13
        for d in range(1, 9):
            for sx, sy in ((1, 1), (-1, -1), (1, -1), (-1, 1)): b.put(hx0 + sx * d, hy0 + sy * d, (220, 210, 190, 255))
        b.put(hx0, hy0, (90, 60, 36, 255))
    elif era == "magic":
        float_crystal(b, 8, top - 4, "#9aff7a", 7); float_crystal(b, W - 8, top - 2, RUNE, 5)
    else:   # เรือนกระจก
        glass_dome(b, W // 2, top, 11); antenna(b, W // 2, top - 18, 6)


TYPES = {
    "tavern": (34, 32, tavern), "stall": (24, 22, stall), "shrine": (24, 22, shrine), "bakery": (32, 26, bakery),
    "forge": (34, 26, forge), "healer": (28, 26, healer), "school": (34, 30, school), "stable": (36, 22, stable),
    "bank": (32, 30, bank), "fountain": (24, 22, fountain), "cemetery": (32, 20, cemetery), "bathhouse": (32, 26, bathhouse),
    "courthouse": (34, 32, courthouse), "alchemist": (30, 26, alchemist), "fishery": (28, 22, fishery), "mine": (32, 26, mine),
    "farm": (30, 32, farm),
}


def build(write=True, only=None):
    prev = []
    for era in ERA_IDS:
        for t, (w, h, fn) in TYPES.items():
            bid = "%s_%s" % (era, t)
            if only and only not in bid: continue
            b = B(w, h, seed=hash(bid) & 0xffff)
            fn(b, era)
            if era == "magic": glow(b, "#7af0ff", 30)
            prev.append(b.img)
            if write: save_frames(bid, b.img)
    return prev


if __name__ == "__main__":
    prev = build(write="preview" not in sys.argv)
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(os.path.dirname(os.path.abspath(__file__)), "previews", "eras8.png")
    from PIL import Image
    cell = 100; types = list(TYPES)
    img = Image.new("RGBA", (cell * len(types), cell * 6), (74, 112, 52, 255))
    for i, im in enumerate(prev):
        r, c = divmod(i, len(types)); s = min(3, 96 / im.width, 96 / im.height)
        im = im.resize((int(im.width * s), int(im.height * s)), Image.NEAREST)
        img.alpha_composite(im, (c * cell + (cell - im.width) // 2, r * cell + cell - im.height - 2))
    img.save(out); print("ok", len(prev), out)
