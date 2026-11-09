# ไอคอนโหนด Skill Tree 81 ช่อง (9 สาย × [ใช้งาน 4 + ติดตัว 4 + สุดยอด 1]) + ไอคอนกุญแจล็อก
# บันทึก: GameResources/ui/Icons/ngd/sk_<branch>_<a1..a4|p1..p4|m>.png (28x28) · lock.png (12x12)
import os, sys
from art2_core import Canvas, sheet
from art2_gear import weapon_icon, emblem, R, quad
import art2_ui as U

OUT = U.NGD

BR = {  # สี, ตราประจำสาย
    "blade": ("#ffd24a", "star"), "martial": ("#ff8a3a", "flame"), "archery": ("#9aff7a", "swirl"),
    "arcane": ("#7ab0ff", "star"), "holy": ("#fff0a0", "sun"), "shadow": ("#c07aff", "eye"),
    "nature": ("#7ae05a", "leaf"), "command": ("#ff5a5a", "sun"), "beast": ("#ffb04a", "claw"),
}


def W(kind, metal="#c8d0dc", trim="#c8a040", acc="#ffd24a"):
    return lambda cv: weapon_icon(cv, "sk", (metal, trim, acc, kind, set(), "sun"))


def E(name, cx, cy, s, col):
    return lambda cv: emblem(cv, name, cx, cy, s, col)


def I(name):
    return lambda cv: U.ICONS[name][0](cv)


def heart(col="#ff4a5a"): return lambda cv: U.heart(cv, 14, 15, 9, col)
def arrow(col): return lambda cv: U.arrow_up(cv, 14, 14, 10, col)
def hour(col): return lambda cv: U.hourglass(cv, 14, 14, 8, col)
def star(col, r=11): return lambda cv: U.star(cv, 14, 14, r, col)
def plus(col): return lambda cv: U.plus(cv, 14, 14, 9, col)
def book(col): return lambda cv: U.book(cv, 14, 15, 9, col, True)
def glow(col): return lambda cv: cv.glow(col, 60, 1)


def shield(col, emb=None, ecol="#ffffff"):
    def f(cv):
        m = cv.mask(); m.poly([(5, 4), (23, 4), (23, 15), (14, 25), (5, 15)]); cv.part(m, R(col))
        if emb: emblem(cv, emb, 14, 13, 5, ecol)
    return f


def banner(col):
    def f(cv):
        cv.line([(7, 3), (7, 26)], (120, 80, 40), 2)
        m = cv.mask(); m.poly([(8, 4), (24, 6), (20, 11), (24, 16), (8, 15)]); cv.part(m, R(col))
        emblem(cv, "sun", 14, 10, 3, "#ffd24a")
    return f


def crown(col="#f2c040"):
    def f(cv):
        m = cv.mask(); m.poly([(3, 22), (3, 9), (9, 15), (14, 6), (19, 15), (25, 9), (25, 22)]); cv.part(m, R(col))
        for x, c in ((8, "#ff4a5a"), (14, "#4ab0ff"), (20, "#4ae07a")):
            m = cv.mask(); m.ell(x - 1.6, 16, x + 1.6, 19.2); cv.part(m, R(c), outline=False)
    return f


def mini(name, col):   # ตราสายเล็กมุมขวาบน
    return lambda cv: emblem(cv, name, 22, 6, 3.2, col)


STONE = {1: "#8a5a3a", 2: "#8a96a8", 3: "#c8a040", 4: "#6ad0e0"}   # หินรูน: ทองแดง เงิน ทอง ผลึก ตามขั้น


def rune(br, t, kind):
    col, emb = BR[br]
    def f(cv):
        m = cv.mask(); m.poly([(6, 2), (22, 2), (26, 6), (26, 22), (22, 26), (6, 26), (2, 22), (2, 6)]); cv.part(m, R(STONE[t]))
        m = cv.mask(); m.poly([(7, 5), (21, 5), (23, 7), (23, 21), (21, 23), (7, 23), (5, 21), (5, 7)]); cv.part(m, R("#2a2438"), outline=False)
        emblem(cv, emb, 13, 13, 6.5, col)
        if kind == "hp": U.heart(cv, 21, 21, 4, "#ff4a5a")
        elif kind == "dmg": U.arrow_up(cv, 21, 21, 4, "#ff6a4a")
        elif kind == "crit": emblem(cv, "eye", 21, 21, 3.6, "#ffd24a")
        else: U.hourglass(cv, 21, 21, 3.2, "#8ad8ff")
    return f


def ptype(br, t):
    if t == 1: return "hp" if br in ("holy", "nature", "beast") else "dmg"
    if t == 2: return "crit" if br in ("shadow", "archery", "blade") else "hp"
    if t == 3: return "cd"
    return "dmg"


ACT = {
    "blade": [[W("holy_sword")], [W("katana"), E("swirl", 21, 7, 4, "#ffd24a")], [W("katana", acc="#9affc8"), E("wave", 20, 20, 5, "#9affc8")],
              [W("holy_sword", trim="#ffd24a", acc="#ffffff"), E("star", 20, 7, 5, "#fff27a"), glow("#ffd24a")]],
    "martial": [[I("s_str")], [E("bolt", 14, 14, 10, "#fff27a")], [E("swirl", 14, 14, 10, "#9affc8")], [E("mountain", 14, 15, 10, "#c8964a"), I("s_str")]],
    "archery": [[W("wind_bow")], [W("wind_bow", acc="#ff8a3a"), E("star", 21, 21, 3, "#ffd24a")], [E("bolt", 9, 9, 5, "#9aff7a"), E("bolt", 19, 9, 5, "#9aff7a"), E("bolt", 14, 19, 5, "#9aff7a")],
                [W("moon_bow", metal="#d8e8ff", acc="#8ad8ff"), glow("#8ad8ff")]],
    "arcane": [[E("star", 14, 14, 9, "#7ab0ff"), glow("#7ab0ff")], [W("star_staff", metal="#2a3478", trim="#d0dcff", acc="#8fb4ff")],
               [E("swirl", 14, 14, 11, "#b080ff"), E("bolt", 14, 14, 5, "#fff27a")], [E("eye", 14, 14, 10, "#ff5aff"), glow("#ff5aff")]],
    "holy": [[E("sun", 14, 14, 10, "#fff0a0")], [heart("#ffb0c0"), plus("#ffffff")], [E("sun", 14, 14, 11, "#ffd24a"), E("star", 14, 14, 4, "#ffffff")],
             [W("holy_sword", trim="#f2c040", acc="#fff0a0"), E("bolt", 20, 7, 5, "#fff27a"), glow("#fff0a0")]],
    "shadow": [[W("plague_dagger", acc="#c07aff")], [E("moon", 14, 14, 10, "#c07aff"), E("swirl", 20, 20, 4, "#6a44a0")],
               [E("skull", 14, 14, 9, "#9aff4a"), E("drop", 21, 21, 4, "#9aff4a")], [E("skull", 14, 14, 10, "#c07aff"), glow("#c07aff")]],
    "nature": [[E("leaf", 14, 14, 10, "#7ae05a")], [W("vine_hammer", metal="#6e4c2c", trim="#5ac85a", acc="#c8ff6a")],
               [E("flower", 14, 14, 10, "#c8ff6a"), E("leaf", 21, 21, 4, "#5ac85a")], [E("leaf", 14, 14, 11, "#5ac85a"), E("bolt", 14, 14, 5, "#ffd24a"), glow("#7ae05a")]],
    "command": [[I("s_cha")], [shield("#46508e", "star", "#ffd24a")], [banner("#ff5a5a")], [banner("#ffd24a"), glow("#ffd24a")]],
    "beast": [[E("claw", 14, 14, 10, "#ffb04a")], [W("dragon_axe", metal="#8a3a2a", trim="#c8c8c8", acc="#ff5a3a")],
              [E("eye", 14, 14, 9, "#ff5a3a"), E("bolt", 21, 7, 4, "#ffb04a")], [E("claw", 14, 14, 11, "#ff3a4a"), E("flame", 21, 21, 4, "#ffb04a"), glow("#ff3a4a")]],
}


def mastery(br):
    col, emb = BR[br]
    def f(cv):
        m = cv.mask(); m.ell(2, 2, 26, 26); cv.part(m, R("#2a2438"))
        emblem(cv, emb, 14, 15, 9, col)
        m = cv.mask(); m.poly([(8, 7), (8, 2), (11, 5), (14, 1), (17, 5), (20, 2), (20, 7)]); cv.part(m, R("#f2c040"))
        cv.glow(col, 70, 1)
        cv.sparkle(24, 24)
    return f


def lock(cv):
    m = cv.mask(); m.rect(2, 5, 10, 11); cv.part(m, R("#c8a040"))
    m = cv.mask(); m.ell(3, 0, 9, 8); m.cut_ell(4.5, 1.5, 7.5, 7); m.cut_rect(0, 5, 12, 12); cv.part(m, R("#a8a8b8"))
    cv.dot(6, 8, (40, 30, 20))


def build(write=True):
    prev = []
    for br in BR:
        nodes = []
        for t in range(1, 5):
            nodes.append(("%s_a%d" % (br, t), ACT[br][t - 1]))
            nodes.append(("%s_p%d" % (br, t), [rune(br, t, ptype(br, t))]))
        nodes.append(("%s_m" % br, [mastery(br)]))
        for nid, ops in nodes:
            cv = Canvas(28, 28)
            for op in ops:
                if op: op(cv)
            prev.append(cv.img)
            if write: cv.save(os.path.join(OUT, "sk_%s.png" % nid))
    lk = Canvas(12, 12); lock(lk)
    if write: lk.save(os.path.join(OUT, "lock.png"))
    return prev


if __name__ == "__main__":
    prev = build(write="preview" not in sys.argv)
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(os.environ.get("TEMP", "."), "skills_icons.png")
    sheet(prev, out, scale=4, cols=9)
    print("ok", len(prev), out)
