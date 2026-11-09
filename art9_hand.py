# ภาพอาวุธ "ในมือ" แบบเดียวกับเกมต้นฉบับ: แนวตั้ง ไม่มีเส้นขอบ พิกเซลน้อย สีสด ด้ามอยู่ล่าง
# ขนาดเท่าอาวุธระดับตำนานของเกม (ดาบไฟ 5x11 · ไม้เท้า 3x12 · ค้อน 5x8)
# บันทึก: GameResources/items/weapons/w_<id>.png
import os, sys
from PIL import Image
from art2_core import ramp, sheet
from art2_gear import THEMES, HAND_DIR
from art5_relics import RELICS as RELICS2

# ตัวอักษร: M/m ใบมีด (สว่าง/เข้ม) · A/a สีประกาย · T ขอบ/การ์ด · E อัญมณี · G/g ด้าม · . ว่าง
TPL = {
    "sword": ["..A..", ".mMA.", ".mM..", ".mM..", ".mM..", ".mM..", ".mM..", "TTETT", "..G..", "..g..", "..E.."],
    "flame": ["..A..", ".AMa.", "aMMA.", ".mMa.", "aMMA.", ".mMa.", ".mM..", "TTETT", "..G..", "..g..", "..E.."],
    "holy":  ["..A..", "..M..", ".mMA.", ".mM..", ".mM..", ".mM..", "T.M.T", "TTETT", "..G..", "..g..", "..E.."],
    "blood": ["..A..", ".mM..", "amM..", ".mMa.", "amM..", ".mMa.", ".mM..", "TTETT", "..G..", "..g..", "..E.."],
    "saw":   [".MM..", "aMMa.", ".MM..", "aMMa.", ".MM..", "aMMa.", ".MM..", "TTETT", "..G..", "..g..", "..E.."],
    "katana": [".A.", ".M.", ".M.", ".M.", ".M.", ".mM", ".mM", ".M.", "TET", ".G.", ".G.", ".g."],
    "dagger": [".A.", ".M.", ".M.", "aMa", ".M.", "TET", ".G.", ".g."],
    "axe":   [".M...", "MMG..", "MAGE.", "MMG..", ".MG..", "..G..", "..G..", "..G..", "..G..", "..g.."],
    "scythe": ["MMMM.", "Am.G.", "...G.", "...G.", "...GE", "...G.", "...G.", "...G.", "...G.", "...g."],
    "spear": [".A.", "AMA", ".M.", ".m.", ".T.", ".E.", ".G.", ".G.", ".G.", ".G.", ".G.", ".g."],
    "trident": ["M.M.M", "M.M.M", "MMMMM", "..T..", "..E..", "..G..", "..G..", "..G..", "..G..", "..G..", "..g.."],
    "hammer": ["MAEAM", "mmmmm", "..T..", "..G..", "..G..", "..G..", "..G..", "..g.."],
    "bow":   ["AM..", "A.M.", "A..M", "A..m", "A..E", "A..m", "A..M", "A.M.", "AM.."],
    "staff": [".A.", "AEA", ".A.", ".T.", ".G.", ".G.", ".G.", ".G.", ".G.", ".G.", ".G.", ".g."],
    "orb":   ["aAa", "AEA", "aAa", ".T.", ".G.", ".G.", ".G.", ".G.", ".G.", ".G.", ".G.", ".g."],
}
# ชนิดภาพของ art2 → แม่แบบในมือ
KIND = {"flame_sword": "flame", "holy_sword": "holy", "blood_sword": "blood", "chainsword": "saw", "katana": "katana",
        "plague_dagger": "dagger", "dragon_axe": "axe", "void_scythe": "scythe", "bolt_spear": "spear", "trident": "trident",
        "rock_hammer": "hammer", "vine_hammer": "hammer", "wind_bow": "bow", "moon_bow": "bow", "ice_staff": "staff", "star_staff": "orb"}
# จุดหมุน (พิกเซลจากล่าง) แบบเดียวกับเกม: ดาบ/ขวาน/ค้อน ที่ด้าม · หอก 1.8 · ไม้เท้า/ธนู กลางคัน
PIVOT_Y = {"spear": 1.8, "trident": 1.8, "staff": 5.6, "orb": 5.6, "bow": 4.5, "hammer": 1.45}

BLADE = {"flame_sword": "#ff7a3a", "blood_sword": "#d0283a", "plague_dagger": "#9aaa6a", "katana": "#eef2f8", "chainsword": None,
         "rock_hammer": None, "vine_hammer": None, "dragon_axe": None, "ice_staff": None, "star_staff": None, "moon_bow": None}


def draw(kind, metal, trim, acc):
    t = KIND.get(kind, "sword")
    rows = TPL[t]
    bcol = BLADE.get(kind, "#d8e0ea")
    if bcol is None: bcol = metal
    if kind == "holy_sword": bcol = trim
    M, A, T = ramp(bcol), ramp(acc), ramp(trim)
    G = ramp("#7a4a2a")
    pal = {"M": M[3], "m": M[2], "A": A[4], "a": A[2], "T": T[3], "E": A[4], "G": G[2], "g": G[1]}
    if t in ("bow",): pal["A"] = (235, 230, 215, 255)       # สายธนู
    h, w = len(rows), len(rows[0])
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0)); px = im.load()
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch in pal:
                c = pal[ch]; px[x, y] = tuple(c[:3]) + (255,)
    return im, t


# อาวุธเทพรุ่นแรก 7 ชิ้น
RELICS1 = {
    "ngd_starfall_blade": ("#2a3478", "#ffd24a", "#8fb4ff", "holy_sword"),
    "ngd_chrono_blade": ("#5af0ff", "#d0dcff", "#7af0ff", "katana"),
    "ngd_thunder_spear": ("#46508e", "#ffd84a", "#fff27a", "bolt_spear"),
    "ngd_rift_scythe": ("#3a2a5a", "#7a4aa0", "#ff5aff", "void_scythe"),
    "ngd_cosmos_hammer": ("#3a3a7a", "#d0dcff", "#8fb4ff", "rock_hammer"),
    "ngd_crescent_bow": ("#d8e8ff", "#9aa6c8", "#8ad8ff", "moon_bow"),
    "ngd_galaxy_scepter": ("#2a3478", "#d0dcff", "#b080ff", "star_staff"),
}


def all_items():
    out = {}
    for tid, T in THEMES.items():
        out["ngd_set_%s_weapon" % tid] = (T[0], T[1], T[2], T[3])
    for rid, R in RELICS1.items(): out[rid] = R
    for rid, R in RELICS2.items(): out[rid] = (R[0], R[1], R[2], R[3])
    return out


def build(write=True):
    prev = []
    meta = {}
    for iid, (metal, trim, acc, kind) in all_items().items():
        im, t = draw(kind, metal, trim, acc)
        meta[iid] = (t, im.size)
        prev.append(im)
        if write: im.save(os.path.join(HAND_DIR, "w_%s.png" % iid))
    return prev, meta


if __name__ == "__main__":
    prev, meta = build(write="preview" not in sys.argv)
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(os.environ.get("TEMP", "."), "hand.png")
    sheet([p.resize((p.width, p.height)) for p in prev], out, scale=8, cols=20, bg=(70, 100, 60, 255))
    print("ok", len(prev), out)
