using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace AngeliaFramework {

	public enum Gamekey {
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


		private class State {

			public bool Down => Holding && !PrevHolding;
			public bool Up => !Holding && PrevHolding;

			public bool Holding = false;
			public bool PrevHolding = false;
			public bool Ignored = false;
			public int Frame = -1;
		}


		[System.Serializable]
		private class InputConfig : ISerializationCallbackReceiver {

			public int[] KeyboardConfig = {
				(int)KeyboardKey.A, (int)KeyboardKey.D, (int)KeyboardKey.S, (int)KeyboardKey.W,
				(int)KeyboardKey.L, (int)KeyboardKey.P, (int)KeyboardKey.Escape, (int)KeyboardKey.Space,
			};
			public int[] GamepadConfig = {
				(int)GamepadKey.DpadLeft, (int)GamepadKey.DpadRight,
				(int)GamepadKey.DpadDown, (int)GamepadKey.DpadUp,
				(int)GamepadKey.East, (int)GamepadKey.South,
				(int)GamepadKey.Start, (int)GamepadKey.Select,
			};

			public void OnAfterDeserialize () => Valid();
			public void OnBeforeSerialize () => Valid();

			private void Valid () {

				if (KeyboardConfig == null) KEYBOARD_DEFAULT.CopyTo(KeyboardConfig, 0);
				if (GamepadConfig == null) GAMEPAD_DEFAULT.CopyTo(KeyboardConfig, 0);

				if (KeyboardConfig.Length != 8) {
					var newArray = new int[8];
					for (int i = 0; i < KeyboardConfig.Length && i < newArray.Length; i++) {
						newArray[i] = KeyboardConfig[i];
					}
					for (int i = KeyboardConfig.Length; i < newArray.Length; i++) {
						newArray[i] = KEYBOARD_DEFAULT[i];
					}
					KeyboardConfig = newArray;
				}

				if (GamepadConfig.Length != 8) {
					var newArray = new int[8];
					for (int i = 0; i < GamepadConfig.Length && i < newArray.Length; i++) {
						newArray[i] = GamepadConfig[i];
					}
					for (int i = GamepadConfig.Length; i < newArray.Length; i++) {
						newArray[i] = GAMEPAD_DEFAULT[i];
					}
					GamepadConfig = newArray;
				}

			}

		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int[] KEYBOARD_DEFAULT = {
			(int)KeyboardKey.A, (int)KeyboardKey.D, (int)KeyboardKey.S, (int)KeyboardKey.W,
			(int)KeyboardKey.L, (int)KeyboardKey.P, (int)KeyboardKey.Escape, (int)KeyboardKey.Space,
		};
		private static readonly int[] GAMEPAD_DEFAULT = {
			(int)GamepadKey.DpadLeft, (int)GamepadKey.DpadRight,
			(int)GamepadKey.DpadDown, (int)GamepadKey.DpadUp,
			(int)GamepadKey.East, (int)GamepadKey.South,
			(int)GamepadKey.Start, (int)GamepadKey.Select,
		};

		// Api
		public static bool UsingGamepad { get; private set; } = false;
		public static bool UsingLeftStick { get; private set; } = false;
		public static Direction3 DirectionX { get; private set; } = Direction3.None;
		public static Direction3 DirectionY { get; private set; } = Direction3.None;
		public static Int2 Direction { get; private set; } = default;
		public static bool AllowGamepad {
			get => s_AllowGamepad.Value;
			set => s_AllowGamepad.Value = value;
		}

		// Api - Anykey
		public static bool AnyKeyDown { get; private set; } = false;
		public static bool AnyKeyHolding { get; private set; } = false;
		public static bool AnyGamepadButtonDown { get; private set; } = false;
		public static bool AnyGamepadButtonHolding { get; private set; } = false;
		public static bool AnyGamekeyDown { get; private set; } = false;
		public static bool AnyGamekeyHolding { get; private set; } = false;
		public static bool AnyKeyboardKeyDown { get; private set; } = false;
		public static bool AnyKeyboardKeyHolding { get; private set; } = false;
		public static bool AnyMouseButtonDown { get; private set; } = false;
		public static bool AnyMouseButtonHolding { get; private set; } = false;

		// API - Mouse
		public static Int2 MouseScreenPosition { get; private set; } = default;
		public static Int2 MouseScreenPositionDelta { get; private set; } = default;
		public static Int2 MouseGlobalPosition { get; private set; } = default;
		public static Int2 MouseGlobalPositionDelta { get; private set; } = default;
		public static bool MouseMove { get; private set; } = false;
		public static bool MouseLeftButton => !MouseLeftState.Ignored && MouseLeftState.Holding;
		public static bool MouseRightButton => !MouseRightState.Ignored && MouseRightState.Holding;
		public static bool MouseMidButton => !MouseMidState.Ignored && MouseMidState.Holding;
		public static bool MouseLeftButtonDown => !MouseLeftState.Ignored && MouseLeftState.Down;
		public static bool MouseRightButtonDown => !MouseRightState.Ignored && MouseRightState.Down;
		public static bool MouseMidButtonDown => !MouseMidState.Ignored && MouseMidState.Down;
		public static bool IgnoreMouseToActionJumpForThisFrame { get; set; } = false;
		public static bool LastActionFromMouse { get; private set; } = false;
		public static int MouseWheelDelta { get; private set; } = 0;

		// Short
		private static Camera MainCamera => _MainCamera != null ? _MainCamera : (_MainCamera = Camera.main);
		private static Camera _MainCamera = null;
		private static bool IgnoringInput => Game.GlobalFrame <= IgnoreInputFrame;

		// Data
		private static Keyboard Keyboard = null;
		private static Gamepad Gamepad = null;
		private static Mouse Mouse = null;
		private static readonly Dictionary<Gamekey, State> GamekeyStateMap = new() {
			{ Gamekey.Left, new() },
			{ Gamekey.Right, new() },
			{ Gamekey.Down, new() },
			{ Gamekey.Up, new() },
			{ Gamekey.Action, new() },
			{ Gamekey.Jump, new() },
			{ Gamekey.Start, new() },
			{ Gamekey.Select, new() },
		};
		private static readonly Dictionary<GamepadKey, State> GamepadStateMap = new() {
			{GamepadKey.DpadDown, new() },
			{GamepadKey.DpadUp, new() },
			{GamepadKey.DpadLeft, new() },
			{GamepadKey.DpadRight, new() },
			{GamepadKey.South, new() },
			{GamepadKey.North, new() },
			{GamepadKey.West, new() },
			{GamepadKey.East, new() },
			{GamepadKey.LeftShoulder, new() },
			{GamepadKey.RightShoulder, new() },
			{GamepadKey.LeftTrigger, new() },
			{GamepadKey.RightTrigger, new() },
			{GamepadKey.LeftStick, new() },
			{GamepadKey.RightStick, new() },
			{GamepadKey.Start, new() },
			{GamepadKey.Select, new() },
		};
		private static readonly Dictionary<KeyboardKey, State> KeyboardStateMap = new();
		private static readonly Dictionary<Gamekey, Int2> KeyMap = new() {
			{ Gamekey.Left, new((int)KeyboardKey.A, (int)GamepadKey.DpadLeft) },
			{ Gamekey.Right, new((int)KeyboardKey.D, (int)GamepadKey.DpadRight)},
			{ Gamekey.Down, new((int)KeyboardKey.S, (int)GamepadKey.DpadDown)},
			{ Gamekey.Up, new((int)KeyboardKey.W, (int)GamepadKey.DpadUp)},
			{ Gamekey.Action, new((int)KeyboardKey.L, (int)GamepadKey.East)},
			{ Gamekey.Jump, new((int)KeyboardKey.P, (int)GamepadKey.South)},
			{ Gamekey.Start, new((int)KeyboardKey.Escape, (int)GamepadKey.Start)},
			{ Gamekey.Select, new((int)KeyboardKey.Space, (int)GamepadKey.Select)},
		};
		private readonly static State MouseLeftState = new();
		private readonly static State MouseRightState = new();
		private readonly static State MouseMidState = new();
		private static int GlobalFrame = 0;
		private static int LeftDownFrame = int.MinValue;
		private static int RightDownFrame = int.MinValue;
		private static int DownDownFrame = int.MinValue;
		private static int UpDownFrame = int.MinValue;
		private static int? GamepadRightStickAccumulate = null;
		private static int IgnoreInputFrame = -1;

		// Saving
		private static readonly SavingBool s_AllowGamepad = new("FrameInput.AllowGamepad", true);


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-128)]
		public static void BeforeGameInitialize () {

			// Load Config
			var iConfig = AngeUtil.LoadOrCreateJson<InputConfig>(Application.persistentDataPath);
			for (int i = 0; i < 8; i++) {
				KeyMap[(Gamekey)i] = new Int2(iConfig.KeyboardConfig[i], iConfig.GamepadConfig[i]);
			}
			KeyMap[Gamekey.Start] = new Int2((int)KeyboardKey.Escape, (int)GamepadKey.Start);

			// Add Keys for Keyboard
			var values = System.Enum.GetValues(typeof(KeyboardKey));
			for (int i = 0; i < values.Length; i++) {
				var key = (KeyboardKey)values.GetValue(i);
				switch (key) {
					default:
						KeyboardStateMap.TryAdd(key, new());
						break;
					case KeyboardKey.None:
					case KeyboardKey.OEM1:
					case KeyboardKey.OEM2:
					case KeyboardKey.OEM3:
					case KeyboardKey.OEM4:
					case KeyboardKey.OEM5:
					case KeyboardKey.IMESelected:
					case KeyboardKey.NumpadEnter:
					case KeyboardKey.NumpadDivide:
					case KeyboardKey.NumpadMultiply:
					case KeyboardKey.NumpadPlus:
					case KeyboardKey.NumpadMinus:
					case KeyboardKey.NumpadPeriod:
					case KeyboardKey.NumpadEquals:
					case KeyboardKey.Numpad0:
					case KeyboardKey.Numpad1:
					case KeyboardKey.Numpad2:
					case KeyboardKey.Numpad3:
					case KeyboardKey.Numpad4:
					case KeyboardKey.Numpad5:
					case KeyboardKey.Numpad6:
					case KeyboardKey.Numpad7:
					case KeyboardKey.Numpad8:
					case KeyboardKey.Numpad9:
						break;
				}
			}

		}


		internal static void FrameUpdate (IRect cameraRect) {

			Gamepad = AllowGamepad ? Gamepad.current : null;
			Keyboard = Keyboard.current;
			Mouse = Mouse.current;

			Update_Mouse(cameraRect);
			Update_Gamepad();
			Update_GameKey();
			Update_Keyboard();
			Update_Direction();

			AnyKeyDown = AnyGamepadButtonDown || AnyKeyboardKeyDown || AnyGamekeyDown || AnyMouseButtonDown;
			AnyKeyHolding = AnyGamepadButtonHolding || AnyKeyboardKeyHolding || AnyGamekeyHolding || AnyMouseButtonHolding;

			// Last From Mouse
			if (Cursor.visible) {
				if (AnyGamepadButtonHolding || AnyKeyboardKeyHolding) {
					LastActionFromMouse = false;
				} else if (MouseMove || AnyMouseButtonHolding || MouseWheelDelta != 0) {
					LastActionFromMouse = true;
				}
			} else {
				LastActionFromMouse = false;
			}

			// Final
			GlobalFrame++;
			IgnoreMouseToActionJumpForThisFrame = false;

		}


		private static void Update_Mouse (IRect cameraRect) {
			AnyMouseButtonDown = false;
			AnyMouseButtonHolding = false;
			if (Mouse != null && MainCamera != null) {
				var uCameraRect = MainCamera.rect;
				var mousePos = Mouse.position.ReadValue().ToAngelia();
				MouseScreenPositionDelta = mousePos.RoundToInt() - MouseScreenPosition;
				MouseMove = mousePos.RoundToInt() != MouseScreenPosition;
				MouseScreenPosition = mousePos.RoundToInt();
				var newGlobalPos = new Int2(
					Util.RemapUnclamped(
						uCameraRect.xMin * Screen.width, uCameraRect.xMax * Screen.width,
						cameraRect.xMin, cameraRect.xMax,
						mousePos.x
					).RoundToInt(),
					Util.RemapUnclamped(
						uCameraRect.yMin * Screen.height, uCameraRect.yMax * Screen.height,
						cameraRect.yMin, cameraRect.yMax,
						mousePos.y
					).RoundToInt()
				);
				MouseGlobalPositionDelta = newGlobalPos - MouseGlobalPosition;
				MouseGlobalPosition = newGlobalPos;

				RefreshState(MouseLeftState, Mouse.leftButton.isPressed);
				RefreshState(MouseRightState, Mouse.rightButton.isPressed);
				RefreshState(MouseMidState, Mouse.middleButton.isPressed);

				float scroll = Mouse.scroll.ReadValue().y;
				int scrollDelta = scroll.AlmostZero() ? 0 : scroll > 0f ? 1 : -1;
				MouseWheelDelta = scrollDelta;

			} else {
				MouseScreenPositionDelta = default;
				MouseGlobalPositionDelta = default;
				MouseScreenPosition = default;
				MouseGlobalPosition = default;
				RefreshState(MouseLeftState, false);
				RefreshState(MouseRightState, false);
				RefreshState(MouseMidState, false);
				MouseMove = false;
				MouseWheelDelta = 0;
			}

			static void RefreshState (State state, bool holding) {
				state.PrevHolding = state.Holding;
				state.Holding = holding;
				if (state.PrevHolding != state.Holding) {
					state.Frame = GlobalFrame;
					state.Ignored = false;
				}
				if (!state.Ignored) {
					if (!AnyMouseButtonDown && state.Down) AnyMouseButtonDown = true;
					if (!AnyMouseButtonHolding && state.Holding) AnyMouseButtonHolding = true;
				}
			}
		}


		private static void Update_Gamepad () {
			AnyGamepadButtonDown = false;
			AnyGamepadButtonHolding = false;
			foreach (var (key, state) in GamepadStateMap) {
				state.PrevHolding = state.Holding;
				if (Gamepad != null) {
					state.Holding = Gamepad[(UnityEngine.InputSystem.LowLevel.GamepadButton)key].isPressed;
					if (state.Down) UsingGamepad = true;
				} else {
					state.Holding = false;
				}
				if (state.PrevHolding != state.Holding) {
					state.Frame = GlobalFrame;
					state.Ignored = false;
				}
				// Any
				if (!state.Ignored) {
					if (!AnyGamepadButtonDown && state.Down) AnyGamepadButtonDown = true;
					if (!AnyGamepadButtonHolding && state.Holding) AnyGamepadButtonHolding = true;
				}
			}
			// Mouse Wheel from Right Stick
			if (MouseWheelDelta == 0) {
				int acc = 0;
				if (Gamepad != null) {
					if (Gamepad.rightStick.down.IsPressed()) acc = -1;
					if (Gamepad.rightStick.up.IsPressed()) acc = 1;
				}
				if (acc != 0) {
					if (
						!GamepadRightStickAccumulate.HasValue ||
						(GamepadRightStickAccumulate.Value != 0 && GamepadRightStickAccumulate.Value.Sign() != acc.Sign())
					) {
						GamepadRightStickAccumulate = acc * 10;
					} else {
						GamepadRightStickAccumulate += acc;
					}
					if (GamepadRightStickAccumulate.Value.Abs() >= 10) {
						GamepadRightStickAccumulate = 0;
						MouseWheelDelta = acc;
					}
				} else {
					GamepadRightStickAccumulate = null;
				}
			} else {
				GamepadRightStickAccumulate = null;
			}
		}


		private static void Update_GameKey () {

			AnyGamekeyDown = false;
			AnyGamekeyHolding = false;

			RefreshState(Gamekey.Left);
			RefreshState(Gamekey.Right);
			RefreshState(Gamekey.Down);
			RefreshState(Gamekey.Up);
			RefreshState(Gamekey.Action);
			RefreshState(Gamekey.Jump);
			RefreshState(Gamekey.Start);
			RefreshState(Gamekey.Select);

			static void RefreshState (Gamekey key) {

				var state = GamekeyStateMap[key];
				state.PrevHolding = state.Holding;
				state.Holding = false;

				// Gamepad
				if (Gamepad != null && Gamepad.enabled) {
					state.Holding = Gamepad[(UnityEngine.InputSystem.LowLevel.GamepadButton)KeyMap[key].y].isPressed;
					// Stick >> D-Pad
					if (!state.Holding) {
						switch (key) {
							case Gamekey.Left:
								state.Holding = Gamepad.leftStick.left.isPressed;
								break;
							case Gamekey.Right:
								state.Holding = Gamepad.leftStick.right.isPressed;
								break;
							case Gamekey.Down:
								state.Holding = Gamepad.leftStick.down.isPressed;
								break;
							case Gamekey.Up:
								state.Holding = Gamepad.leftStick.up.isPressed;
								break;
						}
					}
					// Using Gamepad
					if (state.Holding) UsingGamepad = true;
				}

				// Keyboard
				if (Keyboard != null && !state.Holding) {
					state.Holding = Keyboard[(UnityEngine.InputSystem.Key)KeyMap[key].x].isPressed;
					if (state.Holding) UsingGamepad = false;
				}

				// Check Action/Jump for Mouse
				if (
					(key == Gamekey.Action || key == Gamekey.Jump) &&
					Mouse != null && !state.Holding && !IgnoreMouseToActionJumpForThisFrame
				) {
					switch (key) {
						case Gamekey.Jump:
							state.Holding = MouseRightButton;
							break;
						case Gamekey.Action:
							state.Holding = MouseLeftButton;
							break;
					}
				}

				// Check Start from ESC and +
				if (key == Gamekey.Start && !state.Holding) {
					if (Keyboard != null && Keyboard[(UnityEngine.InputSystem.Key)KeyboardKey.Escape].isPressed) {
						state.Holding = true;
						UsingGamepad = false;
					}
					if (Gamepad != null && Gamepad.startButton.isPressed) {
						state.Holding = true;
						UsingGamepad = true;
					}
				}

				// Check Select from -
				if (key == Gamekey.Select && !state.Holding) {
					if (Gamepad != null && Gamepad.selectButton.isPressed) {
						state.Holding = true;
						UsingGamepad = true;
					}
				}

				// Refresh Ignore
				if (state.PrevHolding != state.Holding) {
					state.Frame = GlobalFrame;
					state.Ignored = false;
				}

				// Any
				if (!state.Ignored) {
					if (!AnyGamekeyDown && state.Down) AnyGamekeyDown = true;
					if (!AnyGamekeyHolding && state.Holding) AnyGamekeyHolding = true;
				}
			}
		}


		private static void Update_Keyboard () {
			AnyKeyboardKeyDown = false;
			AnyKeyboardKeyHolding = false;
			foreach (var (key, state) in KeyboardStateMap) {
				state.PrevHolding = state.Holding;
				if (Keyboard != null) {
					state.Holding = Keyboard[(UnityEngine.InputSystem.Key)key].isPressed;
					if (state.Down) UsingGamepad = false;
				} else {
					state.Holding = false;
				}
				if (state.PrevHolding != state.Holding) {
					state.Frame = GlobalFrame;
					state.Ignored = false;
				}
				// Any
				if (!state.Ignored) {
					if (!AnyKeyboardKeyDown && state.Down) AnyKeyboardKeyDown = true;
					if (!AnyKeyboardKeyHolding && state.Holding) AnyKeyboardKeyHolding = true;
				}
			}
		}


		private static void Update_Direction () {

			DirectionX = Direction3.None;
			DirectionY = Direction3.None;

			// Left
			if (GameKeyHolding(Gamekey.Left)) {
				if (LeftDownFrame < 0) {
					LeftDownFrame = GlobalFrame;
				}
				if (LeftDownFrame > RightDownFrame) {
					DirectionX = Direction3.Negative;
				}
			} else if (LeftDownFrame > 0) {
				LeftDownFrame = int.MinValue;
			}

			// Right
			if (GameKeyHolding(Gamekey.Right)) {
				if (RightDownFrame < 0) {
					RightDownFrame = GlobalFrame;
				}
				if (RightDownFrame > LeftDownFrame) {
					DirectionX = Direction3.Positive;
				}
			} else if (RightDownFrame > 0) {
				RightDownFrame = int.MinValue;
			}

			// Down
			if (GameKeyHolding(Gamekey.Down)) {
				if (DownDownFrame < 0) {
					DownDownFrame = GlobalFrame;
				}
				if (DownDownFrame > UpDownFrame) {
					DirectionY = Direction3.Negative;
				}
			} else if (DownDownFrame > 0) {
				DownDownFrame = int.MinValue;
			}

			// Up
			if (GameKeyHolding(Gamekey.Up)) {
				if (UpDownFrame < 0) {
					UpDownFrame = GlobalFrame;
				}
				if (UpDownFrame > DownDownFrame) {
					DirectionY = Direction3.Positive;
				}
			} else if (UpDownFrame > 0) {
				UpDownFrame = int.MinValue;
			}

			// Normalized Direction
			UsingLeftStick = false;
			Float2 direction = default;
			float magnitude = 0f;
			if (Gamepad != null) {
				direction = Gamepad.leftStick.ReadValue();
				magnitude = direction.magnitude;
			}
			if (magnitude < 0.05f) {
				direction.x = (int)DirectionX;
				direction.y = (int)DirectionY;
				magnitude = 1f;
			} else {
				UsingLeftStick = true;
			}
			Direction = (1000f * magnitude * direction.normalized).FloorToInt();

		}


		#endregion




		#region --- API ---


		// Any Key
		public static bool TryGetHoldingGamepadButton (out GamepadKey button) {
			button = GamepadKey.A;
			return Gamepad != null && Gamepad.AnyButtonHolding(out button);
		}
		public static bool TryGetHoldingKeyboardKey (out KeyboardKey key) {
			key = KeyboardKey.None;
			return Keyboard != null && Keyboard.AnyKeyHolding(out key);
		}


		// Game Key
		public static bool GameKeyDown (Gamekey key) => !IgnoringInput && GamekeyStateMap[key].Down && !GamekeyStateMap[key].Ignored;
		public static bool GameKeyDownGUI (Gamekey key) {
			if (IgnoringInput) return false;
			var state = GamekeyStateMap[key];
			if (state.Ignored) return false;
			if (state.Down) return true;
			if (state.Holding) {
				int frame = GlobalFrame - GamekeyStateMap[key].Frame;
				return frame > 22 && frame % 4 == 0;
			}
			return false;
		}
		public static bool GameKeyHolding (Gamekey key) => !IgnoringInput && GamekeyStateMap[key].Holding && !GamekeyStateMap[key].Ignored;
		public static bool GameKeyUp (Gamekey key) => !IgnoringInput && GamekeyStateMap[key].Up && !GamekeyStateMap[key].Ignored;


		// Keyboard Key
		public static bool KeyboardDown (KeyboardKey key) => !IgnoringInput && KeyboardStateMap.TryGetValue(key, out var state) && state.Down && !state.Ignored;
		public static bool KeyboardDownGUI (KeyboardKey key) {
			if (IgnoringInput) return false;
			var state = KeyboardStateMap[key];
			if (state.Ignored) return false;
			if (state.Down) return true;
			if (state.Holding) {
				int frame = GlobalFrame - KeyboardStateMap[key].Frame;
				return frame > 18 && frame % 2 == 0;
			}
			return false;
		}
		public static bool KeyboardHolding (KeyboardKey key) => !IgnoringInput && KeyboardStateMap.TryGetValue(key, out var state) && state.Holding && !state.Ignored;
		public static bool KeyboardUp (KeyboardKey key) => !IgnoringInput && KeyboardStateMap.TryGetValue(key, out var state) && state.Up && !state.Ignored;


		// Use
		public static void UseGameKey (Gamekey key) => GamekeyStateMap[key].Ignored = true;
		public static void UseKeyboardKey (KeyboardKey key) {
			if (KeyboardStateMap.TryGetValue(key, out var state)) {
				state.Ignored = true;
			}
		}
		public static void UseAllHoldingKeys (bool ignoreMouse = false) {
			foreach (var (_, state) in GamekeyStateMap) {
				if (state.Holding) state.Ignored = true;
			}
			foreach (var (_, state) in KeyboardStateMap) {
				if (state.Holding) state.Ignored = true;
			}
			if (!ignoreMouse) {
				if (MouseLeftState.Holding) MouseLeftState.Ignored = true;
				if (MouseRightState.Holding) MouseRightState.Ignored = true;
				if (MouseMidState.Holding) MouseMidState.Ignored = true;
			}
		}
		public static void UseMouseKey (int index) {
			if (index == 0 && MouseLeftState.Holding) MouseLeftState.Ignored = true;
			if (index == 1 && MouseRightState.Holding) MouseRightState.Ignored = true;
			if (index == 2 && MouseMidState.Holding) MouseMidState.Ignored = true;
		}


		public static void UnuseKeyboardKey (KeyboardKey key) {
			if (KeyboardStateMap.TryGetValue(key, out var state)) {
				state.Ignored = false;
			}
		}
		public static void UnuseGameKey (Gamekey key) => GamekeyStateMap[key].Ignored = false;


		// Key Map
		public static KeyboardKey GetKeyboardMap (Gamekey key) => (KeyboardKey)KeyMap[key].x;
		public static KeyboardKey GetDefaultKeyboardMap (Gamekey key) => (KeyboardKey)KEYBOARD_DEFAULT[(int)key];
		public static GamepadKey GetGamepadMap (Gamekey key) => (GamepadKey)KeyMap[key].y;
		public static GamepadKey GetDefaultGamepadMap (Gamekey key) => (GamepadKey)GAMEPAD_DEFAULT[(int)key];


		public static void SetKeyboardMap (Gamekey gameKey, KeyboardKey keyboardKey) {
			var oldValue = KeyMap[gameKey];
			oldValue.x = (int)keyboardKey;
			KeyMap[gameKey] = oldValue;
			SaveInputToDisk();
		}
		public static void SetGamepadMap (Gamekey gameKey, GamepadKey gamepadKey) {
			var oldValue = KeyMap[gameKey];
			oldValue.y = (int)gamepadKey;
			KeyMap[gameKey] = oldValue;
			SaveInputToDisk();
		}


		// Mouse
		public static bool MouseButtonHolding (int button) => !IgnoringInput && button switch {
			0 => MouseLeftButton,
			1 => MouseRightButton,
			2 => MouseMidButton,
			_ => false,
		};


		public static int GetHoldingMouseButton () {
			if (IgnoringInput) return -1;
			if (MouseLeftButton) return 0;
			if (MouseRightButton) return 1;
			if (MouseMidButton) return 2;
			return -1;
		}


		public static void IgnoreAllInput (int duration = 1) => IgnoreInputFrame = Game.GlobalFrame + duration;


		#endregion




		#region --- LGC ---


		private static void SaveInputToDisk () => AngeUtil.SaveJson(new InputConfig() {
			KeyboardConfig = new int[8] {
				KeyMap[(Gamekey)0].x,
				KeyMap[(Gamekey)1].x,
				KeyMap[(Gamekey)2].x,
				KeyMap[(Gamekey)3].x,
				KeyMap[(Gamekey)4].x,
				KeyMap[(Gamekey)5].x,
				(int)KeyboardKey.Escape,
				KeyMap[(Gamekey)7].x,
			},
			GamepadConfig = new int[8] {
				KeyMap[(Gamekey)0].y,
				KeyMap[(Gamekey)1].y,
				KeyMap[(Gamekey)2].y,
				KeyMap[(Gamekey)3].y,
				KeyMap[(Gamekey)4].y,
				KeyMap[(Gamekey)5].y,
				(int)GamepadKey.Start,
				KeyMap[(Gamekey)7].y,
			},
		}, Application.persistentDataPath, "", true);


		#endregion




	}
}
