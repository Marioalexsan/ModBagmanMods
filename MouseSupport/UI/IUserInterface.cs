using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoG.LocalInputHelper;

namespace Marioalexsan.GrindeaMouseSupport.UI
{
    public interface IUserInterface
    {
        public Rectangle Bounds { get; }

        public Action<IUserInterface> OnClick { get; }
    }
}
