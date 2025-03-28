using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;


namespace AngeliA;
public static class Input {




	#region --- SUB ---


	private class State {

		public bool Down => Holding && !PrevHolding;
		public bool Up => !Holding && PrevHolding;

		public bool Holding = false;
		public bool PrevHolding = false;
		public bool Ignored = false;
		public int Frame = -1;
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly KeyboardKey[] KEYBOARD_DEFAULT = [
		KeyboardKey.A, KeyboardKey.D, KeyboardKey.S, KeyboardKey.W,
		KeyboardKey.L, KeyboardKey.P, KeyboardKey.Escape, KeyboardKey.Space,
	];
	private static readonly GamepadKey[] GAMEPAD_DEFAULT = [
		GamepadKey.DpadLeft, GamepadKey.DpadRight,
		GamepadKey.DpadDown, GamepadKey.DpadUp,
		GamepadKey.East, GamepadKey.South,
		GamepadKey.Start, GamepadKey.Select,
	];

	// Api
	public static bool UsingGamepad { get; private set; } = false;
	public static bool UsingLeftStick { get; private set; } = false;
	public static Direction3 DirectionX => IgnoringKeyInput ? Direction3.None : _DirectionX;
	public static Direction3 DirectionY => IgnoringKeyInput ? Direction3.None : _DirectionY;
	public static Int2 Direction => IgnoringKeyInput ? default : _Direction;
	public static bool AllowGamepad {
		get => s_AllowGamepad.Value;
		set => s_AllowGamepad.Value = value;
	}
	public static bool HoldingCtrl => KeyboardHolding(KeyboardKey.LeftCtrl) || KeyboardHolding(KeyboardKey.RightCtrl) || Game.IsGamepadKeyHolding(GamepadKey.LeftTrigger);
	public static bool HoldingShift => KeyboardHolding(KeyboardKey.LeftShift) || KeyboardHolding(KeyboardKey.RightShift) || Game.IsGamepadKeyHolding(GamepadKey.LeftShoulder);
	public static bool HoldingAlt => KeyboardHolding(KeyboardKey.LeftAlt) || KeyboardHolding(KeyboardKey.RightAlt) || Game.IsGamepadKeyHolding(GamepadKey.RightTrigger);

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
	public static Int2 MouseGlobalPositionDelta { get; private set; } = default;
	public static Int2 UnshiftedMouseGlobalPosition => _MouseGlobalPosition;
	public static Int2 MouseGlobalPosition => _MouseGlobalPosition + MousePositionShift;
	public static Int2 MouseLeftDownGlobalPosition => _MouseLeftDownGlobalPosition + MousePositionShift;
	public static Int2 MouseRightDownGlobalPosition => _MouseRightDownGlobalPosition + MousePositionShift;
	public static Int2 MouseMidDownGlobalPosition => _MouseMidDownGlobalPosition + MousePositionShift;
	public static bool MouseMove { get; private set; } = false;
	public static bool MouseLeftButtonHolding => !IgnoringMouseInput && !MouseLeftState.Ignored && MouseLeftState.Holding;
	public static bool MouseRightButtonHolding => !IgnoringMouseInput && !MouseRightState.Ignored && MouseRightState.Holding;
	public static bool MouseMidButtonHolding => !IgnoringMouseInput && !MouseMidState.Ignored && MouseMidState.Holding;
	public static bool MouseLeftButtonDown => !IgnoringMouseInput && !MouseLeftState.Ignored && MouseLeftState.Down;
	public static bool MouseRightButtonDown => !IgnoringMouseInput && !MouseRightState.Ignored && MouseRightState.Down;
	public static bool MouseMidButtonDown => !IgnoringMouseInput && !MouseMidState.Ignored && MouseMidState.Down;
	public static int IgnoreMouseToActionFrame { get; private set; } = int.MinValue;
	public static int IgnoreMouseToJumpFrame { get; private set; } = int.MinValue;
	public static int IgnoreRightStickToMouseWheelFrame { get; private set; } = int.MinValue;
	public static int MidMouseToActionFrame { get; private set; } = int.MinValue;
	public static bool LastActionFromMouse { get; private set; } = false;
	public static int MouseWheelDelta => IgnoringMouseInput ? 0 : _MouseWheelDelta;
	public static bool IgnoringMouseInput => Game.GlobalFrame <= IgnoreMouseInputFrame;
	public static bool IgnoringKeyInput => Game.GlobalFrame <= IgnoreKeyInputFrame;
	public static Int2 MousePositionShift { get; private set; } = default;
	public static (int prev, int current) LastMouseLeftButtonDownFrame { get; private set; } = (-120, -120);
	public static (int prev, int current) LastMouseRightButtonDownFrame { get; private set; } = (-120, -120);
	public static (int prev, int current) LastMouseMidButtonDownFrame { get; private set; } = (-120, -120);

	// Data
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
	private static readonly Dictionary<KeyboardKey, State> KeyboardStateMap = [];
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
	private static int IgnoreMouseInputFrame = -1;
	private static int IgnoreKeyInputFrame = -1;
	private static KeyboardKey[] AllKeyboardKeys = [];
	private static int _MouseWheelDelta = 0;
	private static Direction3 _DirectionX = Direction3.None;
	private static Direction3 _DirectionY = Direction3.None;
	private static Int2 _Direction = default;
	private static Int2 _MouseGlobalPosition = default;
	private static Int2 _MouseLeftDownGlobalPosition = default;
	private static Int2 _MouseRightDownGlobalPosition = default;
	private static Int2 _MouseMidDownGlobalPosition = default;

	// Saving
	private static readonly SavingBool s_AllowGamepad = new("Input.AllowGamepad", true, SavingLocation.Global);
	private static readonly SavingInt[] KeyboardConfigSaving = [
		new("Input.Left", (int)KEYBOARD_DEFAULT[(int)Gamekey.Left], SavingLocation.Global),
		new("Input.Right", (int)KEYBOARD_DEFAULT[(int)Gamekey.Right], SavingLocation.Global),
		new("Input.Down", (int)KEYBOARD_DEFAULT[(int)Gamekey.Down], SavingLocation.Global),
		new("Input.Up", (int)KEYBOARD_DEFAULT[(int)Gamekey.Up], SavingLocation.Global),
		new("Input.Action",(int) KEYBOARD_DEFAULT[(int)Gamekey.Action], SavingLocation.Global),
		new("Input.Jump", (int)KEYBOARD_DEFAULT[(int)Gamekey.Jump], SavingLocation.Global),
		new("Input.Start", (int)KEYBOARD_DEFAULT[(int)Gamekey.Start], SavingLocation.Global),
		new("Input.Select", (int)KEYBOARD_DEFAULT[(int)Gamekey.Select], SavingLocation.Global),
	];
	private static readonly SavingInt[] GamepadConfigSaving = [
		new("Input.Pad.Left", (int)GAMEPAD_DEFAULT[(int)Gamekey.Left], SavingLocation.Global),
		new("Input.Pad.Right", (int)GAMEPAD_DEFAULT[(int)Gamekey.Right], SavingLocation.Global),
		new("Input.Pad.Down", (int)GAMEPAD_DEFAULT[(int)Gamekey.Down], SavingLocation.Global),
		new("Input.Pad.Up", (int)GAMEPAD_DEFAULT[(int)Gamekey.Up], SavingLocation.Global),
		new("Input.Pad.Action", (int)GAMEPAD_DEFAULT[(int)Gamekey.Action], SavingLocation.Global),
		new("Input.Pad.Jump", (int)GAMEPAD_DEFAULT[(int)Gamekey.Jump], SavingLocation.Global),
		new("Input.Pad.Start", (int)GAMEPAD_DEFAULT[(int)Gamekey.Start], SavingLocation.Global),
		new("Input.Pad.Select", (int)GAMEPAD_DEFAULT[(int)Gamekey.Select], SavingLocation.Global),
	];


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	public static void OnGameInitialize () {

		// Add Keys for Keyboard
		var values = System.Enum.GetValues(typeof(KeyboardKey));
		AllKeyboardKeys = new KeyboardKey[values.Length];
		for (int i = 0; i < values.Length; i++) {
			var key = (KeyboardKey)values.GetValue(i);
			AllKeyboardKeys[i] = key;
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
					break;
			}
		}

		// Custom Default Input
		foreach (var (_, att) in Util.ForAllAssemblyWithAttribute<DefaultKeyboardGamekeyAttribute>()) {
			KEYBOARD_DEFAULT[(int)att.Gamekey] = att.InputKey;
		}
		foreach (var (_, att) in Util.ForAllAssemblyWithAttribute<DefaultGamepadGamekeyAttribute>()) {
			GAMEPAD_DEFAULT[(int)att.Gamekey] = att.InputKey;
		}

		// Save all Default Key Config
		for (int i = 0; i < KeyboardConfigSaving.Length; i++) {
			var saving = KeyboardConfigSaving[i];
			int value = saving.Value;
			if (!SavingSystem.HasKey(saving)) {
				value = (int)KEYBOARD_DEFAULT[i];
			}
			saving.SetValue(value, true);
		}
		for (int i = 0; i < GamepadConfigSaving.Length; i++) {
			var saving = GamepadConfigSaving[i];
			int value = saving.Value;
			if (!SavingSystem.HasKey(saving)) {
				value = (int)GAMEPAD_DEFAULT[i];
			}
			saving.SetValue(value, true);
		}

	}


	[OnGameInitializeLater]
	public static TaskResult OnGameInitializeLater () {

		if (!SavingSystem.PoolReady) return TaskResult.Continue;

		// Load Config
		for (int i = 0; i < 8; i++) {
			KeyMap[(Gamekey)i] = new Int2(KeyboardConfigSaving[i].Value, GamepadConfigSaving[i].Value);
		}

		return TaskResult.End;
	}


	[OnGameUpdate(-2048)]
	internal static void FrameUpdate () {

		IRect cameraRect = Renderer.CameraRect;

		bool gamepadAvailable = AllowGamepad && Game.IsGamepadAvailable;
		bool keyboardAvailable = Game.IsKeyboardAvailable;
		bool mouseAvailable = Game.IsMouseAvailable;

		Update_Mouse(cameraRect, mouseAvailable);
		Update_Gamepad(gamepadAvailable);
		Update_GameKey(gamepadAvailable, keyboardAvailable, mouseAvailable);
		Update_Keyboard(keyboardAvailable);
		Update_Direction(gamepadAvailable);

		MousePositionShift = default;
		AnyKeyDown = AnyGamepadButtonDown || AnyKeyboardKeyDown || AnyGamekeyDown || AnyMouseButtonDown;
		AnyKeyHolding = AnyGamepadButtonHolding || AnyKeyboardKeyHolding || AnyGamekeyHolding || AnyMouseButtonHolding;

		// Last From Mouse
		if (Game.CursorVisible) {
			if (AnyGamepadButtonHolding || AnyKeyboardKeyHolding) {
				LastActionFromMouse = false;
			} else if (MouseMove || AnyMouseButtonHolding || _MouseWheelDelta != 0) {
				LastActionFromMouse = true;
			}
		} else {
			LastActionFromMouse = false;
		}

		// Final
		GlobalFrame++;

	}


	[OnGameUpdateLater]
	internal static void UpdateIgnore () {



	}


	[OnGameUpdatePauseless(-1024)]
	internal static void UpdatePausing () {
		if (Game.IsPlaying) return;
		FrameUpdate();
	}


	private static void Update_Mouse (IRect cameraRect, bool available) {
		AnyMouseButtonDown = false;
		AnyMouseButtonHolding = false;
		if (available) {
			var uCameraRect = Renderer.CameraRange;
			var mousePos = Game.MouseScreenPosition;
			MouseScreenPositionDelta = mousePos - MouseScreenPosition;
			MouseMove = mousePos != MouseScreenPosition;
			MouseScreenPosition = mousePos;
			var newGlobalPos = new Int2(
				Util.RemapUnclamped(
					uCameraRect.xMin * Game.ScreenWidth, uCameraRect.xMax * Game.ScreenWidth,
					cameraRect.xMin, cameraRect.xMax,
					mousePos.x
				).RoundToInt(),
				Util.RemapUnclamped(
					uCameraRect.yMin * Game.ScreenHeight, uCameraRect.yMax * Game.ScreenHeight,
					cameraRect.yMin, cameraRect.yMax,
					mousePos.y
				).RoundToInt()
			);
			MouseGlobalPositionDelta = newGlobalPos - _MouseGlobalPosition;
			_MouseGlobalPosition = newGlobalPos;

			bool leftHolding = Game.IsMouseLeftHolding;
			bool rightHolding = Game.IsMouseRightHolding;
			bool midHolding = Game.IsMouseMidHolding;
			if (leftHolding && !MouseLeftState.Holding) {
				LastMouseLeftButtonDownFrame = (LastMouseLeftButtonDownFrame.current, Game.GlobalFrame);
			}
			if (rightHolding && !MouseRightState.Holding) {
				LastMouseRightButtonDownFrame = (LastMouseRightButtonDownFrame.current, Game.GlobalFrame);
			}
			if (midHolding && !MouseMidState.Holding) {
				LastMouseMidButtonDownFrame = (LastMouseMidButtonDownFrame.current, Game.GlobalFrame);
			}
			RefreshState(MouseLeftState, leftHolding);
			RefreshState(MouseRightState, rightHolding);
			RefreshState(MouseMidState, midHolding);

			_MouseLeftDownGlobalPosition = MouseLeftButtonDown ? newGlobalPos : _MouseLeftDownGlobalPosition;
			_MouseRightDownGlobalPosition = MouseRightButtonDown ? newGlobalPos : _MouseRightDownGlobalPosition;
			_MouseMidDownGlobalPosition = MouseMidButtonDown ? newGlobalPos : _MouseMidDownGlobalPosition;

			_MouseWheelDelta = Game.MouseScrollDelta;

		} else {
			MouseScreenPositionDelta = default;
			MouseGlobalPositionDelta = default;
			MouseScreenPosition = default;
			_MouseGlobalPosition = default;
			_MouseLeftDownGlobalPosition = default;
			_MouseRightDownGlobalPosition = default;
			_MouseMidDownGlobalPosition = default;
			RefreshState(MouseLeftState, false);
			RefreshState(MouseRightState, false);
			RefreshState(MouseMidState, false);
			MouseMove = false;
			_MouseWheelDelta = 0;
		}

		static void RefreshState (State state, bool holding) {
			state.PrevHolding = state.Holding;
			state.Holding = holding;
			if (state.PrevHolding != state.Holding) {
				state.Frame = GlobalFrame;
			}
			if (!state.PrevHolding && state.Holding) {
				state.Ignored = false;
			}
			if (!state.Ignored) {
				if (!AnyMouseButtonDown && state.Down) AnyMouseButtonDown = true;
				if (!AnyMouseButtonHolding && state.Holding) AnyMouseButtonHolding = true;
			}
		}
	}


	private static void Update_Gamepad (bool available) {
		AnyGamepadButtonDown = false;
		AnyGamepadButtonHolding = false;
		foreach (var (key, state) in GamepadStateMap) {
			state.PrevHolding = state.Holding;
			if (available) {
				state.Holding = Game.IsGamepadKeyHolding(key);
				if (state.Down) UsingGamepad = true;
			} else {
				state.Holding = false;
			}
			if (state.PrevHolding != state.Holding) {
				state.Frame = GlobalFrame;
			}
			if (!state.PrevHolding && state.Holding) {
				state.Ignored = false;
			}
			// Any
			if (!state.Ignored) {
				if (!AnyGamepadButtonDown && state.Down) AnyGamepadButtonDown = true;
				if (!AnyGamepadButtonHolding && state.Holding) AnyGamepadButtonHolding = true;
			}
		}
		// Mouse Wheel from Right Stick
		if (Game.PauselessFrame > IgnoreRightStickToMouseWheelFrame && _MouseWheelDelta == 0) {
			int acc = 0;
			if (available) {
				if (Game.IsGamepadRightStickHolding(Direction4.Down)) acc = -1;
				if (Game.IsGamepadRightStickHolding(Direction4.Up)) acc = 1;
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
					_MouseWheelDelta = acc;
				}
			} else {
				GamepadRightStickAccumulate = null;
			}
		} else {
			GamepadRightStickAccumulate = null;
		}
	}


	private static void Update_GameKey (bool gamepadAvailable, bool keyboardAvailable, bool mouseAvailable) {

		AnyGamekeyDown = false;
		AnyGamekeyHolding = false;

		RefreshState(Gamekey.Left, gamepadAvailable, keyboardAvailable, mouseAvailable);
		RefreshState(Gamekey.Right, gamepadAvailable, keyboardAvailable, mouseAvailable);
		RefreshState(Gamekey.Down, gamepadAvailable, keyboardAvailable, mouseAvailable);
		RefreshState(Gamekey.Up, gamepadAvailable, keyboardAvailable, mouseAvailable);
		RefreshState(Gamekey.Action, gamepadAvailable, keyboardAvailable, mouseAvailable);
		RefreshState(Gamekey.Jump, gamepadAvailable, keyboardAvailable, mouseAvailable);
		RefreshState(Gamekey.Start, gamepadAvailable, keyboardAvailable, mouseAvailable);
		RefreshState(Gamekey.Select, gamepadAvailable, keyboardAvailable, mouseAvailable);

		static void RefreshState (Gamekey key, bool gamepadAvailable, bool keyboardAvailable, bool mouseAvailable) {

			var state = GamekeyStateMap[key];
			state.PrevHolding = state.Holding;
			state.Holding = false;

			// Gamepad
			if (gamepadAvailable) {
				state.Holding = Game.IsGamepadKeyHolding((GamepadKey)KeyMap[key].y);
				// Stick >> D-Pad
				if (!state.Holding) {
					switch (key) {
						case Gamekey.Left:
							state.Holding = Game.IsGamepadLeftStickHolding(Direction4.Left);
							break;
						case Gamekey.Right:
							state.Holding = Game.IsGamepadLeftStickHolding(Direction4.Right);
							break;
						case Gamekey.Down:
							state.Holding = Game.IsGamepadLeftStickHolding(Direction4.Down);
							break;
						case Gamekey.Up:
							state.Holding = Game.IsGamepadLeftStickHolding(Direction4.Up);
							break;
					}
				}
				// Using Gamepad
				if (state.Holding) UsingGamepad = true;
			}

			// Keyboard
			if (keyboardAvailable && !state.Holding) {
				state.Holding = Game.IsKeyboardKeyHolding((KeyboardKey)KeyMap[key].x);
				if (state.Holding) UsingGamepad = false;
			}

			// Check Action/Jump for Mouse
			if (
				(key == Gamekey.Action || key == Gamekey.Jump) &&
				mouseAvailable && !state.Holding
			) {
				switch (key) {
					case Gamekey.Jump:
						if (Game.PauselessFrame > IgnoreMouseToJumpFrame) {
							state.Holding = MouseRightButtonHolding;
						}
						break;
					case Gamekey.Action:
						if (Game.PauselessFrame > IgnoreMouseToActionFrame) {
							state.Holding = MouseLeftButtonHolding;
						}
						if (Game.PauselessFrame <= MidMouseToActionFrame) {
							state.Holding = MouseMidButtonHolding;
						}
						break;
				}
			}

			// Check Start from ESC and +
			if (key == Gamekey.Start && !state.Holding) {
				if (keyboardAvailable && Game.IsKeyboardKeyHolding(KeyboardKey.Escape)) {
					state.Holding = true;
					UsingGamepad = false;
				}
				if (gamepadAvailable && Game.IsGamepadKeyHolding(GamepadKey.Start)) {
					state.Holding = true;
					UsingGamepad = true;
				}
			}

			// Check Select from -
			if (gamepadAvailable && key == Gamekey.Select && !state.Holding) {
				if (Game.IsGamepadKeyHolding(GamepadKey.Select)) {
					state.Holding = true;
					UsingGamepad = true;
				}
			}

			// Refresh
			if (state.PrevHolding != state.Holding) {
				state.Frame = GlobalFrame;
			}
			if (!state.PrevHolding && state.Holding) {
				state.Ignored = false;
			}

			// Any
			if (!state.Ignored) {
				if (!AnyGamekeyDown && state.Down) AnyGamekeyDown = true;
				if (!AnyGamekeyHolding && state.Holding) AnyGamekeyHolding = true;
			}
		}
	}


	private static void Update_Keyboard (bool keyboardAvailable) {
		AnyKeyboardKeyDown = false;
		AnyKeyboardKeyHolding = false;
		foreach (var (key, state) in KeyboardStateMap) {
			state.PrevHolding = state.Holding;
			if (keyboardAvailable) {
				state.Holding = Game.IsKeyboardKeyHolding(key);
				if (state.Down) UsingGamepad = false;
			} else {
				state.Holding = false;
			}
			if (state.PrevHolding != state.Holding) {
				state.Frame = GlobalFrame;
			}
			if (!state.PrevHolding && state.Holding) {
				state.Ignored = false;
			}
			// Any
			if (!state.Ignored) {
				if (!AnyKeyboardKeyDown && state.Down) AnyKeyboardKeyDown = true;
				if (!AnyKeyboardKeyHolding && state.Holding) AnyKeyboardKeyHolding = true;
			}
		}
	}


	private static void Update_Direction (bool gamepadAvailable) {

		_DirectionX = Direction3.None;
		_DirectionY = Direction3.None;

		// Left
		if (GameKeyHolding(Gamekey.Left)) {
			if (LeftDownFrame < 0) {
				LeftDownFrame = GlobalFrame;
			}
			if (LeftDownFrame > RightDownFrame) {
				_DirectionX = Direction3.Negative;
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
				_DirectionX = Direction3.Positive;
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
				_DirectionY = Direction3.Negative;
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
				_DirectionY = Direction3.Positive;
			}
		} else if (UpDownFrame > 0) {
			UpDownFrame = int.MinValue;
		}

		// Normalized Direction
		UsingLeftStick = false;
		Float2 direction = default;
		float magnitude = 0f;
		if (gamepadAvailable) {
			var value = Game.GamepadLeftStickDirection;
			direction = new Float2(value.x, value.y);
			magnitude = direction.magnitude;
		}
		if (magnitude < 0.05f) {
			direction.x = (int)DirectionX;
			direction.y = (int)DirectionY;
			magnitude = 1f;
		} else {
			UsingLeftStick = true;
		}
		_Direction = (1000f * magnitude * direction.normalized).FloorToInt();

	}


	#endregion




	#region --- API ---


	// Any Key
	public static bool TryGetHoldingGamepadButton (out GamepadKey button) {
		button = GamepadKey.East;
		if (IgnoringKeyInput) return false;
		return Game.IsGamepadAvailable && SearchAnyGamepadButtonHolding(out button);
	}
	public static bool TryGetHoldingKeyboardKey (out KeyboardKey key) {
		key = KeyboardKey.None;
		if (IgnoringKeyInput) return false;
		return Game.IsKeyboardAvailable && SearchAnyKeyboardKeyHolding(out key);
	}


	// Game Key
	public static bool GameKeyDown (Gamekey key) => !IgnoringKeyInput && GamekeyStateMap[key].Down && !GamekeyStateMap[key].Ignored;
	public static bool GameKeyDownGUI (Gamekey key) {
		if (IgnoringKeyInput) return false;
		var state = GamekeyStateMap[key];
		if (state.Ignored) return false;
		if (state.Down) return true;
		if (state.Holding) {
			int frame = GlobalFrame - GamekeyStateMap[key].Frame;
			return frame > 22 && frame % 4 == 0;
		}
		return false;
	}
	public static bool GameKeyHolding (Gamekey key) => !IgnoringKeyInput && GamekeyStateMap[key].Holding && !GamekeyStateMap[key].Ignored;
	public static bool GameKeyUp (Gamekey key) => !IgnoringKeyInput && GamekeyStateMap[key].Up && !GamekeyStateMap[key].Ignored;


	// Keyboard Key
	public static bool KeyboardDown (KeyboardKey key) => !IgnoringKeyInput && KeyboardStateMap.TryGetValue(key, out var state) && state.Down && !state.Ignored;
	public static bool KeyboardDownGUI (KeyboardKey key) {
		if (IgnoringKeyInput) return false;
		var state = KeyboardStateMap[key];
		if (state.Ignored) return false;
		if (state.Down) return true;
		if (state.Holding) {
			int frame = GlobalFrame - KeyboardStateMap[key].Frame;
			return frame > 18 && frame % 2 == 0;
		}
		return false;
	}
	public static bool KeyboardHolding (KeyboardKey key) => !IgnoringKeyInput && KeyboardStateMap.TryGetValue(key, out var state) && state.Holding && !state.Ignored;
	public static bool KeyboardUp (KeyboardKey key) => !IgnoringKeyInput && KeyboardStateMap.TryGetValue(key, out var state) && state.Up && !state.Ignored;


	// Use
	public static bool MouseKeyUsed (int key) => key switch {
		0 => MouseLeftState.Ignored,
		1 => MouseRightState.Ignored,
		2 => MouseMidState.Ignored,
		_ => false,
	};
	public static bool KeyboardKeyUsed (KeyboardKey key) => KeyboardStateMap.TryGetValue(key, out var state) && state.Ignored;


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
	public static void UseAllMouseKey () {
		MouseLeftState.Ignored = true;
		MouseRightState.Ignored = true;
		MouseMidState.Ignored = true;
		_MouseWheelDelta = 0;
	}
	public static void UseMouseKey (int index) {
		if (index == 0 && MouseLeftState.Holding) MouseLeftState.Ignored = true;
		if (index == 1 && MouseRightState.Holding) MouseRightState.Ignored = true;
		if (index == 2 && MouseMidState.Holding) MouseMidState.Ignored = true;
		if (index == 3) _MouseWheelDelta = 0;
	}


	public static void UnuseKeyboardKey (KeyboardKey key) {
		if (KeyboardStateMap.TryGetValue(key, out var state)) {
			state.Ignored = false;
		}
	}
	public static void UnuseGameKey (Gamekey key) => GamekeyStateMap[key].Ignored = false;
	public static void UnuseMouseKey (int key) {
		switch (key) {
			case 0:
				MouseLeftState.Ignored = false;
				break;
			case 1:
				MouseRightState.Ignored = false;
				break;
			case 2:
				MouseMidState.Ignored = false;
				break;
		}
	}
	public static void UnuseAllMouseKey () {
		MouseLeftState.Ignored = false;
		MouseRightState.Ignored = false;
		MouseMidState.Ignored = false;
	}


	public static void IgnoreMouseToActionJump (bool ignoreAction = true, bool ignoreJump = true, bool useMidButtonAsAction = false, int duration = 1) {
		if (ignoreAction) IgnoreMouseToActionFrame = Game.PauselessFrame + duration;
		if (ignoreJump) IgnoreMouseToJumpFrame = Game.PauselessFrame + duration;
		if (useMidButtonAsAction) MidMouseToActionFrame = Game.PauselessFrame + duration;
	}


	public static void IgnoreRightStickToMouseWheel (int duration = 1) => IgnoreRightStickToMouseWheelFrame = Game.PauselessFrame + duration;


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
	public static bool MouseButtonHolding (int button) => !IgnoringKeyInput && button switch {
		0 => MouseLeftButtonHolding,
		1 => MouseRightButtonHolding,
		2 => MouseMidButtonHolding,
		_ => false,
	};


	public static int GetHoldingMouseButton () {
		if (IgnoringKeyInput) return -1;
		if (MouseLeftButtonHolding) return 0;
		if (MouseRightButtonHolding) return 1;
		if (MouseMidButtonHolding) return 2;
		return -1;
	}


	// Misc
	public static void IgnoreAllInput (int duration = 0) {
		IgnoreMouseInput(duration);
		IgnoreKeyInput(duration);
	}
	public static void IgnoreMouseInput (int duration = 0) {
		IgnoreMouseInputFrame = Game.GlobalFrame + duration;
	}

	public static void IgnoreKeyInput (int duration = 0) => IgnoreKeyInputFrame = Game.GlobalFrame + duration;


	public static void CancelIgnoreMouseInput () => IgnoreMouseInputFrame = Game.GlobalFrame - 1;
	public static void CancelIgnoreKeyInput () => IgnoreKeyInputFrame = Game.GlobalFrame - 1;


	public static void SetMousePositionShift (int x, int y) => MousePositionShift = new Int2(x, y);


	#endregion




	#region --- LGC ---


	private static void SaveInputToDisk () {
		for (int i = 0; i < 8; i++) {
			KeyboardConfigSaving[i].Value = KeyMap[(Gamekey)i].x;
			GamepadConfigSaving[i].Value = KeyMap[(Gamekey)i].y;
		}
	}


	private static bool SearchAnyGamepadButtonHolding (out GamepadKey button) {
		button = GamepadKey.DpadUp;
		for (int i = 0; i < 16; i++) {
			if (i == 14) i = 0x20;
			if (i == 15) i = 33;
			var btn = (GamepadKey)i;
			if (Game.IsGamepadKeyHolding(btn)) {
				button = btn;
				return true;
			}
		}
		if (Game.IsGamepadLeftStickHolding(Direction4.Left)) {
			button = GamepadKey.DpadLeft;
			return true;
		}
		if (Game.IsGamepadLeftStickHolding(Direction4.Right)) {
			button = GamepadKey.DpadRight;
			return true;
		}
		if (Game.IsGamepadLeftStickHolding(Direction4.Down)) {
			button = GamepadKey.DpadDown;
			return true;
		}
		if (Game.IsGamepadLeftStickHolding(Direction4.Up)) {
			button = GamepadKey.DpadUp;
			return true;
		}
		return false;
	}


	private static bool SearchAnyKeyboardKeyHolding (out KeyboardKey key) {
		key = KeyboardKey.None;
		for (int i = 0; i < AllKeyboardKeys.Length; i++) {
			var k = AllKeyboardKeys[i];
			if (Game.IsKeyboardKeyHolding(k)) {
				key = k;
				return true;
			}
		}
		return false;
	}


	#endregion




}
