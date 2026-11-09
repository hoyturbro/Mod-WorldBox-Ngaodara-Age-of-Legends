# เซ็ตอุปกรณ์ 16 ธีม × 6 ชิ้น (ไอคอน 28px + อาวุธในมือ 16px) + ตราธาตุ e_* 32px
# ใช้: python art2_gear.py [preview]
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from art2_core import Canvas, Mask, ramp, hx, diag, sheet

ROOT = os.path.dirname(os.path.abspath(__file__))
RES = os.path.join(ROOT, "NgaodaraPack", "GameResources")
ICON_DIR = os.path.join(RES, "ui", "Icons", "items")
HAND_DIR = os.path.join(RES, "items", "weapons")
NGD_DIR = os.path.join(RES, "ui", "Icons", "ngd")

# id: (โลหะหลัก, ขอบ/โลหะรอง, สีเรือง/อัญมณี, อาวุธ, ของตกแต่ง, ตรา)
THEMES = {
    "fire":      ("#6a2a1c", "#e0702a", "#ffb03a", "flame_sword", {"horns", "lava"}, "flame"),
    "ice":       ("#6aa8e0", "#eaf8ff", "#9af0ff", "ice_staff", {"crystal"}, "snow"),
    "thunder":   ("#46508e", "#ffd84a", "#fff27a", "bolt_spear", {"bolt", "plume"}, "bolt"),
    "wind":      ("#48b890", "#eafff4", "#b8ffe4", "wind_bow", {"feather"}, "swirl"),
    "earth":     ("#86684a", "#6a9a4a", "#ffc860", "rock_hammer", {"rock"}, "mountain"),
    "light":     ("#ece6d0", "#f2c040", "#8ad8ff", "holy_sword", {"wings", "halo"}, "sun"),
    "dark":      ("#2c2440", "#6a44a0", "#c07aff", "void_scythe", {"horns", "hood"}, "eye"),
    "star":      ("#2a3478", "#d0dcff", "#8fb4ff", "star_staff", {"star"}, "star"),
    "blood":     ("#6a121c", "#2a1a20", "#ff3a4a", "blood_sword", {"spikes", "horns"}, "drop"),
    "plague":    ("#55553a", "#2e2c26", "#9aff4a", "plague_dagger", {"beak"}, "skull"),
    "abyss":     ("#246a8a", "#80e0d8", "#4affe8", "trident", {"fins", "scales"}, "wave"),
    "clockwork": ("#b07e34", "#6a4426", "#5af0ff", "chainsword", {"gears", "goggles"}, "gear"),
    "dragon":    ("#2e6a46", "#e6b440", "#ffd24a", "dragon_axe", {"horns", "scales", "spikes"}, "claw"),
    "nature":    ("#6e4c2c", "#5ac85a", "#c8ff6a", "vine_hammer", {"leaves"}, "leaf"),
    "sakura":    ("#f2b4c6", "#2a2030", "#ff7aa8", "katana", {"kabuto", "petals"}, "flower"),
    "moon":      ("#9aa6c8", "#3c3c70", "#e8eeff", "moon_bow", {"crescent"}, "moon"),
    "void":      ("#1c1428", "#4a2a6a", "#ff4aff", "void_scythe", {"hood", "star"}, "eye"),
    "phoenix":   ("#b8361a", "#ffd24a", "#ff8a3a", "flame_sword", {"wings", "plume"}, "flame"),
    "crystal":   ("#7a5ac8", "#ece0ff", "#ff9aff", "ice_staff", {"crystal", "halo"}, "star"),
    "titan":     ("#4a4e5a", "#d8a040", "#ff6a2a", "rock_hammer", {"spikes", "horns"}, "mountain"),
}
PIECES = ["weapon", "helmet", "armor", "boots", "ring", "amulet"]


def R(c, spread=1.0):
    return ramp(c, spread=spread)


# ---------------------------------------------------------------- ตรา (ใช้ทั้งไอคอนธาตุ จี้ อัญมณี ลายอก)
def emblem(cv, kind, cx, cy, s, col):
    rp = R(col, 0.8)
    m = cv.mask()
    k = s / 6.0
    P = lambda pts: [(cx + x * k, cy + y * k) for x, y in pts]
    if kind == "flame":
        m.poly(P([(0, -6), (3, -1), (4, 2), (2, 5), (-2, 5), (-4, 2), (-3, -1), (-1, 0)]))
    elif kind == "snow":
        for a in range(0, 180, 60):
            ax, ay = math.cos(math.radians(a)) * 5.5 * k, math.sin(math.radians(a)) * 5.5 * k
            m.line([(cx - ax, cy - ay), (cx + ax, cy + ay)], max(1, int(1.6 * k)))
        m.ell(cx - 1.5 * k, cy - 1.5 * k, cx + 1.5 * k, cy + 1.5 * k)
    elif kind == "bolt":
        m.poly(P([(1, -6), (-3, 0), (0, 0), (-2, 6), (4, -1), (1, -1), (3, -6)]))
    elif kind == "swirl":
        m.pie((cx - 5 * k, cy - 5 * k, cx + 5 * k, cy + 5 * k), 180, 450)
        m.cut_ell(cx - 3 * k, cy - 3 * k, cx + 3 * k, cy + 3 * k)
        m.ell(cx - 1.5 * k, cy - 1.5 * k, cx + 1.5 * k, cy + 1.5 * k)
    elif kind == "mountain":
        m.poly(P([(-6, 5), (-2, -3), (0, 0), (2, -5), (6, 5)]))
    elif kind == "sun":
        for a in range(0, 360, 45):
            m.line([(cx, cy), (cx + math.cos(math.radians(a)) * 6 * k, cy + math.sin(math.radians(a)) * 6 * k)], max(1, int(1.4 * k)))
        m.ell(cx - 3.2 * k, cy - 3.2 * k, cx + 3.2 * k, cy + 3.2 * k)
    elif kind == "eye":
        m.ell(cx - 6 * k, cy - 3.5 * k, cx + 6 * k, cy + 3.5 * k)
    elif kind == "star":
        pts = []
        for i in range(10):
            r = 6 if i % 2 == 0 else 2.6
            a = math.radians(-90 + i * 36)
            pts.append((math.cos(a) * r, math.sin(a) * r))
        m.poly(P(pts))
    elif kind == "drop":
        m.ell(cx - 4 * k, cy - 1 * k, cx + 4 * k, cy + 6 * k); m.poly(P([(-3.4, 1), (0, -6), (3.4, 1)]))
    elif kind == "skull":
        m.ell(cx - 5 * k, cy - 5.5 * k, cx + 5 * k, cy + 3 * k); m.rect(cx - 3 * k, cy + 1 * k, cx + 3 * k, cy + 5.5 * k)
    elif kind == "wave":
        m.pie((cx - 6 * k, cy - 6 * k, cx + 6 * k, cy + 6 * k), 180, 360); m.rect(cx - 6 * k, cy, cx + 6 * k, cy + 3 * k)
        m.cut_ell(cx - 1 * k, cy - 4 * k, cx + 7 * k, cy + 3 * k)
    elif kind == "gear":
        for a in range(0, 360, 45):
            ax, ay = math.cos(math.radians(a)) * 6 * k, math.sin(math.radians(a)) * 6 * k
            m.ell(cx + ax - 1.4 * k, cy + ay - 1.4 * k, cx + ax + 1.4 * k, cy + ay + 1.4 * k)
        m.ell(cx - 4.8 * k, cy - 4.8 * k, cx + 4.8 * k, cy + 4.8 * k)
    elif kind == "claw":
        for dx in (-3.5, 0, 3.5):
            m.poly(P([(dx - 1.6, 5), (dx + 1.6, 5), (dx + 2.4, -2), (dx, -6.5)]))
    elif kind == "leaf":
        m.ell(cx - 3.5 * k, cy - 6 * k, cx + 3.5 * k, cy + 6 * k)
    elif kind == "flower":
        for a in range(0, 360, 72):
            ax, ay = math.cos(math.radians(a - 90)) * 3.5 * k, math.sin(math.radians(a - 90)) * 3.5 * k
            m.ell(cx + ax - 2.6 * k, cy + ay - 2.6 * k, cx + ax + 2.6 * k, cy + ay + 2.6 * k)
    elif kind == "moon":
        m.ell(cx - 5.5 * k, cy - 5.5 * k, cx + 5.5 * k, cy + 5.5 * k); m.cut_ell(cx - 2 * k, cy - 7 * k, cx + 8 * k, cy + 3.5 * k)
    cv.part(m, rp)
    # รายละเอียดเพิ่ม
    if kind == "flame":
        m2 = cv.mask(); m2.poly(P([(0, -1), (1.6, 2), (0, 4.2), (-1.6, 2)])); cv.part(m2, R("#fff3a0"), outline=False, flat=True)
    if kind == "eye":
        m2 = cv.mask(); m2.ell(cx - 2.2 * k, cy - 2.2 * k, cx + 2.2 * k, cy + 2.2 * k); cv.part(m2, R("#14081e"), outline=False, flat=True)
        cv.dot(int(cx - 0.6 * k), int(cy - 0.8 * k), (255, 255, 255))
    if kind == "skull":
        for dx in (-2, 2):
            m2 = cv.mask(); m2.ell(cx + (dx - 1.4) * k, cy - 2.2 * k, cx + (dx + 1.4) * k, cy + 0.6 * k); cv.part(m2, R("#18200e"), outline=False, flat=True)
    if kind == "gear":
        m2 = cv.mask(); m2.ell(cx - 1.8 * k, cy - 1.8 * k, cx + 1.8 * k, cy + 1.8 * k); cv.part(m2, R("#2a1a10"), outline=False, flat=True)
    if kind == "flower":
        m2 = cv.mask(); m2.ell(cx - 1.4 * k, cy - 1.4 * k, cx + 1.4 * k, cy + 1.4 * k); cv.part(m2, R("#ffe070"), outline=False, flat=True)
    if kind == "leaf":
        cv.line([(cx, cy - 5 * k), (cx, cy + 5 * k)], rp[1][:3])


# ---------------------------------------------------------------- อาวุธ (ไอคอน 28 เอียง 45°)
def quad(m, cx, cy, a0, a1, w0, w1=None, ang=-45):
    w1 = w0 if w1 is None else w1
    m.poly([diag(cx, cy, a0, -w0, ang), diag(cx, cy, a1, -w1, ang), diag(cx, cy, a1, w1, ang), diag(cx, cy, a0, w0, ang)])


def weapon_icon(cv, tid, T):
    metal, trim, acc, kind, deco, emb = T
    C = (14, 14)
    blade = R(metal if kind in ("chainsword", "dragon_axe", "rock_hammer", "vine_hammer") else trim if kind in ("holy_sword",) else "#c8d0dc")
    if kind in ("flame_sword",): blade = R("#ff7a3a")
    if kind in ("blood_sword",): blade = R("#b01828")
    if kind in ("plague_dagger",): blade = R("#8a9a6a")
    if kind in ("katana",): blade = R("#e8eef6")
    if kind in ("ice_staff", "star_staff"): blade = R(metal)
    grip = R("#4a2c1c")
    gem = R(acc, 0.7)
    tr = R(trim)
    if kind in ("flame_sword", "holy_sword", "blood_sword", "plague_dagger", "katana", "chainsword"):
        L = {"plague_dagger": 8, "katana": 13, "chainsword": 12}.get(kind, 12)
        w = {"chainsword": 3.6, "katana": 2.0, "plague_dagger": 3.0}.get(kind, 3.0)
        C = (12, 16)
        m = cv.mask(); quad(m, *C, -1, L - 2, w, w * 0.9); m.poly([diag(*C, L - 2, -w * 0.9), diag(*C, L + 2, 0), diag(*C, L - 2, w * 0.9)])
        if kind == "flame_sword":
            for t in range(1, L, 3): m.poly([diag(*C, t, w), diag(*C, t + 1.5, w + 1.6), diag(*C, t + 2.5, w)])
        if kind == "blood_sword":
            for t in range(1, L, 3): m.poly([diag(*C, t, -w), diag(*C, t + 1, -w - 1.8), diag(*C, t + 2, -w)])
        cv.part(m, blade)
        if kind == "chainsword":
            for t in range(0, L, 2): cv.dot(*map(int, diag(*C, t, w + 0.6)), R("#e0e0e0")[3])
            m2 = cv.mask(); quad(m2, *C, 0, L - 2, 1.0); cv.part(m2, R("#3a3a3a"), outline=False, flat=True)
        else:
            cv.line([diag(*C, 0, 0), diag(*C, L, 0)], blade[4][:3])  # สันคม
        # การ์ด
        m = cv.mask()
        if "wings" in deco:
            m.poly([diag(*C, -1, -2), diag(*C, 2, -8), diag(*C, -3, -6)]); m.poly([diag(*C, -1, 2), diag(*C, 2, 8), diag(*C, -3, 6)])
        elif "horns" in deco or "spikes" in deco:
            m.poly([diag(*C, -2, -2), diag(*C, 3, -7), diag(*C, -1, -5)]); m.poly([diag(*C, -2, 2), diag(*C, 3, 7), diag(*C, -1, 5)])
        elif kind == "katana":
            m.ell(*diag(*C, -2.5, -2.6), *diag(*C, -0.5, 2.6)) if False else m.ell(C[0] + diag(0, 0, -1.5, 0)[0] - 2.5, C[1] + diag(0, 0, -1.5, 0)[1] - 2.5, C[0] + diag(0, 0, -1.5, 0)[0] + 2.5, C[1] + diag(0, 0, -1.5, 0)[1] + 2.5)
        elif "gears" in deco:
            gx, gy = diag(*C, -2, 0); m.ell(gx - 3.5, gy - 3.5, gx + 3.5, gy + 3.5)
        else:
            quad(m, *C, -2.8, -1, 5)
        cv.part(m, tr)
        m = cv.mask(); quad(m, *C, -9, -2.6, 1.1); cv.part(m, grip)
        px, py = diag(*C, -10, 0); m = cv.mask(); m.ell(px - 1.8, py - 1.8, px + 1.8, py + 1.8); cv.part(m, gem)
        gx, gy = diag(*C, -1.8, 0); cv.dot(int(gx), int(gy), gem[3])
    elif kind in ("void_scythe", "dragon_axe"):
        C = (13, 15)
        m = cv.mask(); quad(m, *C, -12, 10, 1.2); cv.part(m, R("#3a2a22") if kind == "dragon_axe" else R("#2a2030"))
        m = cv.mask()
        if kind == "void_scythe":
            hx0, hy0 = diag(*C, 9, 0)
            m.ell(hx0 - 12, hy0 - 3, hx0 + 4, hy0 + 11); m.cut_ell(hx0 - 11, hy0 - 1, hx0 + 4, hy0 + 14)
            m.cut_rect(hx0 + 1, hy0 - 4, hx0 + 6, hy0 + 14)
        else:
            m.poly([diag(*C, 2, -1), diag(*C, -1, -9), diag(*C, 5, -12), diag(*C, 12, -9), diag(*C, 10, -1)])
            m.poly([diag(*C, 3, 1), diag(*C, 2, 7), diag(*C, 7, 8), diag(*C, 9, 1)])
        cv.part(m, R(metal if kind == "dragon_axe" else "#3a2a5a"))
        if kind == "void_scythe":
            hx0, hy0 = diag(*C, 9, 0); cv.line([(hx0 - 10, hy0 + 4), (hx0 - 2, hy0 - 1)], gem[3][:3])
        else:
            m = cv.mask(); quad(m, *C, 4, 8, 1.4); cv.part(m, tr)
        ex, ey = diag(*C, 7, 0); m = cv.mask(); m.ell(ex - 1.6, ey - 1.6, ex + 1.6, ey + 1.6); cv.part(m, gem)
    elif kind in ("bolt_spear", "trident"):
        m = cv.mask(); quad(m, *C, -13, 6, 0.9); cv.part(m, R("#5a3a24") if kind == "trident" else R(metal))
        m = cv.mask()
        if kind == "trident":
            for off in (-3.2, 0, 3.2):
                quad(m, *C, 6, 11 if off else 13, 0.8); m.poly([diag(*C, 6, off - 0.9), diag(*C, 12 if off else 13, off), diag(*C, 6, off + 0.9)])
                quad(m, *C, 6, 11.5, 0.8) if False else None
                m.poly([diag(*C, 5, off - 0.9), diag(*C, 11 if off else 13.5, off), diag(*C, 5, off + 0.9)])
            quad(m, *C, 4.5, 6.5, 3.8)
        else:
            m.poly([diag(*C, 5, -1.6), diag(*C, 8, -2.6), diag(*C, 9, 0), diag(*C, 11, -1.8), diag(*C, 14, 0.6), diag(*C, 10, 1.4), diag(*C, 9, 2.6), diag(*C, 5, 1.6)])
        cv.part(m, R(trim) if kind == "trident" else gem)
        bx, by = diag(*C, 4.5, 0); m = cv.mask(); m.ell(bx - 1.8, by - 1.8, bx + 1.8, by + 1.8); cv.part(m, gem if kind == "trident" else tr)
    elif kind in ("rock_hammer", "vine_hammer"):
        m = cv.mask(); quad(m, *C, -12, 5, 1.1); cv.part(m, R("#5a3a22"))
        m = cv.mask(); quad(m, *C, 3, 11, 7.2, 6.2); cv.part(m, R(metal))
        if kind == "rock_hammer":
            for t, o in ((5, -3), (7, 2), (8, -1)): cv.dot(*map(int, diag(*C, t, o)), R(acc)[3])
            m = cv.mask(); quad(m, *C, 5.5, 7.5, 6.6); cv.part(m, tr)
        else:
            for t in (4, 6, 8):
                lx, ly = diag(*C, t, 6.5); m = cv.mask(); m.ell(lx - 1.5, ly - 1.2, lx + 1.8, ly + 1.6); cv.part(m, R(trim))
            cv.line([diag(*C, 3, -5), diag(*C, 9, 4)], R(trim)[2][:3])
        gx, gy = diag(*C, 6.5, 0); m = cv.mask(); m.ell(gx - 1.6, gy - 1.6, gx + 1.6, gy + 1.6); cv.part(m, gem)
    elif kind in ("wind_bow", "moon_bow"):
        m = cv.mask()
        m.ell(1, 1, 31, 31); m.cut_ell(3.5, 3.5, 33, 33)
        m.cut_poly([(0, 28), (28, 0), (28, 28)])
        m.cut_rect(0, 0, 3, 3)
        cv.part(m, R(metal if kind == "moon_bow" else "#6a4a2a"))
        cv.line([(5, 22), (22, 5)], (235, 235, 220))
        for (x, y) in ((4, 21), (21, 4)):
            mm = cv.mask(); mm.ell(x - 1.6, y - 1.6, x + 2.6, y + 2.6); cv.part(mm, gem)
        if kind == "moon_bow":
            mm = cv.mask(); mm.ell(5, 5, 13, 13); mm.cut_ell(7, 3, 15, 11); cv.part(mm, R(acc))
        else:
            for (x, y) in ((8, 6), (6, 9)):
                mm = cv.mask(); mm.poly([(x, y), (x + 4, y - 2), (x + 2, y + 2)]); cv.part(mm, R(trim))
        # ลูกธนู
        cv.line([(9, 18), (19, 8)], (180, 150, 110))
        cv.dot(19, 8, gem[4]); cv.dot(20, 7, gem[3])
    elif kind in ("ice_staff", "star_staff"):
        m = cv.mask(); quad(m, *C, -13, 6, 1.0); cv.part(m, R("#4a3a5a" if kind == "star_staff" else "#d8e8f4"))
        hx0, hy0 = diag(*C, 9, 0)
        if kind == "ice_staff":
            m = cv.mask(); m.poly([(hx0, hy0 - 7), (hx0 + 3, hy0), (hx0, hy0 + 3), (hx0 - 3, hy0)])
            m.poly([(hx0 - 4, hy0 - 3), (hx0 - 2, hy0 + 1), (hx0 - 5, hy0 + 1)]); m.poly([(hx0 + 4, hy0 - 3), (hx0 + 5, hy0 + 1), (hx0 + 2, hy0 + 1)])
            cv.part(m, gem)
        else:
            m = cv.mask(); m.ell(hx0 - 5, hy0 - 5, hx0 + 5, hy0 + 5); m.cut_ell(hx0 - 3.5, hy0 - 3.5, hx0 + 3.5, hy0 + 3.5); cv.part(m, tr)
            emblem(cv, "star", hx0, hy0, 4, acc)
        cv.sparkle(int(hx0) + 3, int(hy0) - 5)


# ---------------------------------------------------------------- หมวก
def helmet_icon(cv, tid, T):
    metal, trim, acc, kind, deco, emb = T
    M, Tr, A = R(metal), R(trim), R(acc, 0.7)
    if "beak" in deco:  # หน้ากากหมอโรคระบาด
        m = cv.mask(); m.ell(5, 3, 21, 21); m.rect(5, 12, 21, 23); cv.part(m, R("#2e2c26"))
        m = cv.mask(); m.poly([(12, 12), (25, 22), (23, 24), (11, 18)]); cv.part(m, R("#d8ccaa"))
        for x in (9, 15):
            m = cv.mask(); m.ell(x - 2.5, 9, x + 2.5, 14); cv.part(m, A)
        m = cv.mask(); m.ell(3, 2, 23, 7); cv.part(m, R("#3a3830"))
        return
    if "goggles" in deco:
        m = cv.mask(); m.ell(5, 4, 23, 24); m.cut_rect(0, 18, 28, 28); m.rect(5, 14, 23, 19); cv.part(m, R("#6a4426"))
        m = cv.mask(); m.rect(4, 11, 24, 14); cv.part(m, M)
        for x in (10, 18):
            m = cv.mask(); m.ell(x - 3.5, 8.5, x + 3.5, 15.5); cv.part(m, M)
            m = cv.mask(); m.ell(x - 2, 10, x + 2, 14); cv.part(m, A, outline=False)
        emblem(cv, "gear", 21, 21, 4, metal)
        return
    if "kabuto" in deco:
        m = cv.mask(); m.ell(6, 7, 22, 23); m.cut_rect(0, 17, 28, 28); cv.part(m, Tr)
        m = cv.mask(); m.poly([(2, 18), (8, 14), (20, 14), (26, 18), (24, 22), (4, 22)]); cv.part(m, M)
        m = cv.mask(); m.poly([(14, 10), (6, 2), (9, 1), (14, 7), (19, 1), (22, 2)]); cv.part(m, R("#f2c040"))
        emblem(cv, "flower", 14, 12, 3, acc)
        return
    if "hood" in deco:
        m = cv.mask(); m.poly([(14, 1), (24, 10), (25, 25), (3, 25), (4, 10)]); cv.part(m, M)
        m = cv.mask(); m.ell(8, 9, 20, 22); cv.part(m, R("#0c0814"), outline=False, flat=True)
        for x in (11, 17): cv.dot(x, 14, A[4]); cv.dot(x, 15, A[3])
        for side in (1, -1):
            hxs = 14 + side * 9
            m = cv.mask(); m.poly([(hxs, 9), (hxs + side * 4, 1), (hxs + side * 1, 7)]); cv.part(m, Tr)
        return
    # หมวกเกราะพื้นฐาน
    m = cv.mask(); m.ell(4, 3, 24, 25); m.cut_rect(0, 15, 28, 28); m.poly([(4, 13), (24, 13), (23, 22), (19, 26), (9, 26), (5, 22)])
    cv.part(m, M)
    m = cv.mask(); m.rect(7, 14, 21, 16); m.rect(13, 16, 15, 22); cv.part(m, R("#120a10"), outline=False, flat=True)
    m = cv.mask(); m.rect(4, 11, 24, 13); cv.part(m, Tr)
    m = cv.mask(); m.poly([(13, 3), (15, 3), (15, 11), (13, 11)]); cv.part(m, Tr, outline=False)
    for x, y in ((7, 19), (21, 19), (6, 12), (22, 12)): cv.dot(x, y, Tr[4])
    if "horns" in deco:
        for side in (-1, 1):
            bx = 14 + side * 8
            m = cv.mask(); m.poly([(bx, 12), (bx + side * 5, 9), (bx + side * 7, 3), (bx + side * 6, -1), (bx + side * 4, 4), (bx + side * 1, 7), (bx - side * 1, 8)]); cv.part(m, R("#efe4c8") if tid != "dragon" else Tr)
    if "wings" in deco:
        for side in (-1, 1):
            bx = 14 + side * 9
            m = cv.mask(); m.poly([(bx, 12), (bx + side * 5, 3), (bx + side * 4, 9), (bx + side * 2, 14)]); cv.part(m, R("#ffffff"))
    if "halo" in deco:
        m = cv.mask(); m.ell(7, 0, 21, 5); m.cut_ell(9, 1.5, 19, 4); cv.part(m, R("#ffe27a"), outline=False)
    if "crystal" in deco:
        for x, h in ((9, 6), (14, 9), (19, 6)):
            m = cv.mask(); m.poly([(x - 2, 7), (x, 7 - h), (x + 2, 7)]); cv.part(m, A)
    if "plume" in deco:
        m = cv.mask(); m.poly([(14, 5), (22, 0), (26, 3), (20, 6)]); cv.part(m, A)
    if "fins" in deco:
        m = cv.mask(); m.poly([(10, 6), (15, -1), (20, 2), (18, 6)]); cv.part(m, Tr)
    if "leaves" in deco:
        for x, y in ((8, 5), (14, 2), (20, 5)):
            m = cv.mask(); m.ell(x - 2, y - 3, x + 2, y + 3); cv.part(m, Tr)
    if "rock" in deco:
        m = cv.mask(); m.poly([(6, 9), (9, 4), (12, 7)]); m.poly([(17, 6), (21, 4), (22, 9)]); cv.part(m, Tr)
    if "feather" in deco:
        for side in (-1, 1):
            m = cv.mask(); m.poly([(14 + side * 8, 12), (14 + side * 12, 3), (14 + side * 10, 12)]); cv.part(m, A)
    if "spikes" in deco and "horns" not in deco:
        for x in (9, 14, 19):
            m = cv.mask(); m.poly([(x - 1.5, 6), (x, 0), (x + 1.5, 6)]); cv.part(m, Tr)
    if "crescent" in deco:
        emblem(cv, "moon", 14, 8, 3.5, acc)
    elif "star" in deco:
        emblem(cv, "star", 14, 8, 3.5, acc)
    elif "bolt" in deco:
        emblem(cv, "bolt", 14, 9, 3.2, acc)
    elif "lava" in deco:
        cv.line([(7, 18), (10, 21), (12, 19)], hx(acc)); cv.line([(17, 20), (20, 17)], hx(acc))
    if "scales" in deco:
        for x in (8, 12, 16, 20): cv.dot(x, 19, M[3]); cv.dot(x + 1, 20, M[1])
    cv.dot(9, 7, (255, 255, 255))


# ---------------------------------------------------------------- เกราะอก
def armor_icon(cv, tid, T):
    metal, trim, acc, kind, deco, emb = T
    M, Tr, A = R(metal), R(trim), R(acc, 0.7)
    if "wings" in deco:
        for side in (-1, 1):
            m = cv.mask(); m.poly([(14 + side * 6, 10), (14 + side * 14, 2), (14 + side * 13, 12), (14 + side * 9, 18)]); cv.part(m, R("#ffffff"))
    m = cv.mask(); m.poly([(7, 6), (11, 4), (17, 4), (21, 6), (22, 14), (20, 16), (20, 25), (8, 25), (8, 16), (6, 14)])
    cv.part(m, M)
    m = cv.mask(); m.poly([(11, 4), (14, 8), (17, 4)]); cv.part(m, R("#140c10"), outline=False, flat=True)
    for side in (-1, 1):  # บ่า
        sx = 14 + side * 8
        m = cv.mask(); m.pie((sx - 6, 3, sx + 6, 15), 180, 360); m.rect(sx - 6, 8, sx + 6, 9); cv.part(m, Tr)
        m = cv.mask(); m.pie((sx - 5, 8, sx + 5, 16), 180, 360); cv.part(m, M)
        if "spikes" in deco or "horns" in deco:
            m = cv.mask(); m.poly([(sx - 1, 5), (sx + side * 4, -1), (sx + side * 1, 6)]); cv.part(m, R("#e8dcc0"))
        if "gears" in deco:
            emblem(cv, "gear", sx, 8, 2.6, metal)
        if "leaves" in deco:
            m = cv.mask(); m.ell(sx - 2, 1, sx + 2, 6); cv.part(m, Tr)
        if "crystal" in deco:
            m = cv.mask(); m.poly([(sx - 1.5, 5), (sx, -1), (sx + 1.5, 5)]); cv.part(m, A)
        if "fins" in deco:
            m = cv.mask(); m.poly([(sx, 4), (sx + side * 3, 0), (sx + side * 4, 5)]); cv.part(m, A)
    m = cv.mask(); m.rect(8, 19, 20, 21); cv.part(m, Tr, outline=False)
    cv.line([(9, 23), (19, 23)], M[1][:3])
    if "scales" in deco:
        for y in range(10, 18, 2):
            for x in range(10, 19, 3): cv.dot(x + (y // 2) % 2, y, M[1]); cv.dot(x + 1 + (y // 2) % 2, y, M[3])
    if "lava" in deco:
        cv.line([(9, 9), (11, 13), (10, 17)], hx(acc)); cv.line([(19, 10), (17, 14), (18, 17)], hx(acc))
    if "petals" in deco:
        for (x, y) in ((10, 14), (18, 12), (16, 22)): cv.dot(x, y, hx(acc)); cv.dot(x + 1, y, hx("#ffd0e0"))
    emblem(cv, emb, 14, 13, 4.2, acc)
    cv.dot(10, 8, (255, 255, 255))


# ---------------------------------------------------------------- รองเท้า
def boots_icon(cv, tid, T):
    metal, trim, acc, kind, deco, emb = T
    M, Tr, A = R(metal), R(trim), R(acc, 0.7)
    for i, (x0, y0) in enumerate(((13, 3), (5, 7))):  # ข้างหลังก่อน
        rp = R(metal, 0.85) if i == 0 else M
        m = cv.mask(); m.rect(x0 + 1, y0, x0 + 7, y0 + 13); m.poly([(x0 + 1, y0 + 13), (x0 + 11, y0 + 13), (x0 + 12, y0 + 18), (x0, y0 + 18)])
        cv.part(m, rp)
        m = cv.mask(); m.rect(x0, y0, x0 + 8, y0 + 3); cv.part(m, Tr)
        m = cv.mask(); m.rect(x0, y0 + 17, x0 + 12, y0 + 19); cv.part(m, R("#2a1c18"), outline=False)
        if "wings" in deco or "feather" in deco:
            m = cv.mask(); m.poly([(x0 + 1, y0 + 5), (x0 - 4, y0 + 1), (x0 - 2, y0 + 7)]); cv.part(m, R("#ffffff") if "wings" in deco else A)
        if "spikes" in deco or "horns" in deco:
            m = cv.mask(); m.poly([(x0 + 7, y0 + 5), (x0 + 11, y0 + 3), (x0 + 7, y0 + 8)]); cv.part(m, R("#e8dcc0"))
        if "lava" in deco:
            cv.line([(x0 + 3, y0 + 8), (x0 + 5, y0 + 12)], hx(acc))
        if "crystal" in deco:
            m = cv.mask(); m.poly([(x0 + 2, y0), (x0 + 4, y0 - 4), (x0 + 6, y0)]); cv.part(m, A)
        if "gears" in deco and i == 1:
            emblem(cv, "gear", x0 + 4, y0 + 9, 2.4, metal)
        if "leaves" in deco:
            m = cv.mask(); m.ell(x0 + 5, y0 - 3, x0 + 9, y0 + 1); cv.part(m, Tr)
        cv.dot(x0 + 4, y0 + 9, A[3])


# ---------------------------------------------------------------- แหวน / จี้
def ring_icon(cv, tid, T):
    metal, trim, acc, kind, deco, emb = T
    band = R(trim if tid not in ("light", "dragon", "sakura") else "#f2c040")
    if tid in ("sakura", "moon", "ice"): band = R("#d8dde8")
    m = cv.mask(); m.ell(5, 9, 23, 26); m.cut_ell(9, 13, 19, 22); cv.part(m, band)
    m = cv.mask(); m.poly([(9, 10), (14, 13), (19, 10), (14, 7)]); cv.part(m, band)
    emblem(cv, emb, 14, 7, 4.2, acc)
    cv.sparkle(18, 4)


def amulet_icon(cv, tid, T):
    metal, trim, acc, kind, deco, emb = T
    chain = R("#e0c060" if tid in ("light", "dragon", "fire", "clockwork") else "#c8ccd8")
    for i in range(0, 11):
        for x0 in (5, 23):
            x = x0 + (14 - x0) * i / 10.0; y = 1 + 10 * i / 10.0 - 3 * math.sin(math.pi * i / 10.0)
            cv.dot(int(round(x)), int(round(y)), chain[3] if i % 2 else chain[1])
    m = cv.mask(); m.ell(7, 10, 21, 25); cv.part(m, R(trim))
    m = cv.mask(); m.ell(9, 12, 19, 23); cv.part(m, R(metal), outline=False)
    emblem(cv, emb, 14, 17.5, 4.0, acc)
    cv.sparkle(19, 11)


# ---------------------------------------------------------------- อาวุธในมือ 16px (ตั้งตรง ด้ามอยู่ล่าง)
def weapon_hand(tid, T):
    metal, trim, acc, kind, deco, emb = T
    cv = Canvas(16, 16)
    tr, gem = R(trim), R(acc, 0.7)
    grip = R("#4a2c1c")
    if kind in ("flame_sword", "holy_sword", "blood_sword", "plague_dagger", "katana", "chainsword"):
        top = {"plague_dagger": 6, "katana": 0}.get(kind, 1)
        col = {"flame_sword": "#ff7a3a", "blood_sword": "#b01828", "plague_dagger": "#8a9a6a", "chainsword": metal, "holy_sword": trim}.get(kind, "#d0d8e2")
        w = 2 if kind == "chainsword" else 1
        m = cv.mask(); m.rect(8 - w, top + 1, 8 + w - (0 if w > 1 else 0), 11); m.poly([(8 - w, top + 1), (8, top - 1), (8 + w, top + 1)]); cv.part(m, R(col))
        m = cv.mask(); m.rect(5, 11, 11, 12); cv.part(m, tr)
        m = cv.mask(); m.rect(7, 13, 9, 15); cv.part(m, grip, outline=False)
        cv.dot(8, 15, gem[3])
    elif kind in ("void_scythe", "dragon_axe"):
        m = cv.mask(); m.rect(7, 2, 8, 15); cv.part(m, R("#3a2a22"), outline=False)
        m = cv.mask()
        if kind == "void_scythe": m.poly([(8, 1), (1, 3), (0, 7), (3, 4), (8, 4)])
        else: m.poly([(8, 2), (13, 0), (15, 5), (13, 9), (8, 7)])
        cv.part(m, R(metal if kind == "dragon_axe" else "#3a2a5a"))
        cv.dot(8, 4, gem[4])
    elif kind in ("bolt_spear", "trident"):
        m = cv.mask(); m.rect(7, 5, 8, 15); cv.part(m, R("#5a3a24"), outline=False)
        m = cv.mask()
        if kind == "trident": m.rect(4, 4, 11, 5); m.rect(4, 0, 5, 4); m.rect(7, 0, 8, 4); m.rect(10, 0, 11, 4)
        else: m.poly([(7, 0), (10, 3), (8, 4), (9, 6), (6, 5), (7, 3), (5, 3)])
        cv.part(m, R(trim) if kind == "trident" else gem)
    elif kind in ("rock_hammer", "vine_hammer"):
        m = cv.mask(); m.rect(7, 4, 8, 15); cv.part(m, R("#5a3a22"), outline=False)
        m = cv.mask(); m.rect(3, 0, 12, 5); cv.part(m, R(metal))
        cv.dot(7, 2, gem[4])
    elif kind in ("wind_bow", "moon_bow"):
        m = cv.mask(); m.ell(1, 0, 15, 16); m.cut_ell(3, 1, 17, 15); m.cut_rect(9, 0, 16, 16); cv.part(m, R(metal if kind == "moon_bow" else "#6a4a2a"))
        cv.line([(7, 1), (7, 14)], (230, 230, 220))
    else:  # staff
        m = cv.mask(); m.rect(7, 5, 8, 15); cv.part(m, R("#d8e8f4" if kind == "ice_staff" else "#4a3a5a"), outline=False)
        m = cv.mask(); m.ell(4, 0, 11, 7); cv.part(m, gem)
        cv.dot(6, 2, (255, 255, 255))
    return cv


DRAW = {"weapon": weapon_icon, "helmet": helmet_icon, "armor": armor_icon, "boots": boots_icon, "ring": ring_icon, "amulet": amulet_icon}


def emblem_icon(tid, T):
    cv = Canvas(32, 32)
    emblem(cv, T[5], 16, 16, 12, T[2])
    cv.glow(T[2], 50, 1)
    return cv


def build(write=True):
    os.makedirs(ICON_DIR, exist_ok=True); os.makedirs(HAND_DIR, exist_ok=True)
    previews = []
    for tid, T in THEMES.items():
        for piece in PIECES:
            cv = Canvas(28, 28)
            DRAW[piece](cv, tid, T)
            if piece in ("weapon", "ring", "amulet"):
                cv.glow(T[2], 40, 1)
            previews.append(cv.img)
            if write:
                cv.save(os.path.join(ICON_DIR, "icon_ngd_set_%s_%s.png" % (tid, piece)))
        # ภาพในมือ: art9_hand.py (ขนาดเท่าอาวุธของเกม)
        e = emblem_icon(tid, T)
        if write:
            e.save(os.path.join(NGD_DIR, "e_%s.png" % tid))
    return previews


if __name__ == "__main__":
    prev = build(write="preview" not in sys.argv)
    out = os.path.join(os.environ.get("TEMP", "."), "gear_preview.png")
    if len(sys.argv) > 2: out = sys.argv[2]
    sheet(prev, out, scale=4, cols=12)
    print("ok", len(prev), out)
