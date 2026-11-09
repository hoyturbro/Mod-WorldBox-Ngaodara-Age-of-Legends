using System;
using System.Collections.Generic;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NgaodaraPack
{
    /// <summary>
    /// เมนูเทพ: แก้ไขสิ่งมีชีวิตที่เลือก ใช้หน้าต่าง ปุ่ม แถบ และฟอนต์ของเกมเอง
    /// ไอคอนทั้งหมดเป็น PNG ใน GameResources/ui/Icons/ngd
    /// </summary>
    public static class GodMenu
    {
        public const string WindowId = "ngd_god_menu";
        const float W = 196f;

        static ScrollWindow _window;
        static Transform _content;
        static Actor _target;

        // ส่วนที่ต้องอัปเดตตามตัวที่เลือก
        static Image _rankBadge;
        static Text _header, _sub, _info;
        static SimpleStatBar _hpBar, _expBar;
        static readonly TextInput[] _statInputs = new TextInput[7];
        static TextInput _rankInput, _starInput;
        static readonly SimpleButton[] _rankBtns = new SimpleButton[11];
        static readonly SimpleButton[] _starBtns = new SimpleButton[12];
        static readonly Image[] _classFrames = new Image[Classes.All.Length];
        static Image _immortalFrame;

        public static Actor Target => _target;
        internal static TextInput StatInput(int i) => _statInputs[i];

        public static void Open(Actor a)
        {
            if (a == null || !a.isAlive()) { Tip("ngd_no_target"); return; }
            _target = a;
            NgdData.EnsureStats(a);
            EditorTabs.SetTarget(a);
            ActionLibrary.openUnitWindow(a);   // เครื่องมือแก้ไขอยู่ในแท็บข้างของหน้าต่าง Info
            NgdTicker.Later(0.35f, () => EditorTabs.ShowTab(EditorTabs.TabStats));
        }

        /// <summary>ตัวที่ถูกเลือกล่าสุด จำไว้เพราะการกดปุ่มในแถบพลังจะล้างการเลือกของเกม</summary>
        public static Actor LastSelected;

        public static void OpenForSelected()
        {
            Actor a = SelectedUnit.isSet() ? SelectedUnit.unit : null;
            if (a == null || !a.isAlive()) a = LastSelected;
            if ((a == null || !a.isAlive()) && _target != null && _target.isAlive()) a = _target;
            Open(a);
        }

        public static void Tip(string key)
        {
            try { WorldTip.showNow(key, true, "top", 2f); } catch { }
        }

        // ---------------- สร้างหน้าต่าง ----------------
        static void EnsureWindow()
        {
            if (_window != null) return;
            _window = WindowCreator.CreateEmptyWindow(WindowId, WindowId, "ngd/eye");
            _content = _window.transform.Find("Background/Scroll View/Viewport/Content");
            BuildUI();
            _window.gameObject.AddComponent<GodMenuUpdater>();
        }

        static Transform _embedded;

        /// <summary>ฝังเมนูเทพไว้ท้ายหน้าต่าง Info ของยูนิต ผูกกับตัวที่หน้าต่างแสดงอยู่จริง</summary>
        public static void AttachTo(Transform unitContent, Actor a)
        {
            if (unitContent == null || a == null) return;
            if (_embedded == null || _embedded.parent != unitContent)
            {
                var go = new GameObject("ngd_god_panel", typeof(RectTransform));
                go.transform.SetParent(unitContent, false);
                var le = go.AddComponent<LayoutElement>();
                le.minWidth = W + 8; le.preferredWidth = W + 8;
                _embedded = go.transform;
                _content = _embedded;
                BuildUI();
                go.AddComponent<GodMenuUpdater>();
            }
            _embedded.SetAsLastSibling();
            _content = _embedded;
            if (_target != a) { _target = a; NgdData.EnsureStats(a); }
            Refresh();
        }

        static void BuildUI()
        {
            var vlg = Get<VerticalLayoutGroup>(_content.gameObject);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 3;
            vlg.padding = new RectOffset(4, 4, 6, 10);
            vlg.childControlHeight = false; vlg.childControlWidth = false;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = false;
            Get<ContentSizeFitter>(_content.gameObject).verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // หัว: ป้ายแรงค์ + ชื่อ
            var head = Row(22);
            _rankBadge = MakeImage(head, Icon("r_E"), 20);
            _header = MakeText(head, W - 26, 20, 10, TextAnchor.MiddleLeft);
            _sub = MakeText(_content, W, 12, 8, TextAnchor.MiddleLeft);

            _hpBar = MakeBar(Icon("heal"), new Color(0.85f, 0.25f, 0.3f));
            _expBar = MakeBar(Icon("exp"), new Color(0.3f, 0.65f, 0.95f));
            _info = MakeText(_content, W, 34, 8, TextAnchor.UpperLeft);

            // ค่าสถานะ
            Section("ngd_sec_stats");
            for (int i = 0; i < 7; i++)
            {
                int idx = i;
                var r = Row(18);
                var ic = MakeImage(r, Icon("s_" + NgdData.StatKeys[i]), 14);
                Tooltip(ic.gameObject, "ngd_stat_" + NgdData.StatKeys[i], "ngd_stat_" + NgdData.StatKeys[i] + "_desc");
                MakeText(r, 30, 16, 9, TextAnchor.MiddleLeft).text = NgdData.StatNames[i];
                _statInputs[i] = MakeInput(r, 44, v =>
                {
                    if (int.TryParse(v.Trim(), out int n)) NgdData.SetStat(_target, idx, n);
                });
                IconButton(r, "minus", "ngd_btn_minus", () => NgdData.SetStat(_target, idx, NgdData.GetStatL(_target, idx) - 1));
                IconButton(r, "plus", "ngd_btn_plus", () => NgdData.SetStat(_target, idx, NgdData.GetStatL(_target, idx) + 1));
                TextButton(r, "+10", "ngd_btn_plus10", () => NgdData.SetStat(_target, idx, NgdData.GetStatL(_target, idx) + 10));
            }

            // แรงค์และดาว
            Section("ngd_sec_rank");
            // กดเลือกแรงค์ (ป้าย E…G) และดาว 1–12 โดยตรง
            Transform rk = null;
            for (int i = 0; i < NgdData.RankNames.Length; i++)
            {
                if (i % 6 == 0) rk = Row(22);
                int ri = i;
                _rankBtns[i] = IconButton(rk, "r_" + NgdData.RankNames[i], "ngd_btn_rank_up",
                    () => { Progression.Awaken(_target); NgdData.SetRank(_target, ri); });
            }
            TextButton(rk, "-", "ngd_no_rank", () => NgdData.SetRank(_target, -1));
            Transform st = null;
            for (int i = 0; i < NgdData.MaxStar; i++)
            {
                if (i % 6 == 0) st = Row(22);
                int si = i + 1;
                _starBtns[i] = TextButton(st, si + "★", "ngd_btn_star_up",
                    () => { Progression.Awaken(_target); NgdData.SetStar(_target, si); });
            }
            var rr = Row(22);
            IconButton(rr, "minus", "ngd_btn_rank_down", () => NgdData.SetRank(_target, NgdData.GetRank(_target) - 1));
            IconButton(rr, "r_E", "ngd_btn_rank_up", () => { Progression.Awaken(_target); NgdData.SetRank(_target, NgdData.GetRank(_target) + 1); });
            IconButton(rr, "minus", "ngd_btn_star_down", () => NgdData.SetStar(_target, NgdData.GetStar(_target) - 1));
            IconButton(rr, "star16", "ngd_btn_star_up", () => { Progression.Awaken(_target); NgdData.SetStar(_target, NgdData.GetStar(_target) + 1); });
            IconButton(rr, "break", "ngd_btn_break", () =>
            {
                Progression.Awaken(_target);
                if (NgdData.GetRank(_target) >= NgdData.RankNames.Length - 1) { Tip("ngd_tip_max_rank"); return; }
                Progression.TryBreakthrough(_target, true);
            });
            TextButton(rr, "G12", "ngd_btn_max", () =>
            {
                Progression.Awaken(_target);
                NgdData.SetRank(_target, NgdData.RankNames.Length - 1);
                NgdData.SetStar(_target, NgdData.MaxStar);
            });

            var er = Row(22);
            IconButton(er, "exp", "ngd_btn_exp", () => Progression.AddExp(_target, Progression.ExpToNext(_target)));
            IconButton(er, "pts", "ngd_btn_pts", () => { Progression.Awaken(_target); NgdData.SetPoints(_target, NgdData.GetPoints(_target) + 10); });
            IconButton(er, "dice", "ngd_btn_allot", () => Classes.AutoAllocate(_target));

            // อาชีพ
            Section("ngd_sec_class");
            Transform cr = null;
            for (int i = 0; i < Classes.All.Length; i++)
            {
                if (i % 7 == 0) cr = Row(22);
                var c = Classes.All[i];
                var btn = IconButton(cr, "c_" + c.Id, "ngd_class_" + c.Id, () => { Progression.Awaken(_target); NgdData.SetClass(_target, c.Id); });
                _classFrames[i] = btn.Background;
            }

            // อาวุธเทพ
            Section("ngd_sec_relic");
            var wr = Row(22);
            foreach (var r in Arsenal.Relics)
            {
                var rid = r.Id;
                var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, wr);
                b.Setup(() => Do(() => Arsenal.Give(_target, rid)), SpriteTextureLoader.getSprite("ui/Icons/items/icon_" + rid), null, new Vector2(20, 20));
                b.name = "btn_relic_" + rid;
                b.TipButton.SetTextTooltip(() => LM.Get("item_" + rid), () => LM.Get(rid + "_description"), true);
            }

            // คลังอุปกรณ์: เซ็ตธาตุ 8 ชุด + ชุดวัสดุ 4 ระดับ
            Section("ngd_sec_armory");
            Transform er8 = null;
            for (int i = 0; i < Gear.Elements.Length; i++)
            {
                if (i % 7 == 0) er8 = Row(22);
                var el = Gear.Elements[i].Id;
                IconButton(er8, "e_" + el, "ngd_set_" + el, () => Gear.EquipSet(_target, el));
            }
            var mr = Row(22);
            for (int t = 0; t < Gear.Materials.Length; t++)
            {
                int tt = t;
                IconButton(mr, "m_" + Gear.Materials[t], "ngd_mat_" + Gear.Materials[t], () => Gear.EquipMaterial(_target, tt));
            }

            // สถานะ
            Section("ngd_sec_status");
            var sr = Row(22);
            IconButton(sr, "heal", "ngd_btn_heal", () => _target.restoreHealth(_target.getMaxHealth()));
            _immortalFrame = IconButton(sr, "immortal", "ngd_btn_immortal", () => NgdData.SetImmortal(_target, !NgdData.IsImmortal(_target))).Background;
            IconButton(sr, "young", "ngd_btn_young", () => SetAge(_target, 18));
            IconButton(sr, "older", "ngd_btn_older", () => SetAge(_target, _target.getAge() + 10));
            IconButton(sr, "kill", "ngd_btn_kill", () =>
            {
                NgdData.SetImmortal(_target, false);
                _target.data.set("ngd_heal_after", false);
                _target.getHit(_target.getMaxHealth() * 10f, true, AttackType.Divine, null, false, false, false);
                if (_target.isAlive()) _target.die(false, AttackType.Divine, true, true);   // บางนิสัยของเกมชุบชีวิตเหลือ 1 HP
            });

        }

        /// <summary>อายุในเกมคิดจาก created_time จึงต้องเลื่อนเวลาเกิดแทนการตั้งค่า age</summary>
        static void SetAge(Actor a, int years)
        {
            years = Math.Max(0, years);
            double yearLen = Date.getMonthTime() * 12.0;
            a.data.created_time = World.world.getCurWorldTime() - years * yearLen;
            a.setStatsDirty();
        }

        // ---------------- ตัวช่วยสร้าง UI ----------------
        static T Get<T>(GameObject go) where T : Component => go.GetComponent<T>() ?? go.AddComponent<T>();

        static Sprite Icon(string name) => SpriteTextureLoader.getSprite("ui/Icons/ngd/" + name);

        static Transform Row(float h)
        {
            var go = new GameObject("row", typeof(RectTransform));
            go.transform.SetParent(_content, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(W, h);
            var hl = go.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 3;
            hl.childControlWidth = false; hl.childControlHeight = false;
            hl.childForceExpandWidth = false; hl.childForceExpandHeight = false;
            hl.childAlignment = TextAnchor.MiddleLeft;
            return go.transform;
        }

        static void Section(string key)
        {
            var t = MakeText(_content, W, 14, 9, TextAnchor.MiddleLeft);
            t.text = "<color=#ffd76a>" + LM.Get(key) + "</color>";
        }

        static Text MakeText(Transform parent, float w, float h, int size, TextAnchor anchor)
        {
            var go = new GameObject("text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(w, h);
            var t = go.AddComponent<Text>();
            t.font = LocalizedTextManager.current_font;
            t.fontSize = size;
            t.color = Color.white;
            t.alignment = anchor;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var sh = go.AddComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.6f);
            sh.effectDistance = new Vector2(1, -1);
            return t;
        }

        /// <summary>ช่องพิมพ์ค่า (ใช้ช่องกรอกของเกมผ่าน NML) ยืนยันเมื่อกด Enter หรือคลิกออก</summary>
        static TextInput MakeInput(Transform parent, float w, Action<string> onCommit)
        {
            var t = UnityEngine.Object.Instantiate(TextInput.Prefab, parent);
            t.Setup("", _ => { }, null, null);
            t.SetSize(new Vector2(w, 18));
            t.text.font = LocalizedTextManager.current_font;
            t.text.fontSize = 9;
            t.text.alignment = TextAnchor.MiddleCenter;
            t.input.onEndEdit.AddListener(v => Do(() => onCommit(v)));
            return t;
        }

        static void SetInput(TextInput t, string v)
        {
            if (t == null || t.input.isFocused) return;   // ไม่ทับค่าขณะผู้เล่นกำลังพิมพ์
            if (t.input.text != v) t.input.SetTextWithoutNotify(v);
        }

        static Image MakeImage(Transform parent, Sprite s, float size)
        {
            var go = new GameObject("img", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(size, size);
            var img = go.AddComponent<Image>();
            img.sprite = s;
            img.preserveAspect = true;
            return img;
        }

        static SimpleStatBar MakeBar(Sprite icon, Color col)
        {
            var bar = UnityEngine.Object.Instantiate(SimpleStatBar.Prefab, _content);
            bar.Setup(0, 1, "", icon, null, col, new Vector2(W - 8, 14), true, true, true, 0.3f);
            return bar;
        }

        static void Tooltip(GameObject go, string titleKey, string descKey)
        {
            var tip = go.GetComponent<TipButton>() ?? go.AddComponent<TipButton>();
            tip.SetTextTooltip(() => LM.Get(titleKey), () => LM.Get(descKey), true);
        }

        static SimpleButton IconButton(Transform parent, string icon, string tipKey, Action act)
        {
            var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, parent);
            b.Setup(() => Do(act), Icon(icon), null, new Vector2(20, 20));
            b.name = "btn_" + icon;
            b.TipButton.SetTextTooltip(() => LM.Get(tipKey), () => Desc(tipKey), true);
            return b;
        }

        static string Desc(string key)
        {
            string k = key + "_desc", v = LM.Get(k);
            return v == k ? "" : v;
        }

        static SimpleButton TextButton(Transform parent, string text, string tipKey, Action act)
        {
            var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, parent);
            b.Setup(() => Do(act), null, text, new Vector2(24, 20));
            b.name = "btn_" + text;
            b.Text.font = LocalizedTextManager.current_font;
            b.Text.fontSize = 8;
            b.TipButton.SetTextTooltip(() => LM.Get(tipKey), () => "", true);
            return b;
        }

        static void Do(Action act)
        {
            if (_target == null || !_target.isAlive()) { Tip("ngd_no_target"); return; }
            try
            {
                act();
                if (_target.isAlive() && _target.getHealth() > 0) _target.data.set("ngd_heal_after", true);
                _target.setStatsDirty();
            }
            catch (Exception e) { NgdMod.LogOnce("menu: " + e); }
            Refresh();
        }

        // ---------------- อัปเดตข้อมูล ----------------
        public static void Refresh()
        {
            if (_content == null || _target == null) return;
            var a = _target;
            if (!a.isAlive()) { _header.text = "<color=#888888>" + LM.Get("ngd_dead") + "</color>"; return; }

            int r = NgdData.GetRank(a);
            _rankBadge.enabled = r >= 0;
            if (r >= 0) _rankBadge.sprite = Icon("r_" + NgdData.RankNames[r]);

            string col = r >= 0 ? NgdData.RankColors[r] : "#cccccc";
            _header.text = "<b>" + Legends.FullName(a) + "</b>" + (r >= 0
                ? "  <color=" + col + ">" + NgdData.RankNames[r] + " " + Stars(NgdData.GetStar(a)) + "</color>"
                : "  <color=#aaaaaa>" + LM.Get("ngd_no_rank") + "</color>");

            var c = Classes.Get(NgdData.GetClass(a));
            string tags = "";
            if (NgdData.IsGenius(a)) tags += " <color=#ffd76a>" + LM.Get("ngd_genius") + "</color>";
            if (NgdData.IsImmortal(a)) tags += " <color=#ff9a9a>" + LM.Get("ngd_immortal") + "</color>";
            if (Cult.IsCultist(a)) tags += " <color=#c070f0>" + LM.Get(Cult.IsExposed(a) ? "ngd_cultist_exposed" : "ngd_cultist") + "</color>";
            _sub.text = (c != null ? Classes.TierName(c, r) : LM.Get("ngd_no_class")) + tags;

            _hpBar.UpdateBar(a.getHealth(), Math.Max(1, a.getMaxHealth()), "/" + a.getMaxHealth(),
                new Color(0.85f, 0.25f, 0.3f), false, false, true, 0.3f);
            int need = Progression.ExpToNext(a);
            _expBar.UpdateBar(NgdData.GetExp(a), need, "/" + need,
                new Color(0.3f, 0.65f, 0.95f), false, false, true, 0.3f);

            string bt = Progression.CanBreakthrough(a)
                ? "  <color=#ffd76a>" + LM.Get("ngd_ready") + " " + (int)(Progression.BreakthroughChance(a) * 100) + "%</color>"
                : "";
            _info.text = "CP <color=#ffd76a>" + NgdData.CombatPower(a) + "</color>   " + LM.Get("ngd_points") + " " + NgdData.GetPoints(a)
                         + "   " + LM.Get("ngd_age") + " " + a.getAge() + bt;

            for (int i = 0; i < 7; i++) SetInput(_statInputs[i], NgdData.GetStat(a, i).ToString());
            SetInput(_rankInput, r >= 0 ? NgdData.RankNames[r] : "-");
            SetInput(_starInput, r >= 0 ? NgdData.GetStar(a).ToString() : "-");

            string cid = NgdData.GetClass(a);
            for (int i = 0; i < Classes.All.Length; i++)
                if (_classFrames[i] != null) _classFrames[i].color = Classes.All[i].Id == cid ? new Color(1f, 0.85f, 0.4f) : Color.white;
            int curR = NgdData.GetRank(a), curS = NgdData.GetStar(a);
            for (int i = 0; i < _rankBtns.Length; i++)
                if (_rankBtns[i] != null) _rankBtns[i].Background.color = i == curR ? new Color(1f, 0.85f, 0.4f) : Color.white;
            for (int i = 0; i < _starBtns.Length; i++)
                if (_starBtns[i] != null) _starBtns[i].Background.color = curR >= 0 && i + 1 == curS ? new Color(1f, 0.85f, 0.4f) : Color.white;
            if (_immortalFrame != null) _immortalFrame.color = NgdData.IsImmortal(a) ? new Color(1f, 0.85f, 0.4f) : Color.white;
        }

        static string Stars(int n)
        {
            return n + "★";
        }
    }

    /// <summary>อัปเดตเมนูเทพเป็นระยะขณะเปิดอยู่ (เลือด/EXP เปลี่ยนตามเวลาจริง)</summary>
    public class GodMenuUpdater : MonoBehaviour
    {
        float _t;
        void Update()
        {
            _t += Time.unscaledDeltaTime;
            if (_t < 0.4f) return;
            _t = 0;
            GodMenu.Refresh();
        }
    }
}
