using ModBagman;
using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoG.SpellCodex;

namespace Marioalexsan.GrindeaMouseSupport
{
    public static class Targeter
    {
        public static bool IsCastedViaMouse(Equipment.QuickSlot spellSlot, LocalInputHelper input)
        {
            var player = Globals.Game.xLocalPlayer.xEquipment;

            var quickSlots = new List<Equipment.QuickSlot>
            {
                player.xQuickSlot1,
                player.xQuickSlot2,
                player.xQuickSlot3,
                player.xQuickSlot4,
                player.xQuickSlot5,
                player.xQuickSlot6,
                player.xQuickSlot7,
                player.xQuickSlot8,
                player.xQuickSlot9,
                player.xQuickSlot10,
            };

            var index = quickSlots.FindIndex(x => x == spellSlot);

            if (index == -1)
                return false;

            var gameButtons = new List<GameButton>
            {
                input.Spell01,
                input.Spell02,
                input.Spell03,
                input.Spell04,
                input.Spell05,
                input.Spell06,
                input.Spell07,
                input.Spell08,
                input.Spell09,
                input.Spell10,
            };

            return gameButtons[index].xModifiedKey.xMainKey.bIsMouseButton;
        }

        public static bool ShouldApplyMouseTargeting(SpellCharge charge)
        {
            if (charge == null)
                return false;

            if (!MouseSupportMod.LocalSpellSlot.TryGetTarget(out var slot))
                return false;

            if (!MouseSupportMod.Instance.UseAllKeys && !IsCastedViaMouse(slot, charge.xOwnerView.xInputHelper))
                return false;

            if (slot.enSpellType != charge.enBaseSpell)
                return false;

            if (charge.bInCast)
                return false;

            return true;
        }

        public static void ReplaceKeyboardMovementWithMouse(TargetableSpellCharge charge, float targetSpeed)
        {
            ref var targetPos = ref charge.xTargetEffect.xTransform.v2Pos;
            var playerInput = charge.xOwnerView.xEntity.xInput;

            if (playerInput.bMoveRight)
            {
                targetPos.X -= targetSpeed;
            }
            if (playerInput.bMoveLeft)
            {
                targetPos.X += targetSpeed;
            }
            if (playerInput.bMoveUp)
            {
                targetPos.Y += targetSpeed;
            }
            if (playerInput.bMoveDown)
            {
                targetPos.Y -= targetSpeed;
            }

            // Apply movement while targeting

            if (MouseSupportMod.Instance.MoveWhileTargeting)
                charge.bChangeDirectionAllowed = true;

            // Apply cursor based movement

            Vector2 worldMouse = MouseInput.WorldMousePos;
            Vector2 distance = worldMouse - targetPos;

            Vector2 raycastedPos = Globals.Game._CollisionMaster_RayCastVsStatic_ReturnPositionOfLastNonCollision(targetPos, worldMouse, 2, Math.Min((int)(distance.Length() / 5), 3));

            if (MouseSupportMod.Instance.InstantTargeting)
                targetPos = raycastedPos;

            Vector2 movement = distance.Length() != 0 ? Vector2.Normalize(distance) * Math.Min(targetSpeed * 2, distance.Length()) : Vector2.Zero;

            targetPos += movement;
        }
    }
}
