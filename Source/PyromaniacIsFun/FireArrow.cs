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
    [HarmonyPatch(typeof(DefGenerator))]
    [HarmonyPatch(nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    public static class Patch_DefGeneratorGenerateImpliedDefs_PreResolve
    {
        public static ThingDef GenerateFireArrow(ThingDef arrow, ThingDef fireArrowGeneric)
        {
            var fireArrow = Gen.MemberwiseClone(arrow);
            fireArrow.defName = "Fire_" + arrow.defName;
            fireArrow.label = arrow.label + " with fire";
            fireArrow.graphicData = fireArrowGeneric.graphicData;
            // Make a copy in order to modify
            // TODO: Any better to clone?
            fireArrow.projectile = Gen.MemberwiseClone(arrow.projectile);
            fireArrow.projectile.extraDamages = new();
            if (arrow.projectile.extraDamages is not null)
            {
                fireArrow.projectile.extraDamages.AddRange(arrow.projectile.extraDamages);
            }
            if (fireArrowGeneric.projectile.extraDamages is not null)
            {
                fireArrow.projectile.extraDamages.AddRange(fireArrowGeneric.projectile.extraDamages);
            }
            return fireArrow;
        }


        public static void Postfix()
        {
            foreach (var t in DefDatabase<ThingDef>.AllDefs)
            {
                if (t.projectile?.damageDef is { defName: "Arrow" or "ArrowHighVelocity"}){
                    var fireArrow = GenerateFireArrow(t, PyromaniacUtility.FireArrowGenericDef);
                    PyromaniacUtility.ArrowDict.Add(t, fireArrow);
                }

            }
            foreach (var fireArrow in PyromaniacUtility.ArrowDict.Values)
            {
                DefGenerator.AddImpliedDef(fireArrow);
            }
        }
    }

    [HarmonyPatch(typeof(Verb_LaunchProjectile))]

    public static class Patch_Verb_LaunchProjectile_Projectile
    {
        public static bool AllowFireArrow = false;
        public static NeedPyromania? Need;

        [HarmonyPrefix]
        [HarmonyPatch("TryCastShot")]
        public static void Prefix_TryCastShot(Verb_LaunchProjectile __instance) {
            if (__instance.caster is Pawn pawn && pawn.IsPyromaniac()
                && Rand.Chance(0.6f)
            )
            {
                if (pawn.needs.TryGetNeed<NeedPyromania>() is { } need)
                {
                    if (need.CurLevel > Patcher.Settings.NeedPyromaniaPerFireArrow)
                    {
                        AllowFireArrow = true;
                        Need = need;
                    }
                }
                else
                {
                    AllowFireArrow = true;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Verb_LaunchProjectile.Projectile), MethodType.Getter)]
        public static void Postfix_Projectile(Verb_LaunchProjectile __instance, ref ThingDef __result)
        {
            // Patch arrows in all `.Projectile` access in `.TryCastShot`.
            // In vanilla only the first one is needed, but the sequence might change due to mod.
            if (AllowFireArrow && PyromaniacUtility.ArrowDict.TryGetValue(__result, out var fireArrow))
            {
                __result = fireArrow;
            } else
            {
                // Reset it if no fire arrow is shot
                Need = null;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("TryCastShot")]
        public static void Postfix_TryCastShot() {
            Need?.AdjustExternally(-Patcher.Settings.NeedPyromaniaPerFireArrow);
            // reset
            AllowFireArrow = false;
            Need = null;
        }
    }

}