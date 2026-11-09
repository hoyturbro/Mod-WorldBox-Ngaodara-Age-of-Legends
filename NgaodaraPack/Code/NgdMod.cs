using System;
using System.Collections.Generic;
using HarmonyLib;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;

namespace NgaodaraPack
{
    public class NgdMod : BasicMod<NgdMod>
    {
        static readonly HashSet<string> _logged = new HashSet<string>();

        protected override void OnModLoad()
        {
            try
            {
                new Harmony("hoyturbro.ngaodarapack").PatchAll(typeof(NgdMod).Assembly);
                Arsenal.Register();
                Gear.Register();
                SkillTree.Build();
                ModFolder = GetDeclaration().FolderPath;
                NewBuildings.Register();
                WorldSize.Register();
                WorldGen.Register();
                RegisterPowers();
                CreateTab();
                ArtProbe.Run(GetDeclaration().FolderPath);
                var go = new GameObject("NgaodaraTicker");
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<NgdTicker>();
                go.AddComponent<RankBadges>();
                go.AddComponent<FxPlayer>();
                go.AddComponent<DamageNumbers>();
                if (System.IO.File.Exists(SelfTest.FlagPath)) { go.AddComponent<SelfTest>(); LogInfo("SELFTEST enabled"); }
                LogInfo("Ngaodara Pack loaded");
            }
            catch (Exception e)
            {
                LogError("Ngaodara Pack failed to load: " + e);
            }
        }

        public static string ModFolder = "";
        public static void Log(string msg) { LogInfo(msg); }

        public static void LogOnce(string msg)
        {
            bool fresh; lock (_logged) fresh = _logged.Add(msg);
            if (fresh) LogWarning(msg);
        }

        static string BasePowerId()
        {
            foreach (var id in new[] { "inspect", "inspect_unit", "finger" })
                if (AssetManager.powers.has(id)) { Log("base power: " + id); return id; }
            throw new Exception("no base power found to clone");
        }

        static GodPower MakePower(string id, string icon, string basePower, PowerActionWithID action)
        {
            var p = AssetManager.powers.clone(id, basePower);
            p.name = id;
            p.path_icon = icon;
            p.allow_unit_selection = false;
            p.click_action = action;
            return p;
        }

        static void RegisterPowers()
        {
            string basePower = BasePowerId();
            MakePower("ngd_gift_relic", "ngd/p_relic", basePower, DivinePowers.GiftRelic);
            MakePower("ngd_starfall", "ngd/p_starfall", basePower, DivinePowers.Starfall);
            MakePower("ngd_time_stop", "ngd/p_timestop", basePower, DivinePowers.TimeStop);
            MakePower("ngd_mass_ascension", "ngd/p_ascension", basePower, DivinePowers.MassAscension);
            MakePower("ngd_open_rift", "ngd/p_rift", basePower, DivinePowers.OpenRift);
            // ดวงตาเทพ: คลิกคนแล้วเปิดเมนูเทพ (โคลนจากพลัง inspect_unit ของเกม)
            var eye = AssetManager.powers.clone("ngd_divine_eye", basePower);
            eye.name = "ngd_divine_eye";
            eye.path_icon = "ngd/eye";
            // ปิดการเลือกยูนิตของเกม ไม่งั้นคลิกแล้วเกมจะเปิดหน้าต่างยูนิตเองแทน click_action ของเรา
            eye.allow_unit_selection = false;
            eye.click_action = (pTile, pPower) =>
            {
                var a = PickActor(pTile);
                if (a == null) return false;
                SelectedUnit.select(a, true);
                GodMenu.Open(a);
                return true;
            };

            // ประทานดาว: คลิกคนแล้ว +1 ดาว (ปลุกเป็นสายต่อสู้ถ้ายังเป็นคนธรรมดา)
            var bless = AssetManager.powers.clone("ngd_bless_star", basePower);
            bless.name = "ngd_bless_star";
            bless.path_icon = "ngd/star";
            bless.allow_unit_selection = false;
            bless.click_action = (pTile, pPower) =>
            {
                var a = PickActor(pTile);
                if (a == null) return false;
                Progression.Awaken(a);
                if (NgdData.GetStar(a) < NgdData.MaxStar) NgdData.SetStar(a, NgdData.GetStar(a) + 1);
                else if (NgdData.GetRank(a) < NgdData.RankNames.Length - 1) Progression.TryBreakthrough(a, true);
                a.setStatsDirty();
                a.restoreHealth(a.getMaxHealth());
                if (GodMenu.Target == a) GodMenu.Refresh();
                return true;
            };
        }

        public static Actor PickActor(WorldTile pTile)
        {
            Actor a = null;
            if (pTile != null) a = ActionLibrary.getActorFromTile(pTile);
            if (a == null) a = World.world.getActorNearCursor();
            return a != null && a.isAlive() ? a : null;
        }

        static void CreateTab()
        {
            var tab = TabManager.CreateTab("Ngaodara", "ngd_tab", "ngd_tab Description", Sprite("ngd/tab"));
            PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateGodPowerButton("ngd_divine_eye", Sprite("ngd/eye")), tab);
            PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateGodPowerButton("ngd_bless_star", Sprite("ngd/star")), tab);
            foreach (var (id, icon) in new[] { ("ngd_gift_relic", "p_relic"), ("ngd_mass_ascension", "p_ascension"),
                                                ("ngd_time_stop", "p_timestop"), ("ngd_starfall", "p_starfall"), ("ngd_open_rift", "p_rift") })
                PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateGodPowerButton(id, Sprite("ngd/" + icon)), tab);
            PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateSimpleButton("ngd_era_forward", () => ShiftEra(+1), Sprite("ngd/era_forward")), tab);
            PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateSimpleButton("ngd_era_back", () => ShiftEra(-1), Sprite("ngd/era_back")), tab);
            PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateSimpleButton("ngd_toggle_badges", () =>
            {
                RankBadges.Enabled = !RankBadges.Enabled;
                GodMenu.Tip(RankBadges.Enabled ? "ngd_badges_on" : "ngd_badges_off");
            }, Sprite("ngd/r_S")), tab);
            PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateSimpleButton("ngd_worldgen_btn", WorldGen.Open, Sprite("ngd/worldgen")), tab);
            PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateSimpleButton("ngd_skilltree_btn",
                () => SkillTreeWindow.Open(SelectedUnit.isSet() ? SelectedUnit.unit : EditorTabs.Target), Sprite("ngd/skilltree")), tab);
            PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateSimpleButton("ngd_chronicle_btn", Legends.OpenChronicle, Sprite("ngd/chronicle")), tab);
            PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateSimpleButton("ngd_open_menu", GodMenu.OpenForSelected, Sprite("ngd/menu")), tab);
            PowerButtonCreator.AddButtonToTab(PowerButtonCreator.CreateSimpleButton("ngd_toggle_auto",
                () =>
                {
                    NgdConfig.AutoBreakthrough = !NgdConfig.AutoBreakthrough;
                    GodMenu.Tip(NgdConfig.AutoBreakthrough ? "ngd_auto_on" : "ngd_auto_off");
                }, Sprite("ngd/break")), tab);
        }

        /// <summary>เร่ง/ย้อนยุคโลก: เลื่อนปีไปยังจุดเริ่มยุคถัดไป/ก่อนหน้า</summary>
        static void ShiftEra(int dir)
        {
            int e = Mathf.Clamp(Eras.WorldEra() + dir, 0, Eras.Ids.Length - 1);
            Eras.YearOffset += Eras.StartYear[e] - Eras.WorldYear();
        }

        static Sprite Sprite(string path) => SpriteTextureLoader.getSprite("ui/Icons/" + path);
    }
}
