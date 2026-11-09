# อาคาร 6 ยุค × 10 ประเภท + อาคารพิเศษ → GameResources/buildings/ngd/<era>_<type>/{main_0,construction_0,ruin_0,mini_0}.png
# ใช้: python art3_buildings.py [preview out.png]
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from PIL import Image
from art3_build_core import *  # noqa
from art2_core import sheet
from art2_build import construction, ruin, mini

ROOT = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(ROOT, "NgaodaraPack", "GameResources", "buildings", "ngd")

ERAS = {
    "stone": dict(wall="#8a6a48", wt="hide", roof="KC", rt="thatch", trim="#5a3a20", glass="#ffb050", door="#4a2c18",
                  stone="#7a7060", accent="#e07a3a"),
    "bronze": dict(wall="#d4a670", wt="plaster", roof="KC", rt="plaster", trim="#b07a3a", glass="#ffcf6a", door="#6a4426",
                   stone="#a08a6a", accent="#3aa0a0"),
    "iron": dict(wall="#e2d6b8", wt="plaster", roof="KC", rt="shingle", trim="#5a3a24", glass="#ffd86a", door="#6a4426",
                 stone="#7a7468", accent="#4a6a9a"),
    "kingdom": dict(wall="#a8aab2", wt="stone", roof="KC", rt="tile", trim="#6a6a74", glass="#ffe08a", door="#6a4426",
                    stone="#6a6a70", accent="#e6b440"),
    "magic": dict(wall="#80789a", wt="stone", roof="KC", rt="slate", trim="#5a4a8a", glass="#7af0ff", door="#2a1a3a",
                  stone="#5a5470", accent="#7af0ff"),
    "star": dict(wall="#eceaf4", wt="marble", roof="#e6b440", rt="gold", trim="#b8b0d8", glass="#9ad8ff", door="#3a3a6a",
                 stone="#c8c4d8", accent="#8fb4ff"),
}


def base_y(b): return b.h - 3


def props(b, E, era, x0, x1, base):
    if era == "stone":
        b.put(x1 + 2, base, (120, 100, 80, 255)); torch(b, x0 - 2, base - 2)
    elif era in ("bronze", "iron"):
        barrel(b, x1 + 1, base - 3); crate(b, x0 - 4, base - 2)
    elif era == "kingdom":
        flowers(b, x0 + 1, base + 1); barrel(b, x1 + 1, base - 3)
    elif era == "magic":
        crystal(b, x0 - 2, base, E["accent"], 5); crystal(b, x1 + 2, base, E["accent"], 3)
    elif era == "star":
        bush(b, x0 - 4, base - 2, "#5aa07a"); crystal(b, x1 + 2, base, "#fff2a0", 4)


# ======================================================================= บ้าน
def house(b, E, era, big):
    base = base_y(b); W = b.w
    if era == "stone":
        cx = W // 2; r = 9 if big else 7
        m, d = b.mask(); d.ellipse((cx - r - 1, base - r - 2, cx + r + 1, base + 1), fill=255); b.fill(m, R(E["wall"]), tex(b, "hide"))
        roof_cone(b, cx, base - r + 1, r + 2, 12 if big else 10, E["roof"])
        door(b, cx, base, 3, 4, "#2a1a10", "#5a3a20", False)
        for dx in (-2, 0, 2): b.put(cx + dx, base - r - 11 + (2 if not big else 0) - abs(dx), (90, 60, 40, 255))  # ไม้ไขว้ยอด
        if big: flag(b, cx + r + 2, base - 12, 7)
        props(b, E, era, cx - r, cx + r, base); return
    x0, x1 = (3, W - 4) if big else (4, W - 5)
    wh = 9 if big else 7
    top = base - wh
    wall(b, x0, top, x1, base, E["wall"], E["wt"], E["trim"] if era == "iron" else None)
    foundation(b, x0, x1, base, E["stone"])
    cx = (x0 + x1) // 2
    if era == "bronze":
        roof_flat(b, x0, x1, top, 6 if big else 5, E["roof"])
        for x in range(x0 + 1, x1, 3): b.put(x, top + 1, R(E["trim"])[1])  # ปลายคาน
        m, d = b.mask(); d.polygon([(x0 + 2, top + 2), (x0 + 9, top + 2), (x0 + 10, top + 5), (x0 + 1, top + 5)], fill=255)
        b.fill(m, R(E["accent"]), None, outline=True)  # ผ้าใบกันแดด
    elif era == "star":
        roof_flat(b, x0, x1, top, 3, E["wall"], parapet=False)
        dome(b, cx, top - 2, (x1 - x0) // 2 - 3, E["roof"])
    elif era == "magic":
        roof_gable(b, x0, x1, top, 12 if big else 10, E["roof"], "slate")
        tx = x1 - 4
        m, d = b.mask(); d.rectangle((tx, top - 13, tx + 4, top + 2), fill=255); b.fill(m, R(E["wall"]), tex(b, "stone"))
        roof_cone(b, tx + 2, top - 13, 4, 9, E["roof"], "slate")
        window(b, tx + 1, top - 9, 2, 2, E["glass"], E["trim"], sill=False)
    else:
        roof_gable(b, x0, x1, top, 13 if big else 10, E["roof"], E["rt"])
        chimney(b, x1 - 5, top - (15 if big else 12), "#8a5a48" if era == "iron" else "#7a7a84")
    door(b, cx, base - 1, 3, 5 if big else 4, E["door"], E["trim"] if era != "kingdom" else "#5a5a64")
    wins = (x0 + 2, x1 - 3) if not big else (x0 + 2, x0 + 6, x1 - 7, x1 - 3)
    for wx in wins: window(b, wx, top + 2, 2, 3 if big else 2, E["glass"], E["trim"], arch=era in ("kingdom", "magic", "star"))
    if big: banner_wall(b, cx - 1, top + 1, 3) if era in ("kingdom", "star") else flag(b, x0, top - 14, 6)
    props(b, E, era, x0, x1, base)


# ======================================================================= ศาลากลาง
def hall(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":   # ลานเผ่า: กระท่อมใหญ่ + โทเท็ม + กองไฟ
        m, d = b.mask(); d.ellipse((cx - 13, base - 12, cx + 13, base + 1), fill=255); b.fill(m, R(E["wall"]), tex(b, "hide"))
        roof_cone(b, cx, base - 9, 15, 16, E["roof"])
        door(b, cx, base, 4, 5, "#2a1a10", "#5a3a20", False)
        for tx in (cx - 16, cx + 15):
            m, d = b.mask(); d.rectangle((tx, base - 14, tx + 2, base), fill=255); b.fill(m, R("#8a5a30"), tex(b, "wood"))
            b.put(tx + 1, base - 12, (230, 80, 60, 255)); b.put(tx + 1, base - 9, (240, 220, 90, 255)); b.put(tx, base - 15, (240, 240, 230, 255)); b.put(tx + 2, base - 15, (240, 240, 230, 255))
        flag(b, cx, base - 28, 6, True); return
    x0, x1 = 4, W - 5; wh = 12; top = base - wh
    wall(b, x0, top, x1, base, E["wall"], E["wt"], E["trim"] if era == "iron" else None)
    foundation(b, x0, x1, base, E["stone"])
    if era == "bronze":   # วังอิฐดิน 2 ชั้นแบบขั้นบันได
        roof_flat(b, x0, x1, top, 4, E["roof"])
        m, d = b.mask(); d.rectangle((x0 + 6, top - 9, x1 - 6, top - 3), fill=255); b.fill(m, R(E["wall"]), tex(b, "plaster"))
        roof_flat(b, x0 + 6, x1 - 6, top - 9, 4, E["roof"])
        for wx in range(x0 + 8, x1 - 7, 4): window(b, wx, top - 7, 2, 2, E["glass"], E["trim"], sill=False)
    elif era == "kingdom":   # ปราสาท: เชิงเทิน + หอคอยสองข้าง
        battlements(b, x0, x1, top, E["wall"])
        for tx in (x0 - 3, x1 - 3):
            m, d = b.mask(); d.rectangle((tx, top - 12, tx + 6, base), fill=255); b.fill(m, R(E["wall"]), tex(b, "stone"))
            roof_cone(b, tx + 3, top - 12, 5, 10, E["roof"], "tile")
            window(b, tx + 2, top - 7, 2, 3, E["glass"], E["trim"], arch=True, sill=False)
        flag(b, cx, top - 12, 8, True)
    elif era == "magic":
        roof_gable(b, x0, x1, top, 12, E["roof"], "slate")
        m, d = b.mask(); d.rectangle((cx - 4, top - 20, cx + 4, top), fill=255); b.fill(m, R(E["wall"]), tex(b, "stone"))
        roof_cone(b, cx, top - 20, 6, 12, E["roof"], "slate")
        crystal(b, cx, top - 33, E["accent"], 5)
        window(b, cx - 1, top - 15, 3, 4, E["glass"], E["trim"], arch=True, sill=False)
    elif era == "star":
        roof_flat(b, x0, x1, top, 3, E["wall"], parapet=False)
        dome(b, cx, top - 2, 11, E["roof"])
        for tx in (x0 + 1, x1 - 3):
            m, d = b.mask(); d.rectangle((tx, top - 12, tx + 2, top), fill=255); b.fill(m, R(E["wall"]), tex(b, "marble"))
            b.put(tx + 1, top - 13, (255, 240, 160, 255))
        m, d = b.mask(); d.ellipse((cx - 13, top - 17, cx + 13, top - 13), fill=255); d.ellipse((cx - 11, top - 16, cx + 11, top - 14), fill=0)
        b.fill(m, R(E["accent"], 0.7), None, outline=False)   # วงแหวนลอย
    else:   # iron: โถงไม้ยาว
        roof_gable(b, x0, x1, top, 14, E["roof"], E["rt"])
        chimney(b, x1 - 7, top - 16)
        flag(b, cx, top - 20, 7, True)
    door(b, cx, base - 1, 5, 6, E["door"], E["trim"], arch=era != "iron")
    for wx in (x0 + 3, x0 + 8, x1 - 9, x1 - 4):
        window(b, wx, top + 3, 2, 3, E["glass"], E["trim"], arch=era in ("kingdom", "magic", "star"))
    props(b, E, era, x0, x1, base)


# ======================================================================= วิหาร
def temple(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":   # วงหินศักดิ์สิทธิ์
        for i, a in enumerate(range(0, 360, 45)):
            x = cx + int(math.cos(math.radians(a)) * 11); y = base - 6 + int(math.sin(math.radians(a)) * 5)
            m, d = b.mask(); d.rectangle((x - 1, y - 7, x + 1, y), fill=255); b.fill(m, R("#8a8a90"), tex(b, "stone"))
        m, d = b.mask(); d.ellipse((cx - 2, base - 9, cx + 2, base - 5), fill=255); b.fill(m, R("#ff8a3a", 0.7), None)
        b.put(cx, base - 11, (255, 230, 140, 255)); return
    if era == "bronze":   # ซิกกูรัต
        for i, (w, h) in enumerate(((13, 6), (10, 6), (7, 6))):
            y1 = base - i * 6; y0 = y1 - h
            m, d = b.mask(); d.rectangle((cx - w, y0, cx + w, y1), fill=255); b.fill(m, R(E["wall"]), tex(b, "brick"))
        m, d = b.mask(); d.rectangle((cx - 2, base - 18, cx + 2, base), fill=255); b.fill(m, R(E["trim"]), tex(b, "wood"))  # บันได
        m, d = b.mask(); d.rectangle((cx - 4, base - 24, cx + 4, base - 18), fill=255); b.fill(m, R(E["accent"]), None)
        flag(b, cx, base - 30, 6); return
    x0, x1 = 6, W - 7; top = base - 13
    wall(b, x0, top, x1, base, E["wall"], E["wt"], E["trim"] if era == "iron" else None)
    foundation(b, x0, x1, base, E["stone"])
    if era == "star":
        roof_flat(b, x0, x1, top, 3, E["wall"], parapet=False); dome(b, cx, top - 2, 10, E["roof"])
        for x in range(x0 + 1, x1, 4):
            m, d = b.mask(); d.rectangle((x, top + 1, x + 1, base - 1), fill=255); b.fill(m, R("#ffffff"), None, outline=False)
        glow(b, E["accent"], 30)
    else:
        roof_gable(b, x0, x1, top, 10, E["roof"], E["rt"] if era != "magic" else "slate")
        m, d = b.mask(); d.rectangle((cx - 3, top - 18, cx + 3, top + 2), fill=255); b.fill(m, R(E["wall"]), tex(b, E["wt"]))
        roof_cone(b, cx, top - 18, 4, 12, E["roof"], "slate" if era == "magic" else "tile")
        b.put(cx, top - 31, (240, 210, 90, 255)); b.put(cx - 1, top - 30, (240, 210, 90, 255)); b.put(cx + 1, top - 30, (240, 210, 90, 255))
        m, d = b.mask(); d.ellipse((cx - 2, top - 12, cx + 2, top - 8), fill=255)   # หน้าต่างกุหลาบ
        b.fill(m, R("#c060e0" if era != "iron" else "#ffd86a", 0.6), None)
    door(b, cx, base - 1, 4, 6, E["door"], E["trim"], arch=True)
    for wx in (x0 + 2, x1 - 3): window(b, wx, top + 3, 2, 5, "#a070ff" if era in ("kingdom", "magic") else E["glass"], E["trim"], arch=True)
    props(b, E, era, x0, x1, base)


# ======================================================================= ค่ายทหาร
def barracks(b, E, era):
    base = base_y(b); W = b.w
    if era == "stone":
        for tx in (6, 18):
            m, d = b.mask(); d.polygon([(tx - 6, base), (tx, base - 11), (tx + 6, base)], fill=255); b.fill(m, R("#b89a70"), tex(b, "hide"))
            b.put(tx, base - 1, (40, 25, 15, 255)); b.put(tx, base - 2, (40, 25, 15, 255))
        for sx in range(W - 7, W - 1, 2): b.line([(sx, base), (sx, base - 9)], (110, 80, 50)); b.put(sx, base - 10, (200, 200, 210, 255))
        return
    x0, x1 = 3, W - 9; top = base - 8
    wall(b, x0, top, x1, base, E["wall"], E["wt"], E["trim"] if era == "iron" else None)
    foundation(b, x0, x1, base, E["stone"])
    if era == "bronze": roof_flat(b, x0, x1, top, 5, E["roof"])
    elif era == "star": roof_flat(b, x0, x1, top, 4, E["roof"], parapet=False)
    else: roof_gable(b, x0, x1, top, 10, E["roof"], E["rt"] if era != "magic" else "slate")
    if era == "kingdom": battlements(b, x0, x1, top - 10, E["wall"])
    door(b, (x0 + x1) // 2, base - 1, 4, 5, E["door"], E["trim"])
    for wx in range(x0 + 2, x1 - 2, 5):
        if abs(wx - (x0 + x1) // 2) > 3: window(b, wx, top + 2, 2, 2, E["glass"], E["trim"])
    # ลานฝึก: หุ่นซ้อม + ชั้นวางอาวุธ
    dx = W - 6
    b.line([(dx, base), (dx, base - 7)], (110, 80, 50)); b.line([(dx - 2, base - 5), (dx + 2, base - 5)], (110, 80, 50))
    m, d = b.mask(); d.ellipse((dx - 2, base - 10, dx + 2, base - 6), fill=255); b.fill(m, R("#d8b878"), None)
    for sx in (dx + 3, dx + 4): b.line([(sx, base), (sx, base - 8)], (150, 150, 160)); b.put(sx, base - 9, (230, 230, 240, 255))
    flag(b, x0 + 1, top - 15, 6)


# ======================================================================= ตลาด
def market(b, E, era):
    base = base_y(b); W = b.w
    stripes = {"stone": ("#c8a060", "#8a6a48"), "bronze": ("#e0b070", "#3aa0a0"), "iron": ("#d84a4a", "#f0e8d8"),
               "kingdom": ("#d84a4a", "#f0e8d8"), "magic": ("#5a3aa0", "#7af0ff"), "star": ("#ffffff", "#e6b440")}[era]
    for i, x0 in enumerate((2, 15)):
        x1 = x0 + 11
        m, d = b.mask(); d.rectangle((x0 + 1, base - 7, x1 - 1, base), fill=255); b.fill(m, R("#a07a4a"), tex(b, "wood"))
        m, d = b.mask(); d.polygon([(x0 - 1, base - 7), (x0 + 1, base - 13), (x1 - 1, base - 13), (x1 + 1, base - 7)], fill=255)
        b.fill(m, R(stripes[0]), lambda x, y: 0, outline=True)
        for x in range(x0, x1 + 1):
            if (x - x0) % 4 < 2:
                for y in range(base - 12, base - 7):
                    c = b.get(x, y)
                    if c[3] and c[:3] != R(stripes[0])[0][:3]: b.put(x, y, R(stripes[1])[2] if i == 0 else KC)
        goods = ((x0 + 2, "#ffcc40"), (x0 + 5, "#5ad05a"), (x0 + 8, "#e05a5a")) if era not in ("magic", "star") else ((x0 + 2, "#7af0ff"), (x0 + 5, "#c07aff"), (x0 + 8, "#ffe27a"))
        for gx, c in goods:
            m, d = b.mask(); d.ellipse((gx - 1, base - 6, gx + 1, base - 4), fill=255); b.fill(m, R(c), None, outline=False)
    barrel(b, W - 4, base - 3)


# ======================================================================= สถาบันวิจัย / หอสมุด
def academy(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":   # ผาภาพเขียน + กระท่อมผู้เฒ่า
        m, d = b.mask(); d.polygon([(2, base), (6, base - 16), (14, base - 19), (20, base - 12), (22, base)], fill=255); b.fill(m, R("#8a8070"), tex(b, "stone"))
        for (x, y, c) in ((8, base - 10, (200, 80, 50)), (11, base - 12, (200, 80, 50)), (14, base - 9, (240, 220, 200))): b.put(x, y, c + (255,)); b.put(x + 1, y + 1, c + (255,))
        m, d = b.mask(); d.ellipse((18, base - 8, 30, base + 1), fill=255); b.fill(m, R(E["wall"]), tex(b, "hide"))
        roof_cone(b, 24, base - 6, 7, 8, E["roof"]); return
    x0, x1 = 4, W - 5; top = base - 11
    wall(b, x0, top, x1, base, E["wall"], E["wt"], E["trim"] if era == "iron" else None)
    foundation(b, x0, x1, base, E["stone"])
    if era in ("kingdom", "magic", "star"):
        roof_flat(b, x0, x1, top, 3, E["wall"], parapet=False)
        dome(b, cx, top - 2, 9, "#4a70c0" if era == "kingdom" else E["roof"], "slate" if era != "star" else "gold")
        b.line([(cx + 3, top - 9), (cx + 9, top - 14)], (200, 180, 120))   # กล้องดูดาว
        b.put(cx + 9, top - 15, (230, 240, 255, 255))
    elif era == "bronze":
        roof_flat(b, x0, x1, top, 5, E["roof"])
        for i in range(3):   # แผ่นดินเหนียวจารึก
            m, d = b.mask(); d.rectangle((x1 + 1, base - 4 - i * 3, x1 + 3, base - 2 - i * 3), fill=255); b.fill(m, R("#c89a64"), None)
    else:
        roof_gable(b, x0, x1, top, 12, E["roof"], E["rt"])
    # ตราหนังสือ
    m, d = b.mask(); d.rectangle((cx - 3, top + 1, cx + 3, top + 5), fill=255); b.fill(m, R("#4a70d0"), None)
    b.line([(cx, top + 2), (cx, top + 4)], (240, 230, 200))
    door(b, cx, base - 1, 3, 4, E["door"], E["trim"], arch=True)
    for wx in (x0 + 2, x0 + 6, x1 - 7, x1 - 3): window(b, wx, top + 3, 2, 3, E["glass"], E["trim"], arch=era in ("kingdom", "magic", "star"))
    props(b, E, era, x0, x1, base)


# ======================================================================= กิลด์นักผจญภัย
def guild(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    x0, x1 = 5, W - 6; top = base - 13
    if era == "stone":
        m, d = b.mask(); d.ellipse((x0 - 1, base - 13, x1 + 1, base + 1), fill=255); b.fill(m, R(E["wall"]), tex(b, "hide"))
        roof_cone(b, cx, base - 10, (x1 - x0) // 2 + 2, 14, E["roof"])
    else:
        wall(b, x0, top, x1, base, E["wall"], E["wt"], E["trim"] if era in ("iron", "bronze") else None)
        foundation(b, x0, x1, base, E["stone"])
        if era == "bronze": roof_flat(b, x0, x1, top, 5, E["roof"])
        elif era == "star":
            roof_flat(b, x0, x1, top, 3, E["wall"], parapet=False); dome(b, cx, top - 2, 9, E["roof"])
        else:
            roof_gable(b, x0, x1, top, 13, E["roof"], "slate" if era == "magic" else E["rt"])
        if era in ("kingdom", "magic", "star"):
            for tx in (x0 - 4, x1):
                m, d = b.mask(); d.rectangle((tx, top - 8, tx + 4, base), fill=255); b.fill(m, R(E["wall"]), tex(b, E["wt"]))
                roof_cone(b, tx + 2, top - 8, 3, 9, E["roof"], "slate" if era == "magic" else ("gold" if era == "star" else "tile"))
                window(b, tx + 1, top - 3, 2, 3, E["glass"], E["trim"], arch=True, sill=False)
        for wx in (x0 + 3, x1 - 4): window(b, wx, top + 3, 2, 3, E["glass"], E["trim"])
    # ป้ายกิลด์ดาบไขว้ (ห้อยเหนือประตู) + กระดานภารกิจ
    sx, sy = cx, (top + 3) if era != "stone" else base - 13
    m, d = b.mask(); d.rectangle((sx - 4, sy, sx + 4, sy + 5), fill=255); b.fill(m, R("#7a5230"), tex(b, "wood"))
    b.line([(sx - 2, sy + 4), (sx + 2, sy + 1)], (235, 235, 245)); b.line([(sx + 2, sy + 4), (sx - 2, sy + 1)], (235, 235, 245))
    b.put(sx, sy + 2, (255, 210, 80, 255))
    door(b, cx, base - 1, 5, 5, E["door"], E["trim"], arch=era != "stone")
    bx = x1 + 1 if era == "stone" else x0 - 6
    m, d = b.mask(); d.rectangle((bx, base - 8, bx + 5, base - 3), fill=255); b.fill(m, R("#8a5a30"), tex(b, "wood"))
    for (px_, py_) in ((bx + 1, base - 7), (bx + 3, base - 6), (bx + 2, base - 5)): b.put(px_, py_, (240, 230, 200, 255))
    b.line([(bx + 1, base - 3), (bx + 1, base)], (90, 60, 40)); b.line([(bx + 4, base - 3), (bx + 4, base)], (90, 60, 40))
    flag(b, x0 + 1, (top if era != "stone" else base - 14) - 12, 7, True); flag(b, x1 - 4, (top if era != "stone" else base - 14) - 12, 7, True)
    torch(b, cx - 4, base - 4); torch(b, cx + 4, base - 4)


# ======================================================================= หอคอย
def tower(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era in ("stone", "iron"):   # หอไม้
        for x in (cx - 5, cx + 4): b.line([(x, base), (x + (2 if x < cx else -2), base - 22)], (110, 76, 46), 2)
        b.line([(cx - 4, base - 6), (cx + 3, base - 16)], (110, 76, 46)); b.line([(cx + 3, base - 6), (cx - 4, base - 16)], (110, 76, 46))
        m, d = b.mask(); d.rectangle((cx - 6, base - 27, cx + 6, base - 21), fill=255); b.fill(m, R("#9a6a3a"), tex(b, "wood"))
        roof_cone(b, cx, base - 27, 7, 7, E["roof"], "thatch" if era == "stone" else "shingle")
        flag(b, cx, base - 40, 5); return
    m, d = b.mask(); d.rectangle((cx - 5, base - 26, cx + 5, base), fill=255); b.fill(m, R(E["wall"]), tex(b, E["wt"] if era != "bronze" else "brick"))
    foundation(b, cx - 5, cx + 5, base, E["stone"])
    if era == "bronze": roof_flat(b, cx - 5, cx + 5, base - 26, 3, E["roof"]); battlements(b, cx - 5, cx + 5, base - 29, E["wall"])
    elif era == "kingdom": battlements(b, cx - 6, cx + 6, base - 26, E["wall"]); flag(b, cx, base - 36, 7, True)
    elif era == "magic":
        roof_cone(b, cx, base - 26, 7, 12, E["roof"], "slate"); crystal(b, cx, base - 39, E["accent"], 5); glow(b, E["accent"], 25)
    else:
        dome(b, cx, base - 26, 5, E["roof"]); b.line([(cx, base - 32), (cx, base - 38)], (255, 240, 160))
        m, d = b.mask(); d.ellipse((cx - 8, base - 30, cx + 8, base - 27), fill=255); d.ellipse((cx - 6, base - 29, cx + 6, base - 28), fill=0); b.fill(m, R(E["accent"], 0.7), None, outline=False)
    for wy in (base - 21, base - 13): window(b, cx - 1, wy, 2, 3, E["glass"], E["trim"], arch=True, sill=False)
    door(b, cx, base - 1, 3, 4, E["door"], E["trim"])


# ======================================================================= ฟาร์ม / กังหัน / ยุ้งฉาง
def farm(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    # แปลงพืช
    for y in range(base - 4, base + 1, 2):
        for x in range(2, W - 2):
            b.put(x, y, (110, 80, 50, 255))
            if (x + y) % 3: b.put(x, y - 1, (120, 170, 60, 255) if era != "magic" else (90, 200, 220, 255))
    if era == "stone":
        for x in (cx - 6, cx + 6): b.line([(x, base - 5), (x, base - 15)], (110, 76, 46))
        b.line([(cx - 6, base - 14), (cx + 6, base - 14)], (110, 76, 46))
        for x in range(cx - 5, cx + 6, 2): b.line([(x, base - 13), (x, base - 9)], (200, 170, 90))   # ราวตากธัญพืช
        return
    if era in ("bronze", "star"):   # ยุ้งฉางทรงกระบอก
        m, d = b.mask(); d.rectangle((cx - 6, base - 20, cx + 6, base - 5), fill=255); b.fill(m, R(E["wall"]), tex(b, E["wt"]))
        dome(b, cx, base - 20, 6, E["roof"], "gold" if era == "star" else "plaster")
        door(b, cx, base - 6, 3, 4, E["door"], E["trim"]); return
    # กังหันลม
    m, d = b.mask(); d.polygon([(cx - 5, base - 5), (cx - 3, base - 22), (cx + 3, base - 22), (cx + 5, base - 5)], fill=255)
    b.fill(m, R(E["wall"]), tex(b, "wood" if era == "iron" else E["wt"]))
    roof_cone(b, cx, base - 22, 5, 6, E["roof"], E["rt"] if era != "magic" else "slate")
    hx_, hy_ = cx, base - 19
    for a in (45, 135, 225, 315):
        ca, sa = math.cos(math.radians(a)), math.sin(math.radians(a))
        b.line([(hx_, hy_), (hx_ + ca * 11, hy_ + sa * 11)], (90, 64, 40))
        sail = (240, 232, 216) if era != "magic" else (122, 240, 255)
        for k in (1, 2):
            ox, oy = -sa * k, ca * k
            b.line([(hx_ + ca * 4 + ox, hy_ + sa * 4 + oy), (hx_ + ca * 10 + ox, hy_ + sa * 10 + oy)], sail)
    b.put(hx_, hy_, (60, 40, 30, 255))
    door(b, cx, base - 6, 3, 4, E["door"], E["trim"])




# ======================================================================= ส่วนร่วม: ผนัง+หลังคาตามยุค
def body(b, E, era, x0, x1, base, wh, rh=11, flat_depth=5, dome_r=None):
    top = base - wh
    wall(b, x0, top, x1, base, E["wall"], E["wt"], E["trim"] if era == "iron" else None)
    foundation(b, x0, x1, base, E["stone"])
    cx = (x0 + x1) // 2
    if era == "stone":
        roof_gable(b, x0, x1, top, rh, E["roof"], "thatch")
    elif era == "bronze":
        roof_flat(b, x0, x1, top, flat_depth, E["roof"])
        for x in range(x0 + 1, x1, 3): b.put(x, top + 1, R(E["trim"])[1])
    elif era == "star":
        roof_flat(b, x0, x1, top, 3, E["wall"], parapet=False)
        dome(b, cx, top - 2, dome_r or max(4, (x1 - x0) // 2 - 3), E["roof"])
    else:
        roof_gable(b, x0, x1, top, rh, E["roof"], "slate" if era == "magic" else E["rt"])
    return top, cx


def sign(b, x, y, kind, era):
    m, d = b.mask(); d.rectangle((x - 3, y, x + 3, y + 5), fill=255); b.fill(m, R("#7a5230"), tex(b, "wood"))
    b.line([(x - 4, y - 1), (x + 4, y - 1)], (90, 64, 40))
    if kind == "mug":
        m, d = b.mask(); d.rectangle((x - 2, y + 1, x + 1, y + 4), fill=255); b.fill(m, R("#e6b440"), None, outline=False); b.put(x + 2, y + 2, (230, 180, 60, 255)); b.put(x - 1, y + 1, (255, 255, 240, 255))
    elif kind == "coin":
        m, d = b.mask(); d.ellipse((x - 2, y + 1, x + 2, y + 5), fill=255); b.fill(m, R("#f2c040"), None, outline=False)
    elif kind == "book":
        m, d = b.mask(); d.rectangle((x - 2, y + 1, x + 2, y + 4), fill=255); b.fill(m, R("#4a70d0"), None, outline=False); b.line([(x, y + 1), (x, y + 4)], (240, 230, 200))
    elif kind == "cross":
        b.line([(x, y + 1), (x, y + 4)], (230, 60, 60)); b.line([(x - 1, y + 2), (x + 1, y + 2)], (230, 60, 60))
    elif kind == "anvil":
        b.line([(x - 2, y + 2), (x + 2, y + 2)], (170, 175, 190)); b.line([(x - 1, y + 3), (x + 1, y + 3)], (170, 175, 190)); b.put(x, y + 4, (170, 175, 190, 255))


def fence(b, x0, x1, y, col="#9a7046"):
    for x in range(x0, x1 + 1):
        b.put(x, y - 2, R(col)[2])
        if (x - x0) % 3 == 0: b.put(x, y - 1, R(col)[1]); b.put(x, y, R(col)[1]); b.put(x, y - 3, R(col)[3])


# ======================================================================= ปราสาทราชา
def castle(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    stoneish = era in ("kingdom", "magic", "star", "bronze")
    wallc = E["wall"] if stoneish else "#8a5a30"
    # ปราการด้านหลัง (keep)
    kx0, kx1, ktop = cx - 11, cx + 11, base - 30
    m, d = b.mask(); d.rectangle((kx0, ktop, kx1, base - 8), fill=255); b.fill(m, R(E["wall"] if era != "stone" else "#8a6a48"), tex(b, E["wt"] if stoneish else "wood"))
    if era == "stone":
        roof_cone(b, cx, ktop + 2, 14, 16, E["roof"])
    elif era == "bronze":
        roof_flat(b, kx0, kx1, ktop, 4, E["roof"])
        m, d = b.mask(); d.rectangle((cx - 6, ktop - 9, cx + 6, ktop - 3), fill=255); b.fill(m, R(E["wall"]), tex(b, "brick"))
        roof_flat(b, cx - 6, cx + 6, ktop - 9, 3, E["roof"])
        for x in range(kx0 + 2, kx1, 4):
            m, d = b.mask(); d.rectangle((x, ktop + 2, x + 1, base - 9), fill=255); b.fill(m, R("#f0e0c0"), None, outline=False)
    elif era == "iron":
        roof_gable(b, kx0, kx1, ktop, 13, E["roof"], "shingle")
    elif era == "kingdom":
        battlements(b, kx0, kx1, ktop, E["wall"])
        m, d = b.mask(); d.rectangle((cx - 4, ktop - 12, cx + 4, ktop), fill=255); b.fill(m, R(E["wall"]), tex(b, "stone"))
        roof_cone(b, cx, ktop - 12, 6, 12, E["roof"], "tile")
    elif era == "magic":
        roof_gable(b, kx0, kx1, ktop, 10, E["roof"], "slate")
        for sx, hh in ((cx - 6, 16), (cx, 24), (cx + 6, 16)):
            m, d = b.mask(); d.rectangle((sx - 2, ktop - hh, sx + 2, ktop), fill=255); b.fill(m, R(E["wall"]), tex(b, "stone"))
            roof_cone(b, sx, ktop - hh, 3, 9, E["roof"], "slate")
        crystal(b, cx, ktop - 35, E["accent"], 6)
    else:
        roof_flat(b, kx0, kx1, ktop, 3, E["wall"], parapet=False)
        dome(b, cx, ktop - 2, 10, E["roof"])
        for sx in (kx0 + 1, kx1 - 1):
            m, d = b.mask(); d.rectangle((sx - 1, ktop - 14, sx + 1, ktop), fill=255); b.fill(m, R(E["wall"]), tex(b, "marble")); b.put(sx, ktop - 15, (255, 240, 160, 255))
        m, d = b.mask(); d.ellipse((cx - 16, ktop - 18, cx + 16, ktop - 13), fill=255); d.ellipse((cx - 14, ktop - 17, cx + 14, ktop - 14), fill=0)
        b.fill(m, R(E["accent"], 0.7), None, outline=False)
    for wx in (kx0 + 3, kx1 - 4): window(b, wx, ktop + 4, 2, 3, E["glass"], E["trim"], arch=stoneish, sill=False)
    flag(b, cx, ktop - (40 if era == "magic" else 30 if era in ("kingdom", "star") else 24), 8, True)
    # กำแพงหน้า + หอมุม + ประตูใหญ่
    wtop = base - 11
    m, d = b.mask(); d.rectangle((3, wtop, W - 4, base), fill=255); b.fill(m, R(wallc), tex(b, "stone" if stoneish else "wood"))
    if stoneish: battlements(b, 3, W - 4, wtop, wallc)
    else:
        for x in range(3, W - 3, 2): b.put(x, wtop - 1, R(wallc)[3]); b.put(x, wtop - 2, R(wallc)[2])   # ปลายไม้แหลม
    for tx in (1, W - 9):
        m, d = b.mask(); d.rectangle((tx, wtop - 9, tx + 7, base), fill=255); b.fill(m, R(wallc), tex(b, "stone" if stoneish else "wood"))
        if era in ("kingdom", "magic"): roof_cone(b, tx + 3.5, wtop - 9, 5, 9, E["roof"], "slate" if era == "magic" else "tile")
        elif era == "star": dome(b, tx + 4, wtop - 9, 4, E["roof"])
        elif era == "bronze": battlements(b, tx, tx + 7, wtop - 9, wallc)
        else: roof_cone(b, tx + 3.5, wtop - 9, 5, 7, E["roof"], "thatch" if era == "stone" else "shingle")
        window(b, tx + 3, wtop - 5, 2, 2, E["glass"], E["trim"], sill=False)
    m, d = b.mask(); d.rectangle((cx - 4, base - 9, cx + 4, base), fill=255); d.ellipse((cx - 4, base - 12, cx + 4, base - 6), fill=255)
    b.fill(m, R("#2a1c14"), None)
    for x in range(cx - 3, cx + 4, 2):
        for y in range(base - 9, base + 1): b.put(x, y, (110, 110, 120, 255))
    banner_wall(b, cx - 9, wtop + 2, 5); banner_wall(b, cx + 7, wtop + 2, 5)
    torch(b, cx - 6, base - 5); torch(b, cx + 6, base - 5)
    if era == "stone":
        for x in (6, W - 7): b.put(x, wtop - 4, (240, 240, 230, 255)); b.put(x, wtop - 3, (240, 240, 230, 255))  # หัวกะโหลกบนรั้ว


# ======================================================================= คฤหาสน์ขุนนาง / บ้าน 2 ชั้น
def mansion(b, E, era):
    base = base_y(b) - 2; W = b.w
    x0, x1 = 4, W - 5
    top, cx = body(b, E, era, x0, x1, base, 13, 13)
    if era not in ("bronze", "star", "stone"):
        m, d = b.mask(); d.polygon([(cx - 6, top + 1), (cx, top - 9), (cx + 6, top + 1)], fill=255); b.fill(m, R(E["roof"]), tex(b, "slate" if era == "magic" else E["rt"]))
        window(b, cx - 1, top - 4, 2, 2, E["glass"], E["trim"], arch=True, sill=False)
    door(b, cx, base - 1, 3, 5, E["door"], E["trim"], arch=True)
    for wx in (x0 + 2, x0 + 6, x1 - 7, x1 - 3):
        window(b, wx, top + 2, 2, 3, E["glass"], E["trim"], arch=era in ("kingdom", "magic", "star"))
        window(b, wx, top + 8, 2, 2, E["glass"], E["trim"], sill=False)
    flowers(b, x0, base + 1, 3); flowers(b, x1 - 4, base + 1, 3)
    fence(b, 1, W - 2, base + 3, "#e8e0d0" if era in ("kingdom", "star") else "#9a7046")
    banner_wall(b, cx - 1, top + 7, 3)


def house2(b, E, era):
    base = base_y(b); W = b.w
    x0, x1 = 5, W - 6
    top, cx = body(b, E, era, x0, x1, base, 13, 11, 5, 6)
    door(b, cx, base - 1, 3, 4, E["door"], E["trim"])
    for wx in (x0 + 2, x1 - 3):
        window(b, wx, top + 2, 2, 2, E["glass"], E["trim"]); window(b, wx, top + 7, 2, 2, E["glass"], E["trim"])
    if era not in ("star", "bronze"): chimney(b, x0 + 2, top - 12)
    m, d = b.mask(); d.rectangle((x0 - 1, top + 6, x1 + 1, top + 6), fill=255); b.fill(m, R(E["trim"]), None, outline=False)
    props(b, E, era, x0, x1, base)


# ======================================================================= ร้านกลางแจ้ง
def stall(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    stripes = {"stone": "#c8a060", "bronze": "#3aa0a0", "iron": "#d84a4a", "kingdom": "#3a6ad0", "magic": "#7a3ad0", "star": "#e6b440"}[era]
    for x in (cx - 7, cx + 6): b.line([(x, base), (x, base - 11)], (110, 76, 46))
    m, d = b.mask(); d.rectangle((cx - 8, base - 5, cx + 7, base), fill=255); b.fill(m, R("#a07a4a"), tex(b, "wood"))
    m, d = b.mask(); d.polygon([(cx - 10, base - 10), (cx - 7, base - 15), (cx + 6, base - 15), (cx + 9, base - 10)], fill=255); b.fill(m, R(stripes), None)
    for x in range(cx - 9, cx + 9):
        if (x - cx) % 4 < 2:
            for y in range(base - 14, base - 10):
                if b.get(x, y)[3] and b.get(x, y)[:3] != R(stripes)[0][:3]: b.put(x, y, (245, 240, 230, 255))
    for i, c in enumerate(("#e05a5a", "#ffcc40", "#5ad05a", "#e08a3a", "#c07aff")):
        m, d = b.mask(); d.ellipse((cx - 7 + i * 3, base - 7, cx - 5 + i * 3, base - 5), fill=255); b.fill(m, R(c), None, outline=False)
    crate(b, cx + 8, base - 3)


# ======================================================================= ยุ้งข้าว
def granary(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era in ("stone", "iron"):   # ยุ้งไม้ยกพื้น
        for x in (cx - 7, cx - 2, cx + 3, cx + 7): b.line([(x, base), (x, base - 6)], (100, 70, 40))
        m, d = b.mask(); d.rectangle((cx - 8, base - 15, cx + 8, base - 6), fill=255); b.fill(m, R("#9a6a3a"), tex(b, "wood"))
        roof_gable(b, cx - 8, cx + 8, base - 15, 9, E["roof"], "thatch" if era == "stone" else "shingle")
        b.line([(cx + 9, base), (cx + 4, base - 7)], (120, 90, 50))   # บันไดพาด
        for y in range(base - 6, base, 2): b.put(cx + 7 - (base - y) // 2, y, (140, 100, 60, 255))
        for i in range(3): b.put(cx - 9 + i * 2, base, (220, 190, 90, 255))   # กระสอบข้าว
        return
    for i, ox in enumerate((-6, 6)):
        m, d = b.mask(); d.rectangle((cx + ox - 5, base - 18, cx + ox + 5, base), fill=255); b.fill(m, R(E["wall"]), tex(b, E["wt"] if era != "bronze" else "plaster"))
        if era == "kingdom": roof_cone(b, cx + ox, base - 18, 6, 7, E["roof"], "tile")
        else: dome(b, cx + ox, base - 18, 5, E["roof"], "gold" if era == "star" else ("slate" if era == "magic" else "plaster"))
        door(b, cx + ox, base - 1, 3, 4, E["door"], E["trim"], arch=False)
    if era == "magic": crystal(b, cx, base - 2, E["accent"], 5)


# ======================================================================= นาข้าว / แปลงผัก
def rice_field(b, E, era):
    base = base_y(b); W = b.w
    water = (90, 150, 190) if era != "magic" else (90, 200, 220)
    for y in range(base - 11, base + 1):
        for x in range(1, W - 1):
            edge = y in (base - 11, base) or x in (1, W - 2) or x == W // 2
            if edge: b.put(x, y, (120, 96, 60, 255)); continue
            b.put(x, y, water + (255,) if (x + y) % 5 else (150, 200, 230, 255))
            if (x % 3 == 0) and y % 3 == 1:
                b.put(x, y, (110, 180, 60, 255)); b.put(x, y - 1, (160, 210, 80, 255) if era != "star" else (230, 210, 90, 255))
    if era == "stone": return
    b.line([(3, base - 13), (3, base - 18)], (110, 76, 46)); b.put(3, base - 19, (220, 190, 90, 255)); b.put(2, base - 17, (90, 64, 40, 255)); b.put(4, base - 17, (90, 64, 40, 255))  # หุ่นไล่กา


def veg_plot(b, E, era):
    base = base_y(b); W = b.w
    crops = ((60, 160, 60), (230, 120, 40), (220, 170, 40), (170, 60, 160))
    for r, y in enumerate(range(base - 10, base + 1, 3)):
        for x in range(2, W - 2):
            b.put(x, y, (100, 72, 46, 255)); b.put(x, y + 1, (84, 60, 38, 255))
            if x % 3 == 1:
                c = crops[r % 4]
                b.put(x, y - 1, (70, 140, 50, 255)); b.put(x, y, c + (255,))
    fence(b, 1, W - 2, base + 2)
    if era in ("magic", "star"):
        crystal(b, W - 3, base - 11, E["accent"], 4)


# ======================================================================= ธนาคาร / โรงเรียน / โรงเตี๊ยม / โรงตีเหล็ก / บ้านหมอ
def bank(b, E, era):
    base = base_y(b); W = b.w
    x0, x1 = 4, W - 5
    if era == "stone":
        m, d = b.mask(); d.ellipse((x0, base - 12, x1, base + 1), fill=255); b.fill(m, R("#9a8a70"), tex(b, "stone"))
        roof_cone(b, W // 2, base - 9, 11, 9, E["roof"])
        for i in range(4): b.put(x0 + 3 + i * 3, base - 3, (250, 240, 220, 255))   # เปลือกหอยเงินตรา
        door(b, W // 2, base, 3, 4, "#2a1a10", "#5a3a20", False); return
    top = base - 11; cx = W // 2
    wall(b, x0, top, x1, base, E["wall"] if era != "iron" else "#c8c0b0", "stone" if era != "star" else "marble")
    foundation(b, x0, x1, base, E["stone"])
    # หน้าจั่วแบบวิหารกรีก + เสา
    m, d = b.mask(); d.polygon([(x0 - 2, top + 1), (cx, top - 8), (x1 + 2, top + 1)], fill=255)
    b.fill(m, R(E["roof"] if era in ("magic", "star") else "#e8e4d8"), tex(b, "gold" if era == "star" else "marble"))
    for x in range(x0 + 1, x1, 4):
        m, d = b.mask(); d.rectangle((x, top + 2, x + 1, base - 1), fill=255); b.fill(m, R("#f2eee4"), None, outline=False)
    sign(b, cx, top - 5, "coin", era)
    m, d = b.mask(); d.ellipse((cx - 3, base - 7, cx + 3, base - 1), fill=255); b.fill(m, R("#8a8a96" if era != "magic" else "#5a3aa0"), None)   # ประตูห้องนิรภัย
    b.put(cx, base - 4, (240, 210, 90, 255))
    if era == "magic": glow(b, E["accent"], 20)
    banner_wall(b, x0, top + 2, 4)


def school(b, E, era):
    base = base_y(b); W = b.w
    x0, x1 = 3, W - 9
    top, cx = body(b, E, era, x0, x1, base, 9, 10, 5, 6)
    # หอระฆัง
    tx = x1 + 1
    m, d = b.mask(); d.rectangle((tx, top - 10, tx + 5, base), fill=255); b.fill(m, R(E["wall"]), tex(b, E["wt"]))
    m, d = b.mask(); d.ellipse((tx + 1, top - 8, tx + 4, top - 5), fill=255); b.fill(m, R("#e6b440"), None)   # ระฆัง
    roof_cone(b, tx + 2.5, top - 10, 4, 7, E["roof"], "slate" if era == "magic" else ("gold" if era == "star" else E["rt"] if era != "bronze" else "plaster"))
    sign(b, cx, top + 1, "book", era)
    door(b, cx, base - 1, 3, 4, E["door"], E["trim"])
    for wx in (x0 + 2, x1 - 3): window(b, wx, top + 2, 2, 3, E["glass"], E["trim"])
    for i in range(3):   # ลานเรียน: กระดานชนวน
        b.put(tx + 1 + i, base + 1, (60, 70, 60, 255))


def tavern(b, E, era):
    base = base_y(b); W = b.w
    x0, x1 = 4, W - 7
    top, cx = body(b, E, era, x0, x1, base, 13, 12, 5, 7)
    m, d = b.mask(); d.rectangle((x0 - 1, top + 6, x1 + 1, top + 6), fill=255); b.fill(m, R(E["trim"]), None, outline=False)
    door(b, cx, base - 1, 4, 5, E["door"], E["trim"])
    for wx in (x0 + 2, x1 - 3):
        window(b, wx, top + 2, 2, 2, E["glass"], E["trim"]); window(b, wx, top + 8, 3, 3, E["glass"], E["trim"])
    b.line([(x1 + 1, top + 3), (x1 + 5, top + 3)], (90, 64, 40)); sign(b, x1 + 4, top + 4, "mug", era)
    barrel(b, x1 + 2, base - 3); barrel(b, x0 - 4, base - 3)
    if era not in ("bronze", "star"): chimney(b, x0 + 3, top - 13)
    torch(b, cx - 4, base - 5)


def forge(b, E, era):
    base = base_y(b); W = b.w
    x0, x1 = 3, W - 10
    top, cx = body(b, E, era, x0, x1, base, 9, 9, 5, 5)
    m, d = b.mask(); d.rectangle((x0 + 2, base - 6, x0 + 8, base - 1), fill=255); b.fill(m, R("#3a2a22"), None)
    m, d = b.mask(); d.ellipse((x0 + 3, base - 5, x0 + 7, base - 2), fill=255); b.fill(m, R("#ff8a3a", 0.6), None, outline=False)   # เตาไฟ
    chimney(b, x1 - 4, top - 12, "#6a5a50")
    m, d = b.mask(); d.rectangle((x1 + 1, base - 5, x1 + 8, base), fill=255); b.fill(m, R("#7a5a3a"), tex(b, "wood"))   # หลังคากันแดดหน้าร้าน
    m, d = b.mask(); d.polygon([(x1 + 3, base - 3), (x1 + 8, base - 3), (x1 + 6, base - 1), (x1 + 6, base), (x1 + 4, base), (x1 + 4, base - 1)], fill=255)
    b.fill(m, R("#9aa0b0"), None)   # ทั่ง
    for i in range(3): b.line([(x1 + 9, base - 1 - i * 3), (x1 + 9, base - 3 - i * 3)], (180, 185, 200))   # อาวุธวางพิง
    sign(b, cx + 3, top + 1, "anvil", era)


def healer(b, E, era):
    base = base_y(b); W = b.w
    x0, x1 = 4, W - 5
    top, cx = body(b, E, era, x0, x1, base, 9, 10, 5, 6)
    sign(b, cx, top + 1, "cross", era)
    door(b, cx, base - 1, 3, 4, E["door"], E["trim"])
    for wx in (x0 + 2, x1 - 3): window(b, wx, top + 2, 2, 3, E["glass"], E["trim"])
    for x in range(x0 - 2, x1 + 3, 3): b.put(x, base + 1, (90, 170, 80, 255)); b.put(x, base, (140, 210, 110, 255))   # แปลงสมุนไพร
    if era == "magic": crystal(b, x1 + 2, base, "#9aff9a", 4)


# ======================================================================= บ่อน้ำ / คอกม้า / ประตูเมือง
def well(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.ellipse((cx - 5, base - 5, cx + 5, base + 1), fill=255); b.fill(m, R(E["stone"]), tex(b, "stone"))
    m, d = b.mask(); d.ellipse((cx - 3, base - 4, cx + 3, base - 1), fill=255); b.fill(m, R("#3a6a9a" if era != "magic" else "#3aa0b0"), None, outline=False)
    for x in (cx - 4, cx + 4): b.line([(x, base - 3), (x, base - 10)], (110, 76, 46))
    roof_gable(b, cx - 4, cx + 4, base - 10, 4, E["roof"], "thatch" if era == "stone" else "shingle" if era == "iron" else "tile", over=1)


def stable(b, E, era):
    base = base_y(b); W = b.w
    x0, x1 = 3, W - 4
    top = base - 8
    m, d = b.mask(); d.rectangle((x0, top, x1, base), fill=255); b.fill(m, R("#8a5a30"), tex(b, "wood"))
    roof_gable(b, x0, x1, top, 8, E["roof"], "thatch" if era in ("stone", "bronze") else "slate" if era == "magic" else E["rt"])
    for sx in range(x0 + 2, x1 - 3, 7):
        m, d = b.mask(); d.rectangle((sx, base - 6, sx + 5, base), fill=255); b.fill(m, R("#3a2618"), None)
    for i, hx_ in enumerate((x0 + 3, x0 + 10)):   # ม้า
        c = ((140, 90, 50), (230, 225, 215))[i]
        for (dx, dy) in ((0, 0), (1, 0), (2, 0), (3, 0), (1, -1), (2, -1), (3, -1), (4, -2), (4, -1), (0, 1), (3, 1)):
            b.put(hx_ + dx, base - 3 + dy, c + (255,))
    for x in range(x1 - 6, x1): b.put(x, base + 1, (220, 190, 90, 255)); b.put(x + 1, base, (230, 205, 110, 255))   # กองฟาง
    fence(b, x1 + 1, W - 1, base + 1)


def gate(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    stoneish = era in ("kingdom", "magic", "star", "bronze")
    wallc = E["wall"] if stoneish else "#8a5a30"
    for tx in (2, W - 11):
        m, d = b.mask(); d.rectangle((tx, base - 22, tx + 8, base), fill=255); b.fill(m, R(wallc), tex(b, "stone" if stoneish else "wood"))
        if era in ("kingdom", "magic"): roof_cone(b, tx + 4, base - 22, 6, 10, E["roof"], "slate" if era == "magic" else "tile")
        elif era == "star": dome(b, tx + 4, base - 22, 5, E["roof"])
        elif stoneish: battlements(b, tx, tx + 8, base - 22, wallc)
        else: roof_cone(b, tx + 4, base - 22, 6, 8, E["roof"], "thatch" if era == "stone" else "shingle")
        window(b, tx + 3, base - 17, 2, 3, E["glass"], E["trim"], arch=stoneish, sill=False)
    m, d = b.mask(); d.rectangle((10, base - 16, W - 11, base), fill=255); b.fill(m, R(wallc), tex(b, "stone" if stoneish else "wood"))
    if stoneish: battlements(b, 10, W - 11, base - 16, wallc)
    m, d = b.mask(); d.rectangle((cx - 4, base - 10, cx + 4, base), fill=255); d.ellipse((cx - 4, base - 13, cx + 4, base - 7), fill=255); b.fill(m, R("#2a1c14"), None)
    for x in range(cx - 3, cx + 4, 2):
        for y in range(base - 10, base - 3): b.put(x, y, (110, 110, 120, 255))
    banner_wall(b, cx - 1, base - 15, 3)
    flag(b, 6, base - 33 if era in ("kingdom", "magic") else base - 28, 6); flag(b, W - 7, base - 33 if era in ("kingdom", "magic") else base - 28, 6)




# ======================================================================= ชุดที่ 3: บ้านหลากแบบ
def house3(b, E, era):   # บ้านมุมมีสวน
    base = base_y(b); W = b.w
    if era == "stone": return house(b, E, era, False) or bush(b, 1, base - 3) or flowers(b, W - 7, base + 1)
    x0, x1 = 3, W - 11
    top, cx = body(b, E, era, x0, x1, base, 8, 10, 5, 5)
    door(b, cx, base - 1, 3, 4, E["door"], E["trim"]); window(b, x0 + 2, top + 2, 2, 2, E["glass"], E["trim"])
    for x, y in ((W - 9, base - 4), (W - 5, base - 3)): bush(b, x, y, "#4a9a4a" if era != "magic" else "#4a8a9a")
    flowers(b, W - 9, base + 1, 4); fence(b, W - 10, W - 1, base + 2)


def shack(b, E, era):    # เพิงคนจน
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.polygon([(3, base), (3, base - 7), (W - 4, base - 9), (W - 4, base)], fill=255); b.fill(m, R("#8a6a48"), tex(b, "wood"))
    m, d = b.mask(); d.polygon([(1, base - 7), (W - 2, base - 10), (W - 2, base - 8), (1, base - 5)], fill=255)
    b.fill(m, R("#c8a050" if era in ("stone", "bronze", "iron") else "#7a6a5a"), tex(b, "thatch" if era in ("stone", "bronze", "iron") else "wood"))
    door(b, cx, base, 3, 4, "#3a2414", "#5a3a20", False)
    crate(b, W - 6, base - 2); b.put(4, base - 4, (150, 150, 160, 255))


def rowhouse(b, E, era):   # บ้านแถว 3 หลังติดกัน
    base = base_y(b); W = b.w
    cols = [E["wall"], R(E["wall"], 0.6)[3], R(E["wall"], 0.6)[1]]
    for i in range(3):
        x0 = 2 + i * 12; x1 = x0 + 11
        if era == "stone":
            m, d = b.mask(); d.ellipse((x0, base - 9, x1, base + 1), fill=255); b.fill(m, R(E["wall"]), tex(b, "hide"))
            roof_cone(b, (x0 + x1) // 2, base - 7, 7, 8, E["roof"]); door(b, (x0 + x1) // 2, base, 3, 3, "#2a1a10", "#5a3a20", False); continue
        c = cols[i] if isinstance(cols[i], str) else "#%02x%02x%02x" % cols[i][:3]
        top = base - 11
        wall(b, x0, top, x1, base, c, E["wt"], E["trim"] if era == "iron" else None)
        if era == "bronze": roof_flat(b, x0, x1, top, 4, E["roof"])
        elif era == "star": roof_flat(b, x0, x1, top, 3, E["roof"], parapet=False)
        else: roof_gable(b, x0, x1, top, 8, E["roof"], "slate" if era == "magic" else E["rt"], over=1)
        door(b, x0 + 3, base - 1, 2, 4, E["door"], E["trim"], arch=False)
        window(b, x0 + 6, top + 2, 2, 2, E["glass"], E["trim"]); window(b, x0 + 6, top + 6, 2, 2, E["glass"], E["trim"], sill=False)
    foundation(b, 2, W - 3, base, E["stone"])


# ======================================================================= อาชีพ/การผลิต
def shop_body(b, E, era, signkind, x1pad=8):
    base = base_y(b); W = b.w
    if era == "stone":
        house(b, E, era, False); return base, W // 2, base - 10
    x0, x1 = 3, W - x1pad
    top, cx = body(b, E, era, x0, x1, base, 9, 10, 5, 5)
    door(b, cx, base - 1, 3, 4, E["door"], E["trim"]); window(b, x0 + 2, top + 2, 2, 2, E["glass"], E["trim"])
    if signkind: sign(b, x1 - 3, top + 1, signkind, era)
    return base, cx, top


def bakery(b, E, era):
    base, cx, top = shop_body(b, E, era, None)
    W = b.w
    m, d = b.mask(); d.ellipse((W - 9, base - 7, W - 2, base + 1), fill=255); b.fill(m, R("#b89070"), tex(b, "brick"))   # เตาอบโดม
    m, d = b.mask(); d.ellipse((W - 7, base - 4, W - 4, base - 1), fill=255); b.fill(m, R("#ff9a3a", 0.6), None, outline=False)
    for i in range(3): b.put(cx - 2 + i * 2, base + 1, (220, 160, 80, 255))   # ขนมปังวางขาย
    b.put(W - 5, base - 9, (210, 210, 215, 170)); b.put(W - 4, base - 11, (210, 210, 215, 120))


def fishery(b, E, era):
    base = base_y(b); W = b.w
    for y in range(base - 2, base + 2):   # น้ำริมฝั่ง
        for x in range(0, W): b.put(x, y, (70, 130, 180, 255) if (x + y) % 4 else (120, 180, 220, 255))
    for x in range(W - 12, W - 1, 2): b.line([(x, base - 2), (x, base + 1)], (100, 70, 40))   # ท่าไม้
    b.line([(W - 13, base - 3), (W - 1, base - 3)], (140, 100, 60))
    m, d = b.mask(); d.rectangle((2, base - 11, 14, base - 3), fill=255); b.fill(m, R("#8a6a48"), tex(b, "wood"))
    roof_gable(b, 2, 14, base - 11, 6, E["roof"], "thatch" if era in ("stone", "bronze") else "slate" if era == "magic" else E["rt"], over=1)
    door(b, 8, base - 4, 2, 3, "#3a2414", "#5a3a20", False)
    b.line([(16, base - 10), (24, base - 10)], (110, 76, 46))   # ราวตากปลา
    for x in range(17, 24, 2): b.put(x, base - 9, (170, 190, 200, 255)); b.put(x, base - 8, (140, 160, 175, 255))
    b.line([(16, base - 10), (16, base - 3)], (110, 76, 46)); b.line([(24, base - 10), (24, base - 3)], (110, 76, 46))


def lumber_mill(b, E, era):
    base = base_y(b); W = b.w
    m, d = b.mask(); d.rectangle((2, base - 9, 16, base), fill=255); b.fill(m, R("#9a6a3a"), tex(b, "wood"))
    roof_gable(b, 2, 16, base - 9, 7, E["roof"], "thatch" if era in ("stone", "bronze") else "slate" if era == "magic" else E["rt"], over=1)
    for i, y in enumerate((base - 1, base - 3, base - 5)):   # กองซุง
        for k in range(3 - i):
            x = 19 + k * 4 + i * 2
            m, d = b.mask(); d.ellipse((x, y - 2, x + 3, y + 1), fill=255); b.fill(m, R("#b07a44"), None)
            b.put(x + 1, y - 1, (230, 190, 130, 255))
    m, d = b.mask(); d.ellipse((5, base - 7, 11, base - 1), fill=255); b.fill(m, R("#b0b4c0"), None)   # ใบเลื่อยวงกลม
    b.put(8, base - 4, (60, 60, 70, 255))


def mine(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.polygon([(1, base), (6, base - 15), (14, base - 20), (24, base - 16), (W - 2, base)], fill=255); b.fill(m, R("#8a8478"), tex(b, "stone"))
    m, d = b.mask(); d.rectangle((cx - 5, base - 9, cx + 5, base), fill=255); d.ellipse((cx - 5, base - 12, cx + 5, base - 6), fill=255); b.fill(m, R("#1a1410"), None)
    for x in (cx - 5, cx + 5): b.line([(x, base), (x, base - 10)], (130, 90, 50), 2)
    b.line([(cx - 6, base - 10), (cx + 6, base - 10)], (130, 90, 50), 2)
    m, d = b.mask(); d.rectangle((cx + 7, base - 4, cx + 12, base - 1), fill=255); b.fill(m, R("#6a6a74"), None)   # รถแร่
    for x in (cx + 8, cx + 10): b.put(x, base - 5, (230, 200, 90, 255) if era != "magic" else (120, 240, 255, 255))
    b.line([(0, base + 1), (W - 1, base + 1)], (110, 90, 70)); torch(b, cx - 7, base - 6)


def quarry(b, E, era):
    base = base_y(b); W = b.w
    m, d = b.mask(); d.polygon([(1, base), (3, base - 10), (W - 4, base - 12), (W - 2, base)], fill=255); b.fill(m, R("#a8a49a"), tex(b, "stone"))
    m, d = b.mask(); d.polygon([(6, base), (8, base - 6), (W - 9, base - 7), (W - 7, base)], fill=255); b.fill(m, R("#7a766c"), tex(b, "stone"))   # หลุมขุด
    for i, (x, y) in enumerate(((4, base - 3), (9, base - 2), (W - 10, base - 3))):
        m, d = b.mask(); d.rectangle((x, y, x + 3, y + 2), fill=255); b.fill(m, R("#d0ccc0"), None)
    b.line([(W - 6, base), (W - 6, base - 14)], (110, 76, 46)); b.line([(W - 6, base - 14), (W - 12, base - 8)], (110, 76, 46))   # ปั้นจั่นไม้
    b.put(W - 12, base - 7, (200, 200, 200, 255))


def tailor(b, E, era):
    base, cx, top = shop_body(b, E, era, None, 10)
    W = b.w
    b.line([(W - 9, base - 11), (W - 1, base - 11)], (110, 76, 46))
    for i, c in enumerate(((220, 70, 70), (70, 120, 220), (240, 200, 70), (255, 0, 255))):   # ผ้าตาก
        x = W - 9 + i * 2
        for y in range(base - 10, base - 5 - i % 2): b.put(x, y, c + (255,)); b.put(x + 1, y, c + (255,))
    b.line([(W - 9, base - 11), (W - 9, base)], (110, 76, 46)); b.line([(W - 1, base - 11), (W - 1, base)], (110, 76, 46))


def kiln(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.polygon([(cx - 7, base), (cx - 5, base - 12), (cx + 5, base - 12), (cx + 7, base)], fill=255); b.fill(m, R("#b07a5a"), tex(b, "brick"))
    m, d = b.mask(); d.ellipse((cx - 3, base - 5, cx + 3, base), fill=255); b.fill(m, R("#ff8a3a", 0.6), None)
    chimney(b, cx - 1, base - 18, "#8a5a48")
    for i, x in enumerate((2, 5, W - 7, W - 4)):   # หม้อไห
        m, d = b.mask(); d.ellipse((x, base - 4, x + 3, base), fill=255); b.fill(m, R(("#c86a3a", "#d8a060", "#8a5a3a", "#c86a3a")[i]), None)


def brewery(b, E, era):
    base, cx, top = shop_body(b, E, era, "mug", 12)
    W = b.w
    for i, x in enumerate((W - 11, W - 6)):   # ถังหมักใหญ่
        m, d = b.mask(); d.rectangle((x, base - 9, x + 4, base), fill=255); b.fill(m, R("#9a6a3a"), tex(b, "wood"))
        for y in (base - 7, base - 2): b.line([(x, y), (x + 4, y)], (90, 90, 100))
    if era in ("bronze", "kingdom", "star"):
        for x in range(1, 6): b.put(x, base, (90, 160, 70, 255)); b.put(x, base - 1, (120, 60, 140, 255) if x % 2 else (90, 160, 70, 255))   # เถาองุ่น


# ======================================================================= เกษตร
def orchard(b, E, era):
    base = base_y(b); W = b.w
    fruit = {"stone": (230, 80, 60), "bronze": (240, 170, 50), "iron": (220, 60, 60), "kingdom": (230, 60, 80), "magic": (120, 230, 255), "star": (255, 220, 90)}[era]
    for i, (x, y) in enumerate(((3, base - 2), (13, base - 4), (23, base - 2), (8, base + 1 - 9), (19, base - 10))):
        b.line([(x + 3, y), (x + 3, y - 4)], (110, 76, 46))
        m, d = b.mask(); d.ellipse((x, y - 10, x + 7, y - 3), fill=255); b.fill(m, R("#3e8a3e" if era != "magic" else "#3e7a8a"), tex(b, "plaster"))
        for k in range(3): b.put(x + 1 + k * 2, y - 7 + (k % 2) * 2, fruit + (255,))
    fence(b, 0, W - 1, base + 2)


def sheep_pen(b, E, era):
    base = base_y(b); W = b.w
    for x in range(1, W - 1): b.put(x, base + 1, (110, 160, 70, 255))
    fence(b, 1, W - 2, base + 1); fence(b, 1, W - 2, base - 9)
    for x in (1, W - 2): b.line([(x, base + 1), (x, base - 9)], (130, 96, 60))
    for (x, y) in ((5, base - 4), (12, base - 2), (19, base - 5), (24, base - 1)):   # แกะ
        m, d = b.mask(); d.ellipse((x, y - 2, x + 4, y + 1), fill=255); b.fill(m, R("#f0ece4"), None)
        b.put(x + 4, y - 1, (60, 50, 50, 255))


def chicken_coop(b, E, era):
    base = base_y(b); W = b.w
    m, d = b.mask(); d.rectangle((3, base - 7, 12, base - 2), fill=255); b.fill(m, R("#a07040"), tex(b, "wood"))
    roof_gable(b, 3, 12, base - 7, 4, E["roof"], "thatch" if era in ("stone", "bronze") else "shingle", over=1)
    for x in (4, 11): b.line([(x, base - 2), (x, base)], (110, 76, 46))
    for (x, y) in ((15, base - 1), (18, base), (14, base + 1)):   # ไก่
        b.put(x, y, (245, 240, 230, 255)); b.put(x + 1, y, (245, 240, 230, 255)); b.put(x + 1, y - 1, (230, 60, 50, 255)); b.put(x + 2, y, (240, 180, 50, 255))
    fence(b, 13, W - 1, base + 2)


# ======================================================================= ส่วนกลางเมือง
def fountain(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    stone = E["stone"] if era != "star" else "#e8e6f0"
    m, d = b.mask(); d.ellipse((2, base - 8, W - 3, base + 1), fill=255); b.fill(m, R(stone), tex(b, "stone" if era != "star" else "marble"))
    m, d = b.mask(); d.ellipse((4, base - 6, W - 5, base - 1), fill=255); b.fill(m, R("#4a8ac8" if era != "magic" else "#4ac8d8"), None, outline=False)
    m, d = b.mask(); d.rectangle((cx - 1, base - 13, cx + 1, base - 4), fill=255); b.fill(m, R(stone), None)
    for (dx, dy) in ((-2, -14), (2, -14), (-3, -12), (3, -12), (0, -15)): b.put(cx + dx, base + dy, (180, 220, 255, 220))
    if era in ("magic", "star"): crystal(b, cx, base - 15, E["accent"], 3)


def cemetery(b, E, era):
    base = base_y(b); W = b.w
    for x in range(1, W - 1): b.put(x, base + 1, (90, 120, 70, 255))
    for i, (x, y) in enumerate(((3, base - 2), (10, base - 4), (17, base - 2), (24, base - 4), (7, base + 0), (20, base))):
        if era == "stone":
            m, d = b.mask(); d.ellipse((x, y - 2, x + 4, y + 1), fill=255); b.fill(m, R("#8a8478"), None)
        else:
            m, d = b.mask(); d.rectangle((x, y - 5, x + 3, y), fill=255); d.ellipse((x, y - 7, x + 3, y - 3), fill=255); b.fill(m, R("#a0a0a8"), tex(b, "stone"))
            if i % 2 == 0: b.line([(x + 1, y - 4), (x + 2, y - 4)], (80, 80, 90))
    b.line([(W - 3, base), (W - 3, base - 12)], (60, 50, 50)); b.line([(W - 3, base - 12), (W - 7, base - 15)], (60, 50, 50))   # ต้นไม้ตาย
    if era in ("magic", "star"): b.put(12, base - 8, (180, 255, 220, 160))   # วิญญาณ


def bathhouse(b, E, era):
    base = base_y(b); W = b.w
    x0, x1 = 3, W - 4
    top, cx = body(b, E, era, x0, x1, base, 9, 9, 5, 6)
    door(b, cx, base - 1, 3, 4, E["door"], E["trim"])
    for x in (x0 + 3, x1 - 3):
        for i in range(3): b.put(x + (i % 2), top - 12 - i * 2 + (4 if era in ("bronze", "star") else 0), (225, 225, 235, 150 - i * 40))   # ไอน้ำ
    m, d = b.mask(); d.ellipse((x0 + 1, base + 0, x1 - 1, base + 2), fill=255); b.fill(m, R("#5aa0d8"), None, outline=False)


def courthouse(b, E, era):
    base = base_y(b); W = b.w
    x0, x1 = 4, W - 5
    if era == "stone": return hall(b, E, era)
    top = base - 12; cx = W // 2
    wall(b, x0, top, x1, base, E["wall"], E["wt"] if era != "iron" else "stone")
    foundation(b, x0, x1, base, E["stone"])
    m, d = b.mask(); d.polygon([(x0 - 2, top + 1), (cx, top - 8), (x1 + 2, top + 1)], fill=255); b.fill(m, R(E["roof"]), tex(b, E["rt"] if era not in ("bronze",) else "plaster"))
    for x in range(x0 + 1, x1, 4):
        m, d = b.mask(); d.rectangle((x, top + 2, x + 1, base - 1), fill=255); b.fill(m, R("#f2eee4"), None, outline=False)
    b.line([(cx - 3, top - 3), (cx + 3, top - 3)], (230, 200, 90)); b.put(cx, top - 4, (230, 200, 90, 255))   # ตาชั่ง
    b.put(cx - 3, top - 2, (230, 200, 90, 255)); b.put(cx + 3, top - 2, (230, 200, 90, 255))
    door(b, cx, base - 1, 4, 6, E["door"], E["trim"], arch=True)
    banner_wall(b, x0 + 1, top + 2, 4); banner_wall(b, x1 - 3, top + 2, 4)


def jail(b, E, era):
    base = base_y(b); W = b.w
    x0, x1 = 3, W - 4
    top = base - 10
    wall(b, x0, top, x1, base, "#7a7a80" if era != "stone" else "#8a8478", "stone")
    if era in ("stone", "bronze"): roof_flat(b, x0, x1, top, 4, "#6a6a70")
    else: battlements(b, x0, x1, top, "#7a7a80")
    for wx in range(x0 + 2, x1 - 2, 5):
        for xx in range(wx, wx + 3):
            for yy in range(top + 3, top + 6): b.put(xx, yy, (20, 18, 22, 255))
        b.put(wx + 1, top + 3, (120, 120, 130, 255)); b.put(wx + 1, top + 5, (120, 120, 130, 255))   # ลูกกรง
    door(b, W // 2, base - 1, 3, 5, "#4a4a52", "#3a3a40", arch=False)
    torch(b, x0 - 1, base - 5)


def lighthouse(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    for y in range(base - 2, base + 2):
        for x in range(0, W): b.put(x, y, (70, 130, 180, 255) if (x + y) % 4 else (120, 180, 220, 255))
    m, d = b.mask(); d.polygon([(cx - 5, base - 2), (cx - 3, base - 30), (cx + 3, base - 30), (cx + 5, base - 2)], fill=255)
    b.fill(m, R("#f0ece4" if era in ("kingdom", "star") else E["wall"]), tex(b, "brick" if era != "star" else "marble"))
    for y in range(base - 26, base - 2, 8):
        for x in range(cx - 5, cx + 6):
            c = b.get(x, y)
            if c[3] and c[:3] != R("#f0ece4")[0][:3]: b.put(x, y, (200, 60, 60, 255)); b.put(x, y + 1, (200, 60, 60, 255))
    m, d = b.mask(); d.rectangle((cx - 3, base - 35, cx + 3, base - 30), fill=255); b.fill(m, R("#ffe27a" if era != "magic" else "#7af0ff", 0.6), None)
    roof_cone(b, cx, base - 35, 4, 5, E["roof"], "slate")
    glow(b, "#ffe27a" if era != "magic" else "#7af0ff", 30)


# ======================================================================= ทหาร
def city_wall(b, E, era):
    base = base_y(b); W = b.w
    stoneish = era in ("kingdom", "magic", "star", "bronze")
    c = E["wall"] if stoneish else "#8a5a30"
    m, d = b.mask(); d.rectangle((0, base - 12, W - 1, base), fill=255); b.fill(m, R(c), tex(b, "stone" if stoneish else "wood"))
    if stoneish: battlements(b, 0, W - 1, base - 12, c)
    else:
        for x in range(0, W, 2): b.put(x, base - 13, R(c)[3]); b.put(x, base - 14, R(c)[2])
    m, d = b.mask(); d.rectangle((W // 2 - 4, base - 18, W // 2 + 4, base), fill=255); b.fill(m, R(c), tex(b, "stone" if stoneish else "wood"))
    if era in ("kingdom", "magic"): roof_cone(b, W // 2, base - 18, 5, 8, E["roof"], "slate" if era == "magic" else "tile")
    elif era == "star": dome(b, W // 2, base - 18, 4, E["roof"])
    window(b, W // 2 - 1, base - 14, 2, 3, E["glass"], E["trim"], arch=stoneish, sill=False)
    flag(b, W // 2, base - (32 if era in ("kingdom", "magic") else 26), 6)


def archery(b, E, era):
    base = base_y(b); W = b.w
    for x in range(1, W - 1): b.put(x, base + 1, (110, 150, 70, 255))
    for i, x in enumerate((W - 8, W - 15)):   # เป้าธนู
        b.line([(x + 2, base), (x + 2, base - 4)], (110, 76, 46))
        m, d = b.mask(); d.ellipse((x, base - 10, x + 5, base - 4), fill=255); b.fill(m, R("#f0e8d8"), None)
        m, d = b.mask(); d.ellipse((x + 1, base - 9, x + 4, base - 5), fill=255); b.fill(m, R("#d84a4a"), None, outline=False)
        b.put(x + 2, base - 7, (250, 230, 90, 255)); b.line([(x - 2, base - 8), (x + 1, base - 7)], (150, 110, 70))
    m, d = b.mask(); d.rectangle((2, base - 8, 9, base), fill=255); b.fill(m, R("#9a6a3a"), tex(b, "wood"))   # ซุ้มเก็บธนู
    roof_gable(b, 2, 9, base - 8, 4, E["roof"], "thatch" if era in ("stone", "bronze") else "slate" if era == "magic" else E["rt"], over=1)
    for x in range(3, 9, 2): b.line([(x, base - 1), (x, base - 6)], (150, 110, 70))
    flag(b, 1, base - 16, 5)


# ======================================================================= สายผจญภัย / ศรัทธา
def alchemist(b, E, era):
    base, cx, top = shop_body(b, E, era, None, 9)
    W = b.w
    for i, c in enumerate(("#ff5a7a", "#5affa0", "#5aa0ff", "#ffd04a")):   # ขวดยาหน้าร้าน
        x = W - 8 + (i % 2) * 3; y = base - 1 - (i // 2) * 4
        m, d = b.mask(); d.ellipse((x, y - 2, x + 2, y + 1), fill=255); b.fill(m, R(c, 0.7), None)
        b.put(x + 1, y - 3, (200, 190, 170, 255))
    m, d = b.mask(); d.ellipse((cx - 2, top - (13 if era not in ("bronze", "star") else 7), cx + 2, top - (9 if era not in ("bronze", "star") else 3)), fill=255)
    b.fill(m, R("#5affa0", 0.7), None)   # ควันยาเขียวลอย
    glow(b, "#5affa0", 15)


def shrine(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    if era == "stone":
        m, d = b.mask(); d.polygon([(cx - 3, base), (cx - 2, base - 12), (cx + 2, base - 12), (cx + 3, base)], fill=255); b.fill(m, R("#8a8478"), tex(b, "stone"))
        b.put(cx, base - 9, (230, 80, 60, 255)); b.put(cx, base - 6, (230, 80, 60, 255))
        for x in (cx - 5, cx + 5): torch(b, x, base - 3)
        return
    for x in (cx - 6, cx + 5):   # เสาประตูศาลเจ้า
        m, d = b.mask(); d.rectangle((x, base - 13, x + 1, base), fill=255); b.fill(m, R("#c83a2a" if era in ("iron", "kingdom") else E["trim"]), None)
    m, d = b.mask(); d.polygon([(cx - 9, base - 13), (cx + 9, base - 13), (cx + 8, base - 15), (cx - 8, base - 15)], fill=255)
    b.fill(m, R("#c83a2a" if era in ("iron", "kingdom") else E["roof"]), None)
    m, d = b.mask(); d.rectangle((cx - 3, base - 7, cx + 3, base), fill=255); b.fill(m, R(E["wall"]), tex(b, E["wt"]))
    roof_gable(b, cx - 3, cx + 3, base - 7, 4, E["roof"], "slate" if era == "magic" else "tile", over=1)
    b.put(cx, base - 3, (255, 220, 120, 255))
    if era in ("magic", "star"): glow(b, E["accent"], 20)




# ======================================================================= รายละเอียดความสมจริง
def shutters(b, x, y, h, col):
    rp = R(col)
    for yy in range(y, y + h): b.put(x - 2, yy, rp[2]); b.put(x + 3, yy, rp[2])


def dirt(b, x0, x1, base):
    for x in range(x0, x1 + 1):
        c = b.get(x, base - 1)
        if c[3] and b.rnd.random() < 0.5:
            b.put(x, base - 1, (max(0, c[0] - 25), max(0, c[1] - 25), max(0, c[2] - 25), 255))


def path(b, cx, base, n=3):
    for i in range(n):
        y = base + 1 + i
        for x in (cx - 1 - i % 2, cx + 1 - i % 2):
            if 0 <= y < b.h: b.put(x, y, (150, 140, 125, 255))


def floors_body(b, E, era, x0, x1, base, floors, rh=12, fh=6):
    """อาคารหลายชั้น: ผนัง + หลังคาตามยุค + หน้าต่างแยกชั้น + บานเกล็ด + คราบดิน"""
    wh = floors * fh
    top, cx = body(b, E, era, x0, x1, base, wh, rh, 5, None)
    for f in range(floors):
        y = top + 2 + f * fh
        if f > 0:
            for x in range(x0, x1 + 1): b.put(x, y - 2, R(E["trim"])[2])   # คิ้วแบ่งชั้น
        for wx in range(x0 + 3, x1 - 2, 5):
            if f == floors - 1 and abs(wx + 1 - cx) < 4: continue
            window(b, wx, y, 2, 3, E["glass"], E["trim"], arch=era in ("kingdom", "magic", "star"))
            if era in ("iron", "kingdom") and f < floors - 1: shutters(b, wx, y, 3, "#4a6a9a" if era == "kingdom" else "#6a4a2a")
    dirt(b, x0, x1, base)
    return top, cx


# ======================================================================= ปราสาทราชา (มหึมา): กำแพง 2 ชั้น + ลาน + หอคอยหลายต้น + ป้อมหลักหลายชั้น
def castle_grand(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    stoneish = era in ("kingdom", "magic", "star", "bronze")
    wallc = E["wall"] if stoneish else "#8a5a30"
    wtex = "stone" if stoneish else "wood"

    def tower(tx, ty_top, w, ybot, roof=True, big=False):
        m, d = b.mask(); d.rectangle((tx, ty_top, tx + w, ybot), fill=255); b.fill(m, R(wallc), tex(b, wtex))
        if not roof: battlements(b, tx, tx + w, ty_top, wallc); return
        if era in ("kingdom", "magic"): roof_cone(b, tx + w / 2, ty_top, w / 2 + 2, (14 if big else 10), E["roof"], "slate" if era == "magic" else "tile")
        elif era == "star": dome(b, int(tx + w / 2), ty_top, w // 2 + 1, E["roof"])
        elif era == "bronze": battlements(b, tx, tx + w, ty_top, wallc)
        else: roof_cone(b, tx + w / 2, ty_top, w / 2 + 2, 8, E["roof"], "thatch" if era == "stone" else "shingle")
        for wy in range(ty_top + 4, ybot - 4, 7): window(b, int(tx + w / 2) - 1, wy, 2, 3, E["glass"], E["trim"], arch=stoneish, sill=False)

    # ---- ป้อมหลัก (ด้านหลังสุด) หลายชั้น
    kx0, kx1 = cx - 16, cx + 16
    ktop = base - 52
    m, d = b.mask(); d.rectangle((kx0, ktop, kx1, base - 24), fill=255); b.fill(m, R(E["wall"] if era != "stone" else "#8a6a48"), tex(b, E["wt"] if stoneish else "wood"))
    for f in range(3):
        y = ktop + 4 + f * 8
        for x in range(kx0, kx1 + 1): b.put(x, y - 2, R(E["trim"])[2])
        for wx in range(kx0 + 3, kx1 - 2, 5): window(b, wx, y, 2, 4 if f == 0 else 3, E["glass"], E["trim"], arch=stoneish, sill=False)
    if era == "stone":
        roof_cone(b, cx, ktop + 2, 20, 22, E["roof"])
    elif era == "bronze":
        roof_flat(b, kx0, kx1, ktop, 4, E["roof"])
        for i, w in enumerate((12, 8)):
            m, d = b.mask(); d.rectangle((cx - w, ktop - 8 - i * 7, cx + w, ktop - 2 - i * 7), fill=255); b.fill(m, R(E["wall"]), tex(b, "brick"))
            roof_flat(b, cx - w, cx + w, ktop - 8 - i * 7, 3, E["roof"])
    elif era == "iron":
        roof_gable(b, kx0, kx1, ktop, 16, E["roof"], "shingle")
    elif era == "kingdom":
        battlements(b, kx0, kx1, ktop, E["wall"])
        tower(cx - 6, ktop - 18, 12, ktop, True, True)
    elif era == "magic":
        roof_gable(b, kx0, kx1, ktop, 12, E["roof"], "slate")
        for sx, hh in ((cx - 10, 18), (cx, 30), (cx + 10, 18)):
            m, d = b.mask(); d.rectangle((sx - 3, ktop - hh, sx + 3, ktop), fill=255); b.fill(m, R(E["wall"]), tex(b, "stone"))
            roof_cone(b, sx, ktop - hh, 4, 12, E["roof"], "slate")
        crystal(b, cx, ktop - 44, E["accent"], 7)
    else:
        roof_flat(b, kx0, kx1, ktop, 3, E["wall"], parapet=False)
        dome(b, cx, ktop - 2, 14, E["roof"])
        for sx in (kx0 + 2, kx1 - 2):
            m, d = b.mask(); d.rectangle((sx - 2, ktop - 20, sx + 2, ktop), fill=255); b.fill(m, R(E["wall"]), tex(b, "marble")); b.put(sx, ktop - 21, (255, 240, 160, 255))
        m, d = b.mask(); d.ellipse((cx - 22, ktop - 24, cx + 22, ktop - 17), fill=255); d.ellipse((cx - 20, ktop - 23, cx + 20, ktop - 18), fill=0)
        b.fill(m, R(E["accent"], 0.7), None, outline=False)
    flag(b, cx, ktop - (56 if era == "magic" else 40 if era in ("kingdom", "star", "stone") else 30), 9, True)

    # ---- กำแพงชั้นใน + หอมุมใน
    iy = base - 26
    m, d = b.mask(); d.rectangle((10, iy, W - 11, base - 14), fill=255); b.fill(m, R(wallc), tex(b, wtex))
    if stoneish: battlements(b, 10, W - 11, iy, wallc)
    for tx in (6, W - 15): tower(tx, iy - 12, 8, base - 14)
    # ---- ลานใน: ต้นไม้ + บ่อน้ำ
    for x in (16, W - 20): bush(b, x, base - 16, "#3e8a3e")
    # ---- กำแพงชั้นนอก + หอมุมใหญ่ + ประตูหอคู่
    oy = base - 13
    m, d = b.mask(); d.rectangle((2, oy, W - 3, base), fill=255); b.fill(m, R(wallc), tex(b, wtex))
    if stoneish: battlements(b, 2, W - 3, oy, wallc)
    else:
        for x in range(2, W - 2, 2): b.put(x, oy - 1, R(wallc)[3]); b.put(x, oy - 2, R(wallc)[2])
    for tx in (0, W - 11): tower(tx, oy - 14, 10, base, True, True)
    for tx in (cx - 12, cx + 5): tower(tx, oy - 10, 7, base)
    m, d = b.mask(); d.rectangle((cx - 5, base - 11, cx + 5, base), fill=255); d.ellipse((cx - 5, base - 15, cx + 5, base - 7), fill=255); b.fill(m, R("#2a1c14"), None)
    for x in range(cx - 4, cx + 5, 2):
        for y in range(base - 12, base - 3): b.put(x, y, (110, 110, 120, 255))
    for x in (cx - 9, cx + 7): banner_wall(b, x, oy + 2, 6)
    for x in (cx - 7, cx + 7): torch(b, x, base - 6)
    if era == "stone":
        for x in range(4, W - 4, 8): b.put(x, oy - 4, (240, 240, 230, 255)); b.put(x, oy - 3, (240, 240, 230, 255))
    dirt(b, 2, W - 3, base)
    path(b, cx, base, 2)


# ======================================================================= มหาวิหาร (ใหญ่)
def cathedral(b, E, era):
    if era in ("stone", "bronze"): return temple(b, E, era)
    base = base_y(b); W = b.w; cx = W // 2
    x0, x1 = 4, W - 5; top = base - 20
    wall(b, x0, top, x1, base, E["wall"], E["wt"])
    foundation(b, x0, x1, base, E["stone"])
    if era == "star":
        roof_flat(b, x0, x1, top, 3, E["wall"], parapet=False); dome(b, cx, top - 2, 14, E["roof"])
        for sx in (x0 + 2, x1 - 2):
            m, d = b.mask(); d.rectangle((sx - 2, top - 22, sx + 2, top), fill=255); b.fill(m, R(E["wall"]), tex(b, "marble")); dome(b, sx, top - 22, 3, E["roof"])
        glow(b, E["accent"], 25)
    else:
        roof_gable(b, x0, x1, top, 14, E["roof"], "slate" if era == "magic" else E["rt"])
        for sx, hh in ((x0 + 4, 26), (x1 - 4, 26)):
            m, d = b.mask(); d.rectangle((sx - 4, top - hh, sx + 4, base), fill=255); b.fill(m, R(E["wall"]), tex(b, E["wt"]))
            roof_cone(b, sx, top - hh, 5, 16, E["roof"], "slate" if era == "magic" else "tile")
            window(b, sx - 1, top - hh + 6, 2, 5, "#a070ff", E["trim"], arch=True, sill=False)
            b.put(sx, top - hh - 17, (240, 210, 90, 255))
        m, d = b.mask(); d.ellipse((cx - 5, top - 10, cx + 5, top), fill=255); b.fill(m, R("#c060e0", 0.6), None)   # หน้าต่างกุหลาบใหญ่
        for a in range(0, 360, 45): b.put(cx + int(math.cos(math.radians(a)) * 3), top - 5 + int(math.sin(math.radians(a)) * 3), (255, 220, 120, 255))
    for wx in range(x0 + 10, x1 - 9, 5):
        if abs(wx - cx) > 4: window(b, wx, top + 4, 2, 8, "#a070ff" if era != "iron" else E["glass"], E["trim"], arch=True)
    door(b, cx, base - 1, 6, 9, E["door"], E["trim"], arch=True)
    path(b, cx, base, 2); dirt(b, x0, x1, base)


# ======================================================================= กิลด์ใหญ่ 3 ชั้น + ปีกข้าง + ลานฝึก
def guild_grand(b, E, era):
    if era == "stone": return guild(b, E, era)
    base = base_y(b); W = b.w
    x0, x1 = 12, W - 13
    top, cx = floors_body(b, E, era, x0, x1, base, 3, 14, 7)
    # ปีกซ้าย (โรงเตี๊ยมนักผจญภัย) ปีกขวา (คลังของรางวัล)
    for wx0, wx1 in ((2, x0 - 1), (x1 + 1, W - 3)):
        t2, _ = floors_body(b, E, era, wx0, wx1, base, 2, 8, 6)
    sx, sy = cx, top + 2
    m, d = b.mask(); d.rectangle((sx - 6, sy, sx + 6, sy + 7), fill=255); b.fill(m, R("#7a5230"), tex(b, "wood"))
    b.line([(sx - 3, sy + 6), (sx + 3, sy + 1)], (235, 235, 245)); b.line([(sx + 3, sy + 6), (sx - 3, sy + 1)], (235, 235, 245))
    m, d = b.mask(); d.ellipse((sx - 2, sy + 2, sx + 2, sy + 5), fill=255); b.fill(m, R("#e6b440"), None, outline=False)
    door(b, cx, base - 1, 6, 7, E["door"], E["trim"], arch=True)
    for x in (cx - 5, cx + 5): torch(b, x, base - 6)
    bx = x0 - 9   # กระดานภารกิจหน้าอาคาร
    m, d = b.mask(); d.rectangle((bx, base - 9, bx + 6, base - 3), fill=255); b.fill(m, R("#8a5a30"), tex(b, "wood"))
    for (px_, py_) in ((bx + 1, base - 8), (bx + 3, base - 7), (bx + 5, base - 8), (bx + 2, base - 5), (bx + 4, base - 5)): b.put(px_, py_, (240, 230, 200, 255))
    flag(b, x0 + 2, top - 26, 9, True); flag(b, x1 - 4, top - 26, 9, True)
    path(b, cx, base, 2)




# ======================================================================= สายเทคโนโลยี: อาคารประจำยุค (ยุคละ 5)
def water(b, y0, y1, x0=0, x1=None, col=(70, 130, 180)):
    x1 = b.w - 1 if x1 is None else x1
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1): b.put(x, y, col + (255,) if (x + y) % 4 else (120, 180, 220, 255))


def gear_icon(b, cx, cy, r, col="#b08a40"):
    for a in range(0, 360, 45):
        b.put(cx + int(round(math.cos(math.radians(a)) * (r + 1))), cy + int(round(math.sin(math.radians(a)) * (r + 1))), R(col)[1])
    m, d = b.mask(); d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=255); b.fill(m, R(col), None)
    b.put(cx, cy, (40, 30, 20, 255))


def wheel(b, cx, cy, r, col="#8a5a30"):
    for a in range(0, 360, 30):
        b.line([(cx, cy), (cx + math.cos(math.radians(a)) * r, cy + math.sin(math.radians(a)) * r)], R(col)[1][:3])
    m, d = b.mask(); d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=255); d.ellipse((cx - r + 2, cy - r + 2, cx + r - 2, cy + r - 2), fill=0); b.fill(m, R(col), None)


# ---- ยุคหิน
def t_cave(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.polygon([(0, base), (4, base - 16), (12, base - 22), (22, base - 20), (W - 1, base - 8), (W - 1, base)], fill=255); b.fill(m, R("#8a8478"), tex(b, "stone"))
    m, d = b.mask(); d.ellipse((cx - 6, base - 10, cx + 6, base + 2), fill=255); b.fill(m, R("#1a1410"), None)
    m, d = b.mask(); d.ellipse((cx - 2, base - 3, cx + 2, base), fill=255); b.fill(m, R("#ff8a3a", 0.6), None, outline=False)
    for x in (cx - 8, cx + 8): torch(b, x, base - 4)
    b.put(cx - 4, base - 15, (200, 80, 50, 255)); b.put(cx + 3, base - 16, (200, 80, 50, 255))   # ภาพเขียนผนัง


def t_flint(b, E, era):
    base = base_y(b); W = b.w
    m, d = b.mask(); d.ellipse((2, base - 5, 12, base + 1), fill=255); b.fill(m, R("#8a8478"), tex(b, "stone"))   # หินทั่ง
    for i, x in enumerate(range(14, W - 2, 3)):
        m, d = b.mask(); d.polygon([(x, base), (x + 1, base - 4), (x + 2, base)], fill=255); b.fill(m, R("#5a5a64"), None)   # หัวหอกหิน
    b.line([(3, base - 9), (W - 3, base - 9)], (110, 76, 46))
    for x in (3, W - 3): b.line([(x, base), (x, base - 9)], (110, 76, 46))
    for x in range(5, W - 4, 3): b.line([(x, base - 9), (x, base - 7)], (200, 190, 170))   # หนังตาก


def t_roast(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.ellipse((cx - 5, base - 3, cx + 5, base + 1), fill=255); b.fill(m, R("#7a7468"), tex(b, "stone"))
    m, d = b.mask(); d.ellipse((cx - 3, base - 4, cx + 3, base), fill=255); b.fill(m, R("#ff8a3a", 0.6), None, outline=False)
    for x in (cx - 6, cx + 6): b.line([(x, base), (x, base - 9)], (110, 76, 46))
    b.line([(cx - 7, base - 8), (cx + 7, base - 8)], (110, 76, 46))
    m, d = b.mask(); d.ellipse((cx - 4, base - 10, cx + 4, base - 6), fill=255); b.fill(m, R("#a0502a"), None)   # เนื้อย่าง
    b.put(cx, base - 12, (210, 210, 215, 160)); b.put(cx + 1, base - 14, (210, 210, 215, 110))


def t_totem(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    for i, c in enumerate(("#b05a30", "#3a8aa0", "#c8a040", "#a03a3a")):
        y1 = base - i * 6; y0 = y1 - 6
        m, d = b.mask(); d.rectangle((cx - 3, y0, cx + 3, y1), fill=255); b.fill(m, R(c), tex(b, "wood"))
        b.put(cx - 1, y0 + 2, (20, 15, 10, 255)); b.put(cx + 1, y0 + 2, (20, 15, 10, 255)); b.put(cx, y0 + 4, (240, 230, 210, 255))
    m, d = b.mask(); d.polygon([(cx - 9, base - 22), (cx, base - 25), (cx + 9, base - 22), (cx, base - 20)], fill=255); b.fill(m, R("#e0b040"), None)   # ปีกนกบนยอด


def t_hunt(b, E, era):
    base = base_y(b); W = b.w
    for x in range(1, W - 1, 2): b.line([(x, base), (x, base - 7)], (110, 76, 46)); b.put(x, base - 8, (200, 190, 170, 255))
    m, d = b.mask(); d.ellipse((W // 2 - 6, base - 6, W // 2 + 6, base - 1), fill=255); b.fill(m, R("#8a5a3a"), None)   # หนังสัตว์ขึง
    for (dx, dy) in ((-3, -9), (3, -9)): b.line([(W // 2 + dx, base - 7), (W // 2 + dx * 2, base + dy)], (240, 235, 225))   # เขาสัตว์


# ---- ยุคสำริด
def t_foundry(b, E, era):
    base = base_y(b); W = b.w
    top, cx = body(b, E, era, 3, W - 12, base, 9, 9, 5)
    m, d = b.mask(); d.polygon([(W - 11, base), (W - 9, base - 14), (W - 4, base - 14), (W - 2, base)], fill=255); b.fill(m, R("#9a6a4a"), tex(b, "brick"))
    m, d = b.mask(); d.ellipse((W - 9, base - 5, W - 4, base - 1), fill=255); b.fill(m, R("#ff9a3a", 0.6), None)
    for i in range(3): b.put(W - 7 + i % 2, base - 16 - i * 2, (210, 210, 215, 160 - i * 40))
    for x in (5, 9): m, d = b.mask(); d.rectangle((x, base + 0, x + 2, base + 1), fill=255); b.fill(m, R("#c88a3a"), None, outline=False)   # แท่งสำริด
    door(b, cx, base - 1, 3, 4, E["door"], E["trim"])


def t_waterwheel(b, E, era):
    base = base_y(b); W = b.w
    water(b, base - 1, base + 1)
    top, cx = body(b, E, era, 2, W - 13, base - 2, 9, 9, 5)
    wheel(b, W - 7, base - 7, 6, "#7a5230")
    door(b, cx, base - 3, 3, 4, E["door"], E["trim"])


def t_scribe(b, E, era):
    base = base_y(b); W = b.w
    top, cx = body(b, E, era, 3, W - 4, base, 9, 9, 5)
    door(b, cx, base - 1, 3, 4, E["door"], E["trim"])
    for i in range(4):   # แผ่นดินเหนียวจารึกวางตาก
        x = 3 + i * 4
        m, d = b.mask(); d.rectangle((x, base + 0, x + 2, base + 1), fill=255); b.fill(m, R("#c89a64"), None, outline=False)
        b.put(x + 1, base, (110, 80, 50, 255))
    sign(b, cx + 6, top + 1, "book", era)


def t_chariot(b, E, era):
    base = base_y(b); W = b.w
    m, d = b.mask(); d.rectangle((2, base - 9, 14, base), fill=255); b.fill(m, R(E["wall"]), tex(b, "plaster"))
    roof_flat(b, 2, 14, base - 9, 4, E["roof"])
    m, d = b.mask(); d.polygon([(17, base - 7), (26, base - 7), (27, base - 3), (17, base - 3)], fill=255); b.fill(m, R("#b07a3a"), tex(b, "wood"))   # รถศึก
    wheel(b, 21, base - 2, 3, "#6a4a2a")
    b.line([(26, base - 5), (W - 1, base - 6)], (110, 76, 46))


def t_canal(b, E, era):
    base = base_y(b); W = b.w
    water(b, base - 4, base + 1, 0, W - 1)
    for x in (W // 2 - 6, W // 2 + 5):
        m, d = b.mask(); d.rectangle((x, base - 12, x + 2, base + 1), fill=255); b.fill(m, R(E["stone"]), tex(b, "brick"))
    m, d = b.mask(); d.rectangle((W // 2 - 4, base - 10, W // 2 + 4, base - 6), fill=255); b.fill(m, R("#8a5a30"), tex(b, "wood"))   # ประตูน้ำ
    b.line([(W // 2 - 6, base - 13), (W // 2 + 7, base - 13)], (110, 76, 46))
    for x in range(2, W - 2, 4): b.put(x, base - 6, (110, 180, 60, 255)); b.put(x, base - 7, (160, 210, 80, 255))


# ---- ยุคเหล็ก
def t_blast(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.polygon([(cx - 8, base), (cx - 5, base - 22), (cx + 5, base - 22), (cx + 8, base)], fill=255); b.fill(m, R("#7a6a5a"), tex(b, "stone"))
    m, d = b.mask(); d.ellipse((cx - 4, base - 7, cx + 4, base - 1), fill=255); b.fill(m, R("#ff9a3a", 0.6), None)
    for i in range(4): b.put(cx - 1 + i % 2, base - 25 - i * 2, (90, 90, 95, 200 - i * 40))   # ควันดำ
    for x in (2, 5): m, d = b.mask(); d.rectangle((x, base - 3, x + 2, base), fill=255); b.fill(m, R("#2a2a2a"), None)   # กองถ่าน
    b.line([(cx + 9, base), (cx + 9, base - 6)], (110, 76, 46)); b.put(cx + 10, base - 4, (150, 155, 170, 255))


def t_watermill(b, E, era):
    t_waterwheel(b, E, era)
    m, d = b.mask(); d.ellipse((4, b.h - 5, 9, b.h - 3), fill=255); b.fill(m, R("#e8d8b8"), None)   # กระสอบแป้ง


def t_siege(b, E, era):
    base = base_y(b); W = b.w
    m, d = b.mask(); d.rectangle((2, base - 10, 16, base), fill=255); b.fill(m, R("#9a6a3a"), tex(b, "wood"))
    roof_gable(b, 2, 16, base - 10, 7, E["roof"], "shingle", over=1)
    # เครื่องยิงหิน
    b.line([(19, base - 2), (W - 2, base - 2)], (110, 76, 46), 2)
    b.line([(22, base - 2), (25, base - 9)], (110, 76, 46)); b.line([(W - 4, base - 2), (25, base - 9)], (110, 76, 46))
    b.line([(25, base - 9), (20, base - 16)], (130, 90, 50), 2)
    m, d = b.mask(); d.ellipse((18, base - 18, 21, base - 15), fill=255); b.fill(m, R("#8a8478"), None)
    wheel(b, 21, base - 1, 2, "#6a4a2a"); wheel(b, W - 4, base - 1, 2, "#6a4a2a")


def t_shipyard(b, E, era):
    base = base_y(b); W = b.w
    water(b, base - 1, base + 1)
    for x in range(4, W - 4, 5): b.line([(x, base - 1), (x, base - 16)], (110, 76, 46))   # โครงอู่
    b.line([(3, base - 16), (W - 4, base - 16)], (110, 76, 46))
    m, d = b.mask(); d.polygon([(6, base - 6), (W - 6, base - 6), (W - 9, base - 1), (9, base - 1)], fill=255); b.fill(m, R("#9a6a3a"), tex(b, "wood"))   # ตัวเรือ
    b.line([(W // 2, base - 6), (W // 2, base - 15)], (90, 64, 40))
    for x in range(W // 2 + 1, W // 2 + 6): b.line([(x, base - 14), (x, base - 9)], (240, 235, 220))   # ใบเรือ


def t_coal(b, E, era):
    mine(b, E, era)
    base = base_y(b)
    for x in range(b.w - 10, b.w - 2): b.put(x, base + 1, (30, 30, 32, 255)); b.put(x + 1, base, (45, 45, 48, 255))


# ---- ยุคอาณาจักร
def t_printing(b, E, era):
    base = base_y(b); W = b.w
    top, cx = floors_body(b, E, era, 3, W - 4, base, 2, 11, 6)
    sign(b, cx, top - 1 + 8, "book", era)
    door(b, cx, base - 1, 3, 5, E["door"], E["trim"])
    for i in range(3): b.put(W - 3 + 0, base - 2 - i * 2, (240, 236, 220, 255))   # กองกระดาษ
    gear_icon(b, 5, base - 3, 2)


def t_clocktower(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.rectangle((cx - 6, base - 40, cx + 6, base), fill=255); b.fill(m, R(E["wall"]), tex(b, E["wt"]))
    foundation(b, cx - 6, cx + 6, base, E["stone"])
    m, d = b.mask(); d.ellipse((cx - 5, base - 36, cx + 5, base - 26), fill=255); b.fill(m, R("#f2eee4"), None)   # หน้าปัด
    for a in range(0, 360, 30): b.put(cx + int(math.cos(math.radians(a)) * 4), base - 31 + int(math.sin(math.radians(a)) * 4), (60, 60, 70, 255))
    b.line([(cx, base - 31), (cx, base - 34)], (30, 30, 40)); b.line([(cx, base - 31), (cx + 2, base - 30)], (30, 30, 40))
    roof_cone(b, cx, base - 40, 7, 12, E["roof"], "tile")
    for wy in (base - 20, base - 12): window(b, cx - 1, wy, 2, 4, E["glass"], E["trim"], arch=True, sill=False)
    door(b, cx, base - 1, 3, 5, E["door"], E["trim"])
    flag(b, cx, base - 58, 6)


def t_university(b, E, era):
    base = base_y(b); W = b.w
    top, cx = floors_body(b, E, era, 3, W - 4, base, 3, 13, 6)
    m, d = b.mask(); d.rectangle((cx - 4, top - 18, cx + 4, top), fill=255); b.fill(m, R(E["wall"]), tex(b, E["wt"]))
    dome(b, cx, top - 18, 5, "#4a70c0", "slate")
    door(b, cx, base - 1, 4, 6, E["door"], E["trim"], arch=True)
    for x in (3, W - 8): flowers(b, x, base + 1, 3)
    banner_wall(b, cx - 1, top + 2, 4)


def t_arsenal(b, E, era):
    base = base_y(b); W = b.w
    top, cx = body(b, E, era, 3, W - 14, base, 10, 10, 5)
    battlements(b, 3, W - 14, top - 10, E["wall"])
    door(b, cx, base - 1, 4, 5, E["door"], E["trim"])
    # ปืนใหญ่
    m, d = b.mask(); d.polygon([(W - 12, base - 6), (W - 2, base - 9), (W - 1, base - 7), (W - 11, base - 4)], fill=255); b.fill(m, R("#4a4a52"), None)
    wheel(b, W - 9, base - 2, 3, "#6a4a2a")
    for i in range(3):   # กองลูกปืน
        m, d = b.mask(); d.ellipse((W - 13 + i * 2, base - 1 - (i == 1) * 2, W - 11 + i * 2, base + 1 - (i == 1) * 2), fill=255); b.fill(m, R("#3a3a40"), None, outline=False)


def t_harbor(b, E, era):
    base = base_y(b); W = b.w
    water(b, base - 2, base + 1)
    top, cx = floors_body(b, E, era, 2, W - 16, base - 3, 2, 10, 6)
    for x in range(W - 15, W - 1, 2): b.line([(x, base - 3), (x, base + 1)], (100, 70, 40))
    b.line([(W - 15, base - 4), (W - 1, base - 4)], (140, 100, 60))
    for x in (W - 12, W - 8): crate(b, x, base - 8)
    b.line([(W - 3, base - 4), (W - 3, base - 18)], (90, 64, 40)); b.line([(W - 3, base - 18), (W - 9, base - 12)], (90, 64, 40))   # ปั้นจั่นท่าเรือ


# ---- ยุคเวทมนตร์ (magitech)
def t_mana_reactor(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.ellipse((cx - 11, base - 8, cx + 11, base + 1), fill=255); b.fill(m, R(E["wall"]), tex(b, "stone"))
    for x in (cx - 8, cx + 7):
        m, d = b.mask(); d.rectangle((x, base - 22, x + 1, base - 4), fill=255); b.fill(m, R("#4a4060"), None)
    m, d = b.mask(); d.polygon([(cx, base - 30), (cx + 6, base - 18), (cx, base - 8), (cx - 6, base - 18)], fill=255); b.fill(m, R(E["accent"], 0.7), None)
    for a in range(0, 360, 40): b.put(cx + int(math.cos(math.radians(a)) * 10), base - 18 + int(math.sin(math.radians(a)) * 3), (180, 255, 255, 200))
    glow(b, E["accent"], 40)


def t_golem(b, E, era):
    base = base_y(b); W = b.w
    top, cx = body(b, E, era, 2, W - 13, base, 10, 11, 5)
    door(b, cx, base - 1, 5, 7, E["door"], E["trim"])
    gx = W - 7   # โกเลมหิน
    m, d = b.mask(); d.rectangle((gx - 3, base - 13, gx + 3, base - 4), fill=255); d.ellipse((gx - 2, base - 18, gx + 2, base - 13), fill=255)
    d.rectangle((gx - 5, base - 12, gx - 4, base - 6), fill=255); d.rectangle((gx + 4, base - 12, gx + 5, base - 6), fill=255)
    d.rectangle((gx - 3, base - 4, gx - 1, base), fill=255); d.rectangle((gx + 1, base - 4, gx + 3, base), fill=255)
    b.fill(m, R("#8a8478"), tex(b, "stone"))
    b.put(gx - 1, base - 16, E["accent"] if isinstance(E["accent"], tuple) else (122, 240, 255, 255)); b.put(gx + 1, base - 16, (122, 240, 255, 255))
    b.put(gx, base - 9, (122, 240, 255, 255))


def t_airship_dock(b, E, era):
    base = base_y(b); W = b.w
    for x in (4, W - 6):
        m, d = b.mask(); d.rectangle((x, base - 22, x + 2, base), fill=255); b.fill(m, R(E["wall"]), tex(b, "stone"))
    b.line([(4, base - 22), (W - 4, base - 22)], (110, 90, 120), 2)
    # เรือเหาะ
    m, d = b.mask(); d.ellipse((6, base - 38, W - 7, base - 27), fill=255); b.fill(m, R("#c8c0a8"), tex(b, "plaster"))
    m, d = b.mask(); d.polygon([(10, base - 26), (W - 11, base - 26), (W - 13, base - 21), (12, base - 21)], fill=255); b.fill(m, R("#8a5a30"), tex(b, "wood"))
    for x in (12, W // 2, W - 13): b.line([(x, base - 28), (x, base - 26)], (80, 60, 40))
    b.put(W // 2, base - 33, (255, 0, 255, 255)); b.put(W // 2 + 1, base - 33, (255, 0, 255, 255))
    m, d = b.mask(); d.ellipse((W // 2 - 2, base - 24, W // 2 + 2, base - 22), fill=255); b.fill(m, R(E["accent"], 0.7), None, outline=False)


def t_teleport(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.ellipse((2, base - 7, W - 3, base + 1), fill=255); b.fill(m, R("#5a5470"), tex(b, "stone"))
    m, d = b.mask(); d.ellipse((5, base - 5, W - 6, base - 1), fill=255); d.ellipse((8, base - 4, W - 9, base - 2), fill=0); b.fill(m, R(E["accent"], 0.7), None, outline=False)
    for i in range(6):
        a = math.radians(i * 60); x = cx + int(math.cos(a) * (W // 2 - 4)); y = base - 3 + int(math.sin(a) * 3)
        crystal(b, x, y, "#c07aff", 4)
    for y in range(base - 22, base - 4, 3): b.put(cx + (y % 2), y, (200, 240, 255, 160))
    glow(b, E["accent"], 30)


def t_arcane_library(b, E, era):
    base = base_y(b); W = b.w
    top, cx = floors_body(b, E, era, 4, W - 5, base, 3, 14, 6)
    for (x, y) in ((2, top - 6), (W - 4, top - 10), (cx, top - 20)):   # หนังสือลอย
        m, d = b.mask(); d.rectangle((x - 1, y, x + 2, y + 2), fill=255); b.fill(m, R("#c07aff"), None)
    door(b, cx, base - 1, 4, 6, E["door"], E["trim"], arch=True)
    glow(b, E["accent"], 15)


# ---- ยุคดารา
def t_skyport(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.rectangle((cx - 3, base - 24, cx + 3, base), fill=255); b.fill(m, R(E["wall"]), tex(b, "marble"))
    m, d = b.mask(); d.ellipse((2, base - 28, W - 3, base - 22), fill=255); b.fill(m, R("#d8d4e8"), tex(b, "marble"))   # ลานจอด
    m, d = b.mask(); d.ellipse((cx - 9, base - 40, cx + 9, base - 30), fill=255); b.fill(m, R("#e6b440"), tex(b, "gold"))   # ยานดารา
    m, d = b.mask(); d.ellipse((cx - 4, base - 41, cx + 4, base - 35), fill=255); b.fill(m, R("#9ad8ff", 0.6), None)
    for x in (cx - 6, cx + 6): b.put(x, base - 29, (150, 220, 255, 220))
    glow(b, E["accent"], 25)


def t_starforge(b, E, era):
    base = base_y(b); W = b.w
    top, cx = body(b, E, era, 3, W - 4, base, 11, 10, 5, 8)
    m, d = b.mask(); d.ellipse((cx - 4, base - 8, cx + 4, base - 1), fill=255); b.fill(m, R("#8fd0ff", 0.6), None)
    for a in range(0, 360, 60): b.put(cx + int(math.cos(math.radians(a)) * 7), base - 5 + int(math.sin(math.radians(a)) * 4), (255, 240, 160, 255))
    glow(b, "#8fd0ff", 25)


def t_observatory(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    top, cx = body(b, E, era, 4, W - 5, base, 12, 10, 5, 10)
    b.line([(cx + 3, top - 10), (cx + 13, top - 20)], (220, 200, 140), 2)
    m, d = b.mask(); d.ellipse((cx + 11, top - 23, cx + 15, top - 19), fill=255); b.fill(m, R("#9ad8ff", 0.6), None)
    for (x, y) in ((3, 4), (W - 6, 8), (10, 2)): b.put(x, y, (255, 255, 220, 255))   # ดาว


def t_crystal_spire(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.rectangle((cx - 5, base - 6, cx + 5, base), fill=255); b.fill(m, R(E["wall"]), tex(b, "marble"))
    m, d = b.mask(); d.polygon([(cx, base - 44), (cx + 4, base - 20), (cx + 2, base - 6), (cx - 2, base - 6), (cx - 4, base - 20)], fill=255)
    b.fill(m, R("#9ad8ff", 0.7), None)
    for y in range(base - 40, base - 8, 6): b.put(cx, y, (255, 255, 255, 220))
    for r in (8, 12):
        m, d = b.mask(); d.ellipse((cx - r, base - 26 - r // 4, cx + r, base - 26 + r // 4), fill=255); d.ellipse((cx - r + 1, base - 26 - r // 4 + 1, cx + r - 1, base - 26 + r // 4 - 1), fill=0)
        b.fill(m, R("#e6b440", 0.7), None, outline=False)
    glow(b, "#9ad8ff", 40)


def t_floating_isle(b, E, era):
    base = base_y(b); W = b.w; cx = W // 2
    m, d = b.mask(); d.polygon([(3, base - 22), (W - 4, base - 22), (cx + 6, base - 10), (cx, base - 4), (cx - 6, base - 10)], fill=255); b.fill(m, R("#8a7458"), tex(b, "stone"))
    for x in range(4, W - 4): b.put(x, base - 23, (90, 170, 80, 255))
    house_x0 = cx - 7
    m, d = b.mask(); d.rectangle((house_x0, base - 31, house_x0 + 14, base - 23), fill=255); b.fill(m, R(E["wall"]), tex(b, "marble"))
    dome(b, cx, base - 31, 5, E["roof"])
    bush(b, 4, base - 27, "#5aa07a"); bush(b, W - 9, base - 27, "#5aa07a")
    for i in range(3): b.put(cx - 2 + i * 2, base - 2 + i % 2, (150, 220, 255, 200))   # หยดน้ำตกลงมา
    glow(b, E["accent"], 20)


TECH = {  # id: (ยุค, กว้าง, สูง, ฟังก์ชัน)
    "stone_cave": ("stone", 32, 26, t_cave), "stone_flint": ("stone", 28, 16, t_flint), "stone_roast": ("stone", 20, 18, t_roast),
    "stone_totem": ("stone", 20, 30, t_totem), "stone_hunt": ("stone", 26, 16, t_hunt),
    "bronze_foundry": ("bronze", 32, 24, t_foundry), "bronze_waterwheel": ("bronze", 32, 24, t_waterwheel), "bronze_scribe": ("bronze", 28, 22, t_scribe),
    "bronze_chariot": ("bronze", 32, 18, t_chariot), "bronze_canal": ("bronze", 30, 18, t_canal),
    "iron_blast_furnace": ("iron", 30, 32, t_blast), "iron_watermill": ("iron", 32, 26, t_watermill), "iron_siege": ("iron", 36, 24, t_siege),
    "iron_shipyard": ("iron", 34, 24, t_shipyard), "iron_coal_mine": ("iron", 32, 26, t_coal),
    "kingdom_printing": ("kingdom", 30, 30, t_printing), "kingdom_clocktower": ("kingdom", 22, 64, t_clocktower), "kingdom_university": ("kingdom", 44, 46, t_university),
    "kingdom_arsenal": ("kingdom", 38, 30, t_arsenal), "kingdom_harbor": ("kingdom", 40, 30, t_harbor),
    "magic_mana_reactor": ("magic", 28, 34, t_mana_reactor), "magic_golem_works": ("magic", 34, 30, t_golem), "magic_airship_dock": ("magic", 34, 44, t_airship_dock),
    "magic_teleport": ("magic", 30, 26, t_teleport), "magic_arcane_library": ("magic", 34, 44, t_arcane_library),
    "star_skyport": ("star", 34, 46, t_skyport), "star_starforge": ("star", 30, 30, t_starforge), "star_observatory": ("star", 34, 40, t_observatory),
    "star_crystal_spire": ("star", 26, 50, t_crystal_spire), "star_floating_isle": ("star", 32, 44, t_floating_isle),
}


def stone_variant(fn, kind):
    """ยุคหินไม่มีบ้านผนังเรียบมีกระจก: ใช้กระท่อมใหญ่ + สัญลักษณ์หน้าที่ของอาคาร"""
    def f(b, E, era):
        if era != "stone":
            return fn(b, E, era)
        house(b, E, era, True)
        base = base_y(b); cx = b.w // 2
        if kind == "mansion":
            for x in (2, b.w - 4): b.line([(x, base), (x, base - 12)], (110, 76, 46)); b.put(x, base - 13, (240, 240, 230, 255))
        elif kind == "school":
            m, d = b.mask(); d.rectangle((b.w - 6, base - 8, b.w - 2, base - 3), fill=255); b.fill(m, R("#8a8070"), tex(b, "stone"))
            b.put(b.w - 4, base - 6, (200, 80, 50, 255))   # หินจารึกภาพ
        elif kind == "tavern":
            barrel(b, b.w - 5, base - 3); m, d = b.mask(); d.ellipse((1, base - 3, 6, base + 1), fill=255); b.fill(m, R("#ff8a3a", 0.6), None)
        elif kind == "forge":
            m, d = b.mask(); d.ellipse((b.w - 8, base - 4, b.w - 2, base + 1), fill=255); b.fill(m, R("#ff8a3a", 0.6), None)
            b.put(b.w - 5, base - 6, (160, 160, 170, 255))
        elif kind == "healer":
            for x in range(1, 6): b.put(x, base, (90, 170, 80, 255)); b.put(x, base - 1, (140, 210, 110, 255))
        elif kind == "house2":
            pass
    return f


TYPES = {  # ประเภท: (กว้าง, สูง, ฟังก์ชัน)
    "house0": (24, 26, lambda b, E, e: house(b, E, e, False)),
    "house1": (28, 30, lambda b, E, e: house(b, E, e, True)),
    "hall": (40, 40, hall), "temple": (34, 44, temple), "barracks": (36, 30, barracks), "market": (30, 26, market),
    "academy": (34, 36, academy), "guild": (40, 38, guild), "tower": (20, 44, tower), "farm": (30, 32, farm),
    "castle": (84, 84, castle_grand), "cathedral": (50, 66, cathedral), "guild_grand": (60, 50, guild_grand), "mansion": (36, 34, stone_variant(mansion, "mansion")), "house2": (26, 32, stone_variant(house2, "house2")), "stall": (24, 22, stall),
    "granary": (28, 30, granary), "rice_field": (32, 16, rice_field), "veg_plot": (28, 16, veg_plot), "bank": (32, 30, bank),
    "school": (34, 30, stone_variant(school, "school")), "tavern": (34, 32, stone_variant(tavern, "tavern")), "forge": (34, 26, stone_variant(forge, "forge")), "healer": (28, 26, stone_variant(healer, "healer")),
    "well": (16, 18, well), "stable": (36, 22, stable), "gate": (40, 36, gate),
    "house3": (32, 26, house3), "shack": (20, 18, shack), "rowhouse": (40, 28, rowhouse),
    "bakery": (32, 26, bakery), "fishery": (28, 22, fishery), "lumber_mill": (34, 22, lumber_mill), "mine": (32, 26, mine),
    "quarry": (32, 20, quarry), "tailor": (32, 26, tailor), "kiln": (26, 26, kiln), "brewery": (34, 26, brewery),
    "orchard": (32, 22, orchard), "sheep_pen": (30, 18, sheep_pen), "chicken_coop": (26, 18, chicken_coop),
    "fountain": (24, 22, fountain), "cemetery": (32, 20, cemetery), "bathhouse": (32, 26, bathhouse), "courthouse": (34, 32, courthouse),
    "jail": (30, 22, jail), "lighthouse": (20, 42, lighthouse), "city_wall": (32, 30, city_wall), "archery": (34, 22, archery),
    "alchemist": (30, 26, alchemist), "shrine": (24, 22, shrine),
}


def save_frames(bid, main):
    d = os.path.join(OUT, bid); os.makedirs(d, exist_ok=True)
    main.save(os.path.join(d, "main_0.png"))
    construction(main).save(os.path.join(d, "construction_0.png"))
    ruin(main, hash(bid) & 0xffff).save(os.path.join(d, "ruin_0.png"))
    mini(main).save(os.path.join(d, "mini_0.png"))


def build_tech(write=True, only=None):
    prev = []
    for bid, (era, w, h, fn) in TECH.items():
        if only and only not in bid: continue
        b = B(w, h, seed=hash(bid) & 0xffff)
        fn(b, ERAS[era], era)
        prev.append(b.img)
        if write: save_frames(bid, b.img)
    return prev


def build(write=True, only=None):
    prev = []
    for era, E in ERAS.items():
        for tname, (w, h, fn) in TYPES.items():
            bid = "%s_%s" % (era, tname)
            if only and only not in bid: continue
            b = B(w, h, seed=hash(bid) & 0xffff)
            fn(b, E, era)
            main = b.img
            prev.append(main)
            if write:
                d = os.path.join(OUT, bid); os.makedirs(d, exist_ok=True)
                main.save(os.path.join(d, "main_0.png"))
                construction(main).save(os.path.join(d, "construction_0.png"))
                ruin(main, hash(bid) & 0xffff).save(os.path.join(d, "ruin_0.png"))
                mini(main).save(os.path.join(d, "mini_0.png"))
    return prev


if __name__ == "__main__":
    imgs = build(write="preview" not in sys.argv)
    if "preview" not in sys.argv:   # อาคารหลักของเมืองใช้สถาปัตยกรรมแยกยุค (art7) ทับ
        import art7_eras; art7_eras.build(write=True)
        import art8_shops; art8_shops.build(write=True)
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(ROOT, "previews", "buildings_eras.png")
    sheet(imgs, out, scale=3, cols=10, bg=(74, 112, 52, 255))
    print("ok", len(imgs), out)
