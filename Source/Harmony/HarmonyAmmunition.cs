using Harmony;
using RimWorld;
using System.Linq;
using Verse;

namespace Ammunition {
    [StaticConstructorOnStartup]
    internal static class HarmonyAmmunition {
        static HarmonyAmmunition() {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("rimworld.limetreesnake.ammunition");
            #region Ticks
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "TickRare"), null, new HarmonyMethod(typeof(HarmonyAmmunition).GetMethod("TickRare_PostFix")));
            #endregion Ticks
            #region Functionality
            harmonyInstance.Patch(AccessTools.Method(typeof(Verb_LaunchProjectile), "WarmupComplete"), new HarmonyMethod(typeof(HarmonyAmmunition).GetMethod("WarmupComplete_Ranged_PreFix")), null);

            harmonyInstance.Patch(typeof(PawnGenerator).GetMethods().FirstOrDefault(x => x.Name == "GeneratePawn" && x.GetParameters().Count() == 1 ), null, new HarmonyMethod(typeof(HarmonyAmmunition).GetMethod("GeneratePawn_PostFix")));
            #endregion Functionality
        }


        #region Ticks
        public static void TickRare_PostFix(Pawn __instance) {
            if (__instance.Spawned && __instance.IsFreeColonist) {
                if (SettingsHelper.LatestVersion.FetchAmmo && __instance.jobs.posture == 0 && !__instance.jobs.curJob.def.alwaysShowWeapon && Utility.AmmoCheck(__instance, out ThingDef ammo)) {
                    Utility.FetchAmmo(__instance, ammo);
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
            if (__result != null) {
               ThingDef ammodef = Utility.WeaponCheck(__result);
                if(ammodef != null) {
                    Thing ammo = ThingMaker.MakeThing(ammodef);
                    ammo.stackCount = Rand.Range(25, 75);
                    __result.inventory.innerContainer.TryAdd(ammo);
                }
            }
        }
        #endregion Functionality
    }
}