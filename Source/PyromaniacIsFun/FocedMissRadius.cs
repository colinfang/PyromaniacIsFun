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
    [HarmonyPatch(typeof(VerbProperties))]
    [HarmonyPatch(nameof(VerbProperties.GetForceMissFactorFor))]
    public static class Patch_VerbProperties_GetForceMissFactorFor
    {
        public static void Postfix(VerbProperties __instance, Thing equipment, Pawn caster, ref float __result)
        {
            if (Patcher.Settings.RemoveForcedMissRadius && __instance.defaultProjectile?.projectile.damageDef == DamageDefOf.Flame
                && equipment.def.IsRangedWeapon
                && caster.IsPyromaniac()
            )
            {
                __result = 0;
                PyromaniacUtility.ThrowText(caster, () => $"CF_PyromaniacIsFun_PyromaniacUtility_ReducedForcedMissedRadius".Translate(caster, equipment), 4);

            }
        }
    }


    [HarmonyPatch(typeof(ShotReport))]
    [HarmonyPatch(nameof(ShotReport.HitReportFor))]
    public static class Patch_ShotReport_HitReportFor
    {
        public static readonly FieldInfo ShotReport_forcedMissRadius = typeof(ShotReport).GetField("forcedMissRadius", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new ArgumentException("ShotReport.forcedMissRadius is not found");
        public static void Postfix(ref ShotReport __result, Thing caster, Verb verb)
        {
            if (Patcher.Settings.RemoveForcedMissRadius && caster is Pawn pawn && verb.GetProjectile()?.projectile.damageDef == DamageDefOf.Flame
                && pawn.IsPyromaniac()
            )
            {
                // TODO: This involves copy, not ideal
                object r = __result;
                ShotReport_forcedMissRadius.SetValue(r, 0);
                __result = (ShotReport)r;
            }
        }
    }


}