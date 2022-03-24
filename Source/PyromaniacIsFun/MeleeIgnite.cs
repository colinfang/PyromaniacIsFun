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
    [HarmonyPatch(typeof(VerbUtility))]
    [HarmonyPatch(nameof(VerbUtility.FinalSelectionWeight))]

    public static class Patch_VerbUtility_FinalSelectionWeight
    {
        public static void Postfix(Verb verb, Pawn p, ref float __result)
        {
            if (p.IsPyromaniac())
            {
                var need = p.needs.TryGetNeed<NeedPyromania>();
                if (need is null || need.CurLevel > Patcher.Settings.NeedPyromaniaPerIgnite)
                {
                    if (verb.maneuver == PyromaniacUtility.IgniteDef)
                    {
                        var chance = Patcher.Settings.MeleeIgniteChance;
                        // Assume original weight = 0, and all weights sum to 1
                        // In fact they don't add to 1 often
                        // weight / (1 + weight)
                        __result = chance / (1 - chance);
                    }
                }
            }
        }
    }

    class Verb_MeleeAttackDamageIgnite: Verb_MeleeAttackDamage
    {
        protected override bool TryCastShot()
        {
            // TODO: Fire icon
            if (CasterPawn?.needs.TryGetNeed<NeedPyromania>() is { } need)
            {

                need.AdjustExternally(-Patcher.Settings.NeedPyromaniaPerIgnite);
            }
            // Enemies don't consume NeedPyromania
            return base.TryCastShot();
        }
    }
}