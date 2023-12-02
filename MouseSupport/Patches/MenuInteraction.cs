using ModBagman;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MenuLevel = SoG.GlobalData.MainMenu.MenuLevel;

namespace Marioalexsan.GrindeaMouseSupport.Patches
{
    [HarmonyPatch]
    internal static class MenuInteraction
    {
        private static Dictionary<MethodInfo, Func<UI.Menu>> Menus =>
            new MainMenuLogic().InterfaceMenuMap
            .Concat(new InGameMenuLogic().InterfaceMenuMap)
            .ToDictionary(x => x.Key, x => x.Value);

        // Spooky hacky methodd??!!1one
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            return Menus.Keys;
        }

        private static void Prefix(MethodBase __originalMethod)
        {
            if (!MouseInput.IsPressed(LocalInputHelper.MouseButton.Left_Mouse))
                return;

            var menu = Menus[__originalMethod as MethodInfo]();

            menu?.OnClick(MouseInput.WorldMouseGUIPos);
        }
    }
}
