using RimWorld;
using Verse;
namespace Ammunition.Defs {
    [DefOf]
    public static class ThingDefOf {

		public static ThingDef AmmoPrimitive;

		public static ThingDef AmmoIndustrial;

		public static ThingDef AmmoCharge;

		public static ThingDef AmmoBomb;

		static ThingDefOf() {
			DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
		}
	}
}
