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
    /// แท็บข้างของหน้าต่างเมือง (โคลนแท็บของเกม)
    ///   1) คลังเมือง: ทรัพยากรทุกชนิดของเกม แยกหมวด อาหาร/วัตถุดิบ/ของมีค่า เลือกจำนวน +10/+100/+1000/ลบ แล้วกดไอคอน
    ///   2) การปกครอง: ทอง เร่งวิจัย/ยุค ชื่อเสียงกิลด์ ล้างวิกฤต/หนี้ กวาดล้างลัทธิ รอยแยก มหันตภัย
    /// </summary>
    public static class CityTabs
    {
        const float W = 206f;
        public const string TabStock = "ngd_tab_city_stock", TabGov = "ngd_tab_city_gov", TabForge = "ngd_tab_city_forge";
        static City _city;
        public static City Target => _city;
        static WindowMetaTabButtonsContainer _cont;
        static readonly Dictionary<string, WindowMetaTab> _tabs = new Dictionary<string, WindowMetaTab>();
        static int _step = 100;
        static readonly Dictionary<int, SimpleButton> _stepBtns = new Dictionary<int, SimpleButton>();
        static readonly Dictionary<string, Text> _amounts = new Dictionary<string, Text>();
        static Text _stockHeader, _govHeader, _forgeHeader, _forgeSet, _forgeRelic;

        public static void Inject(CityWindow w, City c)
        {
            _city = c;
            var cont = w.GetComponentsInChildren<WindowMetaTabButtonsContainer>(true).FirstOrDefault(x => x.name == "Tabs")
                       ?? w.GetComponentInChildren<WindowMetaTabButtonsContainer>(true);
            if (cont == null) { NgdMod.LogOnce("city tabs: container not found"); return; }
            if (_cont != cont || !_tabs.ContainsKey(TabStock) || _tabs[TabStock] == null)
            {
                _cont = cont; _tabs.Clear(); _amounts.Clear(); _stepBtns.Clear();
                var tpl = cont.GetComponentsInChildren<WindowMetaTab>(true).FirstOrDefault(t => t.name != "MainTab" && t.gameObject.activeSelf)
                          ?? cont.GetComponentsInChildren<WindowMetaTab>(true).FirstOrDefault();
                var content = w.transform.Find("Background/Scroll View/Viewport/Content");
                if (tpl == null || content == null) { NgdMod.LogOnce("city tabs: template/content missing"); return; }
                TwoColumns(cont, 3);
                MakeTab(cont, tpl, content, TabStock, "tab_city_stock", BuildStock);
                MakeTab(cont, tpl, content, TabGov, "tab_city_gov", BuildGov);
                MakeTab(cont, tpl, content, TabForge, "tab_city_forge", BuildForge);
                content.gameObject.AddComponent<CityTabsUpdater>();
            }
            Refresh();
        }

        public static void ShowTab(string id)
        {
            try { if (_cont != null && _tabs.TryGetValue(id, out var t) && t != null) _cont.showTab(t); }
            catch (Exception e) { NgdMod.LogOnce("city show tab: " + e.Message); }
        }

        /// <summary>แท็บเกินความสูงหน้าต่าง → ขึ้นแถวที่ 2 ด้านนอก</summary>
        internal static void TwoColumns(WindowMetaTabButtonsContainer cont, int adding)
        {
            var grid = cont.GetComponent<GridLayoutGroup>();
            if (grid == null) return;
            int rows = 0;
            foreach (Transform ch in cont.transform) if (ch.gameObject.activeSelf) rows++;
            if (rows + adding <= 10) return;
            grid.startAxis = GridLayoutGroup.Axis.Vertical;
            grid.startCorner = GridLayoutGroup.Corner.UpperRight;
            grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            grid.constraintCount = Math.Max(1, rows);
            // เริ่มจากขวา (แถวเดิมของเกมอยู่ติดหน้าต่าง) แล้วเลื่อนทั้งกลุ่มไปซ้าย 1 แถว ให้แถวเดิมอยู่ที่เดิม
            var rt = (RectTransform)cont.transform;
            rt.anchoredPosition -= new Vector2(grid.cellSize.x + grid.spacing.x, 0);
        }

        static void MakeTab(WindowMetaTabButtonsContainer cont, WindowMetaTab tpl, Transform content, string id, string icon, Action<Transform> build)
        {
            var go = UnityEngine.Object.Instantiate(tpl.gameObject, cont.transform);
            go.name = id;
            var tab = go.GetComponent<WindowMetaTab>();
            var iconImg = go.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImg != null) iconImg.sprite = EditorTabs.Icon(icon);
            var tip = go.GetComponent<TipButton>();
            if (tip != null) { tip.textOnClick = id; tip.textOnClickDescription = id + "_desc"; }
            tab._worldtip_text = LM.Get(id);
            tab.tab_action = new WindowMetaTabEvent();
            tab.tab_action.AddListener(t => cont.showTab(t));
            var root = new GameObject(id + "_content", typeof(RectTransform));
            root.transform.SetParent(content, false);
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter; vlg.spacing = 3; vlg.padding = new RectOffset(4, 4, 4, 8);
            vlg.childControlHeight = false; vlg.childControlWidth = false; vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = false;
            root.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var le = root.AddComponent<LayoutElement>(); le.preferredWidth = W + 8; le.minWidth = W + 8;
            build(root.transform);
            root.SetActive(false);
            tab.tab_elements = new List<Transform> { root.transform };
            if (!cont._tabs.Contains(tab)) cont._tabs.Add(tab);
            _tabs[id] = tab;
        }

        // ------------------------------------------------ คลังเมือง
        static bool IsFood(ResourceAsset r) => r.type == ResType.Food || r.type == ResType.Ingredient_Food;
        static bool IsMaterial(ResourceAsset r) => r.type == ResType.Strategic || r.type == ResType.Ingredient;

        static void BuildStock(Transform p)
        {
            EditorTabs.Title(p, "ngd_city_stock_title");
            _stockHeader = EditorTabs.Txt(p, W, 24, 8, TextAnchor.UpperLeft);
            var sr = EditorTabs.Row(p, 22);
            EditorTabs.Txt(sr, 40, 20, 8, TextAnchor.MiddleLeft).text = LM.Get("ngd_city_amount");
            foreach (var n in new[] { 10, 100, 1000, -1 })
            {
                int nn = n;
                _stepBtns[n] = EditorTabs.TxtBtn(sr, n < 0 ? LM.Get("ngd_city_take") : "+" + n, 38, n < 0 ? "ngd_city_take" : "ngd_city_amount", () => { _step = nn; Refresh(); }, 20);
            }
            var all = AssetManager.resources.list.Where(r => r != null && !string.IsNullOrEmpty(r.path_icon) && Sprite(r) != null).ToList();
            Group(p, "ngd_city_food", all.Where(IsFood).ToList());
            Group(p, "ngd_city_materials", all.Where(IsMaterial).ToList());
            Group(p, "ngd_city_valuables", all.Where(r => !IsFood(r) && !IsMaterial(r)).ToList());
            var fr = EditorTabs.Row(p, 24);
            Big(fr, "ngd_city_fill_all", () => { foreach (var r in all) Give(r.id, Math.Abs(_step)); });
            Big(fr, "ngd_city_food_all", () => { foreach (var r in all.Where(IsFood)) Give(r.id, Math.Abs(_step)); });
        }

        static Sprite Sprite(ResourceAsset r) => SpriteTextureLoader.getSprite("ui/Icons/" + r.path_icon);

        static void Group(Transform p, string key, List<ResourceAsset> list)
        {
            if (list.Count == 0) return;
            EditorTabs.Section(p, key);
            Transform row = null;
            for (int i = 0; i < list.Count; i++)
            {
                if (i % 7 == 0) row = EditorTabs.Row(p, 30);
                var r = list[i]; var id = r.id;
                var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, row);
                b.Setup(() => Give(id, _step), Sprite(r), null, new Vector2(26, 26));
                b.name = "res_" + id;
                b.TipButton.SetTextTooltip(() => LocalizedTextManager.stringExists(id) ? LM.Get(id) : id, () => LM.Get("ngd_city_res_tip"), true);
                var t = EditorTabs.Txt(b.transform, 26, 26, 7, TextAnchor.LowerRight);
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                var rt = t.rectTransform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = new Vector2(1, 0); rt.offsetMax = new Vector2(-1, 0);
                _amounts[id] = t;
            }
        }

        static void Give(string id, int n)
        {
            var c = _city;
            if (c == null || !c.isAlive()) return;
            try
            {
                if (n < 0) { int have = c.getResourcesAmount(id); if (have > 0) c.takeResource(id, have); }
                else
                {
                    int added = AddSpread(c, id, n);
                    if (added < n) EditorTabs.Toast(string.Format(LM.Get("ngd_city_storage_full"), added, n));
                }
            }
            catch (Exception e) { NgdMod.LogOnce("city give " + id + ": " + e.Message); }
            Refresh();
        }

        /// <summary>คลังแต่ละหลังเก็บได้ไม่เกิน maximum ของทรัพยากร: กระจายใส่หลายคลังจนครบหรือเต็มทั้งเมือง</summary>
        public static int AddSpread(City c, string id, int n)
        {
            int start = c.getResourcesAmount(id), left = n;
            for (int k = 0; k < 40 && left > 0; k++)
            {
                int b0 = c.getResourcesAmount(id);
                c.addResourcesToRandomStockpile(id, left);
                left -= c.getResourcesAmount(id) - b0;
            }
            return c.getResourcesAmount(id) - start;
        }

        // ------------------------------------------------ การปกครอง
        static void BuildGov(Transform p)
        {
            EditorTabs.Title(p, "ngd_city_gov_title");
            _govHeader = EditorTabs.Txt(p, W, 36, 8, TextAnchor.UpperLeft);
            EditorTabs.Section(p, "ngd_city_sec_wealth");
            var r1 = EditorTabs.Row(p, 24);
            Big(r1, "ngd_city_gold100", () => AddSpread(_city, "gold", 100));
            Big(r1, "ngd_city_gold1000", () => AddSpread(_city, "gold", 1000));
            var r2 = EditorTabs.Row(p, 24);
            Big(r2, "ngd_city_clear_crisis", () => Economy.Set(_city, "crisis", 0));
            Big(r2, "ngd_city_clear_debt", () => { Economy.Set(_city, "debt", 0); Economy.Set(_city, "debt_late", 0); });

            EditorTabs.Section(p, "ngd_city_sec_progress");
            var r3 = EditorTabs.Row(p, 24);
            Big(r3, "ngd_city_era_up", () =>
            {
                int e = Mathf.Min(Eras.CityEra(_city) + 1, Eras.Ids.Length - 1);
                Economy.Set(_city, "research", Math.Max(Eras.Research(_city), Eras.ResearchNeeded[e]));
            });
            Big(r3, "ngd_city_research", () => Economy.Set(_city, "research", Eras.Research(_city) + 500));
            var r4 = EditorTabs.Row(p, 24);
            Big(r4, "ngd_city_fame", () => _city.data.set("ngd_guild_fame", GuildSystem.Fame(_city) + 50));
            Big(r4, "ngd_city_purge_cult", () =>
            {
                if (Economy.ByCity.TryGetValue(_city.getID(), out var ppl))
                    foreach (var a in ppl) if (a != null && a.isAlive()) { a.data.set("ngd_cult", false); a.data.set("ngd_cult_exposed", false); }
            });

            EditorTabs.Section(p, "ngd_sec_danger");
            var r5 = EditorTabs.Row(p, 24);
            Big(r5, "ngd_city_rift", () => Rifts.OpenNear(_city, -1)).Background.color = new Color(1f, 0.7f, 0.5f);
            Big(r5, "ngd_city_calamity", () => Calamity.Spawn(_city)).Background.color = new Color(1f, 0.55f, 0.55f);
        }

        // ------------------------------------------------ โรงตีเหล็กเทพ
        static void BuildForge(Transform p)
        {
            EditorTabs.Title(p, "ngd_city_forge_title");
            _forgeHeader = EditorTabs.Txt(p, W, 46, 8, TextAnchor.UpperLeft);
            EditorTabs.Section(p, "ngd_forge_sec_set");
            _forgeSet = EditorTabs.Txt(p, W, 82, 8, TextAnchor.UpperLeft);
            var r1 = EditorTabs.Row(p, 24);
            Big(r1, "ngd_forge_craft_set", () => Report(Forge.CraftSet(_city, out var h, out var it), h, it), false);
            Big(r1, "ngd_forge_fill_set", () => Fill(Forge.SetCost));
            EditorTabs.Section(p, "ngd_forge_sec_relic");
            _forgeRelic = EditorTabs.Txt(p, W, 82, 8, TextAnchor.UpperLeft);
            var r2 = EditorTabs.Row(p, 24);
            Big(r2, "ngd_forge_craft_relic", () => Report(Forge.CraftRelic(_city, out var h, out var it), h, it), false).Background.color = new Color(1f, 0.85f, 0.4f);
            Big(r2, "ngd_forge_fill_relic", () => Fill(Forge.RelicCost));
        }

        static void Fill((string id, int n)[] cost)
        {
            Forge.FillDeposit(_city, cost);
        }

        static void Report(string err, Actor hero, string item)
        {
            if (err == null) EditorTabs.Toast("✔ " + string.Format(LM.Get("ngd_forge_done"), LM.Get("item_" + item), hero?.getName()));
            else EditorTabs.Toast("✖ " + LM.Get(err));
        }

        static SimpleButton Big(Transform parent, string key, Action act, bool toast = true)
        {
            var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, parent);
            b.Setup(() =>
            {
                if (_city == null || !_city.isAlive()) return;
                try { act(); if (toast) EditorTabs.Toast("✔ " + LM.Get(key)); } catch (Exception e) { NgdMod.LogOnce("city act: " + e); }
                Refresh();
            }, null, LM.Get(key), new Vector2(100, 22));
            b.name = "btn_" + key;
            b.Text.font = LocalizedTextManager.current_font;
            b.Text.resizeTextForBestFit = false;
            b.Text.fontSize = 8;
            b.Text.alignment = TextAnchor.MiddleCenter;
            b.Text.horizontalOverflow = HorizontalWrapMode.Overflow;
            b.TipButton.SetTextTooltip(() => LM.Get(key), () => EditorTabs.Desc(key), true);
            return b;
        }

        public static void Refresh()
        {
            var c = _city;
            if (c == null || !c.isAlive() || _stockHeader == null) return;
            foreach (var kv in _amounts) if (kv.Value != null) { int n = c.getResourcesAmount(kv.Key); kv.Value.text = n <= 0 ? "" : n >= 1000 ? (n / 1000f).ToString("0.#") + "k" : n.ToString(); }
            foreach (var kv in _stepBtns) if (kv.Value != null) kv.Value.Background.color = kv.Key == _step ? new Color(1f, 0.85f, 0.4f) : Color.white;
            _stockHeader.text = "<b>" + c.name + "</b>  " + LM.Get("ngd_city_pop") + " " + c.getPopulationPeople() + "\n"
                + LM.Get("ngd_city_gold") + " " + Economy.Gold(c) + "   " + LM.Get("ngd_city_food_total") + " " + Economy.FoodOf(c);
            if (_forgeHeader != null)
            {
                var hs = Forge.BestHero(c, Forge.SetRank);
                var hr = Forge.BestHero(c, Forge.RelicRank);
                _forgeHeader.text = "<b>" + c.name + "</b>  " + Eras.Name(Eras.CityEra(c)) + "\n"
                    + (Forge.HasForge(c) ? "<color=#8aff8a>" + LM.Get("ngd_forge_has_forge") + "</color>" : "<color=#ff8a8a>" + LM.Get("ngd_forge_no_forge") + "</color>") + "\n"
                    + LM.Get("ngd_forge_hero") + " " + (hs != null ? hs.getName() + " (" + NgdData.RankNames[NgdData.GetRank(hs)] + ")" : "-");
                string next = hs != null ? Forge.NextSetPiece(hs) : null;
                _forgeSet.text = LM.Get("ngd_forge_next") + " " + (next != null ? LM.Get("item_" + next) : hs != null ? LM.Get("ngd_forge_set_full") : "-") + "\n" + Forge.CostText(c, Forge.SetCost);
                _forgeRelic.text = LM.Get("ngd_forge_for") + " " + (hr != null ? hr.getName() : "-") + "\n" + Forge.CostText(c, Forge.RelicCost);
            }
            if (_govHeader != null)
                _govHeader.text = "<b>" + c.name + "</b>  " + Eras.Name(Eras.CityEra(c)) + " (" + Eras.Research(c) + ")\n"
                    + LM.Get("ngd_city_gold") + " " + Economy.Gold(c) + "   " + LM.Get("ngd_row_debt") + " " + Economy.Get(c, "debt")
                    + (Economy.Get(c, "crisis") > 0 ? "   <color=#ff6060>" + LM.Get("ngd_row_crisis") + "</color>" : "") + "\n"
                    + (GuildSystem.HasGuild(c) ? LM.Get("ngd_guild_tier_" + GuildSystem.GuildTier(c)) + " (" + GuildSystem.Fame(c) + ")" : "-")
                    + "   " + LM.Get("ngd_cultist") + " " + Cult.CountIn(c);
        }
    }

    public class CityTabsUpdater : MonoBehaviour
    {
        float _t;
        void Update()
        {
            _t += Time.unscaledDeltaTime;
            if (_t < 0.5f) return;
            _t = 0;
            try
            {
                var sel = SelectedMetas.selected_city;
                if (sel != null && sel.isAlive() && sel != CityTabs.Target) CityTabs.Inject(GetComponentInParent<CityWindow>(), sel);
                else CityTabs.Refresh();
            }
            catch (Exception e) { NgdMod.LogOnce("city tabs refresh: " + e.Message); }
        }
    }
}
