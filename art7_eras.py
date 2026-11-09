# อาคารเมืองรุ่นใหม่: แต่ละยุคมี "สถาปัตยกรรม" (รูปทรง/โครงสร้าง) ของตัวเอง ไม่ใช่แค่เปลี่ยนสี
#   หิน     กระท่อมกลมหลังคากรวยหญ้า รั้วไม้แหลม โทเท็ม กองไฟ
#   สำริด   บ้านอิฐโคลนหลังคาแบน ผ้าใบกันแดด ซิกกูรัต เสาหิน ยุ้งโดมดิน
#   เหล็ก   เรือนยาวไม้ (แบบนอร์ส) หลังคาชันเกือบถึงพื้น หัวมังกรปลายจั่ว โล่กลม
#   อาณาจักร บ้านโครงไม้ 2 ชั้นชั้นบนยื่น หลังคาสีอาณาจักร ป้อมหินมีใบเสมา
#   เวทมนตร์ หอคอยหินม่วง หลังคากรวยบิดเกลียว คริสตัลลอย หน้าต่างรูนเรืองแสง
#   ดวงดาว  แคปซูลโลหะขาว โดมกระจก เสาอากาศ แผงโซลาร์ แท่นลอย
# สีอาณาจักร (KC) ใช้เป็น "สีประจำ" บนธง ผ้า แถบหลังคา — หลังคาเต็มผืนเป็นสีอาณาจักรเฉพาะยุคอาณาจักร (แบบเกมต้นฉบับ)
# ใช้: python art7_eras.py [preview out.png]
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from art3_build_core import *  # noqa
from art3_buildings import save_frames
from art2_core import sheet

ERA_IDS = ["stone", "bronze", "iron", "kingdom", "magic", "star"]


def base_y(b): return b.h - 3


def rect(b, x0, y0, x1, y1, col, t=None, outline=True):
    m, d = b.mask(); d.rectangle((x0, y0, x1, y1), fill=255); b.fill(m, R(col), tex(b, t) if t else None, outline=outline)


def poly(b, pts, col, t=None, outline=True):
    m, d = b.mask(); d.polygon(pts, fill=255); b.fill(m, R(col), tex(b, t) if t else None, outline=outline)


def ell(b, x0, y0, x1, y1, col, t=None, outline=True):
    m, d = b.mask(); d.ellipse((x0, y0, x1, y1), fill=255); b.fill(m, R(col), tex(b, t) if t else None, outline=outline)


def kc_cloth(b, x0, y0, x1, y1):
    rect(b, x0, y0, x1, y1, "KC", None)


def kc_dots(b, x0, x1, y, step=2):
    for x in range(x0, x1 + 1, step): b.put(x, y, KC)


def shadow(b, x0, x1, base):
    for x in range(x0, x1 + 1):
        for y in (base + 1, base + 2):
            if b.get(x, y)[3] == 0: b.put(x, y, (0, 0, 0, 60))


# =============================================================== ยุคหิน
THATCH, HIDE, LOG = "#b89a5a", "#9a7650", "#7a5a3a"


def hut(b, cx, base, r, h, door_=True, stripe=True):
    ell(b, cx - r, base - h, cx + r, base + 1, HIDE, "hide")
    if stripe: kc_dots(b, cx - r + 2, cx + r - 2, base - h // 2 - 1, 2)
    roof_cone(b, cx, base - h + 3, r + 2, h + 4, THATCH)
    for dx in (-1, 1): b.put(cx + dx, base - h - h - 0, (90, 60, 40, 255))   # ไม้ไขว้ยอดกรวย
    if door_:
        for y in range(base - 4, base + 1):
            for x in range(cx - 1, cx + 2): b.put(x, y, (40, 26, 16, 255))
        b.put(cx - 2, base - 4, (120, 90, 60, 255)); b.put(cx + 2, base - 4, (120, 90, 60, 255))


def palisade(b, x0, x1, base, h):
    for x in range(x0, x1 + 1, 2):
        hh = h - (1 if (x // 2) % 2 else 0)
        poly(b, [(x, base), (x, base - hh + 1), (x + 0.5, base - hh), (x + 1, base - hh + 1), (x + 1, base)], LOG, "wood")


def totem(b, x, base, h):
    cols = ["#8a5a3a", "#c84a3a", "#e0b040", "#3a8a9a"]
    for i in range(h // 3):
        rect(b, x - 1, base - (i + 1) * 3, x + 1, base - i * 3, cols[i % 4], None)
        b.put(x, base - i * 3 - 2, (20, 14, 10, 255))
    kc_cloth(b, x - 3, base - h - 1, x + 3, base - h + 1)


def firepit(b, x, base):
    for dx in (-2, -1, 0, 1, 2): b.put(x + dx, base, (110, 100, 90, 255))
    b.put(x, base - 1, (255, 140, 40, 255)); b.put(x - 1, base - 1, (255, 200, 80, 255)); b.put(x + 1, base - 1, (230, 90, 30, 255))
    b.put(x, base - 2, (255, 230, 140, 255))


def menhir(b, x, base, h, w=2):
    poly(b, [(x - w, base), (x - w, base - h + 1), (x, base - h), (x + w, base - h + 1), (x + w, base)], "#b0aa9c", "stone")


def rack(b, x, base):
    for dx in (0, 5):
        for y in range(base - 6, base + 1): b.put(x + dx, y, (100, 70, 40, 255))
    rect(b, x + 1, base - 6, x + 4, base - 2, "#c8a070", None)


# =============================================================== ยุคสำริด
MUD, SAND, PALM = "#c89a64", "#e2c896", "#4a8a3a"


def mudbox(b, x0, x1, top, base, col=MUD, roof_terrace=True):
    rect(b, x0, top, x1, base, col, "brick")
    rect(b, x0 - 1, top - 2, x1 + 1, top, SAND, "plaster")            # หลังคาแบนหนา
    for x in range(x0 + 1, x1, 3): b.put(x, top + 1, (90, 60, 36, 255))   # ปลายคานไม้โผล่
    if roof_terrace:
        for x in range(x0, x1 + 1, 3): b.put(x, top - 3, R(SAND)[3])
    shadow(b, x0, x1, base)


def hole_window(b, x, y, w=2, h=2):
    for yy in range(y, y + h):
        for xx in range(x, x + w): b.put(xx, yy, (54, 34, 22, 255))
    b.put(x, y, (255, 200, 110, 255))


def awning(b, x0, x1, y, depth=3):
    poly(b, [(x0, y), (x1, y), (x1 + 1, y + depth), (x0 - 1, y + depth)], "KC")
    for x in (x0 - 1, x1 + 1):
        for yy in range(y + depth, y + depth + 6): b.put(x, yy, (110, 80, 50, 255))


def column(b, x, top, base, col="#ece0c0"):
    rect(b, x - 1, top, x + 1, base, col, None)
    rect(b, x - 2, top - 1, x + 2, top, col, None)
    rect(b, x - 2, base - 1, x + 2, base, col, None)


def beehive(b, cx, base, r, col="#c08a5a"):
    ell(b, cx - r, base - r * 2 + 1, cx + r, base + 1, col, "brick")
    rect(b, cx - 1, base - 3, cx + 1, base, "#4a2c18", None)


def palm(b, x, base, h=10):
    for i in range(h):
        b.put(x + (1 if i > h // 2 else 0), base - i, (130, 90, 50, 255))
    tx, ty = x + 1, base - h
    for dx, dy in ((-4, 1), (-3, 0), (-2, -1), (2, -1), (3, 0), (4, 1), (-1, -2), (1, -2), (0, 1)):
        b.put(tx + dx, ty + dy, R(PALM)[2]); b.put(tx + dx, ty + dy + 1, R(PALM)[1])


def pots(b, x, base):
    for i, c in enumerate(("#b0603a", "#c8804a")):
        ell(b, x + i * 4, base - 3, x + i * 4 + 3, base, c, None)


def ziggurat(b, cx, base, levels, w0, step, h, col=MUD):
    for i in range(levels):
        w = w0 - i * step
        rect(b, cx - w, base - (i + 1) * h, cx + w, base - i * h, col, "brick")
        for x in range(cx - w + 1, cx + w, 3): b.put(x, base - (i + 1) * h + 1, R(SAND)[3])
    # บันไดกลาง
    for y in range(base - levels * h + 1, base + 1):
        b.put(cx, y, R(SAND)[3]); b.put(cx - 1, y, R(SAND)[2]); b.put(cx + 1, y, R(SAND)[2])


# =============================================================== ยุคเหล็ก (นอร์ส)
PLANK, TURF, STRAW = "#6a4a2e", "#6a8a3a", "#a08a50"


def dragon_head(b, x, y, side):
    pts = [(0, 0), (side, -1), (side * 2, -2), (side * 2, -3), (side, -3)]
    for dx, dy in pts: b.put(x + dx, y + dy, (70, 44, 26, 255))
    b.put(x + side * 2, y - 4, (70, 44, 26, 255))


def longhouse(b, x0, x1, base, wallh, roofh, roof=STRAW, shields=True, door_=True):
    rect(b, x0, base - wallh, x1, base, PLANK, "wood")
    for x in range(x0 + 2, x1 - 1, 3):
        for y in range(base - wallh + 1, base): b.put(x, y, R(PLANK)[1])     # แผ่นไม้ตั้ง
    top = base - wallh
    poly(b, [(x0 - 3, top + 2), (x1 + 3, top + 2), (x1 - 2, top - roofh), (x0 + 2, top - roofh)], roof, "thatch")
    for x in range(x0 + 2, x1 - 1): b.put(x, top - roofh, KC); b.put(x, top - roofh - 1, R("KC")[1])   # สันหลังคาสีอาณาจักร
    dragon_head(b, x0 + 2, top - roofh, -1); dragon_head(b, x1 - 2, top - roofh, 1)
    b.shade_band(x0, x1, top + 3, 120, 2)
    if shields:
        for x in range(x0 + 3, x1 - 2, 6):
            ell(b, x - 2, base - wallh + 2, x + 2, base - wallh + 6, "KC", None)
            b.put(x, base - wallh + 4, (200, 190, 160, 255))
    if door_:
        cx = (x0 + x1) // 2
        rect(b, cx - 2, base - 6, cx + 2, base, "#3a2414", None, outline=False)
        for y in range(base - 6, base + 1): b.put(cx - 3, y, R(PLANK)[0]); b.put(cx + 3, y, R(PLANK)[0])
    shadow(b, x0, x1, base)


def stave_roof(b, cx, base, w, h, tiers=3):
    for i in range(tiers):
        ww = w - i * 3; yb = base - i * (h - 1)
        poly(b, [(cx - ww, yb), (cx + ww, yb), (cx + ww - 3, yb - h), (cx - ww + 3, yb - h)], "#3a2a22", "shingle")
        dragon_head(b, cx - ww + 3, yb - h, -1); dragon_head(b, cx + ww - 3, yb - h, 1)
    top = base - tiers * (h - 1)
    for y in range(top - 5, top + 1): b.put(cx, y, (90, 60, 36, 255))
    b.put(cx, top - 6, KC); b.put(cx + 1, top - 6, KC)


def wattle(b, x0, x1, y):
    for x in range(x0, x1 + 1):
        b.put(x, y, (120, 90, 50, 255)) if x % 3 else b.put(x, y - 1, (100, 70, 40, 255))
        if x % 3 == 0: b.put(x, y, (90, 60, 30, 255)); b.put(x, y + 1, (90, 60, 30, 255))


# =============================================================== ยุคอาณาจักร (โครงไม้ยุโรป)
STONEW, PLASTER, TIMBER = "#9a968c", "#ece2c8", "#4a2c18"


def tudor(b, x0, x1, base, floors=2, roofh=12, chim=True, dormer=True):
    fh = 7
    rect(b, x0, base - fh, x1, base, STONEW, "stone")
    top = base - fh
    for f in range(1, floors):
        ox = f   # ชั้นบนยื่นออก
        wall(b, x0 - ox, top - fh, x1 + ox, top, PLASTER, "plaster", TIMBER)
        for x in range(x0 - ox, x1 + ox + 1): b.put(x, top, R(TIMBER)[1])
        top -= fh
    ox = floors - 1
    roof_gable(b, x0 - ox, x1 + ox, top, roofh, "KC", "tile")
    if chim: chimney(b, x1 - 3, top - roofh + 2, "#8a5a48")
    if dormer and x1 - x0 > 12:
        cx = (x0 + x1) // 2
        poly(b, [(cx - 3, top - 3), (cx + 3, top - 3), (cx, top - 8)], "KC", "tile")
        window(b, cx - 1, top - 5, 2, 2, "#ffe08a", TIMBER, sill=False)
    # หน้าต่าง/ประตู
    cx = (x0 + x1) // 2
    door(b, cx, base - 1, 3, 5, "#6a4426", "#5a5a64")
    for wx in (x0 + 2, x1 - 3): window(b, wx, base - fh + 2, 2, 2, "#ffe08a", TIMBER)
    yy = base - fh
    for f in range(1, floors):
        yy -= fh
        for wx in range(x0 + 1, x1 - 1, 5): window(b, wx, yy + 2, 2, 3, "#ffe08a", TIMBER, sill=False)
    shadow(b, x0, x1, base)


def keep_tower(b, x0, x1, base, h, cone=True):
    rect(b, x0, base - h, x1, base, STONEW, "stone")
    battlements(b, x0, x1, base - h, STONEW)
    cx = (x0 + x1) // 2
    if cone: roof_cone(b, cx, base - h - 1, (x1 - x0) // 2 + 2, 10, "KC", "slate")
    for y in range(base - h + 4, base - 3, 6): window(b, cx - 1, y, 2, 3, "#ffe08a", "#4a4a52", arch=True, sill=False)


# =============================================================== ยุคเวทมนตร์
MSTONE, MROOF, RUNE = "#6a5a8a", "#2e2e66", "#7af0ff"


def twist_cone(b, cx, base, w, h, col=MROOF, bend=3):
    pts = [(cx - w, base), (cx + w, base)]
    poly(b, [(cx - w, base), (cx + w, base), (cx + bend, base - h + 3), (cx + bend + 2, base - h), (cx + bend - 1, base - h + 2)], col, "slate")
    for i in range(1, 4):   # เกลียว
        y = base - i * h // 4
        for x in range(cx - w + i * w // 4, cx + w - i * w // 4):
            if (x + i) % 3 == 0: b.put(x, y, R("KC")[2])
    b.put(cx + bend + 2, base - h - 1, (255, 240, 160, 255))


def rune_window(b, x, y):
    b.put(x, y, R(RUNE)[4]); b.put(x + 1, y, R(RUNE)[3]); b.put(x, y + 1, R(RUNE)[3]); b.put(x + 1, y + 1, R(RUNE)[2])
    b.put(x - 1, y, (40, 30, 70, 255)); b.put(x + 2, y + 1, (40, 30, 70, 255))


def float_crystal(b, x, y, col=RUNE, h=5):
    poly(b, [(x, y), (x + 2, y - h // 2), (x, y - h), (x - 2, y - h // 2)], col, None)
    b.put(x, y - h // 2, (255, 255, 255, 230))
    b.put(x, y + 2, (*R(col)[2][:3], 120))


def spire_house(b, x0, x1, base, wallh, roofh, bend=3):
    rect(b, x0, base - wallh, x1, base, MSTONE, "stone")
    cx = (x0 + x1) // 2
    twist_cone(b, cx, base - wallh, (x1 - x0) // 2 + 3, roofh, MROOF, bend)
    for wx in range(x0 + 2, x1 - 1, 4): rune_window(b, wx, base - wallh + 3)
    rect(b, cx - 1, base - 5, cx + 1, base, "#2a1a3a", None)
    b.put(cx, base - 6, R(RUNE)[4])
    shadow(b, x0, x1, base)


def mushroom(b, cx, base, r, stalk=6):
    rect(b, cx - 2, base - stalk, cx + 2, base, "#e8dcc8", "plaster")
    ell(b, cx - r, base - stalk - r, cx + r, base - stalk + 2, "KC", None)
    for dx, dy in ((-r // 2, -r // 2), (r // 3, -r // 2 - 1), (0, -2)): b.put(cx + dx, base - stalk + dy, (255, 240, 220, 255))
    rune_window(b, cx - 1, base - stalk + 2)


# =============================================================== ยุคดวงดาว
ALLOY, GLASS, PANEL = "#e6e8f0", "#7ad8ff", "#2a4a8a"


def capsule(b, x0, x1, base, h, glass_band=True):
    m, d = b.mask(); d.rounded_rectangle((x0, base - h, x1, base), radius=min(5, h // 2), fill=255)
    b.fill(m, R(ALLOY), tex(b, "marble"))
    if glass_band:
        y = base - h + 3
        for x in range(x0 + 2, x1 - 1):
            b.put(x, y, R(GLASS)[3]); b.put(x, y + 1, R(GLASS)[2])
    for x in range(x0 + 2, x1 - 1): b.put(x, base - 1, KC)       # แถบไฟสีอาณาจักร
    shadow(b, x0, x1, base)


def glass_dome(b, cx, base, r):
    m, d = b.mask(); d.pieslice((cx - r, base - r, cx + r, base + r), 180, 360, fill=255)
    rp = R(GLASS, 0.7)
    mp = m.load()
    for y in range(b.h):
        for x in range(b.w):
            if mp[x, y]: b.put(x, y, (*rp[2][:3], 200) if (x + y) % 4 else (*rp[3][:3], 220))
    for i in range(r - 1): b.put(cx - r // 2 + i // 3, base - r + 2 + i // 2, (255, 255, 255, 230)) if i < 4 else None
    for x in range(cx - r, cx + r + 1): b.put(x, base, R(ALLOY)[1])


def antenna(b, x, top, h):
    for y in range(top, top + h): b.put(x, y, (150, 150, 170, 255))
    b.put(x, top - 1, (255, 60, 60, 255)); b.put(x - 1, top + 2, (150, 150, 170, 255)); b.put(x + 1, top + 2, (150, 150, 170, 255))


def solar(b, x0, x1, y):
    poly(b, [(x0, y), (x1, y), (x1 - 2, y - 3), (x0 + 2, y - 3)], PANEL, None)
    for x in range(x0 + 2, x1 - 1, 3): b.put(x, y - 1, R(PANEL)[4])


def hover_pad(b, x0, x1, base):
    ell(b, x0, base - 1, x1, base + 2, "#4a4a5a", None)
    for x in range(x0 + 2, x1 - 1, 3): b.put(x, base + 1, R(GLASS)[4])


# =============================================================== บ้าน (4 ขนาด + เพิง + บ้านแถว + คฤหาสน์)
def house(b, era, size):
    """size: 0 เล็ก · 1 กลาง · 2 ใหญ่ · 3 มีสวน"""
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":
        r = (6, 7, 8, 7)[size]; h = (8, 9, 10, 9)[size]
        hut(b, cx - (3 if size >= 2 else 0), base, r, h)
        if size >= 2: hut(b, cx + r + 2, base, 4, 6, door_=False)
        if size == 3: rack(b, 1, base)
        if size >= 1: firepit(b, cx + r + (8 if size >= 2 else 3), base)
        return
    if era == "bronze":
        x0, x1 = (5, W - 6) if size == 0 else (3, W - 4)
        top = base - (7, 8, 13, 8)[size]
        mudbox(b, x0, x1, top, base)
        if size == 2: rect(b, x0 + 3, top - 7, x1 - 6, top - 2, MUD, "brick"); rect(b, x0 + 2, top - 9, x1 - 5, top - 7, SAND, None)
        hole_window(b, x0 + 2, top + 3); hole_window(b, x1 - 3, top + 3)
        rect(b, cx - 1, base - 5, cx + 1, base, "#3a2414", None, outline=False)
        awning(b, x0 + 1, min(x1 - 1, x0 + 8), top + 1) if size >= 1 else kc_cloth(b, x0 + 1, top + 1, x0 + 3, top + 4)
        if size == 3: palm(b, W - 3, base, 11); pots(b, 0, base)
        elif size >= 1: pots(b, x1 + 1 if x1 + 7 < W else x0 - 7, base)
        return
    if era == "iron":
        x0, x1 = (3, W - 4)
        longhouse(b, x0, x1, base, (5, 5, 6, 5)[size], (9, 11, 13, 10)[size], TURF if size == 3 else STRAW, shields=size >= 1)
        if size == 3: wattle(b, 0, 4, base); wattle(b, W - 5, W - 1, base)
        if size == 2: chimney(b, x1 - 5, base - 6 - 13, "#6a6a70")
        return
    if era == "kingdom":
        x0, x1 = (5, W - 6) if size == 0 else (4, W - 5)
        tudor(b, x0, x1, base, 1 if size == 0 else 2, (9, 10, 12, 10)[size], chim=size >= 1, dormer=size == 2)
        if size == 3: flowers(b, 0, base, 2); flowers(b, W - 4, base, 2); fence(b, 0, 3, base)
        return
    if era == "magic":
        x0, x1 = (6, W - 7) if size == 0 else (4, W - 5)
        if size == 3:
            mushroom(b, cx - 4, base, 8, 7); float_crystal(b, W - 3, base - 10)
        else:
            spire_house(b, x0, x1, base, (8, 9, 12, 9)[size], (12, 14, 17, 14)[size], bend=(2, 3, 4, 3)[size])
            float_crystal(b, x1 + 1, base - (8, 10, 14, 10)[size])
            if size == 2: rect(b, x0 - 3, base - 6, x0, base, MSTONE, "stone"); rune_window(b, x0 - 2, base - 4)
        return
    if era == "star":
        x0, x1 = (4, W - 5) if size == 0 else (3, W - 4)
        h = (8, 9, 12, 9)[size]
        hover_pad(b, x0 - 1, x1 + 1, base)
        capsule(b, x0, x1, base - 1, h)
        glass_dome(b, (x0 + x1) // 2, base - 1 - h, (x1 - x0) // 3)
        if size >= 1: solar(b, x0 + 1, x0 + 7, base - h - 1)
        antenna(b, x1 - 2, base - h - 8, 7)
        if size == 3: bush(b, 0, base - 3, "#5aa07a")
        return


def shack(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":
        poly(b, [(3, base), (cx, base - 9), (W - 4, base)], HIDE, "hide"); kc_dots(b, cx - 2, cx + 2, base - 4); firepit(b, W - 2, base)
    elif era == "bronze":
        mudbox(b, 4, W - 5, base - 6, base, roof_terrace=False); kc_cloth(b, 5, base - 5, 7, base - 3)
    elif era == "iron":
        longhouse(b, 3, W - 4, base, 3, 7, STRAW, shields=False, door_=True)
    elif era == "kingdom":
        rect(b, 4, base - 6, W - 5, base, "#8a6a48", "wood"); roof_gable(b, 4, W - 5, base - 6, 6, "#7a6a50", "thatch")
        door(b, cx, base - 1, 2, 4, "#5a3a20", "#3a2414", False)
    elif era == "magic":
        mushroom(b, cx, base, 6, 5)
    else:
        capsule(b, 3, W - 4, base, 7); antenna(b, W - 6, base - 13, 6)


def rowhouse(b, era):
    base = base_y(b); W = b.w
    if era == "kingdom":
        for i, (x0, x1) in enumerate(((2, 13), (14, 25), (26, 37))):
            tudor(b, x0, x1, base, 2 + (i == 1), 8 + (i % 2) * 2, chim=i != 1, dormer=False)
        return
    if era == "bronze":
        mudbox(b, 2, W - 3, base - 10, base); rect(b, 12, base - 16, 27, base - 10, MUD, "brick")
        for x in (5, 15, 24, 33): hole_window(b, x, base - 7)
        awning(b, 4, 11, base - 9); awning(b, 26, 33, base - 9); return
    if era == "iron":
        longhouse(b, 2, W - 3, base, 6, 12, STRAW); return
    if era == "magic":
        for i, x0 in enumerate((2, 15, 28)): spire_house(b, x0, x0 + 9, base, 9 + (i == 1) * 3, 13 + i, bend=2 + i)
        return
    if era == "star":
        hover_pad(b, 1, W - 2, base); capsule(b, 2, W - 3, base - 1, 12)
        for cx in (10, 20, 30): glass_dome(b, cx, base - 13, 4)
        antenna(b, W - 5, base - 22, 8); return
    for cx in (8, 20, 32): hut(b, cx, base, 6, 8)


def mansion(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":
        palisade(b, 0, W - 1, base, 6); hut(b, cx, base - 1, 9, 11); totem(b, 3, base - 1, 10); totem(b, W - 4, base - 1, 10); return
    if era == "bronze":
        mudbox(b, 3, W - 4, base - 10, base); rect(b, 8, base - 18, W - 9, base - 10, MUD, "brick"); rect(b, 7, base - 20, W - 8, base - 18, SAND, None)
        for x in (6, W - 7): column(b, x, base - 9, base)
        awning(b, 11, W - 12, base - 17); palm(b, 1, base, 12); hole_window(b, 12, base - 7); hole_window(b, W - 14, base - 7)
        rect(b, cx - 2, base - 6, cx + 2, base, "#3a2414", None, outline=False); return
    if era == "iron":
        longhouse(b, 2, W - 3, base, 7, 15, STRAW); keep = None; flag(b, cx, base - 25, 6); return
    if era == "kingdom":
        tudor(b, 9, W - 10, base, 3, 12, dormer=True)
        keep_tower(b, 1, 8, base, 18); keep_tower(b, W - 9, W - 2, base, 18); return
    if era == "magic":
        spire_house(b, 9, W - 10, base, 14, 20, 4)
        spire_house(b, 1, 9, base, 9, 12, 2); spire_house(b, W - 10, W - 2, base, 9, 12, -2)
        float_crystal(b, cx + 7, base - 30, "#ff9aff", 6); return
    hover_pad(b, 1, W - 2, base); capsule(b, 3, W - 4, base - 1, 14); glass_dome(b, cx, base - 15, 8)
    solar(b, 3, 11, base - 15); solar(b, W - 12, W - 4, base - 15); antenna(b, W - 6, base - 26, 10)


# =============================================================== ศาลากลาง
def hall(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":
        palisade(b, 0, W - 1, base, 7)
        hut(b, cx, base - 1, 12, 13)
        totem(b, 4, base - 1, 14); totem(b, W - 5, base - 1, 14); firepit(b, cx + 15, base - 1); return
    if era == "bronze":
        ziggurat(b, cx, base, 3, 17, 4, 6)
        for x in range(cx - 13, cx + 14, 6): column(b, x, base - 5, base)
        kc_cloth(b, cx - 3, base - 24, cx + 3, base - 19); palm(b, 1, base, 13); palm(b, W - 3, base, 13); return
    if era == "iron":   # หอดื่มเหล้าใหญ่ (mead hall)
        longhouse(b, 2, W - 3, base, 8, 18, STRAW)
        totem(b, 1, base, 12); flag(b, cx, base - 30, 7, True); return
    if era == "kingdom":   # ศาลากลางเมือง + หอนาฬิกา
        tudor(b, 3, W - 4, base, 2, 12, dormer=False)
        rect(b, cx - 4, base - 34, cx + 4, base - 12, STONEW, "stone")
        ell(b, cx - 3, base - 31, cx + 3, base - 25, "#f0e8d0", None); b.put(cx, base - 28, (30, 30, 30, 255)); b.put(cx, base - 29, (30, 30, 30, 255)); b.put(cx + 1, base - 28, (30, 30, 30, 255))
        roof_cone(b, cx, base - 34, 6, 9, "KC", "slate"); return
    if era == "magic":
        spire_house(b, 4, W - 5, base, 12, 14, 0)
        spire_house(b, cx - 5, cx + 5, base - 12, 12, 16, 3)
        for x in (2, W - 3): float_crystal(b, x, base - 18, RUNE, 6)
        float_crystal(b, cx + 3, base - 44, "#ff9aff", 5); return
    hover_pad(b, 1, W - 2, base); capsule(b, 3, W - 4, base - 1, 14); glass_dome(b, cx, base - 15, 12)
    antenna(b, 6, base - 26, 11); antenna(b, W - 7, base - 26, 11); solar(b, 3, 10, base - 15)


def castle(b, era):   # ปราสาทราชา (เมืองหลวง) 84x84
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":
        palisade(b, 2, W - 3, base, 12)
        for x in (10, W - 11): rect(b, x - 4, base - 24, x + 4, base - 10, LOG, "wood"); poly(b, [(x - 6, base - 24), (x + 6, base - 24), (x, base - 32)], THATCH, "thatch")
        hut(b, cx, base - 10, 16, 18); totem(b, cx - 22, base - 10, 18); totem(b, cx + 22, base - 10, 18)
        for i in range(3): firepit(b, cx - 8 + i * 8, base - 1)
        return
    if era == "bronze":
        ziggurat(b, cx, base, 5, 38, 7, 8, MUD)
        rect(b, cx - 6, base - 50, cx + 6, base - 40, SAND, "brick"); kc_cloth(b, cx - 4, base - 56, cx + 4, base - 50)
        for x in (4, W - 5): palm(b, x, base, 16)
        for x in range(cx - 30, cx + 31, 10): column(b, x, base - 7, base)
        return
    if era == "iron":   # ป้อมไม้บนเนิน + เรือนยาวของจาร์ล
        poly(b, [(0, base), (W - 1, base), (W - 10, base - 12), (10, base - 12)], "#6a7a3a", "thatch")
        palisade(b, 4, W - 5, base - 11, 10)
        longhouse(b, 12, W - 13, base - 14, 10, 24, STRAW)
        for x in (8, W - 9): rect(b, x - 3, base - 40, x + 3, base - 12, PLANK, "wood"); stave_roof(b, x, base - 40, 6, 6, 2)
        return
    if era == "kingdom":
        rect(b, 6, base - 22, W - 7, base, STONEW, "stone"); battlements(b, 6, W - 7, base - 22, STONEW)
        for x0 in (2, W - 15): keep_tower(b, x0, x0 + 12, base, 34)
        keep_tower(b, cx - 12, cx + 12, base - 22, 30, cone=False)
        for x0 in (cx - 12, cx + 6): keep_tower(b, x0, x0 + 6, base - 52, 8)
        roof_cone(b, cx, base - 52, 8, 14, "KC", "slate")
        door(b, cx, base - 1, 7, 10, "#4a2c18", "#5a5a64"); flag(b, cx, base - 72, 8, True); return
    if era == "magic":
        for x0, h, bend in ((6, 30, 3), (W - 20, 30, -3), (cx - 12, 44, 5)):
            spire_house(b, x0, x0 + (24 if x0 == cx - 12 else 14), base, h, 22, bend)
        for x, y in ((14, base - 58), (W - 14, base - 58), (cx + 8, base - 72)): float_crystal(b, x, y, RUNE, 7)
        ell(b, cx - 6, base - 16, cx + 6, base - 4, "#4a3a7a", None); return
    hover_pad(b, 2, W - 3, base)
    capsule(b, 6, W - 7, base - 1, 22); glass_dome(b, cx, base - 23, 22)
    for x in (12, W - 13): capsule(b, x - 5, x + 5, base - 23, 30, glass_band=True); antenna(b, x, base - 64, 10)
    solar(b, 8, 22, base - 23); solar(b, W - 23, W - 9, base - 23)


# =============================================================== วัด/ศาสนสถาน
def temple(b, era, grand=False):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":   # วงหินตั้ง
        for x0 in (3, W - 13):   # ซุ้มหินสามก้อน (trilithon)
            menhir(b, x0 + 2, base, 14, 2); menhir(b, x0 + 8, base, 14, 2)
            rect(b, x0, base - 17, x0 + 10, base - 14, "#b0aa9c", "stone")
        menhir(b, cx, base - 1, 20, 3)
        kc_cloth(b, cx - 2, base - 24, cx + 2, base - 21); firepit(b, cx - 6, base); firepit(b, cx + 6, base); return
    if era == "bronze":
        ziggurat(b, cx, base, 4 if not grand else 5, W // 2 - 2, 3, 7 if not grand else 8)
        rect(b, cx - 3, base - (30 if not grand else 42), cx + 3, base - (28 if not grand else 40), "#e0b040", "gold"); return
    if era == "iron":   # โบสถ์ไม้ซ้อนชั้น (stave church)
        rect(b, cx - 9, base - 10, cx + 9, base, PLANK, "wood")
        stave_roof(b, cx, base - 10, 12 if not grand else 16, 7, 3 if not grand else 4)
        rect(b, cx - 2, base - 6, cx + 2, base, "#3a2414", None, outline=False); return
    if era == "kingdom":
        rect(b, cx - 10, base - 18, cx + 10, base, STONEW, "stone"); roof_gable(b, cx - 10, cx + 10, base - 18, 9, "KC", "slate")
        keep_tower(b, cx + 4, cx + 11, base, 30 if not grand else 40)
        ell(b, cx - 6, base - 15, cx, base - 9, "#e0b040", None); b.put(cx - 3, base - 12, KC)   # หน้าต่างกุหลาบ
        door(b, cx - 3, base - 1, 4, 7, "#4a2c18", "#5a5a64")
        if grand: keep_tower(b, cx - 18, cx - 11, base, 40)
        return
    if era == "magic":
        rect(b, cx - 8, base - 12, cx + 8, base, MSTONE, "stone")
        twist_cone(b, cx, base - 12, 11, 22 if not grand else 32, MROOF, 2)
        ell(b, cx - 3, base - 46, cx + 3, base - 40, "#ff9aff", None)  # ลูกแก้วลอย
        for x in (cx - 12, cx + 12): float_crystal(b, x, base - 16, RUNE, 6)
        rune_window(b, cx - 1, base - 9); return
    hover_pad(b, 1, W - 2, base); capsule(b, cx - 12, cx + 12, base - 1, 10); dome(b, cx, base - 11, 11, "#e6b440", "gold")
    for y in range(4, base - 22): b.put(cx, y, (255, 250, 200, 140))   # ลำแสงสู่ฟ้า
    if grand:
        for x in (4, W - 5): capsule(b, x - 3, x + 3, base - 1, 30); antenna(b, x, base - 40, 8)


# =============================================================== ค่ายทหาร หอคอย ตลาด ยุ้ง บ่อน้ำ กิลด์
def barracks(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":
        hut(b, cx - 5, base, 8, 9)
        for x in range(W - 12, W - 2, 3):   # ชั้นวางหอก
            for y in range(base - 11, base + 1): b.put(x, y, (110, 80, 50, 255))
            b.put(x, base - 12, (190, 190, 200, 255))
        return
    if era == "bronze":
        mudbox(b, 3, W - 4, base - 10, base)
        for x in range(6, W - 6, 6): ell(b, x - 2, base - 8, x + 2, base - 3, "#c89a3a", None); b.put(x, base - 6, KC)
        return
    if era == "iron":
        longhouse(b, 2, W - 3, base, 6, 12, STRAW); return
    if era == "kingdom":
        rect(b, 3, base - 14, W - 4, base, STONEW, "stone"); battlements(b, 3, W - 4, base - 14, STONEW)
        banner_wall(b, cx - 1, base - 12, 6); door(b, cx + 6, base - 1, 4, 7, "#4a2c18", "#5a5a64")
        for x in (6, 11): window(b, x, base - 10, 2, 3, "#ffe08a", "#4a4a52", arch=True)
        return
    if era == "magic":
        spire_house(b, 3, W - 4, base, 11, 12, 1); float_crystal(b, 3, base - 14, "#ff6a6a", 5); return
    hover_pad(b, 1, W - 2, base); capsule(b, 2, W - 3, base - 1, 13, glass_band=False)
    rect(b, cx - 6, base - 10, cx + 6, base - 2, "#3a3a4a", None)   # ประตูโรงเก็บยาน
    for x in range(cx - 5, cx + 6, 2): b.put(x, base - 9, (255, 200, 60, 255))


def tower(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":
        for x in (cx - 5, cx + 5):
            for y in range(base - 26, base + 1): b.put(x, y, (110, 80, 50, 255))
        rect(b, cx - 7, base - 28, cx + 7, base - 25, LOG, "wood"); poly(b, [(cx - 8, base - 28), (cx + 8, base - 28), (cx, base - 36)], THATCH, "thatch")
        kc_cloth(b, cx + 7, base - 34, cx + 10, base - 31); return
    if era == "bronze":
        rect(b, cx - 5, base - 30, cx + 5, base, MUD, "brick"); rect(b, cx - 6, base - 32, cx + 6, base - 30, SAND, None)
        hole_window(b, cx - 1, base - 26); hole_window(b, cx - 1, base - 16); kc_cloth(b, cx - 2, base - 38, cx + 2, base - 33); return
    if era == "iron":
        rect(b, cx - 5, base - 28, cx + 5, base, PLANK, "wood"); stave_roof(b, cx, base - 28, 8, 6, 2); return
    if era == "kingdom":
        keep_tower(b, cx - 6, cx + 6, base, 30); return
    if era == "magic":
        rect(b, cx - 4, base - 26, cx + 4, base, MSTONE, "stone"); twist_cone(b, cx, base - 26, 7, 14, MROOF, 3)
        for y in (base - 20, base - 12): rune_window(b, cx - 1, y)
        float_crystal(b, cx + 5, base - 42, RUNE, 5); return
    capsule(b, cx - 4, cx + 4, base, 30); glass_dome(b, cx, base - 30, 5); antenna(b, cx, base - 42, 7)


def market(b, era):
    base = base_y(b); W = b.w
    stalls = [(2, 13), (16, 27)]
    for i, (x0, x1) in enumerate(stalls):
        if era == "stone":
            for x in (x0, x1):
                for y in range(base - 8, base + 1): b.put(x, y, (110, 80, 50, 255))
            rect(b, x0, base - 10, x1, base - 8, "#c8a070", "hide"); kc_dots(b, x0 + 1, x1 - 1, base - 9)
            ell(b, x0 + 2, base - 3, x0 + 6, base, "#8a5a3a", None)
        elif era == "bronze":
            rect(b, x0, base - 4, x1, base, MUD, "brick"); awning(b, x0, x1, base - 10, 3); pots(b, x0 + 2, base - 4)
        elif era == "iron":
            rect(b, x0, base - 4, x1, base, PLANK, "wood"); poly(b, [(x0 - 1, base - 6), (x1 + 1, base - 6), (x1 - 1, base - 11), (x0 + 1, base - 11)], STRAW, "thatch")
            for x in (x0, x1):
                for y in range(base - 6, base - 3): b.put(x, y, (90, 60, 36, 255))
        elif era == "kingdom":
            rect(b, x0, base - 4, x1, base, "#a87a44", "wood")
            for j, x in enumerate(range(x0 - 1, x1 + 2, 2)):
                for y in range(base - 12, base - 8): b.put(x, y, KC if j % 2 == 0 else (240, 236, 224, 255)); b.put(x + 1, y, KC if j % 2 == 0 else (240, 236, 224, 255))
            for x in (x0, x1):
                for y in range(base - 8, base - 3): b.put(x, y, (90, 60, 36, 255))
        elif era == "magic":
            rect(b, x0, base - 4, x1, base, MSTONE, "stone"); mushroom(b, (x0 + x1) // 2, base - 4, 7, 3); float_crystal(b, x1, base - 6, RUNE, 4)
        else:
            capsule(b, x0, x1, base, 7, glass_band=False)
            for x in range(x0 + 2, x1 - 1): b.put(x, base - 9, (*R(GLASS)[3][:3], 200))
        for k in range(3): b.put(x0 + 3 + k * 3, base - 5, b.rnd.choice(((230, 80, 60, 255), (240, 200, 60, 255), (100, 190, 80, 255))))


def granary(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":
        for x in (cx - 6, cx + 6):
            for y in range(base - 6, base + 1): b.put(x, y, (110, 80, 50, 255))
        hut(b, cx, base - 6, 9, 9, door_=False); return
    if era == "bronze":
        beehive(b, cx - 6, base, 6); beehive(b, cx + 6, base, 6); beehive(b, cx, base - 2, 7, "#d09a64"); return
    if era == "iron":
        for x in (cx - 8, cx - 1, cx + 7):
            for y in range(base - 5, base + 1): b.put(x, y, (90, 60, 36, 255))
        longhouse(b, cx - 10, cx + 10, base - 5, 6, 10, STRAW, shields=False, door_=False); return
    if era == "kingdom":
        rect(b, 3, base - 12, W - 4, base, "#8a4a3a", "wood"); roof_gable(b, 3, W - 4, base - 12, 10, "KC", "shingle")
        rect(b, cx - 4, base - 9, cx + 4, base, "#5a3a20", None)
        for d in range(-3, 4): b.put(cx + d, base - 9 + abs(d), (220, 200, 160, 255))
        return
    if era == "magic":
        ell(b, cx - 9, base - 18, cx + 9, base, MSTONE, "stone"); twist_cone(b, cx, base - 14, 8, 12, MROOF, 2); rune_window(b, cx - 1, base - 8); return
    for x in (cx - 7, cx + 7): capsule(b, x - 5, x + 5, base, 22); glass_dome(b, x, base - 22, 4)
    for x in range(cx - 2, cx + 3): b.put(x, base - 14, (150, 150, 170, 255))


def well(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    col = {"stone": "#8a8478", "bronze": MUD, "iron": PLANK, "kingdom": STONEW, "magic": MSTONE, "star": ALLOY}[era]
    ell(b, cx - 6, base - 5, cx + 6, base + 1, col, "stone" if era != "iron" else "wood")
    ell(b, cx - 4, base - 4, cx + 4, base - 1, "#3a6aa0" if era != "magic" else "#5a3aa0", None, outline=False)
    if era == "kingdom":
        for x in (cx - 5, cx + 5):
            for y in range(base - 12, base - 3): b.put(x, y, (90, 60, 36, 255))
        roof_gable(b, cx - 5, cx + 5, base - 12, 4, "KC", "tile")
    elif era == "magic":
        float_crystal(b, cx, base - 8, RUNE, 5)
    elif era == "star":
        antenna(b, cx, base - 12, 7)
    elif era in ("bronze", "iron"):
        for x in (cx - 5, cx + 5):
            for y in range(base - 10, base - 3): b.put(x, y, (110, 80, 50, 255))
        for x in range(cx - 5, cx + 6): b.put(x, base - 10, (110, 80, 50, 255))
        kc_cloth(b, cx - 1, base - 9, cx + 1, base - 7)


def guild(b, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":
        palisade(b, 0, W - 1, base, 5); hut(b, cx, base - 1, 11, 12)
        for x in (cx - 8, cx + 8): b.put(x, base - 16, (230, 220, 200, 255)); b.put(x, base - 17, (230, 220, 200, 255))   # เขาสัตว์
        totem(b, 3, base - 1, 12); return
    if era == "bronze":
        mudbox(b, 3, W - 4, base - 12, base); rect(b, cx - 8, base - 20, cx + 8, base - 12, MUD, "brick")
        for x in (6, W - 7, cx - 6, cx + 6): column(b, x, base - 11, base)
        kc_cloth(b, cx - 3, base - 19, cx + 3, base - 14); awning(b, cx - 6, cx + 6, base - 11); return
    if era == "iron":
        longhouse(b, 2, W - 3, base, 7, 16, STRAW); flag(b, 4, base - 26, 7, True); flag(b, W - 6, base - 26, 7, True); return
    if era == "kingdom":
        tudor(b, 3, W - 4, base, 3, 12, dormer=True)
        rect(b, cx - 6, base - 9, cx + 6, base - 7, "#8a5a30", "wood")   # ป้ายกิลด์
        for x in range(cx - 4, cx + 5, 2): b.put(x, base - 8, KC)
        flag(b, 2, base - 30, 7, True); return
    if era == "magic":
        spire_house(b, 4, W - 5, base, 14, 18, 3)
        for x in (3, W - 4): float_crystal(b, x, base - 20, RUNE, 6)
        kc_cloth(b, cx - 2, base - 12, cx + 2, base - 6); return
    hover_pad(b, 1, W - 2, base); capsule(b, 3, W - 4, base - 1, 16); glass_dome(b, cx, base - 17, 10); antenna(b, W - 6, base - 30, 10)
    for x in range(cx - 6, cx + 7): b.put(x, base - 8, KC)


def fence(b, x0, x1, y):
    for x in range(x0, x1 + 1):
        b.put(x, y - 2, (150, 110, 70, 255))
        if x % 2 == 0: b.put(x, y - 1, (130, 90, 50, 255)); b.put(x, y, (130, 90, 50, 255))


TYPES = {   # ชนิด: (กว้าง, สูง, ฟังก์ชัน)
    "house0": (24, 26, lambda b, e: house(b, e, 0)),
    "house1": (28, 30, lambda b, e: house(b, e, 1)),
    "house2": (26, 32, lambda b, e: house(b, e, 2)),
    "house3": (32, 26, lambda b, e: house(b, e, 3)),
    "shack": (20, 18, shack), "rowhouse": (40, 28, rowhouse), "mansion": (36, 34, mansion),
    "hall": (40, 40, hall), "castle": (84, 84, castle),
    "temple": (34, 44, temple), "cathedral": (50, 66, lambda b, e: temple(b, e, True)),
    "barracks": (36, 30, barracks), "tower": (20, 44, tower), "market": (30, 26, market),
    "granary": (28, 30, granary), "well": (16, 18, well), "guild": (40, 38, guild),
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
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(os.path.dirname(os.path.abspath(__file__)), "previews", "eras7.png")
    sheet(prev, out, scale=3, cols=len(TYPES), bg=(74, 112, 52, 255))
    print("ok", len(prev), out)
