using HarmonyLib;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaQoL.Patches
{
    [HarmonyPatch]
    internal static class NegativeDefenseMultiplication
    {
        [HarmonyPatch(typeof(BaseStats), nameof(BaseStats.iDefense), MethodType.Getter)]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryHigh)]
        private static void MitigateNegativeDefense(BaseStats __instance, ref int __result)
        {
            if (__instance.iBaseDEF < 0)
            {
                // Modify output defense to be reduced by defense multipliers instead of being amplified
                __result = (int)(__instance.iBaseDEF / __instance.fBaseDEFMultiplier);
            }
        }
    }
}
