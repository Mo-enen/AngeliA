using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;


namespace AngeliaFramework {


	public enum GameKey {
		Left = 0,
		Right = 1,
		Down = 2,
		Up = 3,
		Action = 4,
		Jump = 5,
		Start = 6,
		Select = 7,
	}


	public static class FrameInput {




		#region --- SUB ---


		private enum KeyState {
			None = 0,
			Down = 1,
			Holding = 2,
			Up = 3,
		}


		#endregion




		#region --- VAR ---


		// Api
		public static Vector2Int MouseScreenPosition { get; private set; } = default;
		public static Vector2 MousePosition01 { get; private set; } = default;
		public static bool MouseLeft { get; private set; } = false;
		public static bool MouseRight { get; private set; } = false;
		public static bool MouseMid { get; private set; } = false;
		public static bool MouseLeftDown { get; private set; } = false;
		public static bool MouseRightDown { get; private set; } = false;
		public static bool MouseMidDown { get; private set; } = false;

		// Data
		private static Keyboard Keyboard = null;
		private static Gamepad Gamepad = null;
		private static Mouse Mouse = null;
		private static readonly Dictionary<GameKey, KeyState> StateMap = new() {
			{ GameKey.Left, KeyState.None },
			{ GameKey.Right, KeyState.None },
			{ GameKey.Down, KeyState.None },
			{ GameKey.Up, KeyState.None },
			{ GameKey.Action, KeyState.None },
			{ GameKey.Jump, KeyState.None },
			{ GameKey.Start, KeyState.None },
			{ GameKey.Select, KeyState.None },
		};
		private static readonly Dictionary<GameKey, (Key a, Key b)> KeyboardMap = new() {
			{ GameKey.Left, (Key.A, Key.LeftArrow) },
			{ GameKey.Right, (Key.D, Key.RightArrow) },
			{ GameKey.Down, (Key.S, Key.DownArrow) },
			{ GameKey.Up, (Key.W, Key.UpArrow) },
			{ GameKey.Action, (Key.P, Key.Z) },
			{ GameKey.Jump, (Key.L, Key.LeftShift) },
			{ GameKey.Start, (Key.E, Key.Enter) },
			{ GameKey.Select, (Key.Q, Key.Tab) },
		};
		private static bool PrevMouseLeft = false;
		private static bool PrevMouseRight = false;
		private static bool PrevMouseMid = false;

		// Saving
		private static SavingString KeyboardSetup = new("FrameInput.KeyboardSetup", "");


		#endregion




		#region --- MSG ---


		public static void Initialize () {
			if (!string.IsNullOrEmpty(KeyboardSetup.Value)) {
				var strs = KeyboardSetup.Value.Split(',');
				if (strs != null && strs.Length >= 16) {
					for (int i = 0; i < 8; i++) {
						if (
							int.TryParse(strs[i * 2], out int a) &&
							int.TryParse(strs[i * 2 + 1], out int b)
						) {
							KeyboardMap[(GameKey)i] = ((Key)a, (Key)b);
						}
					}
				}
			} else {
				SaveKeyboardSetupToDisk();
			}
		}


		public static void FrameUpdate () {

			if (Keyboard == null) {
				Keyboard = Keyboard.current;
			}

			if (Gamepad == null) {
				Gamepad = Gamepad.current;
			}

			if (Mouse == null) {
				Mouse = Mouse.current;
			}

			// Keys
			StateMap[GameKey.Left] = GetState(GameKey.Left);
			StateMap[GameKey.Right] = GetState(GameKey.Right);
			StateMap[GameKey.Down] = GetState(GameKey.Down);
			StateMap[GameKey.Up] = GetState(GameKey.Up);
			StateMap[GameKey.Action] = GetState(GameKey.Action);
			StateMap[GameKey.Jump] = GetState(GameKey.Jump);
			StateMap[GameKey.Start] = GetState(GameKey.Start);
			StateMap[GameKey.Select] = GetState(GameKey.Select);

			// Pointer
			if (Mouse != null) {
				var pos = Mouse.position.ReadValue();
				MouseScreenPosition = pos.RoundToInt();
				MousePosition01 = new Vector2(pos.x / Screen.width, pos.y / Screen.height);
				MouseLeft = Mouse.leftButton.isPressed;
				MouseRight = Mouse.rightButton.isPressed;
				MouseMid = Mouse.middleButton.isPressed;
				MouseLeftDown = !PrevMouseLeft && MouseLeft;
				MouseRightDown = !PrevMouseRight && MouseRight;
				MouseMidDown = !PrevMouseMid && MouseMid;
			} else {
				MouseScreenPosition = default;
				MousePosition01 = default;
				MouseLeft = false;
				MouseRight = false;
				MouseMid = false;
				MouseLeftDown = false;
				MouseRightDown = false;
				MouseMidDown = false;
			}
			PrevMouseLeft = MouseLeft;
			PrevMouseRight = MouseRight;
			PrevMouseMid = MouseMid;

		}


		#endregion




		#region --- API ---


		public static bool KeyDown (GameKey key) => StateMap[key] == KeyState.Down;
		public static bool KeyPressing (GameKey key) => StateMap[key] == KeyState.Holding || StateMap[key] == KeyState.Down;
		public static bool KeyUp (GameKey key) => StateMap[key] == KeyState.Up;


		public static (Key a, Key b) GetKeyboardMap (GameKey key) => KeyboardMap[key];
		public static void SetKeyboardMap (GameKey key, Key a, Key b) => KeyboardMap[key] = (a, b);
		public static void SaveKeyboardSetupToDisk () {
			var builder = new StringBuilder();
			for (int i = 0; i < 8; i++) {
				builder.Append((int)KeyboardMap[(GameKey)i].a);
				builder.Append(',');
				builder.Append((int)KeyboardMap[(GameKey)i].b);
				if (i < 7) {
					builder.Append(',');
				}
			}
			KeyboardSetup.Value = builder.ToString();
		}


		public static void ClearMouseLeftEvent () {
			MouseLeft = false;
			MouseLeftDown = false;
		}
		public static void ClearMouseRightEvent () {
			MouseRight = false;
			MouseRightDown = false;
		}
		public static void ClearMouseMidEvent () {
			MouseMid = false;
			MouseMidDown = false;
		}
		public static void ClearKeyState (GameKey key) => StateMap[key] = KeyState.None;


		#endregion




		#region --- LGC ---


		private static KeyState GetState (GameKey key) {
			bool prevHolding = StateMap[key] == KeyState.Holding || StateMap[key] == KeyState.Down;
			bool holding = false;
			if (Gamepad != null) {
				switch (key) {
					case GameKey.Left:
						holding =
							Gamepad.dpad.left.IsPressed() ||
							Gamepad.leftStick.left.IsPressed();
						break;
					case GameKey.Right:
						holding =
							Gamepad.dpad.right.IsPressed() ||
							Gamepad.leftStick.right.IsPressed();
						break;
					case GameKey.Down:
						holding =
							Gamepad.dpad.down.IsPressed() ||
							Gamepad.leftStick.down.IsPressed();
						break;
					case GameKey.Up:
						holding =
							Gamepad.dpad.up.IsPressed() ||
							Gamepad.leftStick.up.IsPressed();
						break;
					case GameKey.Action:
						holding =
							Gamepad.buttonEast.IsPressed() ||
							Gamepad.buttonWest.IsPressed() ||
							Gamepad.leftTrigger.IsPressed() ||
							Gamepad.leftShoulder.IsPressed();
						break;
					case GameKey.Jump:
						holding =
							Gamepad.buttonNorth.IsPressed() ||
							Gamepad.buttonSouth.IsPressed() ||
							Gamepad.rightTrigger.IsPressed() ||
							Gamepad.rightShoulder.IsPressed();
						break;
					case GameKey.Start:
						holding = Gamepad.startButton.IsPressed();
						break;
					case GameKey.Select:
						holding = Gamepad.selectButton.IsPressed();
						break;
				}
			} else if (Keyboard != null) {
				holding =
					Keyboard[KeyboardMap[key].a].isPressed ||
					Keyboard[KeyboardMap[key].b].isPressed;
			}
			return holding ?
				prevHolding ? KeyState.Holding : KeyState.Down :
				prevHolding ? KeyState.Up : KeyState.None;
		}


		#endregion




	}
}
