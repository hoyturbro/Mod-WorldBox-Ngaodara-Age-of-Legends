using System;
using System.Collections.Generic;
using System.Linq;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NgaodaraPack
{
    /// <summary>
    /// แท็บข้างของหน้าต่าง Info (โคลนปุ่มแท็บของเกมเอง หน้าตาและการทำงานเหมือนแท็บ "นิสัย/อุปกรณ์")
    ///   1) แก้ค่าสถานะ: ค่า 7 ตัว (ช่องกรอกกว้าง) · แรงค์/ดาว · อาชีพ → แก้เป็น "ร่าง" แล้วกด บันทึก / ยกเลิก / รีเซ็ต
    ///   2) คลังอาวุธ: อาวุธเทพ · เซ็ตธาตุ (ทั้งชุด/รายชิ้น) · ชุดวัสดุ · ถอดทั้งหมด
    ///   3) พลังเทพ: ปุ่มมีชื่อกำกับ (อมตะ เปิด/ปิด, ฟื้นเลือด, อายุ, EXP, ทะลวง, สังหาร ...)
    /// </summary>
    public static class EditorTabs
    {
        const float W = 206f;
        public const string TabStats = "ngd_tab_stats", TabArmory = "ngd_tab_armory", TabDivine = "ngd_tab_divine", TabSkills = "ngd_tab_skills";

        static Actor _target;
        public static Actor Target => _target;
        static WindowMetaTabButtonsContainer _cont;
        static readonly Dictionary<string, WindowMetaTab> _tabs = new Dictionary<string, WindowMetaTab>();

        // ---------- ร่างการแก้ไข ----------
        class Draft
        {
            public long[] Stats = new long[7];
            public int Rank, Star, Level;
            public string Cls, Name;
            public int[] Info = new int[InfoKeys.Length];   // อายุ ฆ่า เงิน ของปล้น ความสุข ความอิ่ม ชื่อเสียง เลือด มานา สตามินา
            public Draft Clone() => new Draft { Stats = (long[])Stats.Clone(), Rank = Rank, Star = Star, Cls = Cls, Level = Level, Name = Name, Info = (int[])Info.Clone() };
            public bool Same(Draft o) => o != null && Rank == o.Rank && Star == o.Star && Cls == o.Cls && Level == o.Level && Name == o.Name
                                         && Stats.SequenceEqual(o.Stats) && Info.SequenceEqual(o.Info);
        }

        static readonly string[] InfoKeys = { "age", "kills", "money", "loot", "happiness", "nutrition", "renown", "health", "mana", "stamina" };
        const int I_AGE = 0, I_HP = 7;
        static Draft _draft, _current;
        static readonly Dictionary<long, Draft> _original = new Dictionary<long, Draft>();

        static Draft Read(Actor a)
        {
            NgdData.EnsureStats(a);
            var d = new Draft { Rank = NgdData.GetRank(a), Star = NgdData.GetStar(a), Cls = NgdData.GetClass(a), Level = a.data.level };
            for (int i = 0; i < 7; i++) d.Stats[i] = NgdData.GetStatL(a, i);
            d.Name = a.getName();
            d.Info = new[] { a.getAge(), a.data.kills, a.data.money, a.data.loot, a.data.happiness, a.data.nutrition, a.data.renown, a.getHealth(), a.getMana(), a.data.stamina };
            return d;
        }

        // ---------- อ้างอิง UI ----------
        static Text _statsHeader, _statsInfo, _armoryHeader, _divineHeader, _applyLabel;
        static Image _badge;
        static readonly TextInput[] _inputs = new TextInput[7];
        static readonly TextInput[] _infoInputs = new TextInput[10];
        static TextInput _nameInput;
        public static TextInput StatInput(int i) => _inputs[i];
        static Transform _kidsBox;
        static long _kidsFor = -1; static int _kidsCount = -1;
        static TextInput _levelInput;
        static readonly SimpleButton[] _rankBtns = new SimpleButton[11];
        static readonly SimpleButton[] _starBtns = new SimpleButton[12];
        static readonly Dictionary<string, SimpleButton> _classBtns = new Dictionary<string, SimpleButton>();
        static SimpleButton _immortalBtn;
        static Text _immortalLabel;

        static readonly Color Gold = new Color(1f, 0.85f, 0.4f);
        static readonly Color DirtyCol = new Color(1f, 0.93f, 0.45f);

        public static void SetTarget(Actor a)
        {
            if (a == null) return;
            if (_target != a)
            {
                _target = a;
                _current = Read(a);
                _draft = _current.Clone();
                if (!_original.ContainsKey(a.data.id)) _original[a.data.id] = _current.Clone();
                foreach (var t in _inputs) if (t != null) t.input.SetTextWithoutNotify("");
            }
        }

        /// <summary>เรียกจาก UnitWindow.showStatsRows: ฝังแท็บ (ครั้งแรก) และผูกกับตัวที่แสดงอยู่</summary>
        public static void Inject(UnitWindow w, Actor a)
        {
            SetTarget(a);
            var cont = w.GetComponentsInChildren<WindowMetaTabButtonsContainer>(true).FirstOrDefault(c => c.name == "Tabs")
                       ?? w.GetComponentInChildren<WindowMetaTabButtonsContainer>(true);
            if (cont == null) { NgdMod.LogOnce("editor: tab container not found"); return; }
            if (_cont != cont || !_tabs.ContainsKey(TabStats) || _tabs[TabStats] == null)
            {
                _cont = cont;
                _tabs.Clear();
                Build(w, cont);
            }
            Refresh(true);
        }

        public static void ShowTab(string id)
        {
            try { if (_cont != null && _tabs.TryGetValue(id, out var t) && t != null) _cont.showTab(t); }
            catch (Exception e) { NgdMod.LogOnce("editor show tab: " + e.Message); }
        }

        static void Build(UnitWindow w, WindowMetaTabButtonsContainer cont)
        {
            var tpl = cont.GetComponentsInChildren<WindowMetaTab>(true).FirstOrDefault(t => t.name == "Traits")
                      ?? cont.GetComponentsInChildren<WindowMetaTab>(true).FirstOrDefault();
            var content = w.transform.Find("Background/Scroll View/Viewport/Content");
            if (tpl == null || content == null) { NgdMod.LogOnce("editor: template/content missing"); return; }
            CityTabs.TwoColumns(cont, 3);   // แท็บเกินความสูงหน้าต่าง → แถวที่ 2 ด้านนอก
            MakeTab(cont, tpl, content, TabStats, "tab_stats", BuildStats);
            AttachToVanillaTab(cont, content, "Equipment", TabArmory, BuildArmory);
            MakeTab(cont, tpl, content, TabDivine, "tab_divine", BuildDivine);
            MakeTab(cont, tpl, content, TabSkills, "tab_skills", BuildSkills);
            content.gameObject.AddComponent<EditorUpdater>();
        }

        /// <summary>ใส่เนื้อหาของมอดต่อท้ายแท็บที่เกมมีอยู่แล้ว (เช่นคลังอาวุธ → แท็บ Equipment ของเกม)</summary>
        static void AttachToVanillaTab(WindowMetaTabButtonsContainer cont, Transform content, string vanillaTab, string id, Action<Transform> build)
        {
            var tab = cont.GetComponentsInChildren<WindowMetaTab>(true).FirstOrDefault(t => t.name == vanillaTab);
            if (tab == null) { NgdMod.LogOnce("vanilla tab missing: " + vanillaTab); return; }
            var root = new GameObject(id + "_content", typeof(RectTransform));
            root.transform.SetParent(content, false);
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter; vlg.spacing = 3; vlg.padding = new RectOffset(4, 4, 4, 8);
            vlg.childControlHeight = false; vlg.childControlWidth = false; vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = false;
            root.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var le = root.AddComponent<LayoutElement>(); le.preferredWidth = W + 8; le.minWidth = W + 8;
            build(root.transform);
            root.SetActive(false);
            if (tab.tab_elements == null) tab.tab_elements = new List<Transform>();
            tab.tab_elements.Add(root.transform);
            _tabs[id] = tab;
        }

        static void MakeTab(WindowMetaTabButtonsContainer cont, WindowMetaTab tpl, Transform content, string id, string icon, Action<Transform> build)
        {
            var go = UnityEngine.Object.Instantiate(tpl.gameObject, cont.transform);
            go.name = id;
            var tab = go.GetComponent<WindowMetaTab>();
            var iconImg = go.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImg != null) iconImg.sprite = Icon(icon);
            var tip = go.GetComponent<TipButton>();
            if (tip != null) { tip.textOnClick = id; tip.textOnClickDescription = id + "_desc"; }
            tab._worldtip_text = LM.Get(id);
            tab.tab_action = new WindowMetaTabEvent();
            tab.tab_action.AddListener(t => cont.showTab(t));

            var root = new GameObject(id + "_content", typeof(RectTransform));
            root.transform.SetParent(content, false);
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 3;
            vlg.padding = new RectOffset(4, 4, 4, 8);
            vlg.childControlHeight = false; vlg.childControlWidth = false;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = false;
            root.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var le = root.AddComponent<LayoutElement>(); le.preferredWidth = W + 8; le.minWidth = W + 8;
            build(root.transform);
            root.SetActive(false);
            tab.tab_elements = new List<Transform> { root.transform };
            if (!cont._tabs.Contains(tab)) cont._tabs.Add(tab);
            _tabs[id] = tab;
        }

        // =====================================================================
        // แท็บ 1: แก้ค่าสถานะ
        // =====================================================================
        static void BuildStats(Transform p)
        {
            Title(p, "ngd_editor_title");
            var head = Row(p, 24);
            _badge = Img(head, Icon("r_E"), 22);
            _statsHeader = Txt(head, W - 28, 24, 9, TextAnchor.MiddleLeft);
            _statsInfo = Txt(p, W, 24, 8, TextAnchor.UpperLeft);

            Section(p, "ngd_sec_basic");
            var nr = Row(p, 20);
            Txt(nr, 34, 18, 8, TextAnchor.MiddleLeft).text = LM.Get("ngd_f_name");
            _nameInput = TextField(nr, 166, v => { if (!string.IsNullOrEmpty(v.Trim())) { _draft.Name = v.Trim(); Refresh(true); } });
            for (int i = 0; i < InfoKeys.Length; i += 2)
            {
                var ir = Row(p, 20);
                for (int k = i; k < i + 2 && k < InfoKeys.Length; k++)
                {
                    int idx = k;
                    Txt(ir, 38, 18, 8, TextAnchor.MiddleLeft).text = LM.Get("ngd_f_" + InfoKeys[k]);
                    _infoInputs[k] = Input(ir, 62, v => { if (ParseBig(v, int.MaxValue, out long n)) _draft.Info[idx] = (int)Math.Max(0, n); Refresh(true); });
                }
            }

            Section(p, "ngd_sec_stats");
            for (int i = 0; i < 7; i++)
            {
                int idx = i;
                var r = Row(p, 20);
                var ic = Img(r, Icon("s_" + NgdData.StatKeys[i]), 14);
                Tooltip(ic.gameObject, "ngd_stat_" + NgdData.StatKeys[i], "ngd_stat_" + NgdData.StatKeys[i] + "_desc");
                Txt(r, 26, 18, 9, TextAnchor.MiddleLeft).text = NgdData.StatNames[i];
                TxtBtn(r, "-10", 22, "ngd_btn_minus", () => Bump(idx, -10));
                TxtBtn(r, "-", 16, "ngd_btn_minus", () => Bump(idx, -1));
                _inputs[i] = Input(r, 72, v => { if (ParseBig(v, NgdData.StatMax, out long n)) _draft.Stats[idx] = Math.Max(1L, n); Refresh(true); });
                TxtBtn(r, "+", 16, "ngd_btn_plus", () => Bump(idx, 1));
                TxtBtn(r, "+10", 22, "ngd_btn_plus10", () => Bump(idx, 10));
            }

            // เลเวลของเกม (แยกจากแรงค์กิลด์)
            Section(p, "ngd_sec_level");
            var lr = Row(p, 20);
            Img(lr, Icon("exp"), 14);
            Txt(lr, 26, 18, 9, TextAnchor.MiddleLeft).text = "LV";
            TxtBtn(lr, "-10", 22, "ngd_btn_level_minus", () => { _draft.Level = Mathf.Clamp(_draft.Level - 10, 1, Balance.LevelMax); Refresh(true); });
            TxtBtn(lr, "-", 16, "ngd_btn_level_minus", () => { _draft.Level = Mathf.Clamp(_draft.Level - 1, 1, Balance.LevelMax); Refresh(true); });
            _levelInput = Input(lr, 72, v =>
            {
                if (ParseBig(v, Balance.LevelMax, out long n)) _draft.Level = Mathf.Clamp((int)n, 1, Balance.LevelMax);
                Refresh(true);
            });
            TxtBtn(lr, "+", 16, "ngd_btn_level_plus", () => { _draft.Level = Mathf.Clamp(_draft.Level + 1, 1, Balance.LevelMax); Refresh(true); });
            TxtBtn(lr, "+10", 22, "ngd_btn_level_plus", () => { _draft.Level = Mathf.Clamp(_draft.Level + 10, 1, Balance.LevelMax); Refresh(true); });

            Section(p, "ngd_sec_rank");
            Transform rk = null;
            for (int i = 0; i < NgdData.RankNames.Length; i++)
            {
                if (i % 6 == 0) rk = Row(p, 22);
                int ri = i;
                _rankBtns[i] = IconBtn(rk, "r_" + NgdData.RankNames[i], 22, "ngd_pick_rank", () => { _draft.Rank = ri; Refresh(true); });
            }
            TxtBtn(rk, LM.Get("ngd_no_rank_short"), 40, "ngd_no_rank", () => { _draft.Rank = -1; Refresh(true); });
            Transform st = null;
            for (int i = 0; i < NgdData.MaxStar; i++)
            {
                if (i % 6 == 0) st = Row(p, 20);
                int si = i + 1;
                _starBtns[i] = TxtBtn(st, si + "★", 30, "ngd_pick_star", () => { _draft.Star = si; if (_draft.Rank < 0) _draft.Rank = 0; Refresh(true); });
            }

            Section(p, "ngd_sec_class");
            Transform cr = null;
            for (int i = 0; i < Classes.All.Length; i++)
            {
                if (i % 7 == 0) cr = Row(p, 24);
                var c = Classes.All[i];
                _classBtns[c.Id] = IconBtn(cr, "c_" + c.Id, 24, "ngd_class_" + c.Id, () => { _draft.Cls = c.Id; if (_draft.Rank < 0) _draft.Rank = 0; Refresh(true); });
            }

            Spacer(p, 4);
            var br = Row(p, 24);
            var apply = TxtBtn(br, LM.Get("ngd_btn_apply"), 84, "ngd_btn_apply", Apply, 24);
            apply.Background.color = new Color(0.55f, 1f, 0.55f);
            _applyLabel = apply.Text;
            TxtBtn(br, LM.Get("ngd_btn_cancel"), 56, "ngd_btn_cancel", () => { _draft = _current.Clone(); ClearInputs(); Refresh(true); }, 24).Background.color = new Color(1f, 0.6f, 0.6f);
            BuildKids(p);
            TxtBtn(br, LM.Get("ngd_btn_reset"), 56, "ngd_btn_reset", () =>
            {
                if (_target != null && _original.TryGetValue(_target.data.id, out var o)) { _draft = o.Clone(); ClearInputs(); Refresh(true); }
            }, 24).Background.color = new Color(0.8f, 0.8f, 0.85f);
        }

        static void Bump(int idx, int d) { _draft.Stats[idx] = Math.Max(1L, Math.Min(NgdData.StatMax, _draft.Stats[idx] + d)); Refresh(true); }
        static void ClearInputs() { foreach (var t in _inputs) if (t != null) t.input.SetTextWithoutNotify(""); _levelInput?.input.SetTextWithoutNotify(""); }

        /// <summary>บันทึกร่างลงตัวละคร แล้วแสดงผลที่เปลี่ยนจริง (เลือด/พลังโจมตี)</summary>
        public static void Apply()
        {
            var a = _target;
            if (a == null || !a.isAlive()) { GodMenu.Tip("ngd_no_target"); return; }
            if (_draft.Same(_current)) { Toast(LM.Get("ngd_nothing_to_save")); return; }
            int hp0 = a.getMaxHealth(); int dmg0 = (int)a.stats["damage"];
            try
            {
                if (_draft.Rank >= 0 && NgdData.GetRank(a) < 0) Progression.Awaken(a);
                for (int i = 0; i < 7; i++) if (_draft.Stats[i] != _current.Stats[i]) NgdData.SetStat(a, i, _draft.Stats[i]);
                NgdData.SetRank(a, _draft.Rank);
                if (_draft.Rank >= 0) NgdData.SetStar(a, _draft.Star);
                NgdData.SetClass(a, _draft.Cls);
                if (_draft.Level != a.data.level) { a.data.level = Mathf.Clamp(_draft.Level, 1, Balance.LevelMax); a.data.experience = 0; }
                if (_draft.Name != _current.Name && !string.IsNullOrEmpty(_draft.Name)) a.setName(_draft.Name, true);
                var inf = _draft.Info; var cur = _current.Info;
                if (inf[I_AGE] != cur[I_AGE]) SetAge(a, inf[I_AGE]);
                if (inf[1] != cur[1]) a.data.kills = inf[1];
                if (inf[2] != cur[2]) a.data.money = inf[2];
                if (inf[3] != cur[3]) a.data.loot = inf[3];
                if (inf[4] != cur[4]) a.data.happiness = inf[4];
                if (inf[5] != cur[5]) a.data.nutrition = inf[5];
                if (inf[6] != cur[6]) a.data.renown = inf[6];
                if (inf[8] != cur[8]) a.data.mana = inf[8];
                if (inf[9] != cur[9]) a.data.stamina = inf[9];
                bool hpEdited = inf[I_HP] != cur[I_HP];
                int hpWant = inf[I_HP];
                if (hpEdited) NgdTicker.Later(0.1f, () => { if (a.isAlive()) a.setHealth(Math.Max(1, Math.Min(hpWant, a.getMaxHealth())), true); });
                else a.data.set("ngd_heal_after", true);
                a.setStatsDirty();
            }
            catch (Exception e) { NgdMod.LogOnce("editor apply: " + e); }
            _current = Read(a);
            _draft = _current.Clone();
            ClearInputs();
            Refresh(true);
            var who = a;
            NgdTicker.Later(0.15f, () =>
            {
                if (who == null || !who.isAlive()) return;
                Toast(string.Format(LM.Get("ngd_saved"), hp0, who.getMaxHealth(), dmg0, (int)who.stats["damage"]));
            });
        }

        // =====================================================================
        // แท็บ 2: คลังอาวุธ
        // =====================================================================
        static void BuildArmory(Transform p)
        {
            Title(p, "ngd_armory_title");
            _armoryHeader = Txt(p, W, 24, 8, TextAnchor.UpperLeft);

            Section(p, "ngd_sec_relic");
            Transform wr = null; int k = 0;
            foreach (var r in Arsenal.Relics)
            {
                if (k++ % 8 == 0) wr = Row(p, 24);
                var rid = r.Id;
                var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, wr);
                b.Setup(() => Act(() => Arsenal.Give(_target, rid), "item_" + rid), SpriteTextureLoader.getSprite("ui/Icons/items/icon_" + rid), null, new Vector2(24, 24));
                b.TipButton.SetTextTooltip(() => LM.Get("item_" + rid), () => LM.Get(rid + "_description"), true);
            }

            Section(p, "ngd_sec_sets");
            foreach (var el in Gear.Elements)
            {
                var id = el.Id;
                var r = Row(p, 22);
                IconBtn(r, "e_" + id, 22, "ngd_set_" + id, () => Act(() => Gear.EquipSet(_target, id), "ngd_set_" + id));
                Txt(r, 46, 20, 8, TextAnchor.MiddleLeft).text = "<color=" + el.Color + ">" + LM.Get("ngd_el_" + id) + "</color>";
                foreach (var piece in Gear.Pieces)
                {
                    var itemId = Gear.SetId(id, piece);
                    var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, r);
                    b.Setup(() => Act(() => Gear.Equip(_target, itemId), "item_" + itemId), SpriteTextureLoader.getSprite("ui/Icons/items/icon_" + itemId), null, new Vector2(20, 20));
                    b.TipButton.SetTextTooltip(() => LM.Get("item_" + itemId), () => LM.Get(itemId + "_description"), true);
                }
            }

            Section(p, "ngd_sec_materials");
            var mr = Row(p, 24);
            for (int t = 0; t < Gear.Materials.Length; t++)
            {
                int tt = t;
                IconBtn(mr, "m_" + Gear.Materials[t], 24, "ngd_mat_" + Gear.Materials[t], () => Act(() => Gear.EquipMaterial(_target, tt), "ngd_mat_" + Gear.Materials[tt]));
            }
            TxtBtn(mr, LM.Get("ngd_btn_unequip"), 70, "ngd_btn_unequip", () => Act(() => Gear.UnequipAll(_target), "ngd_btn_unequip"), 24);
        }

        // =====================================================================
        // แท็บ 3: พลังเทพ (ปุ่มมีชื่อกำกับ)
        // =====================================================================
        static void BuildDivine(Transform p)
        {
            Title(p, "ngd_divine_title");
            _divineHeader = Txt(p, W, 24, 8, TextAnchor.UpperLeft);

            Section(p, "ngd_sec_life");
            var r1 = Row(p, 24);
            _immortalBtn = Labeled(r1, "immortal", "ngd_btn_immortal", () => NgdData.SetImmortal(_target, !NgdData.IsImmortal(_target)));
            _immortalLabel = _immortalBtn.Text;
            Labeled(r1, "heal", "ngd_btn_heal", () => _target.restoreHealth(_target.getMaxHealth()));
            var r2 = Row(p, 24);
            Labeled(r2, "young", "ngd_btn_young", () => SetAge(_target, 18));
            Labeled(r2, "older", "ngd_btn_older", () => SetAge(_target, _target.getAge() + 10));

            Section(p, "ngd_sec_power");
            var r0 = Row(p, 24);
            Labeled(r0, "plus", "ngd_btn_level_up", () => LevelUp(_target, 1));
            Labeled(r0, "plus", "ngd_btn_level_up10", () => LevelUp(_target, 10));
            var r3 = Row(p, 24);
            Labeled(r3, "exp", "ngd_btn_exp", () => Progression.AddExp(_target, Progression.ExpToNext(_target)));
            Labeled(r3, "break", "ngd_btn_break", () =>
            {
                Progression.Awaken(_target);
                if (NgdData.GetRank(_target) < NgdData.RankNames.Length - 1) Progression.TryBreakthrough(_target, true);
            });
            var r4 = Row(p, 24);
            Labeled(r4, "pts", "ngd_btn_pts", () => { Progression.Awaken(_target); NgdData.SetPoints(_target, NgdData.GetPoints(_target) + 10); });
            Labeled(r4, "dice", "ngd_btn_allot", () => Classes.AutoAllocate(_target));
            var r5 = Row(p, 24);
            Labeled(r5, "star16", "ngd_btn_awaken", () => Progression.Awaken(_target));
            Labeled(r5, "minus", "ngd_btn_unrank", () => NgdData.SetRank(_target, -1));

            Section(p, "ngd_sec_danger");
            var r6 = Row(p, 24);
            var kill = Labeled(r6, "kill", "ngd_btn_kill", () =>
            {
                NgdData.SetImmortal(_target, false);
                _target.data.set("ngd_heal_after", false);
                _target.getHit(_target.getMaxHealth() * 10f, true, AttackType.Divine, null, false, false, false);
                if (_target.isAlive()) _target.die(false, AttackType.Divine, true, true);
            });
            kill.Background.color = new Color(1f, 0.55f, 0.55f);
        }

        /// <summary>ขึ้นเลเวลของเกมจริง (ผ่าน addExperience → levelUp ของเกม) ชนเพดานเกมแล้วจึงตั้งค่าตรง</summary>
        public static void LevelUp(Actor a, int times)
        {
            for (int i = 0; i < times; i++)
            {
                if (a.data.level >= Balance.LevelMax) { a.data.level = Balance.LevelMax; break; }
                int lv = a.data.level;
                try { a.addExperience(Math.Max(1, a.getExpToLevelup() - a.data.experience)); } catch { }
                if (a.data.level == lv) { a.data.level = lv + 1; a.data.experience = 0; }
            }
            a.setStatsDirty();
        }

        // =====================================================================
        // แท็บ 4: สกิล + Skill Tree (หน้าเดียวกับเมนูเทพ: ดู · ใส่/ถอดสล็อต · เรียน)
        // =====================================================================
        static Transform _skillRoot;
        static string _skillSig = "";
        static string _branch = "blade";

        static void BuildSkills(Transform p)
        {
            _skillRoot = p;
            Title(p, "ngd_skills_title");
        }

        static string Sec(float v) => v <= 0 ? LM.Get("ngd_sk_instant") : v.ToString("0.#") + " " + LM.Get("ngd_sk_sec");

        public static string DescribeSkill(Actor a, Skills.Skill s) => Describe(a, s);

        /// <summary>รายละเอียดสกิลเรียงทีละบรรทัด</summary>
        static string Describe(Actor a, Skills.Skill s)
        {
            var sb = new System.Text.StringBuilder();
            if (s.Passive)
            {
                string k = s.PassKey == "damage_pct" ? "ngd_pas_damage" : s.PassKey == "health_pct" ? "ngd_pas_health" : s.PassKey == "crit" ? "ngd_pas_crit" : "ngd_pas_cd";
                sb.Append("  " + string.Format(LM.Get(k), (int)(s.PassVal * 100)));
                return sb.ToString();
            }
            sb.Append("  " + LM.Get("ngd_sk_kind_" + s.Kind));
            if (s.Mult > 0)
            {
                sb.Append("\n  " + LM.Get("ngd_sk_damage") + ": " + (int)(s.Mult * 100) + "%");
                if (s.Stat >= 0) sb.Append(" +" + s.Coef.ToString("0.#") + "%/" + NgdData.StatNames[s.Stat]);
                sb.Append("  ≈ <color=#ffd76a>" + Num.Short(Skills.Damage(a, s)) + "</color>" + (s.Hits > 1 ? " × " + s.Hits : ""));
            }
            if (s.HealPct > 0) sb.Append("\n  " + LM.Get("ngd_sk_heal") + ": " + (int)(s.HealPct * 100) + "% HP");
            sb.Append("\n  " + LM.Get("ngd_sk_cd") + ": " + Sec(s.Cooldown) + "   " + LM.Get("ngd_sk_cast") + ": " + Sec(s.Cast));
            if (s.Range > 0 || s.Radius > 0)
                sb.Append("\n  " + (s.Range > 0 ? LM.Get("ngd_sk_range") + ": " + s.Range.ToString("0.#") + "   " : "")
                          + (s.Radius > 0 ? LM.Get("ngd_sk_area") + ": " + s.Radius.ToString("0.#") : ""));
            if (s.Mana > 0) sb.Append("\n  " + LM.Get("ngd_sk_mana") + ": " + s.Mana);
            if (s.TStatus != null) sb.Append("\n  " + LM.Get("ngd_sk_effect") + ": " + LM.Get("ngd_st_" + s.TStatus) + " " + Sec(s.TTime));
            if (s.SStatus != null) sb.Append("\n  " + LM.Get("ngd_sk_effect") + " (" + LM.Get(s.Kind == Skills.Kind.BuffAllies || s.Kind == Skills.Kind.Heal ? "ngd_sk_allies" : "ngd_sk_self") + "): "
                                             + LM.Get("ngd_st_" + s.SStatus) + " " + Sec(s.STime));
            float left = Skills.ReadyIn(a, s);
            if (left > 0) sb.Append("\n  <color=#aaaaaa>" + string.Format(LM.Get("ngd_skill_cd_left"), left.ToString("0.0")) + "</color>");
            return sb.ToString();
        }

        static Text Para(Transform p, string text, int size = 8)
        {
            var t = Txt(p, W, 12, size, TextAnchor.UpperLeft);
            t.text = text;
            ((RectTransform)t.transform).sizeDelta = new Vector2(W, t.preferredHeight + 4);
            return t;
        }

        static void SkillChanged(Actor a) { a.data.set("ngd_loadout_dirty", true); _skillSig = ""; Refresh(true); }

        static void RefreshSkills(Actor a)
        {
            if (_skillRoot == null || !_skillRoot.gameObject.activeInHierarchy && _skillSig != "") return;
            var (slots, ult) = Skills.LoadoutOf(a);
            var learned = SkillTree.Learned(a);
            string sig = a.data.id + "|" + string.Join(",", slots.Select(s => s.Id)) + "|" + (ult?.Id ?? "-") + "|" + string.Join(",", learned) + "|" + SkillTree.Points(a)
                         + "|" + _branch + "|" + NgdData.GetRank(a) + "|" + NgdData.GetClass(a) + "|" + (int)(Time.time / 2f);
            if (sig == _skillSig) return;
            _skillSig = sig;
            for (int i = _skillRoot.childCount - 1; i >= 1; i--) UnityEngine.Object.Destroy(_skillRoot.GetChild(i).gameObject);
            var p = _skillRoot;
            int rank = NgdData.GetRank(a);
            var c = Classes.Get(NgdData.GetClass(a));

            // ---- หัว
            Para(p, "<b>" + Legends.FullName(a) + "</b>\n"
                    + (c != null ? Classes.TierName(c, Math.Max(0, rank)) : LM.Get("ngd_sk_no_class"))
                    + (rank >= 0 ? "   <color=" + NgdData.RankColors[rank] + ">" + NgdData.RankNames[rank] + "</color>" : "")
                    + "\n" + LM.Get("ngd_sp") + ": <color=#7fd0ff>" + SkillTree.Points(a) + "</color>", 9);
            var hr = Row(p, 20);
            TxtBtn(hr, LM.Get("ngd_sp_add5"), 64, "ngd_sp_add5", () => { SkillTree.AddPoints(a, 5); SkillChanged(a); }, 18);
            TxtBtn(hr, LM.Get("ngd_slots_auto"), 70, "ngd_slots_auto", () => { SkillTree.AutoSlots(a); SkillChanged(a); }, 18);
            TxtBtn(hr, LM.Get("ngd_tree_reset"), 64, "ngd_tree_reset", () => { SkillTree.ResetTree(a, true); SkillChanged(a); }, 18).Background.color = new Color(1f, 0.65f, 0.65f);
            var tr0 = Row(p, 24);
            var ob0 = TxtBtn(tr0, LM.Get("ngd_st_open"), 200, "ngd_st_open", () => SkillTreeWindow.Open(a), 22);
            ob0.Background.color = Gold;

            // ---- อัลติเมท
            Section(p, "ngd_sec_ult");
            if (ult != null)
            {
                Para(p, "<b><color=#ff9a5a>★ " + SkillTree.Name(ult) + "</color></b>  <color=#aaaaaa>[" + SkillTree.SourceName(ult) + "]</color>\n" + Describe(a, ult));
                var ur = Row(p, 20);
                TxtBtn(ur, LM.Get("ngd_ult_unique"), 100, "ngd_ult_unique", () => { a.data.set("ngd_ult_pick", "unique"); SkillChanged(a); }, 18);
                TxtBtn(ur, LM.Get("ngd_ult_class"), 100, "ngd_ult_class", () => { a.data.set("ngd_ult_pick", "class"); SkillChanged(a); }, 18);
            }
            else Para(p, "<color=#aaaaaa>" + LM.Get("ngd_ult_locked") + "</color>");

            // ---- สล็อต 6 ช่อง
            Section(p, "ngd_sec_slots");
            for (int i = 0; i < SkillTree.Slots; i++)
            {
                if (i >= slots.Count) { Para(p, "<color=#777777>" + (i + 1) + ". " + LM.Get("ngd_slot_empty") + "</color>"); continue; }
                var s = slots[i];
                var r = Row(p, 18);
                Txt(r, W - 46, 18, 8, TextAnchor.MiddleLeft).text = "<b><color=#ffd76a>" + (i + 1) + ". " + SkillTree.Name(s) + "</color></b> <color=#aaaaaa>[" + SkillTree.SourceName(s) + "]</color>";
                var sid = s.Id;
                TxtBtn(r, LM.Get("ngd_slot_remove"), 42, "ngd_slot_remove", () => { SkillTree.ToggleSlot(a, sid); SkillChanged(a); }, 16).Background.color = new Color(1f, 0.7f, 0.7f);
                Para(p, Describe(a, s));
            }

            // ---- สกิลที่ใช้ได้แต่ยังไม่ได้ใส่
            var avail = SkillTree.Available(a).Where(s => !slots.Contains(s)).ToList();
            if (avail.Count > 0)
            {
                Section(p, "ngd_sec_avail");
                foreach (var s in avail)
                {
                    var r = Row(p, 18);
                    Txt(r, W - 46, 18, 8, TextAnchor.MiddleLeft).text = SkillTree.Name(s) + " <color=#aaaaaa>[" + SkillTree.SourceName(s) + "]</color>";
                    var sid = s.Id;
                    TxtBtn(r, LM.Get("ngd_slot_add"), 42, "ngd_slot_add", () => { SkillTree.ToggleSlot(a, sid); SkillChanged(a); }, 16).Background.color = new Color(0.6f, 1f, 0.6f);
                }
            }

            // ---- Skill Tree: หน้าต่างใหญ่แยก (3 สายเคียงกัน)
            Section(p, "ngd_sec_tree");
            var learnedN = learned.Count;
            Para(p, string.Format(LM.Get("ngd_st_summary"), learnedN, SkillTree.Nodes.Count) + (SkillTree.TopBranch(a) != null ? "\n" + LM.Get("ngd_st_main") + " " + (SkillTree.Thai ? SkillTree.Branches.First(b => b.Id == SkillTree.TopBranch(a)).Th : SkillTree.Branches.First(b => b.Id == SkillTree.TopBranch(a)).En) : ""));
            var tr = Row(p, 26);
            var ob = TxtBtn(tr, "🌳 " + LM.Get("ngd_st_open"), 200, "ngd_st_open", () => SkillTreeWindow.Open(a), 24);
            ob.Background.color = Gold;
            ob.name = "btn_open_skilltree";
            var dr = Row(p, 24);
            Labeled(dr, "pts", "ngd_btn_dmgnum", () => DamageNumbers.Enabled = !DamageNumbers.Enabled);
        }

        static void SetAge(Actor a, int years)
        {
            years = Math.Max(0, years);
            double yearLen = Date.getMonthTime() * 12.0;
            a.data.created_time = World.world.getCurWorldTime() - years * yearLen;
            a.setStatsDirty();
        }

        /// <summary>ทำทันที (อุปกรณ์/พลังเทพ) แล้วแจ้งผล</summary>
        static void Act(Action act, string key)
        {
            if (_target == null || !_target.isAlive()) { GodMenu.Tip("ngd_no_target"); return; }
            try
            {
                act();
                if (_target.isAlive() && _target.getHealth() > 0) _target.data.set("ngd_heal_after", true);
                _target.setStatsDirty();
                Toast("✔ " + LM.Get(key));
            }
            catch (Exception e) { NgdMod.LogOnce("editor act: " + e); }
            Refresh(true);   // ร่างที่ยังไม่บันทึกคงไว้ ค่าที่เกมเปลี่ยนจะตามมาเองใน Refresh
        }

        static bool _dirtyFlag;

        internal static void Toast(string text)
        {
            try { WorldTip.showNow(text, false, "top", 2.5f); } catch { }
        }

        // =====================================================================
        // อัปเดต
        // =====================================================================
        public static void Refresh(bool full)
        {
            var a = _target;
            if (a == null || _statsHeader == null) return;
            if (!a.isAlive()) { _statsHeader.text = "<color=#888888>" + LM.Get("ngd_dead") + "</color>"; return; }
            // ค่าปัจจุบันเปลี่ยนจากเกม (เช่นขึ้นดาว) และยังไม่ได้แก้อะไร: ร่างตามค่าจริง
            var now = Read(a);
            if (!now.Same(_current))
            {   // ค่าที่ผู้เล่นยังไม่ได้แก้ ให้ตามค่าจริงของเกม (เลือด/มานา/อายุ/แต้มจากเลเวล ฯลฯ) — ช่องที่แก้ไว้คงค่าเดิม
                for (int i = 0; i < 7; i++) if (_draft.Stats[i] == _current.Stats[i]) _draft.Stats[i] = now.Stats[i];
                for (int i = 0; i < _draft.Info.Length; i++) if (_draft.Info[i] == _current.Info[i]) _draft.Info[i] = now.Info[i];
                if (_draft.Rank == _current.Rank) _draft.Rank = now.Rank;
                if (_draft.Star == _current.Star) _draft.Star = now.Star;
                if (_draft.Cls == _current.Cls) _draft.Cls = now.Cls;
                if (_draft.Level == _current.Level) _draft.Level = now.Level;
                if (_draft.Name == _current.Name) _draft.Name = now.Name;
                _current = now;
            }
            _dirtyFlag = !_draft.Same(_current);

            int r = _draft.Rank;
            _badge.enabled = r >= 0;
            if (r >= 0) _badge.sprite = Icon("r_" + NgdData.RankNames[r]);
            var c = Classes.Get(_draft.Cls);
            string col = r >= 0 ? NgdData.RankColors[r] : "#cccccc";
            _statsHeader.text = "<b>" + Legends.FullName(a) + "</b>\n"
                + (r >= 0 ? "<color=" + col + ">" + NgdData.RankNames[r] + " " + _draft.Star + "★</color>" : "<color=#aaaaaa>" + LM.Get("ngd_no_rank") + "</color>")
                + "  " + (c != null ? Classes.TierName(c, Math.Max(0, r)) : LM.Get("ngd_no_class"));

            long cpNow = NgdData.CombatPower(a), cpNew = PreviewCP(a, _draft);
            string cp = cpNew != cpNow ? cpNow + " → <color=#ffe070>" + cpNew + "</color>" : cpNow.ToString();
            _statsInfo.text = "HP " + a.getHealth() + "/" + a.getMaxHealth() + "   CP " + cp + "   LV " + a.data.level + " (" + a.data.experience + "/" + a.getExpToLevelup() + ")\n"
                + LM.Get("ngd_points") + " " + NgdData.GetPoints(a) + "   " + LM.Get("ngd_age") + " " + a.getAge()
                + "   " + LM.Get("ngd_lv_bonus") + " +" + Math.Max(0, a.data.level - 1) * Balance.PointsPerLevel
                + (_dirtyFlag ? "   <color=#ffe070>" + LM.Get("ngd_unsaved") + "</color>" : "");

            for (int i = 0; i < 7; i++)
            {
                var t = _inputs[i];
                if (t == null || t.input.isFocused) continue;
                string v = _draft.Stats[i] >= NgdData.StatMax ? "MAX" : _draft.Stats[i].ToString();
                if (t.input.text != v) t.input.SetTextWithoutNotify(v);
                t.text.color = _draft.Stats[i] != _current.Stats[i] ? DirtyCol : Color.white;
            }
            for (int i = 0; i < _infoInputs.Length; i++)
            {
                var t = _infoInputs[i];
                if (t == null || t.input.isFocused) continue;
                string v = _draft.Info[i].ToString();
                if (t.input.text != v) t.input.SetTextWithoutNotify(v);
                t.text.color = _draft.Info[i] != _current.Info[i] ? DirtyCol : Color.white;
            }
            if (_nameInput != null && !_nameInput.input.isFocused)
            {
                if (_nameInput.input.text != _draft.Name) _nameInput.input.SetTextWithoutNotify(_draft.Name ?? "");
                _nameInput.text.color = _draft.Name != _current.Name ? DirtyCol : Color.white;
            }
            RefreshKids(a);
            if (_levelInput != null && !_levelInput.input.isFocused)
            {
                string lv = Balance.LevelText(_draft.Level);
                if (_levelInput.input.text != lv) _levelInput.input.SetTextWithoutNotify(lv);
                _levelInput.text.color = _draft.Level != _current.Level ? DirtyCol : Color.white;
            }
            for (int i = 0; i < _rankBtns.Length; i++)
                if (_rankBtns[i] != null) _rankBtns[i].Background.color = i == _draft.Rank ? Gold : Color.white;
            for (int i = 0; i < _starBtns.Length; i++)
                if (_starBtns[i] != null) _starBtns[i].Background.color = _draft.Rank >= 0 && i + 1 == _draft.Star ? Gold : Color.white;
            foreach (var kv in _classBtns) if (kv.Value != null) kv.Value.Background.color = kv.Key == _draft.Cls ? Gold : Color.white;
            if (_applyLabel != null)
            {
                int n = 0;
                for (int i = 0; i < 7; i++) if (_draft.Stats[i] != _current.Stats[i]) n++;
                if (_draft.Rank != _current.Rank) n++;
                if (_draft.Star != _current.Star) n++;
                if (_draft.Cls != _current.Cls) n++;
                if (_draft.Level != _current.Level) n++;
                if (_draft.Name != _current.Name) n++;
                for (int i = 0; i < _draft.Info.Length; i++) if (_draft.Info[i] != _current.Info[i]) n++;
                _applyLabel.text = LM.Get("ngd_btn_apply") + (n > 0 ? " (" + n + ")" : "");
            }

            RefreshSkills(a);
            var (atk, def, pieces) = Gear.Info(a);
            string w = a.equipment?.weapon?.getItem()?.getAsset()?.id;
            if (_armoryHeader != null)
                _armoryHeader.text = LM.Get("ngd_weapon") + ": " + (w != null ? LM.Get("item_" + w) : "-") + "\n"
                    + LM.Get("ngd_set_bonus") + ": " + (def != null ? LM.Get("ngd_el_" + def) + " " + pieces + "/6" : "-");
            bool imm = NgdData.IsImmortal(a);
            if (_immortalBtn != null)
            {
                _immortalBtn.Background.color = imm ? Gold : Color.white;
                _immortalLabel.text = LM.Get(imm ? "ngd_immortal_on" : "ngd_immortal_off");
            }
            if (_divineHeader != null)
            {
                string tags = "";
                if (imm) tags += " <color=#ffd76a>" + LM.Get("ngd_immortal") + "</color>";
                if (NgdData.IsGenius(a)) tags += " <color=#ffd76a>" + LM.Get("ngd_genius") + "</color>";
                if (Cult.IsCultist(a)) tags += " <color=#c070f0>" + LM.Get("ngd_cultist") + "</color>";
                _divineHeader.text = "<b>" + Legends.FullName(a) + "</b>" + tags + "\nHP " + a.getHealth() + "/" + a.getMaxHealth()
                    + "   EXP " + NgdData.GetExp(a) + "/" + Progression.ExpToNext(a);
            }
        }

        /// <summary>CP ของร่าง (ใช้สูตรเดียวกับ NgdData.CombatPower)</summary>
        static long PreviewCP(Actor a, Draft d)
        {
            double sum = d.Stats.Sum();
            int pl = d.Rank < 0 ? 0 : d.Rank * NgdData.MaxStar + d.Star;
            float m = Balance.HealthMul(pl);
            float baseHp = a.getMaxHealth() / Math.Max(0.01f, NgdData.RankMultiplier(a)) - _current.Stats[2] * 12f;
            double hp = Math.Min(StatCaps.Big, (baseHp + d.Stats[2] * 12.0) * m);
            return (long)Math.Min(9e18, sum * 10.0 * m + hp * 0.5);
        }

        // =====================================================================
        // ตัวช่วยสร้าง UI (ฟอนต์/ปุ่ม/ช่องกรอกของเกม)
        // =====================================================================
        internal static Sprite Icon(string name) => SpriteTextureLoader.getSprite("ui/Icons/ngd/" + name);

        internal static Transform Row(Transform p, float h)
        {
            var go = new GameObject("row", typeof(RectTransform));
            go.transform.SetParent(p, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(W, h);
            var hl = go.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 2;
            hl.childControlWidth = false; hl.childControlHeight = false;
            hl.childForceExpandWidth = false; hl.childForceExpandHeight = false;
            hl.childAlignment = TextAnchor.MiddleCenter;
            return go.transform;
        }

        internal static void Spacer(Transform p, float h) { var go = new GameObject("space", typeof(RectTransform)); go.transform.SetParent(p, false); ((RectTransform)go.transform).sizeDelta = new Vector2(W, h); }

        internal static void Title(Transform p, string key)
        {
            var t = Txt(p, W, 16, 10, TextAnchor.MiddleCenter);
            t.text = "<b><color=#ffd76a>" + LM.Get(key) + "</color></b>";
        }

        internal static void Section(Transform p, string key)
        {
            var t = Txt(p, W, 14, 9, TextAnchor.MiddleLeft);
            t.text = "<color=#ffd76a>— " + LM.Get(key) + " —</color>";
        }

        internal static Text Txt(Transform parent, float w, float h, int size, TextAnchor anchor)
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
            t.raycastTarget = false;
            var sh = go.AddComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.6f);
            sh.effectDistance = new Vector2(1, -1);
            return t;
        }

        internal static Image Img(Transform parent, Sprite s, float size)
        {
            var go = new GameObject("img", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(size, size);
            var img = go.AddComponent<Image>();
            img.sprite = s;
            img.preserveAspect = true;
            return img;
        }

        static TextInput TextField(Transform parent, float w, Action<string> onCommit)
        {
            var t = Input(parent, w, onCommit);
            t.input.contentType = InputField.ContentType.Standard;
            t.input.characterLimit = 32;
            return t;
        }

        // ---------------- ลูก: ไปหา · ตัดออก · เปลี่ยนชื่อ · เพิ่มลูก ----------------
        static void BuildKids(Transform p)
        {
            Section(p, "ngd_sec_children");
            var box = new GameObject("kids", typeof(RectTransform));
            box.transform.SetParent(p, false);
            var vlg = box.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2; vlg.childControlHeight = false; vlg.childControlWidth = false; vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = false;
            box.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ((RectTransform)box.transform).sizeDelta = new Vector2(W, 0);
            _kidsBox = box.transform;
            var r = Row(p, 22);
            TxtBtn(r, LM.Get("ngd_add_son"), 100, "ngd_add_son", () => AddKid(ActorSex.Male), 20);
            TxtBtn(r, LM.Get("ngd_add_daughter"), 100, "ngd_add_daughter", () => AddKid(ActorSex.Female), 20);
        }

        static void AddKid(ActorSex sex)
        {
            var a = _target;
            if (a == null || !a.isAlive()) return;
            try
            {
                var kid = BabyMaker.makeBaby(a, null, sex, false, 0, null, true, false);
                if (kid != null) Toast("✔ " + string.Format(LM.Get("ngd_kid_born"), kid.getName()));
            }
            catch (Exception e) { NgdMod.LogOnce("add kid: " + e); }
            _kidsCount = -1;
        }

        static void RefreshKids(Actor a)
        {
            if (_kidsBox == null || a == null) return;
            var kids = new List<Actor>();
            try { foreach (var k in a.getChildren(false)) if (k != null && k.isAlive()) kids.Add(k); } catch { }
            if (_kidsFor == a.data.id && _kidsCount == kids.Count) return;
            _kidsFor = a.data.id; _kidsCount = kids.Count;
            for (int i = _kidsBox.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(_kidsBox.GetChild(i).gameObject);
            if (kids.Count == 0) { Txt(_kidsBox, W, 14, 8, TextAnchor.MiddleLeft).text = "<color=#aaaaaa>" + LM.Get("ngd_kid_none") + "</color>"; return; }
            foreach (var k in kids)
            {
                var kid = k;
                var r = Row(_kidsBox, 20);
                var nameIn = TextField(r, 96, v => { if (kid.isAlive() && !string.IsNullOrEmpty(v.Trim())) { kid.setName(v.Trim(), true); Toast("✔ " + v.Trim()); } });
                nameIn.input.SetTextWithoutNotify(kid.getName());
                Txt(r, 22, 18, 8, TextAnchor.MiddleCenter).text = kid.getAge().ToString();
                TxtBtn(r, LM.Get("ngd_kid_goto"), 38, "ngd_kid_goto", () => { if (kid.isAlive()) { World.world.locatePosition(kid.current_position); ActionLibrary.openUnitWindow(kid); } }, 18);
                TxtBtn(r, LM.Get("ngd_kid_remove"), 38, "ngd_kid_remove", () =>
                {
                    long me = _target != null ? _target.data.id : -1;
                    if (kid.data.parent_id_1 == me) kid.data.parent_id_1 = -1;
                    if (kid.data.parent_id_2 == me) kid.data.parent_id_2 = -1;
                    _kidsCount = -1; Toast("✔ " + LM.Get("ngd_kid_removed"));
                }, 18).Background.color = new Color(1f, 0.6f, 0.6f);
            }
        }

        /// <summary>อ่านตัวเลขที่ผู้เล่นพิมพ์: รองรับ 1,234 · 1.5k · 2m · 3b · MAX · ช่องว่าง/ผิด = ไม่เปลี่ยน</summary>
        internal static bool ParseBig(string v, long max, out long n)
        {
            n = 0;
            if (string.IsNullOrWhiteSpace(v)) return false;
            var t = v.Trim().Replace(",", "").Replace(" ", "").ToUpperInvariant();
            if (t == "MAX") { n = max; return true; }
            double mul = 1;
            if (t.EndsWith("K")) { mul = 1e3; t = t.Substring(0, t.Length - 1); }
            else if (t.EndsWith("M")) { mul = 1e6; t = t.Substring(0, t.Length - 1); }
            else if (t.EndsWith("B")) { mul = 1e9; t = t.Substring(0, t.Length - 1); }
            if (!double.TryParse(t, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d)) return false;
            d *= mul;
            if (double.IsNaN(d)) return false;
            n = (long)Math.Max(0, Math.Min(max, Math.Round(d)));
            return true;
        }

        static TextInput Input(Transform parent, float w, Action<string> onCommit)
        {
            var t = UnityEngine.Object.Instantiate(TextInput.Prefab, parent);
            t.Setup("", _ => { }, null, null);
            t.SetSize(new Vector2(w, 18));
            t.text.font = LocalizedTextManager.current_font;
            t.text.fontSize = 10;
            t.text.alignment = TextAnchor.MiddleCenter;
            // ห้ามใช้ IntegerNumber: Unity จะลบตัวอักษรตอนตั้งค่าข้อความ ทำให้ "MAX" กลายเป็นช่องว่าง (เลขหาย)
            t.input.contentType = InputField.ContentType.Standard;
            t.input.characterValidation = InputField.CharacterValidation.None;
            t.input.characterLimit = 16;
            t.input.onEndEdit.AddListener(v => onCommit(v));
            if (t.icon != null) t.icon.gameObject.SetActive(false);   // ไอคอนดินสอของ NML ล้นทับปุ่มข้าง ๆ
            return t;
        }

        static void Tooltip(GameObject go, string titleKey, string descKey)
        {
            var tip = go.GetComponent<TipButton>() ?? go.AddComponent<TipButton>();
            tip.SetTextTooltip(() => LM.Get(titleKey), () => LM.Get(descKey), true);
        }

        internal static string Desc(string key) { string k = key + "_desc"; return LocalizedTextManager.stringExists(k) ? LM.Get(k) : ""; }

        internal static SimpleButton IconBtn(Transform parent, string icon, float size, string tipKey, Action act)
        {
            var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, parent);
            b.Setup(() => act(), Icon(icon), null, new Vector2(size, size));
            b.name = "btn_" + icon;
            b.TipButton.SetTextTooltip(() => LM.Get(tipKey), () => Desc(tipKey), true);
            return b;
        }

        internal static SimpleButton TxtBtn(Transform parent, string text, float w, string tipKey, Action act, float h = 18)
        {
            var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, parent);
            b.Setup(() => act(), null, text, new Vector2(w, h));
            b.name = "btn_" + tipKey + "_" + text;
            b.Text.font = LocalizedTextManager.current_font;
            b.Text.fontSize = 8;
            b.TipButton.SetTextTooltip(() => LM.Get(tipKey), () => Desc(tipKey), true);
            return b;
        }

        /// <summary>ปุ่มใหญ่ ไอคอน + ชื่อ (เข้าใจได้ทันทีว่ากดแล้วทำอะไร)</summary>
        static SimpleButton Labeled(Transform parent, string icon, string key, Action act)
        {
            var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, parent);
            b.Setup(() => Act(act, key), null, LM.Get(key), new Vector2(100, 22));
            b.name = "btn_" + key;
            b.Text.font = LocalizedTextManager.current_font;
            b.Text.resizeTextForBestFit = false;
            b.Text.fontSize = 8;
            b.Text.alignment = TextAnchor.MiddleLeft;
            b.Text.horizontalOverflow = HorizontalWrapMode.Overflow;
            b.Text.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = b.Text.rectTransform; rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 1); rt.offsetMin = new Vector2(21, 0); rt.offsetMax = new Vector2(-3, 0);
            var ic = Img(b.transform, Icon(icon), 16);
            var irt = ic.rectTransform; irt.anchorMin = irt.anchorMax = new Vector2(0, 0.5f); irt.anchoredPosition = new Vector2(11, 0);
            b.TipButton.SetTextTooltip(() => LM.Get(key), () => Desc(key), true);
            return b;
        }
    }

    public class EditorUpdater : MonoBehaviour
    {
        float _t;
        void Update()
        {
            _t += Time.unscaledDeltaTime;
            if (_t < 0.4f) return;
            _t = 0;
            try
            {
                if (SelectedUnit.isSet() && SelectedUnit.unit != null && SelectedUnit.unit.isAlive() && SelectedUnit.unit != EditorTabs.Target)
                    EditorTabs.SetTarget(SelectedUnit.unit);
                EditorTabs.Refresh(false);
            } catch (Exception e) { NgdMod.LogOnce("editor refresh: " + e.Message); }
        }
    }
}
