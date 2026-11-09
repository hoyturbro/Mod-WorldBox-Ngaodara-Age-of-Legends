using System;
using System.Collections.Generic;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace NgaodaraPack
{
    /// <summary>
    /// ตำนานมีชีวิต: ยอดฝีมือได้ "ฉายา" (เช่น Starbreaker) เก็บผลงาน (deeds)
    /// และโลกมี "พงศาวดาร" บันทึกเหตุการณ์ใหญ่ เปิดอ่านได้จากปุ่มในแท็บเงาดารา
    /// </summary>
    public static class Legends
    {
        static readonly string[] Pre = { "Star", "Storm", "Moon", "Crimson", "Void", "Iron", "Dawn", "Frost", "Thunder", "Silent", "Ember", "Rift", "Sky", "Ash", "Golden" };
        static readonly string[] Suf = { "breaker", "blade", "walker", "fang", "heart", "sage", "bane", "wing", "fist", "eye", "caller", "reaver", "warden", "seeker", "crown" };
        static readonly string[] BossPre = { "Abyssal", "Plague", "Eclipse", "Chaos", "Hollow", "Dread" };
        static readonly string[] BossSuf = { "Tyrant", "Devourer", "Harbinger", "Lord", "Horror", "Wyrm" };

        const char Sep = '¦';   // ตัวคั่นรายการพงศาวดาร (ไม่ใช้ในข้อความปกติ)

        static List<string> LoadChronicle()
        {
            var raw = WorldData.GetString("chronicle");
            return string.IsNullOrEmpty(raw) ? new List<string>() : new List<string>(raw.Split(Sep));
        }

        public static string MakeTitle(bool boss)
        {
            if (boss) return BossPre[UnityEngine.Random.Range(0, BossPre.Length)] + " " + BossSuf[UnityEngine.Random.Range(0, BossSuf.Length)];
            return Pre[UnityEngine.Random.Range(0, Pre.Length)] + Suf[UnityEngine.Random.Range(0, Suf.Length)];
        }

        public static string Title(Actor a) { a.data.get("ngd_title", out string v, ""); return v; }
        public static int Deeds(Actor a) { a.data.get("ngd_deeds", out int v, 0); return v; }

        public static string FullName(Actor a)
        {
            var t = Title(a);
            return string.IsNullOrEmpty(t) ? a.getName() : a.getName() + " \"" + t + "\"";
        }

        public static void AddDeed(Actor a, int n)
        {
            a.data.set("ngd_deeds", Deeds(a) + n);
            if (Deeds(a) >= 10 && string.IsNullOrEmpty(Title(a))) GrantTitle(a);
        }

        public static void GrantTitle(Actor a)
        {
            if (!string.IsNullOrEmpty(Title(a))) return;
            a.data.set("ngd_title", MakeTitle(false));
            a.data.favorite = true;   // ติดดาวเป็นตัวโปรด ให้ผู้เล่นตามดูได้
            Record(string.Format(LM.Get("ngd_chron_title"), a.getName(), Title(a)), a);
        }

        const char Link = '¤';   // ต่อท้ายรายการ: ¤a<id> = ตัวละคร · ¤c<id> = เมือง (กดในพงศาวดารเพื่อไปหา)

        public static void Record(string text, Actor who = null, City city = null)
        {
            string stamp = "[" + Eras.WorldYear() + "] ";
            string link = who != null ? Link + "a" + who.getID() : city != null ? Link + "c" + city.getID() : "";
            var list = LoadChronicle();
            list.Insert(0, stamp + text.Replace(Sep, ' ').Replace(Link, ' ') + link);
            if (list.Count > 60) list.RemoveRange(60, list.Count - 60);
            WorldData.SetString("chronicle", string.Join(Sep.ToString(), list));
        }

        /// <summary>เมื่อมีคนทะลวงแรงค์ S ขึ้นไป</summary>
        public static void OnRankUp(Actor a, int rank)
        {
            if (rank < 5) return;
            AddDeed(a, rank >= 6 ? 6 : 3);
            if (rank >= 6) GrantTitle(a);
            string key = rank >= 6 ? "ngd_news_legend" : "ngd_news_rank_s";
            string text = string.Format(LM.Get(key), FullName(a), NgdData.RankNames[rank]);
            GuildSystem.Announce(text);
            Record(text, a);
        }

        // ---------- หน้าต่างพงศาวดาร (กดรายการเพื่อไปหาตัวละคร/เมือง) ----------
        const string WindowId = "ngd_chronicle";
        static ScrollWindow _win;
        static Transform _list;
        static Text _fame;

        public static void EnsureWindow()
        {
            if (_win != null) return;
            _win = WindowCreator.CreateEmptyWindow(WindowId, WindowId, "ngd/menu");
            var content = _win.transform.Find("Background/Scroll View/Viewport/Content");
            var fit = content.gameObject.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = content.gameObject.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.spacing = 3;
            vlg.childControlHeight = true; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = false;
            vlg.childAlignment = TextAnchor.UpperCenter;
            _list = content;
            _fame = MakeText(content, 8);
        }

        static Text MakeText(Transform parent, int size)
        {
            var go = new GameObject("text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = LocalizedTextManager.current_font;
            t.fontSize = size;
            t.supportRichText = true;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            go.AddComponent<LayoutElement>().preferredWidth = 178;
            return t;
        }

        public static void OpenChronicle()
        {
            EnsureWindow();
            for (int i = _list.childCount - 1; i >= 0; i--)
            {
                var ch = _list.GetChild(i);
                if (ch != _fame.transform) UnityEngine.Object.Destroy(ch.gameObject);
            }
            _fame.text = "";
            _fame.gameObject.SetActive(false);

            // ---- ทำเนียบยอดฝีมือ: การ์ดพร้อมรูปตัวละครจริง ----
            Header(LM.Get("ngd_hall_title"));
            var top = new List<Actor>();
            foreach (var a in World.world.units)
                if (a != null && a.isAlive() && NgdData.GetRank(a) >= 0 && !Rifts.IsBoss(a)) top.Add(a);
            top.Sort((x, y) => NgdData.CombatPower(y).CompareTo(NgdData.CombatPower(x)));
            if (top.Count == 0) MakeText(_list, 8).text = "<color=#aaaaaa>" + LM.Get("ngd_chron_empty") + "</color>";
            for (int i = 0; i < top.Count && i < 10; i++) HeroCard(i + 1, top[i]);

            // ---- บันทึกเหตุการณ์ ----
            Header(LM.Get("ngd_chron_events"));
            var chron = LoadChronicle();
            if (chron.Count == 0) MakeText(_list, 8).text = "<color=#aaaaaa>" + LM.Get("ngd_chron_empty") + "</color>";
            foreach (var raw in chron) AddEntry(raw);
            ScrollWindow.showWindow(WindowId);
        }

        static void Header(string text)
        {
            var t = MakeText(_list, 10);
            t.alignment = TextAnchor.MiddleCenter;
            t.text = "<b><color=#ffd76a>✦ " + text + " ✦</color></b>";
        }

        /// <summary>แถว/การ์ด: พื้นหลังจาง กดได้ (ปุ่มอยู่บนพื้นหลัง ข้อความ/รูปเป็นลูก)</summary>
        static Transform Card(float h, Color bg, Action onClick)
        {
            var go = new GameObject("card", typeof(RectTransform));
            go.transform.SetParent(_list, false);
            var img = go.AddComponent<Image>();
            img.color = bg;
            var le = go.AddComponent<LayoutElement>(); le.preferredWidth = 178; le.minHeight = h; le.preferredHeight = h;
            var hl = go.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(3, 3, 2, 2); hl.spacing = 4; hl.childAlignment = TextAnchor.MiddleLeft;
            hl.childControlWidth = false; hl.childControlHeight = false; hl.childForceExpandWidth = false; hl.childForceExpandHeight = false;
            if (onClick != null)
            {
                var b = go.AddComponent<Button>();
                b.targetGraphic = img;
                var cb = b.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1.6f, 1.4f, 0.8f); cb.pressedColor = new Color(0.8f, 0.8f, 0.8f); b.colors = cb;
                b.onClick.AddListener(() => onClick());
            }
            return go.transform;
        }

        static Image Pic(Transform parent, Sprite s, float size, Color frame)
        {
            var f = new GameObject("frame", typeof(RectTransform));
            f.transform.SetParent(parent, false);
            ((RectTransform)f.transform).sizeDelta = new Vector2(size + 4, size + 4);
            var fi = f.AddComponent<Image>(); fi.color = frame;
            var go = new GameObject("pic", typeof(RectTransform));
            go.transform.SetParent(f.transform, false);
            var rt = (RectTransform)go.transform; rt.sizeDelta = new Vector2(size, size);
            var img = go.AddComponent<Image>(); img.sprite = s; img.preserveAspect = true; img.raycastTarget = false;
            if (s == null) img.color = new Color(0, 0, 0, 0);
            return img;
        }

        static Sprite Portrait(Actor a)
        {
            Sprite s = null;
            try { s = a.getLastColoredSprite(); } catch { }
            if (s == null) try { s = a.calculateMainSprite(); } catch { }
            if (s != null && s.texture != null) s.texture.filterMode = FilterMode.Point;
            return s;
        }

        static Text CardText(Transform parent, float w, int size)
        {
            var t = MakeText(parent, size);
            t.GetComponent<LayoutElement>().preferredWidth = w;
            ((RectTransform)t.transform).sizeDelta = new Vector2(w, 26);
            t.raycastTarget = false;
            return t;
        }

        static readonly Color[] Medal = { new Color(1f, 0.82f, 0.3f), new Color(0.82f, 0.86f, 0.95f), new Color(0.85f, 0.55f, 0.3f) };

        static void HeroCard(int place, Actor a)
        {
            long id = a.getID();
            var card = Card(34, place <= 3 ? new Color(Medal[place - 1].r, Medal[place - 1].g, Medal[place - 1].b, 0.22f) : new Color(0, 0, 0, 0.22f), () => GoTo(true, id));
            var num = CardText(card, 14, 10);
            num.alignment = TextAnchor.MiddleCenter;
            num.text = "<b>" + place + "</b>";
            if (place <= 3) num.color = Medal[place - 1];
            Pic(card, Portrait(a), 26, new Color(0.08f, 0.08f, 0.1f, 0.85f));
            int r = NgdData.GetRank(a);
            var badge = new GameObject("badge", typeof(RectTransform));
            badge.transform.SetParent(card, false);
            ((RectTransform)badge.transform).sizeDelta = new Vector2(16, 16);
            var bi = badge.AddComponent<Image>(); bi.sprite = EditorTabs.Icon("r_" + NgdData.RankNames[r]); bi.preserveAspect = true; bi.raycastTarget = false;
            var c = Classes.Get(NgdData.GetClass(a));
            var city = a.getCity();
            var t = CardText(card, 108, 8);
            t.text = "<b>" + Legends.FullName(a) + "</b>\n"
                + "<color=" + NgdData.RankColors[r] + ">" + NgdData.RankNames[r] + " " + NgdData.GetStar(a) + "★</color> "
                + (c != null ? Classes.TierName(c, r) : "") + "\n"
                + "<color=#ffd76a>CP " + NgdData.CombatPower(a) + "</color>" + (city != null ? "  <color=#aaaaaa>" + city.name + "</color>" : "");
        }

        static void AddEntry(string raw)
        {
            string text = raw, link = null;
            int k = raw.IndexOf(Link);
            if (k >= 0) { text = raw.Substring(0, k); link = raw.Substring(k + 1); }
            // ปีเป็นสีทอง
            if (text.StartsWith("["))
            {
                int e = text.IndexOf(']');
                if (e > 0) text = "<color=#ffd76a>" + text.Substring(0, e + 1) + "</color>" + text.Substring(e + 1);
            }
            long id = -1; bool isActor = false; Actor who = null; City city = null;
            if (!string.IsNullOrEmpty(link) && link.Length >= 2 && long.TryParse(link.Substring(1), out id))
            {
                isActor = link[0] == 'a';
                if (isActor) { try { who = World.world.units.get(id); } catch { } }
                else { try { city = World.world.cities.get(id); } catch { } }
            }
            bool linked = id >= 0;
            bool alive = isActor ? who != null && who.isAlive() : city != null && city.isAlive();
            long cid = id; bool ia = isActor;
            var row = Card(28, new Color(0, 0, 0, linked ? 0.18f : 0.08f), linked ? (Action)(() => GoTo(ia, cid)) : null);
            Sprite pic = isActor && alive ? Portrait(who) : EditorTabs.Icon(linked && !isActor ? "tab_city_gov" : "chronicle");
            Pic(row, pic, 20, linked && alive ? new Color(0.08f, 0.08f, 0.1f, 0.85f) : new Color(0, 0, 0, 0));
            var t = CardText(row, 146, 8);
            t.text = text + (linked ? (alive ? "  <color=#7fd0ff>[" + LM.Get(isActor ? "ngd_chron_goto" : "ngd_chron_goto_city") + "]</color>"
                                             : "  <color=#888888>[" + LM.Get("ngd_chron_gone_short") + "]</color>") : "");
        }

        /// <summary>ไปหาตัวละคร (กล้องตาม + เปิดหน้าต่าง Info) หรือเมือง (เปิดหน้าต่างเมือง)</summary>
        public static void GoTo(bool isActor, long id)
        {
            try
            {
                if (isActor)
                {
                    Actor a = null;
                    try { a = World.world.units.get(id); } catch { }
                    if (a == null) foreach (var u in World.world.units) if (u != null && u.getID() == id) { a = u; break; }
                    if (a == null || !a.isAlive()) { NgdMod.Log("chronicle: actor " + id + " not found"); GodMenu.Tip("ngd_chron_gone"); return; }
                    NgdMod.Log("chronicle: go to " + a.getName());
                    ScrollWindow.hideAllEvent(false);
                    World.world.locatePosition(a.current_position);
                    var target = a;
                    NgdTicker.Later(0.4f, () => { if (target.isAlive()) ActionLibrary.openUnitWindow(target); });
                }
                else
                {
                    var c = World.world.cities.get(id);
                    if (c == null || !c.isAlive()) { GodMenu.Tip("ngd_chron_gone"); return; }
                    var tile = c.getTile(false);
                    ScrollWindow.hideAllEvent(false);
                    if (tile != null) World.world.locatePosition(tile.posV3);
                    var city = c;
                    NgdTicker.Later(0.4f, () => { if (city.isAlive()) { SelectedMetas.selected_city = city; ScrollWindow.showWindow("city"); } });
                }
            }
            catch (Exception e) { NgdMod.LogOnce("chronicle goto: " + e.Message); }
        }
    }
}
