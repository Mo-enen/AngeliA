using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace AngeliA;

/// <summary>
/// Frame based core system for user input from keyboard, mouse and gamepad
/// </summary>
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
	/// <summary>
	/// True if the user just used the gamepad
	/// </summary>
	public static bool UsingGamepad { get; private set; } = false;
	/// <summary>
	/// True if the user just used the list joystick
	/// </summary>
	public static bool UsingLeftStick { get; private set; } = false;
	/// <summary>
	/// Horizontal direction at current frame from gamekey, d-pad and joystick
	/// </summary>
	public static Direction3 DirectionX => IgnoringKeyInput ? Direction3.None : _DirectionX;
	/// <summary>
	/// Vertical direction at current frame from gamekey, d-pad and joystick
	/// </summary>
	public static Direction3 DirectionY => IgnoringKeyInput ? Direction3.None : _DirectionY;
	/// <summary>
	/// Direction at current frame from gamekey, d-pad and joystick
	/// </summary>
	public static Int2 Direction => IgnoringKeyInput ? default : _Direction;
	/// <summary>
	/// True if gamepad is allow to use from game setting
	/// </summary>
	public static bool AllowGamepad {
		get => s_AllowGamepad.Value;
		set => s_AllowGamepad.Value = value;
	}
	/// <summary>
	/// True if ctrl key is holding at current frame
	/// </summary>
	public static bool HoldingCtrl => KeyboardHolding(KeyboardKey.LeftCtrl) || KeyboardHolding(KeyboardKey.RightCtrl) || Game.IsGamepadKeyHolding(GamepadKey.LeftTrigger);
	/// <summary>
	/// True if shift key is holding at current frame
	/// </summary>
	public static bool HoldingShift => KeyboardHolding(KeyboardKey.LeftShift) || KeyboardHolding(KeyboardKey.RightShift) || Game.IsGamepadKeyHolding(GamepadKey.LeftShoulder);
	/// <summary>
	/// True if alt key is holding at current frame
	/// </summary>
	public static bool HoldingAlt => KeyboardHolding(KeyboardKey.LeftAlt) || KeyboardHolding(KeyboardKey.RightAlt) || Game.IsGamepadKeyHolding(GamepadKey.RightTrigger);

	// Api - Anykey
	/// <summary>
	/// True if any keyboard/gamepad/mouse key start to be holding at current frame
	/// </summary>
	public static bool AnyKeyDown { get; private set; } = false;
	/// <summary>
	/// True if any keyboard/gamepad/mouse key is holding at current frame
	/// </summary>
	public static bool AnyKeyHolding { get; private set; } = false;
	/// <summary>
	/// True if any gamepad button start to be holding at current frame
	/// </summary>
	public static bool AnyGamepadButtonDown { get; private set; } = false;
	/// <summary>
	/// True if any gamepad button is holding at current frame
	/// </summary>
	public static bool AnyGamepadButtonHolding { get; private set; } = false;
	/// <summary>
	/// True if any game-key key start to be holding at current frame
	/// </summary>
	public static bool AnyGamekeyDown { get; private set; } = false;
	/// <summary>
	/// True if any game-key is holding at current frame
	/// </summary>
	public static bool AnyGamekeyHolding { get; private set; } = false;
	/// <summary>
	/// True if any keyboard key start to be holding at current frame
	/// </summary>
	public static bool AnyKeyboardKeyDown { get; private set; } = false;
	/// <summary>
	/// True if any keyboard key is holding at current frame
	/// </summary>
	public static bool AnyKeyboardKeyHolding { get; private set; } = false;
	/// <summary>
	/// True if any mouse button start to be holding at current frame
	/// </summary>
	public static bool AnyMouseButtonDown { get; private set; } = false;
	/// <summary>
	/// True if any mouse button is holding at current frame
	/// </summary>
	public static bool AnyMouseButtonHolding { get; private set; } = false;

	// API - Mouse
	/// <summary>
	/// Position of the mouse at current frame in screen space
	/// </summary>
	public static Int2 MouseScreenPosition { get; private set; } = default;
	/// <summary>
	/// Position changed of the mouse at current frame in screen space
	/// </summary>
	public static Int2 MouseScreenPositionDelta { get; private set; } = default;
	/// <summary>
	/// Position changed of the mouse at current frame in global space
	/// </summary>
	public static Int2 MouseGlobalPositionDelta { get; private set; } = default;
	/// <summary>
	/// Position changed of the mouse at current frame in global space which is not effect by Input.SetMousePositionShift
	/// </summary>
	public static Int2 UnshiftedMouseGlobalPosition => _MouseGlobalPosition;
	/// <summary>
	/// Position of the mouse at current frame in global space
	/// </summary>
	public static Int2 MouseGlobalPosition => _MouseGlobalPosition + MousePositionShift;
	/// <summary>
	/// Position of the mouse in global space when last time mouse left button press down 
	/// </summary>
	public static Int2 MouseLeftDownGlobalPosition => _MouseLeftDownGlobalPosition + MousePositionShift;
	/// <summary>
	/// Position of the mouse in global space when last time mouse right button press down 
	/// </summary>
	public static Int2 MouseRightDownGlobalPosition => _MouseRightDownGlobalPosition + MousePositionShift;
	/// <summary>
	/// Position of the mouse in global space when last time mouse middle button press down 
	/// </summary>
	public static Int2 MouseMidDownGlobalPosition => _MouseMidDownGlobalPosition + MousePositionShift;
	/// <summary>
	/// True if mouse moved at current frame
	/// </summary>
	public static bool MouseMove { get; private set; } = false;
	/// <summary>
	/// True if mouse left button is holding at current frame
	/// </summary>
	public static bool MouseLeftButtonHolding => !IgnoringMouseInput && !MouseLeftState.Ignored && MouseLeftState.Holding;
	/// <summary>
	/// True if mouse right button is holding at current frame
	/// </summary>
	public static bool MouseRightButtonHolding => !IgnoringMouseInput && !MouseRightState.Ignored && MouseRightState.Holding;
	/// <summary>
	/// True if mouse middle button is holding at current frame
	/// </summary>
	public static bool MouseMidButtonHolding => !IgnoringMouseInput && !MouseMidState.Ignored && MouseMidState.Holding;
	/// <summary>
	/// True if mouse left button start to be holding at current frame
	/// </summary>
	public static bool MouseLeftButtonDown => !IgnoringMouseInput && !MouseLeftState.Ignored && MouseLeftState.Down;
	/// <summary>
	/// True if mouse right button start to be holding at current frame
	/// </summary>
	public static bool MouseRightButtonDown => !IgnoringMouseInput && !MouseRightState.Ignored && MouseRightState.Down;
	/// <summary>
	/// True if mouse middle button start to be holding at current frame
	/// </summary>
	public static bool MouseMidButtonDown => !IgnoringMouseInput && !MouseMidState.Ignored && MouseMidState.Down;
	/// <summary>
	/// True if last user action is from mouse instead of keyboard
	/// </summary>
	public static bool LastActionFromMouse { get; private set; } = false;
	/// <summary>
	/// Mouse wheel scroll value at current frame. Return negative value when the page scrolls down (the content appears to move upward)
	/// </summary>
	public static int MouseWheelDelta => IgnoringMouseInput ? 0 : _MouseWheelDelta;
	/// <summary>
	/// True if the input system do not receive mouse input currently 
	/// </summary>
	public static bool IgnoringMouseInput => Game.GlobalFrame <= IgnoreMouseInputFrame;
	/// <summary>
	/// True if the input system do not receive keyboard/game-key input currently 
	/// </summary>
	public static bool IgnoringKeyInput => Game.GlobalFrame <= IgnoreKeyInputFrame;

	// Internal Cache
	internal static Int2 MousePositionShift { get; private set; } = default;
	internal static int IgnoreMouseToActionFrame { get; private set; } = int.MinValue;
	internal static int IgnoreMouseToJumpFrame { get; private set; } = int.MinValue;
	internal static int IgnoreRightStickToMouseWheelFrame { get; private set; } = int.MinValue;
	internal static int MidMouseToActionFrame { get; private set; } = int.MinValue;
	internal static (int prev, int current) LastMouseLeftButtonDownFrame { get; private set; } = (-120, -120);
	internal static (int prev, int current) LastMouseRightButtonDownFrame { get; private set; } = (-120, -120);
	internal static (int prev, int current) LastMouseMidButtonDownFrame { get; private set; } = (-120, -120);

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
	internal static void OnGameInitialize () {

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
	internal static TaskResult OnGameInitializeLater () {

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
			magnitude = direction.Magnitude;
		}
		if (magnitude < 0.05f) {
			direction.x = (int)DirectionX;
			direction.y = (int)DirectionY;
			magnitude = 1f;
		} else {
			UsingLeftStick = true;
		}
		_Direction = (1000f * magnitude * direction.Normalized).FloorToInt();

	}


	#endregion




	#region --- API ---


	// Any Key
	/// <summary>
	/// Get the gamepad button that currently holding
	/// </summary>
	/// <param name="button">Result holding button</param>
	/// <returns>True if holding button founded</returns>
	public static bool TryGetHoldingGamepadButton (out GamepadKey button) {
		button = default;
		if (IgnoringKeyInput) return false;
		return Game.IsGamepadAvailable && SearchAnyGamepadButtonHolding(out button);
	}

	/// <summary>
	/// Get the keyboard key that currently holding
	/// </summary>
	/// <param name="key">Result holding key</param>
	/// <returns>True if holding key founded</returns>
	public static bool TryGetHoldingKeyboardKey (out KeyboardKey key) {
		key = KeyboardKey.None;
		if (IgnoringKeyInput) return false;
		return Game.IsKeyboardAvailable && SearchAnyKeyboardKeyHolding(out key);
	}


	// Game Key
	/// <summary>
	/// True if given game-key start to be holding at current frame
	/// </summary>
	public static bool GameKeyDown (Gamekey key) => !IgnoringKeyInput && GamekeyStateMap[key].Down && !GamekeyStateMap[key].Ignored;
	/// <summary>
	/// True if given game-key pressed down repeatedly by holding down
	/// </summary>
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
	/// <summary>
	/// True if given game-key is currently holding
	/// </summary>
	public static bool GameKeyHolding (Gamekey key) => !IgnoringKeyInput && GamekeyStateMap[key].Holding && !GamekeyStateMap[key].Ignored;
	/// <summary>
	/// True if the given game-key just released at current frame
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public static bool GameKeyUp (Gamekey key) => !IgnoringKeyInput && GamekeyStateMap[key].Up && !GamekeyStateMap[key].Ignored;


	// Keyboard Key
	/// <summary>
	/// True if given keyboard-key start to be holding at current frame
	/// </summary>
	public static bool KeyboardDown (KeyboardKey key) => !IgnoringKeyInput && KeyboardStateMap.TryGetValue(key, out var state) && state.Down && !state.Ignored;
	/// <summary>
	/// True if given keyboard-key pressed down repeatedly by holding down
	/// </summary>
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
	/// <summary>
	/// True if given keyboard-key is currently holding
	/// </summary>
	public static bool KeyboardHolding (KeyboardKey key) => !IgnoringKeyInput && KeyboardStateMap.TryGetValue(key, out var state) && state.Holding && !state.Ignored;
	/// <summary>
	/// True if the given keyboard-key just released at current frame
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public static bool KeyboardUp (KeyboardKey key) => !IgnoringKeyInput && KeyboardStateMap.TryGetValue(key, out var state) && state.Up && !state.Ignored;


	// Mouse
	/// <summary>
	/// True if mouse left button performed a double click at current frame
	/// </summary>
	/// <param name="clickDeltaFrame">Two clicks inside this time range count as a double click</param>
	public static bool IsMouseLeftButtonDoubleClick (int clickDeltaFrame = 30) => Game.GlobalFrame < LastMouseLeftButtonDownFrame.current + clickDeltaFrame && Game.GlobalFrame < LastMouseLeftButtonDownFrame.prev + clickDeltaFrame;
	/// <summary>
	/// True if mouse right button performed a double click at current frame
	/// </summary>
	/// <param name="clickDeltaFrame">Two clicks inside this time range count as a double click</param>
	public static bool IsMouseRightButtonDoubleClick (int clickDeltaFrame = 30) => Game.GlobalFrame < LastMouseRightButtonDownFrame.current + clickDeltaFrame && Game.GlobalFrame < LastMouseRightButtonDownFrame.prev + clickDeltaFrame;
	/// <summary>
	/// True if mouse middle button performed a double click at current frame
	/// </summary>
	/// <param name="clickDeltaFrame">Two clicks inside this time range count as a double click</param>
	public static bool IsMouseMiddleButtonDoubleClick (int clickDeltaFrame = 30) => Game.GlobalFrame < LastMouseMidButtonDownFrame.current + clickDeltaFrame && Game.GlobalFrame < LastMouseMidButtonDownFrame.prev + clickDeltaFrame;


	// Use
	/// <summary>
	/// True if the given mouse button is mark as used
	/// </summary>
	/// <param name="key">0 means left, 1 means right, 2 means middle</param>
	public static bool MouseKeyUsed (int key) => key switch {
		0 => MouseLeftState.Ignored,
		1 => MouseRightState.Ignored,
		2 => MouseMidState.Ignored,
		_ => false,
	};
	/// <summary>
	/// True if the given keyboard-key is mark as used
	/// </summary>
	public static bool KeyboardKeyUsed (KeyboardKey key) => KeyboardStateMap.TryGetValue(key, out var state) && state.Ignored;


	/// <summary>
	/// Mark given game-key as used so it will not be "down" or "holding" at current frame
	/// </summary>
	public static void UseGameKey (Gamekey key) => GamekeyStateMap[key].Ignored = true;
	/// <summary>
	/// Mark given keyboard-key as used so it will not be "down" or "holding" at current frame
	/// </summary>
	public static void UseKeyboardKey (KeyboardKey key) {
		if (KeyboardStateMap.TryGetValue(key, out var state)) {
			state.Ignored = true;
		}
	}
	/// <summary>
	/// Mark all current holding game-keys ans keyboard-keys as used so they will not be "down" or "holding" at current frame
	/// </summary>
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
	/// <summary>
	/// Mark all current holding mouse buttons as used so they will not be "down" or "holding" at current frame
	/// </summary>
	public static void UseAllMouseKey () {
		MouseLeftState.Ignored = true;
		MouseRightState.Ignored = true;
		MouseMidState.Ignored = true;
		_MouseWheelDelta = 0;
	}
	/// <summary>
	/// Mark given mouse button as used so it will not be "down" or "holding" at current frame
	/// </summary>
	public static void UseMouseKey (int index) {
		if (index == 0 && MouseLeftState.Holding) MouseLeftState.Ignored = true;
		if (index == 1 && MouseRightState.Holding) MouseRightState.Ignored = true;
		if (index == 2 && MouseMidState.Holding) MouseMidState.Ignored = true;
		if (index == 3) _MouseWheelDelta = 0;
	}


	/// <summary>
	/// Remove the used mark for given keyboard-key
	/// </summary>
	public static void UnuseKeyboardKey (KeyboardKey key) {
		if (KeyboardStateMap.TryGetValue(key, out var state)) {
			state.Ignored = false;
		}
	}
	/// <summary>
	/// Remove the used mark for given game-key
	/// </summary>
	public static void UnuseGameKey (Gamekey key) => GamekeyStateMap[key].Ignored = false;
	/// <summary>
	/// Remove the used mark for given mouse button
	/// </summary>
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
	/// <summary>
	/// Remove the used mark for all mouse button
	/// </summary>
	public static void UnuseAllMouseKey () {
		MouseLeftState.Ignored = false;
		MouseRightState.Ignored = false;
		MouseMidState.Ignored = false;
	}


	/// <summary>
	/// Ignore "mouse left to action and mouse right to jump" for given frames long
	/// </summary>
	public static void IgnoreMouseToActionJump (bool ignoreAction = true, bool ignoreJump = true, bool useMidButtonAsAction = false, int duration = 1) {
		if (ignoreAction) IgnoreMouseToActionFrame = Game.PauselessFrame + duration;
		if (ignoreJump) IgnoreMouseToJumpFrame = Game.PauselessFrame + duration;
		if (useMidButtonAsAction) MidMouseToActionFrame = Game.PauselessFrame + duration;
	}


	/// <summary>
	/// Ignore "gamepad right stick to control mouse wheel" for given frames long
	/// </summary>
	/// <param name="duration"></param>
	public static void IgnoreRightStickToMouseWheel (int duration = 1) => IgnoreRightStickToMouseWheelFrame = Game.PauselessFrame + duration;


	// Key Map
	/// <summary>
	/// Get which keyboard key is mapping into given game-key
	/// </summary>
	public static KeyboardKey GetKeyboardMap (Gamekey key) => (KeyboardKey)KeyMap[key].x;
	/// <summary>
	/// Get which keyboard key is default mapping key for given game-key
	/// </summary>
	public static KeyboardKey GetDefaultKeyboardMap (Gamekey key) => (KeyboardKey)KEYBOARD_DEFAULT[(int)key];
	/// <summary>
	/// Get which gamepad-button is mapping into given game-key
	/// </summary>
	public static GamepadKey GetGamepadMap (Gamekey key) => (GamepadKey)KeyMap[key].y;
	/// <summary>
	/// Get which gamepad-button is default mapping key for given game-key
	/// </summary>
	public static GamepadKey GetDefaultGamepadMap (Gamekey key) => (GamepadKey)GAMEPAD_DEFAULT[(int)key];


	/// <summary>
	/// Map given keyboard-key into given game-key and save it to disk
	/// </summary>
	public static void SetKeyboardMap (Gamekey gameKey, KeyboardKey keyboardKey) {
		var oldValue = KeyMap[gameKey];
		oldValue.x = (int)keyboardKey;
		KeyMap[gameKey] = oldValue;
		SaveInputToDisk();
	}
	/// <summary>
	/// Map given gamepad-button into given game-key and save it to disk
	/// </summary>
	public static void SetGamepadMap (Gamekey gameKey, GamepadKey gamepadKey) {
		var oldValue = KeyMap[gameKey];
		oldValue.y = (int)gamepadKey;
		KeyMap[gameKey] = oldValue;
		SaveInputToDisk();
	}


	// Mouse
	/// <summary>
	/// True if the given mouse button is holding at current frame
	/// </summary>
	/// <param name="button">0 means left, 1 means right, 2 means middle</param>
	public static bool MouseButtonHolding (int button) => !IgnoringKeyInput && button switch {
		0 => MouseLeftButtonHolding,
		1 => MouseRightButtonHolding,
		2 => MouseMidButtonHolding,
		_ => false,
	};


	/// <summary>
	/// Get index of the current holding mouse button. (order: left > right > middle)
	/// </summary>
	/// <returns>0 means left, 1 means right, 2 means middle, -1 means no button holding</returns>
	public static int GetHoldingMouseButton () {
		if (IgnoringKeyInput) return -1;
		if (MouseLeftButtonHolding) return 0;
		if (MouseRightButtonHolding) return 1;
		if (MouseMidButtonHolding) return 2;
		return -1;
	}


	// Misc
	/// <summary>
	/// Make all user input ignored by the system for given frames long
	/// </summary>
	public static void IgnoreAllInput (int duration = 0) {
		IgnoreMouseInput(duration);
		IgnoreKeyInput(duration);
	}

	/// <summary>
	/// Make all user mouse input ignored by the system for given frames long
	/// </summary>
	public static void IgnoreMouseInput (int duration = 0) {
		IgnoreMouseInputFrame = Game.GlobalFrame + duration;
	}

	/// <summary>
	/// Make user keyboard/game-key input ignored by the system for given frames long
	/// </summary>
	public static void IgnoreKeyInput (int duration = 0) => IgnoreKeyInputFrame = Game.GlobalFrame + duration;


	/// <summary>
	/// Do not ignore mouse input any more
	/// </summary>
	public static void CancelIgnoreMouseInput () => IgnoreMouseInputFrame = Game.GlobalFrame - 1;

	/// <summary>
	/// Do not ignore key input any more
	/// </summary>
	public static void CancelIgnoreKeyInput () => IgnoreKeyInputFrame = Game.GlobalFrame - 1;

	/// <summary>
	/// Shift mouse position for current frame. Only effect internal system not where the cursor appearingly is.
	/// </summary>
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
