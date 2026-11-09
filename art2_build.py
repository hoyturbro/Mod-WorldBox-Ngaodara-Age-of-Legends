# อาคารสไตล์ WorldBox: มุมมองหน้าเฉียงบน ขอบเข้มสีเดียวกัน แสงบนซ้าย · สีม่วงบานเย็น (#ff00ff) = สีอาณาจักร (เกมแทนสีให้)
# แต่ละอาคาร: main_0 · construction_0 · ruin_0 · mini_0 (7x5) → GameResources/buildings/ngd/<id>/
# ใช้: python art2_build.py [preview out.png]
import os, sys, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from PIL import Image
from art2_core import Canvas, ramp, hx, sheet

ROOT = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(ROOT, "NgaodaraPack", "GameResources", "buildings", "ngd")
KC = (255, 0, 255, 255)  # สีอาณาจักร


def R(c, s=1.0):
    return ramp(c, spread=s)


# ------------------------------------------------------------------ ชิ้นส่วนพื้นฐาน
def wall(cv, x0, y0, x1, y1, col, bricks=None, beams=None):
    m = cv.mask(); m.rect(x0, y0, x1, y1); cv.part(m, R(col))
    rp = R(col)
    if bricks:
        for y in range(y0 + 2, y1, 3):
            off = 0 if (y // 3) % 2 else 2
            for x in range(x0 + 1 + off, x1, 4):
                cv.dot(x, y, rp[1])
    if beams:
        b = R(beams)
        for x in (x0, x1):
            mm = cv.mask(); mm.rect(x, y0, x, y1); cv.part(mm, b, outline=False, flat=True)
        mm = cv.mask(); mm.rect(x0, y0, x1, y0); cv.part(mm, b, outline=False, flat=True)
        cx = (x0 + x1) // 2
        cv.line([(x0 + 1, y1 - 1), (cx, y0 + 1)], b[1][:3]); cv.line([(x1 - 1, y1 - 1), (cx, y0 + 1)], b[1][:3])


def gable(cv, x0, x1, ybase, h, col, over=2, tiles=True):
    m = cv.mask(); cx = (x0 + x1) / 2
    m.poly([(x0 - over, ybase), (cx, ybase - h), (x1 + over, ybase), (x1 + over, ybase + 2), (x0 - over, ybase + 2)])
    cv.part(m, R(col))
    if tiles:
        rp = R(col)
        for y in range(int(ybase - h + 3), int(ybase + 1), 3):
            half = (y - (ybase - h)) / h * ((x1 - x0) / 2 + over)
            cv.line([(cx - half + 1, y), (cx + half - 1, y)], rp[1][:3])


def roof3q(cv, x0, x1, top, h, col, over=2):
    """หลังคามุมเฉียงบน: ผืนหลังคาด้านหน้า (สี่เหลี่ยมคางหมู) + สันหลังคา + จั่วข้างสองฝั่ง"""
    rp = R(col)
    m = cv.mask(); m.poly([(x0 - over, top + 1), (x1 + over, top + 1), (x1 + 1, top - h), (x0 - 1, top - h)]); cv.part(m, rp)
    for y in range(top - h + 3, top, 2):
        for x in range(x0 - 1 + (y % 4 == 0), x1 + 2, 3):
            cv.dot(x, y, rp[1])
    cv.line([(x0, top - h), (x1, top - h)], rp[4][:3])
    for side in (-1, 1):
        ex = x0 - 1 if side < 0 else x1 + 1
        m = cv.mask(); m.poly([(ex, top - h), (ex + side * (over + 1), top + 1), (ex, top + 1)]); cv.part(m, R(col, 0.7), outline=True)


def flat_roof(cv, x0, x1, y, col, depth=4):
    m = cv.mask(); m.poly([(x0 - 1, y), (x0 + 2, y - depth), (x1 - 2, y - depth), (x1 + 1, y)]); m.rect(x0 - 1, y, x1 + 1, y + 1); cv.part(m, R(col))


def cone(cv, cx, ybase, w, h, col):
    m = cv.mask(); m.poly([(cx - w, ybase), (cx, ybase - h), (cx + w, ybase), (cx + w - 1, ybase + 2), (cx - w + 1, ybase + 2)]); cv.part(m, R(col))
    rp = R(col)
    for i in range(1, 4):
        y = ybase - h + i * h // 4
        hw = w * i / 4
        cv.line([(cx - hw + 1, y), (cx + hw - 1, y)], rp[1][:3])


def dome(cv, cx, ybase, r, col):
    m = cv.mask(); m.pie((cx - r, ybase - r, cx + r, ybase + r), 180, 360); m.rect(cx - r, ybase - 1, cx + r, ybase + 1); cv.part(m, R(col))
    cv.dot(cx - r // 2, ybase - r + 2, (255, 255, 255))


def door(cv, cx, ybot, w=3, h=5, col="#4a2c18", arch=True):
    m = cv.mask(); m.rect(cx - w // 2, ybot - h, cx + w // 2, ybot)
    if arch: m.ell(cx - w // 2, ybot - h - 1, cx + w // 2, ybot - h + 2)
    cv.part(m, R(col))


def window(cv, x, y, lit="#ffd86a", w=2, h=2):
    m = cv.mask(); m.rect(x, y, x + w - 1, y + h - 1); cv.part(m, R(lit, 0.6), outline=False, flat=True)
    cv.dot(x, y, R(lit)[4])


def banner(cv, x, y, h=6):
    cv.line([(x, y - 2), (x, y + h)], (70, 50, 40))
    m = cv.mask(); m.poly([(x + 1, y - 2), (x + 5, y - 1), (x + 4, y + 1), (x + 5, y + 3), (x + 1, y + 3)]); cv.part(m, [KC] * 5, outline=False)
    cv.dot(x + 1, y - 2, (60, 0, 60))


def chimney(cv, x, y, col="#7a5a4a", smoke=True):
    m = cv.mask(); m.rect(x, y, x + 2, y + 4); cv.part(m, R(col))
    if smoke:
        for i, (dx, dy) in enumerate(((1, -2), (2, -4), (1, -6))):
            cv.dot(x + dx, y + dy, (200, 200, 200, 160 - i * 40))


def emblem_sign(cv, cx, cy, kind):
    """ป้ายตราหน้าอาคาร"""
    m = cv.mask(); m.rect(cx - 3, cy - 3, cx + 3, cy + 3); cv.part(m, R("#6a4a2a"))
    if kind == "guild":   # ดาบไขว้
        cv.line([(cx - 2, cy + 2), (cx + 2, cy - 2)], (230, 230, 240)); cv.line([(cx + 2, cy + 2), (cx - 2, cy - 2)], (230, 230, 240))
        cv.dot(cx, cy, (255, 210, 80))
    elif kind == "coin":
        m = cv.mask(); m.ell(cx - 2, cy - 2, cx + 2, cy + 2); cv.part(m, R("#f2c040"), outline=False)
    elif kind == "book":
        m = cv.mask(); m.rect(cx - 2, cy - 2, cx + 2, cy + 2); cv.part(m, R("#4a70d0"), outline=False); cv.line([(cx, cy - 2), (cx, cy + 2)], (240, 230, 200))
    elif kind == "anvil":
        m = cv.mask(); m.poly([(cx - 2, cy - 1), (cx + 3, cy - 1), (cx + 1, cy + 1), (cx + 1, cy + 2), (cx - 1, cy + 2), (cx - 1, cy + 1)]); cv.part(m, R("#a0a8b8"), outline=False)


def ground(cv, x0, x1, y, col="#6a5a44"):
    m = cv.mask(); m.rect(x0, y, x1, y + 1); cv.part(m, R(col), outline=False)


# ------------------------------------------------------------------ บ้านตามยุค
ERA = {  # ผนัง, หลังคา, ขอบ/คาน, ประตู, หน้าต่าง
    "stone":   ("#8a6a4a", "#c8a050", "#5a3a20", "#3a2414", "#e8a040"),
    "bronze":  ("#c89a64", "#9a6a3a", "#b07a3a", "#4a2c18", "#ffcf6a"),
    "iron":    ("#d8cdb0", "#6a4a30", "#5a3a24", "#4a2c18", "#ffd86a"),
    "kingdom": ("#9aa0a8", "#b04a3a", "#6a6a74", "#4a2c18", "#ffe08a"),
    "magic":   ("#7a7090", "#3a4a9a", "#5a4a7a", "#2a1a3a", "#7af0ff"),
    "star":    ("#e8e6f0", "#e6b440", "#b8b0d0", "#3a3a6a", "#8fd0ff"),
}


def house(cv, era, big):
    wl, rf, tr, dr, wn = ERA[era]
    W, H = cv.w, cv.h
    base = H - 3
    if era == "stone":
        cx = W // 2
        if big:
            m = cv.mask(); m.ell(5, base - 9, 23, base + 1); cv.part(m, R(wl))
            cone(cv, cx, base - 6, 10, 13, rf)
        else:
            m = cv.mask(); m.ell(7, base - 7, 21, base + 1); cv.part(m, R(wl))
            cone(cv, cx, base - 4, 8, 11, rf)
        door(cv, cx, base, 3, 4, dr)
        cv.line([(cx - 1, base - 18 + (0 if big else 3)), (cx + 2, base - 21 + (0 if big else 3))], (90, 60, 40))
        banner(cv, cx + 8, base - 10, 5) if big else None
        return
    x0, x1 = (3, 24) if big else (5, 22)
    top = base - (9 if big else 6)
    wall(cv, x0, top, x1, base, wl, bricks=era in ("kingdom", "magic", "star", "bronze"), beams=tr if era == "iron" else None)
    if era == "bronze":
        flat_roof(cv, x0, x1, top, rf, 8 if big else 6)
        for x in range(x0 + 2, x1, 4): cv.dot(x, top + 1, R(tr)[1])
    elif era == "magic":
        roof3q(cv, x0, x1, top, 11 if big else 9, rf)
        m = cv.mask(); m.rect(x1 - 4, top - 12, x1 - 1, top + 2); cv.part(m, R(wl))
        cone(cv, x1 - 2.5, top - 12, 3, 7, rf)
        cv.dot(int(x1 - 2.5), top - 19, R(wn)[4])
    elif era == "star":
        dome(cv, (x0 + x1) // 2, top, (x1 - x0) // 2 - 1, rf)
        star_y = top - (x1 - x0) // 2
        cv.dot((x0 + x1) // 2, star_y - 1, (255, 255, 220)); cv.dot((x0 + x1) // 2, star_y - 2, (255, 240, 160))
    else:
        roof3q(cv, x0, x1, top, 12 if big else 10, rf)
    if era in ("kingdom", "iron"):
        chimney(cv, x1 - 5, top - (14 if big else 12))
    cx = (x0 + x1) // 2
    door(cv, cx, base, 3, 5 if big else 4, dr)
    for wx in (x0 + 3, x1 - 4):
        window(cv, wx, top + 3, wn)
    if big:
        window(cv, x0 + 7, top + 3, wn); window(cv, x1 - 8, top + 3, wn)
        banner(cv, x0 + 1, top - 16, 5)
    if era == "magic":
        for (x, y) in ((x0 - 1, base - 2), (x1 + 1, base - 3)):
            m = cv.mask(); m.poly([(x - 1, y + 2), (x, y - 3), (x + 1, y + 2)]); cv.part(m, R(wn, 0.7), outline=False)
    ground(cv, x0 - 1, x1 + 1, base + 1)


# ------------------------------------------------------------------ อาคารพิเศษ
def guild(cv, tier):
    W, H = cv.w, cv.h; base = H - 3
    wl = ("#8a6a4a", "#9aa0a8", "#d8d0c0")[tier]; rf = ("#7a4a2a", "#3a5aa0", "#c8302a")[tier]; tr = ("#5a3a20", "#6a6a80", "#e6b440")[tier]
    x0, x1 = 6, W - 7
    top = base - 15
    wall(cv, x0, top, x1, base, wl, bricks=tier > 0, beams=tr if tier == 0 else None)
    roof3q(cv, x0, x1, top, 11, rf, over=3)
    if tier >= 1:  # หอคอยข้าง
        for tx in (x0 - 4, x1 + 1):
            m = cv.mask(); m.rect(tx, top - 4, tx + 4, base); cv.part(m, R(wl))
            cone(cv, tx + 2, top - 4, 3 if tier == 1 else 4, 7 if tier == 1 else 10, rf)
            window(cv, tx + 1, top + 1, "#ffd86a", 2, 3)
    if tier == 2:  # ยอดทอง + ประตูใหญ่
        cv.dot(W // 2, top - 10, (255, 230, 120)); cv.dot(W // 2, top - 11, (255, 250, 200))
    cx = W // 2
    door(cv, cx, base, 5, 6, "#4a2c18")
    m = cv.mask(); m.rect(cx - 4, base - 9, cx + 4, base - 8); cv.part(m, R(tr), outline=False)
    emblem_sign(cv, cx, top + 5, "guild")
    for wx in (x0 + 2, x1 - 3):
        window(cv, wx, top + 4, "#ffd86a", 2, 3)
    banner(cv, x0 + 2, top - 10, 6); banner(cv, x1 - 5, top - 10, 6)
    ground(cv, x0 - 5, x1 + 5, base + 1)


def bank(cv):
    W, H = cv.w, cv.h; base = H - 3
    x0, x1 = 4, W - 5; top = base - 12
    wall(cv, x0, top, x1, base, "#d8d4c8", bricks=True)
    m = cv.mask(); m.poly([(x0 - 2, top), (W // 2, top - 7), (x1 + 2, top), (x1 + 2, top + 2), (x0 - 2, top + 2)]); cv.part(m, R("#e8e4d8"))
    emblem_sign(cv, W // 2, top - 2, "coin")
    for x in range(x0 + 2, x1, 4):
        m = cv.mask(); m.rect(x, top + 3, x + 1, base - 1); cv.part(m, R("#f2eee4"), outline=False)
    door(cv, W // 2, base, 3, 5, "#6a4a2a", False)
    m = cv.mask(); m.rect(x0 - 1, base - 1, x1 + 1, base + 1); cv.part(m, R("#b8b4a8"))
    banner(cv, x1 - 1, top - 8, 5)


def market(cv):
    W, H = cv.w, cv.h; base = H - 3
    for i, (x0, col) in enumerate(((3, "#d84a4a"), (15, "#4a8ad8"))):
        x1 = x0 + 10
        m = cv.mask(); m.rect(x0 + 1, base - 7, x1 - 1, base); cv.part(m, R("#a07a4a"))
        m = cv.mask(); m.poly([(x0 - 1, base - 7), (x0 + 1, base - 12), (x1 - 1, base - 12), (x1 + 1, base - 7)]); cv.part(m, R(col))
        for x in range(x0 + 1, x1, 3):
            cv.line([(x, base - 11), (x - 1, base - 8)], (245, 240, 230) if i == 0 else KC[:3])
        for x, c in ((x0 + 2, "#ffcc40"), (x0 + 5, "#5ad05a"), (x0 + 8, "#e05a5a")):
            m = cv.mask(); m.ell(x - 1, base - 6, x + 1, base - 4); cv.part(m, R(c), outline=False)
    ground(cv, 2, W - 2, base + 1)


def academy(cv):
    W, H = cv.w, cv.h; base = H - 3
    x0, x1 = 5, W - 6; top = base - 12
    wall(cv, x0, top, x1, base, "#c8c0b0", bricks=True)
    dome(cv, W // 2, top, 7, "#4a70c0")
    cv.line([(W // 2 + 2, top - 8), (W // 2 + 7, top - 12)], (200, 180, 120))  # กล้องดูดาว
    emblem_sign(cv, W // 2, top + 4, "book")
    door(cv, W // 2, base, 3, 4)
    for wx in (x0 + 2, x1 - 3): window(cv, wx, top + 3)
    ground(cv, x0 - 1, x1 + 1, base + 1)


def forge(cv):
    W, H = cv.w, cv.h; base = H - 3
    x0, x1 = 5, W - 8; top = base - 9
    wall(cv, x0, top, x1, base, "#8a7a6a", bricks=True)
    roof3q(cv, x0, x1, top, 8, "#5a3a2a")
    chimney(cv, x1 - 3, top - 11, "#6a5a50")
    m = cv.mask(); m.rect(x1 + 1, base - 4, x1 + 6, base); cv.part(m, R("#7a5a3a"))
    m = cv.mask(); m.ell(x0 + 3, base - 6, x0 + 9, base - 1); cv.part(m, R("#ff8a3a", 0.7), outline=False)  # เตาไฟ
    emblem_sign(cv, (x0 + x1) // 2 + 3, top + 3, "anvil")
    ground(cv, x0 - 1, x1 + 7, base + 1)


def arena(cv):
    W, H = cv.w, cv.h; base = H - 3
    m = cv.mask(); m.ell(2, base - 14, W - 3, base + 1); cv.part(m, R("#c8b48a"))
    m = cv.mask(); m.ell(7, base - 11, W - 8, base - 3); cv.part(m, R("#d8b878"), outline=False, flat=True)
    for i in range(0, 360, 30):
        a = math.radians(i)
        cv.dot(int(W / 2 + math.cos(a) * (W / 2 - 3)), int(base - 6.5 + math.sin(a) * 7), R("#8a7a5a")[1])
    for x in (4, W - 6):
        banner(cv, x, base - 18, 5)
    cv.dot(W // 2 - 2, base - 7, (200, 60, 60)); cv.dot(W // 2 + 2, base - 7, (60, 60, 200))


def mage_tower(cv):
    W, H = cv.w, cv.h; base = H - 3; cx = W // 2
    m = cv.mask(); m.rect(cx - 5, base - 20, cx + 5, base); m.rect(cx - 7, base - 4, cx + 7, base); cv.part(m, R("#6a6080"))
    for y in range(base - 18, base - 4, 3): cv.dot(cx - 3, y, R("#6a6080")[1]); cv.dot(cx + 2, y + 1, R("#6a6080")[1])
    cone(cv, cx, base - 20, 7, 10, "#3a3a9a")
    m = cv.mask(); m.poly([(cx, base - 36), (cx + 2, base - 32), (cx, base - 29), (cx - 2, base - 32)]); cv.part(m, R("#7af0ff", 0.7))
    cv.glow("#7af0ff", 30, 1)
    window(cv, cx - 1, base - 15, "#7af0ff", 2, 3); window(cv, cx - 1, base - 9, "#7af0ff", 2, 2)
    door(cv, cx, base, 3, 4, "#2a1a3a")
    banner(cv, cx + 6, base - 12, 4)


def statue(cv):
    W, H = cv.w, cv.h; base = H - 3; cx = W // 2
    m = cv.mask(); m.rect(cx - 6, base - 5, cx + 6, base); cv.part(m, R("#a8a8b0"))
    m = cv.mask(); m.rect(cx - 2, base - 15, cx + 2, base - 5); m.ell(cx - 2, base - 19, cx + 2, base - 15); cv.part(m, R("#e6b440"))
    cv.line([(cx + 3, base - 9), (cx + 6, base - 20)], (240, 240, 250))   # ดาบชูฟ้า
    cv.dot(cx + 6, base - 21, (255, 255, 255))
    m = cv.mask(); m.poly([(cx - 3, base - 14), (cx - 7, base - 7), (cx - 3, base - 8)]); cv.part(m, [KC] * 5, outline=False)  # ผ้าคลุมสีอาณาจักร


def rift_gate(cv):
    W, H = cv.w, cv.h; base = H - 3; cx = W // 2
    for x in (cx - 9, cx + 6):
        m = cv.mask(); m.poly([(x, base), (x + 1, base - 16), (x + 3, base - 18), (x + 3, base)]); cv.part(m, R("#4a4458"))
    m = cv.mask(); m.ell(cx - 6, base - 18, cx + 6, base); cv.part(m, R("#5a2a9a"))
    m = cv.mask(); m.ell(cx - 4, base - 15, cx + 4, base - 3); cv.part(m, R("#c07aff", 0.7), outline=False)
    m = cv.mask(); m.ell(cx - 1, base - 11, cx + 1, base - 7); cv.part(m, R("#ffffff"), outline=False, flat=True)
    cv.glow("#a060ff", 50, 1)


def cult_shrine(cv):
    W, H = cv.w, cv.h; base = H - 3; cx = W // 2
    m = cv.mask(); m.poly([(cx - 3, base), (cx - 2, base - 18), (cx, base - 21), (cx + 2, base - 18), (cx + 3, base)]); cv.part(m, R("#2a2430"))
    m = cv.mask(); m.rect(cx - 7, base - 2, cx + 7, base); cv.part(m, R("#3a3440"))
    cv.dot(cx, base - 12, (190, 60, 255)); cv.dot(cx, base - 13, (230, 150, 255))
    for x in (cx - 6, cx + 6): cv.dot(x, base - 4, (255, 140, 60)); cv.dot(x, base - 5, (255, 220, 120))


BUILDINGS = {}
for era in ERA:
    BUILDINGS["house_%s_0" % era] = (28, 29, lambda cv, e=era: house(cv, e, False))
    BUILDINGS["house_%s_1" % era] = (28, 29, lambda cv, e=era: house(cv, e, True))
for t in range(3):
    BUILDINGS["guild_hall_%d" % t] = (36, 34, lambda cv, t=t: guild(cv, t))
BUILDINGS.update({
    "bank": (30, 29, bank), "market": (28, 26, market), "academy": (30, 31, academy), "forge": (30, 26, forge),
    "arena": (34, 26, arena), "mage_tower": (24, 40, mage_tower), "hero_statue": (20, 26, statue),
    "rift_gate": (28, 26, rift_gate), "cult_shrine": (20, 26, cult_shrine),
})


# ------------------------------------------------------------------ เฟรมอื่นจากภาพหลัก
def construction(img):
    """ภาพกำลังสร้าง: ครึ่งล่างของอาคาร + นั่งร้านไม้"""
    w, h = img.size
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    a = img.split()[3]
    bbox = a.getbbox() or (0, 0, w, h)
    cut = bbox[1] + (bbox[3] - bbox[1]) * 55 // 100
    low = img.crop((0, cut, w, h)); out.paste(low, (0, cut))
    px = out.load()
    for y in range(cut, h):
        for x in range(w):
            r, g, b, al = px[x, y]
            if al: px[x, y] = (int(r * 0.8 + 30), int(g * 0.8 + 20), int(b * 0.7 + 10), al)
    cv = Canvas(w, h); cv.img = out
    wood = (150, 105, 60)
    for x in (bbox[0] + 1, (bbox[0] + bbox[2]) // 2, bbox[2] - 2):
        cv.line([(x, bbox[3] - 1), (x, bbox[1] + 2)], wood)
    for y in (cut - 3, bbox[1] + 2):
        cv.line([(bbox[0], y), (bbox[2] - 1, y)], wood)
    return cv.img


def ruin(img, seed=7):
    """ซากปรักหักพัง: ส่วนบนพังเป็นฟันเลื่อย สีหม่น เศษหิน"""
    rnd = random.Random(seed)
    w, h = img.size
    out = img.copy(); px = out.load()
    a = img.split()[3]; bbox = a.getbbox() or (0, 0, w, h)
    for x in range(w):
        cut = bbox[1] + (bbox[3] - bbox[1]) * (45 + rnd.randint(0, 25)) // 100
        for y in range(0, cut):
            px[x, y] = (0, 0, 0, 0)
    for y in range(h):
        for x in range(w):
            r, g, b, al = px[x, y]
            if al:
                if (r, g, b) == (255, 0, 255): r, g, b = 90, 70, 90
                gray = (r + g + b) // 3
                px[x, y] = ((r + gray * 2) // 4, (g + gray * 2) // 4, (b + gray * 2) // 4, al)
    cv = Canvas(w, h); cv.img = out
    for _ in range(6):
        x = rnd.randint(bbox[0], bbox[2] - 1); y = bbox[3] - rnd.randint(0, 2)
        cv.dot(x, y, (110, 100, 95))
    return cv.img


def mini(img):
    a = img.split()[3]; bbox = a.getbbox()
    if not bbox: return Image.new("RGBA", (7, 5))
    c = img.crop(bbox).resize((7, 5), Image.BOX)
    px = c.load()
    for y in range(5):
        for x in range(7):
            r, g, b, al = px[x, y]
            px[x, y] = (r, g, b, 255 if al > 60 else 0)
    return c


def build(write=True):
    prev = []
    for bid, (w, h, fn) in BUILDINGS.items():
        cv = Canvas(w, h); fn(cv)
        main = cv.img
        frames = {"main_0": main, "construction_0": construction(main), "ruin_0": ruin(main, hash(bid) & 0xffff), "mini_0": mini(main)}
        prev += [main, frames["construction_0"], frames["ruin_0"]]
        if write:
            d = os.path.join(OUT, bid); os.makedirs(d, exist_ok=True)
            for k, im in frames.items(): im.save(os.path.join(d, k + ".png"))
    return prev


if __name__ == "__main__":
    imgs = build(write="preview" not in sys.argv)
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(os.environ.get("TEMP", "."), "build_preview.png")
    # พื้นหญ้าแบบในเกม
    sheet(imgs, out, scale=3, cols=9, bg=(74, 112, 52, 255))
    print("ok", len(imgs) // 3, out)
