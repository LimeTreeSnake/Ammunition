using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;

namespace Ammunition.Comps {
    public class AmmunitionComponent : ThingComp {
        
    }

    public class CompProps_Ammunition : CompProperties {
        public List<string> defs;

        public CompProps_Ammunition() {
            compClass = typeof(AmmunitionComponent);
        }
    }
}
