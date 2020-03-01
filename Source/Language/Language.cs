using Verse;

namespace Ammunition.Language {
    public static class Language {
        public static string AmmunitionAssociation => "AmmunitionAssociation".Translate();
        public static string Filter => "Filter".Translate();
        public static string Exclude => "Exclude".Translate();
        public static string Reload => "Reload".Translate();
        public static string OutOfAmmo(Pawn pawn) => "OutOfAmmo".Translate(pawn.Named("PAWN"));
        public static string NoMag(Pawn pawn, string ammo) => "NoMag".Translate(ammo,pawn.Named( "PAWN"));
        public static string ClearAmmo(int count) => "ClearAmmo".Translate(count);
        public static string ClearAmmoDesc => "ClearAmmoDesc".Translate();
        public static string AmmoFetchPercent => "AmmoFetchPercent".Translate();
    }
}
