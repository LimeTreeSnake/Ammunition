using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using UnityEngine;

namespace Ammunition
{
    public class Ammunition_Comp : ThingComp
    {
        #region Fields
        private int desiredCount = SettingsHelper.LatestVersion.DesiredAmmo;
        private bool fetchAmmo = true;
        #endregion Fields

        #region Properties
        public int DesiredCount {
            get { return desiredCount; }
            set { desiredCount = value; }
        }
        public bool FetchAmmo { get => fetchAmmo; set => fetchAmmo = value; }
        #endregion Properties

        #region Methods
        public void ExposeData() {
            Scribe_Values.Look(ref desiredCount, "DesiredCount");
            Scribe_Values.Look(ref fetchAmmo, "FetchAmmo");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            if (parent.def.race != null && parent.def.race.intelligence == Intelligence.Humanlike) {
                Pawn pawn = (Pawn)parent;
                if (pawn.equipment.AllEquipmentListForReading.Count > 0) {
                    foreach (Thing thing in pawn.equipment.AllEquipmentListForReading) {
                        if (SettingsHelper.LatestVersion.AssociationDictionary.ContainsKey(thing.def.defName)) {
                            int current = Utility.AmmunitionStatus(pawn, Utility.AmmunitionFinder(SettingsHelper.LatestVersion.AssociationDictionary[thing.def.defName]));
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
                    if (pawn.equipment.AllEquipmentListForReading.FirstOrDefault(x => (x.def.IsRangedWeapon && SettingsHelper.LatestVersion.AssociationDictionary.ContainsKey(x.def.defName))) != null)
                        yield return new Command_Action {
                            icon = fetchAmmo ?
                            ContentFinder<Texture2D>.Get("Designations/Hunt", true) :
                            ContentFinder<Texture2D>.Get("UI/Icons/medical/NoCare", true),
                            defaultLabel = fetchAmmo ? "Fetch".Translate() : "NoFetch".Translate(),
                            defaultDesc = "FetchDescription".Translate(),
                            action = delegate {
                                fetchAmmo = !fetchAmmo;
                            }
                        };
                }
            }
        }

        #endregion Methods
    }

    public class CompProps_Ammunition : CompProperties
    {
        public CompProps_Ammunition() {
            compClass = typeof(Ammunition_Comp);
        }
    }
}
