# วาดอาวุธเทพ "โบราณวัตถุเงาดารา" + ไอคอนพลัง (pixel art PNG จริง)
# - ไอคอน 32x32: GameResources/ui/Icons/items/icon_{id}.png
# - ภาพในมือ (เล็ก): GameResources/items/weapons/w_{id}.png
# - ไอคอนพลัง 32x32: GameResources/ui/Icons/ngd/p_*.png
# สเกลภาพในมือเป็นค่าประมาณ จะปรับให้ตรงเกมหลังได้ภาพอ้างอิงจาก _art_ref
from PIL import Image, ImageDraw
import math, os

ROOT = os.path.dirname(os.path.abspath(__file__))
RES = os.path.join(ROOT, "NgaodaraPack", "GameResources")
ICON_DIR = os.path.join(RES, "ui", "Icons", "items")
HAND_DIR = os.path.join(RES, "items", "weapons")
NGD_DIR = os.path.join(RES, "ui", "Icons", "ngd")
INK = (18, 14, 30, 255)


def C(h, a=255):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4)) + (a,)


def canvas(w, h=None):
    im = Image.new("RGBA", (w, h or w), (0, 0, 0, 0))
    return im, ImageDraw.Draw(im)


def outline(im, col=INK):
    w, h = im.size
    src = im.copy().load()
    px = im.load()
    for y in range(h):
        for x in range(w):
            if src[x, y][3] == 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and src[nx, ny][3] > 0 and src[nx, ny] != col:
                        px[x, y] = col
                        break
    return im


def glow(im, col, strength=90):
    """รัศมีเรืองแสงจางๆ รอบรูปทรง (ใต้เส้นขอบ)"""
    w, h = im.size
    base = im.copy()
    g = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    gp = g.load()
    src = base.load()
    for y in range(h):
        for x in range(w):
            if src[x, y][3] == 0:
                best = 99
                for dy in range(-2, 3):
                    for dx in range(-2, 3):
                        nx, ny = x + dx, y + dy
                        if 0 <= nx < w and 0 <= ny < h and src[nx, ny][3] > 0:
                            best = min(best, abs(dx) + abs(dy))
                if best <= 3:
                    gp[x, y] = col[:3] + (int(strength / best),)
    g.alpha_composite(base)
    return g


def sparkle(d, x, y, col=(255, 255, 255, 255), big=False):
    d.point((x, y), fill=col)
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        d.point((x + dx, y + dy), fill=col[:3] + (180,))
    if big:
        for dx, dy in ((2, 0), (-2, 0), (0, 2), (0, -2)):
            d.point((x + dx, y + dy), fill=col[:3] + (110,))


def line_px(d, pts, col, w=1):
    d.line(pts, fill=col, width=w)


def diag_blade(d, x0, y0, x1, y1, width, edge, core, hi):
    """ใบมีดแนวทแยงจาก (x0,y0) ด้าม ไป (x1,y1) ปลาย"""
    steps = max(abs(x1 - x0), abs(y1 - y0))
    for i in range(steps + 1):
        t = i / steps
        x = x0 + (x1 - x0) * t
        y = y0 + (y1 - y0) * t
        ww = width * (1 - t * 0.55) if t > 0.75 else width
        r = max(0.6, ww / 2)
        d.ellipse((x - r, y - r, x + r, y + r), fill=edge)
    for i in range(steps + 1):
        t = i / steps
        x = x0 + (x1 - x0) * t
        y = y0 + (y1 - y0) * t
        if t < 0.92:
            d.point((round(x), round(y)), fill=core)
            if i % 3 == 0:
                d.point((round(x) - 1, round(y)), fill=hi)


# ================= ไอคอน 32x32 =================
def icon_starfall_blade():
    im, d = canvas(32)
    diag_blade(d, 9, 23, 27, 4, 5, C("#cfd8ff"), C("#ffffff"), C("#9fb0ff"))
    # ร่องเลือดสีทอง + ดาวฝังใบ
    for i in range(4, 15, 3):
        d.point((9 + i, 23 - i), fill=C("#ffd76a"))
    d.polygon([(4, 22), (14, 28), (15, 26), (6, 20)], fill=C("#f0c040"))   # การ์ด
    d.polygon([(5, 22), (13, 27), (13, 26), (6, 21)], fill=C("#fff0a0"))
    line_px(d, [(9, 24), (4, 29)], C("#5a2a7a"), 3)                          # ด้าม
    d.ellipse((1, 28, 5, 31), fill=C("#8f7cff"))                             # ปุ่มด้าม
    im = outline(glow(im, C("#8fb4ff"), 110))
    d = ImageDraw.Draw(im)
    sparkle(d, 26, 5, big=True)
    sparkle(d, 20, 8, C("#ffd76a"))
    return im


def icon_chrono_blade():
    im, d = canvas(32)
    diag_blade(d, 9, 23, 27, 4, 4, C("#e8d48a"), C("#fff6d0"), C("#b8902a"))
    # หน้าปัดเวลาที่การ์ด
    d.ellipse((3, 19, 13, 29), fill=C("#c070f0"))
    d.ellipse((5, 21, 11, 27), fill=C("#f6ecff"))
    line_px(d, [(8, 24), (8, 22)], C("#5a2a7a"))
    line_px(d, [(8, 24), (10, 24)], C("#5a2a7a"))
    line_px(d, [(4, 28), (1, 31)], C("#3a2050"), 3)
    im = outline(glow(im, C("#d9a8ff"), 100))
    d = ImageDraw.Draw(im)
    for (x, y) in ((22, 6), (18, 11), (25, 9)):
        d.point((x, y), fill=C("#ffffff"))
    sparkle(d, 27, 3, C("#ffe9a8"), True)
    return im


def icon_thunder_spear():
    im, d = canvas(32)
    line_px(d, [(4, 29), (21, 12)], C("#3f4a8a"), 2)                        # ด้าม
    for i in range(3):
        line_px(d, [(6 + i * 5, 26 - i * 5), (7 + i * 5, 25 - i * 5)], C("#ffd76a"), 2)
    d.polygon([(19, 10), (29, 2), (23, 14)], fill=C("#a8e8ff"))             # หัวหอก
    d.polygon([(21, 10), (28, 3), (22, 12)], fill=C("#ffffff"))
    d.polygon([(17, 12), (22, 9), (24, 15), (20, 15)], fill=C("#f0c040"))   # คอหอก
    im = outline(glow(im, C("#7fd8ff"), 120))
    d = ImageDraw.Draw(im)
    # สายฟ้ารอบหัว
    for pts in ([(27, 9), (25, 11), (28, 12), (26, 15)], [(14, 4), (17, 6), (15, 7), (18, 9)]):
        d.line(pts, fill=C("#fff7a0"), width=1)
    sparkle(d, 29, 2, big=True)
    return im


def icon_rift_scythe():
    im, d = canvas(32)
    line_px(d, [(6, 30), (19, 5)], C("#2a1838"), 2)                         # ด้าม
    # ใบเคียวโค้ง
    for r, col in ((15, C("#7a3fd0")), (13, C("#c070f0")), (11, (0, 0, 0, 0))):
        d.pieslice((4, 1, 4 + r * 2, 1 + r * 2), 200, 300, fill=col)
    d.arc((5, 2, 31, 28), 205, 295, fill=C("#ffffff"), width=1)
    # รอยแยกมิติ
    for (x, y) in ((10, 8), (12, 6), (9, 11)):
        d.point((x, y), fill=C("#00e5ff"))
    im = outline(glow(im, C("#b070ff"), 120))
    d = ImageDraw.Draw(im)
    sparkle(d, 6, 14, C("#00e5ff"), True)
    return im


def icon_cosmos_hammer():
    im, d = canvas(32)
    line_px(d, [(5, 30), (15, 15)], C("#4a3020"), 3)
    d.polygon([(10, 6), (22, 2), (29, 13), (17, 18)], fill=C("#2a2a6a"))    # หัวค้อน
    d.polygon([(12, 7), (21, 4), (27, 12), (18, 16)], fill=C("#4a3fb0"))
    d.ellipse((15, 6, 23, 14), fill=C("#1a1040"))                          # กาแล็กซีฝัง
    d.arc((15, 6, 23, 14), 0, 270, fill=C("#ff9ad8"), width=1)
    d.point((19, 10), fill=C("#ffffff"))
    d.polygon([(13, 15), (18, 13), (19, 17), (15, 19)], fill=C("#f0c040"))
    im = outline(glow(im, C("#8f7cff"), 120))
    d = ImageDraw.Draw(im)
    for (x, y) in ((13, 9), (24, 6), (26, 11)):
        d.point((x, y), fill=C("#ffffff"))
    sparkle(d, 28, 3, C("#ffd6f0"), True)
    return im


def icon_crescent_bow():
    im, d = canvas(32)
    d.arc((4, 2, 26, 30), 300, 60, fill=C("#e8f0ff"), width=3)             # คันธนูจันทร์เสี้ยว
    d.arc((6, 4, 24, 28), 300, 60, fill=C("#9fb4ff"), width=1)
    line_px(d, [(19, 5), (19, 27)], C("#fff7c0"))                           # สาย
    line_px(d, [(4, 16), (27, 16)], C("#d0d8e8"))                           # ลูกธนู
    d.polygon([(28, 16), (24, 13), (24, 19)], fill=C("#ffd76a"))
    d.polygon([(4, 16), (7, 13), (7, 19)], fill=C("#8fb4ff"))
    im = outline(glow(im, C("#b0c8ff"), 110))
    d = ImageDraw.Draw(im)
    sparkle(d, 23, 4, big=True)
    sparkle(d, 23, 28, C("#ffd76a"))
    return im


def icon_galaxy_scepter():
    im, d = canvas(32)
    line_px(d, [(5, 30), (18, 14)], C("#e0c070"), 2)                        # ด้ามทอง
    for i in range(3):
        d.point((8 + i * 4, 26 - i * 5), fill=C("#8f7cff"))
    d.ellipse((15, 3, 29, 17), fill=C("#1a1040"))                          # ลูกแก้วกาแล็กซี
    d.ellipse((17, 5, 27, 15), fill=C("#3a2a90"))
    d.arc((16, 4, 28, 16), 30, 210, fill=C("#ff9ad8"), width=1)
    d.arc((18, 6, 26, 14), 210, 390, fill=C("#7fd8ff"), width=1)
    d.point((22, 10), fill=C("#ffffff"))
    d.polygon([(14, 15), (18, 13), (20, 17), (16, 18)], fill=C("#f0c040"))
    im = outline(glow(im, C("#a090ff"), 130))
    d = ImageDraw.Draw(im)
    for (x, y) in ((19, 7), (25, 13), (24, 6)):
        d.point((x, y), fill=C("#ffffff"))
    sparkle(d, 29, 2, C("#ffd6f0"), True)
    return im


# ================= ภาพในมือ (เล็ก) =================
def hand(icon_fn, size):
    """ย่อจากไอคอนแบบรักษาพิกเซล: ใช้ภาพไอคอนแล้วลดขนาด (nearest) ให้เหลือสเกลในมือ"""
    im = icon_fn()
    return im.resize((size, size), Image.NEAREST)


# ================= ไอคอนพลัง 32x32 =================
def p_relic():
    im, d = canvas(32)
    d.ellipse((2, 2, 29, 29), fill=C("#3a2060"))
    d.ellipse((5, 5, 26, 26), fill=C("#6a40b0"))
    ic = icon_starfall_blade().resize((24, 24), Image.NEAREST)
    im.alpha_composite(ic, (4, 4))
    d = ImageDraw.Draw(im)
    sparkle(d, 25, 6, C("#ffd76a"), True)
    return im


def p_starfall():
    im, d = canvas(32)
    d.rectangle((0, 0, 31, 31), fill=(0, 0, 0, 0))
    d.ellipse((2, 2, 29, 29), fill=C("#141030"))
    for k, (x, y) in enumerate(((22, 6), (14, 10), (24, 16))):
        d.line((x, y, x - 9, y + 9), fill=C("#ffd76a", 160), width=2)
        d.polygon([(x, y - 3), (x + 3, y), (x, y + 3), (x - 3, y)], fill=C("#fff6d0"))
    d.rectangle((6, 24, 25, 26), fill=C("#c05030"))
    d.polygon([(10, 24), (16, 19), (21, 24)], fill=C("#ff9040"))
    return outline(im)


def p_timestop():
    im, d = canvas(32)
    d.ellipse((2, 2, 29, 29), fill=C("#5a2a7a"))
    d.ellipse((5, 5, 26, 26), fill=C("#f6ecff"))
    for a in range(0, 360, 30):
        r = math.radians(a)
        d.point((15.5 + 9 * math.cos(r), 15.5 + 9 * math.sin(r)), fill=C("#5a2a7a"))
    d.line((16, 16, 16, 9), fill=C("#3a1050"), width=2)
    d.line((16, 16, 21, 18), fill=C("#3a1050"), width=1)
    d.ellipse((14, 14, 17, 17), fill=C("#c070f0"))
    im = outline(im)
    d = ImageDraw.Draw(im)
    for (x, y) in ((4, 8), (27, 22), (26, 6)):
        d.point((x, y), fill=C("#a8e8ff"))
    return im


def p_ascension():
    im, d = canvas(32)
    d.ellipse((2, 2, 29, 29), fill=C("#2a3a70"))
    for i, col in enumerate(("#fff2c0", "#ffd76a", "#f0c040")):
        d.polygon([(16, 4 + i * 2), (23 - i, 16), (16, 28 - i * 2), (9 + i, 16)], fill=C(col))
    d.polygon([(16, 2), (18, 8), (14, 8)], fill=C("#ffffff"))
    for x in (8, 24):
        d.line((x, 26, x, 18), fill=C("#fff2c0"), width=1)
        d.polygon([(x, 16), (x + 2, 19), (x - 2, 19)], fill=C("#fff2c0"))
    return outline(im)


def p_rift():
    """ประตูมิติ (ธีม U): วงรีแตกร้าวสีม่วงดำ มีเศษมิติลอย"""
    im, d = canvas(32)
    d.ellipse((5, 3, 26, 29), fill=C("#1a0830"))
    d.ellipse((8, 6, 23, 26), fill=C("#5a2a7a"))
    d.ellipse((11, 9, 20, 23), fill=C("#c070f0"))
    d.ellipse((13, 12, 18, 20), fill=C("#0a0018"))
    for pts in ([(15, 3), (14, 8), (17, 11)], [(26, 14), (22, 16), (24, 20)], [(6, 18), (10, 17), (8, 23)]):
        d.line(pts, fill=C("#00e5ff"), width=1)
    im = outline(glow(im, C("#b070ff"), 120))
    d = ImageDraw.Draw(im)
    for (x, y) in ((3, 6), (28, 25), (27, 5)):
        d.rectangle((x, y, x + 1, y + 1), fill=C("#c070f0"))
    sparkle(d, 16, 16, C("#ffffff"))
    return im


RELICS = {
    "ngd_starfall_blade": (icon_starfall_blade, 16),
    "ngd_chrono_blade": (icon_chrono_blade, 16),
    "ngd_thunder_spear": (icon_thunder_spear, 18),
    "ngd_rift_scythe": (icon_rift_scythe, 16),
    "ngd_cosmos_hammer": (icon_cosmos_hammer, 16),
    "ngd_crescent_bow": (icon_crescent_bow, 14),
    "ngd_galaxy_scepter": (icon_galaxy_scepter, 16),
}

if __name__ == "__main__":
    for d_ in (ICON_DIR, HAND_DIR, NGD_DIR):
        os.makedirs(d_, exist_ok=True)
    tiles = []
    for rid, (fn, hs) in RELICS.items():
        ic = fn()
        ic.save(os.path.join(ICON_DIR, "icon_" + rid + ".png"))
        hand(fn, hs).save(os.path.join(HAND_DIR, "w_" + rid + ".png"))
        tiles.append(ic)
    for name, fn in (("p_relic", p_relic), ("p_starfall", p_starfall), ("p_timestop", p_timestop), ("p_ascension", p_ascension), ("p_rift", p_rift)):
        im = fn()
        im.save(os.path.join(NGD_DIR, name + ".png"))
        tiles.append(im)
    sheet = Image.new("RGBA", (len(tiles) * 104 + 8, 112), (52, 60, 82, 255))
    for i, t in enumerate(tiles):
        big = t.resize((96, 96), Image.NEAREST)
        sheet.paste(big, (8 + i * 104, 8), big)
    sheet.save(os.path.join(ROOT, "build", "relics_preview.png"))
    print("ok", len(tiles))
