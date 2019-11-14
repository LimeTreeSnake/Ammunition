
using Verse;
using System.Linq;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;
using UnityEngine;

namespace Ammunition {
    public static class Utility {

        private static Texture2D Acid => ContentFinder<Texture2D>.Get("UI/Icons/Acid", true);
        private static Texture2D Battery => ContentFinder<Texture2D>.Get("UI/Icons/Battery", true);
        private static Texture2D Chemical => ContentFinder<Texture2D>.Get("UI/Icons/Chemical", true);
        private static Texture2D Industrial => ContentFinder<Texture2D>.Get("UI/Icons/Industrial", true);
        private static Texture2D Nitrogen => ContentFinder<Texture2D>.Get("UI/Icons/Nitrogen", true);
        private static Texture2D PreIndustrial => ContentFinder<Texture2D>.Get("UI/Icons/PreIndustrial", true);
        private static Texture2D Primitive => ContentFinder<Texture2D>.Get("UI/Icons/Primitive", true);

        public static IEnumerable<ThingDef> AvailableWeapons = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsRangedWeapon && x.equipmentType == EquipmentType.Primary && x.weaponTags != null && x.Verbs.FirstOrDefault(y => y.verbClass.Name.Contains("Verb_Shoot")) != null && x.Verbs.FirstOrDefault(y => !y.verbClass.Name.Contains("Verb_ShootOneUse")) != null && !x.destroyOnDrop);


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
        public static void AmmoCheck(Pawn pawn, out List<ThingDef> ammoType) {
            ammoType = new List<ThingDef>();
            if (pawn.equipment.AllEquipmentListForReading.Count > 0) {
                foreach (ThingWithComps def in pawn.equipment.AllEquipmentListForReading) {
                    ThingDef ammo = WeaponAmmunition(def.def);
                    if (ammo != null) {
                        int count = AmmoCount(pawn, ammo) + SettingsHelper.LatestVersion.LeastAmmoFetch;
                        if (count < SettingsHelper.LatestVersion.DesiredAmmo) {
                            ammoType.Add(ammo);
                        }
                    }
                }
            }
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
                    Job job = new Job(JobDefOf.FetchAmmunitionCase, thing, pawn.Position);
                    return pawn.jobs.TryTakeOrderedJob(job);
                }
            }
            return false;
        }
        public static bool TryFire(Verb verb, out ThingDef requiredAmmo) {
            bool failed = false;
            ThingDef ammo = WeaponAmmunition(verb.CasterPawn.equipment.Primary.def);
            if (ammo != null) {
                List<Thing> ammunition = verb.CasterPawn.inventory.innerContainer.Where(x => x.def == ammo).ToList();
                if (ammunition.Count > 0) {
                    ammunition[0].stackCount--;
                    if (ammunition[0].stackCount < 1)
                        ammunition[0].Destroy();
                }
                else {
                    failed = true;
                    if (!FetchAmmo(verb.CasterPawn, ammo)) {
                        verb.CasterPawn.jobs.ClearQueuedJobs();
                        verb.CasterPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                        verb.CasterPawn.equipment.Remove(verb.EquipmentSource);
                        verb.CasterPawn.inventory.innerContainer.TryAdd(verb.EquipmentSource);
                    }
                    Messages.Message("OutOfAmmo".Translate(ammo.label, verb.CasterPawn.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
                }
            }
            requiredAmmo = ammo;
            return failed;
        }
        public static void CheckWeaponAssociation() {
            foreach (ThingDef def in AvailableWeapons) {
                AddWeaponAssociation(def);
            }
        }
        public static void AddWeaponAssociation(ThingDef weapon) {
            WeaponAssociation weaponassociation = SettingsHelper.LatestVersion.WeaponAssociation.FirstOrDefault(x => x.WeaponDef == weapon.defName);
            if (weaponassociation == null || weaponassociation.WeaponDef == "" || weaponassociation.WeaponLabel == "") {
                if (weapon != null)
                    SettingsHelper.LatestVersion.WeaponAssociation.Remove(weaponassociation);
                weaponassociation = new WeaponAssociation {
                    WeaponDef = weapon.defName,
                    WeaponLabel = weapon.label
                };
                if (weapon.HasModExtension<AmmoExtension>()) {
                    weaponassociation.Ammo = weapon.GetModExtension<AmmoExtension>().ammo;
                    SettingsHelper.LatestVersion.WeaponAssociation.Add(weaponassociation);
                }
                else {
                    weaponassociation.Ammo = WeaponTranslationCheck(weapon);
                    SettingsHelper.LatestVersion.WeaponAssociation.Add(weaponassociation);
                }
            }
        }
        public static ThingDef WeaponAmmunition(ThingDef weapon) {
            WeaponAssociation associated = SettingsHelper.LatestVersion.WeaponAssociation.FirstOrDefault(x => x.WeaponDef == weapon.defName);
            if (associated != null)
                return AmmunitionFinder(associated.Ammo);
            return null;
        }
        public static ThingDef WeaponAmmunition(string weapon) {
            WeaponAssociation associated = SettingsHelper.LatestVersion.WeaponAssociation.FirstOrDefault(x => x.WeaponDef == weapon);
            if (associated != null)
                return AmmunitionFinder(associated.Ammo);
            return null;
        }
        public static ThingDef AmmunitionFinder(ammoType ammo) {
            switch (ammo) {
                case ammoType.primitive:
                    return ThingDefOf.PrimitiveAmmunitionCase;
                case ammoType.preindustrial:
                    return ThingDefOf.PreIndustrialAmmunitionCase;
                case ammoType.industrial:
                    return ThingDefOf.IndustrialAmmunitionCase;
                case ammoType.chemical:
                    return ThingDefOf.ChemicalAmmunitionCanister;
                case ammoType.acid:
                    return ThingDefOf.AcidAmmunitionCanister;
                case ammoType.nitrogen:
                    return ThingDefOf.NitrogenAmmunitionCanister;
                case ammoType.battery:
                    return ThingDefOf.BatteryAmmunitionCharge;
            }
            return null;
        }
        public static ammoType AmmunitionFinder(ThingDef ammo) {

            if (ammo.defName == ThingDefOf.PrimitiveAmmunitionCase.defName) {
                return ammoType.primitive;
            }
            else if (ammo.defName == ThingDefOf.PreIndustrialAmmunitionCase.defName) {
                return ammoType.preindustrial;
            }
            else if (ammo.defName == ThingDefOf.IndustrialAmmunitionCase.defName) {
                return ammoType.industrial;
            }
            else if (ammo.defName == ThingDefOf.ChemicalAmmunitionCanister.defName) {
                return ammoType.chemical;
            }
            else if (ammo.defName == ThingDefOf.AcidAmmunitionCanister.defName) {
                return ammoType.acid;
            }
            else if (ammo.defName == ThingDefOf.NitrogenAmmunitionCanister.defName) {
                return ammoType.nitrogen;
            }
            else if (ammo.defName == ThingDefOf.BatteryAmmunitionCharge.defName) {
                return ammoType.battery;
            }
            else return 0;
        }
        public static ammoType WeaponTranslationCheck(ThingDef weapon) {
            VerbProperties prop = weapon.Verbs.FirstOrDefault(x => x.verbClass.Name.Contains("Verb_Shoot") && !x.verbClass.Name.Contains("OneUse"));
            if (prop != null) {
                ThingDef damageType = prop.defaultProjectile;
                if (damageType.projectile.damageDef == DamageDefOf.Arrow || damageType.projectile.damageDef == DamageDefOf.Blunt || damageType.projectile.damageDef == DamageDefOf.Crush || damageType.projectile.damageDef == DamageDefOf.Cut || damageType.projectile.damageDef == DamageDefOf.Scratch || damageType.projectile.damageDef == DamageDefOf.Stab) {
                    return ammoType.primitive;
                }
                else if (damageType.projectile.damageDef == DamageDefOf.Flame || damageType.projectile.damageDef == DamageDefOf.Burn) {
                    return ammoType.chemical;
                }
                else if (damageType.projectile.damageDef == DamageDefOf.Stun || damageType.projectile.damageDef == DamageDefOf.EMP) {
                    return ammoType.battery;
                }
                else if (damageType.projectile.damageDef == DamageDefOf.Frostbite) {
                    return ammoType.nitrogen;
                }
                else if (damageType.projectile.damageDef == DamageDefOf.Bullet || damageType.projectile.damageDef == DamageDefOf.Bomb) {
                    if (damageType.defName.Contains("harge") || damageType.defName.Contains("aser") || damageType.defName.Contains("olarisbloc"))
                        return ammoType.battery;
                    else if (damageType.defName.Contains("usket") || damageType.defName.Contains("lint") || damageType.defName.Contains("olarisbloc"))
                        return ammoType.preindustrial;
                    return ammoType.industrial;
                }
                else {
                    if (damageType.projectile.damageDef.defName.Contains("angedStab") || damageType.projectile.damageDef.defName.Contains("lunt") || damageType.projectile.damageDef.defName.Contains("rrow") || damageType.projectile.damageDef.defName.Contains("axe")) {
                        return ammoType.primitive;
                    }
                    else if (damageType.projectile.damageDef.defName.Contains("laze") || damageType.projectile.damageDef.defName.Contains("ire") || damageType.projectile.damageDef.defName.Contains("cind")) {
                        return ammoType.chemical;
                    }
                    else if (damageType.projectile.damageDef.defName.Contains("lectr") || damageType.projectile.damageDef.defName.Contains("lasma") || damageType.projectile.damageDef.defName.Contains("aser") || damageType.projectile.damageDef.defName.Contains("harge") || damageType.projectile.damageDef.defName.Contains("nergy") || damageType.projectile.damageDef.defName.Contains("hock") || damageType.projectile.damageDef.defName.Contains("ptic") || damageType.projectile.damageDef.defName.Contains("auss") || damageType.projectile.damageDef.defName.Contains("last")) {
                        return ammoType.battery;
                    }
                    else if (damageType.projectile.damageDef.defName.Contains("cid") || damageType.projectile.damageDef.defName.Contains("lague")) {
                        return ammoType.acid;
                    }
                    else if (damageType.projectile.damageDef.defName.Contains("old") || damageType.projectile.damageDef.defName.Contains("ce")) {
                        return ammoType.nitrogen;
                    }
                    else if (damageType.projectile.damageDef.defName.Contains("ullet") || damageType.defName.Contains("ullet")) {
                        return ammoType.industrial;
                    }
                }
            }
            return 0;

        }
        public static Texture2D ImageAssociation(WeaponAssociation associate) {
            switch (associate.Ammo) {
                case ammoType.primitive:
                    return Primitive;
                case ammoType.preindustrial:
                    return PreIndustrial;
                case ammoType.industrial:
                    return Industrial;
                case ammoType.chemical:
                    return Chemical;
                case ammoType.acid:
                    return Acid;
                case ammoType.nitrogen:
                    return Nitrogen;
                case ammoType.battery:
                    return Battery;
            }
            return Widgets.CheckboxOffTex;
        }

        public static int AmmunitionStatus(Pawn pawn, WeaponAssociation associate) {
            int current = 0;
            IEnumerable<Thing> list = pawn.inventory.innerContainer.Where(x => x.def.defName == WeaponAmmunition(associate.WeaponDef).defName);
            foreach(Thing thing in list) {
                current += thing.stackCount;
            }
            return current;
        }

        public static void CleanUpList() {
            List<WeaponAssociation> removal = new List<WeaponAssociation>();
            foreach (WeaponAssociation association in SettingsHelper.LatestVersion.WeaponAssociation) {
                ThingDef def = AvailableWeapons.FirstOrDefault(x => x.defName == association.WeaponDef);
                if (def == null) {
                    removal.Add(association);
                }
            }
            if (removal.Count > 0) {
                foreach (WeaponAssociation association in removal) {
                    SettingsHelper.LatestVersion.WeaponAssociation.Remove(association);
                }
            }
        }
    }
}
