using ModBagman;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaMouseSupport.Patches
{
    [HarmonyPatch]
    internal static class TargetedSpellMouseInputPatches
    {
        private static float SupportChargeSpeed(SpellCharge charge)
            => 3f + charge.fChargeSpeedModifier;

        private static float MeleeChargeSpeed(SpellCharge charge)
            => 3f * charge.xOwnerView.xEntity.xBaseStats.fAttackSPDFactor;

        private static float MagicChargeSpeed(SpellCharge charge)
            => 3f * charge.xOwnerView.xEntity.xBaseStats.fCastSPDFactor;

        private static readonly Dictionary<MethodInfo, Func<SpellCharge, float>> TargetedSpellData = new Dictionary<MethodInfo, Func<SpellCharge, float>>()
        {
            [AccessTools.Method(typeof(DeathMarkSpellCharge), nameof(DeathMarkSpellCharge.Update))] = SupportChargeSpeed,
            [AccessTools.Method(typeof(BlinkSpellCharge), nameof(BlinkSpellCharge.Update))] = SupportChargeSpeed,
            [AccessTools.Method(typeof(StasisSpellCharge), nameof(StasisSpellCharge.Update))] = SupportChargeSpeed,
            [AccessTools.Method(typeof(MeteorSpellCharge), nameof(MeteorSpellCharge.Update))] = MagicChargeSpeed,
            [AccessTools.Method(typeof(EarthSpikeSpellCharge), nameof(EarthSpikeSpellCharge.Update))] = MagicChargeSpeed,
            [AccessTools.Method(typeof(ThrowSpellCharge), nameof(ThrowSpellCharge.Update))] = MeleeChargeSpeed,
        };

        private static IEnumerable<MethodInfo> TargetMethods() => TargetedSpellData.Keys;

        private static void Prefix(TargetableSpellCharge __instance, MethodBase __originalMethod)
        {
            if (!Targeter.ShouldApplyMouseTargeting(__instance))
                return;

            var chargeSpeed = TargetedSpellData[__originalMethod as MethodInfo](__instance);

            Targeter.ReplaceKeyboardMovementWithMouse(__instance, chargeSpeed);
        }
    }
}
