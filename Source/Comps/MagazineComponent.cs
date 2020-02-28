using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using UnityEngine;

namespace Ammunition.Comps {
    public class MagazineComponent : ThingComp {
        private int desired;
        public CompProps_Magazine Props => (CompProps_Magazine)props;
        public int Count = 0;
        public int Desired { get => desired; set => desired = Props.AmmoCapacity > value && value > 0 ? desired = value : desired; }
        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look(ref Count, "Count");
            Scribe_Values.Look(ref desired, "Desired");
        }        
    }

    public class CompProps_Magazine : CompProperties {
        public int AmmoCapacity;
        public string AmmoDef;
        public string Icon;

        public CompProps_Magazine() {
            compClass = typeof(MagazineComponent);
        }
    }
}
