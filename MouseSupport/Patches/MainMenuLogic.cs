using ModBagman;
using HarmonyLib;
using Marioalexsan.GrindeaMouseSupport.UI;
using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static SoG.GlobalData.MainMenu;

namespace Marioalexsan.GrindeaMouseSupport.Patches
{
    internal class MainMenuLogic
    {
        public MainMenuLogic()
        {
            RenderMenuMap = new Dictionary<MethodInfo, Func<Menu>>
            {
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_CharacterSelect_Render))] = GetCharacterSelectMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Render_TopMenu))] = GetMainMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Render_StoryModeRender))] = () => StoryModeMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Render_ArcadeModeRender))] = () => ArcadeModeMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Render_ArcadeModeCreateOrRandomRender))] = () => ArcadeModeCreateOrRandomMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Render_HighScoreRender))] = () => null,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_CharacterCreation_Render))] = GetCharacterCreationMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_EnterName_Render))] = () => EnterNameMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_MultiplayerRoot_Render))] = GetMultiplayerTopMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_MultiplayerJoinSteamGame_Render))] = () => null,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_LanguageBrowser_Render))] = () => null,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_MultiplayerClientEnterIP_Render))] = () => null,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_MultiplayerLobby_Render))] = GetLobbyMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_OptionsTop_Render))] = () => OptionsMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_SelectDifficulty_Render))] = () => null,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Render_PatchNotesRender))] = () => null,
            };

            InterfaceMenuMap = new Dictionary<MethodInfo, Func<Menu>>
            {
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_CharacterSelect_Interface))] = GetCharacterSelectMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_TopMenu_Interface))] = GetMainMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_SelectSingleOrMultiInterface))] = () =>
                {
                    return Globals.Game.xStateMaster.enGameMode == StateMaster.GameModes.Story ? StoryModeMenu : ArcadeModeMenu;
                },
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_SelectCreateOrRandomize))] = () => ArcadeModeCreateOrRandomMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Highscore_Interface))] = () => null,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_CharacterCreation_Interface))] = GetCharacterCreationMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_EnterName_Interface))] = () => EnterNameMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Multiplayer_Interface))] = GetMultiplayerTopMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Multiplayer_JoinSteamGameInterface))] = () => null,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_LanguageBrowser_Interface))] = () => null,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Multiplayer_ClientEnterIP))] = () => null,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Lobby_Interface))] = GetLobbyMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_Options_Top))] = () => OptionsMenu,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_SelectDifficulty))] = () => null,
                [AccessTools.Method(typeof(Game1), nameof(Game1._Menu_PatchNotes))] = () => null,
            };
        }

        public Menu GetCharacterCreationMenu() => MenuData.enCharCreationLevel switch
        {
            CharCreationLevel.Top => CharacterCreationMenu,
            CharCreationLevel.Sweater => GetColorMenu(),
            CharCreationLevel.HairStyle => GetHairMenu(),
            CharCreationLevel.HairColor => GetColorMenu(),
            CharCreationLevel.Pants => GetColorMenu(),
            CharCreationLevel.Poncho => GetColorMenu(),
            CharCreationLevel.Skin => GetSkinColorMenu(),
            _ => null
        };

        public Menu GetMainMenu()
        {
            if (MenuData.iVoteForTranslationPopup > 0)
                return null;

            if (MenuData.iTranslationExistsNoticePopup > 0)
                return null;

            if (MenuData.iSaveRecoveredPopup > 0)
                return null;

            return MainMenu;
        }

        public Menu GetMultiplayerTopMenu() => MenuData.enMultiplayerLevel switch
        {
            MultiplayerLevel.Top => MultiplayerTopMenu,
            MultiplayerLevel.HostEnterPass => HostEnterPassMenu,
            MultiplayerLevel.HostCharSel => null,
            _ => null
        };

        public Menu GetLobbyMenu()
        {
            if (MenuData.iInviteScreenAppearOrDisappear != 0 || MenuData.bInviteScreenUp)
                return null;

            return MultiplayerLobbyMenu;
        }

        private Menu GetHairMenu()
        {
            var menu = new Menu();

            MenuHelper.AddGridButtons(menu, 4, 3, 320 + 2, 180 - 16, 10, 6, 32, 24, (x, y) => (_) => ClickCharacterCreationPick(y * 4 + x));

            menu.Elements.Add(new Button(Textures.txMainMenu_Back, 320, 295, (_) => ClickCharacterCreationPick(24)));

            return menu;
        }

        private Menu GetColorMenu()
        {
            var menu = new Menu();

            MenuHelper.AddGridButtons(menu, 6, 5, 320 + 1, 180 + 14, 6, 12, 21, 21, (x, y) => (_) => ClickCharacterCreationPick(y * 6 + x));

            menu.Elements.Add(new Button(Textures.txMainMenu_Back, 320, 295, (_) => ClickCharacterCreationPick(30)));

            return menu;
        }

        private Menu GetSkinColorMenu()
        {
            var menu = new Menu();

            MenuHelper.AddGridButtons(menu, 1, 6, 320 + 2, 180 + 6, 0, 6, 156, 19, (x, y) => (_) => ClickCharacterCreationPick(y));

            menu.Elements.Add(new Button(Textures.txMainMenu_Back, 320, 295, (_) => ClickCharacterCreationPick(6)));

            return menu;
        }

        private Menu GetCharacterSelectMenu()
        {
            var width = 209;
            var height = 79;
            var ySpacing = 90;

            var xStart = 320 - width / 2;
            var yStart = 66 - height / 2;

            var menu = new Menu(new IUserInterface[]
            {
                new Button(width, height, xStart, yStart + 0 * ySpacing, (_) => ClickCharacterSelectMenu(0, null)),
                new Button(width, height, xStart, yStart + 1 * ySpacing, (_) => ClickCharacterSelectMenu(1, null)),
                new Button(width, height, xStart, yStart + 2 * ySpacing, (_) => ClickCharacterSelectMenu(2, null)),
                new Button(Textures.txMainMenu_Back, 320 - Textures.txMainMenu_Back.GetRenderTexture().Width / 2 - 1, 311, (_) => ClickCharacterSelectMenu(3, null)),
                new Button(Textures.txMainMenu_Delete, 320 + Textures.txMainMenu_Delete.GetRenderTexture().Width / 2 + 1, 311, (_) => ClickCharacterSelectMenu(4, null)),
            });

            // Go to left button
            if (MenuData.iCurrentCharSelectPage > 0)
            {
                menu.Elements.Add(new Button(160, 320, 0, 0, (_) => ClickCharacterSelectMenu(null, MenuData.iCurrentCharSelectPage - 1)));
            }

            // Go to right button
            if (MenuData.iCurrentCharSelectPage < 2)
            {
                menu.Elements.Add(new Button(160, 320, 480, 0, (_) => ClickCharacterSelectMenu(null, MenuData.iCurrentCharSelectPage + 1)));
            }

            return menu;
        }

        public readonly Menu MultiplayerLobbyMenu = new Menu(new IUserInterface[]
        {
            new Button(Textures.txMainMenu_Invite, 95, 219, (_) => ClickMultiplayerLobbyMenu(6)),
            new Button(Textures.txMainMenu_StartGame, 95, 267, (_) => ClickMultiplayerLobbyMenu(2)),
            new Button(Textures.txMainMenu_Back, 95, 329, (_) => ClickMultiplayerLobbyMenu(3)),
        });

        public readonly Menu HostEnterPassMenu = new Menu(new IUserInterface[]
        { 
            new Button(Textures.txMainMenu_Start, 320 - Textures.txMainMenu_Start.GetRenderTexture().Width / 2 - 12, 288, (_) => ClickHostGameMenu(1)),
            new Button(Textures.txMainMenu_Back, 320 + Textures.txMainMenu_Back.GetRenderTexture().Width / 2 + 12, 288, (_) => ClickHostGameMenu(2)),
        });

        public readonly Menu MultiplayerTopMenu = new Menu(new IUserInterface[]
        {
            new Button(Textures.txMainMenu_Multiplayer_HostGame, 320, 224, (_) => ClickMultiplayerTopMenu(0)),
            new Button(Textures.txMainMenu_Multiplayer_JoinGame, 320, 249, (_) => ClickMultiplayerTopMenu(1)),
            new Button(Textures.txMainMenu_Back, 320, 288, (_) => ClickMultiplayerTopMenu(2))
        });

        public readonly Menu MainMenu = new Menu(new IUserInterface[]
        {
            new Button(Textures.txMainMenu_StoryMode, 320, 219, (_) => ClickMainMenu(0)),
            new Button(Textures.txMainMenu_ArcadeMode, 320, 245, (_) => ClickMainMenu(1)),
            new Button(Textures.txMainMenu_Options, 320, 271, (_) => ClickMainMenu(2)),
            new Button(Textures.txMainMenu_Quit, 320, 297, (_) => ClickMainMenu(3)),
        });

        public readonly Menu CharacterCreationMenu = new Menu(new IUserInterface[]
        {
            new Button(Textures.txMainMenu_CharacterCreation_Randomize, 320, 95, (_) => ClickCharacterCreationMenu(0)),
            new Button(Textures.txMainMenu_CharacterCreation_SwitchGender, 320, 119, (_) => ClickCharacterCreationMenu(1)),
            new Button(Textures.txMainMenu_CharacterCreation_Hairstyle, 320, 145, (_) => ClickCharacterCreationMenu(2)),
            new Button(Textures.txMainMenu_CharacterCreation_HairColor, 320, 167, (_) => ClickCharacterCreationMenu(3)),
            new Button(Textures.txMainMenu_CharacterCreation_SkinColor, 320, 191, (_) => ClickCharacterCreationMenu(4)),
            new Button(Textures.txMainMenu_CharacterCreation_Scarf, 320, 215, (_) => ClickCharacterCreationMenu(5)),
            new Button(Textures.txMainMenu_CharacterCreation_Sweater, 320, 240, (_) => ClickCharacterCreationMenu(6)),
            new Button(Textures.txMainMenu_CharacterCreation_Pants, 320, 264, (_) => ClickCharacterCreationMenu(7)),
            new Button(Textures.txMainMenu_Start, 320 - Textures.txMainMenu_Start.GetRenderTexture().Width / 2 - 10, 297, (_) => ClickCharacterCreationMenu(8)),
            new Button(Textures.txMainMenu_Back, 320 + Textures.txMainMenu_Back.GetRenderTexture().Width / 2 + 10, 297, (_) => ClickCharacterCreationMenu(9)),
        });

        public readonly Menu EnterNameMenu = new Menu(new IUserInterface[]
        {
            new Button(Textures.txMainMenu_Start, 320 - Textures.txMainMenu_Start.GetRenderTexture().Width / 2 - 10, 228, (_) => ClickEnterNameMenu(1)),
            new Button(Textures.txMainMenu_Back, 320 + Textures.txMainMenu_Back.GetRenderTexture().Width / 2 + 10, 228, (_) => ClickEnterNameMenu(2))
        });

        public readonly Menu StoryModeMenu = new Menu(new IUserInterface[]
        {
            new Button(Textures.txMainMenu_SinglePlayer, 320, 224, (_) => ClickSingleMultiMenu(0)),
            new Button(Textures.txMainMenu_Multiplayer, 320, 249, (_) => ClickSingleMultiMenu(1)),
            new Button(Textures.txMainMenu_Back, 320, 288, (_) => ClickSingleMultiMenu(2))
        });

        public readonly Menu ArcadeModeMenu = new Menu(new IUserInterface[]
        {
            new Button(Textures.txMainMenu_SinglePlayer, 320, 217, (_) => ClickSingleMultiMenu(0)),
            new Button(Textures.txMainMenu_Multiplayer, 320, 240, (_) => ClickSingleMultiMenu(1)),
            new Button(Textures.txMainMenu_HighScore, 320, 263, (_) => ClickSingleMultiMenu(2)),
            new Button(Textures.txMainMenu_Back, 320, 293, (_) => ClickSingleMultiMenu(3))
        });

        public readonly Menu OptionsMenu = new Menu(new IUserInterface[]
        {
            new Button(Textures.txMainMenu_Options_Graphics, 320, 217, (_) => ClickOptionsMenu(0)),
            new Button(Textures.txMainMenu_Options_Audio, 320, 240, (_) => ClickOptionsMenu(1)),
            new Button(Textures.txMainMenu_Options_Game, 320, 263, (_) => ClickOptionsMenu(2)),
            new Button(Textures.txMainMenu_Back, 320, 293, (_) => ClickOptionsMenu(3))
        });

        public readonly Menu ArcadeModeCreateOrRandomMenu = new Menu(new IUserInterface[]
        {
            new Button(Textures.txMainMenu_DesignHero, 320, 224, (_) => ClickCreateOrRandomMenu(0)),
            new Button(Textures.txMainMenu_Randomize, 320, 249, (_) => ClickCreateOrRandomMenu(1)),
            new Button(Textures.txMainMenu_Back, 320, 291, (_) => ClickCreateOrRandomMenu(2))
        });

        public readonly Dictionary<MethodInfo, Func<Menu>> RenderMenuMap;

        public readonly Dictionary<MethodInfo, Func<Menu>> InterfaceMenuMap;

        public static GlobalData.MainMenu MenuData => Globals.Game.xGlobalData.xMainMenuData;
        public static LocalInputHelper MenuInput => Globals.Game.xInput_Menu;
        public static RenderMaster.LocalizedTextures Textures => RenderMaster.xLocalizedTextures;

        public static void SimulateActionButtonPress(LocalInputHelper input)
        {
            input.Action.bPressed = true;

            input.Up.bPressed = false;
            input.Down.bPressed = false;
            input.Left.bPressed = false;
            input.Right.bPressed = false;
        }

        public static void ClickMainMenu(int selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.iTopMenuSelection = selection;
        }

        public static void ClickSingleMultiMenu(int selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.iMultiOrSingleplayerSelection = selection;
        }

        public static void ClickOptionsMenu(int selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.iMultiplayerRootSelection = selection;
        }

        private static void ClickCreateOrRandomMenu(int selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.iCreateOrRandomizeRogueSelection = selection;
        }

        private static void ClickCharacterCreationMenu(int selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.iCharCreateTop = selection;
        }

        private static void ClickEnterNameMenu(int selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.iMultiplayerSetPasswordSelection = selection;
        }

        private static void ClickCharacterCreationPick(int selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.iCharCreatePick = selection;
        }

        private static void ClickMultiplayerTopMenu(int selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.iMultiplayerRootSelection = selection;
        }

        private static void ClickHostGameMenu(int selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.iMultiplayerSetPasswordSelection = selection;
        }

        private static void ClickMultiplayerLobbyMenu(int selection)
        {
            SimulateActionButtonPress(MenuInput);
            MenuData.iMultiplayerLobbySelection = selection;
        }

        private static void ClickCharacterSelectMenu(int? charSelect, int? pageSelect)
        {
            if (charSelect != null)
            {
                SimulateActionButtonPress(MenuInput);
                MenuData.iSelectedChar = charSelect.Value;
            }
            else if (pageSelect != null)
            {
                if (MenuData.iSelectedChar > 2)
                    MenuData.iSelectedChar = 2;

                if (pageSelect.Value < MenuData.iCurrentCharSelectPage)
                    MenuInput.Left.bPressed = true;

                else MenuInput.Right.bPressed = true;
            }
        }
    }
}
