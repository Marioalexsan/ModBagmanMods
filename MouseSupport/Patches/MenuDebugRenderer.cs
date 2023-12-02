using HarmonyLib;
using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaMouseSupport.Patches
{
    [HarmonyPatch]
    internal static class MenuDebugRenderer
    {
        private static Dictionary<MethodInfo, Func<UI.Menu>> Menus =>
            new MainMenuLogic().RenderMenuMap
            .Concat(new InGameMenuLogic().RenderMenuMap)
            .ToDictionary(x => x.Key, x => x.Value);

        // Spooky hacky methodd??!!1one
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            return Menus.Keys;
        }

        private static void Postfix(MethodBase __originalMethod)
        {
            var menu = Menus[__originalMethod as MethodInfo]();
            menu?.DebugRender();
        }
    }
}
