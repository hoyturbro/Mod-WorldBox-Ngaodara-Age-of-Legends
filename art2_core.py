# ชุดเครื่องมือวาด pixel art สไตล์ WorldBox (ไอคอน 28px ลอยตัว ไม่มีกรอบ แรเงา 4–5 โทน แสงจากบนซ้าย)
# วาดเป็น "ชิ้นส่วน" (mask) แล้วลงเงาอัตโนมัติ: ขอบ = โทนเข้มสุดของสีเดียวกัน · ขอบบนซ้าย = สว่าง · ล่างขวา = มืด
from PIL import Image, ImageDraw
import colorsys, math


def hx(c):
    c = c.lstrip('#')
    return tuple(int(c[i:i + 2], 16) for i in (0, 2, 4))


def ramp(base, n=5, hue_shift=0.04, spread=1.0):
    """โทนสี 5 ระดับ: [ขอบ, มืด, กลาง, สว่าง, ไฮไลต์] เงาเลื่อนสีไปทางเย็น/ม่วง ไฮไลต์อุ่น (เทคนิค pixel art)"""
    r, g, b = [v / 255 for v in hx(base)]
    h, s, v = colorsys.rgb_to_hsv(r, g, b)
    out = []
    for i, (dv, ds, dh) in enumerate([(-0.55, 0.15, hue_shift), (-0.30, 0.10, hue_shift * 0.6), (0, 0, 0),
                                       (0.18, -0.12, -hue_shift * 0.5), (0.38, -0.35, -hue_shift)]):
        nv = min(1, max(0.06, v + dv * spread)); ns = min(1, max(0, s + ds)); nh = (h + dh) % 1
        rr, gg, bb = colorsys.hsv_to_rgb(nh, ns, nv)
        out.append((int(rr * 255), int(gg * 255), int(bb * 255), 255))
    return out


class Canvas:
    def __init__(self, w=28, h=28):
        self.w, self.h = w, h
        self.img = Image.new("RGBA", (w, h), (0, 0, 0, 0))

    def mask(self):
        return Mask(self.w, self.h)

    def part(self, m, rp, outline=True, flat=False, light=(-1, -1)):
        """ลงเงาให้ mask แล้ววางบนภาพ"""
        px = self.img.load(); mp = m.img.load()
        W, H = self.w, self.h
        inside = lambda x, y: 0 <= x < W and 0 <= y < H and mp[x, y] > 0
        lx, ly = light
        for y in range(H):
            for x in range(W):
                if not mp[x, y]:
                    continue
                edge = not (inside(x - 1, y) and inside(x + 1, y) and inside(x, y - 1) and inside(x, y + 1))
                if edge and outline:
                    px[x, y] = rp[0]; continue
                if flat:
                    px[x, y] = rp[2]; continue
                lit = (not inside(x + lx, y + ly)) or (not inside(x + 2 * lx, y + 2 * ly))
                shd = (not inside(x - lx, y - ly)) or (not inside(x - 2 * lx, y - 2 * ly))
                if lit and not shd:
                    px[x, y] = rp[3] if inside(x + lx, y + ly) else rp[4] if outline else rp[3]
                elif shd and not lit:
                    px[x, y] = rp[1]
                else:
                    px[x, y] = rp[2]
        return self

    def dot(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.img.load()[x, y] = c if len(c) == 4 else c + (255,)

    def line(self, pts, c, w=1):
        ImageDraw.Draw(self.img).line(pts, fill=c if len(c) == 4 else c + (255,), width=w)

    def glow(self, color, strength=90, radius=1):
        """แสงเรืองรอบวัตถุ (ของเทพ)"""
        src = self.img.copy(); a = src.split()[3]
        g = Image.new("RGBA", src.size, (0, 0, 0, 0)); gp = g.load(); ap = a.load()
        c = hx(color) if isinstance(color, str) else color[:3]
        for y in range(self.h):
            for x in range(self.w):
                if ap[x, y]:
                    continue
                near = 0
                for dy in range(-radius, radius + 1):
                    for dx in range(-radius, radius + 1):
                        xx, yy = x + dx, y + dy
                        if 0 <= xx < self.w and 0 <= yy < self.h and ap[xx, yy] > 0:
                            near += 1
                if near:
                    gp[x, y] = c + (min(255, strength + near * 10),)
        g.alpha_composite(src)
        self.img = g
        return self

    def sparkle(self, x, y, c="#ffffff"):
        c = hx(c) + (255,)
        for dx, dy in ((0, 0), (1, 0), (-1, 0), (0, 1), (0, -1)):
            self.dot(x + dx, y + dy, c if (dx, dy) == (0, 0) else c[:3] + (170,))

    def save(self, path, scale=1):
        im = self.img if scale == 1 else self.img.resize((self.w * scale, self.h * scale), Image.NEAREST)
        im.save(path)


class Mask:
    def __init__(self, w, h):
        self.img = Image.new("L", (w, h), 0)
        self.d = ImageDraw.Draw(self.img)

    def poly(self, pts):
        self.d.polygon(pts, fill=255); return self

    def rect(self, x0, y0, x1, y1):
        self.d.rectangle((x0, y0, x1, y1), fill=255); return self

    def ell(self, x0, y0, x1, y1):
        self.d.ellipse((x0, y0, x1, y1), fill=255); return self

    def line(self, pts, w=1):
        self.d.line(pts, fill=255, width=w); return self

    def pie(self, box, a0, a1):
        self.d.pieslice(box, a0, a1, fill=255); return self

    def cut_ell(self, x0, y0, x1, y1):
        self.d.ellipse((x0, y0, x1, y1), fill=0); return self

    def cut_poly(self, pts):
        self.d.polygon(pts, fill=0); return self

    def cut_rect(self, x0, y0, x1, y1):
        self.d.rectangle((x0, y0, x1, y1), fill=0); return self


def diag(cx, cy, along, across, ang=-45):
    """จุดบนแนวทแยง (ใช้วาดอาวุธเอียง 45° แบบไอคอนเกม)"""
    a = math.radians(ang)
    ux, uy = math.cos(a), math.sin(a)
    px, py = -uy, ux
    return (cx + ux * along + px * across, cy + uy * along + py * across)


def sheet(images, out, scale=3, cols=8, bg=(56, 66, 58, 255), labels=None):
    w = max(i.width for i in images); h = max(i.height for i in images)
    rows = (len(images) + cols - 1) // cols
    S = Image.new("RGBA", (cols * (w + 3) * scale, rows * (h + 3) * scale), bg)
    for k, im in enumerate(images):
        x = (k % cols) * (w + 3) * scale; y = (k // cols) * (h + 3) * scale
        S.alpha_composite(im.resize((im.width * scale, im.height * scale), Image.NEAREST), (x, y))
    S.save(out)
