#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace CF_PyromaniacIsFun
{
    [HarmonyPatch(typeof(PawnObserver))]
    [HarmonyPatch("ObserveSurroundingThings")]
    public static class Patch_PawnObserver_ObserveSurroundingThings
    {
        public static readonly MethodInfo PawnObserver_PossibleToObserve = typeof(PawnObserver).GetMethod("PossibleToObserve", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new ArgumentException("PawnObserver.PossibleToObserve is not found");

        public static void TryCreateObservedThought(Thing thing, Pawn pawn)
        {
            if (thing is Fire fire)
            {
                // See `Building_Skullspike.GiveObservedThought`
                Thought_MemoryObservation thought;
                if (fire.parent is Pawn pawnOnFire)
                {
                    thought = (Thought_MemoryObservation)ThoughtMaker.MakeThought(PyromaniacUtility.ObservedBurningPawnDef);
                    thought.Target = pawnOnFire;
                    PyromaniacUtility.ThrowText(pawn, () => $"CF_PyromaniacIsFun_PyromaniacUtility_ObservedFireOnPawn".Translate(fire, pawnOnFire), 4);
                }
                else
                {
                    thought = (Thought_MemoryObservation)ThoughtMaker.MakeThought(PyromaniacUtility.ObservedWildFireDef);
                    thought.Target = fire;
                    PyromaniacUtility.ThrowText(pawn, () => $"CF_PyromaniacIsFun_PyromaniacUtility_ObservedFire".Translate(fire), 4);
                }
                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            }
        }

        public static void Postfix(PawnObserver __instance, Pawn ___pawn)
        {
            var pawn = ___pawn;
            RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => pawn.Position.InHorDistOf(to.extentsClose.ClosestCellTo(pawn.Position), 5f), delegate (Region reg)
            {
                foreach (Thing item in reg.ListerThings.ThingsInGroup(ThingRequestGroup.Fire))
                {
                    if ((bool)PawnObserver_PossibleToObserve.Invoke(__instance, new object[] { item }))
                    {
                        TryCreateObservedThought(item, pawn);
                    }
                }
                return true;
            });
        }
    }


}