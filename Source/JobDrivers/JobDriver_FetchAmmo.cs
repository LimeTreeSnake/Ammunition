using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;
using UnityEngine;
using System;
using Verse.Sound;

namespace Ammunition.JobDrivers {
    public class JobDriver_FetchAmmo : JobDriver {
        public override bool TryMakePreToilReservations(bool errorOnFailed) {
            Pawn pawn = base.pawn;
            LocalTargetInfo targetA = base.job.targetA;
            Job job = base.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils() {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.A);
            Utility.Utility.GetMagAndAmmo(TargetC.Thing as Apparel, out Comps.MagazineComponent mag, out ThingDef ammo);
            this.FailOn(() => mag == null || ammo == null);
            int count = mag.Props.AmmoCapacity - mag.Count;

            int num = Mathf.Min(job.targetA.Thing.stackCount, count);

            Toil reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A);
            yield return reserveTargetA;
            Toil go = Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
            yield return go;
            Toil take = Toils_Take.LoadMagazine(TargetIndex.A, () => num, num, mag);
            yield return take;
            if (pawn.Drafted) {
                Toil go2 = Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
                yield return go2;
            }
            yield break;
        }
    }
    public static class Toils_Take {
        public static Toil LoadMagazine(TargetIndex ind, Func<int> countGetter, int count, Comps.MagazineComponent mag) {
            Toil toil = new Toil();
            toil.initAction = delegate {
                Pawn actor = toil.actor;
                Thing thing = actor.CurJob.GetTarget(ind).Thing;
                if (count <= 0) {
                    actor.jobs.curDriver.ReadyForNextToil();
                }
                else {
                    mag.Count += count;
                    thing.SplitOff(count);
                    thing.def.soundPickup.PlayOneShot(new TargetInfo(actor.Position, actor.Map));
                }
            };
            //toil.tickAction = delegate {
            //    Pawn actor = toil.actor;
            //    Thing thing = actor.CurJob.GetTarget(ind).Thing;
            //};
            return toil;
        }
    }
}
