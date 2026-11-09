# บาลานซ์รอบ 2: แนวฝึกยุทธ์อายุยืน (S ~100+ ปี · S5 ~500 ปี)
import random, math, sys

RANKS = ["E", "D", "C", "B", "A", "S", "SS", "SSS", "U", "M", "G"]
# ---- ตัวเลขที่ปรับ ----
# EXP ต่อดาวที่ดาว 1 ของแต่ละแรงค์ (ดาว 12 = ×2) — S ขึ้นไปกระโดดแรง
EXPR = [70, 150, 300, 600, 1200, 52000, 160000, 600000, 2400000, 9000000, 36000000]
BT = [0.80, 0.60, 0.45, 0.25, 0.10, 0.03, 0.02, 0.012, 0.008, 0.004]
LIFE = [0, 5, 12, 25, 70, 650, 1500, 4000, 10000, 30000, 100000]   # อายุขัยเพิ่ม (ปี)
DEATH = [0.0030, 0.0024, 0.0019, 0.0015, 0.0011, 0.0002, 0.0001, 0.00005, 0.00002, 0.00001, 0.000005]  # ตายในงานต่อเดือน
TRAIN, KILL_RATE = 6, 0.25
PRE_MAGIC_S, STAR_ERA = 0.5, 1.5


def exp_next(pl):
    r = min((pl - 1) // 12, 10); st = (pl - 1) % 12
    return int(EXPR[r] * (1 + st / 11))
def kill_exp(pl): return 15 + pl * 1.5


def poisson(lam, rnd):
    L, k, p = math.exp(-lam), 0, 1.0
    while True:
        p *= rnd.random()
        if p <= L: return k
        k += 1


def life(era, rnd, cap_years=1500):
    genius = rnd.random() < 0.03
    spi = rnd.randint(3, 10) + (6 if genius else 0)
    s7 = sum(rnd.randint(3, 10) + (6 if genius else 0) for _ in range(7))
    age = rnd.randint(18, 28); lifespan = rnd.gauss(72, 10)
    r = 0 if s7 < 60 else 1 if s7 < 72 else 2 if s7 < 86 else 3 if s7 < 100 else 4 if s7 < 115 else 5
    star, exp, m = 1, 0.0, 0
    reach = {r: age}; s5 = None
    while True:
        m += 1
        if m % 12 == 0:
            age += 1; spi += 0.2
            if age - 18 > cap_years: return r, star, age, reach, s5, genius
        if age > lifespan + LIFE[r]: return r, star, age, reach, s5, genius
        if rnd.random() < DEATH[r]: return r, star, age, reach, s5, genius
        gain = TRAIN + sum(kill_exp(r * 12 + star) for _ in range(poisson(KILL_RATE, rnd)))
        if genius: gain *= 2
        exp += gain
        while star < 12 and exp >= exp_next(r * 12 + star):
            exp -= exp_next(r * 12 + star); star += 1
            if r == 5 and star == 5 and s5 is None: s5 = age
        if star >= 12 and exp >= exp_next(r * 12 + star) and r < 10:
            c = BT[r] * (1 + spi * 0.01)
            if r + 1 >= 5 and era < 4: c *= PRE_MAGIC_S
            if r + 1 >= 6 and era >= 5: c *= STAR_ERA
            if rnd.random() < c:
                r += 1; star = 1; exp = 0; reach[r] = age
                if r == 5 and s5 is None: pass
            else:
                if r >= 4: star = max(1, star - 2); exp = 0     # ประตูตำนาน (A→S ขึ้นไป) ล้ม: เสีย 2 ดาว
                else: exp *= 0.7


def run(era, n=6000):
    rnd = random.Random(2)
    final = [0] * 11; ages = {k: [] for k in range(11)}; tw = [0.0] * 11; s5 = []; gen_s = 0; s_total = 0
    for _ in range(n):
        r, star, age, reach, s5age, genius = life(era, rnd)
        final[r] += 1
        for k, a in reach.items(): ages[k].append(a)
        ks = sorted(reach)
        for i, k in enumerate(ks):
            end = reach[ks[i + 1]] if i + 1 < len(ks) else age
            tw[k] += max(0, end - reach[k])
        if s5age: s5.append(s5age)
        if 5 in reach:
            s_total += 1; gen_s += genius
    T = sum(tw)
    print("=== ยุค %d ===  (อายุตอนถึงแรงค์ = เฉลี่ย / ต่ำสุด)" % era)
    for k in range(11):
        if ages[k] and k > 0:
            a = ages[k]
            print("  %-3s พบในโลก %6.2f%%   ถึงตอนอายุ %6.0f ปี (ต่ำสุด %4d)" % (RANKS[k], 100 * tw[k] / T, sum(a) / len(a), min(a)))
        elif k == 0:
            print("  E   พบในโลก %6.2f%%" % (100 * tw[0] / T))
    if s5: print("  S5★ ถึงตอนอายุเฉลี่ย %.0f ปี (ต่ำสุด %d) · %d คน" % (sum(s5) / len(s5), min(s5), len(s5)))
    print("  คนที่ถึง S: %d คน (อัจฉริยะ %d คน)" % (s_total, gen_s))


if __name__ == "__main__":
    for era in (3, 5): run(era)
