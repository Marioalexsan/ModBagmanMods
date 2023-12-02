using ModBagman;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SoG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoG.LocalInputHelper;

namespace Marioalexsan.GrindeaMouseSupport
{
    public static class MouseInput
    {
        private static GraphicsDeviceManager _graphics;
        private static GraphicsDeviceManager Graphics => _graphics ??=
            AccessTools.Field(typeof(Game1), "graphics").GetValue(Globals.Game) as GraphicsDeviceManager;

        public static Vector2 MousePos { get; internal set; }
        public static Vector2 WorldMousePos
        {
            get
            {
                var renderAreaSize = new Vector2(
                    Graphics.PreferredBackBufferWidth,
                    Graphics.PreferredBackBufferHeight
                    );

                var normalizedPos = MousePos / renderAreaSize;

                var cameraPos = CAS.Camera.v2TopLeft;
                var cameraSize = new Vector2(CAS.Camera.recViewRec.Width, CAS.Camera.recViewRec.Height);

                var worldPos = cameraPos + cameraSize * normalizedPos;

                return worldPos;
            }
        }

        public static Vector2 WorldMouseGUIPos => WorldMousePos - CAS.Camera.v2TopLeft;

        private static bool[] _lastFrame = new bool[5];
        private static bool[] _thisFrame = new bool[5];

        internal static void AddNewFrame(MouseState state)
        {
            (_lastFrame, _thisFrame) = (_thisFrame, _lastFrame);

            Array.Clear(_thisFrame, 0, _thisFrame.Length);

            if (!Globals.Game.IsActive || Globals.Game.xGlobalData.xMainMenuData.enMenuLevel == GlobalData.MainMenu.MenuLevel.PushStart)
                return;  // Do not take input while unfocused

            MousePos = new Vector2(state.X, state.Y);
            _thisFrame[GetIndex(MouseButton.Left_Mouse)] = state.LeftButton == ButtonState.Pressed;
            _thisFrame[GetIndex(MouseButton.Right_Mouse)] = state.RightButton == ButtonState.Pressed;
            _thisFrame[GetIndex(MouseButton.Middle_Mouse)] = state.MiddleButton == ButtonState.Pressed;
            _thisFrame[GetIndex(MouseButton.MouseXB1)] = state.XButton1 == ButtonState.Pressed;
            _thisFrame[GetIndex(MouseButton.MouseXB2)] = state.XButton2 == ButtonState.Pressed;
        }

        private static int GetIndex(MouseButton button) => button switch
        {
            MouseButton.Left_Mouse => 0,
            MouseButton.Middle_Mouse => 1,
            MouseButton.Right_Mouse => 2,
            MouseButton.MouseXB1 => 3,
            MouseButton.MouseXB2 => 4,
            _ => -1
        };

        public static bool IsActive(MouseButton button)
        {
            var index = GetIndex(button);

            if (index == -1)
                return false;

            return _thisFrame[index];
        }

        public static bool IsPressed(MouseButton button)
        {
            var index = GetIndex(button);

            if (index == -1)
                return false;

            return !_lastFrame[index] && _thisFrame[index];
        }

        public static bool IsReleased(MouseButton button)
        {
            var index = GetIndex(button);

            if (index == -1)
                return false;

            return _lastFrame[index] && !_thisFrame[index];
        }
    }
}
