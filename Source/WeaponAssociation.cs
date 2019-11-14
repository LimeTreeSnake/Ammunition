using RimWorld;
using System;
using Verse;

namespace Ammunition {
    public enum ammoType { none = 0, primitive = 1, preindustrial, industrial, chemical, acid, nitrogen, battery };
    public class WeaponAssociation : IExposable {
        private static ammoType ammo = 0;
        private static string weaponDef = "";
        private static string weaponLabel = "";

        public ammoType Ammo = ammo;
        public string WeaponDef = weaponDef;
        public string WeaponLabel = weaponLabel;
        public void ExposeData() {
            Scribe_Values.Look(ref Ammo, "Ammo", ammo);
            Scribe_Values.Look(ref WeaponDef, "WeaponDef", weaponDef);
            Scribe_Values.Look(ref WeaponLabel, "WeaponLabel", weaponLabel);
        }
    }
}
