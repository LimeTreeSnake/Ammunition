using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Ammunition {
    internal class AmmunitionSettings : ModSettings {

        #region Fields
        private static readonly bool needAmmo = true;
        private static readonly bool compatibilityMode = false;
        private static readonly bool npcNeedAmmo = true;
        private static readonly bool fetchAmmo = true;
        private static readonly int desiredAmmo = 25;
        private static readonly int leastAmmoFetch = 3;
        private static readonly int npcMinAmmo = 15;
        private static readonly int npcMaxAmmo = 45;

        private static readonly float weaponViewHeight = 300;
        private static readonly List<string> weaponExclusion = new List<string>();

        #endregion Fields
        #region Properties
        public bool NeedAmmo = needAmmo;
        public bool CompatibilityMode = compatibilityMode;
        public bool NPCNeedAmmo = npcNeedAmmo;
        public bool FetchAmmo = fetchAmmo;
        public int DesiredAmmo = desiredAmmo;
        public int LeastAmmoFetch = leastAmmoFetch;
        public int NPCMinAmmo = npcMinAmmo;
        public int NPCMaxAmmo = npcMaxAmmo;
        public string Filter { get; set; }

        public float WeaponViewHeight = weaponViewHeight;
        public List<string> WeaponExclusion = weaponExclusion.Count > 0 ? weaponExclusion.ListFullCopy() : new List<string>();
        public IEnumerable<ThingDef> AvailableWeapons = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsRangedWeapon && x.equipmentType == EquipmentType.Primary && x.weaponTags != null && x.Verbs.FirstOrDefault(y => y.verbClass.Name.Contains("Verb_Shoot")) != null && x.Verbs.FirstOrDefault(y => !y.verbClass.Name.Contains("Verb_ShootOneUse")) != null && !x.destroyOnDrop);
        #endregion Properties
        public override void ExposeData() {
            Scribe_Values.Look(ref NeedAmmo, "NeedAmmo", needAmmo);
            Scribe_Values.Look(ref NPCNeedAmmo, "NPCNeedAmmo", npcNeedAmmo);
            Scribe_Values.Look(ref FetchAmmo, "FetchAmmo", fetchAmmo);
            Scribe_Values.Look(ref CompatibilityMode, "CompatibilityMode", compatibilityMode);
            Scribe_Values.Look(ref DesiredAmmo, "DesiredAmmo", desiredAmmo);
            Scribe_Values.Look(ref LeastAmmoFetch, "LeastAmmoFetch", leastAmmoFetch);
            Scribe_Values.Look(ref NPCMinAmmo, "NPCMinAmmo", npcMinAmmo);
            Scribe_Values.Look(ref NPCMaxAmmo, "NPCMaxAmmo", npcMaxAmmo);
            Scribe_Values.Look(ref WeaponViewHeight, "WeaponViewHeight", weaponViewHeight);
            Scribe_Collections.Look(ref WeaponExclusion, "WeaponExclusion", LookMode.Value);
        }

        public void Reset() {
            Filter = "";
            NeedAmmo = needAmmo;
            CompatibilityMode = compatibilityMode;
            NPCNeedAmmo = npcNeedAmmo;
            FetchAmmo = fetchAmmo;
            DesiredAmmo = desiredAmmo;
            LeastAmmoFetch = leastAmmoFetch;
            NPCMinAmmo = npcMinAmmo;
            NPCMaxAmmo = npcMaxAmmo;
            WeaponViewHeight = weaponViewHeight;
            WeaponExclusion = weaponExclusion.Count > 0 ? weaponExclusion.ListFullCopy() : new List<string>();
        }

    }
    internal static class SettingsHelper {
        public static AmmunitionSettings LatestVersion;
        public static void Reset() {
            LatestVersion.Reset();
        }
    }


    public class ModMain : Mod {
        public static Vector2 scrollPosition = Vector2.zero;
        public static Vector2 scrollPosition2 = Vector2.zero;
        private AmmunitionSettings ammunitionSettings = new AmmunitionSettings();

        public ModMain(ModContentPack content) : base(content) {
            ammunitionSettings = GetSettings<AmmunitionSettings>();
            SettingsHelper.LatestVersion = ammunitionSettings != null ? ammunitionSettings : new AmmunitionSettings();
        }
        public override string SettingsCategory() {
            return "Ammunition";
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            inRect.yMin += 20;
            inRect.yMax -= 20;
            Listing_Standard list = new Listing_Standard();
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, inRect.height * 2 + ammunitionSettings.WeaponViewHeight);
            Rect rect3 = new Rect(0f, 0f, inRect.width - 30f, ammunitionSettings.WeaponViewHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            list.Begin(rect2);
            if (list.ButtonText("Default Settings")) {
                ammunitionSettings.Reset();
            };
            list.CheckboxLabeled("Is ammo required to fire weapons?", ref ammunitionSettings.NeedAmmo);
            list.CheckboxLabeled("Mod Compatibility Mode", ref ammunitionSettings.CompatibilityMode, "Any weapon not covered by this mod should default to use Industrial Ammo.");
            list.CheckboxLabeled("Should a pawn actively search for ammo?", ref ammunitionSettings.FetchAmmo);
            if (ammunitionSettings.FetchAmmo) {
                list.Label(string.Format("Desired carried ammo. {0}", ammunitionSettings.DesiredAmmo));
                ammunitionSettings.DesiredAmmo = (int)Mathf.Round(list.Slider(ammunitionSettings.DesiredAmmo, 1, 100));
                list.Label(string.Format("Least ammo to fetch. {0}", ammunitionSettings.LeastAmmoFetch));
                ammunitionSettings.LeastAmmoFetch = (int)Mathf.Round(list.Slider(ammunitionSettings.LeastAmmoFetch, 1, 10));
            }
            list.CheckboxLabeled("NPC needs ammo.", ref ammunitionSettings.NPCNeedAmmo);
            if (ammunitionSettings.NPCNeedAmmo) {
                list.Label(string.Format("NPC's spawn if appliable with a minumum of {0} ammo.", ammunitionSettings.NPCMinAmmo));
                ammunitionSettings.NPCMinAmmo = (int)Mathf.Round(list.Slider(ammunitionSettings.NPCMinAmmo, 1, 100));
                list.Label(string.Format("NPC's spawn if appliable with a maxmimum of {0} ammo.", ammunitionSettings.NPCMaxAmmo));
                ammunitionSettings.NPCMaxAmmo = (int)Mathf.Round(list.Slider(ammunitionSettings.NPCMaxAmmo, ammunitionSettings.NPCMinAmmo, 200));
            }
            list.GapLine();
            if (ammunitionSettings.AvailableWeapons != null && ammunitionSettings.AvailableWeapons.Count() > 0) {
                list.Label(string.Format("Exclude weapons from ammunition check."));
                list.Label(string.Format("({0}) Height.", ammunitionSettings.WeaponViewHeight));
                ammunitionSettings.WeaponViewHeight = (int)Mathf.Round(list.Slider(ammunitionSettings.WeaponViewHeight, 1, 2500));
                ammunitionSettings.Filter = list.TextEntryLabeled("Filter:", ammunitionSettings.Filter, 1);
                Listing_Standard list2 = list.BeginSection(ammunitionSettings.WeaponViewHeight);
                list2.ColumnWidth = (rect2.width - 50) / 3;
                foreach (ThingDef def in ammunitionSettings.AvailableWeapons) {
                    if (def.defName.ToUpper().Contains(ammunitionSettings.Filter.ToUpper())) {
                        bool contains = ammunitionSettings.WeaponExclusion.Contains(def.defName);
                        list2.CheckboxLabeled(def.defName, ref contains);
                        if (contains == false && ammunitionSettings.WeaponExclusion.Contains(def.defName)) {
                            ammunitionSettings.WeaponExclusion.Remove(def.defName);
                        }
                        else if (contains == true && !ammunitionSettings.WeaponExclusion.Contains(def.defName)) {
                            ammunitionSettings.WeaponExclusion.Add(def.defName);
                        }
                    }
                }
                list.EndSection(list2);
            }

            list.End();
            Widgets.EndScrollView();
            ammunitionSettings.Write();
        }
    }
}
