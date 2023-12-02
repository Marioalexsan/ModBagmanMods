using HarmonyLib;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaMouseSupport.Patches
{
    [HarmonyPatch]
    internal class MenuPatches
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1._Menu_CharacterCreation_Interface))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> FixBrokenColorSelection(IEnumerable<CodeInstruction> code)
        {
            // _Menu_CharacterCreation_Interface sets xGlobalData.xMainMenuData.enCharCreationLevel
            // to CharCreationLevel.Top before using the same value for switching colors

            // This bug isn't noticeable with Keyboard controls because moving also sets the colors correctly, but it breaks mouse controls.
            // For mouse controls to work, we need to move the assignment at the end of the block.

            return new CodeMatcher(code).MatchStartForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Game1), nameof(Game1.xGlobalData))),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GlobalData), nameof(GlobalData.xMainMenuData))),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(GlobalData.MainMenu), nameof(GlobalData.MainMenu.enCharCreationLevel))),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Game1), nameof(Game1.xGlobalData))),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GlobalData), nameof(GlobalData.xMainMenuData))),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GlobalData.MainMenu), nameof(GlobalData.MainMenu.enCharCreationLevel))),
                    new CodeMatch(OpCodes.Ldc_I4_1)
                )
                .RemoveInstructions(5)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Game1), nameof(Game1.xGlobalData))),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GlobalData), nameof(GlobalData.xMainMenuData))),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GlobalData.MainMenu), nameof(GlobalData.MainMenu.xCreationDisplay))),
                    new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(PlayerPalette), nameof(PlayerPalette.denHairPalettes)))
                )
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Game1), nameof(Game1.xGlobalData))),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GlobalData), nameof(GlobalData.xMainMenuData))),
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(GlobalData.MainMenu), nameof(GlobalData.MainMenu.enCharCreationLevel)))
                )
                .InstructionEnumeration();
        }
    }
}
