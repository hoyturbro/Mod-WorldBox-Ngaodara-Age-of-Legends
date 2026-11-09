# วาดไอคอน pixel art ของมอด (ภาพ PNG จริง) ให้เข้ากับสไตล์ WorldBox
# - ปุ่มแท็บ 32x32: tab eye star menu break
# - ไอคอน UI เล็ก 16x16: plus minus heal immortal kill young older dice exp pts
# - ค่าสถานะ 16x16: s_str s_agi s_vit s_int s_spi s_luk s_cha
# - อาชีพ 16x16: c_swordsman c_knight c_archer c_mage c_priest c_assassin c_monk
# - ป้ายแรงค์ 16x16: r_E … r_G (สีตาม AURA_DESIGN)
from PIL import Image, ImageDraw
import math, os

ROOT = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(ROOT, "NgaodaraPack", "GameResources", "ui", "Icons", "ngd")
INK = (20, 16, 28, 255)


def canvas(s):
    im = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    return im, ImageDraw.Draw(im)


def outline(im):
    """เส้นขอบเข้ม 1px รอบรูปทรง แบบไอคอนในเกม"""
    s = im.size[0]
    src = im.copy().load()
    px = im.load()
    for y in range(s):
        for x in range(s):
            if src[x, y][3] == 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < s and 0 <= ny < s and src[nx, ny][3] > 0 and src[nx, ny] != INK:
                        px[x, y] = INK
                        break
    return im


def star_pts(cx, cy, r1, r2, n=5, rot=-90):
    pts = []
    for i in range(n * 2):
        r = r1 if i % 2 == 0 else r2
        a = math.radians(rot + i * 180 / n)
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return pts


def hexc(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4)) + (255,)


def shade(c, f):
    return tuple(max(0, min(255, int(v * f))) for v in c[:3]) + (255,)


# ---------- ปุ่มแท็บ 32px ----------
def eye():
    im, d = canvas(32)
    d.ellipse((3, 9, 28, 23), fill=(240, 236, 220, 255))
    d.ellipse((10, 9, 21, 22), fill=(80, 150, 230, 255))
    d.ellipse((12, 11, 19, 20), fill=(30, 60, 140, 255))
    d.polygon(star_pts(15.5, 15.5, 3, 1.3, 4), fill=(255, 230, 120, 255))
    d.point((13, 12), fill=(255, 255, 255, 255))
    for x in (6, 10, 15, 20, 25):
        d.line((x, 8, x + (x - 15) // 5, 5), fill=(255, 215, 106, 255))
    return outline(im)


def star32():
    im, d = canvas(32)
    d.polygon(star_pts(16, 17, 14, 6), fill=(255, 205, 60, 255))
    d.polygon(star_pts(16, 17, 9, 4), fill=(255, 236, 140, 255))
    d.point((13, 12), fill=(255, 255, 255, 255))
    d.point((14, 12), fill=(255, 255, 255, 255))
    return outline(im)


def menu():
    im, d = canvas(32)
    d.rectangle((5, 4, 26, 28), fill=(120, 80, 200, 255))
    d.rectangle((7, 6, 24, 26), fill=(200, 180, 240, 255))
    for i, y in enumerate((10, 15, 20)):
        d.rectangle((10, y, 21, y + 2), fill=(60, 40, 110, 255))
        d.rectangle((10, y, 10 + 4 + i * 3, y + 2), fill=(255, 215, 106, 255))
    d.polygon(star_pts(24, 6, 4, 1.8), fill=(255, 230, 120, 255))
    return outline(im)


def brk():
    im, d = canvas(32)
    d.ellipse((4, 4, 27, 27), fill=(90, 40, 160, 255))
    d.ellipse((8, 8, 23, 23), fill=(170, 120, 255, 255))
    d.polygon([(17, 3), (11, 17), (16, 17), (13, 29), (22, 13), (17, 13), (20, 3)], fill=(255, 240, 150, 255))
    return outline(im)


def tab():
    im, d = canvas(32)
    d.ellipse((2, 2, 29, 29), fill=(25, 20, 60, 255))
    d.ellipse((5, 5, 26, 26), fill=(50, 40, 110, 255))
    d.polygon(star_pts(16, 16, 11, 4.5), fill=(255, 215, 106, 255))
    d.polygon(star_pts(16, 16, 6, 2.5), fill=(255, 250, 220, 255))
    for (x, y) in ((7, 8), (24, 9), (8, 23), (23, 24)):
        d.point((x, y), fill=(200, 220, 255, 255))
    return outline(im)


# ---------- ไอคอน UI 16px ----------
def plus():
    im, d = canvas(16)
    d.rectangle((6, 2, 9, 13), fill=(110, 220, 110, 255))
    d.rectangle((2, 6, 13, 9), fill=(110, 220, 110, 255))
    d.point((6, 2), fill=(200, 255, 200, 255))
    return outline(im)


def minus():
    im, d = canvas(16)
    d.rectangle((2, 6, 13, 9), fill=(235, 100, 90, 255))
    return outline(im)


def heart(col=(230, 60, 70, 255)):
    im, d = canvas(16)
    d.ellipse((1, 2, 8, 9), fill=col)
    d.ellipse((7, 2, 14, 9), fill=col)
    d.polygon([(1, 6), (14, 6), (7.5, 14)], fill=col)
    d.point((4, 4), fill=(255, 220, 220, 255))
    return outline(im)


def heal():
    im = heart()
    d = ImageDraw.Draw(im)
    d.rectangle((7, 4, 8, 10), fill=(255, 255, 255, 255))
    d.rectangle((5, 6, 10, 7), fill=(255, 255, 255, 255))
    return im


def immortal():
    im, d = canvas(16)
    gold = (255, 205, 60, 255)
    d.ellipse((1, 4, 8, 11), outline=gold, width=2)
    d.ellipse((7, 4, 14, 11), outline=gold, width=2)
    return outline(im)


def kill():
    im, d = canvas(16)
    bone = (235, 230, 215, 255)
    d.ellipse((2, 1, 13, 11), fill=bone)
    d.rectangle((4, 9, 11, 13), fill=bone)
    d.rectangle((4, 5, 6, 7), fill=INK)
    d.rectangle((9, 5, 11, 7), fill=INK)
    d.point((7, 9), fill=INK)
    d.point((8, 9), fill=INK)
    for x in (5, 7, 9):
        d.line((x, 12, x, 13), fill=INK)
    return outline(im)


def hourglass(sand_top=True):
    im, d = canvas(16)
    wood = (150, 100, 60, 255)
    glass = (200, 230, 245, 255)
    sand = (240, 200, 90, 255)
    d.rectangle((2, 1, 13, 2), fill=wood)
    d.rectangle((2, 13, 13, 14), fill=wood)
    d.polygon([(3, 3), (12, 3), (8, 8), (7, 8)], fill=glass)
    d.polygon([(7, 8), (8, 8), (12, 12), (3, 12)], fill=glass)
    if sand_top:
        d.polygon([(4, 4), (11, 4), (8, 7), (7, 7)], fill=sand)
        d.line((7, 8, 7, 11), fill=sand)
    else:
        d.polygon([(5, 11), (10, 11), (8, 9), (7, 9)], fill=sand)
        d.rectangle((4, 11, 11, 12), fill=sand)
    return outline(im)


def dice():
    im, d = canvas(16)
    d.rectangle((2, 2, 13, 13), fill=(240, 240, 240, 255))
    for (x, y) in ((4, 4), (10, 4), (7, 7), (4, 10), (10, 10)):
        d.rectangle((x, y, x + 1, y + 1), fill=(200, 40, 60, 255))
    return outline(im)


def exp_icon():
    im, d = canvas(16)
    d.polygon([(8, 1), (14, 8), (8, 15), (2, 8)], fill=(90, 200, 255, 255))
    d.polygon([(8, 3), (12, 8), (8, 8)], fill=(200, 240, 255, 255))
    return outline(im)


def pts_icon():
    im, d = canvas(16)
    d.ellipse((1, 1, 14, 14), fill=(255, 205, 60, 255))
    d.ellipse((3, 3, 12, 12), fill=(255, 230, 130, 255))
    d.rectangle((7, 4, 8, 11), fill=(170, 110, 20, 255))
    d.rectangle((4, 7, 11, 8), fill=(170, 110, 20, 255))
    return outline(im)


def star16():
    im, d = canvas(16)
    d.polygon(star_pts(8, 8.5, 7, 3), fill=(255, 205, 60, 255))
    d.point((6, 6), fill=(255, 255, 255, 255))
    return outline(im)


# ---------- ค่าสถานะ ----------
def s_str():
    im, d = canvas(16)   # กำปั้น
    sk = (235, 170, 120, 255)
    d.rectangle((3, 4, 12, 12), fill=sk)
    for x in (3, 6, 9):
        d.line((x + 2, 4, x + 2, 7), fill=shade(sk, 0.7))
    d.rectangle((3, 9, 7, 12), fill=shade(sk, 0.85))
    d.rectangle((5, 12, 12, 14), fill=(200, 60, 60, 255))
    return outline(im)


def s_agi():
    im, d = canvas(16)   # ปีก
    w = (240, 250, 255, 255)
    d.polygon([(2, 12), (6, 4), (14, 2), (11, 7), (14, 8), (9, 11), (12, 12)], fill=w)
    d.line((4, 11, 12, 4), fill=(170, 200, 230, 255))
    return outline(im)


def s_vit():
    return heart((230, 80, 90, 255))


def s_int():
    im, d = canvas(16)   # หนังสือ
    d.rectangle((2, 3, 13, 13), fill=(60, 110, 210, 255))
    d.rectangle((4, 3, 13, 12), fill=(240, 235, 210, 255))
    d.line((4, 3, 4, 13), fill=(30, 60, 140, 255))
    for y in (6, 8, 10):
        d.line((6, y, 11, y), fill=(150, 140, 120, 255))
    return outline(im)


def s_spi():
    im, d = canvas(16)   # ลูกแก้วปราณ
    d.ellipse((2, 2, 13, 13), fill=(120, 80, 230, 255))
    d.ellipse((4, 4, 11, 11), fill=(180, 150, 255, 255))
    d.ellipse((5, 4, 7, 6), fill=(255, 255, 255, 255))
    return outline(im)


def s_luk():
    im, d = canvas(16)   # ใบโคลเวอร์
    g = (90, 200, 90, 255)
    for (x, y) in ((5, 2), (9, 6), (5, 10), (1, 6)):
        d.ellipse((x, y, x + 5, y + 5), fill=g)
    d.line((8, 10, 12, 15), fill=(60, 140, 60, 255))
    d.point((7, 4), fill=(200, 255, 200, 255))
    return outline(im)


def s_cha():
    im, d = canvas(16)   # มงกุฎ
    gold = (255, 205, 60, 255)
    d.polygon([(2, 12), (2, 4), (5, 8), (8, 3), (11, 8), (14, 4), (14, 12)], fill=gold)
    d.rectangle((2, 11, 14, 13), fill=shade(gold, 0.8))
    d.point((8, 6), fill=(230, 60, 90, 255))
    return outline(im)


# ---------- อาชีพ ----------
def c_swordsman():
    im, d = canvas(16)
    d.polygon([(12, 1), (14, 3), (6, 11), (4, 9)], fill=(220, 230, 240, 255))
    d.line((5, 8, 8, 11), fill=(150, 100, 60, 255), width=2)
    d.line((2, 13, 4, 11), fill=(120, 80, 40, 255), width=2)
    return outline(im)


def c_knight():
    im, d = canvas(16)
    d.polygon([(2, 2), (13, 2), (13, 8), (7.5, 14), (2, 8)], fill=(70, 110, 200, 255))
    d.polygon([(4, 4), (11, 4), (11, 8), (7.5, 12), (4, 8)], fill=(110, 150, 230, 255))
    d.rectangle((7, 4, 8, 11), fill=(255, 215, 106, 255))
    return outline(im)


def c_archer():
    im, d = canvas(16)
    d.arc((1, 1, 15, 15), 300, 60 + 360 - 360, fill=(150, 100, 60, 255), width=2)
    d.arc((-6, 1, 10, 15), -60, 60, fill=(150, 100, 60, 255), width=2)
    d.line((5, 2, 5, 14), fill=(230, 230, 230, 255))
    d.line((5, 8, 14, 8), fill=(200, 200, 200, 255))
    d.polygon([(14, 8), (11, 6), (11, 10)], fill=(200, 200, 210, 255))
    return outline(im)


def c_mage():
    im, d = canvas(16)
    d.line((4, 14, 10, 5), fill=(150, 100, 60, 255), width=2)
    d.ellipse((8, 1, 14, 7), fill=(90, 200, 255, 255))
    d.point((10, 3), fill=(255, 255, 255, 255))
    return outline(im)


def c_priest():
    im, d = canvas(16)
    gold = (255, 225, 120, 255)
    d.rectangle((7, 1, 9, 14), fill=gold)
    d.rectangle((3, 4, 13, 6), fill=gold)
    d.ellipse((5, 2, 11, 8), outline=(255, 255, 220, 255))
    return outline(im)


def c_assassin():
    im, d = canvas(16)
    d.polygon([(13, 2), (14, 3), (8, 10), (7, 9)], fill=(190, 200, 210, 255))
    d.line((5, 9, 8, 12), fill=(80, 40, 100, 255), width=2)
    d.line((2, 14, 5, 11), fill=(60, 30, 80, 255), width=2)
    d.point((3, 3), fill=(160, 60, 220, 255))
    return outline(im)


def c_monk():
    return s_str()


# ---------- ป้ายแรงค์ ----------
GLYPH = {  # ฟอนต์ pixel 3x5
    "E": ["111", "100", "110", "100", "111"],
    "D": ["110", "101", "101", "101", "110"],
    "C": ["011", "100", "100", "100", "011"],
    "B": ["110", "101", "110", "101", "110"],
    "A": ["010", "101", "111", "101", "101"],
    "S": ["011", "100", "010", "001", "110"],
    "U": ["101", "101", "101", "101", "111"],
    "M": ["101", "111", "111", "101", "101"],
    "G": ["011", "100", "101", "101", "011"],
}
RANKS = ["E", "D", "C", "B", "A", "S", "SS", "SSS", "U", "M", "G"]
RANK_COLORS = ["#9fb4c4", "#7fbfb0", "#4fa9d8", "#3f6fd8", "#7a5fe0", "#f0c040",
               "#c070f0", "#8f7cff", "#5a2a7a", "#f08fd0", "#fff2c0"]


def rank_badge(name, col):
    im, d = canvas(16)
    c = hexc(col)
    d.polygon([(8, 0), (15, 4), (15, 11), (8, 15), (1, 11), (1, 4)], fill=shade(c, 0.55))
    d.polygon([(8, 2), (13, 5), (13, 10), (8, 13), (3, 10), (3, 5)], fill=c)
    txt = (255, 255, 255, 255) if sum(c[:3]) < 560 else (60, 40, 20, 255)
    w = len(name) * 4 - 1
    x0 = 8 - w // 2
    for k, ch in enumerate(name):
        g = GLYPH[ch]
        for yy, row in enumerate(g):
            for xx, bit in enumerate(row):
                if bit == "1":
                    d.point((x0 + k * 4 + xx, 5 + yy), fill=txt)
    return outline(im)


ICONS = {
    "eye": eye, "star": star32, "menu": menu, "break": brk, "tab": tab,
    "plus": plus, "minus": minus, "heal": heal, "immortal": immortal, "kill": kill,
    "young": lambda: hourglass(True), "older": lambda: hourglass(False), "dice": dice,
    "exp": exp_icon, "pts": pts_icon, "star16": star16,
    "s_str": s_str, "s_agi": s_agi, "s_vit": s_vit, "s_int": s_int, "s_spi": s_spi, "s_luk": s_luk, "s_cha": s_cha,
    "c_swordsman": c_swordsman, "c_knight": c_knight, "c_archer": c_archer, "c_mage": c_mage,
    "c_priest": c_priest, "c_assassin": c_assassin, "c_monk": c_monk,
}


# ---------- ยุค (16px) ----------
def era_stone():
    im, d = canvas(16)
    d.polygon([(3, 13), (5, 4), (11, 2), (13, 9), (10, 14)], fill=(150, 150, 150, 255))
    d.polygon([(5, 6), (9, 4), (8, 9)], fill=(190, 190, 190, 255))
    return outline(im)


def era_bronze():
    im, d = canvas(16)
    d.polygon([(8, 1), (10, 10), (8, 12), (6, 10)], fill=(205, 127, 50, 255))
    d.line((8, 3, 8, 10), fill=(240, 180, 110, 255))
    d.rectangle((4, 11, 12, 12), fill=(140, 80, 30, 255))
    d.rectangle((7, 13, 9, 15), fill=(110, 70, 30, 255))
    return outline(im)


def era_iron():
    im, d = canvas(16)
    d.polygon([(3, 2), (13, 2), (13, 8), (8, 14), (3, 8)], fill=(150, 160, 175, 255))
    d.polygon([(5, 4), (11, 4), (11, 8), (8, 12), (5, 8)], fill=(195, 205, 220, 255))
    d.line((8, 4, 8, 11), fill=(110, 120, 135, 255))
    return outline(im)


def era_kingdom():
    return s_cha()


def era_magic():
    im, d = canvas(16)
    d.polygon([(8, 1), (13, 13), (3, 13)], fill=(120, 80, 220, 255))
    d.rectangle((2, 12, 14, 14), fill=(90, 50, 170, 255))
    d.point((8, 6), fill=(255, 230, 120, 255)); d.point((6, 10), fill=(255, 255, 255, 255)); d.point((10, 9), fill=(255, 255, 255, 255))
    return outline(im)


def era_star():
    return star16()


def era_arrow(forward=True):
    im, d = canvas(32)
    d.ellipse((2, 2, 29, 29), fill=(30, 50, 90, 255))
    d.ellipse((5, 5, 26, 26), fill=(60, 110, 180, 255))
    if forward:
        d.polygon([(10, 9), (22, 16), (10, 23)], fill=(255, 215, 106, 255))
        d.polygon([(16, 9), (26, 16), (16, 23)], fill=(255, 240, 180, 255))
    else:
        d.polygon([(22, 9), (10, 16), (22, 23)], fill=(255, 215, 106, 255))
        d.polygon([(16, 9), (6, 16), (16, 23)], fill=(255, 240, 180, 255))
    return outline(im)


def map_icon(level):
    im, d = canvas(16)
    d.rectangle((1, 2, 14, 13), fill=(60, 120, 200, 255))
    land = (110, 180, 90, 255)
    if level >= 1: d.ellipse((2, 4, 9, 11), fill=land)
    if level >= 2: d.ellipse((7, 3, 14, 9), fill=land)
    if level >= 3: d.ellipse((6, 8, 13, 13), fill=land)
    d.point((4, 6), fill=(230, 230, 230, 255))
    return outline(im)

def chronicle():
    im, d = canvas(32)
    d.rectangle((5, 6, 26, 27), fill=(120, 70, 40, 255))
    d.rectangle((7, 4, 24, 25), fill=(240, 225, 185, 255))
    d.line((15, 4, 15, 25), fill=(170, 140, 100, 255))
    for y in (9, 13, 17, 21):
        d.line((9, y, 13, y), fill=(120, 100, 80, 255)); d.line((17, y, 22, y), fill=(120, 100, 80, 255))
    d.polygon(star_pts(24, 6, 4, 1.8), fill=(255, 215, 106, 255))
    d.rectangle((19, 22, 21, 30), fill=(200, 50, 60, 255))
    return outline(im)

ICONS.update({
    "chronicle": chronicle,
    "era_stone": era_stone, "era_bronze": era_bronze, "era_iron": era_iron, "era_kingdom": era_kingdom,
    "era_magic": era_magic, "era_star": era_star,
    "era_forward": lambda: era_arrow(True), "era_back": lambda: era_arrow(False),
    "map_colossal": lambda: map_icon(1), "map_continent": lambda: map_icon(2), "map_mega": lambda: map_icon(3),
})

if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    for name, fn in ICONS.items():
        fn().save(os.path.join(OUT, name + ".png"))
    for r, col in zip(RANKS, RANK_COLORS):
        pass   # ป้ายแรงค์รุ่นใหม่: art10_ranks.py
    tab().resize((64, 64), Image.NEAREST).save(os.path.join(ROOT, "NgaodaraPack", "icon.png"))
    # แผ่นพรีวิว
    names = sorted(os.listdir(OUT))
    sheet = Image.new("RGBA", (len(names) * 40 + 8, 48), (60, 70, 90, 255))
    for i, n in enumerate(names):
        im = Image.open(os.path.join(OUT, n))
        im = im.resize((32, 32), Image.NEAREST)
        sheet.paste(im, (8 + i * 40, 8), im)
    sheet.save(os.path.join(ROOT, "build", "icons_preview.png"))
    print(len(names), "icons")
