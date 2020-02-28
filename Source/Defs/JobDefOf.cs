using RimWorld;
using Verse;
namespace Ammunition.Defs {
	[DefOf]
	public static class JobDefOf {

		public static JobDef JobDriver_FetchAmmo;

		static JobDefOf() {
			DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
		}
	}
}