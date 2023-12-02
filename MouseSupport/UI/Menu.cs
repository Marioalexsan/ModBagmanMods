using ModBagman;
using Marioalexsan.GrindeaMouseSupport.Patches;
using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static SoG.LocalInputHelper;

namespace Marioalexsan.GrindeaMouseSupport.UI
{
    public class Menu
    {
        public Menu() { }

        public Menu(IEnumerable<IUserInterface> elements)
        {
            Elements = new List<IUserInterface>(elements);
        }

        public List<IUserInterface> Elements { get; } = new List<IUserInterface>();

        public void OnClick(Vector2 position)
        {
            foreach (var element in Elements)
            {
                if (element.Bounds.Contains((int)position.X, (int)position.Y))
                {
                    element.OnClick(element);
                    break;
                }
            }
        }

        public void DebugRender()
        {
            foreach (var element in Elements)
            {
                if (element.OnClick != null)
                {
                    Globals.SpriteBatch.Draw(RenderMaster.txNoTex, element.Bounds, Color.Yellow * 0.15f);
                }
            }
        }
    }
}
