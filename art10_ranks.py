# ป้ายแรงค์บนหัวตัวละคร 16x16: รูปทรงยิ่งสูงยิ่งอลังการ (มองแวบเดียวรู้ระดับ)
#   E D C = หกเหลี่ยมเรียบ · B A = โล่มีขอบ · S = โล่ + มงกุฎทอง · SS = โล่ + ปีก · SSS = โล่ + ปีก + มงกุฎ · U M G = ดาวแปดแฉกเรืองแสง
import os, sys
from art2_core import Canvas, sheet
from art2_gear import R
import art2_ui as U

RANKS = ["E", "D", "C", "B", "A", "S", "SS", "SSS", "U", "M", "G"]
COL = ["#9fb4c4", "#7fbfb0", "#4fa9d8", "#3f6fd8", "#7a5fe0", "#f0b030", "#c070f0", "#8f7cff", "#6a2a9a", "#f08fd0", "#fff2c0"]
GLYPH = {
    "E": ["111", "100", "110", "100", "111"], "D": ["110", "101", "101", "101", "110"], "C": ["011", "100", "100", "100", "011"],
    "B": ["110", "101", "110", "101", "110"], "A": ["010", "101", "111", "101", "101"], "S": ["011", "100", "010", "001", "110"],
    "U": ["101", "101", "101", "101", "111"], "M": ["101", "111", "111", "101", "101"], "G": ["011", "100", "101", "101", "011"],
}
GOLD = "#f2c040"


def text(cv, name, cy, light):
    w = len(name) * 4 - 1
    x0 = 8 - w // 2
    fg = (255, 255, 255) if light else (60, 36, 16)
    sh = (30, 20, 40) if light else (255, 240, 200)
    for k, ch in enumerate(name):
        for yy, row in enumerate(GLYPH[ch]):
            for xx, bit in enumerate(row):
                if bit == "1":
                    cv.dot(x0 + k * 4 + xx + 1, cy + yy + 1, sh + (200,))
                    cv.dot(x0 + k * 4 + xx, cy + yy, fg + (255,))


def hexagon(cv, col):
    m = cv.mask(); m.poly([(8, 0), (15, 4), (15, 11), (8, 15), (1, 11), (1, 4)]); cv.part(m, R(col))


def shield(cv, col, top=2, trim=GOLD):
    m = cv.mask(); m.poly([(2, top), (14, top), (14, 10), (8, 15), (2, 10)]); cv.part(m, R(trim))
    m = cv.mask(); m.poly([(3, top + 1), (13, top + 1), (13, 10), (8, 14), (3, 10)]); cv.part(m, R(col), outline=False)


def crown(cv):
    m = cv.mask(); m.poly([(4, 3), (4, 0), (6, 2), (8, 0), (10, 2), (12, 0), (12, 3)]); cv.part(m, R(GOLD))
    cv.dot(8, 1, (255, 90, 110, 255))


def wings(cv, col):
    """ปีกขนนกที่มุมบนสองข้าง (โผล่พ้นโล่)"""
    for side in (-1, 1):
        for k in range(3):   # ขนนก 3 เส้น ยาวลดหลั่น
            x0 = 2 if side < 0 else 13
            for j in range(4 - k):
                cv.dot(x0 + side * (j + 1), 1 + k * 2 + j // 2, (245, 245, 255, 255) if j < 3 - k else (190, 200, 230, 255))
        cv.dot(2 if side < 0 else 13, 2, (210, 215, 240, 255))


def star8(cv, col):
    import math
    pts = []
    for i in range(16):
        a = math.radians(i * 22.5 - 90)
        r = 7.5 if i % 2 == 0 else 4.2
        pts.append((8 + math.cos(a) * r, 8 + math.sin(a) * r))
    m = cv.mask(); m.poly(pts); cv.part(m, R(col))
    m = cv.mask(); m.ell(4, 4, 12, 12); cv.part(m, R(col, 0.6), outline=False)


def badge(i):
    name, col = RANKS[i], COL[i]
    cv = Canvas(16, 16)
    light = name not in ("S", "G", "M")
    if i <= 2:
        hexagon(cv, col); text(cv, name, 5, light)
    elif i <= 4:
        shield(cv, col, 2, "#c8ccd8" if i == 3 else GOLD); text(cv, name, 5, True)
    elif name == "S":
        shield(cv, col, 3); crown(cv); text(cv, name, 6, False)
    elif name == "SS":
        shield(cv, col, 3); wings(cv, col); text(cv, name, 6, True)
    elif name == "SSS":
        shield(cv, col, 3); wings(cv, col); crown(cv); text(cv, name, 7, True)
    else:
        star8(cv, col); cv.glow(col, 70, 1); text(cv, name, 6, name == "U")
    return cv


def build(write=True):
    out = []
    for i in range(len(RANKS)):
        cv = badge(i); out.append(cv.img)
        if write: cv.save(os.path.join(U.NGD, "r_%s.png" % RANKS[i]))
    return out


if __name__ == "__main__":
    prev = build(write="preview" not in sys.argv)
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(os.environ.get("TEMP", "."), "ranks2.png")
    sheet(prev, out, scale=5, cols=11)
    print("ok", out)
