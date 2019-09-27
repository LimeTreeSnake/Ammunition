using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Ammunition {
    public static class Toils_Take {
        public static Toil TakeToInventory(TargetIndex ind, Func<int> countGetter) {
            Toil takeThing = new Toil();
            takeThing.initAction = delegate {
                Pawn actor = takeThing.actor;
                Thing thing = actor.CurJob.GetTarget(ind).Thing;

                int num = Math.Min(Mathf.Min(countGetter(), thing.stackCount), MassUtility.CountToPickUpUntilOverEncumbered(actor, thing));
                if (num <= 0) {
                    actor.jobs.curDriver.ReadyForNextToil();
                }
                else {
                    actor.inventory.GetDirectlyHeldThings().TryAdd(thing.SplitOff(num));
                    thing.def.soundPickup.PlayOneShot(new TargetInfo(actor.Position, actor.Map));
                }

            };
            return takeThing;
        }
    }

}
