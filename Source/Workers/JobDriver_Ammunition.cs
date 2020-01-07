using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;
using UnityEngine;

namespace Ammunition {
    public class JobDriver_FetchAmmunitionCase : JobDriver {

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            //FAIL ON OVERBURDENED
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.A);

            int ammocount = Utility.AmmoCount(pawn, Utility.WeaponAmmunition(pawn.equipment.Primary.def));
            this.FailOn(() => ammocount >= SettingsHelper.LatestVersion.DesiredAmmo);
            int max = MassUtility.CountToPickUpUntilOverEncumbered(pawn, job.targetA.Thing);
            int num = 0;
            if (pawn.IsFighting()) {
                num = Mathf.Min(job.targetA.Thing.stackCount, max);
            }
            else {
                num = Mathf.Min(Mathf.Min(SettingsHelper.LatestVersion.DesiredAmmo - ammocount, job.targetA.Thing.stackCount), max);
            }
            Toil go = Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
            yield return go;
            Toil take = Toils_Take.TakeToInventory(TargetIndex.A, () => num);
            yield return take;
            if (pawn.Drafted) {
                Toil go2 = Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
                yield return go2;
            }
            yield break;
        }
    }
}
