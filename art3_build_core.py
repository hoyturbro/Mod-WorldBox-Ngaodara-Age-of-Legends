# ตัววาดอาคารรุ่นใหม่: ลายพื้นผิวรายพิกเซล + แรเงา 5 โทน + เงาชายคา + ของประกอบ
import math, random
from PIL import Image, ImageDraw
from art2_core import ramp, hx

KC = (255, 0, 255, 255)  # สีอาณาจักร (เกมแทนสีให้)


# หลังคาสีอาณาจักร: เกมแทน 3 เฉดนี้ด้วยสีอาณาจักร (เข้ม/กลาง/สว่าง) เหมือนอาคารของเกมเอง
KROOF = [(74, 28, 58, 255), (167, 0, 167, 255), (222, 0, 222, 255), (255, 0, 255, 255), (255, 0, 255, 255)]


def R(c, s=1.0):
    if c == "KC":
        return KROOF
    return ramp(c, spread=s)


class B:
    """ผืนวาดอาคาร"""
    def __init__(self, w, h, seed=1):
        self.w, self.h = w, h
        self.img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        self.px = self.img.load()
        self.rnd = random.Random(seed)

    def mask(self):
        m = Image.new("L", (self.w, self.h), 0)
        return m, ImageDraw.Draw(m)

    def put(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.px[x, y] = c if len(c) == 4 else tuple(c) + (255,)

    def get(self, x, y):
        return self.px[x, y] if 0 <= x < self.w and 0 <= y < self.h else (0, 0, 0, 0)

    def fill(self, m, rp, tex=None, outline=True, light=True, keep=None):
        """ลงสีพื้นผิว: โทนจากแสง (บนซ้ายสว่าง ล่างขวามืด) + ออฟเซ็ตจากลาย tex(x,y)->int"""
        mp = m.load(); W, H = self.w, self.h
        ins = lambda x, y: 0 <= x < W and 0 <= y < H and mp[x, y] > 0
        for y in range(H):
            for x in range(W):
                if not mp[x, y]:
                    continue
                edge = not (ins(x - 1, y) and ins(x + 1, y) and ins(x, y - 1) and ins(x, y + 1))
                if edge and outline:
                    self.put(x, y, rp[0]); continue
                t = 2
                if light:
                    if not ins(x - 1, y - 1) or not ins(x, y - 2): t = 3
                    elif not ins(x + 1, y + 1) or not ins(x + 2, y): t = 1
                if tex:
                    t += tex(x, y)
                self.put(x, y, rp[max(1, min(4, t))])

    def line(self, pts, c, w=1):
        ImageDraw.Draw(self.img).line(pts, fill=tuple(c[:3]) + (c[3] if len(c) == 4 else 255,), width=w)
        self.px = self.img.load()

    def shade_band(self, x0, x1, y, alpha=110, rows=1):
        """เงาใต้ชายคา"""
        for r in range(rows):
            for x in range(x0, x1 + 1):
                c = self.get(x, y + r)
                if c[3]:
                    a = alpha // (r + 1)
                    self.put(x, y + r, (c[0] * (255 - a) // 255, c[1] * (255 - a) // 255, c[2] * (255 - a) // 255, c[3]))


# ---------------------------------------------------------------- ลายพื้นผิว
def tex_brick(rnd, row=3, brick=5):
    cache = {}
    def f(x, y):
        if y % row == 0: return -1
        off = (y // row) % 2 * (brick // 2)
        if (x + off) % brick == 0: return -1
        k = ((x + off) // brick, y // row)
        if k not in cache: cache[k] = rnd.choice((0, 0, 0, 1, -1))
        return cache[k] if rnd.random() > 0.08 else cache[k] + 1
    return f


def tex_stone(rnd):
    rows = {}; joints = {}; tone = {}
    def f(x, y):
        r = y // 4
        if y % 4 == 0: return -1
        if r not in joints:
            xs, x0 = set(), rnd.randint(0, 3)
            while x0 < 80: xs.add(x0); x0 += rnd.randint(3, 7)
            joints[r] = sorted(xs)
        if x in joints[r]: return -1
        k = (r, sum(1 for j in joints[r] if j < x))
        if k not in tone: tone[k] = rnd.choice((0, 0, 1, -1, 0))
        return tone[k]
    return f


def tex_plaster(rnd):
    return lambda x, y: (-1 if rnd.random() < 0.05 else 0)


def tex_wood(rnd):
    return lambda x, y: (-1 if y % 2 == 0 else (1 if rnd.random() < 0.06 else 0))


def tex_thatch(rnd):
    cols = {}
    def f(x, y):
        if x not in cols: cols[x] = rnd.choice((0, 1, -1, 0, 1))
        return cols[x] + (-1 if (y + x * 3) % 7 == 0 else 0)
    return f


def tex_shingle(rnd):
    def f(x, y):
        if y % 3 == 2: return -1
        if (x + (y // 3) * 2) % 4 == 0: return -1
        return 1 if y % 3 == 0 and rnd.random() < 0.5 else 0
    return f


def tex_tile(rnd):   # กระเบื้องดินเผาโค้ง
    def f(x, y):
        p = (x + (y // 3) % 2 * 2) % 4
        return (1, 0, 0, -1)[p] + (-1 if y % 3 == 2 else 0)
    return f


def tex_slate(rnd):
    def f(x, y):
        if y % 2 == 1: return -1 if rnd.random() < 0.5 else 0
        return 1 if rnd.random() < 0.15 else 0
    return f


def tex_marble(rnd):
    def f(x, y):
        if (x * 2 + y * 3 + rnd.randint(0, 2)) % 17 == 0: return -1
        return 1 if rnd.random() < 0.25 else 0
    return f


def tex_gold(rnd):
    return lambda x, y: (1 if y % 3 == 0 else 0) + (1 if (x + y) % 9 == 0 else 0)


def tex_hide(rnd):
    blobs = [(rnd.randint(0, 40), rnd.randint(0, 40), rnd.randint(2, 4)) for _ in range(30)]
    def f(x, y):
        for bx, by, r in blobs:
            if (x - bx) ** 2 + (y - by) ** 2 < r * r: return -1 if (bx + by) % 2 else 1
        return 0
    return f


TEX = {"brick": tex_brick, "stone": tex_stone, "plaster": tex_plaster, "wood": tex_wood, "thatch": tex_thatch,
       "shingle": tex_shingle, "tile": tex_tile, "slate": tex_slate, "marble": tex_marble, "gold": tex_gold, "hide": tex_hide}


def tex(b, name):
    return TEX[name](b.rnd) if name else None


# ---------------------------------------------------------------- ชิ้นส่วน
def wall(b, x0, y0, x1, y1, col, t="plaster", timber=None):
    m, d = b.mask(); d.rectangle((x0, y0, x1, y1), fill=255)
    b.fill(m, R(col), tex(b, t))
    if timber:
        tr = R(timber)
        for x in (x0, x1) + tuple(range(x0 + 5, x1 - 2, 6)):
            for y in range(y0, y1 + 1): b.put(x, y, tr[1] if x != x0 else tr[0])
        for y in (y0 + 1, (y0 + y1) // 2):
            for x in range(x0, x1 + 1): b.put(x, y, tr[2])


def foundation(b, x0, x1, y, col="#7a7468"):
    m, d = b.mask(); d.rectangle((x0 - 1, y, x1 + 1, y + 1), fill=255)
    b.fill(m, R(col), tex(b, "stone"))


def roof_gable(b, x0, x1, top, h, col, t="shingle", over=2, ridge=None):
    """หลังคาจั่วมุมเฉียงบน: ผืนหน้า + สันหลังคา + ปลายจั่วซ้ายขวา + เงาชายคา"""
    m, d = b.mask()
    d.polygon([(x0 - over, top + 1), (x1 + over, top + 1), (x1 + 1, top - h), (x0 - 1, top - h)], fill=255)
    b.fill(m, R(col), tex(b, t))
    rp = R(ridge or col)
    for x in range(x0 - 1, x1 + 2): b.put(x, top - h, rp[3]); b.put(x, top - h - 1, rp[0])
    for side in (-1, 1):
        ex = x0 - 1 if side < 0 else x1 + 1
        m, d = b.mask(); d.polygon([(ex, top - h), (ex + side * (over + 1), top + 1), (ex, top + 1)], fill=255)
        b.fill(m, R(col, 0.75), None)
    b.shade_band(x0, x1, top + 2, 120, 2)


def roof_hip(b, x0, x1, top, h, col, t="tile", over=2):
    m, d = b.mask()
    d.polygon([(x0 - over, top + 1), (x1 + over, top + 1), (x1 - h // 2, top - h), (x0 + h // 2, top - h)], fill=255)
    b.fill(m, R(col), tex(b, t))
    b.shade_band(x0, x1, top + 2, 120, 2)


def roof_cone(b, cx, base, w, h, col, t="thatch"):
    m, d = b.mask(); d.polygon([(cx - w, base), (cx, base - h), (cx + w, base), (cx + w - 1, base + 2), (cx - w + 1, base + 2)], fill=255)
    b.fill(m, R(col), tex(b, t))
    for x in range(int(cx - w + 1), int(cx + w)):   # ชายหญ้า
        if b.rnd.random() < 0.6: b.put(x, base + 3, R(col)[1])


def roof_flat(b, x0, x1, top, depth, col, parapet=True):
    m, d = b.mask(); d.polygon([(x0 - 1, top), (x0 + 1, top - depth), (x1 - 1, top - depth), (x1 + 1, top)], fill=255)
    b.fill(m, R(col), tex(b, "plaster"))
    if parapet:
        for x in range(x0 - 1, x1 + 2, 3): b.put(x, top - depth - 1, R(col)[3])
    b.shade_band(x0, x1, top + 1, 100, 1)


def dome(b, cx, base, r, col, t="gold"):
    m, d = b.mask(); d.pieslice((cx - r, base - r, cx + r, base + r), 180, 360, fill=255); d.rectangle((cx - r, base - 1, cx + r, base + 1), fill=255)
    b.fill(m, R(col), tex(b, t))
    b.put(cx, base - r - 1, R(col)[4]); b.put(cx, base - r - 2, (255, 255, 230, 255))


def battlements(b, x0, x1, y, col):
    for x in range(x0, x1 + 1, 3):
        m, d = b.mask(); d.rectangle((x, y - 2, x + 1, y), fill=255); b.fill(m, R(col), None)


def window(b, x, y, w=2, h=3, glass="#ffd86a", frame="#4a2c18", arch=False, sill=True):
    fr = R(frame); g = R(glass, 0.6)
    for yy in range(y - 1, y + h + 1):
        for xx in range(x - 1, x + w + 1): b.put(xx, yy, fr[1])
    if arch:
        for xx in range(x, x + w): b.put(xx, y - 2, fr[1])
    for yy in range(y, y + h):
        for xx in range(x, x + w): b.put(xx, yy, g[2] if (xx + yy) % 3 else g[3])
    b.put(x, y, g[4])
    if w >= 3:
        for yy in range(y, y + h): b.put(x + w // 2, yy, fr[1])
    if sill:
        for xx in range(x - 1, x + w + 1): b.put(xx, y + h + 1, R("#c8c0b0")[2])


def door(b, cx, base, w=3, h=5, col="#6a4426", frame="#4a4a52", arch=True):
    fr = R(frame); dc = R(col)
    x0 = cx - w // 2
    for y in range(base - h - 1, base + 1):
        for x in range(x0 - 1, x0 + w + 1): b.put(x, y, fr[2])
    if arch:
        for x in range(x0, x0 + w): b.put(x, base - h - 2, fr[2])
    for y in range(base - h, base + 1):
        for x in range(x0, x0 + w): b.put(x, y, dc[2] if (x - x0) % 2 == 0 else dc[1])
    b.put(x0 + w - 1, base - h // 2, (230, 200, 90, 255))


def chimney(b, x, y, col="#8a5a48", smoke=True):
    m, d = b.mask(); d.rectangle((x, y, x + 2, y + 5), fill=255); b.fill(m, R(col), tex(b, "brick"))
    if smoke:
        for i, (dx, dy) in enumerate(((1, -2), (2, -4), (1, -6), (3, -7))):
            b.put(x + dx, y + dy, (210, 210, 215, 170 - i * 35))


def flag(b, x, y, h=7, big=False):
    for yy in range(y - 2, y + h): b.put(x, yy, (90, 64, 40, 255))
    b.put(x, y - 3, (230, 200, 90, 255))
    w = 5 if big else 4
    for yy in range(y - 2, y + (3 if big else 2)):
        for xx in range(x + 1, x + 1 + w - (1 if (yy - y) % 3 == 2 else 0)):
            b.put(xx, yy, KC)


def banner_wall(b, x, y, h=5):
    for yy in range(y, y + h):
        for xx in range(x, x + 3): b.put(xx, yy, KC)
    b.put(x + 1, y + h, KC)
    for xx in range(x - 1, x + 4): b.put(xx, y - 1, (90, 64, 40, 255))


# ---------------------------------------------------------------- ของประกอบ
def barrel(b, x, y):
    m, d = b.mask(); d.ellipse((x, y, x + 3, y + 4), fill=255); b.fill(m, R("#8a5a30"), tex(b, "wood"))
    b.put(x + 1, y + 1, (200, 170, 120, 255))


def crate(b, x, y):
    m, d = b.mask(); d.rectangle((x, y, x + 3, y + 3), fill=255); b.fill(m, R("#a87a44"), None)
    b.put(x + 1, y + 1, R("#a87a44")[1]); b.put(x + 2, y + 2, R("#a87a44")[1])


def flowers(b, x, y, n=3):
    for i in range(n):
        b.put(x + i * 2, y, (90, 150, 60, 255))
        b.put(x + i * 2, y - 1, b.rnd.choice(((240, 90, 110, 255), (250, 220, 90, 255), (200, 120, 240, 255))))


def torch(b, x, y):
    b.put(x, y, (90, 64, 40, 255)); b.put(x, y + 1, (90, 64, 40, 255))
    b.put(x, y - 1, (255, 160, 60, 255)); b.put(x, y - 2, (255, 230, 140, 255))


def crystal(b, x, y, col="#7af0ff", h=5):
    m, d = b.mask(); d.polygon([(x - 1, y), (x, y - h), (x + 1, y)], fill=255); b.fill(m, R(col, 0.7), None)
    b.put(x, y - h - 1, (255, 255, 255, 200))


def bush(b, x, y, col="#4a8a3a"):
    m, d = b.mask(); d.ellipse((x, y, x + 4, y + 3), fill=255); b.fill(m, R(col), tex(b, "plaster"))


def glow(b, color, strength=60):
    src = b.img.copy(); a = src.split()[3]; ap = a.load()
    g = Image.new("RGBA", src.size, (0, 0, 0, 0)); gp = g.load()
    c = hx(color)
    for y in range(b.h):
        for x in range(b.w):
            if ap[x, y]: continue
            n = sum(1 for dy in (-1, 0, 1) for dx in (-1, 0, 1)
                    if 0 <= x + dx < b.w and 0 <= y + dy < b.h and ap[x + dx, y + dy] > 0 and src.getpixel((x + dx, y + dy))[:3] != (255, 0, 255))
            if n: gp[x, y] = c + (min(255, strength + n * 8),)
    g.alpha_composite(src); b.img = g; b.px = b.img.load()
