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
		A = 4,
		B = 5,
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
		public static bool SwapeGamePadAB {
			get => SwapeAB.Value;
			set => SwapeAB.Value = value;
		}
		public static Vector2 MousePosition { get; private set; } = default;
		public static bool MouseLeftDown { get; private set; } = false;
		public static bool MouseRightDown { get; private set; } = false;

		// Data
		private static Keyboard Keyboard = null;
		private static Gamepad Gamepad = null;
		private static Mouse Mouse = null;
		private static readonly Dictionary<GameKey, KeyState> StateMap = new() {
			{ GameKey.Left, KeyState.None },
			{ GameKey.Right, KeyState.None },
			{ GameKey.Down, KeyState.None },
			{ GameKey.Up, KeyState.None },
			{ GameKey.A, KeyState.None },
			{ GameKey.B, KeyState.None },
			{ GameKey.Start, KeyState.None },
			{ GameKey.Select, KeyState.None },
		};
		private static readonly Dictionary<GameKey, (Key a, Key b)> KeyboardMap = new() {
			{ GameKey.Left, (Key.A, Key.LeftArrow) },
			{ GameKey.Right, (Key.D, Key.RightArrow) },
			{ GameKey.Down, (Key.S, Key.DownArrow) },
			{ GameKey.Up, (Key.W, Key.UpArrow) },
			{ GameKey.A, (Key.P, Key.LeftShift) },
			{ GameKey.B, (Key.L, Key.Z) },
			{ GameKey.Start, (Key.Enter, Key.Space) },
			{ GameKey.Select, (Key.Tab, Key.Tab) },
		};

		// Saving
		private static SavingBool SwapeAB = new("FrameInput.SwapeAB", false);
		private static SavingString KeyboardSetup = new("FrameInput.KeyboardSetup", "");


		#endregion




		#region --- MSG ---


		[RuntimeInitializeOnLoadMethod]
		private static void Init () {
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
			StateMap[GameKey.A] = GetState(GameKey.A);
			StateMap[GameKey.B] = GetState(GameKey.B);
			StateMap[GameKey.Start] = GetState(GameKey.Start);
			StateMap[GameKey.Select] = GetState(GameKey.Select);

			// Pointer
			if (Mouse != null) {
				MousePosition = Mouse.position.ReadValue();
				MouseLeftDown = Mouse.leftButton.isPressed;
				MouseRightDown = Mouse.rightButton.isPressed;
			} else {
				MousePosition = default;
				MouseLeftDown = false;
				MouseRightDown = false;
			}

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


		#endregion




		#region --- LGC ---


		private static KeyState GetState (GameKey key) {
			bool prevHolding = StateMap[key] == KeyState.Holding || StateMap[key] == KeyState.Down;
			bool holding = false;
			if (Gamepad != null) {
				if (SwapeAB.Value && (key == GameKey.A || key == GameKey.B)) {
					key = key == GameKey.A ? GameKey.B : GameKey.A;
				}
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
					case GameKey.A:
						holding =
							Gamepad.buttonEast.IsPressed() ||
							Gamepad.buttonWest.IsPressed() ||
							Gamepad.leftTrigger.IsPressed() ||
							Gamepad.leftShoulder.IsPressed();
						break;
					case GameKey.B:
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
