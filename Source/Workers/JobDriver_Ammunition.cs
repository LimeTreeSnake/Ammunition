using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;
using UnityEngine;

namespace Ammunition {
    public class JobDriver_FetchAmmunitionCase : JobDriver {

        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            Pawn pawn = base.pawn;
            LocalTargetInfo targetA = base.job.targetA;
            Job job = base.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils() {
            //FAIL ON OVERBURDENED
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.A);

            int ammocount = Utility.AmmoCount(pawn, Utility.WeaponCheck(pawn));
            this.FailOn(() => ammocount >= SettingsHelper.LatestVersion.DesiredAmmo);
            int max = MassUtility.CountToPickUpUntilOverEncumbered(pawn, job.targetA.Thing);
            int num = 0;
            if (pawn.IsFighting()) {
                num = Mathf.Min(job.targetA.Thing.stackCount, max);
            }
            else {
                num = Mathf.Min(Mathf.Min(SettingsHelper.LatestVersion.DesiredAmmo - ammocount, job.targetA.Thing.stackCount), max);
            }

            this.FailOn(() => num < 3);

            Toil reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A);
            yield return reserveTargetA;
            Toil go = Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
            yield return go;
            Toil take = Toils_Take.TakeToInventory(TargetIndex.A, () => num);
            yield return take;
            yield break;
        }
    }
}
