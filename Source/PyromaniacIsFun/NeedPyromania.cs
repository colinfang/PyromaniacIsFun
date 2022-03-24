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

    public enum PyromaniaCategory: byte
    {
        VeryLow,
        Low,
        Satisfied,
        High,
        VeryHigh
    }

    [HarmonyPatch(typeof(Pawn_NeedsTracker))]
    [HarmonyPatch("ShouldHaveNeed")]
    public static class Patch_Pawn_NeedsTracker_ShouldHaveNeed
    {
        public static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result, Pawn ___pawn)
        {
            if (__result && nd == PyromaniacUtility.NeedPyromaniaDef)
            {
                if (!ModsConfig.RoyaltyActive || !___pawn.IsPyromaniac())
                {
                    // Disable if Royalty DLC not installed, or a pawn is not pyromaniac
                    __result = false;
                }
            }
        }
    }

    public class NeedPyromania : Need
    {
        // See `Need_Joy`
        public bool IsAdjustExternally => Find.TickManager.TicksGame < lastAdjustExternallyTick + 10;
        private int lastAdjustExternallyTick = -999;
        private float lastAdjustExternallyAmount;
        public string? ExplanationFromAdjustExternally;
        public override int GUIChangeArrow => IsFrozen ? 0 : (IsAdjustExternally ? Math.Sign(lastAdjustExternallyAmount) : Math.Sign(lastDelta));

        public void AdjustExternally(float amount, string? explaination=null)
        {
            // Called by `JobDriver_Meditate.MeditationTick`
            // Or by e.g. fire arrow
            CurLevel = Mathf.Clamp01(CurLevel + amount);
            lastAdjustExternallyTick = Find.TickManager.TicksGame;
            lastAdjustExternallyAmount = amount;
            ExplanationFromAdjustExternally = explaination;
        }

        public const float ThresholdVeryLow = 0.1f;
        public const float ThresholdLow = 0.3f;
        public const float ThresholdSatisfied = 0.6f;
        public const float ThresholdHigh = 0.8f;

        public PyromaniaCategory CurCategory => CurLevel switch
        {
            < ThresholdVeryLow => PyromaniaCategory.VeryLow,
            < ThresholdLow => PyromaniaCategory.Low,
            < ThresholdSatisfied => PyromaniaCategory.Satisfied,
            < ThresholdHigh => PyromaniaCategory.High,
            _ => PyromaniaCategory.VeryHigh
        };

        public override string GetTipString()
        {
            var text = base.GetTipString();
            return text + "\n\n" + ExplanationAll;
        }

        // End of see `Need_joy`

        public float lastDelta;
        public string ExplanationFromObservedFire = "";
        public string ExplanationAll = "";

        public void CheckSelfOnFire()
        {
            if (pawn.HasAttachment(ThingDefOf.Fire))
            {
                var thought = (Thought_Memory)ThoughtMaker.MakeThought(PyromaniacUtility.SelfOnFireDef);
                PyromaniacUtility.ThrowText(pawn, () => $"I am on fire", 4);
                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            }

        }

        public float GainFromObservedFireInterval()
        {
            CheckSelfOnFire();
            int numWildFire = 0;
            int minAgeWildFire = int.MaxValue;
            int numBurningPawn = 0;
            int minAgeBurningPawn = int.MaxValue;
            int numSelfOnFire = 0;
            int minAgeSelfOnFire = int.MaxValue;
            var sb = new StringBuilder();
            if (pawn.needs.mood?.thoughts.memories is { } handler)
            {
                foreach (var thought in handler.Memories)
                {
                    if (thought.def == PyromaniacUtility.ObservedWildFireDef)
                    {
                        numWildFire += 1;
                        minAgeWildFire = Math.Min(minAgeWildFire, thought.age);
                    } else if (thought.def == PyromaniacUtility.ObservedBurningPawnDef)
                    {
                        numBurningPawn += 1;
                        minAgeBurningPawn = Math.Min(minAgeBurningPawn, thought.age);
                    } else if (thought.def == PyromaniacUtility.SelfOnFireDef)
                    {
                        numSelfOnFire += 1;
                        minAgeSelfOnFire = Math.Min(minAgeSelfOnFire, thought.age);
                    }
                }
            }
            float gain = 0;
            if (numWildFire > 0)
            {
                var gainFromWildFire = numWildFire * Patcher.Settings.NeedPyromaniaGainPerWildFirePerDay;
                gain += gainFromWildFire;
                sb.AppendFormat("Saw wild fire x{0}: +{1:F0}% ~{2}\n",
                    numWildFire, gainFromWildFire * 100,
                    (PyromaniacUtility.ObservedWildFireDef.DurationTicks - minAgeWildFire).ToStringTicksToPeriod()
                );
            }
            if (numBurningPawn > 0)
            {
                var gainFromBurningPawn = numBurningPawn * Patcher.Settings.NeedPyromaniaGainPerBurningPawnPerDay;
                gain += gainFromBurningPawn;
                sb.AppendFormat("Saw burning pawn x{0}: +{1:F0}% ~{2}\n",
                    numBurningPawn, gainFromBurningPawn * 100,
                    (PyromaniacUtility.ObservedBurningPawnDef.DurationTicks - minAgeBurningPawn).ToStringTicksToPeriod()
                );
            }
            if (numSelfOnFire > 0)
            {
                var gainFromSelfOnFire = numSelfOnFire * Patcher.Settings.NeedPyromaniaGainSelfOnFirePerDay;
                gain += gainFromSelfOnFire;
                sb.AppendFormat("I am on fire: +{1:F0}% ~{2}\n",
                    numSelfOnFire, gainFromSelfOnFire * 100,
                    (PyromaniacUtility.SelfOnFireDef.DurationTicks - minAgeSelfOnFire).ToStringTicksToPeriod()
                );
            }
            ExplanationFromObservedFire = sb.ToString();
            return gain;
        }
        public override void NeedInterval()
        {
            if (IsFrozen)
            {
                return;
            }
            if (IsAdjustExternally)
            {
                ExplanationAll = ExplanationFromAdjustExternally ?? "";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Base change rate: -{def.fallPerDay * 100:F0}%");
            var gain = -def.fallPerDay;
            var gainFromObservedFireInterval = GainFromObservedFireInterval();
            if (gainFromObservedFireInterval > 0)
            {
                gain += gainFromObservedFireInterval;
                sb.Append(ExplanationFromObservedFire);
            }

            sb.AppendLine($"\nFinal change rate: {gain * 100:+0;-0}% / day ({gain * 100 / GenDate.HoursPerDay:F0}% / hour)");
            lastDelta = gain * NeedTunings.NeedUpdateInterval / GenDate.TicksPerDay;
            CurLevel = Mathf.Clamp01(CurLevel + lastDelta);
            ExplanationAll = sb.ToString();
        }

        public float GetMentalBreakProtectThreshold()
        {
            // A pawn would be protected from FireStartingSpree is his mood is higher than this threshold
            var threshold = 1 - CurLevel;
            var breakThresholdExtreme = pawn.mindState.mentalBreaker.BreakThresholdExtreme;
            if (threshold < breakThresholdExtreme)
            {
                threshold = breakThresholdExtreme;
            }
            return threshold;
        }

        public NeedPyromania(Pawn pawn) : base(pawn)
        {
            // See `Need_Beauty`
            threshPercents = new()
            {
                ThresholdVeryLow,
                ThresholdLow,
                ThresholdSatisfied,
                ThresholdHigh
            };
        }
    }



    [HarmonyPatch(typeof(JobDriver_Meditate))]
    [HarmonyPatch("MeditationTick")]
    public static class Patch_JobDriver_Meditate_MeditationTick
    {
        public static void Postfix(JobDriver_Meditate __instance)
        {
            var pawn = __instance.pawn;
            // See `Pawn_PsychicEntropyTracker.GainPsyfocus`
            if (__instance.Focus.Thing is ThingWithComps thing && !thing.Destroyed
                && pawn.needs.TryGetNeed<NeedPyromania>() is { }  need
                && thing.def.GetCompProperties<CompProperties_MeditationFocus>() is { } comp
                && comp.focusTypes.Any(focusDef => focusDef == PyromaniacUtility.FocusFlameDef)
            )
            {
                var valuePerDay = thing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) * Patcher.Settings.NeedPyromaniaGainFromMeditationMultiplier;
                need.AdjustExternally(valuePerDay / GenDate.TicksPerDay, $"Watching flame: +{valuePerDay * 100:F0}% / day (+{valuePerDay * 100 / GenDate.HoursPerDay:F0}% / hour)");
            }
        }
    }


}