using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoG.LocalInputHelper;

namespace Marioalexsan.GrindeaMouseSupport.UI
{
    public class Button : IUserInterface
    {
        public Button(LibraryTextAsTexture visuals, float x, float y, Action<IUserInterface> onClick)
            : this(MenuHelper.HitboxFrom(visuals, x, y), onClick) { }

        public Button(float width, float height, float x, float y, Action<IUserInterface> onClick)
            : this(new Rectangle((int)x, (int)y, (int)width, (int)height), onClick) { }

        public Button(Rectangle bounds, Action<IUserInterface> onClick)
        {
            Bounds = bounds;
            OnClick = onClick;
        }

        public Rectangle Bounds { get; set; }

        public Action<IUserInterface> OnClick { get; set; }
    }
}
