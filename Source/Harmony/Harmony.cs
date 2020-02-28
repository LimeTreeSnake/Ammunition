using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;
using System.Collections.Generic;

namespace Ammunition.Harmony {
    [StaticConstructorOnStartup]
    internal static class Harmony {
        static Harmony() {
            var harmony = new HarmonyLib.Harmony("limetreesnake.ammunition");
            harmony.Patch(AccessTools.Method(typeof(Verb_LaunchProjectile), "WarmupComplete")
                , new HarmonyMethod(typeof(Harmony).GetMethod("WarmupComplete_PreFix"))
                , null);
            harmony.Patch(typeof(PawnGenerator).GetMethods().FirstOrDefault(x => x.Name == "GeneratePawn" && x.GetParameters().Count() == 1), null, new HarmonyMethod(typeof(Harmony).GetMethod("GeneratePawn_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "RedressPawn"), null, new HarmonyMethod(typeof(Harmony).GetMethod("RedressPawn_PostFix")));
            Utility.Utility.AmmoCheck();
        }
        public static bool WarmupComplete_PreFix(Verb __instance) {
            if (__instance.EquipmentSource != null && __instance.EquipmentSource.def.IsRangedWeapon && !Settings.SettingsHelper.LatestVersion.Excluded.Contains(__instance.EquipmentSource.def.defName.ToLower())) {
                return Utility.Utility.Fire(__instance.EquipmentSource, __instance.CasterPawn);
            }
            return true;
        }
        public static void GeneratePawn_PostFix(Pawn __result) {
            if (__result != null && __result.equipment != null && __result.equipment.Primary != null && !Settings.SettingsHelper.LatestVersion.Excluded.Contains(__result.equipment.Primary.def.defName.ToLower())) {
                Utility.Utility.EquipPawn(__result, __result.equipment.Primary);
            }
        }
        public static void RedressPawn_PostFix(Pawn pawn) {
            if (pawn != null && pawn.equipment != null && pawn.equipment.Primary != null) {
                Utility.Utility.EquipPawn(pawn, pawn.equipment.Primary);
            }
        }
    }
}
