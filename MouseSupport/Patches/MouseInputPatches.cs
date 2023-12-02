using ModBagman;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaMouseSupport.Patches
{
    [HarmonyPatch]
    internal static class MouseInputPatches
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.FinalDraw))]
        [HarmonyPostfix]
        private static void RenderMouse()
        {
            Globals.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, null);

            // Draw mouse from perspective of GUI for debug purposes

            var worldMouseGUIPos = MouseInput.WorldMouseGUIPos + new Vector2(1, -1);

            Globals.SpriteBatch.Draw(MouseSupportResources.CursorIcon, worldMouseGUIPos * Globals.Game.xOptions.iZoom, null, Color.Black, 0f, Vector2.Zero, Vector2.One * 2, SpriteEffects.None, 0f);

            // Draw mouse from perspective of world for debug purposes

            var worldMousePos = MouseInput.WorldMousePos;
            var mouseDrawPos = worldMousePos - CAS.Camera.v2TopLeft + new Vector2(-1, 1);

            Globals.SpriteBatch.Draw(MouseSupportResources.CursorIcon, mouseDrawPos * Globals.Game.xOptions.iZoom, null, Color.Black, 0f, Vector2.Zero, Vector2.One * 2, SpriteEffects.None, 0f);

            // Draw mouse from perspective of window / OS

            Globals.SpriteBatch.Draw(MouseSupportResources.CursorIcon, MouseInput.MousePos, null, Color.White, 0f, Vector2.Zero, Vector2.One * 2, SpriteEffects.None, 0f);

            Globals.SpriteBatch.End();
        }

        [HarmonyPatch(typeof(Game1), "Update")]
        [HarmonyPrefix]
        private static void GrabMousePosition()
        {
            MouseInput.AddNewFrame(Mouse.GetState());
        }

    }
}
