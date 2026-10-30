using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GiantCompatibility;

public static class MedicalDismount
{
    // Only the medical-rest decision is configurable; emergency reasons remain original.
    public static bool Serious(bool enabled, bool otherMedicalReason, float health, float bleeding, float pain,
        float minimumHealth = .8f, float maximumBleeding = .05f, float maximumPain = .3f) =>
        !enabled || otherMedicalReason || !float.IsFinite(health) || !float.IsFinite(bleeding) || !float.IsFinite(pain) ||
        health <= MountSettings.Limit(minimumHealth, .8f, .1f, 1) ||
        bleeding > MountSettings.Limit(maximumBleeding, .05f, 0, 1) ||
        pain >= MountSettings.Limit(maximumPain, .3f, .05f, .8f);

    public static bool ShouldRestDismount(Pawn pawn)
    {
        if (pawn == null) return true;
        if (!HealthAIUtility.ShouldSeekMedicalRest(pawn)) return false;
        if (MountMod.Settings?.mugirlMinorInjuryFix != true || pawn.health?.hediffSet == null) return true;
        var set = pawn.health.hediffSet;
        bool other = pawn.Dead || pawn.Downed || set.InLabor() || set.HasImmunizableNotImmuneHediff() ||
            HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn) || !set.hediffs.Any(h => h is Hediff_Injury) ||
            set.hediffs.Any(h => h is not Hediff_Injury && h.TendableNow());
        var settings = MountMod.Settings;
        return Serious(true, other, pawn.health.summaryHealth.SummaryHealthPercent, set.BleedRateTotal, set.PainTotal,
            settings.mugirlMinimumHealth, settings.mugirlMaximumBleeding, settings.mugirlMaximumPain);
    }
    public static MethodInfo Target()
    {
        Type type = AccessTools.TypeByName("Mugirl.Features.Mounting.MountEligibilityUtility");
        return type == null ? null : AccessTools.DeclaredMethod(type, "ShouldAutoDismount",
            new[] { typeof(Pawn), typeof(Pawn), typeof(string).MakeByRefType() });
    }
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var code = instructions.ToList();
        MethodInfo original = AccessTools.Method(typeof(HealthAIUtility), nameof(HealthAIUtility.ShouldSeekMedicalRest), new[] { typeof(Pawn) });
        var calls = code.Where(c => c.Calls(original)).ToArray();
        if (calls.Length != 1) throw new InvalidOperationException("Unsupported Mugirl automatic dismount medical check.");
        calls[0].operand = AccessTools.Method(typeof(MedicalDismount), nameof(ShouldRestDismount));
        return code;
    }
}

[StaticConstructorOnStartup]
public static class MugirlMountInstaller
{
    static MugirlMountInstaller()
    {
        try
        {
            MethodInfo target = MedicalDismount.Target();
            if (target == null)
            {
                if (ModsConfig.IsActive("HAR.MugirlRace"))
                    Log.Warning("[Mugirl Mount Fix] Mugirl automatic dismount API not found; medical correction not installed.");
                return;
            }
            new Harmony("w0bderful.giantmeals.mugirl").Patch(target,
                transpiler: new HarmonyMethod(typeof(MedicalDismount), nameof(MedicalDismount.Transpiler)));
            Log.Message("[Mugirl Mount Fix] Mugirl minor-injury dismount correction installed; manual/emergency dismount preserved.");
        }
        catch (Exception e) { Log.Error("[Mugirl Mount Fix] Cannot install optional Mugirl dismount correction: " + e); }
    }
}

public sealed class MountSettings : ModSettings
{
    public bool mugirlMinorInjuryFix = true;
    public float mugirlMinimumHealth = .8f, mugirlMaximumBleeding = .05f, mugirlMaximumPain = .3f;
    public static float Limit(float v, float fallback, float min, float max) => Math.Clamp(float.IsFinite(v) ? v : fallback, min, max);
    public void Validate()
    {
        mugirlMinimumHealth = Limit(mugirlMinimumHealth, .8f, .1f, 1);
        mugirlMaximumBleeding = Limit(mugirlMaximumBleeding, .05f, 0, 1);
        mugirlMaximumPain = Limit(mugirlMaximumPain, .3f, .05f, .8f);
    }
    public override void ExposeData()
    {
        Scribe_Values.Look(ref mugirlMinorInjuryFix, "mugirlMinorInjuryFix", true);
        Scribe_Values.Look(ref mugirlMinimumHealth, "mugirlMinimumHealth", .8f);
        Scribe_Values.Look(ref mugirlMaximumBleeding, "mugirlMaximumBleeding", .05f);
        Scribe_Values.Look(ref mugirlMaximumPain, "mugirlMaximumPain", .3f);
        Validate();
    }
}
public sealed class MountMod : Mod
{
    public static MountSettings Settings;
    public MountMod(ModContentPack content) : base(content) => Settings = GetSettings<MountSettings>();
    public override string SettingsCategory() => "Mugirl Mount Fix";
    public override void WriteSettings() { Settings.Validate(); base.WriteSettings(); }
    public override void DoSettingsWindowContents(UnityEngine.Rect rect)
    {
        var list = new Listing_Standard(); list.Begin(rect);
        list.CheckboxLabeled("GM_MugirlFix".Translate(), ref Settings.mugirlMinorInjuryFix);
        list.Label("GT_Health".Translate() + " " + Settings.mugirlMinimumHealth.ToString("P0"));
        Settings.mugirlMinimumHealth = list.Slider(Settings.mugirlMinimumHealth, .1f, 1);
        list.Label("GT_Bleeding".Translate() + " " + Settings.mugirlMaximumBleeding.ToString("P0"));
        Settings.mugirlMaximumBleeding = list.Slider(Settings.mugirlMaximumBleeding, 0, 1);
        list.Label("GT_Pain".Translate() + " " + Settings.mugirlMaximumPain.ToString("P0"));
        Settings.mugirlMaximumPain = list.Slider(Settings.mugirlMaximumPain, .05f, .8f);
        list.Label("GM_MugirlFixHelp".Translate());
        if (list.ButtonText("GT_ResetTab".Translate()))
        { Settings.mugirlMinorInjuryFix = true; Settings.mugirlMinimumHealth = .8f; Settings.mugirlMaximumBleeding = .05f; Settings.mugirlMaximumPain = .3f; }
        list.End();
    }
}
