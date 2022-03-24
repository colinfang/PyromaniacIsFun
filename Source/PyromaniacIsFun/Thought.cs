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
    public class ThoughtWorker_NeedPyromania : ThoughtWorker
    {
        // See `ThoughtWorker_NeedJoy`
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.needs.TryGetNeed<NeedPyromania>() is {} need)
            {
                return need.CurCategory switch
                {
                    PyromaniaCategory.VeryLow => ThoughtState.ActiveAtStage(0),
                    PyromaniaCategory.Low => ThoughtState.ActiveAtStage(1),
                    PyromaniaCategory.Satisfied => ThoughtState.Inactive,
                    PyromaniaCategory.High => ThoughtState.ActiveAtStage(2),
                    PyromaniaCategory.VeryHigh => ThoughtState.ActiveAtStage(3),
                    var cat => throw new NotImplementedException($"{cat} is not handled")
                };
            }
            else
            {
                return ThoughtState.Inactive;
            }
        }
    }

    public class ThoughtWorker_PyromaniacHappy : ThoughtWorker
    {
        // Modified from ThoughtWorker_IsCarryingIncendiaryWeapon
        // Do not generate thought for e.g. `Gun_SmokeLauncher`
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.equipment.Primary is null)
            {
                return false;
            }
            if (Patcher.Settings.HappyWhenCarryingTrulyIncendiaryWeapon) {
            // TODO: This is the standard way to get verbs from an equipment
                foreach (var verb in p.equipment.Primary.GetComp<CompEquippable>().AllVerbs)
                {
                    // If it is loadable (only mortar in vanilla), get the loaded projectile
                    if (verb.GetProjectile()?.projectile.damageDef == DamageDefOf.Flame)
                    {
                        return true;
                    }
                }
            } else {
                // Original
                foreach (var verb in p.equipment.Primary.GetComp<CompEquippable>().AllVerbs)
                {
                    if (verb.IsIncendiary())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}