#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace CF_PyromaniacIsFun
{
    public class Patcher : Mod
    {
        public static Settings Settings = new();
        string? needPyromaniaPerFireArrowBuffer;
        string? needPyromaniaPerIgniteBuffer;
        string? meleeIgniteChanceBuffer;
        string? needPyromaniaGainPerWildFirePerDayBuffer;
        string? needPyromaniaGainPerBurningPawnPerDayBuffer;
        string? needPyromaniaGainSelfOnFirePerDayBuffer;
        string? needPyromaniaGainFromMeditationMultiplierBuffer;

        public Patcher(ModContentPack pack) : base(pack)
        {
            Settings = GetSettings<Settings>();
            DoPatching();
        }
        public override string SettingsCategory()
        {
            return "Pyromaniac Is Fun";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);

            {
                var rect = list.Label("CF_PyromaniacIsFun_SettingText_CostPerArrow.label".Translate(), tooltip: null);
                Widgets.TextFieldNumeric(rect.RightPartPixels(50), ref Settings.NeedPyromaniaPerFireArrow, ref needPyromaniaPerFireArrowBuffer, 0, 1);
            }
            {
                var rect = list.Label("CF_PyromaniacIsFun_SettingText_CostPerMelee.label".Translate(), tooltip: null);
                Widgets.TextFieldNumeric(rect.RightPartPixels(50), ref Settings.NeedPyromaniaPerIgnite, ref needPyromaniaPerIgniteBuffer, 0, 1);
            }
            {
                var rect = list.Label("CF_PyromaniacIsFun_SettingText_MeleeIgnite.label".Translate(), tooltip: "CF_PyromaniacIsFun_SettingText_MeleeIgnite.description".Translate());
                Widgets.TextFieldNumeric(rect.RightPartPixels(50), ref Settings.MeleeIgniteChance, ref meleeIgniteChanceBuffer, 0, 0.99f);
            }
            {
                var rect = list.Label("CF_PyromaniacIsFun_SettingText_WildFire.label".Translate(), tooltip: null);
                Widgets.TextFieldNumeric(rect.RightPartPixels(50), ref Settings.NeedPyromaniaGainPerWildFirePerDay, ref needPyromaniaGainPerWildFirePerDayBuffer, 0, 1);
            }
            {
                var rect = list.Label("CF_PyromaniacIsFun_SettingText_BurningPawn.label".Translate(), tooltip: null);
                Widgets.TextFieldNumeric(rect.RightPartPixels(50), ref Settings.NeedPyromaniaGainPerBurningPawnPerDay, ref needPyromaniaGainPerBurningPawnPerDayBuffer, 0, 1);
            }
            {
                var rect = list.Label("CF_PyromaniacIsFun_SettingText_SelfOnFire.label".Translate(), tooltip: null);
                Widgets.TextFieldNumeric(rect.RightPartPixels(50), ref Settings.NeedPyromaniaGainSelfOnFirePerDay, ref needPyromaniaGainSelfOnFirePerDayBuffer, 0, 1);
            }
            {
                var rect = list.Label("CF_PyromaniacIsFun_SettingText_Meditation.label".Translate(), tooltip: "CF_PyromaniacIsFun_SettingText_Meditation.description".Translate());
                Widgets.TextFieldNumeric(rect.RightPartPixels(50), ref Settings.NeedPyromaniaGainFromMeditationMultiplier, ref needPyromaniaGainFromMeditationMultiplierBuffer, 0, 100);
            }
            list.CheckboxLabeled("CF_PyromaniacIsFun_SettingText_MoodIncendiaryWeapon.label".Translate(), ref Settings.HappyWhenCarryingTrulyIncendiaryWeapon, "CF_PyromaniacIsFun_SettingText_MoodIncendiaryWeapon.description".Translate());

            list.CheckboxLabeled("CF_PyromaniacIsFun_SettingText_AimIncendiaryWeapon.label".Translate(), ref Settings.RemoveForcedMissRadius, "CF_PyromaniacIsFun_SettingText_AimIncendiaryWeapon.description".Translate());
            list.End();
            base.DoSettingsWindowContents(inRect);
        }

        public void DoPatching()
        {
            var harmony = new Harmony("com.colinfang.PyromaniacIsFun");
            harmony.PatchAll();
        }
    }

    public class Settings : ModSettings
    {
        public float NeedPyromaniaPerFireArrow = 0.02f;
        public float NeedPyromaniaPerIgnite = 0.02f;
        public float MeleeIgniteChance = 0.4f;
        public float NeedPyromaniaGainPerWildFirePerDay = 0.04f;
        public float NeedPyromaniaGainPerBurningPawnPerDay = 0.3f;
        public float NeedPyromaniaGainSelfOnFirePerDay = 0.6f;
        public float NeedPyromaniaGainFromMeditationMultiplier = 4f;
        public bool HappyWhenCarryingTrulyIncendiaryWeapon = true;
        public bool RemoveForcedMissRadius = true;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref NeedPyromaniaPerFireArrow, nameof(NeedPyromaniaPerFireArrow), 0.02f);
            Scribe_Values.Look(ref NeedPyromaniaPerIgnite, nameof(NeedPyromaniaPerIgnite), 0.02f);
            Scribe_Values.Look(ref MeleeIgniteChance, nameof(MeleeIgniteChance), 0.4f);
            Scribe_Values.Look(ref NeedPyromaniaGainPerWildFirePerDay, nameof(NeedPyromaniaGainPerWildFirePerDay), 0.04f);
            Scribe_Values.Look(ref NeedPyromaniaGainPerBurningPawnPerDay, nameof(NeedPyromaniaGainPerBurningPawnPerDay), 0.3f);
            Scribe_Values.Look(ref NeedPyromaniaGainSelfOnFirePerDay, nameof(NeedPyromaniaGainSelfOnFirePerDay), 0.6f);
            Scribe_Values.Look(ref NeedPyromaniaGainFromMeditationMultiplier, nameof(NeedPyromaniaGainFromMeditationMultiplier), 4f);
            Scribe_Values.Look(ref HappyWhenCarryingTrulyIncendiaryWeapon, nameof(HappyWhenCarryingTrulyIncendiaryWeapon), true);
            Scribe_Values.Look(ref RemoveForcedMissRadius, nameof(RemoveForcedMissRadius), true);
            base.ExposeData();
        }
    }

    // Attribute is used for static `Texture2D`
    [StaticConstructorOnStartup]
    public static class TextureUtility
    {
        public static readonly Texture2D PyromaniaIndicator = ContentFinder<Texture2D>.Get("UI/Icons/PassionMinor");
    }


    public static class PyromaniacUtility
    {
        public static readonly ThoughtDef ObservedWildFireDef = DefDatabase<ThoughtDef>.GetNamed("CF_PyromaniacIsFun_ObservedWildFire");
        public static readonly ThoughtDef ObservedBurningPawnDef = DefDatabase<ThoughtDef>.GetNamed("CF_PyromaniacIsFun_ObservedBurningPawn");
        public static readonly ThoughtDef SelfOnFireDef = DefDatabase<ThoughtDef>.GetNamed("CF_PyromaniacIsFun_SelfOnFire");
        public static readonly ThingDef FireArrowGenericDef = DefDatabase<ThingDef>.GetNamed("CF_PyromaniacIsFun_FireArrowTemplate");
        public static readonly NeedDef NeedPyromaniaDef = DefDatabase<NeedDef>.GetNamed("CF_PyromaniacIsFun_NeedPyromania");
        public static readonly MeditationFocusDef FocusFlameDef = DefDatabase<MeditationFocusDef>.GetNamed("Flame");
        public static readonly ManeuverDef IgniteDef = DefDatabase<ManeuverDef>.GetNamed("CF_PyromaniacIsFun_Ignite");
        public static readonly MentalStateDef FireStartingSpreeDef = DefDatabase<MentalStateDef>.GetNamed("FireStartingSpree");

        public static readonly Dictionary<ThingDef, ThingDef> ArrowDict = new();
        public static bool IsDebug = false;

        [DebugAction("PyromaniacIsFun", null, false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ToggleDebug()
        {
            IsDebug = !IsDebug;
            Log.Message($"Debug is {IsDebug}");
        }

        [DebugAction("PyromaniacIsFun", null, false, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void LogPyromaniacThoughts(Pawn pawn)
        {
            if (pawn.needs.mood?.thoughts.memories is { } handler)
            {
                foreach (var thought in handler.Memories)
                {
                    if (thought.def == ObservedWildFireDef)
                    {
                        Log.Message($"{thought}");
                    }
                    else if (thought.def == ObservedBurningPawnDef)
                    {
                        Log.Message($"{thought}");
                    }
                }
            }
        }

        [DebugAction("PyromaniacIsFun", null, false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void LogArrowDict()
        {
            foreach (var (k, v) in ArrowDict)
            {
                Log.Message($"ArrowDict: {k} => {v}");
            }
        }

        [DebugAction("PyromaniacIsFun", null, false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void LogVanillaIncendiaryProjectile()
        {
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef.projectile?.ai_IsIncendiary ?? false)
                {
                    Log.Message($"Incendiary projectile: {thingDef}");
                }
            }
        }

        [DebugAction("PyromaniacIsFun", null, false, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void LogBattleEntry(Pawn pawn)
        {
            foreach (Battle battle in Find.BattleLog.Battles)
            {
                if (!battle.Concerns(pawn))
                {
                    continue;
                }
                foreach (LogEntry entry in battle.Entries)
                {
                    if (entry.Concerns(pawn))
                    {
                        Log.Message($"{battle.GetName()}: {entry}");
                        Log.Message($"{entry.ToGameStringFromPOV(pawn)}");
                    }
                }
            }
        }

        [DebugAction("PyromaniacIsFun", null, false, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RemoveBattleEntry(Pawn pawn)
        {
            int n = 0;
            foreach (Battle battle in Find.BattleLog.Battles)
            {
                if (!battle.Concerns(pawn))
                {
                    continue;
                }
                n += battle.Entries.RemoveAll(entry => entry.Concerns(pawn));
            }
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"Removed {n} entries");
        }

        [DebugAction("PyromaniacIsFun", null, false, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TriggerPyromaniaNeedInterval(Pawn pawn)
        {
            pawn.needs.TryGetNeed<NeedPyromania>()?.NeedInterval();
        }

        [DebugAction("PyromaniacIsFun", null, false, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void IncreaseNeedPyromaniaBy10(Pawn pawn) => pawn.needs.TryGetNeed<NeedPyromania>()?.AdjustExternally(0.1f);

        [DebugAction("PyromaniacIsFun", null, false, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DecreaseNeedPyromaniaBy10(Pawn pawn) => pawn.needs.TryGetNeed<NeedPyromania>()?.AdjustExternally(-0.1f);

        public static void ThrowText(Thing thing, Func<string> GetText, float timeBeforeStartFadeout)
        {
            if (IsDebug)
            {
                MoteMaker.ThrowText(thing.DrawPos, thing.Map, GetText(), timeBeforeStartFadeout);
            }
        }

        public static bool IsPyromaniac(this Pawn pawn) => pawn.story?.traits.HasTrait(TraitDefOf.Pyromaniac) ?? false;
    }


    [HarmonyPatch(typeof(MentalStateWorker))]
    [HarmonyPatch(nameof(MentalStateWorker.StateCanOccur))]
    public static class Patch_MentalStateWorker_StateCanOccur
    {
        public static void Postfix(MentalStateWorker __instance, Pawn pawn, ref bool __result)
        {
            if (__result && __instance.def == PyromaniacUtility.FireStartingSpreeDef
                && pawn.needs.TryGetNeed<NeedPyromania>() is { }  need
                && pawn.mindState.mentalBreaker.CurMood > need.GetMentalBreakProtectThreshold()
                )
            {
                __result = false;
                PyromaniacUtility.ThrowText(pawn, () => "CF_PyromaniacIsFun_PyromaniacUtility_PreventedFireStartingSpree".Translate(), 4);
            }
        }
    }

    [HarmonyPatch(typeof(Need_Mood))]
    [HarmonyPatch(nameof(Need_Mood.DrawOnGUI))]
    public static class Patch_Need_Mood_DrawOnGUI
    {
        public static float BarInstantMarkerSize = (float)typeof(Need).GetField("BarInstantMarkerSize", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        public static void DrawPyromaniaIndicator(Rect barRect, float pct)
        {
            // See `Need.DrawBarInstantMarkerAt`
            // TODO: Which const is 150f?
            var markerSize = BarInstantMarkerSize;
            if (barRect.width < 150f)
            {
                markerSize /= 2f;
            }
            var vector = new Vector2(barRect.x + barRect.width * pct, barRect.y + barRect.height);
            GUI.DrawTexture(new Rect(vector.x - markerSize / 2f, vector.y, markerSize, markerSize), TextureUtility.PyromaniaIndicator);
        }
        public static void Postfix(Rect rect, Pawn ___pawn, float customMargin)
        {
            if (___pawn.needs.TryGetNeed<NeedPyromania>() is { } need)
            {
                var threshold = need.GetMentalBreakProtectThreshold();

                // rect3 is derived from the original function
                var maxDrawHeight = Need.MaxDrawHeight;
                if (rect.height > maxDrawHeight)
                {
                    float num = (rect.height - maxDrawHeight) / 2f;
                    rect.height = maxDrawHeight;
                    rect.y += num;
                }
                // TODO: Which const is 14f, 15f, 50f?
                float num2 = 14f;
                float num3 = ((customMargin >= 0f) ? customMargin : (num2 + 15f));
                if (rect.height < 50f)
                {
                    num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
                }

                var rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
                rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);

                DrawPyromaniaIndicator(rect3, threshold);
            }
        }

    }
}