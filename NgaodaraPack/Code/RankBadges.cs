using System;
using System.Collections.Generic;
using UnityEngine;

namespace NgaodaraPack
{
    /// <summary>
    /// ป้ายแรงค์เหนือหัวในโลก (สไตล์อนิเมะ): แสดงเมื่อซูมเข้าใกล้ เฉพาะตัวที่มีแรงค์และอยู่ในจอ
    /// ใช้ SpriteRenderer จาก pool (ไม่เกิน 160 ป้าย) อัปเดตรายชื่อทุก 0.25 วินาที
    /// บอสมีป้ายสีแดงเรือง
    /// </summary>
    public class RankBadges : MonoBehaviour
    {
        public static bool Enabled = true;
        const int MaxBadges = 160;
        const float ZoomLimit = 70f;      // ซูมออกเกินนี้ไม่แสดงป้าย (ไม่บังแผนที่)
        const float LowRankZoom = 28f;    // แรงค์ E–C แสดงเฉพาะตอนซูมใกล้

        readonly List<SpriteRenderer> _pool = new List<SpriteRenderer>();
        readonly List<Actor> _shown = new List<Actor>();
        readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
        float _scan;
        int _layer = 0;
        bool _layerSet;

        Sprite BadgeSprite(int rank)
        {
            string key = NgdData.RankNames[rank];
            if (!_sprites.TryGetValue(key, out var s))
            {
                _sprites[key] = s = SpriteTextureLoader.getSprite("ui/Icons/ngd/r_" + key);
                if (s != null && s.texture != null) s.texture.filterMode = FilterMode.Point;
            }
            return s;
        }

        SpriteRenderer Get(int i)
        {
            while (_pool.Count <= i)
            {
                var go = new GameObject("ngd_badge");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 32000;
                if (_layerSet) sr.sortingLayerID = _layer;
                go.SetActive(false);
                _pool.Add(sr);
            }
            return _pool[i];
        }

        void LateUpdate()
        {
            try
            {
                Camera cam = World.world != null ? World.world.camera : null;
                if (!Enabled || cam == null || cam.orthographicSize > ZoomLimit) { HideFrom(0); return; }
                if (!_layerSet && SortingLayer.layers.Length > 0)
                {
                    _layer = SortingLayer.layers[SortingLayer.layers.Length - 1].id;   // ชั้นบนสุด
                    _layerSet = true;
                    foreach (var p in _pool) p.sortingLayerID = _layer;
                }

                _scan -= Time.unscaledDeltaTime;
                if (_scan <= 0f) { _scan = 0.25f; Scan(cam); }

                int n = 0;
                foreach (var a in _shown)
                {
                    if (a == null || !a.isAlive()) continue;
                    int r = NgdData.GetRank(a);
                    if (r < 0) continue;
                    if (r < 3 && !Rifts.IsBoss(a) && cam.orthographicSize > LowRankZoom) continue;
                    var sr = Get(n++);
                    var sp = BadgeSprite(r);
                    if (sp == null) { n--; continue; }
                    sr.sprite = sp;
                    float size = Mathf.Clamp(cam.orthographicSize * 0.036f, 0.42f, 3.2f);   // ~20px บนจอทุกระดับซูม (ตัวคน ~0.7 หน่วย)   // หน่วยโลก = พิกเซล (ppu 1)
                    float sc = size / Mathf.Max(0.01f, sp.bounds.size.x);
                    sr.transform.localScale = new Vector3(sc, sc, 1f);
                    float h = Mathf.Max(0.6f, a.current_scale.y * 10f) * 0.8f + size * 0.6f;
                    sr.transform.position = new Vector3(a.current_position.x, a.current_position.y + h, -5f);
                    sr.color = Rifts.IsBoss(a) ? new Color(1f, 0.55f, 0.55f, 0.95f) : new Color(1f, 1f, 1f, 0.85f);
                    if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true);
                    if (n >= MaxBadges) break;
                }
                HideFrom(n);
            }
            catch (Exception e) { NgdMod.LogOnce("badges: " + e.Message); Enabled = false; }
        }

        void Scan(Camera cam)
        {
            _shown.Clear();
            float hh = cam.orthographicSize, hw = hh * cam.aspect;
            Vector2 c = cam.transform.position;
            foreach (var a in World.world.units)
            {
                if (a == null || !a.isAlive()) continue;
                var p = a.current_position;
                if (Mathf.Abs(p.x - c.x) > hw || Mathf.Abs(p.y - c.y) > hh) continue;
                if (NgdData.GetRank(a) < 0) continue;
                _shown.Add(a);
                if (_shown.Count >= MaxBadges) break;
            }
        }

        void HideFrom(int n)
        {
            for (int i = n; i < _pool.Count; i++)
                if (_pool[i].gameObject.activeSelf) _pool[i].gameObject.SetActive(false);
        }
    }
}
