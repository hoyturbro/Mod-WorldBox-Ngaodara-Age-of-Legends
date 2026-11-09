# ไอคอนปุ่มของมอด วาดใหม่สไตล์เกม: วัตถุลอยตัว ไม่มีวงกลม/กรอบ แรเงาหลายโทน
# ใช้: python art2_ui.py [preview out.png]
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from art2_core import Canvas, ramp, hx, diag, sheet
from art2_gear import emblem, weapon_icon, THEMES, R, quad

ROOT = os.path.dirname(os.path.abspath(__file__))
NGD = os.path.join(ROOT, "NgaodaraPack", "GameResources", "ui", "Icons", "ngd")
ICONS = {}


def icon(name, size=32):
    def deco(fn):
        ICONS[name] = (fn, size)
        return fn
    return deco


def star(cv, cx, cy, r, col, inner=0.45, pts=5, rot=-90):
    m = cv.mask()
    p = []
    for i in range(pts * 2):
        rr = r if i % 2 == 0 else r * inner
        a = math.radians(rot + i * 180 / pts)
        p.append((cx + math.cos(a) * rr, cy + math.sin(a) * rr))
    m.poly(p); cv.part(m, R(col))


def heart(cv, cx, cy, s, col):
    m = cv.mask(); m.ell(cx - s, cy - s * 0.8, cx, cy + s * 0.3); m.ell(cx, cy - s * 0.8, cx + s, cy + s * 0.3)
    m.poly([(cx - s, cy - 0.1 * s), (cx + s, cy - 0.1 * s), (cx, cy + s * 1.05)]); cv.part(m, R(col))


def plus(cv, cx, cy, s, col):
    m = cv.mask(); m.rect(cx - s, cy - s / 3, cx + s, cy + s / 3); m.rect(cx - s / 3, cy - s, cx + s / 3, cy + s); cv.part(m, R(col))


def arrow_up(cv, cx, cy, s, col):
    m = cv.mask(); m.poly([(cx, cy - s), (cx + s, cy), (cx + s * 0.4, cy), (cx + s * 0.4, cy + s), (cx - s * 0.4, cy + s), (cx - s * 0.4, cy), (cx - s, cy)]); cv.part(m, R(col))


def arrow_side(cv, cx, cy, s, col, d=1):
    m = cv.mask(); m.poly([(cx + d * s, cy), (cx, cy - s), (cx, cy - s * 0.4), (cx - d * s, cy - s * 0.4), (cx - d * s, cy + s * 0.4), (cx, cy + s * 0.4), (cx, cy + s)]); cv.part(m, R(col))


def hourglass(cv, cx, cy, s, sand):
    m = cv.mask(); m.rect(cx - s, cy - s * 1.25, cx + s, cy - s); m.rect(cx - s, cy + s, cx + s, cy + s * 1.25); cv.part(m, R("#8a5a30"))
    m = cv.mask(); m.poly([(cx - s * 0.8, cy - s), (cx + s * 0.8, cy - s), (cx + 1, cy), (cx + s * 0.8, cy + s), (cx - s * 0.8, cy + s), (cx - 1, cy)]); cv.part(m, R("#cfe8f0"))
    m = cv.mask(); m.poly([(cx - s * 0.5, cy - s * 0.45), (cx + s * 0.5, cy - s * 0.45), (cx, cy)]); m.poly([(cx - s * 0.75, cy + s * 0.95), (cx + s * 0.75, cy + s * 0.95), (cx, cy + s * 0.35)]); cv.part(m, R(sand), outline=False)


def book(cv, cx, cy, s, cover, open_=False):
    if open_:
        m = cv.mask(); m.poly([(cx - s * 1.4, cy - s * 0.6), (cx, cy - s * 0.3), (cx + s * 1.4, cy - s * 0.6), (cx + s * 1.4, cy + s * 0.8), (cx, cy + s * 1.1), (cx - s * 1.4, cy + s * 0.8)]); cv.part(m, R(cover))
        m = cv.mask(); m.poly([(cx - s * 1.2, cy - s * 0.7), (cx - 0.5, cy - s * 0.4), (cx - 0.5, cy + s * 0.8), (cx - s * 1.2, cy + s * 0.5)]); m.poly([(cx + s * 1.2, cy - s * 0.7), (cx + 0.5, cy - s * 0.4), (cx + 0.5, cy + s * 0.8), (cx + s * 1.2, cy + s * 0.5)]); cv.part(m, R("#f2e8cc"))
        for i in range(3):
            y = cy - s * 0.3 + i * s * 0.35
            cv.line([(cx - s * 1.0, y), (cx - s * 0.25, y + 1)], (150, 130, 100)); cv.line([(cx + s * 0.25, y + 1), (cx + s * 1.0, y)], (150, 130, 100))
    else:
        m = cv.mask(); m.rect(cx - s, cy - s * 1.2, cx + s, cy + s * 1.2); cv.part(m, R(cover))
        m = cv.mask(); m.rect(cx - s + 2, cy + s * 0.9, cx + s, cy + s * 1.2); cv.part(m, R("#f2e8cc"), outline=False)


# ======================================================= แท็บข้าง
@icon("tab_stats", 28)
def _(cv):
    m = cv.mask(); m.poly([(5, 3), (21, 3), (23, 6), (23, 25), (7, 25), (5, 22)]); cv.part(m, R("#efe0b8"))
    for i, c in enumerate(("#e04848", "#48c060", "#4890e0")):
        y = 8 + i * 5
        m = cv.mask(); m.rect(8, y, 8 + 6 + i * 3, y + 2); cv.part(m, R(c), outline=False)
    arrow_up(cv, 20, 19, 4, "#ffd040")
    cv.line([(24, 2), (16, 12)], (90, 60, 40)); cv.dot(24, 2, (255, 255, 255))


@icon("tab_armory", 28)
def _(cv):
    m = cv.mask(); m.poly([(6, 6), (14, 3), (22, 6), (21, 16), (14, 25), (7, 16)]); cv.part(m, R("#4a70c0"))
    m = cv.mask(); m.poly([(9, 8), (14, 6), (19, 8), (18, 15), (14, 21), (10, 15)]); cv.part(m, R("#e6b440"), outline=False)
    T = ("#c8d0dc", "#c8a040", "#ff6060", "holy_sword", set(), "sun")
    m = cv.mask(); quad(m, 14, 14, -12, 12, 1.6, 1.2); m.poly([diag(14, 14, 12, -1.2), diag(14, 14, 15, 0), diag(14, 14, 12, 1.2)]); cv.part(m, R("#dfe6ee"))
    m = cv.mask(); quad(m, 14, 14, -9, -7.5, 4); cv.part(m, R("#8a5a30"))


@icon("tab_divine", 28)
def _(cv):
    for a in range(0, 360, 30):
        r = 13 if a % 60 == 0 else 10
        cv.line([(14, 16), (14 + math.cos(math.radians(a)) * r, 16 + math.sin(math.radians(a)) * r)], (255, 226, 120))
    star(cv, 14, 16, 8, "#fff4b0", 0.5, 4, -90)
    m = cv.mask(); m.ell(7, 1, 21, 6); m.cut_ell(9, 2.5, 19, 4.5); cv.part(m, R("#ffd040"))
    cv.glow("#fff0a0", 60, 1)



@icon("tab_city_stock", 28)
def _(cv):  # หีบสมบัติเปิด มีทองล้น
    m = cv.mask(); m.rect(4, 13, 24, 25); cv.part(m, R("#8a5426"))
    m = cv.mask(); m.rect(4, 17, 24, 19); m.rect(12, 13, 16, 25); cv.part(m, R("#e6b440"), outline=False)
    m = cv.mask(); m.poly([(4, 13), (6, 5), (22, 5), (24, 13)]); cv.part(m, R("#6a3a1a"))
    for (x, y) in ((9, 11), (14, 9), (19, 11), (11, 13), (17, 13)):
        m = cv.mask(); m.ell(x - 2.5, y - 2, x + 2.5, y + 2); cv.part(m, R("#ffd040"))
    cv.sparkle(22, 3)


@icon("skilltree", 32)
def _(cv):  # ต้นไม้ทักษะ: โหนดเรืองแสงต่อกันด้วยเส้น (เหมือนหน้าต่าง Skill Tree)
    gold = (242, 192, 64)
    for (x0, y0), (x1, y1) in (((16, 6), (8, 15)), ((16, 6), (24, 15)), ((8, 15), (8, 24)), ((24, 15), (24, 24)), ((8, 24), (16, 28)), ((24, 24), (16, 28))):
        cv.line([(x0, y0), (x1, y1)], gold)
    for (x, y, c) in ((16, 6, "#ffd24a"), (8, 15, "#ff8a3a"), (24, 15, "#7ab0ff"), (8, 24, "#9aff7a"), (24, 24, "#c07aff")):
        m = cv.mask(); m.rect(x - 3, y - 3, x + 3, y + 3); cv.part(m, R("#2a2438"))
        m = cv.mask(); m.rect(x - 2, y - 2, x + 2, y + 2); cv.part(m, R(c), outline=False)
    m = cv.mask(); m.ell(11, 23, 21, 31); cv.part(m, R("#f2c040"))
    cv.dot(16, 27, (255, 255, 230, 255))
    cv.glow("#ffd24a", 50, 1)


@icon("worldgen", 32)
def _(cv):  # ลูกโลก: ทะเล + ทวีป + ประกายสร้างโลก
    m = cv.mask(); m.ell(3, 3, 29, 29); cv.part(m, R("#2a6ad0"))
    m = cv.mask(); m.poly([(8, 9), (14, 6), (17, 10), (13, 15), (9, 14)]); m.poly([(17, 17), (24, 14), (26, 20), (21, 26), (16, 23)]); m.ell(6, 18, 11, 23)
    cv.part(m, R("#5ac85a"), outline=False)
    m = cv.mask(); m.ell(11, 9, 14, 12); cv.part(m, R("#e8e0c8"), outline=False)
    cv.glow("#8ad8ff", 50, 1)
    cv.sparkle(26, 4); cv.sparkle(4, 26)


@icon("tab_city_forge", 28)
def _(cv):  # ทั่ง + ค้อน + ประกายไฟ
    m = cv.mask(); m.poly([(3, 12), (25, 12), (22, 17), (6, 17)]); m.rect(10, 17, 18, 22); m.rect(6, 22, 22, 26); cv.part(m, R("#6a6a7a"))
    m = cv.mask(); m.rect(3, 12, 25, 13); cv.part(m, R("#b8b8c8"), outline=False)
    m = cv.mask(); quad(m, 15, 7, -9, 3, 1.2, ang=-35); cv.part(m, R("#7a4a2a"))
    m = cv.mask(); quad(m, 17, 5, -1, 4, 3.2, ang=55); cv.part(m, R("#a8b0bc"))
    for x, y, c in ((8, 8, "#ffd24a"), (6, 4, "#ff8a3a"), (11, 3, "#fff27a")):
        cv.dot(x, y, R(c)[3])
    cv.glow("#ff8a3a", 40, 1)


@icon("tab_city_gov", 28)
def _(cv):  # ป้อมปราการ + ธง
    m = cv.mask(); m.rect(5, 12, 23, 26); m.rect(3, 8, 9, 26); m.rect(19, 8, 25, 26)
    for x in (3, 7, 19, 23): m.rect(x, 5, x + 2, 8)
    cv.part(m, R("#a8a8b8"))
    m = cv.mask(); m.ell(11, 17, 17, 23); m.rect(11, 20, 17, 26); cv.part(m, R("#3a2a20"), outline=False)
    cv.line([(14, 1), (14, 12)], (90, 60, 40))
    m = cv.mask(); m.poly([(15, 1), (22, 3), (15, 6)]); cv.part(m, R("#e04848"))


@icon("tab_skills", 28)
def _(cv):  # ดาบไขว้ + ประกายพลัง
    for ang in (-45, -135):
        m = cv.mask(); quad(m, 14, 15, -10, 9, 1.4, 1.0, ang=ang); m.poly([diag(14, 15, 9, -1.0, ang), diag(14, 15, 12, 0, ang), diag(14, 15, 9, 1.0, ang)]); cv.part(m, R("#dfe6ee"))
        m = cv.mask(); quad(m, 14, 15, -7.5, -6, 3.2, ang=ang); cv.part(m, R("#e6b440"))
        m = cv.mask(); quad(m, 14, 15, -11, -8, 1.0, ang=ang); cv.part(m, R("#7a4a2a"))
    star(cv, 14, 9, 5, "#7af0ff", 0.4, 4, -90)
    cv.glow("#7af0ff", 50, 1)

# ======================================================= ปุ่มแท็บพลังเงาดารา
@icon("eye")
def _(cv):
    m = cv.mask(); m.ell(2, 9, 30, 25); m.cut_poly([(0, 0), (32, 0), (32, 9), (0, 9)]) ; cv.part(m, R("#f4ecd8"))
    m = cv.mask(); m.pie((2, 6, 30, 28), 180, 360); m.pie((2, 6, 30, 28), 0, 180); m.cut_ell(3, 9, 29, 25); cv.part(m, R("#e6b440"))
    m = cv.mask(); m.ell(10, 10, 22, 24); cv.part(m, R("#3a78d8"))
    star(cv, 16, 17, 4.5, "#fff4b0")
    cv.dot(13, 13, (255, 255, 255))
    for a in range(-160, -10, 30):
        cv.line([(16 + math.cos(math.radians(a)) * 14, 16 + math.sin(math.radians(a)) * 12), (16 + math.cos(math.radians(a)) * 16, 16 + math.sin(math.radians(a)) * 15)], (255, 220, 110))


@icon("star")
def _(cv):
    star(cv, 15, 17, 13, "#ffd040", 0.45)
    plus(cv, 25, 6, 4, "#7aff7a")
    cv.sparkle(8, 6)


@icon("p_relic")
def _(cv):
    m = cv.mask(); quad(m, 16, 16, -4, 15, 3.4, 2.6); m.poly([diag(16, 16, 15, -2.6), diag(16, 16, 19, 0), diag(16, 16, 15, 2.6)]); cv.part(m, R("#b8a8ff"))
    cv.line([diag(16, 16, -3, 0), diag(16, 16, 17, 0)], (240, 236, 255))
    m = cv.mask(); m.poly([diag(16, 16, -6, -2), diag(16, 16, -1, -9), diag(16, 16, -3, -7)]); m.poly([diag(16, 16, -6, 2), diag(16, 16, -1, 9), diag(16, 16, -3, 7)]); quad(m, 16, 16, -6, -4, 4); cv.part(m, R("#f2c040"))
    m = cv.mask(); quad(m, 16, 16, -13, -6, 1.4); cv.part(m, R("#5a2c7a"))
    px, py = diag(16, 16, -14, 0); m = cv.mask(); m.ell(px - 2.4, py - 2.4, px + 2.4, py + 2.4); cv.part(m, R("#ff5aa0"))
    cv.glow("#c0a0ff", 70, 1); cv.sparkle(25, 5)


@icon("p_ascension")
def _(cv):
    for i, (y, c) in enumerate(((22, "#5a9aff"), (14, "#8ab8ff"), (6, "#e0f0ff"))):
        m = cv.mask(); m.poly([(16, y - 6), (27, y + 3), (22, y + 4), (16, y - 1), (10, y + 4), (5, y + 3)]); cv.part(m, R(c))
    cv.glow("#a0c8ff", 60, 1); cv.sparkle(26, 4); cv.sparkle(6, 10)


@icon("p_timestop")
def _(cv):
    m = cv.mask(); m.ell(3, 3, 29, 29); cv.part(m, R("#c8a050"))
    m = cv.mask(); m.ell(6, 6, 26, 26); cv.part(m, R("#eaf4ff"), outline=False)
    for a in range(0, 360, 30):
        cv.dot(int(16 + math.cos(math.radians(a)) * 8), int(16 + math.sin(math.radians(a)) * 8), (90, 90, 130))
    cv.line([(16, 16), (16, 9)], (40, 40, 70), 2); cv.line([(16, 16), (21, 18)], (40, 40, 70), 1)
    m = cv.mask(); m.ell(14, 14, 18, 18); cv.part(m, R("#3a60c0"))
    for x in (9, 23):
        m = cv.mask(); m.rect(x - 2, 0, x + 2, 4); cv.part(m, R("#c8a050"))
    cv.line([(4, 28), (28, 4)], (120, 220, 255))


@icon("p_starfall")
def _(cv):
    for i in range(10):
        x0, y0 = 4 + i, 4 + i
        cv.line([(x0, y0 + 3), (x0 + 3, y0)], (255, 220 - i * 10, 120 + i * 8))
    m = cv.mask(); m.ell(13, 13, 28, 28); cv.part(m, R("#ff8a3a"))
    m = cv.mask(); m.ell(16, 16, 25, 25); cv.part(m, R("#ffe27a"), outline=False)
    star(cv, 21, 21, 4, "#ffffff", 0.5)
    cv.glow("#ffb050", 70, 1)


@icon("p_rift")
def _(cv):
    m = cv.mask(); m.ell(5, 2, 27, 30); cv.part(m, R("#5a2a9a"))
    m = cv.mask(); m.ell(9, 6, 23, 26); cv.part(m, R("#2a0a4a"), outline=False)
    m = cv.mask(); m.ell(12, 10, 20, 22); cv.part(m, R("#b070ff"), outline=False)
    for a in range(0, 360, 60):
        x, y = 16 + math.cos(math.radians(a)) * 11, 16 + math.sin(math.radians(a)) * 14
        cv.line([(16 + math.cos(math.radians(a)) * 7, 16 + math.sin(math.radians(a)) * 9), (x, y)], (220, 170, 255))
    m = cv.mask(); m.ell(14, 14, 18, 18); cv.part(m, R("#ffffff"), outline=False)
    cv.glow("#a060ff", 70, 1)


@icon("era_forward")
def _(cv):
    hourglass(cv, 12, 16, 8, "#e6c060"); arrow_side(cv, 24, 22, 6, "#5ad05a", 1)


@icon("era_back")
def _(cv):
    hourglass(cv, 20, 16, 8, "#e6c060"); arrow_side(cv, 8, 22, 6, "#e05050", -1)


@icon("chronicle")
def _(cv):
    book(cv, 16, 17, 9, "#7a3a24", open_=True)
    star(cv, 16, 7, 4, "#ffd040"); cv.sparkle(27, 6)


@icon("menu")
def _(cv):
    m = cv.mask(); m.poly([(6, 4), (22, 4), (25, 7), (25, 28), (6, 28)]); cv.part(m, R("#efe0b8"))
    for i in range(4):
        cv.line([(9, 9 + i * 4), (20, 9 + i * 4)], (140, 120, 90))
    m = cv.mask(); quad(m, 22, 18, -6, 8, 1.6, 1.2, ang=-60); cv.part(m, R("#ffd040"))
    m = cv.mask(); quad(m, 22, 18, -9, -6, 1.6, ang=-60); cv.part(m, R("#e05050"))


@icon("break")
def _(cv):
    m = cv.mask(); m.rect(3, 14, 13, 18); m.rect(19, 14, 29, 18); cv.part(m, R("#8a8aa0"))
    for (x, y) in ((12, 11), (19, 10), (10, 20), (21, 21)):
        m = cv.mask(); m.poly([(x, y), (x + 2, y - 2), (x + 3, y + 1)]); cv.part(m, R("#a0a0b8"))
    arrow_up(cv, 16, 13, 10, "#ffd040")
    cv.glow("#ffe080", 60, 1)


# ======================================================= ปุ่มพลังเทพ/สถานะ (28px)
@icon("heal", 28)
def _(cv):
    heart(cv, 14, 13, 11, "#e83a4a"); plus(cv, 14, 12, 4, "#ffffff")


@icon("immortal", 28)
def _(cv):  # อังก์ทอง (สัญลักษณ์ชีวิตนิรันดร์)
    m = cv.mask(); m.ell(8, 1, 20, 14); m.cut_ell(11, 4, 17, 12); m.rect(5, 12, 23, 15); m.poly([(12, 14), (16, 14), (17, 27), (11, 27)]); cv.part(m, R("#f2c040"))
    cv.glow("#fff0a0", 70, 1); cv.sparkle(23, 4)


@icon("young", 28)
def _(cv):
    m = cv.mask(); m.rect(13, 12, 15, 26); cv.part(m, R("#5aa040"), outline=False)
    for side in (-1, 1):
        m = cv.mask(); m.ell(14 + side * 1 - 6 + (side + 1) * 3, 6, 14 + side * 1 + (side + 1) * 3, 14); cv.part(m, R("#7ad85a"))
    m = cv.mask(); m.ell(5, 23, 23, 28); cv.part(m, R("#8a5a30"))
    cv.sparkle(22, 4)


@icon("older", 28)
def _(cv):
    hourglass(cv, 14, 14, 9, "#a8a8a8")


@icon("exp", 28)
def _(cv):
    m = cv.mask(); m.poly([(14, 1), (24, 10), (14, 27), (4, 10)]); cv.part(m, R("#4aa0ff"))
    m = cv.mask(); m.poly([(14, 4), (19, 10), (14, 10)]); cv.part(m, R("#e0f4ff"), outline=False)
    plus(cv, 22, 22, 4, "#7aff7a")


@icon("pts", 28)
def _(cv):
    for i, y in enumerate((20, 15, 10)):
        m = cv.mask(); m.ell(5, y, 21, y + 7); cv.part(m, R("#f2c040"))
    plus(cv, 22, 6, 4, "#7aff7a")


@icon("dice", 28)
def _(cv):
    m = cv.mask(); m.poly([(14, 2), (25, 8), (25, 20), (14, 26), (3, 20), (3, 8)]); cv.part(m, R("#f4f0e8"))
    cv.line([(3, 8), (14, 14), (25, 8)], (180, 170, 160)); cv.line([(14, 14), (14, 26)], (180, 170, 160))
    for (x, y) in ((14, 7), (8, 15), (10, 20), (18, 17), (21, 21)):
        m = cv.mask(); m.ell(x - 1.2, y - 1.2, x + 1.2, y + 1.2); cv.part(m, R("#c02030"), outline=False)


@icon("star16", 28)
def _(cv):
    star(cv, 14, 15, 12, "#ffd040"); cv.sparkle(23, 4)


@icon("minus", 28)
def _(cv):
    m = cv.mask(); m.rect(5, 11, 23, 17); cv.part(m, R("#e04848"))


@icon("plus", 28)
def _(cv):
    plus(cv, 14, 14, 9, "#48c048")


@icon("kill", 28)
def _(cv):
    emblem(cv, "skull", 14, 15, 10, "#e8e0d0")
    m = cv.mask(); m.poly([(20, 0), (14, 9), (18, 9), (13, 17), (23, 6), (19, 6), (24, 0)]); cv.part(m, R("#ffe04a"))


# ======================================================= ค่าสถานะ 7 ตัว
@icon("s_str", 28)
def _(cv):  # กำปั้น
    m = cv.mask(); m.rect(6, 9, 21, 22); m.ell(4, 8, 10, 16); cv.part(m, R("#f0b890"))
    for x in (9, 13, 17):
        cv.line([(x, 9), (x, 14)], (180, 110, 80))
    m = cv.mask(); m.rect(8, 21, 19, 27); cv.part(m, R("#a03030"))


@icon("s_agi", 28)
def _(cv):  # ขนนกปีก
    m = cv.mask(); m.poly([(4, 25), (10, 12), (20, 3), (25, 4), (20, 12), (10, 22)]); cv.part(m, R("#7ae0b0"))
    cv.line([(5, 24), (22, 5)], (220, 255, 240))


@icon("s_vit", 28)
def _(cv):
    heart(cv, 14, 13, 11, "#e04050")
    m = cv.mask(); m.poly([(9, 12), (19, 12), (19, 17), (14, 21), (9, 17)]); cv.part(m, R("#c8d0dc"))


@icon("s_int", 28)
def _(cv):
    book(cv, 14, 15, 8, "#3a5ac0")
    star(cv, 14, 13, 4, "#ffd040")


@icon("s_spi", 28)
def _(cv):  # เปลวไฟวิญญาณ
    emblem(cv, "flame", 14, 14, 11, "#6ab0ff")


@icon("s_luk", 28)
def _(cv):  # ใบโคลเวอร์สี่แฉก
    for dx, dy in ((-4, -4), (4, -4), (-4, 4), (4, 4)):
        m = cv.mask(); m.ell(14 + dx - 5, 12 + dy - 5, 14 + dx + 5, 12 + dy + 5); cv.part(m, R("#4ac04a"))
    m = cv.mask(); m.rect(13, 16, 15, 27); cv.part(m, R("#3a8a3a"), outline=False)


@icon("s_cha", 28)
def _(cv):  # มงกุฎ
    m = cv.mask(); m.poly([(3, 22), (3, 8), (9, 14), (14, 5), (19, 14), (25, 8), (25, 22)]); cv.part(m, R("#f2c040"))
    for x, c in ((8, "#ff4a5a"), (14, "#4ab0ff"), (20, "#4ae07a")):
        m = cv.mask(); m.ell(x - 1.8, 16, x + 1.8, 19.6); cv.part(m, R(c), outline=False)


# ======================================================= อาชีพสายต่อสู้ 13
CLASS_WEAPON = {"swordsman": "holy_sword", "spellblade": "flame_sword", "assassin": "plague_dagger", "lancer": "bolt_spear",
                "berserker": "dragon_axe", "archer": "wind_bow", "ranger": "moon_bow", "mage": "star_staff", "paladin": "rock_hammer"}


def class_icon(cid):
    def fn(cv):
        if cid in CLASS_WEAPON:
            kind = CLASS_WEAPON[cid]
            base = {"swordsman": ("#c8d0dc", "#c8a040", "#ff6060"), "spellblade": ("#4a3aa0", "#b080ff", "#c07aff"),
                    "assassin": ("#3a3a44", "#2a2a30", "#ff4a6a"), "lancer": ("#46508e", "#ffd84a", "#fff27a"),
                    "berserker": ("#8a3a2a", "#c8c8c8", "#ff5a3a"), "archer": ("#6a4a2a", "#eaf0e0", "#9aff9a"),
                    "ranger": ("#4a6a3a", "#2a3a2a", "#c8ff8a"), "mage": ("#2a3478", "#d0dcff", "#8fb4ff"),
                    "paladin": ("#c8ccd8", "#f2c040", "#ffe27a")}[cid]
            weapon_icon(cv, cid, (base[0], base[1], base[2], kind, set(), "sun"))
            if cid == "spellblade": cv.glow("#b080ff", 60, 1)
        elif cid == "knight":
            m = cv.mask(); m.poly([(4, 4), (14, 1), (24, 4), (23, 15), (14, 26), (5, 15)]); cv.part(m, R("#7a8696"))
            m = cv.mask(); m.rect(12, 4, 16, 22); m.rect(6, 9, 22, 12); cv.part(m, R("#c03030"), outline=False)
        elif cid == "priest":
            book(cv, 14, 16, 7, "#e8e0c8")
            m = cv.mask(); m.rect(13, 9, 15, 21); m.rect(10, 12, 18, 14); cv.part(m, R("#f2c040"), outline=False)
            cv.glow("#fff0a0", 50, 1)
        elif cid == "monk":
            m = cv.mask(); m.ell(3, 3, 25, 25); m.cut_ell(7, 7, 21, 21); cv.part(m, R("#8a4a2a"))
            for a in range(0, 360, 40):
                x, y = 14 + math.cos(math.radians(a)) * 9, 14 + math.sin(math.radians(a)) * 9
                m = cv.mask(); m.ell(x - 2, y - 2, x + 2, y + 2); cv.part(m, R("#c87a3a"))
            emblem(cv, "flame", 14, 14, 4, "#ffb03a")
        elif cid == "bard":
            m = cv.mask(); m.ell(4, 12, 18, 26); cv.part(m, R("#c88a40"))
            m = cv.mask(); m.ell(9, 17, 13, 21); cv.part(m, R("#3a2010"), outline=False)
            m = cv.mask(); quad(m, 11, 19, 4, 15, 1.4, ang=-50); cv.part(m, R("#6a3a1a"))
            cv.line([diag(11, 19, 0, 0, -50), diag(11, 19, 14, 0, -50)], (240, 230, 200))
            emblem(cv, "flower", 22, 6, 3, "#ff9ac0")
    return fn


# ======================================================= อาชีพใหม่ 17
NEW_WEAPON = {  # อาชีพ: (โลหะ, ขอบ, ประกาย, ชนิดอาวุธ, ตราเสริม, สีตรา)
    "necromancer": ("#3a2a4a", "#9a8aa8", "#b07aff", "void_scythe", "skull", "#d8d0e8"),
    "druid": ("#4a6a2a", "#8a5a2a", "#9aff7a", "vine_hammer", "leaf", "#7ae05a"),
    "shaman": ("#3a5a6a", "#c8a060", "#7affd8", "ice_staff", "eye", "#ffd27a"),
    "samurai": ("#2a2a34", "#d0d8e8", "#ff6a6a", "katana", "flower", "#ffb0c8"),
    "ninja": ("#24242c", "#3a3a44", "#8a8aff", "plague_dagger", "star", "#c8c8d8"),
    "gladiator": ("#8a6a3a", "#d8b060", "#ffe07a", "trident", "sun", "#f2c040"),
    "dragoon": ("#7a2a2a", "#d8a040", "#ff8a4a", "bolt_spear", "claw", "#ffb04a"),
    "warlock": ("#2a1a3a", "#7a4aa0", "#d07aff", "star_staff", "eye", "#ff5aff"),
    "monster_hunter": ("#5a3a2a", "#a8a8b0", "#ff5a5a", "blood_sword", "claw", "#e8e0d0"),
}


def new_class_icon(cv, cid):
    if cid in NEW_WEAPON:
        m_, t_, a_, kind, emb, ec = NEW_WEAPON[cid]
        weapon_icon(cv, cid, (m_, t_, a_, kind, set(), "sun"))
        emblem(cv, emb, 22, 6, 3.2, ec)
        if cid in ("warlock", "necromancer"): cv.glow(a_, 55, 1)
    elif cid == "guardian":   # โล่หอคอยใหญ่ + ตราภูเขา
        m = cv.mask(); m.poly([(5, 3), (23, 3), (23, 17), (14, 26), (5, 17)]); cv.part(m, R("#3a5a9a"))
        m = cv.mask(); m.poly([(8, 6), (20, 6), (20, 16), (14, 22), (8, 16)]); cv.part(m, R("#5a82c8"), outline=False)
        emblem(cv, "mountain", 14, 13, 5, "#e8ecf4")
    elif cid == "engineer":   # เฟือง + ประแจ
        emblem(cv, "gear", 12, 15, 9, "#c8a060")
        m = cv.mask(); quad(m, 18, 10, -6, 7, 1.5, ang=-45); cv.part(m, R("#a8b0bc"))
        m = cv.mask(); m.ell(19, 2, 26, 9); m.cut_ell(21, 1, 25, 5); cv.part(m, R("#a8b0bc"))
    elif cid == "summoner":   # คัมภีร์ + วังวนอัญเชิญ
        emblem(cv, "swirl", 14, 9, 6, "#7ab0ff")
        book(cv, 14, 19, 7, "#5a3a8a", True)
        cv.glow("#7ab0ff", 50, 1)
    elif cid == "elementalist":   # ธาตุทั้งสี่วนรอบ
        for (x, y), k, c in (((9, 8), "flame", "#ff6a3a"), ((19, 8), "snow", "#8ad8ff"), ((9, 20), "bolt", "#ffe24a"), ((19, 20), "drop", "#4a8aff")):
            emblem(cv, k, x, y, 4.2, c)
        m = cv.mask(); m.ell(11.5, 11.5, 16.5, 16.5); cv.part(m, R("#f0f0ff"), outline=False)
        cv.glow("#ffffff", 40, 1)
    elif cid == "alchemist":   # ขวดยาเดือด
        m = cv.mask(); m.ell(5, 10, 23, 27); m.rect(11, 3, 17, 12); cv.part(m, R("#c8e0e8"))
        m = cv.mask(); m.ell(7, 15, 21, 25); cv.part(m, R("#5aff8a"), outline=False)
        m = cv.mask(); m.rect(10, 1, 18, 4); cv.part(m, R("#8a5a2a"))
        for x, y in ((11, 18), (16, 21), (14, 16)):
            m = cv.mask(); m.ell(x - 1.2, y - 1.2, x + 1.2, y + 1.2); cv.part(m, R("#d8ffe0"), outline=False)
        cv.glow("#5aff8a", 45, 1)
    elif cid == "beast_tamer":   # รอยกรงเล็บ + แส้
        m = cv.mask(); m.ell(3, 3, 25, 25); cv.part(m, R("#7a4a2a"))
        emblem(cv, "claw", 14, 14, 8, "#ffb04a")
    elif cid == "dancer":   # ดอกไม้ + ริบบิ้นวน
        emblem(cv, "swirl", 14, 14, 10, "#ff7ab0")
        emblem(cv, "flower", 14, 14, 5, "#ffd0e0")
    elif cid == "freelancer":   # ถุงเงิน + เหรียญดาว
        m = cv.mask(); m.ell(5, 9, 23, 27); m.poly([(10, 10), (14, 4), (18, 10)]); cv.part(m, R("#a87a4a"))
        m = cv.mask(); m.rect(9, 9, 19, 11); cv.part(m, R("#6a4a2a"), outline=False)
        emblem(cv, "star", 14, 18, 4.5, "#f2c040")


NEW_CLASSES = ("guardian", "engineer", "necromancer", "summoner", "druid", "shaman", "elementalist", "samurai", "ninja",
               "gladiator", "dragoon", "warlock", "alchemist", "beast_tamer", "dancer", "monster_hunter", "freelancer")


def build(write=True):
    os.makedirs(NGD, exist_ok=True)
    imgs = []
    for name, (fn, size) in ICONS.items():
        cv = Canvas(size, size); fn(cv)
        imgs.append(cv.img)
        if write: cv.save(os.path.join(NGD, name + ".png"))
    for cid in ("swordsman", "knight", "archer", "mage", "priest", "assassin", "monk", "lancer", "berserker", "ranger", "paladin", "spellblade", "bard"):
        cv = Canvas(28, 28); class_icon(cid)(cv)
        imgs.append(cv.img)
        if write: cv.save(os.path.join(NGD, "c_" + cid + ".png"))
    for cid in NEW_CLASSES:
        cv = Canvas(28, 28); new_class_icon(cv, cid)
        imgs.append(cv.img)
        if write: cv.save(os.path.join(NGD, "c_" + cid + ".png"))
    return imgs


if __name__ == "__main__":
    imgs = build(write="preview" not in sys.argv)
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(os.environ.get("TEMP", "."), "ui_preview.png")
    sheet(imgs, out, scale=3, cols=10)
    print("ok", len(imgs), out)
