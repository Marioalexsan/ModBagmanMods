using ModBagman;
using Marioalexsan.GrindeaMouseSupport.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaMouseSupport
{
    public static class MenuHelper
    {
        public static Rectangle HitboxFrom(LibraryTextAsTexture texture, float x, float y)
        {
            return HitboxFrom(texture.GetRenderTexture(), x, y);
        }

        public static Rectangle HitboxFrom(Texture2D texture, float x, float y)
        {
            var rect = texture.Bounds;

            return new Rectangle(
                (int)(x - rect.Width / 2),
                (int)(y - rect.Height / 2),
                rect.Width,
                rect.Height);
        }

        public static int? GetSelection(Dictionary<int, Rectangle> hitboxes, float x, float y)
        {
            foreach (var kvp in hitboxes)
            {
                if (kvp.Value.Contains((int)x, (int)y))
                    return kvp.Key;
            }

            return null;
        }

        public static void RenderHitboxes(IEnumerable<Rectangle> hitboxes)
        {
            foreach (var hitbox in hitboxes)
            {
                Globals.SpriteBatch.Draw(RenderMaster.txNoTex, hitbox, Color.Yellow * 0.2f);
            }
        }

        public static Menu AddGridButtons(Menu target,
            int cols, int rows, int xCenter, int yCenter,
            int xSpacing, int ySpacing, int width, int height,
            Func<int, int, Action<IUserInterface>> clickAction
            )
        {
            var elements = new List<IUserInterface>();

            int xStart = (int)(xCenter - Math.Max(0, (cols - 1) / 2f) * xSpacing - cols / 2f * width);
            int yStart = (int)(yCenter - Math.Max(0, (rows - 1) / 2f) * ySpacing - rows / 2f * height);


            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var rect = new Rectangle(
                        xStart + x * (width + xSpacing),
                        yStart + y * (height + ySpacing),
                        width,
                        height
                        );
                    elements.Add(new Button(rect, clickAction(x, y)));
                }
            }

            target.Elements.AddRange(elements);

            return target;
        }
    }
}
