using RimWorld;
using Verse;

namespace Ammunition {
    [DefOf]
    public static class ThingDefOf {
        public static ThingDef IndustrialAmmunitionCase;
        public static ThingDef PrimitiveAmmunitionCase;
        public static ThingDef ChemicalAmmunitionCanister;
        public static ThingDef NitrogenAmmunitionCanister;
        public static ThingDef BatteryAmmunitionCharge;
        static ThingDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }
    }
    [DefOf]
    public static class JobDefOf {
        public static JobDef FetchAmmunitionCase;
        static JobDefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDef));
        }
    }
}
