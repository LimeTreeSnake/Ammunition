using Ammunition.Comps;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;


namespace Ammunition.Things {
    [StaticConstructorOnStartup]
    public class Magazine : Apparel {
        public override IEnumerable<Gizmo> GetWornGizmos() {
            if (Find.Selector.SingleSelectedThing == Wearer && Wearer.IsColonist) {
                MagazineComponent mags = this.GetComp<MagazineComponent>();
                if (mags != null) {
                    Command_Action reloadCommand = new Command_Action();
                    reloadCommand.defaultLabel = mags.Count + "/" + mags.Props.AmmoCapacity;
                    reloadCommand.defaultDesc = Language.Language.Reload;
                    reloadCommand.icon = ContentFinder<Texture2D>.Get(mags.Props.Icon);
                    reloadCommand.alsoClickIfOtherInGroupClicked = false;
                    reloadCommand.action = delegate {
                        if (mags.Count == mags.Props.AmmoCapacity)
                            return;
                        Utility.Utility.FetchAmmo(Wearer, this, mags, 999);
                    };
                    yield return reloadCommand;
                    if (mags.Count > 0) {
                        Command_Action unloadCommand = new Command_Action();
                        unloadCommand.defaultLabel = Language.Language.ClearAmmo(mags.Count);
                        unloadCommand.defaultDesc = Language.Language.ClearAmmoDesc;
                        unloadCommand.icon = ContentFinder<Texture2D>.Get(mags.Props.Icon);
                        unloadCommand.alsoClickIfOtherInGroupClicked = false;
                        unloadCommand.action = delegate {
                            Utility.Utility.DropAmmo(mags, Wearer.Position);
                        };
                        yield return unloadCommand;
                    }
                }
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn) {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn)) {
                yield return floatMenuOption;
            }
            MagazineComponent mags = this.GetComp<MagazineComponent>();
            if (mags.Count > 0) {
                void dropAmmo() {
                    Utility.Utility.DropAmmo(mags, Position);
                }
                yield return new FloatMenuOption(Language.Language.ClearAmmo(mags.Count), dropAmmo, MenuOptionPriority.High);
            }
        }
    }
}
