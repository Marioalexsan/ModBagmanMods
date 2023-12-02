using ModBagman;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Marioalexsan.GrindeaMouseSupport
{
    public static class MouseSupportResources
    {
        public static void ReloadResources()
        {
            CursorIcon?.Dispose();
            CursorIcon = null;

            using (MemoryStream stream = new MemoryStream(Resources.Resource.WoodenSword))
            {
                CursorIcon = Texture2D.FromStream(Globals.Game.GraphicsDevice, stream)
                    ?? throw new InvalidOperationException("Failed to load a resource.");
            }
        }

        public static Texture2D CursorIcon { get; private set; }
    }
}