using ModBagman;
using HarmonyLib;
using Marioalexsan.GrindeaMouseSupport.UI;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaMouseSupport.Patches
{
    public class InGameMenuLogic
    {
        public readonly Dictionary<MethodInfo, Func<Menu>> RenderMenuMap;

        public readonly Dictionary<MethodInfo, Func<Menu>> InterfaceMenuMap;

        public InGameMenuLogic()
        {
            RenderMenuMap = new Dictionary<MethodInfo, Func<Menu>>
            {
                [AccessTools.Method(typeof(Game1), nameof(Game1._InGameMenu_RenderTopStuff))] = AssembleInGameMenu,
            };

            InterfaceMenuMap = new Dictionary<MethodInfo, Func<Menu>>
            {
                [AccessTools.Method(typeof(Game1), nameof(Game1._InGameMenu_Input))] = AssembleInGameMenu,
            };
        }

        public static void SimulateActionButtonPress(LocalInputHelper input)
        {
            input.Action.bPressed = true;

            input.Up.bPressed = false;
            input.Down.bPressed = false;
            input.Left.bPressed = false;
            input.Right.bPressed = false;
        }

        public Menu AssembleInGameMenu()
        {
            var menu = AddTopMenuButtons(new Menu());

            return menu;
        }

        public Menu AddTopMenuButtons(Menu target)
        {
            bool isArcade = CAS.GameMode == StateMaster.GameModes.RogueLike;
            bool thirdSlotMissing = isArcade && !CAS.WorldRogueLikeData.henActiveFlags.Contains(FlagCodex.FlagID._Roguelike_PinsUnlocked);

            var width = 34;
            var height = 28;
            var xSpacing = 52;

            int pinShift = thirdSlotMissing ? xSpacing + 7 : 0;
            var xStart = 232 - width / 2;
            var yStart = 38 - width / 2;

            if (!thirdSlotMissing)
                target.Elements.Add(new Button(width, height, xStart + 3 * xSpacing, yStart,
                    (_) => ClickTopMenu(isArcade ? InGameMenu.BaseView.Pins : InGameMenu.BaseView.Crafting)));

            target.Elements.AddRange(new IUserInterface[]
            {
                new Button(width, height, xStart + 0 * xSpacing, yStart, (_) => ClickTopMenu(InGameMenu.BaseView.Character)),
                new Button(width, height, xStart + 1 * xSpacing, yStart, (_) => ClickTopMenu(InGameMenu.BaseView.Equip)),
                new Button(width, height, xStart + 2 * xSpacing, yStart, (_) => ClickTopMenu(InGameMenu.BaseView.Inventory)),
                // Here is Crafting or Pins or empty space
                new Button(width, height, xStart + 4 * xSpacing - pinShift, yStart, (_) => ClickTopMenu(InGameMenu.BaseView.Skills)),
                new Button(width, height, xStart + 5 * xSpacing - pinShift, yStart, (_) => ClickTopMenu(InGameMenu.BaseView.Journal)),
                new Button(width, height, xStart + 6 * xSpacing - pinShift, yStart, (_) => ClickTopMenu(InGameMenu.BaseView.Map)),
                new Button(width, height, xStart + 7 * xSpacing - pinShift, yStart, (_) => ClickTopMenu(InGameMenu.BaseView.System))
            });

            return target;
        }

        public InGameMenu MenuData => Globals.Game.xInGameMenu;
        public LocalInputHelper MenuInput => Globals.Game.xInput_Menu;

        public void ClickTopMenu(InGameMenu.BaseView selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.enCurrentView = InGameMenu.BaseView.Main;
            MenuData.xTopView.enSelectedMainView = selection;
        }
    }
}
