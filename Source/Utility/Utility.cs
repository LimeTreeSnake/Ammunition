using Ammunition.Comps;
using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.AI;

namespace Ammunition.Utility {
    public static class Utility {
        public static IEnumerable<ThingDef> Weapons => DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsRangedWeapon && x.equipmentType == EquipmentType.Primary && x.weaponTags != null && x.Verbs.FirstOrDefault(y => (y.verbClass.Name.Contains("Verb_Shoot")) || y.verbClass.Name.Contains("Verb_LaunchProjectile")) != null && x.Verbs.FirstOrDefault(y => !y.verbClass.Name.Contains("Verb_ShootOneUse")) != null && !x.destroyOnDrop);
        public static IEnumerable<ThingDef> Ammo => DefDatabase<ThingDef>.AllDefs.Where(x => x.GetCompProperties<CompProps_Ammunition>() != null);
        public static IEnumerable<ThingDef> Mags => DefDatabase<ThingDef>.AllDefs.Where(x => x.GetCompProperties<CompProps_Magazine>() != null);
        public static bool Fire(Thing thing, Pawn pawn) {
            Settings.SettingsHelper.LatestVersion.AssociationDictionary.TryGetValue(thing.def.defName.ToLower(), out string ammo);
            if (ammo != null) {
                Apparel apparel = pawn.apparel.WornApparel.FirstOrDefault(x => Mags.FirstOrDefault(y => y.defName.ToLower() == x.def.defName.ToLower()) != null);
                if (apparel != null) {
                    MagazineComponent mag = apparel.GetComp<MagazineComponent>();
                    if (mag != null && mag.Props.AmmoDef.ToLower() == ammo) {
                        if (mag.Count > 0) {
                            mag.Count--;
                            return true;
                        }
                    }
                }
                else {
                    Messages.Message(Language.Language.NoMag(pawn, ammo), MessageTypeDefOf.NegativeEvent);
                }
                pawn.jobs.ClearQueuedJobs();
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                pawn.equipment.Remove((ThingWithComps)thing);
                pawn.inventory.innerContainer.TryAddOrTransfer(thing);
                Messages.Message(Language.Language.OutOfAmmo(pawn), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            return true;
        }
        public static void AmmoCheck() {
            foreach (ThingDef def in Weapons) {
                if (!Settings.SettingsHelper.LatestVersion.AssociationDictionary.ContainsKey(def.defName.ToLower())) {
                    string defOf = null;
                    foreach (ThingDef ammos in Ammo) {
                        if (ammos.GetCompProperties<CompProps_Ammunition>().defs.FirstOrDefault(x => x.ToLower() == def.defName.ToLower()) != null) {
                            defOf = ammos.defName.ToLower();
                        }
                    }
                    if (defOf == null)
                        switch (def.techLevel) {
                            case TechLevel.Neolithic:
                                defOf = Defs.ThingDefOf.AmmoPrimitive.defName.ToLower();
                                break;
                            case TechLevel.Medieval:
                                defOf = Defs.ThingDefOf.AmmoPrimitive.defName.ToLower();
                                break;
                            case TechLevel.Industrial:
                                defOf = Defs.ThingDefOf.AmmoIndustrial.defName.ToLower();
                                break;
                            case TechLevel.Spacer:
                                defOf = Defs.ThingDefOf.AmmoCharge.defName.ToLower();
                                break;
                            case TechLevel.Ultra:
                                defOf = Defs.ThingDefOf.AmmoCharge.defName.ToLower();
                                break;
                            case TechLevel.Archotech:
                                defOf = Defs.ThingDefOf.AmmoCharge.defName.ToLower();
                                break;
                            default:
                                defOf = Defs.ThingDefOf.AmmoIndustrial.defName.ToLower();
                                break;
                        }
                    if (defOf != null)
                        Settings.SettingsHelper.LatestVersion.AssociationDictionary.Add(def.defName.ToLower(), defOf);
                }
            }
        }
        public static void GetMagAndAmmo(Apparel apparel, out MagazineComponent mag, out ThingDef ammo) {
            mag = apparel.GetComp<MagazineComponent>();
            string def = mag.Props.AmmoDef.ToLower();
            ammo = Ammo.FirstOrDefault(x => x.defName.ToLower() == def.ToLower());
        }
        public static void DropAmmo(MagazineComponent mag, IntVec3 pos) {
            DebugThingPlaceHelper.DebugSpawn(Ammo.FirstOrDefault(x => x.defName.ToLower() == mag.Props.AmmoDef.ToLower()), pos, mag.Count);
            mag.Count = 0;
        }
        public static bool FetchAmmo(Pawn pawn, Apparel apparel, MagazineComponent mag, int radius) {
            bool validator(Thing t) {
                if (t.IsForbidden(pawn)) {
                    return false;
                }
                return true;
            }
            ThingDef ammoDef = Ammo.FirstOrDefault(x => x.defName.ToLower() == mag.Props.AmmoDef.ToLower());
            Thing ammo = GenClosest.ClosestThing_Global_Reachable(
                pawn.Position,
                pawn.Map,
                pawn.Map.listerThings.ThingsOfDef(ammoDef),
                PathEndMode.OnCell,
                TraverseParms.For(pawn),
                radius,
                validator);
            if (ammo != null) {
                Job job = new Job(Defs.JobDefOf.JobDriver_FetchAmmo, ammo, pawn.Position, apparel);
                return pawn.jobs.TryTakeOrderedJob(job);
            }
            return false;
        }
        public static void EquipPawn(Pawn pawn, Thing thing) {
            Settings.SettingsHelper.LatestVersion.AssociationDictionary.TryGetValue(thing.def.defName.ToLower(), out string ammo);
            if (ammo != null) {
                Apparel apparel = pawn.apparel.WornApparel.FirstOrDefault(x => Mags.FirstOrDefault(y => y.defName.ToLower() == x.def.defName.ToLower()) != null);
                if (apparel != null) {
                    MagazineComponent mag = apparel.TryGetComp<MagazineComponent>();
                    if (mag.Props.AmmoDef.ToLower() == ammo.ToLower()) {
                        mag.Count = Rand.Range(1, mag.Props.AmmoCapacity);
                        return;
                    }
                }
                IEnumerable<ThingDef> app = Mags.Where(x => x.GetCompProperties<CompProps_Magazine>().AmmoDef.ToLower() == ammo.ToLower());
                if (app != null && app.Count() > 0) {
                    ThingDef def = app.RandomElement();
                    apparel = ThingMaker.MakeThing(def, GenStuff.RandomStuffFor(def)) as Apparel;
                    if (apparel != null && pawn.apparel.CanWearWithoutDroppingAnything(def)) {
                        pawn.apparel.Wear(apparel);
                        MagazineComponent mag = apparel.TryGetComp<MagazineComponent>();
                        mag.Count = Rand.Range(1, mag.Props.AmmoCapacity);
                    }
                }
            }
        }
    }
}
