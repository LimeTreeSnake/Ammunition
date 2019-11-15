using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;

namespace Ammunition {
    public class Ammunition_Comp : ThingComp {
        #region Fields
        private int desiredCount = SettingsHelper.LatestVersion.DesiredAmmo;
        #endregion Fields

        #region Properties
        public int DesiredCount {
            get { return desiredCount; }
            set { desiredCount = value; }
        }
        #endregion Properties

        #region Methods
        public void ExposeData() {
            Scribe_Values.Look(ref desiredCount, "DesiredCount");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            if (parent.def.race != null && parent.def.race.intelligence == Intelligence.Humanlike) {
                Pawn pawn = (Pawn)parent;
                if (pawn.equipment.AllEquipmentListForReading.Count > 0) {
                    foreach (Thing thing in pawn.equipment.AllEquipmentListForReading) {
                        if (SettingsHelper.LatestVersion.AssociationDictionary.ContainsKey(thing.def.defName)) {
                            int current = Utility.AmmunitionStatus(pawn, thing.def);
                            yield return new Command_Action {
                                icon = Utility.ImageAssociation(SettingsHelper.LatestVersion.AssociationDictionary[thing.def.defName]),
                                defaultLabel = current + "/" + SettingsHelper.LatestVersion.DesiredAmmo,
                                defaultDesc = "AmmoGizmoDescription".Translate(),
                                action = delegate {
                                    if (current < SettingsHelper.LatestVersion.DesiredAmmo) {
                                        Utility.FetchAmmo(pawn, Utility.AmmunitionFinder(SettingsHelper.LatestVersion.AssociationDictionary[thing.def.defName]));
                                    }
                                }
                            };
                        }
                    }
                }
            }
        }

        #endregion Methods
    }

    public class CompProps_Ammunition : CompProperties {
        public CompProps_Ammunition() {
            compClass = typeof(Ammunition_Comp);
        }
    }
}
