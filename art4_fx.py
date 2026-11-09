# เอฟเฟกต์ของมอดเอง (pixel art แอนิเมชัน) → GameResources/effects/ngd/<name>/<name>_<i>.png
# 11 ธาตุ × (slash · burst · cast · wave · buff) + bolt · pillar · teleport
import os, sys, math, random
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(ROOT, "NgaodaraPack", "GameResources", "effects", "ngd")

EL = {  # แกนสว่าง, สีหลัก
    "phys": ((255, 255, 255), (190, 205, 225)), "fire": ((255, 244, 170), (255, 110, 40)), "ice": ((240, 255, 255), (110, 200, 255)),
    "thunder": ((255, 255, 225), (255, 220, 60)), "holy": ((255, 255, 235), (255, 205, 80)), "dark": ((240, 205, 255), (140, 60, 220)),
    "poison": ((230, 255, 200), (110, 220, 60)), "wind": ((230, 255, 245), (80, 230, 180)), "star": ((235, 240, 255), (110, 140, 255)),
    "blood": ((255, 210, 210), (210, 30, 50)), "nature": ((230, 255, 210), (90, 190, 70)),
}


def save(name, frames):
    d = os.path.join(OUT, name); os.makedirs(d, exist_ok=True)
    for f in os.listdir(d): os.remove(os.path.join(d, f))
    for i, im in enumerate(frames): im.save(os.path.join(d, "%s_%d.png" % (name, i)))


def mix(a, b, t): return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def px(im, x, y, c, a=255):
    if 0 <= x < im.width and 0 <= y < im.height:
        old = im.getpixel((x, y))
        if old[3] < a: im.putpixel((x, y), tuple(c[:3]) + (a,))


# ---------------------------------------------------------------- อนุภาคประจำธาตุ
def particle(im, el, x, y, core, col, a, rnd):
    x, y = int(x), int(y)
    if el == "fire":      # ลิ้นไฟ
        for k in range(3): px(im, x, y - k, core if k == 0 else col, a)
        px(im, x + 1, y - 1, col, a)
    elif el == "ice":     # เกล็ดน้ำแข็ง
        for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)): px(im, x + dx, y + dy, col, a)
        px(im, x, y, core, a)
    elif el == "thunder":  # สายฟ้าซิกแซก
        for dx, dy in ((0, 0), (1, 1), (0, 2), (1, 3)): px(im, x + dx, y + dy, core if dy % 2 == 0 else col, a)
    elif el in ("holy", "star"):  # ประกายกากบาท
        for dx, dy in ((0, -1), (-1, 0), (1, 0), (0, 1)): px(im, x + dx, y + dy, col, a)
        px(im, x, y, core, a)
    elif el == "dark":    # ควันม่วง
        for dx, dy in ((0, 0), (1, 0), (0, -1), (-1, -1)): px(im, x + dx, y + dy, col, a)
    elif el == "poison":  # ฟองพิษ
        px(im, x, y, core, a); px(im, x + 1, y, col, a); px(im, x, y + 1, col, a); px(im, x + 1, y + 1, col, a)
    elif el == "blood":   # หยดเลือด
        px(im, x, y, col, a); px(im, x, y + 1, col, a); px(im, x, y - 1, core, a)
    elif el == "nature":  # ใบไม้
        px(im, x, y, col, a); px(im, x + 1, y - 1, col, a); px(im, x - 1, y + 1, core, a)
    elif el == "wind":    # เส้นลม
        for dx in range(3): px(im, x + dx, y, core if dx == 1 else col, a)
    else:
        px(im, x, y, core, a)


# ---------------------------------------------------------------- รอยฟัน: เสี้ยวพระจันทร์กวาด (หนา สีสด จางเฉพาะท้าย)
def slash(core, col, n=6, S=32, el="phys"):
    frames = []
    dark = mix(col, (0, 0, 0), 0.45)
    for i in range(n):
        im = Image.new("RGBA", (S, S), (0, 0, 0, 0))
        t = i / (n - 1)
        span = 50 + 150 * min(1, t * 1.8)
        start = 200 - span / 2
        fade = 1 - max(0, t - 0.7) / 0.3
        cx, cy, r = S / 2, S / 2, S / 2 - 2
        for k in range(int(span * 2)):
            a = math.radians(start + k / 2)
            frac = k / (span * 2)
            thick = 1.5 + 5.5 * math.sin(math.pi * frac) * (1 - t * 0.45)
            for w in range(int(thick) + 2):
                rr = r - w
                x, y = int(cx + math.cos(a) * rr), int(cy + math.sin(a) * rr * 0.75)
                c = core if w < thick * 0.35 else col if w < thick * 0.85 else dark
                px(im, x, y, c, int(255 * fade))
        rnd = random.Random(i * 3 + 1)
        for _ in range(int(8 * fade)):
            a = math.radians(start + rnd.random() * span)
            rr = r + rnd.randint(0, 4)
            particle(im, el, cx + math.cos(a) * rr, cy + math.sin(a) * rr * 0.75, core, col, int(240 * fade), rnd)
        frames.append(im)
    return frames


# ---------------------------------------------------------------- ระเบิดตอนโดน (ทึบ มีแฉก + อนุภาคธาตุ)
def burst(core, col, n=7, S=32, el="phys"):
    frames = []
    rnd = random.Random(7)
    parts = [(rnd.random() * 6.283, 5 + rnd.random() * 10) for _ in range(12)]
    dark = mix(col, (0, 0, 0), 0.4)
    for i in range(n):
        im = Image.new("RGBA", (S, S), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
        t = i / (n - 1); c = S / 2
        r = 2.5 + 8 * min(1, t * 1.4)
        fade = 1 - max(0, t - 0.5) / 0.5
        if t < 0.75:
            d.ellipse((c - r - 1, c - r - 1, c + r + 1, c + r + 1), fill=dark + (int(230 * fade),))
            d.ellipse((c - r, c - r, c + r, c + r), fill=col + (int(250 * fade),))
            r2 = r * (0.6 - t * 0.4)
            d.ellipse((c - r2, c - r2, c + r2, c + r2), fill=core + (int(255 * fade),))
        else:
            d.ellipse((c - r, c - r, c + r, c + r), outline=col + (int(230 * fade),), width=2)
        for k in range(8):   # แฉกดาว
            a = math.radians(k * 45 + 22.5 * (i % 2))
            L = r + 2 + 5 * (1 - t)
            d.line([(c + math.cos(a) * r * 0.7, c + math.sin(a) * r * 0.7), (c + math.cos(a) * L, c + math.sin(a) * L)], fill=core + (int(240 * fade),), width=1)
        for a, sp in parts:
            dist = sp * t * 1.2
            particle(im, el, c + math.cos(a) * dist, c + math.sin(a) * dist, core, col, int(255 * fade), rnd)
        frames.append(im)
    return frames


# ---------------------------------------------------------------- วงร่ายเวทบนพื้น (หมุน)
def cast(core, col, n=6, S=32):
    frames = []
    for i in range(n):
        im = Image.new("RGBA", (S, S // 2), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
        c, cy = S / 2, S / 4
        d.ellipse((1, 1, S - 2, S // 2 - 2), outline=col + (230,), width=1)
        d.ellipse((5, 3, S - 6, S // 2 - 4), outline=core + (200,), width=1)
        for k in range(8):   # อักษรรูนรอบวง หมุนทีละเฟรม
            a = math.radians(k * 45 + i * 15)
            x, y = c + math.cos(a) * (S / 2 - 3), cy + math.sin(a) * (S / 4 - 1.5)
            px(im, int(x), int(y), core, 255); px(im, int(x) + 1, int(y), col, 200)
        for k in range(3):   # ดาวสามแฉกกลางวง
            a = math.radians(k * 120 + i * 20)
            d.line([(c, cy), (c + math.cos(a) * 9, cy + math.sin(a) * 4)], fill=col + (180,), width=1)
        frames.append(im)
    return frames


# ---------------------------------------------------------------- คลื่นไม้ตาย (วงขยายหนา + เศษธาตุกระเด็น)
def wave(core, col, n=8, S=48, el="phys"):
    frames = []
    rnd = random.Random(11)
    debris = [(rnd.random() * 6.283, 0.7 + rnd.random() * 0.4) for _ in range(16)]
    dark = mix(col, (0, 0, 0), 0.4)
    for i in range(n):
        im = Image.new("RGBA", (S, S // 2), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
        t = i / (n - 1); c, cy = S / 2, S / 4
        rx, ry = 3 + (S / 2 - 4) * t, 1.5 + (S / 4 - 2) * t
        fade = 1 - max(0, t - 0.55) / 0.45
        if t < 0.6: d.ellipse((c - rx * 0.7, cy - ry * 0.7, c + rx * 0.7, cy + ry * 0.7), fill=col + (int(110 * (1 - t / 0.6)),))
        d.ellipse((c - rx - 1, cy - ry - 1, c + rx + 1, cy + ry + 1), outline=dark + (int(220 * fade),), width=1)
        d.ellipse((c - rx, cy - ry, c + rx, cy + ry), outline=col + (int(255 * fade),), width=3)
        if rx > 5: d.ellipse((c - rx + 2, cy - ry + 1, c + rx - 2, cy + ry - 1), outline=core + (int(255 * fade),), width=1)
        for a, k in debris:
            particle(im, el, c + math.cos(a) * rx * k, cy + math.sin(a) * ry * k - 2 * t, core, col, int(240 * fade), rnd)
        frames.append(im)
    return frames


# ---------------------------------------------------------------- ประกายบัฟลอยขึ้น
def buff(core, col, n=6, W=16, H=24):
    frames = []
    rnd = random.Random(3)
    sp = [(rnd.randint(2, W - 3), rnd.random()) for _ in range(7)]
    for i in range(n):
        im = Image.new("RGBA", (W, H), (0, 0, 0, 0))
        for x, ph in sp:
            y = int(H - 2 - ((i / n + ph) % 1) * (H - 4))
            a = int(255 * (1 - abs(((i / n + ph) % 1) - 0.5) * 1.6))
            px(im, x, y, core, a); px(im, x, y + 1, col, a // 2)
            if (x + i) % 3 == 0: px(im, x - 1, y, col, a // 2); px(im, x + 1, y, col, a // 2)
        frames.append(im)
    return frames


def bolt(n=4, W=12, H=40):
    frames = []
    for i in range(n):
        im = Image.new("RGBA", (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
        rnd = random.Random(i * 11)
        x = W // 2; pts = [(x, 0)]
        for y in range(4, H, 4):
            x = max(1, min(W - 2, x + rnd.randint(-3, 3))); pts.append((x, y))
        a = 255 if i < n - 1 else 120
        d.line(pts, fill=(255, 230, 90, a), width=3); d.line(pts, fill=(255, 255, 235, a), width=1)
        frames.append(im)
    return frames


def pillar(n=4, W=12, H=40):
    frames = []
    for i in range(n):
        im = Image.new("RGBA", (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
        w = 3 + (i % 2)
        d.rectangle((W // 2 - w, 0, W // 2 + w, H - 1), fill=(255, 255, 255, 120))
        d.rectangle((W // 2 - 1, 0, W // 2 + 1, H - 1), fill=(255, 255, 255, 230))
        for k in range(5): px(im, (k * 5 + i * 3) % W, (k * 9 + i * 7) % H, (255, 255, 255), 255)
        frames.append(im)
    return frames


def teleport(n=5, S=24):
    frames = []
    rnd = random.Random(5)
    puffs = [(rnd.randint(4, S - 5), rnd.randint(6, S - 4), rnd.randint(2, 4)) for _ in range(7)]
    for i in range(n):
        im = Image.new("RGBA", (S, S), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
        t = i / (n - 1)
        for x, y, r in puffs:
            rr = r + t * 3
            d.ellipse((x - rr, y - rr - t * 4, x + rr, y + rr - t * 4), fill=(70, 50, 90, int(220 * (1 - t))))
            d.ellipse((x - rr * 0.5, y - rr * 0.5 - t * 4, x + rr * 0.5, y + rr * 0.5 - t * 4), fill=(150, 120, 190, int(200 * (1 - t))))
        frames.append(im)
    return frames


def build():
    out = []
    for el, (core, col) in EL.items():
        for name, fn in (("slash", slash), ("burst", burst), ("cast", cast), ("wave", wave), ("buff", buff)):
            fr = fn(core, col, el=el) if name in ("slash", "burst", "wave") else fn(core, col)
            save("%s_%s" % (name, el), fr); out.append(fr)
    for name, fn in (("bolt", bolt), ("pillar", pillar), ("teleport", teleport)):
        fr = fn(); save(name, fr); out.append(fr)
    return out


if __name__ == "__main__":
    anims = build()
    # ภาพตัวอย่าง: แต่ละแถว = 1 แอนิเมชัน
    scale = 3
    W = max(sum(f.width + 2 for f in a) for a in anims) * scale
    H = sum(max(f.height for f in a) + 2 for a in anims) * scale
    sheet = Image.new("RGBA", (W, H), (30, 34, 40, 255)); y = 0
    for a in anims:
        x = 0
        for f in a:
            sheet.alpha_composite(f.resize((f.width * scale, f.height * scale), Image.NEAREST), (x, y)); x += (f.width + 2) * scale
        y += (max(f.height for f in a) + 2) * scale
    sheet.save(os.path.join(ROOT, "previews", "fx_preview.png"))
    print("ok", len(anims))
