
using Verse;
using System.Linq;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using System;

namespace Ammunition {
    public static class Utility {
        public static bool Eligable(Verb __instance) {
            if (__instance.CasterIsPawn && !__instance.CasterPawn.AnimalOrWildMan() && !__instance.caster.def.IsBuildingArtificial && __instance.CasterPawn.equipment != null && __instance.CasterPawn.equipment.Primary != null) {
                return true;
            }
            return false;
        }
        public static int AmmoCount(Pawn pawn, ThingDef ammo) {
            List<Thing> ammoList = pawn.inventory.innerContainer.Where(x => x.def == ammo).ToList();
            int ammocount = 0;
            if (ammoList.Count > 0)
                ammoList.ForEach(x => ammocount += x.stackCount);
            return ammocount;
        }
        public static bool AmmoCheck(Pawn pawn, out ThingDef ammoType) {
            bool needAmmo = false;
            ammoType = null;
            if (pawn.CanCasuallyInteractNow() && !pawn.IsFighting() && !pawn.jobs.curJob.playerForced && !PawnUtility.WillSoonHaveBasicNeed(pawn)) {
                ammoType = WeaponCheck(pawn);
                if (ammoType != null) {
                    int count = AmmoCount(pawn, ammoType) + SettingsHelper.LatestVersion.LeastAmmoFetch;
                    if (count < SettingsHelper.LatestVersion.DesiredAmmo) {
                        needAmmo = true;
                    }
                }
            }
            return needAmmo;
        }
        public static ThingDef WeaponCheck(Pawn pawn) {
            if (pawn.equipment != null && pawn.equipment.Primary != null) {
                VerbProperties prop = pawn.equipment.Primary.def.Verbs.FirstOrDefault(x => x.verbClass.Name == "Verb_Shoot");
                ThingDef shooter = null;
                if (prop != null) {
                    shooter = prop.defaultProjectile;
                }
                if (shooter != null) {
                    if (shooter.projectile.damageDef == DamageDefOf.Arrow || shooter.projectile.damageDef == DamageDefOf.Blunt || shooter.projectile.damageDef == DamageDefOf.Crush || shooter.projectile.damageDef == DamageDefOf.Cut || shooter.projectile.damageDef == DamageDefOf.Scratch || shooter.projectile.damageDef == DamageDefOf.Stab || shooter.projectile.damageDef.defName.Contains("angedStab") || shooter.projectile.damageDef.defName.Contains("lunt") || shooter.projectile.damageDef.defName.Contains("rrow") || shooter.projectile.damageDef.defName.Contains("axe")) {
                        return ThingDefOf.PrimitiveAmmunitionCase;
                    }
                    else if (shooter.projectile.damageDef == DamageDefOf.Bullet || shooter.projectile.damageDef == DamageDefOf.Bomb || shooter.projectile.damageDef.defName.Contains("ullet")) {
                        if (shooter.defName.Contains("harge"))
                            return ThingDefOf.BatteryAmmunitionCharge;
                        return ThingDefOf.IndustrialAmmunitionCase;
                    }
                    else if (shooter.projectile.damageDef == DamageDefOf.Flame || shooter.projectile.damageDef == DamageDefOf.Burn || shooter.projectile.damageDef.defName.Contains("laze") || shooter.projectile.damageDef.defName.Contains("ire") || shooter.projectile.damageDef.defName.Contains("cind")) {
                        return ThingDefOf.ChemicalAmmunitionCanister;
                    }
                    else if (shooter.projectile.damageDef == DamageDefOf.Stun || shooter.projectile.damageDef == DamageDefOf.EMP || shooter.projectile.damageDef.defName.Contains("lectr") || shooter.projectile.damageDef.defName.Contains("lasma") || shooter.projectile.damageDef.defName.Contains("aser") || shooter.projectile.damageDef.defName.Contains("harge") || shooter.projectile.damageDef.defName.Contains("nergy") || shooter.projectile.damageDef.defName.Contains("hock") || shooter.projectile.damageDef.defName.Contains("ptic")) {
                        return ThingDefOf.BatteryAmmunitionCharge;
                    }
                    else if (shooter.projectile.damageDef == DamageDefOf.Frostbite || shooter.projectile.damageDef.defName.Contains("old") || shooter.projectile.damageDef.defName.Contains("ce")) {
                        return ThingDefOf.NitrogenAmmunitionCanister;
                    }
                }
            }
            return null;

        }
        public static bool FetchAmmo(Pawn pawn, ThingDef ammoType) {
            bool validator(Thing t) {
                if (t.IsForbidden(pawn)) {
                    return false;
                }
                return true;
            }
            Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerThings.ThingsOfDef(ammoType), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, validator);

            if (thing != null) {
                if (MassUtility.CountToPickUpUntilOverEncumbered(pawn, thing) > SettingsHelper.LatestVersion.LeastAmmoFetch) {
                    Job job = new Job(JobDefOf.FetchAmmunitionCase, thing);
                    return pawn.jobs.TryTakeOrderedJob(job);
                }
            }
            return false;
        }
        public static bool TryFire(Verb verb, out ThingDef requiredAmmo, bool npc = false) {
            bool failed = false;
            ThingDef ammo = WeaponCheck(verb.CasterPawn);
            if (ammo != null) {
                List<Thing> ammunition = verb.CasterPawn.inventory.innerContainer.Where(x => x.def == ammo).ToList();
                if (ammunition.Count > 0) {
                    ammunition[0].stackCount--;
                    if (ammunition[0].stackCount < 1)
                        ammunition[0].Destroy();
                }
                else {
                    failed = true;
                    if (SettingsHelper.LatestVersion.DropWeapon || npc) {
                        if (!FetchAmmo(verb.CasterPawn, ammo)) {
                            verb.CasterPawn.equipment.TryDropEquipment(verb.EquipmentSource, out ThingWithComps dropped, verb.CasterPawn.Position, true);
                        }
                    }
                    if (!npc)
                        Messages.Message("OutOfAmmo".Translate(ammo.label, verb.CasterPawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }
            }
            requiredAmmo = ammo;
            return failed;
        }
    }
}
