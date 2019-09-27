using Verse;
using UnityEngine;

namespace Ammunition {
    internal class AmmunitionSettings : ModSettings {

        #region Fields
        private static readonly bool needAmmo = true;
        private static readonly bool npcNeedAmmo = false;
        private static readonly bool fetchAmmo = false;
        private static readonly bool dropWeapon = true;
        private static readonly int desiredAmmo = 25;
        private static readonly int leastAmmoFetch = 3;
        private static readonly int npcMinAmmo = 15;
        private static readonly int npcMaxAmmo = 45;
        #endregion Fields
        #region Properties
        public bool NeedAmmo = needAmmo;
        public bool NPCNeedAmmo = npcNeedAmmo;
        public bool FetchAmmo = fetchAmmo;
        public bool DropWeapon = dropWeapon;
        public int DesiredAmmo = desiredAmmo;
        public int LeastAmmoFetch = leastAmmoFetch;
        public int NPCMinAmmo = npcMinAmmo;
        public int NPCMaxAmmo = npcMaxAmmo;
        #endregion Properties
        public override void ExposeData() {
            Scribe_Values.Look(ref NeedAmmo, "NeedAmmo", needAmmo);
            Scribe_Values.Look(ref NPCNeedAmmo, "NPCNeedAmmo", npcNeedAmmo);
            Scribe_Values.Look(ref FetchAmmo, "FetchAmmo", fetchAmmo);
            Scribe_Values.Look(ref DropWeapon, "DropWeapon", dropWeapon);
            Scribe_Values.Look(ref DesiredAmmo, "DesiredAmmo", desiredAmmo);
            Scribe_Values.Look(ref LeastAmmoFetch, "LeastAmmoFetch", leastAmmoFetch);
            Scribe_Values.Look(ref NPCMinAmmo, "NPCMinAmmo", npcMinAmmo);
            Scribe_Values.Look(ref NPCMaxAmmo, "NPCMaxAmmo", npcMaxAmmo);
        }

        public void Reset() {
            NeedAmmo = needAmmo;
            NPCNeedAmmo = npcNeedAmmo;
            FetchAmmo = fetchAmmo;
            DropWeapon = dropWeapon;
            DesiredAmmo = desiredAmmo;
            LeastAmmoFetch = leastAmmoFetch;
            NPCMinAmmo = npcMinAmmo;
            NPCMaxAmmo = npcMaxAmmo;
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
        private AmmunitionSettings ammunitionSettings;

        public ModMain(ModContentPack content) : base(content) {
            ammunitionSettings = GetSettings<AmmunitionSettings>();
            SettingsHelper.LatestVersion = ammunitionSettings;
        }
        public override string SettingsCategory() {
            return "Ammunition";
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            inRect.yMin += 20;
            inRect.yMax -= 20;
            Listing_Standard list = new Listing_Standard();
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 16f, inRect.height * 2 + 450f);
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
            list.CheckboxLabeled("Drop weapons when out of ammo.", ref ammunitionSettings.DropWeapon);
            list.CheckboxLabeled("NPC needs ammo.", ref ammunitionSettings.NPCNeedAmmo);
            list.Label(string.Format("NPC's spawn if appliable with a minumum of {0} ammo.", ammunitionSettings.NPCMinAmmo));
            ammunitionSettings.NPCMinAmmo = (int)Mathf.Round(list.Slider(ammunitionSettings.NPCMinAmmo, 1, 100));
            list.Label(string.Format("NPC's spawn if appliable with a maxmimum of {0} ammo.", ammunitionSettings.NPCMaxAmmo));
            ammunitionSettings.NPCMaxAmmo = (int)Mathf.Round(list.Slider(ammunitionSettings.NPCMaxAmmo, ammunitionSettings.NPCMinAmmo, 200));


            list.End();
            Widgets.EndScrollView();
            ammunitionSettings.Write();
        }
    }
}
