# จำลองชีวิตนักผจญภัยเพื่อบาลานซ์ (EXP · ทะลวงแรงค์ · อายุขัย · ตายในสนามรบ · พลัง)
# python balance_sim.py [current|new]
import random, sys, math

GROW = 1.9
BT = [0.80, 0.65, 0.50, 0.35, 0.20, 0.02, 0.008, 0.003, 0.0012, 0.0004]
STAR_ERA = 1.5
LIFE = [0, 2, 5, 10, 20, 40, 80, 150, 300, 600, 1500]
RANKS = ["E", "D", "C", "B", "A", "S", "SS", "SSS", "U", "M", "G"]
STAR = 12


class P:   # ชุดตัวเลขบาลานซ์
    def __init__(self, mode):
        self.mode = mode
        if mode == "current":
            self.exp_next = lambda pl: 15 + pl * 8
            self.train = 6                       # EXP ฝึกกิลด์ต่อเดือน (4 + tier*2)
            self.kill_rate = 0.25                # ฆ่าต่อเดือน (เฉลี่ย)
            self.kill_exp = lambda pl: 12 + pl * 1.5 + 3
            self.bt = lambda r, spi, era: self.bt_cur(r, spi, era)
            self.life_bonus = lambda r: 0
            self.death_month = lambda r: 0.004
        else:
            # EXP ต่อดาว: ขั้นละเท่าตัวทุก ~1.5 แรงค์ (ขั้นสูงช้าลงมาก)
            self.exp_next = lambda pl: int(28 + pl * 6 + GROW ** (pl / 12) * 25)
            self.train = 6
            self.kill_rate = 0.25
            self.kill_exp = lambda pl: 12 + pl * 1.5 + 3
            self.bt = lambda r, spi, era: self.bt_new(r, spi, era)
            # อายุขัยเพิ่มตามแรงค์ (แนวฝึกยุทธ์): E–B เล็กน้อย · A +25 · S +60 · SS +120 · SSS +250 · U +500 · M +1000 · G +3000
            self.life_bonus = lambda r: LIFE[r]
            # เสี่ยงตายในงาน: แรงค์ต่ำเสี่ยงสูง (ภารกิจ/สงคราม) แรงค์สูงรอดง่าย
            self.death_month = lambda r: [0.0035, 0.003, 0.0028, 0.0025, 0.002, 0.0016, 0.0012, 0.001, 0.0008, 0.0006, 0.0004][r]

    @staticmethod
    def bt_cur(r, spi, era):
        c = 0.6 - r * 0.045 + spi * 0.004
        c = max(0.05, min(0.95, c))
        if r + 1 >= 6: c *= 0.06 * 0.5 ** (r + 1 - 6)
        if r + 1 >= 5 and era < 4: c *= 0.35
        if r + 1 >= 6 and era >= 5: c *= 2.5
        return min(0.95, c)

    @staticmethod
    def bt_new(r, spi, era):
        # E→D 80% · D→C 65% · C→B 50% · B→A 35% · A→S 20% · S→SS 3% · SS→SSS 1.2% · SSS→U 0.5% · U→M 0.2% · M→G 0.08%
        base = BT[r]
        c = base * (1 + spi * 0.01)
        if r + 1 >= 5 and era < 4: c *= 0.5          # S ก่อนยุคเวทมนตร์ยากขึ้น
        if r + 1 >= 6 and era >= 5: c *= STAR_ERA     # ยุคดารา ตำนานเกิดง่ายขึ้น
        return min(0.95, c)


def life(p, era, rnd):
    genius = rnd.random() < 0.03
    spi = rnd.randint(3, 10) + (6 if genius else 0)
    age = rnd.randint(18, 28)
    lifespan = rnd.gauss(72, 10)
    sum0 = spi * 7
    r = 0 if sum0 < 60 else 1 if sum0 < 72 else 2 if sum0 < 86 else 3 if sum0 < 100 else 4
    r = min(r, 5)
    star, exp, months = 1, 0, 0
    hist = {r: 0}
    while True:
        months += 1
        if months % 12 == 0:
            age += 1
            spi += 0.3
        if age > lifespan + p.life_bonus(r): return r, age, hist, "old"
        if rnd.random() < p.death_month(r): return r, age, hist, "battle"
        gain = p.train + sum(p.kill_exp(r * 12 + star) for _ in range(poisson(p.kill_rate, rnd)))
        if genius: gain *= 2
        exp += gain
        while star < STAR and exp >= p.exp_next(r * 12 + star):
            exp -= p.exp_next(r * 12 + star); star += 1
        if star >= STAR and exp >= p.exp_next(r * 12 + star) and r < 10:
            if rnd.random() < p.bt(r, spi, era):
                r += 1; star = 1; exp = 0; hist[r] = months / 12
            else:
                exp *= 0.7


def poisson(lam, rnd):
    L, k, p = math.exp(-lam), 0, 1.0
    while True:
        p *= rnd.random()
        if p <= L: return k
        k += 1


def run(mode, era=4, n=20000, seed=1):
    p = P(mode); rnd = random.Random(seed)
    final = [0] * 11; years_to = {k: [] for k in range(11)}; ages = []; alive_weight = [0.0] * 11
    for _ in range(n):
        r, age, hist, cause = life(p, era, rnd)
        final[r] += 1; ages.append(age)
        for k, y in hist.items():
            if y: years_to[k].append(y)
        # ประมาณ "สัดส่วนที่มีชีวิตอยู่ ณ เวลาใดเวลาหนึ่ง" ด้วยเวลาที่อยู่ในแต่ละแรงค์
        last = 0; keys = sorted(hist)
        for i, k in enumerate(keys):
            end = hist[keys[i + 1]] if i + 1 < len(keys) else (age - 22)
            alive_weight[k] += max(0, end - hist[k])
    tw = sum(alive_weight)
    print("=== %s (ยุค %d) ===" % (mode, era))
    print("แรงค์สุดท้ายตอนตาย / สัดส่วนที่พบในโลก / ปีเฉลี่ยที่ใช้ถึงแรงค์")
    for k in range(11):
        if final[k] or alive_weight[k]:
            yt = sum(years_to[k]) / len(years_to[k]) if years_to[k] else 0
            print("  %-3s ตาย %6.2f%%   พบในโลก %6.2f%%   ใช้เวลา %5.1f ปี" % (RANKS[k], 100 * final[k] / n, 100 * alive_weight[k] / tw, yt))
    print("  อายุตายเฉลี่ย %.1f ปี · อายุสูงสุด %d" % (sum(ages) / n, max(ages)))


if __name__ == "__main__":
    mode = sys.argv[1] if len(sys.argv) > 1 else "new"
    for era in ((2, 5) if mode == "new" else (4,)):
        run(mode, era)
