using Verse;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Ammunition {
    internal class AmmunitionSettings : ModSettings {

        #region Fields
        private static readonly List<WeaponAssociation> weaponAssociation = new List<WeaponAssociation>();
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
        public List<WeaponAssociation> WeaponAssociation = weaponAssociation.Count > 0 ? weaponAssociation : new List<WeaponAssociation>();
        #endregion Properties
        public override void ExposeData() {
            Scribe_Values.Look(ref NeedAmmo, "NeedAmmo", needAmmo);
            Scribe_Values.Look(ref NPCNeedAmmo, "NPCNeedAmmo", npcNeedAmmo);
            Scribe_Values.Look(ref FetchAmmo, "FetchAmmo", fetchAmmo);
            Scribe_Values.Look(ref DesiredAmmo, "DesiredAmmo", desiredAmmo);
            Scribe_Values.Look(ref LeastAmmoFetch, "LeastAmmoFetch", leastAmmoFetch);
            Scribe_Values.Look(ref NPCMinAmmo, "NPCMinAmmo", npcMinAmmo);
            Scribe_Values.Look(ref NPCMaxAmmo, "NPCMaxAmmo", npcMaxAmmo);
            Scribe_Values.Look(ref WeaponViewHeight, "WeaponViewHeight", weaponViewHeight);
            Scribe_Collections.Look(ref WeaponAssociation, "WeaponAssociation", LookMode.Deep);
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
            WeaponAssociation.Clear();
            Utility.CheckWeaponAssociation();
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

        public TextAnchor Anchor { get; private set; }

        public ModMain(ModContentPack content) : base(content) {
            ammunitionSettings = GetSettings<AmmunitionSettings>();
            SettingsHelper.LatestVersion = ammunitionSettings != null ? ammunitionSettings : new AmmunitionSettings();
            Utility.CheckWeaponAssociation();
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
            if (ammunitionSettings.WeaponAssociation != null) {
                list.Label(string.Format("Ammunition used for weapons."));
                list.Label(string.Format("({0}) Height.", ammunitionSettings.WeaponViewHeight));
                ammunitionSettings.WeaponViewHeight = (int)Mathf.Round(list.Slider(ammunitionSettings.WeaponViewHeight, 1, 2500));
                ammunitionSettings.Filter = list.TextEntryLabeled("Filter:", ammunitionSettings.Filter, 1);
                Listing_Standard list2 = list.BeginSection(ammunitionSettings.WeaponViewHeight);
                list2.ColumnWidth = (rect2.width - 50) / 3;
                foreach (WeaponAssociation association in ammunitionSettings.WeaponAssociation) {
                    if (association.WeaponDef.ToUpper().Contains(ammunitionSettings.Filter.ToUpper())) {
                        float lineHeight = Text.LineHeight;
                        Rect innerRect = list2.GetRect(lineHeight);
                        TextAnchor anchor = Text.Anchor;
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(innerRect, association.WeaponLabel);
                        if (Widgets.ButtonInvisible(innerRect)) {
                            association.Ammo++;
                            if (association.Ammo > ammoType.battery) {
                                association.Ammo = 0;
                            }
                        }
                        Rect position = new Rect(innerRect.x + list2.ColumnWidth- 24f, innerRect.y, 24f, 24f);
                        Widgets.DrawTextureFitted(position, Utility.ImageAssociation(association), 1);
                        Text.Anchor = anchor;
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
