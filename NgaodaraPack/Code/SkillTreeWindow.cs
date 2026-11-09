using System;
using System.Collections.Generic;
using System.Linq;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NgaodaraPack
{
    /// <summary>
    /// หน้าต่าง Skill Tree แบบกว้าง: 3 สายเคียงกัน (3 หน้า รวม 9 สาย)
    ///   โหนดเป็นไอคอน กรอบเรืองแสงสีประจำสาย · เส้นเชื่อม · กุญแจล็อก · โหนดสุดยอดด้านล่าง
    ///   กดโหนด = ดูรายละเอียด และเลือกเรียน (ยังไม่ใช้แต้มจริง) → กด "ตกลง" ค่อยเรียนจริง · "ล้างแต้ม" = คืนแต้มทั้งหมด
    /// </summary>
    public static class SkillTreeWindow
    {
        // หน่วยออกแบบ (สเกลตามความสูงหน้าต่างของเกม)
        const float DW = 600f, DH = 360f;
        static float U = 1f;

        static GameObject _overlay;
        static RectTransform _panel, _body;
        static Actor _a;
        static int _page;
        static string _sel;
        static readonly List<string> _pending = new List<string>();
        static Sprite _frameSprite;

        static readonly Dictionary<string, Color> BranchColor = new Dictionary<string, Color>
        {
            { "blade", Hex("#ffd24a") }, { "martial", Hex("#ff8a3a") }, { "archery", Hex("#9aff7a") },
            { "arcane", Hex("#7ab0ff") }, { "holy", Hex("#fff0a0") }, { "shadow", Hex("#c07aff") },
            { "nature", Hex("#7ae05a") }, { "command", Hex("#ff5a5a") }, { "beast", Hex("#ffb04a") },
        };
        static readonly string[][] Pages = { new[] { "blade", "martial", "archery" }, new[] { "arcane", "holy", "shadow" }, new[] { "nature", "command", "beast" } };

        static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

        public static bool IsOpen => _overlay != null && _overlay.activeSelf;
        public static Actor Target => _a;

        public static void Open(Actor a)
        {
            if (a == null || !a.isAlive()) { GodMenu.Tip("ngd_no_target"); return; }
            try
            {
                Ensure();
                if (_a != a) { _pending.Clear(); _sel = null; }
                _a = a;
                // เปิดหน้าที่มีสายของอาชีพ
                var mine = SkillTree.ClassBranches.TryGetValue(NgdData.GetClass(a) ?? "", out var bs) ? bs : null;
                if (mine != null && mine.Length > 0) for (int i = 0; i < Pages.Length; i++) if (Pages[i].Contains(mine[0])) _page = i;
                _overlay.SetActive(true);
                _overlay.transform.SetAsLastSibling();
                Rebuild();
            }
            catch (Exception e) { NgdMod.LogOnce("skill tree window: " + e); }
        }

        public static void Close() { if (_overlay != null) _overlay.SetActive(false); }

        // ---------------------------------------------------------------- โครง
        static void Ensure()
        {
            if (_overlay != null) return;
            WorldGen.Ensure();
            var sw = Resources.FindObjectsOfTypeAll<ScrollWindow>().FirstOrDefault(w => w.gameObject.scene.IsValid());
            var parent = sw != null ? sw.transform.parent : null;
            if (parent == null) throw new Exception("no window canvas");
            var bg = sw.transform.Find("Background")?.GetComponent<Image>();
            _frameSprite = bg != null ? bg.sprite : null;
            float winH = bg != null ? ((RectTransform)bg.transform).rect.height : 300f;
            U = Mathf.Max(0.5f, winH * 1.05f / DH);

            _overlay = new GameObject("ngd_skilltree", typeof(RectTransform));
            _overlay.transform.SetParent(parent, false);
            var ort = (RectTransform)_overlay.transform;
            ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one; ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;
            var dim = _overlay.AddComponent<Image>(); dim.color = new Color(0, 0, 0, 0.55f);
            _overlay.AddComponent<SkillTreeCloser>();

            var pgo = new GameObject("panel", typeof(RectTransform));
            pgo.transform.SetParent(_overlay.transform, false);
            _panel = (RectTransform)pgo.transform;
            _panel.anchorMin = _panel.anchorMax = new Vector2(0.5f, 0.5f);
            _panel.sizeDelta = new Vector2(DW * U, DH * U);
            var pimg = pgo.AddComponent<Image>();
            if (_frameSprite != null) { pimg.sprite = _frameSprite; pimg.type = Image.Type.Sliced; pimg.color = Color.white; }
            else pimg.color = new Color(0.16f, 0.17f, 0.15f, 0.98f);

            var body = new GameObject("body", typeof(RectTransform));
            body.transform.SetParent(_panel, false);
            _body = (RectTransform)body.transform;
            _body.anchorMin = Vector2.zero; _body.anchorMax = Vector2.one; _body.offsetMin = Vector2.zero; _body.offsetMax = Vector2.zero;
            _overlay.SetActive(false);
        }

        static RectTransform Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x * U, -y * U);
            rt.sizeDelta = new Vector2(w * U, h * U);
            return rt;
        }

        static Image Box(Transform p, float x, float y, float w, float h, Color c, Sprite s = null)
        {
            var go = new GameObject("box", typeof(RectTransform));
            go.transform.SetParent(p, false);
            Place((RectTransform)go.transform, x, y, w, h);
            var img = go.AddComponent<Image>();
            img.color = c; img.raycastTarget = false;
            if (s != null) { img.sprite = s; img.preserveAspect = true; }
            return img;
        }

        static Text Label(Transform p, float x, float y, float w, float h, int size, TextAnchor anchor, string text)
        {
            var t = EditorTabs.Txt(p, w * U, h * U, Mathf.Max(6, Mathf.RoundToInt(size * U)), anchor);
            Place((RectTransform)t.transform, x, y, w, h);
            t.text = text;
            return t;
        }

        static SimpleButton Btn(Transform p, float x, float y, float w, float h, string text, Color bg, Action act)
        {
            var b = UnityEngine.Object.Instantiate(SimpleButton.Prefab, p);
            b.Setup(() => { try { act(); } catch (Exception e) { NgdMod.LogOnce("skill tree btn: " + e); } Rebuild(); }, null, text, new Vector2(w * U, h * U));
            Place((RectTransform)b.transform, x, y, w, h);
            b.Background.color = bg;
            b.Text.font = LocalizedTextManager.current_font;
            b.Text.resizeTextForBestFit = false;
            b.Text.fontSize = Mathf.Max(6, Mathf.RoundToInt(9 * U));
            b.Text.alignment = TextAnchor.MiddleCenter;
            b.Text.horizontalOverflow = HorizontalWrapMode.Overflow;
            b.name = "btn_st_" + text;
            return b;
        }

        static void Line(Transform p, float x0, float y0, float x1, float y1, Color c)
        {
            float t = 2f;
            if (Mathf.Abs(x0 - x1) < 0.01f) Box(p, x0, (y0 + y1) / 2f, t, Mathf.Abs(y1 - y0) + t, c);
            else Box(p, (x0 + x1) / 2f, y0, Mathf.Abs(x1 - x0) + t, t, c);
        }

        // ---------------------------------------------------------------- สถานะโหนด
        static bool KnownOrPending(string id) => SkillTree.Knows(_a, id) || _pending.Contains(id);
        static int PendingCost() => _pending.Sum(id => SkillTree.Cost(SkillTree.Nodes[id]));
        static int FreePoints() => SkillTree.Points(_a) - PendingCost();

        /// <summary>เรียนได้ไหม (นับโหนดที่เลือกไว้แล้วเป็นเหมือนเรียนแล้ว)</summary>
        static bool CanPick(Skills.Skill n, out string why)
        {
            why = null;
            if (KnownOrPending(n.Id)) { why = "known"; return false; }
            if (n.Tier > 1)
            {
                bool prev = n.Tier == 5 ? KnownOrPending(n.Branch + "_a4") && KnownOrPending(n.Branch + "_p4")
                                        : KnownOrPending(n.Branch + "_a" + (n.Tier - 1)) || KnownOrPending(n.Branch + "_p" + (n.Tier - 1));
                if (!prev) { why = "prev"; return false; }
            }
            if (NgdData.GetRank(_a) < n.Tier - 1) { why = "rank"; return false; }
            if (FreePoints() < SkillTree.Cost(n)) { why = "points"; return false; }
            return true;
        }

        static void Click(string id)
        {
            _sel = id;
            var n = SkillTree.Nodes[id];
            if (_pending.Contains(id))
            {   // ยกเลิกที่เลือก (และโหนดที่ต่อจากมันซึ่งไม่ผ่านเงื่อนไขแล้ว)
                _pending.Remove(id);
                bool changed = true;
                while (changed)
                {
                    changed = false;
                    foreach (var pid in _pending.ToList())
                    {
                        _pending.Remove(pid);
                        if (CanPick(SkillTree.Nodes[pid], out _)) _pending.Add(pid); else changed = true;
                    }
                }
                return;
            }
            if (CanPick(n, out _)) _pending.Add(id);
        }

        static void Apply()
        {
            int ok = 0;
            foreach (var id in _pending.OrderBy(i => SkillTree.Nodes[i].Tier).ToList())
                if (SkillTree.Learn(_a, id, false)) ok++;
            _pending.Clear();
            _a.data.set("ngd_loadout_dirty", true);
            _a.setStatsDirty();
            EditorTabs.Toast("✔ " + string.Format(LM.Get("ngd_st_applied"), ok));
            EditorTabs.Refresh(true);
        }

        static void ResetAll()
        {
            _pending.Clear();
            SkillTree.ResetTree(_a, true);
            EditorTabs.Toast("✔ " + LM.Get("ngd_st_reset_done"));
            EditorTabs.Refresh(true);
        }

        // ---------------------------------------------------------------- วาด
        public static void Rebuild()
        {
            if (_body == null || _a == null) return;
            if (!_a.isAlive()) { Close(); return; }
            for (int i = _body.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(_body.GetChild(i).gameObject);
            var p = _body;
            bool th = SkillTree.Thai;
            int rank = NgdData.GetRank(_a);
            var mine = SkillTree.ClassBranches.TryGetValue(NgdData.GetClass(_a) ?? "", out var bs) ? bs : new string[0];

            // หัวเรื่อง
            Label(p, DW / 2, 16, 420, 18, 13, TextAnchor.MiddleCenter, "<b><color=#ffd76a>" + LM.Get("ngd_st_title") + "</color></b>");
            Label(p, DW / 2, 32, 420, 12, 8, TextAnchor.MiddleCenter, "<color=#dddddd>" + Legends.FullName(_a)
                  + (rank >= 0 ? "   <color=" + NgdData.RankColors[rank] + ">" + NgdData.RankNames[rank] + "</color>" : "") + "</color>");
            Btn(p, DW - 26, 18, 22, 18, "✕", new Color(1f, 0.55f, 0.55f), Close);
            // แท็บหน้า
            for (int i = 0; i < Pages.Length; i++)
            {
                int pi = i;
                var pb = Btn(p, 48 + 62 * i, 18, 58, 18, LM.Get("ngd_st_page_" + i), i == _page ? new Color(1f, 0.85f, 0.4f) : Color.white, () => { _page = pi; });
                pb.name = "btn_st_" + (i + 1);
                pb.Text.fontSize = Mathf.Max(6, Mathf.RoundToInt(8 * U));
            }

            // 3 คอลัมน์
            var page = Pages[Mathf.Clamp(_page, 0, Pages.Length - 1)];
            for (int c = 0; c < page.Length; c++) Column(p, page[c], 14 + c * 191, mine.Contains(page[c]), th);

            // แถบข้อมูลโหนดที่เลือก
            Box(p, DW / 2, 318, DW - 28, 30, new Color(0, 0, 0, 0.35f));
            Label(p, DW / 2, 318, DW - 40, 28, 8, TextAnchor.MiddleLeft, InfoText());

            // แถบล่าง: แต้ม · ตกลง · ล้างแต้ม
            int free = FreePoints(), pend = PendingCost();
            Label(p, 110, 346, 200, 16, 11, TextAnchor.MiddleLeft, "<b><color=#ffd76a>" + LM.Get("ngd_st_points") + " " + free.ToString("00") + "</color></b>"
                  + (pend > 0 ? "  <color=#9aff9a>(-" + pend + ")</color>" : ""));
            Btn(p, 228, 346, 34, 16, "+5", Color.white, () => SkillTree.AddPoints(_a, 5));
            var ap = Btn(p, DW - 176, 346, 100, 18, LM.Get("ngd_st_apply"), _pending.Count > 0 ? new Color(0.45f, 1f, 0.5f) : new Color(0.6f, 0.75f, 0.6f), Apply);
            Btn(p, DW - 66, 346, 100, 18, LM.Get("ngd_st_reset"), new Color(1f, 0.5f, 0.5f), ResetAll);
        }

        static void Column(Transform p, string brId, float x0, bool classBranch, bool th)
        {
            var br = SkillTree.Branches.First(b => b.Id == brId);
            var col = BranchColor.TryGetValue(brId, out var cc) ? cc : Color.white;
            float w = 183, cx = x0 + w / 2;
            Box(p, cx, 172, w, 254, new Color(0, 0, 0, 0.28f));
            Box(p, cx, 62, w - 6, 30, new Color(0, 0, 0, 0.35f));
            string hex = "#" + ColorUtility.ToHtmlStringRGB(col);
            Label(p, cx, 55, w - 8, 14, 9, TextAnchor.MiddleCenter, "<b><color=" + hex + ">" + (classBranch ? "★ " : "") + br.En.ToUpperInvariant() + "</color></b>");
            Label(p, cx, 69, w - 8, 12, 8, TextAnchor.MiddleCenter, "<color=" + hex + ">(" + br.Th + ")</color>");

            float lx = cx - 46, rx = cx + 46;
            float[] ry = { 104, 146, 188, 230 };
            float my = 272, half = 17;
            Color dark = new Color(0.32f, 0.32f, 0.36f, 1f);
            Color Lit(string id) => KnownOrPending(id) ? col : dark;

            // เส้นราก
            Line(p, cx, 78, cx, 86, col);
            Line(p, lx, 86, rx, 86, col);
            Line(p, lx, 86, lx, ry[0] - half, col);
            Line(p, rx, 86, rx, ry[0] - half, col);
            for (int t = 1; t <= 4; t++)
            {
                string a = brId + "_a" + t, ps = brId + "_p" + t;
                bool any = KnownOrPending(a) || KnownOrPending(ps);
                Line(p, lx + half, ry[t - 1], rx - half, ry[t - 1], any ? col : dark);   // ขั้นเดียวกันเชื่อมกัน
                if (t < 4)
                {
                    Line(p, lx, ry[t - 1] + half, lx, ry[t] - half, any ? col : dark);
                    Line(p, rx, ry[t - 1] + half, rx, ry[t] - half, any ? col : dark);
                }
            }
            // ไปโหนดสุดยอด (ต้องครบทั้งสองฝั่งขั้น 4)
            float jy = my - 22;
            Line(p, lx, ry[3] + half, lx, jy, Lit(brId + "_a4"));
            Line(p, rx, ry[3] + half, rx, jy, Lit(brId + "_p4"));
            Line(p, lx, jy, rx, jy, KnownOrPending(brId + "_a4") && KnownOrPending(brId + "_p4") ? col : dark);
            Line(p, cx, jy, cx, my - half - 2, KnownOrPending(brId + "_a4") && KnownOrPending(brId + "_p4") ? col : dark);

            // เลขขั้น
            for (int t = 1; t <= 4; t++) Label(p, cx, ry[t - 1] + 13, 30, 10, 7, TextAnchor.MiddleCenter, "<color=#888888>" + t + "</color>");

            for (int t = 1; t <= 4; t++)
            {
                Node(p, brId + "_a" + t, lx, ry[t - 1], 32, col);
                Node(p, brId + "_p" + t, rx, ry[t - 1], 32, col);
            }
            Node(p, brId + "_m", cx, my, 38, col);
        }

        static void Node(Transform p, string id, float x, float y, float size, Color col)
        {
            var n = SkillTree.Nodes[id];
            bool known = SkillTree.Knows(_a, id), pend = _pending.Contains(id);
            bool can = CanPick(n, out string why);
            bool locked = !known && !pend && (why == "prev" || why == "rank");

            if (known) { Box(p, x, y, size + 6, size + 6, new Color(col.r, col.g, col.b, 0.22f)); Box(p, x, y, size + 3, size + 3, new Color(col.r, col.g, col.b, 0.35f)); }   // เรืองแสงจาง ๆ
            Color frame = known ? col : pend ? new Color(0.45f, 1f, 0.5f) : can ? Color.Lerp(col, Color.white, 0.5f) : new Color(0.3f, 0.3f, 0.33f);
            var fr = Box(p, x, y, size, size, frame);
            Box(p, x, y, size - 4, size - 4, new Color(0.12f, 0.11f, 0.16f, 1f));
            var icon = Box(p, x, y, size - 6, size - 6, Color.white, EditorTabs.Icon("sk_" + id));
            if (locked || (!known && !pend && !can)) icon.color = new Color(0.45f, 0.45f, 0.5f, 1f);
            if (locked) Box(p, x + size / 2 - 4, y + size / 2 - 5, 11, 11, Color.white, EditorTabs.Icon("lock"));
            if (pend) Label(p, x + size / 2 - 3, y - size / 2 + 4, 10, 10, 9, TextAnchor.MiddleCenter, "<b><color=#9aff9a>+</color></b>");
            if (_sel == id) { var s = Box(p, x, y - size / 2 - 3, size, 2, Color.white); }

            // ปุ่มกดบนโหนด
            fr.raycastTarget = true;
            var btn = fr.gameObject.AddComponent<Button>();
            btn.targetGraphic = fr;
            string nid = id;
            btn.onClick.AddListener(() => { try { Click(nid); } catch (Exception e) { NgdMod.LogOnce("node click: " + e.Message); } Rebuild(); });
            fr.gameObject.name = "node_" + id;
        }

        static string InfoText()
        {
            if (_sel == null || !SkillTree.Nodes.TryGetValue(_sel, out var n))
                return "<color=#aaaaaa>" + LM.Get("ngd_st_hint") + "</color>";
            var br = SkillTree.Branches.First(b => b.Id == n.Branch);
            string hex = "#" + ColorUtility.ToHtmlStringRGB(BranchColor.TryGetValue(n.Branch, out var c) ? c : Color.white);
            bool known = SkillTree.Knows(_a, n.Id);
            CanPick(n, out string why);
            string state = known ? "<color=#9aff9a>" + LM.Get("ngd_node_known") + "</color>"
                         : _pending.Contains(n.Id) ? "<color=#9aff9a>" + LM.Get("ngd_st_picked") + "</color>"
                         : why == "rank" ? "<color=#ff8a8a>" + LM.Get("ngd_node_need_rank") + " " + NgdData.RankNames[Math.Min(NgdData.RankNames.Length - 1, n.Tier - 1)] + "</color>"
                         : why == "prev" ? "<color=#ff8a8a>" + LM.Get("ngd_node_need_prev") + "</color>"
                         : why == "points" ? "<color=#ff8a8a>" + LM.Get("ngd_st_no_points") + "</color>"
                         : "<color=#ffffff>" + string.Format(LM.Get("ngd_node_cost"), SkillTree.Cost(n)) + "</color>";
            string kind = LM.Get(n.Tier == 5 ? "ngd_st_mastery" : n.Passive ? "ngd_node_passive" : "ngd_node_active");
            string desc = EditorTabs.DescribeSkill(_a, n).Replace("\n", "   ").Trim();
            return "<b><color=" + hex + ">" + SkillTree.Name(n) + "</color></b>  <color=#aaaaaa>" + kind + " · " + LM.Get("ngd_tier") + " " + n.Tier + "</color>   " + state + "\n" + desc;
        }
    }

    /// <summary>กด Esc หรือคลิกพื้นมืดนอกหน้าต่างเพื่อปิด</summary>
    public class SkillTreeCloser : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        void Update() { if (Input.GetKeyDown(KeyCode.Escape)) SkillTreeWindow.Close(); }
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData e) { if (e.pointerCurrentRaycast.gameObject == gameObject) SkillTreeWindow.Close(); }
    }
}
