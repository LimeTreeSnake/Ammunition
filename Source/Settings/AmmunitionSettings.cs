using Verse;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Ammunition
{
    public enum ammoType { none = 0, primitive = 1, preindustrial, industrial, chemical, acid, nitrogen, battery };
    internal class AmmunitionSettings : ModSettings
    {

        #region Fields
        private static readonly Dictionary<string, ammoType> associationDictionary = new Dictionary<string, ammoType>();
        private static readonly bool needAmmo = true;
        private static readonly bool npcNeedAmmo = true;
        private static readonly bool fetchAmmo = true;
        private static readonly int desiredAmmo = 25;
        private static readonly int leastAmmoFetch = 3;
        private static readonly int npcMinAmmo = 15;
        private static readonly int npcMaxAmmo = 45;
        private static readonly float weaponViewHeight = 300;

        #endregion Fields
        #region Properties
        public bool NeedAmmo = needAmmo;
        public bool NPCNeedAmmo = npcNeedAmmo;
        public bool FetchAmmo = fetchAmmo;
        public int DesiredAmmo = desiredAmmo;
        public int LeastAmmoFetch = leastAmmoFetch;
        public int NPCMinAmmo = npcMinAmmo;
        public int NPCMaxAmmo = npcMaxAmmo;
        public string Filter { get; set; }

        public float WeaponViewHeight = weaponViewHeight;
        public Dictionary<string, ammoType> AssociationDictionary = associationDictionary ?? new Dictionary<string, ammoType>();
        #endregion Properties
        public override void ExposeData() {
            Scribe_Values.Look(ref NeedAmmo, "NeedAmmo", needAmmo);
            Scribe_Values.Look(ref NPCNeedAmmo, "NPCNeedAmmo", npcNeedAmmo);
            Scribe_Values.Look(ref DesiredAmmo, "DesiredAmmo", desiredAmmo);
            Scribe_Values.Look(ref LeastAmmoFetch, "LeastAmmoFetch", leastAmmoFetch);
            Scribe_Values.Look(ref NPCMinAmmo, "NPCMinAmmo", npcMinAmmo);
            Scribe_Values.Look(ref NPCMaxAmmo, "NPCMaxAmmo", npcMaxAmmo);
            Scribe_Values.Look(ref WeaponViewHeight, "WeaponViewHeight", weaponViewHeight);
            Scribe_Collections.Look(ref AssociationDictionary, "AssociationDictionary", LookMode.Value);
        }

        public void Reset() {
            Filter = "";
            NeedAmmo = needAmmo;
            NPCNeedAmmo = npcNeedAmmo;
            FetchAmmo = fetchAmmo;
            DesiredAmmo = desiredAmmo;
            LeastAmmoFetch = leastAmmoFetch;
            NPCMinAmmo = npcMinAmmo;
            NPCMaxAmmo = npcMaxAmmo;
            WeaponViewHeight = weaponViewHeight;
            AssociationDictionary = new Dictionary<string, ammoType>();
            Utility.CheckWeaponAssociation();
        }

    }
    internal static class SettingsHelper
    {
        public static AmmunitionSettings LatestVersion;
        public static void Reset() {
            LatestVersion.Reset();
        }
    }


    public class Ammunition : Mod
    {
        public static Vector2 scrollPosition = Vector2.zero;
        public static Vector2 scrollPosition2 = Vector2.zero;
        private AmmunitionSettings ammunitionSettings = new AmmunitionSettings();

        public TextAnchor Anchor { get; private set; }

        public Ammunition(ModContentPack content) : base(content) {
            ammunitionSettings = GetSettings<AmmunitionSettings>();
            SettingsHelper.LatestVersion = ammunitionSettings != null ? ammunitionSettings : new AmmunitionSettings();
        }
        public override string SettingsCategory() {
            return "Ammunition";
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            try {

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
            list.Label(string.Format("Desired carried ammo. {0}", ammunitionSettings.DesiredAmmo));
            ammunitionSettings.DesiredAmmo = (int)Mathf.Round(list.Slider(ammunitionSettings.DesiredAmmo, 1, 100));
            list.Label(string.Format("Least ammo to fetch. {0}", ammunitionSettings.LeastAmmoFetch));
            ammunitionSettings.LeastAmmoFetch = (int)Mathf.Round(list.Slider(ammunitionSettings.LeastAmmoFetch, 1, 10));
            list.CheckboxLabeled("NPC needs ammo.", ref ammunitionSettings.NPCNeedAmmo);
            if (ammunitionSettings.NPCNeedAmmo) {
                list.Label(string.Format("NPC's spawn if appliable with a minumum of {0} ammo up to the maximum set in the following option.", ammunitionSettings.NPCMinAmmo));
                ammunitionSettings.NPCMinAmmo = (int)Mathf.Round(list.Slider(ammunitionSettings.NPCMinAmmo, 1, 100));
                list.Label(string.Format("NPC's spawn/respawn if appliable with maxmimum {0} of extra ammo.", ammunitionSettings.NPCMaxAmmo));
                ammunitionSettings.NPCMaxAmmo = (int)Mathf.Round(list.Slider(ammunitionSettings.NPCMaxAmmo, ammunitionSettings.NPCMinAmmo, 200));
            }
            list.GapLine();
            if (ammunitionSettings.AssociationDictionary != null) {
                list.Label(string.Format("Ammunition used for weapons."));
                list.Label(string.Format("({0}) Height.", ammunitionSettings.WeaponViewHeight));
                ammunitionSettings.WeaponViewHeight = (int)Mathf.Round(list.Slider(ammunitionSettings.WeaponViewHeight, 1, 2500));
                ammunitionSettings.Filter = list.TextEntryLabeled("Filter:", ammunitionSettings.Filter, 1);
                Listing_Standard list2 = list.BeginSection(ammunitionSettings.WeaponViewHeight);
                list2.ColumnWidth = (rect2.width - 50) / 3;
                foreach (ThingDef weapon in Utility.AvailableWeapons) {
                    if (SettingsHelper.LatestVersion.AssociationDictionary.ContainsKey(weapon.defName) &&
                        weapon.defName.ToUpper().Contains(ammunitionSettings.Filter.ToUpper())) {
                        float lineHeight = Text.LineHeight;
                        Rect innerRect = list2.GetRect(lineHeight);
                        TextAnchor anchor = Text.Anchor;
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(innerRect, weapon.label);
                        if (Widgets.ButtonInvisible(innerRect)) {
                            SettingsHelper.LatestVersion.AssociationDictionary[weapon.defName]++;
                            if (SettingsHelper.LatestVersion.AssociationDictionary[weapon.defName] > ammoType.battery) {
                                SettingsHelper.LatestVersion.AssociationDictionary[weapon.defName] = 0;
                            }
                        }
                        Rect position = new Rect(innerRect.x + list2.ColumnWidth - 24f, innerRect.y, 24f, 24f);
                        Widgets.DrawTextureFitted(position, Utility.ImageAssociation(SettingsHelper.LatestVersion.AssociationDictionary[weapon.defName]), 1);
                        Text.Anchor = anchor;
                    }
                }
                list.EndSection(list2);
            }
            list.End();
            Widgets.EndScrollView();
            ammunitionSettings.Write();
            }
            catch (Exception ex) {
                Log.Warning(ex.Message + "- Something went wrong, resetting mod settings.");
                SettingsHelper.LatestVersion.Reset();
            }
        }
    }
}
