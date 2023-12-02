using ModBagman;
using HarmonyLib;
using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static SoG.HitEffectMap;

namespace Marioalexsan.GrindeaMouseSupport.Patches
{
    [HarmonyPatch]
    internal static class SpellPatches
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.__Input_ActivateSpellslot))]
        [HarmonyPostfix]
        private static void SpellCharging(bool __result, Equipment.SpellSlot xSpellSlot)
        {
            if (!__result)
                return;  // Spell charge didn't start

            MouseSupportMod.LocalSpellSlot = new WeakReference<Equipment.SpellSlot>(xSpellSlot);
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1.__Input_ActivateItemslot))]
        [HarmonyPrefix]
        private static void ItemActivated(Equipment.ItemSlot xItemSlot)
        {
            MouseSupportMod.LocalItemSlotActivation = new WeakReference<Equipment.ItemSlot>(xItemSlot);
        }

        private static ushort MapToDirection(Vector2 vector)
        {
            const double Eighth = Math.PI / 4;

            return (double)Utility.Vector2ToRadiansReverse(vector) switch
            {
                >= Eighth and < 3 * Eighth => 2,
                >= -Eighth and < Eighth => 1,
                >= -3 * Eighth and < -Eighth => 0,
                < -3 * Eighth or >= 3 * Eighth => 3,
                _ => 0
            };
        }

        [HarmonyPatch(typeof(SpellCharge), nameof(SpellCharge.ChangeDirection))]
        [HarmonyPrefix]
        private static void UseMouseDirectionsInstead(SpellCharge __instance, ref byte byMas)
        {
            var entity = __instance.xOwnerView.xEntity;
            byte direction = (byte)MapToDirection(MouseInput.WorldMousePos - entity.xTransform.v2Pos);

            if (!(__instance.bChangeDirectionAllowed || __instance.bStartupOver || __instance.xOwnerView != null))
                return;

            if (__instance is TargetableSpellCharge)
                return;

            if (!Targeter.ShouldApplyMouseTargeting(__instance))
                return;

            byMas = direction;
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1._Input_AttackButtonPressed))]
        [HarmonyPrefix]
        private static void UseMouseDirectionsInsteadForAttacking(PlayerView xThisView)
        {
            byte direction = (byte)MapToDirection(MouseInput.WorldMousePos - xThisView.xEntity.xTransform.v2Pos);

            if (!Globals.Game.xInput_Game.Action.xModifiedKey.xMainKey.bIsMouseButton)
                return;

            if (xThisView.xEntity.bTwoHandDisabled && xThisView.xEquipment.xWeapon.enWeaponCategory == WeaponInfo.WeaponCategory.TwoHanded)
                return;

            xThisView.xEntity.byAnimationDirection = direction;
            xThisView.xEntity.xInput.bMoveUp = direction == 0;
            xThisView.xEntity.xInput.bMoveRight = direction == 1;
            xThisView.xEntity.xInput.bMoveDown = direction == 2;
            xThisView.xEntity.xInput.bMoveLeft = direction == 3;
        }

        private static void EditShieldDirection(PlayerView xThisView)
        {
            byte direction = (byte)MapToDirection(MouseInput.WorldMousePos - xThisView.xEntity.xTransform.v2Pos);

            if (!Globals.Game.xInput_Game.Shield.xModifiedKey.xMainKey.bIsMouseButton)
                return;

            xThisView.xEntity.byAnimationDirection = direction;
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1._IngameInput_HandleInput))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> EditShieldDirectionStuff(IEnumerable<CodeInstruction> code)
        {
            var matcher = new CodeMatcher(code);

            matcher.MatchStartForward(
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerEntity), nameof(PlayerEntity.xRenderComponent))),
                    new CodeMatch(OpCodes.Ldc_I4_S),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerEntity), nameof(PlayerEntity.byAnimationDirection))),
                    new CodeMatch(OpCodes.Add)
                    );

            var labels = matcher.Instruction.ExtractLabels();

            matcher
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc, 0),
                    CodeInstruction.Call(() => EditShieldDirection(null)))
                .Instruction.WithLabels(labels);

            matcher.Advance(10);

            matcher
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerEntity), nameof(PlayerEntity.xRenderComponent))),
                    new CodeMatch(OpCodes.Ldc_I4_S),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerEntity), nameof(PlayerEntity.byAnimationDirection))),
                    new CodeMatch(OpCodes.Add)
                    );

            labels = matcher.Instruction.ExtractLabels();

            matcher
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc, 0),
                    CodeInstruction.Call(() => EditShieldDirection(null)))
                .Instruction.WithLabels(labels);

            return matcher.InstructionEnumeration();
        }

        private static byte SavedState = 0;

        [HarmonyPatch(typeof(Game1), nameof(Game1._IngameInput_AnimationHandling))]
        [HarmonyPostfix]
        private static void RestoreInputDirection(PlayerView xPlayerView)
        {
            xPlayerView.xEntity.byInputDirection = SavedState;
        }

        private static void SaveAndEditInputDirection(PlayerEntity entity)
        {
            SavedState = entity.byInputDirection;

            bool reorient = Targeter.ShouldApplyMouseTargeting(entity.xCurrentSpellCharge)
                || entity.xRenderComponent.iActiveAnimation >= 1900 && entity.xRenderComponent.iActiveAnimation <= 1903;

            entity.byInputDirection = !reorient ? entity.byInputDirection : (byte)MapToDirection(MouseInput.WorldMousePos - entity.xTransform.v2Pos);
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1._IngameInput_AnimationHandling))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> EditInputDirection(IEnumerable<CodeInstruction> code)
        {
            var matcher = new CodeMatcher(code)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerEntity), nameof(PlayerEntity.bMoveCancelable))),
                    new CodeMatch(OpCodes.Brfalse)
                );

            var labels = matcher.Instruction.ExtractLabels();

            matcher
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_0),
                    CodeInstruction.Call(() => SaveAndEditInputDirection(null))
                )
                .Instruction.WithLabels(labels);

            return matcher.InstructionEnumeration();
        }

        //private static bool UseBowMouseTargetingRightNow = false;

        //[HarmonyPatch(typeof(Game1), nameof(Game1._Item_Use), typeof(ItemCodex.ItemTypes), typeof(PlayerView), typeof(bool))]
        //[HarmonyPrefix]
        //private static void UseBowMouseTargeting(ItemCodex.ItemTypes enItem, PlayerView xView, bool bSend)
        //{
        //    ItemDescription xDesc = ItemCodex.GetItemDescription(enItem);

        //    bool doTheThing = (xView.xEntity.bSpellCancelable || xView.xEntity.bAttackCancelable)
        //        && xView.xEntity.iBowCooldown <= 0
        //        && xView.xInventory.GetAmount(ItemCodex.ItemTypes._Bow_Arrows) > 0
        //        && xDesc.lenCategory.Contains(ItemCodex.ItemCategories.Bow);

        //    if (doTheThing)
        //    {
        //        if (!MouseSupportMod.LocalItemSlotActivation.TryGetTarget(out var slot))
        //            return;

        //        if (!Targeter.IsCastedViaMouse(slot, xView.xInputHelper))
        //            return;

        //        byte direction = (byte)MapToDirection(MouseInput.WorldMousePos - xView.xEntity.xTransform.v2Pos);

        //        xView.xEntity.byAnimationDirection = direction;

        //        UseBowMouseTargetingRightNow = true;
        //    }
        //    else
        //    {
        //        UseBowMouseTargetingRightNow = false;
        //    }
        //}

        //[HarmonyPatch(typeof(PlayerEntity), nameof(PlayerEntity.Update))]
        //[HarmonyPrefix]
        //private static void EnforceBowDirection(PlayerEntity __instance)
        //{
        //    bool isInBow = Utility.IsWithinRange(__instance.xRenderComponent.iActiveAnimation, 300, 303)
        //        || Utility.IsWithinRange(__instance.xRenderComponent.iActiveAnimation, 320, 339);

        //    if (!UseBowMouseTargetingRightNow || )
        //        return;

        //    if (!slot.xDescription.lenCategory.Contains(ItemCodex.ItemCategories.Bow))
        //        return;

        //    if (!Targeter.IsCastedViaMouse(slot, __instance.Owner.xInputHelper))
        //        return;

        //    byte direction = (byte)MapToDirection(MouseInput.WorldMousePos - __instance.xTransform.v2Pos);

        //    __instance.byAnimationDirection = direction;
        //}
    }
}
