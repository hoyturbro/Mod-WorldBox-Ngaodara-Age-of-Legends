# วาดอุปกรณ์แบบ procedural (pixel art PNG จริง):
#   เซ็ตธาตุ 8 × 6 ชิ้น · ชุดวัสดุ 4 × 3 ชิ้น · ตราธาตุ e_* · ตราวัสดุ m_* · ไอคอนอาชีพใหม่ c_* · อาชีพคนธรรมดา pr_*
# รูปทรงแต่ละชิ้นวาดด้วยมือเป็นโครง แล้วลงสีด้วย "วัสดุ" (โทนหลัก) + "ธาตุ" (สีเน้น อัญมณี และแสงเรือง)
from PIL import Image, ImageDraw
import math, os, sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from tools_make_relics import C, canvas, outline, glow, sparkle, diag_blade, INK  # noqa

ROOT = os.path.dirname(os.path.abspath(__file__))
RES = os.path.join(ROOT, "NgaodaraPack", "GameResources")
ICON_DIR = os.path.join(RES, "ui", "Icons", "items")
HAND_DIR = os.path.join(RES, "items", "weapons")
NGD_DIR = os.path.join(RES, "ui", "Icons", "ngd")

ELEMENTS = {  # สีเน้น · สีโลหะของเซ็ต (โทนเข้ม/โทนกลาง/ไฮไลต์)
    "fire":    ("#ff6a3a", ("#7a2414", "#c0442a", "#ffb08a")),
    "ice":     ("#8fd8ff", ("#2a5a8a", "#6aa8d8", "#e0f6ff")),
    "thunder": ("#ffe45a", ("#5a4a1a", "#b89a30", "#fff3b0")),
    "wind":    ("#9affc8", ("#2a6a4a", "#5ab88a", "#d8ffe8")),
    "earth":   ("#c8964a", ("#4a3018", "#8a6030", "#e0c08a")),
    "light":   ("#fff2c0", ("#8a7a40", "#e0d090", "#ffffff")),
    "dark":    ("#9a5aff", ("#1a1028", "#3a2458", "#8a6ab8")),
    "star":    ("#7fa8ff", ("#141a40", "#2a3a8a", "#c8d8ff")),
}
WEAPON_KIND = {"fire": "sword", "ice": "staff", "thunder": "spear", "wind": "bow",
               "earth": "hammer", "light": "sword", "dark": "axe", "star": "staff"}
MATERIALS = {  # โทนเข้ม/กลาง/ไฮไลต์
    "leather": ("#4a2c18", "#8a5a34", "#c89a6a"),
    "bronze":  ("#5a3414", "#b0722e", "#f0b070"),
    "iron":    ("#3a4048", "#7a8696", "#d0d8e2"),
    "mythril": ("#1a4a6a", "#4aa0c8", "#c8f0ff"),
}


def pal(t):
    return tuple(C(x) for x in t)


# ---------------- รูปทรง ----------------
def helmet(d, m, acc):
    dk, md, hi = m
    d.pieslice((6, 4, 26, 28), 180, 360, fill=md)
    d.rectangle((6, 15, 26, 22), fill=md)
    d.rectangle((8, 16, 24, 18), fill=dk)            # ช่องตา
    d.rectangle((15, 4, 17, 22), fill=hi)            # สันหมวก
    d.polygon([(16, 1), (19, 6), (13, 6)], fill=acc)  # ยอดประดับ
    d.rectangle((6, 22, 10, 26), fill=dk); d.rectangle((22, 22, 26, 26), fill=dk)
    d.point((10, 9), fill=hi)


def armor(d, m, acc):
    dk, md, hi = m
    d.polygon([(6, 6), (12, 4), (20, 4), (26, 6), (27, 14), (24, 16), (24, 28), (8, 28), (8, 16), (5, 14)], fill=md)
    d.polygon([(12, 4), (16, 9), (20, 4)], fill=dk)                # คอ
    d.rectangle((8, 20, 24, 22), fill=dk)                        # เข็มขัด
    d.ellipse((13, 11, 19, 17), fill=acc)                        # อัญมณีอก
    d.point((14, 12), fill=(255, 255, 255, 255))
    d.line((10, 7, 10, 18), fill=hi); d.line((22, 7, 22, 18), fill=hi)


def boots(d, m, acc):
    dk, md, hi = m
    for x0 in (4, 17):
        d.rectangle((x0 + 2, 5, x0 + 8, 22), fill=md)
        d.polygon([(x0 + 2, 22), (x0 + 11, 22), (x0 + 11, 27), (x0, 27), (x0, 24)], fill=md)
        d.rectangle((x0 + 2, 5, x0 + 8, 7), fill=hi)
        d.rectangle((x0, 26, x0 + 11, 28), fill=dk)
        d.point((x0 + 5, 13), fill=acc); d.point((x0 + 5, 14), fill=acc)


def ring(d, m, acc):
    dk, md, hi = m
    d.ellipse((6, 10, 26, 30), outline=md, width=4)
    d.arc((7, 11, 25, 29), 200, 300, fill=hi, width=1)
    d.polygon([(16, 2), (22, 9), (16, 15), (10, 9)], fill=acc)
    d.polygon([(16, 4), (19, 9), (16, 9)], fill=(255, 255, 255, 220))


def amulet(d, m, acc):
    dk, md, hi = m
    d.arc((6, 1, 26, 21), 20, 160, fill=md, width=2)
    d.ellipse((9, 12, 23, 26), fill=md)
    d.ellipse((11, 14, 21, 24), fill=acc)
    d.ellipse((13, 16, 16, 19), fill=(255, 255, 255, 230))
    d.polygon([(16, 25), (18, 30), (14, 30)], fill=hi)


def w_sword(d, m, acc):
    dk, md, hi = m
    diag_blade(d, 9, 23, 27, 4, 4, hi, (255, 255, 255, 255), md)
    for i in range(4, 15, 4):
        d.point((9 + i, 23 - i), fill=acc)
    d.polygon([(4, 22), (14, 28), (15, 26), (6, 20)], fill=md)
    d.line([(9, 24), (4, 29)], fill=dk, width=3)
    d.ellipse((1, 28, 5, 31), fill=acc)


def w_staff(d, m, acc):
    dk, md, hi = m
    d.line([(5, 30), (19, 12)], fill=md, width=2)
    d.ellipse((15, 3, 29, 17), fill=dk)
    d.ellipse((17, 5, 27, 15), fill=acc)
    d.ellipse((19, 7, 22, 10), fill=(255, 255, 255, 230))
    d.arc((14, 2, 30, 18), 200, 340, fill=hi, width=1)
    d.polygon([(14, 15), (18, 13), (20, 17), (16, 18)], fill=hi)


def w_spear(d, m, acc):
    dk, md, hi = m
    d.line([(4, 29), (21, 12)], fill=md, width=2)
    d.polygon([(19, 10), (29, 2), (23, 14)], fill=hi)
    d.polygon([(21, 10), (28, 3), (22, 12)], fill=(255, 255, 255, 255))
    d.polygon([(17, 12), (22, 9), (24, 15), (20, 15)], fill=acc)


def w_bow(d, m, acc):
    dk, md, hi = m
    d.arc((4, 2, 26, 30), 300, 60, fill=md, width=3)
    d.arc((6, 4, 24, 28), 300, 60, fill=hi, width=1)
    d.line([(19, 5), (19, 27)], fill=acc)
    d.line([(4, 16), (27, 16)], fill=(220, 220, 230, 255))
    d.polygon([(28, 16), (24, 13), (24, 19)], fill=acc)


def w_hammer(d, m, acc):
    dk, md, hi = m
    d.line([(5, 30), (15, 15)], fill=dk, width=3)
    d.polygon([(10, 6), (22, 2), (29, 13), (17, 18)], fill=md)
    d.polygon([(12, 7), (21, 4), (27, 12), (18, 16)], fill=hi)
    d.ellipse((16, 7, 22, 13), fill=acc)


def w_axe(d, m, acc):
    dk, md, hi = m
    d.line([(6, 30), (19, 5)], fill=dk, width=2)
    d.pieslice((8, 2, 30, 24), 260, 40, fill=md)
    d.pieslice((11, 5, 27, 21), 260, 40, fill=hi)
    d.ellipse((15, 9, 19, 13), fill=acc)


WEAPONS = {"sword": w_sword, "staff": w_staff, "spear": w_spear, "bow": w_bow, "hammer": w_hammer, "axe": w_axe}
PIECES = {"helmet": helmet, "armor": armor, "boots": boots, "ring": ring, "amulet": amulet}


def draw(fn, metal, accent, glow_col=None, star=False):
    im, d = canvas(32)
    fn(d, metal, accent)
    if glow_col:
        im = glow(im, glow_col, 100)
    im = outline(im)
    if star:
        sparkle(ImageDraw.Draw(im), 27, 4, (255, 255, 255, 255), True)
    return im


# ---------------- ตราธาตุ/วัสดุ (16px) ----------------
def emblem(color, kind):
    im, d = canvas(16)
    c = C(color)
    d.polygon([(8, 0), (15, 4), (15, 11), (8, 15), (1, 11), (1, 4)], fill=(30, 26, 44, 255))
    if kind == "fire":
        d.polygon([(8, 3), (12, 9), (10, 13), (6, 13), (4, 9)], fill=c); d.polygon([(8, 7), (10, 11), (6, 11)], fill=C("#ffe0a0"))
    elif kind == "ice":
        for a in range(0, 180, 60):
            r = math.radians(a); d.line((8 - 5 * math.cos(r), 8 - 5 * math.sin(r), 8 + 5 * math.cos(r), 8 + 5 * math.sin(r)), fill=c)
    elif kind == "thunder":
        d.polygon([(9, 2), (5, 9), (8, 9), (6, 14), (11, 7), (8, 7)], fill=c)
    elif kind == "wind":
        d.arc((3, 3, 13, 11), 180, 360, fill=c); d.arc((5, 6, 13, 13), 0, 180, fill=c); d.line((3, 7, 7, 7), fill=c)
    elif kind == "earth":
        d.polygon([(3, 12), (6, 5), (9, 9), (11, 4), (13, 12)], fill=c)
    elif kind == "light":
        d.ellipse((5, 5, 11, 11), fill=c)
        for a in range(0, 360, 45):
            r = math.radians(a); d.point((8 + 6 * math.cos(r), 8 + 6 * math.sin(r)), fill=c)
    elif kind == "dark":
        d.ellipse((4, 4, 12, 12), fill=c); d.ellipse((6, 3, 13, 10), fill=(30, 26, 44, 255))
    elif kind == "star":
        pts = []
        for i in range(10):
            r = 5.5 if i % 2 == 0 else 2.3; a = math.radians(-90 + i * 36)
            pts.append((8 + r * math.cos(a), 8 + r * math.sin(a)))
        d.polygon(pts, fill=c)
    else:  # วัสดุ: โล่เล็กสีโลหะ
        d.polygon([(4, 3), (12, 3), (12, 8), (8, 13), (4, 8)], fill=c)
        d.line((8, 4, 8, 11), fill=(255, 255, 255, 160))
    return outline(im)


# ---------------- ไอคอนอาชีพใหม่ / คนธรรมดา (16px, ย่อจาก 32) ----------------
def small(fn, metal, acc):
    return draw(fn, pal(metal), C(acc)).resize((16, 16), Image.LANCZOS).point(lambda v: v)


def class_icons():
    out = {
        "c_lancer": draw(w_spear, pal(("#2a3a6a", "#6a7ab8", "#d0d8ff")), C("#ffd76a")),
        "c_berserker": draw(w_axe, pal(("#4a1010", "#a02828", "#ff8a6a")), C("#ff3a3a")),
        "c_ranger": draw(w_bow, pal(("#2a4a20", "#5a8a3a", "#c0e8a0")), C("#9affc8")),
        "c_paladin": draw(w_hammer, pal(("#6a5a20", "#e0c060", "#fff6d0")), C("#ffffff")),
        "c_spellblade": draw(w_sword, pal(("#3a1a6a", "#7a5ad0", "#e0d0ff")), C("#7fd8ff")),
        "c_bard": draw(amulet, pal(("#6a3a1a", "#c08040", "#ffe0a0")), C("#ff9ad8")),
    }
    return {k: v.resize((16, 16), Image.NEAREST) for k, v in out.items()}


def prof_icons():
    P = {}
    def mk(fn, metal, acc):
        return draw(fn, pal(metal), C(acc)).resize((16, 16), Image.NEAREST)
    # ชาวนา: เคียวข้าว / พราน: ธนู / ช่างไม้: ขวาน / คนงานเหมือง: ค้อน / ช่างก่อสร้าง: หมวก
    P["pr_farmer"] = mk(w_axe, ("#5a4a1a", "#e0c060", "#fff3b0"), "#9ad85a")
    P["pr_hunter"] = mk(w_bow, ("#4a2c18", "#8a5a34", "#c89a6a"), "#c8964a")
    P["pr_woodcutter"] = mk(w_axe, ("#3a4048", "#7a8696", "#d0d8e2"), "#8a5a34")
    P["pr_miner"] = mk(w_hammer, ("#3a3a3a", "#808080", "#c0c0c0"), "#8fd8ff")
    P["pr_builder"] = mk(helmet, ("#8a6a10", "#e0b020", "#fff090"), "#ff8a3a")
    P["pr_merchant"] = mk(ring, ("#6a5a20", "#e0c060", "#fff6d0"), "#ffd76a")
    P["pr_scholar"] = mk(amulet, ("#2a3a6a", "#4a6ac0", "#c0d0ff"), "#ffffff")
    P["pr_smith"] = mk(w_hammer, ("#3a2a20", "#6a5040", "#a08070"), "#ff6a3a")
    P["pr_healer"] = mk(amulet, ("#2a6a4a", "#5ab88a", "#d8ffe8"), "#ff6a8a")
    P["pr_noble"] = mk(ring, ("#4a1a5a", "#9a5ac0", "#e0c0ff"), "#ffd76a")
    return P


if __name__ == "__main__":
    for d_ in (ICON_DIR, HAND_DIR, NGD_DIR):
        os.makedirs(d_, exist_ok=True)
    tiles = []
    for el, (acc, metal) in ELEMENTS.items():
        m = pal(metal); a = C(acc); g = C(acc)
        for piece, fn in PIECES.items():
            im = draw(fn, m, a, g, star=(el == "star"))
            im.save(os.path.join(ICON_DIR, "icon_ngd_set_%s_%s.png" % (el, piece)))
            tiles.append(im)
        wfn = WEAPONS[WEAPON_KIND[el]]
        im = draw(wfn, m, a, g, star=(el == "star"))
        im.save(os.path.join(ICON_DIR, "icon_ngd_set_%s_weapon.png" % el))
        im.resize((16, 16), Image.NEAREST).save(os.path.join(HAND_DIR, "w_ngd_set_%s_weapon.png" % el))
        tiles.append(im)
        emblem(acc, el).save(os.path.join(NGD_DIR, "e_%s.png" % el))
    for mat, metal in MATERIALS.items():
        m = pal(metal)
        for piece in ("helmet", "armor", "boots"):
            im = draw(PIECES[piece], m, C(metal[2]), None)
            im.save(os.path.join(ICON_DIR, "icon_ngd_mat_%s_%s.png" % (mat, piece)))
            tiles.append(im)
        emblem(metal[1], "material").save(os.path.join(NGD_DIR, "m_%s.png" % mat))
    for k, im in {**class_icons(), **prof_icons()}.items():
        im.save(os.path.join(NGD_DIR, k + ".png"))
    cols = 12
    rows = (len(tiles) + cols - 1) // cols
    sheet = Image.new("RGBA", (cols * 70 + 6, rows * 70 + 6), (52, 60, 82, 255))
    for i, t in enumerate(tiles):
        big = t.resize((64, 64), Image.NEAREST)
        sheet.paste(big, (6 + (i % cols) * 70, 6 + (i // cols) * 70), big)
    sheet.save(os.path.join(ROOT, "build", "gear_preview.png"))
    print("ok", len(tiles))
