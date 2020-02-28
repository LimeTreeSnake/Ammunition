using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Ammunition.Comps;

namespace Ammunition.Settings {
    internal class Settings : ModSettings {
        #region Fields
        private Dictionary<string, string> associationDictionary;
        private HashSet<string> excluded;
        private int percent;
        private Vector2 scrollPosition = Vector2.zero;
        private string filter = "";
        #endregion Fields     
        #region Properties
        public Dictionary<string, string> AssociationDictionary => associationDictionary;
        public HashSet<string> Excluded => excluded;
        public int Percent => percent;
        #endregion Properties
        #region Methods
        public Settings() {
        }
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref associationDictionary, "AssociationDictionary", LookMode.Value);
            Scribe_Collections.Look(ref excluded, "Excluded", LookMode.Value);
            Scribe_Values.Look(ref percent, "Percent");
        }
        public void Initialize() {
            if (associationDictionary == null)
                associationDictionary = new Dictionary<string, string>();
            if (excluded == null)
                excluded = new HashSet<string>();
        }        

        public void DoWindowContents(Rect inRect) {
            try {
                inRect.yMin += 20;
                inRect.yMax -= 20;
                Listing_Standard list = new Listing_Standard();
                Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
                Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, inRect.height * 2 + 600);
                Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
                list.Begin(rect2);
                list.Label(Language.Language.AmmoFetchPercent);
                percent = (byte)Mathf.Round(list.Slider(percent, 0, 100));


                list.Label(Language.Language.Exclude);
                filter = list.TextEntryLabeled(Language.Language.Filter, filter, 1);
                Listing_Standard list2 = list.BeginSection(600);
                list2.ColumnWidth = (rect2.width - 50) / 4;
                foreach (ThingDef def in Utility.Utility.Weapons) {
                    if (string.IsNullOrEmpty(filter) || def.label.ToUpper().Contains(filter.ToUpper())) {
                        bool contains = Excluded.Contains(def.defName);
                        list2.CheckboxLabeled(def.label, ref contains);
                        if (contains == false && Excluded.Contains(def.defName)) {
                            Excluded.Remove(def.defName);
                        }
                        else if (contains == true && !Excluded.Contains(def.defName)) {
                            Excluded.Add(def.defName);
                        }
                    }
                }
                list.EndSection(list2);

                list.End();
                Widgets.EndScrollView();
                Write();
            }
            catch (Exception ex) {
                Log.Message(ex.Message);
            }
        }
        #endregion Methods
    }

    internal static class SettingsHelper {
        public static Settings LatestVersion;
    }
    public class Ammunition : Mod {
        private Settings settings;
        public Ammunition(ModContentPack content) : base(content) {
            settings = GetSettings<Settings>();
            SettingsHelper.LatestVersion = settings;
            SettingsHelper.LatestVersion.Initialize();
        }
        public override string SettingsCategory() {
            return "Ammunition";
        }
        public override void DoSettingsWindowContents(Rect inRect) {
            settings.DoWindowContents(inRect);
        }
    }
}
