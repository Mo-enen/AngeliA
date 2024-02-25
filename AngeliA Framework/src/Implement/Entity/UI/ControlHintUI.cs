using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
[EntityAttribute.DontDestroyOutOfRange]
[EntityAttribute.DontDestroyOnSquadTransition]
[EntityAttribute.StageOrder(int.MaxValue)]
public class ControlHintUI : EntityUI {


	// Const
	private static readonly int HINT_BUTTON_CODE = BuiltInSprite.HINT_BUTTON;
	private static readonly int BodyCode = BuiltInSprite.GAMEPAD_BODY;
	private static readonly int DPadDownCode = BuiltInSprite.GAMEPAD_HINT_DOWN;
	private static readonly int DPadUpCode = BuiltInSprite.GAMEPAD_HINT_UP;
	private static readonly int DPadLeftCode = BuiltInSprite.GAMEPAD_HINT_LEFT;
	private static readonly int DPadRightCode = BuiltInSprite.GAMEPAD_HINT_RIGHT;
	private static readonly int ButtonACode = BuiltInSprite.GAMEPAD_HINT_A;
	private static readonly int ButtonBCode = BuiltInSprite.GAMEPAD_HINT_B;
	private static readonly int ButtonSelectCode = BuiltInSprite.GAMEPAD_HINT_SELECT;
	private static readonly int ButtonStartCode = BuiltInSprite.GAMEPAD_HINT_START;
	private static readonly Color32 DirectionTint = new(255, 255, 0, 255);
	private static readonly Color32 PressingTint = new(0, 255, 0, 255);
	private static readonly Color32 DarkButtonTint = new(0, 0, 0, 255);
	private static readonly Color32 ColorfulButtonTint = new(240, 86, 86, 255);
	private static readonly Color32 LabelTint = Color32.WHITE;
	private static readonly Color32 KeyTint = new(44, 49, 54, 255);
	private const int KEYSIZE = 24;
	private const int GAP = 4;
	private const int TEXT_SIZE = 21;

	// Api
	public static ControlHintUI Instance { get; private set; } = null;
	public static bool UseGamePadHint {
		get => ShowGamePadUI.Value;
		set => ShowGamePadUI.Value = value;
	}
	public static bool UseControlHint {
		get => ShowControlHint.Value;
		set => ShowControlHint.Value = value;
	}
	public int OffsetX { get; set; } = 0;
	public int OffsetY { get; set; } = 0;

	// Short
	private bool GamepadVisible => ShowGamePadUI.Value && Game.PauselessFrame > ForceHideGamepadFrame;
	private bool HintVisible => ShowControlHint.Value || Game.PauselessFrame <= ForceHintFrame;

	// Data
	private static readonly (string label, int priority, int frame)[] Hints = new (string, int, int)[8] {
		("",int.MinValue,-1), ("",int.MinValue,-1), ("",int.MinValue,-1), ("",int.MinValue,-1),
		("",int.MinValue,-1), ("",int.MinValue,-1), ("",int.MinValue,-1), ("",int.MinValue,-1),
	};
	private static int CurrentHintOffsetY = 0;
	private Int4 ButtonBorder = default;
	private int ForceHintFrame = int.MinValue;
	private int ForceHideGamepadFrame = int.MinValue;
	private int OffsetResetFrame = int.MinValue;
	private static readonly TextContent HintLabel = new() { Alignment = Alignment.MidLeft, CharSize = TEXT_SIZE, Tint = LabelTint, };
	private static readonly TextContent KeyLabel = new() { Alignment = Alignment.MidLeft, CharSize = TEXT_SIZE, Tint = KeyTint, };
	private static readonly TextContent FPSLabel = new() { Alignment = Alignment.TopRight, CharSize = 20, Tint = Color32.WHITE, Shadow = Color32.BLACK, FromString = false, ShadowOffset = 3, };
	private static float GameFPS = 1f;
	private static readonly IntToChars FPS = new();

	// Saving
	private static readonly SavingBool ShowGamePadUI = new("Hint.ShowGamePadUI", false);
	private static readonly SavingBool ShowControlHint = new("Hint.ShowControlHint", true);


	// MSG
	[OnGameInitializeLater(64)]
	public static void Initialize () => Stage.SpawnEntity<ControlHintUI>(0, 0);


	[OnGameUpdatePauseless]
	public static void OnGameUpdateLater () {
		// FPS
		if (Game.ShowFPS) {
			if (Game.PauselessFrame % 12 == 0) {
				GameFPS = Game.CurrentFPS;
			}
			int padding = Unify(6);
			int width = Unify(40);
			int height = Unify(24);
			FPSLabel.Chars = FPS.GetChars(GameFPS.RoundToInt());
			GUI.Label(
				FPSLabel,
				new IRect(
					Renderer.CameraRect.xMax - width - padding,
					Renderer.CameraRect.yMax - height - padding,
					width, height
				)
			);
		}
		// Draw Hints
		if (Instance != null) {
			if (Instance.GamepadVisible) Instance.DrawGamePad();
			if (Instance.HintVisible) Instance.DrawHints();
			CurrentHintOffsetY = 0;
			if (Instance.OffsetResetFrame != int.MinValue && Game.PauselessFrame >= Instance.OffsetResetFrame) {
				Instance.OffsetResetFrame = int.MinValue;
				Instance.OffsetX = 0;
				Instance.OffsetY = 0;
			}
		}
	}


	public ControlHintUI () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();
		if (Renderer.TryGetSprite(HINT_BUTTON_CODE, out var sprite)) {
			ButtonBorder.left = (int)(sprite.GlobalBorder.left * ((float)KEYSIZE / sprite.GlobalWidth));
			ButtonBorder.right = (int)(sprite.GlobalBorder.right * ((float)KEYSIZE / sprite.GlobalWidth));
			ButtonBorder.down = (int)(sprite.GlobalBorder.down * ((float)KEYSIZE / sprite.GlobalHeight));
			ButtonBorder.up = (int)(sprite.GlobalBorder.up * ((float)KEYSIZE / sprite.GlobalHeight));
		}
	}


	private void DrawGamePad () {

		int x = Unify(6);
		int y = Unify(6);
		var rect = new IRect(x, y, Unify(132), Unify(60));

		var DPadLeftPosition = new IRect(Unify(10), Unify(22), Unify(12), Unify(8));
		var DPadRightPosition = new IRect(Unify(22), Unify(22), Unify(12), Unify(8));
		var DPadDownPosition = new IRect(Unify(18), Unify(14), Unify(8), Unify(12));
		var DPadUpPosition = new IRect(Unify(18), Unify(26), Unify(8), Unify(12));
		var DPadCenterPos = new Int2(Unify(22), Unify(26));
		var SelectPosition = new IRect(Unify(44), Unify(20), Unify(12), Unify(4));
		var StartPosition = new IRect(Unify(60), Unify(20), Unify(12), Unify(4));
		var ButtonAPosition = new IRect(Unify(106), Unify(18), Unify(12), Unify(12));
		var ButtonBPosition = new IRect(Unify(86), Unify(18), Unify(12), Unify(12));

		var screenRect = Renderer.CameraRect;

		int oldLayer = Renderer.CurrentLayerIndex;
		Renderer.SetLayerToUI();

		// Body
		Renderer.Draw(BodyCode, rect.Shift(screenRect.x, screenRect.y));

		// DPad
		Renderer.Draw(DPadLeftCode, DPadLeftPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), Input.GameKeyHolding(Gamekey.Left) ? PressingTint : DarkButtonTint);
		Renderer.Draw(DPadRightCode, DPadRightPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), Input.GameKeyHolding(Gamekey.Right) ? PressingTint : DarkButtonTint);
		Renderer.Draw(DPadDownCode, DPadDownPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), Input.GameKeyHolding(Gamekey.Down) ? PressingTint : DarkButtonTint);
		Renderer.Draw(DPadUpCode, DPadUpPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), Input.GameKeyHolding(Gamekey.Up) ? PressingTint : DarkButtonTint);

		// Direction
		if (Input.UsingLeftStick) {
			var nDir = Input.Direction;
			Renderer.Draw(
				Const.PIXEL, DPadCenterPos.x + x + screenRect.x, DPadCenterPos.y + y + screenRect.y,
				500, 0, (int)Float3.SignedAngle(Float3.up, (Float2)nDir, Float3.back),
				Unify(3),
				Unify(nDir.magnitude / 50f),
				DirectionTint
			);
		}

		// Func
		Renderer.Draw(ButtonSelectCode, SelectPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), Input.GameKeyHolding(Gamekey.Select) ? PressingTint : DarkButtonTint);
		Renderer.Draw(ButtonStartCode, StartPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), Input.GameKeyHolding(Gamekey.Start) ? PressingTint : DarkButtonTint);

		// Buttons
		Renderer.Draw(ButtonACode, ButtonAPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), Input.GameKeyHolding(Gamekey.Action) ? PressingTint : ColorfulButtonTint);
		Renderer.Draw(ButtonBCode, ButtonBPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), Input.GameKeyHolding(Gamekey.Jump) ? PressingTint : ColorfulButtonTint);

		Renderer.SetLayer(oldLayer);
	}


	private void DrawHints () {

		int hintPositionY = CurrentHintOffsetY + OffsetY + Renderer.CameraRect.y + Unify(GamepadVisible ? 78 : 12);

		// Draw
		int x = Renderer.CameraRect.x + OffsetX + Unify(12);
		Draw(Gamekey.Start);
		Draw(Gamekey.Select);
		Draw(Gamekey.Down);
		Draw(Gamekey.Up);
		Draw(Gamekey.Left);
		Draw(Gamekey.Right);
		Draw(Gamekey.Action);
		Draw(Gamekey.Jump);

		// Func
		void Draw (Gamekey keyA) {
			int index = (int)keyA;
			var (label, _, frame) = Hints[index];
			if (frame != Game.PauselessFrame) return;
			if (string.IsNullOrEmpty(label)) return;
			int y = hintPositionY;
			var keyB = keyA switch {
				Gamekey.Left => Gamekey.Right,
				Gamekey.Right => Gamekey.Left,
				Gamekey.Down => Gamekey.Up,
				Gamekey.Up => Gamekey.Down,
				Gamekey.Jump => Gamekey.Action,
				Gamekey.Action => Gamekey.Jump,
				Gamekey.Start => Gamekey.Select,
				Gamekey.Select => Gamekey.Start,
				_ => keyA,
			};
			if (
				keyA != keyB &&
				Hints[(int)keyB].frame == Game.PauselessFrame &&
				Hints[(int)keyB].label == label
			) {
				Hints[(int)keyB].frame = -1;
			} else {
				keyB = keyA;
			}
			DrawGamekey(x, y, keyA, keyB, label);
			hintPositionY += Unify(KEYSIZE + GAP);
		}
	}


	// API
	public static void AddHint (Gamekey key, string label, int priority = int.MinValue) => Instance?.AddHintLogic(key, key, label, priority);
	public static void AddHint (Gamekey keyA, Gamekey keyB, string label, int priority = int.MinValue) => Instance?.AddHintLogic(keyA, keyB, label, priority);
	internal static void AddHint (KeyboardKey key, string label) => AddHint(key, key, label);
	internal static void AddHint (KeyboardKey keyA, KeyboardKey keyB, string label) {
		if (Instance == null || !Instance.HintVisible) return;
		int x =
			Renderer.CameraRect.x +
			Instance.OffsetX +
			Unify(12);
		int y =
			CurrentHintOffsetY +
			Instance.OffsetY +
			Renderer.CameraRect.y +
			Unify(Instance.GamepadVisible ? 78 : 12);
		Instance.DrawKey(x, y, keyA, keyB, label);
		CurrentHintOffsetY += Unify(KEYSIZE + GAP);
	}


	public static void DrawGlobalHint (int globalX, int globalY, Gamekey key, string label, bool background = false) => Instance?.DrawGamekey(globalX, globalY, key, key, label, background);
	public static void DrawGlobalHint (int globalX, int globalY, Gamekey keyA, Gamekey keyB, string label, bool background = false) => Instance?.DrawGamekey(globalX, globalY, keyA, keyB, label, background);
	internal static void DrawGlobalHint (int globalX, int globalY, KeyboardKey key, string label, bool background = false) => Instance?.DrawKey(globalX, globalY, key, key, label, background);
	internal static void DrawGlobalHint (int globalX, int globalY, KeyboardKey keyA, KeyboardKey keyB, string label, bool background = false) => Instance?.DrawKey(globalX, globalY, keyA, keyB, label, background);
	internal static void DrawGlobalHint (int globalX, int globalY, GamepadKey key, string label, bool background = false) => Instance?.DrawGamepadButton(globalX, globalY, key, key, label, background);
	internal static void DrawGlobalHint (int globalX, int globalY, GamepadKey keyA, GamepadKey keyB, string label, bool background = false) => Instance?.DrawGamepadButton(globalX, globalY, keyA, keyB, label, background);


	public static void ForceShowHint (int duration = 1) {
		if (Instance == null) return;
		Instance.ForceHintFrame = Game.PauselessFrame + duration;
	}


	public static void ForceHideGamepad (int duration = 1) {
		if (Instance == null) return;
		Instance.ForceHideGamepadFrame = Game.PauselessFrame + duration;
	}


	public static void ForceOffset (int x, int y, int duration = 1) {
		if (Instance == null) return;
		Instance.OffsetResetFrame = Game.PauselessFrame + duration;
		Instance.OffsetX = x;
		Instance.OffsetY = y;
	}


	// LGC
	private void AddHintLogic (Gamekey keyA, Gamekey keyB, string label, int priority = int.MinValue) {
		if (!HintVisible) return;
		if (Hints[(int)keyA].frame != Game.PauselessFrame || priority >= Hints[(int)keyA].priority) {
			Hints[(int)keyA] = (label, priority, Game.PauselessFrame);
		}
		if (Hints[(int)keyB].frame != Game.PauselessFrame || priority >= Hints[(int)keyB].priority) {
			Hints[(int)keyB] = (label, priority, Game.PauselessFrame);
		}
	}


	private void DrawGamekey (int x, int y, Gamekey keyA, Gamekey keyB, string label, bool background = false) {
		if (Input.UsingGamepad) {
			DrawGamepadButton(x, y, Input.GetGamepadMap(keyA), Input.GetGamepadMap(keyB), label, background);
		} else {
			DrawKey(x, y, Input.GetKeyboardMap(keyA), Input.GetKeyboardMap(keyB), label, background);
		}
	}
	private void DrawGamepadButton (int x, int y, GamepadKey buttonA, GamepadKey buttonB, string label, bool background = false) {
		int keyIdA = FrameworkUtil.GAMEPAD_CODE.TryGetValue(buttonA, out int _value0) ? _value0 : 0;
		int keyIdB = FrameworkUtil.GAMEPAD_CODE.TryGetValue(buttonB, out int _value1) ? _value1 : 0;
		DrawKeyLogic(x, y, keyIdA, keyIdB, "", "", label, background);
	}
	private void DrawKey (int x, int y, KeyboardKey keyA, KeyboardKey keyB, string label, bool background = false) {
		DrawKeyLogic(x, y, 0, 0, Util.GetKeyDisplayName(keyA), Util.GetKeyDisplayName(keyB), label, background);
	}
	private void DrawKeyLogic (int x, int y, int keyIdA, int keyIdB, string keyTextA, string keyTextB, string label, bool background) {

		const int BG_PADDING_X = 32;
		const int BG_PADDING_Y = 32;
		Cell bgCell = null;
		var border = ButtonBorder;
		border.left = Unify(border.left);
		border.right = Unify(border.right);
		border.down = Unify(border.down);
		border.up = Unify(border.up);
		int gap = Unify(GAP);
		int keySize = Unify(KEYSIZE);
		var rect = new IRect(x, y, keySize, keySize);
		int widthA = keySize;
		int widthB = keySize;

		// Fix Width
		if (keyIdA != 0) {
			if (Renderer.TryGetSprite(keyIdA, out var spriteA) && spriteA.GlobalWidth > spriteA.GlobalHeight) {
				widthA = ((keySize - border.vertical) * ((float)spriteA.GlobalWidth / spriteA.GlobalHeight)).RoundToInt();
				widthA += border.horizontal;
			}
		}
		if (keyIdB != 0) {
			if (Renderer.TryGetSprite(keyIdB, out var spriteB) && spriteB.GlobalWidth > spriteB.GlobalHeight) {
				widthB = ((keySize - border.vertical) * ((float)spriteB.GlobalWidth / spriteB.GlobalHeight)).RoundToInt();
				widthB += border.horizontal;
			}
		}

		// Draw
		int oldLayer = Renderer.CurrentLayerIndex;
		Renderer.SetLayerToUI();

		rect.width = widthA;
		if (background) {
			bgCell = Renderer.Draw(Const.PIXEL, rect.Expand(BG_PADDING_X), new(12, 12, 12, 255), 0);
		}

		// Button A
		if (keyIdA != 0) {
			Renderer.Draw_9Slice(
				HINT_BUTTON_CODE, rect, border.left, border.right, border.down, border.up, int.MaxValue - 1
			);
			Renderer.Draw(keyIdA, rect.Shrink(border), KeyTint, int.MaxValue);
		} else {
			KeyLabel.Text = keyTextA;
			GUI.Label(KeyLabel, rect.Shrink(border), out var keyBounds);
			int targetWidth = keyBounds.width + border.horizontal;
			if (rect.width < targetWidth) rect.width = targetWidth;
			Renderer.Draw_9Slice(
				HINT_BUTTON_CODE, rect, border.left, border.right, border.down, border.up, int.MaxValue
			);
		}
		rect.x += rect.width + gap;

		// Button B
		if (keyIdA != keyIdB || keyTextA != keyTextB) {
			// Button B
			rect.width = widthB;
			if (keyIdB != 0) {
				Renderer.Draw_9Slice(
					HINT_BUTTON_CODE, rect, border.left, border.right, border.down, border.up, int.MaxValue - 1
				);
				Renderer.Draw(keyIdB, rect.Shrink(border), KeyTint, int.MaxValue);
			} else {
				KeyLabel.Text = keyTextB;
				GUI.Label(KeyLabel, rect.Shrink(border), out var keyBounds);
				int targetWidth = keyBounds.width + border.horizontal;
				if (rect.width < targetWidth) rect.width = targetWidth;
				Renderer.Draw_9Slice(
					HINT_BUTTON_CODE, rect, border.left, border.right, border.down, border.up, int.MaxValue
				);
			}
			rect.x += rect.width + gap;
		}

		// Label
		rect.width = 1;

		HintLabel.Text = label;

		GUI.Label(
			HintLabel, rect, out var bounds
		);
		if (bgCell != null) {
			bgCell.Y = Util.Min(bgCell.Y, bounds.y - BG_PADDING_Y);
			bgCell.Width = Util.Max(bgCell.Width, bounds.xMax - bgCell.X + BG_PADDING_X);
			bgCell.Height = Util.Max(bgCell.Height, bounds.yMax - bgCell.Y + BG_PADDING_Y);
		}

		Renderer.SetLayer(oldLayer);

	}


}