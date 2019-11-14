using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Ammunition {
    [StaticConstructorOnStartup]
    internal static class HarmonyAmmunition {
        static HarmonyAmmunition() {

            HarmonyInstance harmonyInstance = HarmonyInstance.Create("rimworld.limetreesnake.ammunition");

            #region Ticks
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "TickRare"), null, new HarmonyMethod(typeof(HarmonyAmmunition).GetMethod("TickRare_PostFix")));
            #endregion Ticks

            #region Functionality
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos"), null, new HarmonyMethod(typeof(HarmonyAmmunition).GetMethod("GetGizmos_PostFix")));
            harmonyInstance.Patch(AccessTools.Method(typeof(JobGiver_PickUpOpportunisticWeapon), "TryGiveJob"), null, new HarmonyMethod(typeof(HarmonyAmmunition).GetMethod("TryGiveJob_PostFix")));
            harmonyInstance.Patch(AccessTools.Method(typeof(Verb_LaunchProjectile), "WarmupComplete"), new HarmonyMethod(typeof(HarmonyAmmunition).GetMethod("WarmupComplete_Ranged_PreFix")), null);
            harmonyInstance.Patch(typeof(PawnGenerator).GetMethods().FirstOrDefault(x => x.Name == "GeneratePawn" && x.GetParameters().Count() == 1), null, new HarmonyMethod(typeof(HarmonyAmmunition).GetMethod("GeneratePawn_PostFix")));
            #endregion Functionality

            Utility.CleanUpList();
            Utility.CheckWeaponAssociation();
            foreach (ThingDef def in Utility.AvailableWeapons) {
                def.comps.Add(new CompProps_Ammunition());
            };
        }

        #region Ticks
        public static void TickRare_PostFix(Pawn __instance) {
            if (__instance.Spawned && __instance.IsFreeColonist) {
                if (SettingsHelper.LatestVersion.FetchAmmo && __instance.jobs.curJob.def.casualInterruptible && !__instance.jobs.curJob.playerForced && !__instance.IsFighting() && !PawnUtility.WillSoonHaveBasicNeed(__instance)) {
                    Utility.AmmoCheck(__instance, out List<ThingDef> ammo);
                    if (ammo.Count > 0) {
                        foreach (ThingDef a in ammo) {
                            Utility.FetchAmmo(__instance, a);
                        }
                    }
                }
            }
        }
        #endregion Ticks
        #region Functionality

        [HarmonyPriority(200)]
        public static bool WarmupComplete_Ranged_PreFix(Verb_LaunchProjectile __instance) {
            if (Utility.Eligable(__instance) && SettingsHelper.LatestVersion.NeedAmmo) {
                if (__instance.CasterPawn.IsColonistPlayerControlled) {
                    return !Utility.TryFire(__instance, out ThingDef thing);
                }
                else if (SettingsHelper.LatestVersion.NPCNeedAmmo) {
                    return !Utility.TryFire(__instance, out ThingDef thing);
                }
            }
            return true;
        }
        public static void GeneratePawn_PostFix(Pawn __result) {
            if (__result != null && __result.equipment != null && __result.equipment.Primary != null) {
                ThingDef ammodef = Utility.WeaponAmmunition(__result.equipment.Primary.def);
                if (ammodef != null) {
                    Thing ammo = ThingMaker.MakeThing(ammodef);
                    ammo.stackCount = Rand.Range(25, 75);
                    __result.inventory.innerContainer.TryAdd(ammo);
                }
            }
        }
        public static void GetGizmos_PostFix(ref IEnumerable<Gizmo> __result, ref Pawn __instance) {
            try {
                if (__instance != null) {

                }
            }
            catch (Exception ex) {
                Log.Message(ex.Message);
            }
        }
        public static void TryGiveJob_PostFix(Pawn pawn, Job __result) {
            try {
                if (__result != null) {
                    ThingDef def = ((Thing)__result.targetA).def;
                    if (def != null) {
                        ThingDef defOf = Utility.WeaponAmmunition(def);
                        if (defOf == null || pawn.inventory.innerContainer.FirstOrDefault(x => x.def == defOf) == null)
                            __result = null;

                    }
                }
            }
            catch (Exception ex) {
                Log.Message(ex.Message);
            }
        }
        #endregion Functionality
    }
}