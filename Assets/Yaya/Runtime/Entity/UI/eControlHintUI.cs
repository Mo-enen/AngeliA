using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;


namespace Yaya {
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.Order(Const.ENTITY_ORDER_UI + 1)]
	public class eControlHintUI : EntityUI {


		// Const
		private static readonly int KEYBOARD_BUTTON_CODE = "Keyboard Button".AngeHash();
		private static readonly int GAMEPAD_BUTTON_CODE = "Gamepad Button".AngeHash();
		private static readonly int BodyCode = "GamePad Body".AngeHash();
		private static readonly int DPadDownCode = "GamePad Down".AngeHash();
		private static readonly int DPadUpCode = "GamePad Up".AngeHash();
		private static readonly int DPadLeftCode = "GamePad Left".AngeHash();
		private static readonly int DPadRightCode = "GamePad Right".AngeHash();
		private static readonly int ButtonACode = "GamePad A".AngeHash();
		private static readonly int ButtonBCode = "GamePad B".AngeHash();
		private static readonly int ButtonSelectCode = "GamePad Select".AngeHash();
		private static readonly int ButtonStartCode = "GamePad Start".AngeHash();
		private static readonly Color32 DirectionTint = new(255, 255, 0, 255);
		private static readonly Color32 PressingTint = new(0, 255, 0, 255);
		private static readonly Color32 DarkButtonTint = new(0, 0, 0, 255);
		private static readonly Color32 ColorfulButtonTint = new(240, 86, 86, 255);

		// Api
		public int KeySize { get; set; } = 28;
		public int Gap { get; set; } = 6;
		public int TextSize { get; set; } = 23;
		public int OffsetX { get; set; } = 0;
		public int OffsetY { get; set; } = 0;
		public Color32 LabelTint { get; set; } = Const.WHITE;
		public Color32 KeyLabelTint { get; set; } = new Color32(44, 49, 54, 255);
		public static bool UseGamePadHint {
			get => ShowGamePadUI.Value;
			set => ShowGamePadUI.Value = value;
		}
		public static bool UseControlHint {
			get => ShowControlHint.Value;
			set => ShowControlHint.Value = value;
		}

		// Short
		private bool GamepadVisible => ShowGamePadUI.Value && Game.Current.State != GameState.Cutscene && Game.PauselessFrame > ForceHideGamepadFrame;
		private bool HintVisible => ShowControlHint.Value || Game.Current.State == GameState.Cutscene || Game.PauselessFrame <= ForceHintFrame;

		// Data
		private static readonly Dictionary<Key, int> KeyNameIdMap = new();
		private static readonly (int labelID, int priority, int frame)[] Hints = new (int, int, int)[8] {
			(0,int.MinValue,-1), (0,int.MinValue,-1), (0,int.MinValue,-1), (0,int.MinValue,-1),
			(0,int.MinValue,-1), (0,int.MinValue,-1), (0,int.MinValue,-1), (0,int.MinValue,-1),
		};
		private static eControlHintUI Current = null;
		private static int CurrentHintOffsetY = 0;
		private Int4 Border_Keyboard = default;
		private Int4 Border_Gamepad = default;
		private int ForceHintFrame = int.MinValue;
		private int ForceHideGamepadFrame = int.MinValue;
		private int OffsetResetFrame = int.MinValue;
		private readonly CellLabel HintLabel = new() {
			Alignment = Alignment.MidLeft,
		};

		// Saving
		private static readonly SavingBool ShowGamePadUI = new("Yaya.ShowGamePadUI", false);
		private static readonly SavingBool ShowControlHint = new("Yaya.ShowControlHint", true);


		// MSG
		[AfterGameInitialize]
		public static void Initialize () {

			Current = Game.Current.PeekOrGetEntity<eControlHintUI>();

			// Key Name Map
			KeyNameIdMap.Clear();
			foreach (var key in System.Enum.GetValues(typeof(Key))) {
				KeyNameIdMap.TryAdd((Key)key, $"k_{key}".AngeHash());
			}

		}


		public override void OnActived () {
			base.OnActived();
			if (CellRenderer.TryGetSprite(KEYBOARD_BUTTON_CODE, out var sprite)) {
				Border_Keyboard.Left = (int)(sprite.GlobalBorder.Left * ((float)KeySize / sprite.GlobalWidth));
				Border_Keyboard.Right = (int)(sprite.GlobalBorder.Right * ((float)KeySize / sprite.GlobalWidth));
				Border_Keyboard.Down = (int)(sprite.GlobalBorder.Down * ((float)KeySize / sprite.GlobalHeight));
				Border_Keyboard.Up = (int)(sprite.GlobalBorder.Up * ((float)KeySize / sprite.GlobalHeight));
			}
			if (CellRenderer.TryGetSprite(GAMEPAD_BUTTON_CODE, out sprite)) {
				Border_Gamepad.Left = (int)(sprite.GlobalBorder.Left * ((float)KeySize / sprite.GlobalWidth));
				Border_Gamepad.Right = (int)(sprite.GlobalBorder.Right * ((float)KeySize / sprite.GlobalWidth));
				Border_Gamepad.Down = (int)(sprite.GlobalBorder.Down * ((float)KeySize / sprite.GlobalHeight));
				Border_Gamepad.Up = (int)(sprite.GlobalBorder.Up * ((float)KeySize / sprite.GlobalHeight));
			}
		}


		protected override void FrameUpdateUI () {
			if (GamepadVisible) DrawGamePad();
			if (HintVisible) DrawHints();
			CurrentHintOffsetY = 0;
			if (OffsetResetFrame != int.MinValue && Game.PauselessFrame >= OffsetResetFrame) {
				OffsetResetFrame = int.MinValue;
				OffsetX = 0;
				OffsetY = 0;
			}
			// Switch Visible
			if (FrameInput.KeyboardDown(Key.F2)) {
				ShowGamePadUI.Value = !ShowGamePadUI.Value;
				ShowControlHint.Value = ShowGamePadUI.Value ? ShowControlHint.Value : !ShowControlHint.Value;
			}
		}


		private void DrawGamePad () {

			int x = Unify(6);
			int y = Unify(6);
			var rect = new RectInt(x, y, Unify(132), Unify(60));

			var DPadLeftPosition = new RectInt(Unify(10), Unify(22), Unify(12), Unify(8));
			var DPadRightPosition = new RectInt(Unify(22), Unify(22), Unify(12), Unify(8));
			var DPadDownPosition = new RectInt(Unify(18), Unify(14), Unify(8), Unify(12));
			var DPadUpPosition = new RectInt(Unify(18), Unify(26), Unify(8), Unify(12));
			var DPadCenterPos = new Vector2Int(Unify(22), Unify(26));
			var SelectPosition = new RectInt(Unify(44), Unify(20), Unify(12), Unify(4));
			var StartPosition = new RectInt(Unify(60), Unify(20), Unify(12), Unify(4));
			var ButtonAPosition = new RectInt(Unify(106), Unify(18), Unify(12), Unify(12));
			var ButtonBPosition = new RectInt(Unify(86), Unify(18), Unify(12), Unify(12));

			var screenRect = CellRenderer.CameraRect;

			// Body
			CellRenderer.Draw(BodyCode, rect.Shift(screenRect.x, screenRect.y));

			// DPad
			CellRenderer.Draw(DPadLeftCode, DPadLeftPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyHolding(Gamekey.Left) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadRightCode, DPadRightPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyHolding(Gamekey.Right) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadDownCode, DPadDownPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyHolding(Gamekey.Down) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadUpCode, DPadUpPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyHolding(Gamekey.Up) ? PressingTint : DarkButtonTint);

			// Direction
			if (FrameInput.UsingLeftStick) {
				var nDir = FrameInput.Direction;
				CellRenderer.Draw(
					Const.PIXEL, DPadCenterPos.x + x + screenRect.x, DPadCenterPos.y + y + screenRect.y,
					500, 0, (int)Vector3.SignedAngle(Vector3.up, (Vector2)nDir, Vector3.back),
					Unify(3),
					Unify(nDir.magnitude / 50f),
					DirectionTint
				);
			}

			// Func
			CellRenderer.Draw(ButtonSelectCode, SelectPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyHolding(Gamekey.Select) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(ButtonStartCode, StartPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyHolding(Gamekey.Start) ? PressingTint : DarkButtonTint);

			// Buttons
			CellRenderer.Draw(ButtonACode, ButtonAPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyHolding(Gamekey.Action) ? PressingTint : ColorfulButtonTint);
			CellRenderer.Draw(ButtonBCode, ButtonBPosition.Shift(x, y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyHolding(Gamekey.Jump) ? PressingTint : ColorfulButtonTint);

		}


		private void DrawHints () {

			int hintPositionY =
				CurrentHintOffsetY + OffsetY + CellRenderer.CameraRect.y + Unify(GamepadVisible ? 78 : 12);

			// Menu
			if (MenuUI.CurrentMenu != null) {
				if (MenuUI.CurrentMenu.SelectionAdjustable) {
					AddHint(Gamekey.Left, Gamekey.Right, WORD.HINT_ADJUST);
				} else {
					AddHint(Gamekey.Action, WORD.HINT_USE);
				}
				AddHint(Gamekey.Down, Gamekey.Up, WORD.HINT_MOVE);
			}

			// Draw
			int x = CellRenderer.CameraRect.x + OffsetX + Unify(12);
			Draw(Gamekey.Down);
			Draw(Gamekey.Up);
			Draw(Gamekey.Left);
			Draw(Gamekey.Right);
			Draw(Gamekey.Action);
			Draw(Gamekey.Jump);
			Draw(Gamekey.Select);
			Draw(Gamekey.Start);

			// Func
			void Draw (Gamekey keyA) {
				int index = (int)keyA;
				var (labelID, _, frame) = Hints[index];
				if (frame != Game.PauselessFrame) return;
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
					Hints[(int)keyB].labelID == labelID
				) {
					Hints[(int)keyB].frame = -1;
				} else {
					keyB = keyA;
				}
				DrawKey(x, y, keyA, keyB, labelID);
				hintPositionY += Unify(KeySize + Gap);
			}
		}


		// API
		public static void AddHint (Gamekey key, int labelID, int priority = int.MinValue) => AddHint(key, key, labelID, priority);
		public static void AddHint (Gamekey keyA, Gamekey keyB, int labelID, int priority = int.MinValue) => Current?.AddHintLogic(keyA, keyB, labelID, priority);
		public static void AddHint (Key key, int labelID) => AddHint(key, key, labelID);
		public static void AddHint (Key keyA, Key keyB, int labelID) {
			if (Current == null || !Current.HintVisible) return;
			int x =
				CellRenderer.CameraRect.x +
				Current.OffsetX +
				Unify(12);
			int y =
				CurrentHintOffsetY +
				Current.OffsetY +
				CellRenderer.CameraRect.y +
				Unify(Current.GamepadVisible ? 78 : 12);
			Current.DrawKey(x, y, keyA, keyB, labelID);
			CurrentHintOffsetY += Unify(Current.KeySize + Current.Gap);
		}


		public static void DrawGlobalHint (int globalX, int globalY, Gamekey key, int labelID, bool background = false) => Current?.DrawKey(globalX, globalY, key, key, labelID, background);
		public static void DrawGlobalHint (int globalX, int globalY, Gamekey keyA, Gamekey keyB, int labelID, bool background = false) => Current?.DrawKey(globalX, globalY, keyA, keyB, labelID, background);
		public static void DrawGlobalHint (int globalX, int globalY, Key key, int labelID, bool background = false) => Current?.DrawKey(globalX, globalY, key, key, labelID, background);
		public static void DrawGlobalHint (int globalX, int globalY, Key keyA, Key keyB, int labelID, bool background = false) => Current?.DrawKey(globalX, globalY, keyA, keyB, labelID, background);
		public static void DrawGlobalHint (int globalX, int globalY, GamepadButton key, int labelID, bool background = false) => Current?.DrawKey(globalX, globalY, key, key, labelID, background);
		public static void DrawGlobalHint (int globalX, int globalY, GamepadButton keyA, GamepadButton keyB, int labelID, bool background = false) => Current?.DrawKey(globalX, globalY, keyA, keyB, labelID, background);


		public static void ForceShowHint (int duration = 1) {
			if (Current == null) return;
			Current.ForceHintFrame = Game.PauselessFrame + duration;
		}


		public static void ForceHideGamepad (int duration = 1) {
			if (Current == null) return;
			Current.ForceHideGamepadFrame = Game.PauselessFrame + duration;
		}


		public static void ForceOffset (int x, int y, int duration = 1) {
			if (Current == null) return;
			Current.OffsetResetFrame = Game.PauselessFrame + duration;
			Current.OffsetX = x;
			Current.OffsetY = y;
		}


		// LGC
		private void AddHintLogic (Gamekey keyA, Gamekey keyB, int labelID, int priority = int.MinValue) {
			if (!HintVisible) return;
			if (Hints[(int)keyA].frame != Game.PauselessFrame || priority >= Hints[(int)keyA].priority) {
				Hints[(int)keyA] = (labelID, priority, Game.PauselessFrame);
			}
			if (Hints[(int)keyB].frame != Game.PauselessFrame || priority >= Hints[(int)keyB].priority) {
				Hints[(int)keyB] = (labelID, priority, Game.PauselessFrame);
			}
		}


		private void DrawKey (int x, int y, Gamekey keyA, Gamekey keyB, int labelID, bool background = false) {
			if (FrameInput.UsingGamepad) {
				DrawKey(x, y, FrameInput.GetGamepadMap(keyA), FrameInput.GetGamepadMap(keyB), labelID, background);
			} else {
				DrawKey(x, y, FrameInput.GetKeyboardMap(keyA), FrameInput.GetKeyboardMap(keyB), labelID, background);
			}
		}
		private void DrawKey (int x, int y, GamepadButton buttonA, GamepadButton buttonB, int labelID, bool background = false) {
			int keyIdA = YayaConst.GAMEPAD_CODE.TryGetValue(buttonA, out int _value0) ? _value0 : 0;
			int keyIdB = YayaConst.GAMEPAD_CODE.TryGetValue(buttonB, out int _value1) ? _value1 : 0;
			DrawKeyLogic(x, y, keyIdA, keyIdB, labelID, background);
		}
		private void DrawKey (int x, int y, Key keyA, Key keyB, int labelID, bool background = false) {
			int keyIdA = KeyNameIdMap[keyA];
			int keyIdB = KeyNameIdMap[keyB];
			DrawKeyLogic(x, y, keyIdA, keyIdB, labelID, background);
		}
		private void DrawKeyLogic (int x, int y, int keyIdA, int keyIdB, int labelID, bool background = false) {

			const int BG_PADDING_X = 32;
			const int BG_PADDING_Y = 32;
			Cell bgCell = null;
			int buttonCode = FrameInput.UsingGamepad ? GAMEPAD_BUTTON_CODE : KEYBOARD_BUTTON_CODE;
			var border = FrameInput.UsingGamepad ? Border_Gamepad : Border_Keyboard;
			border.Left = Unify(border.Left);
			border.Right = Unify(border.Right);
			border.Down = Unify(border.Down);
			border.Up = Unify(border.Up);
			int gap = Unify(Gap);
			int keySize = Unify(KeySize);
			var rect = new RectInt(x, y, keySize, keySize);
			int widthA = keySize;
			int widthB = keySize;

			// Fix Width for A
			if (CellRenderer.TryGetSprite(keyIdA, out var spriteA) && spriteA.GlobalWidth > spriteA.GlobalHeight) {
				widthA = ((keySize - border.Vertical) * ((float)spriteA.GlobalWidth / spriteA.GlobalHeight)).RoundToInt();
				widthA += border.Horizontal;
			}
			if (CellRenderer.TryGetSprite(keyIdB, out var spriteB) && spriteB.GlobalWidth > spriteB.GlobalHeight) {
				widthB = ((keySize - border.Vertical) * ((float)spriteB.GlobalWidth / spriteB.GlobalHeight)).RoundToInt();
				widthB += border.Horizontal;
			}

			// Draw
			int oldLayer = CellRenderer.CurrentLayerIndex;
			CellRenderer.SetLayerToUI();

			try {

				rect.width = widthA;
				if (background) {
					bgCell = CellRenderer.Draw(Const.PIXEL, rect.Expand(BG_PADDING_X), new(12, 12, 12, 255));
					bgCell.Z = 0;
				}
				// Button A
				CellRenderer.Draw_9Slice(buttonCode, rect, border.Left, border.Right, border.Down, border.Up);

				// Button Label A
				CellRenderer.Draw(keyIdA, rect.Shrink(border), KeyLabelTint);
				rect.x += rect.width + gap;

				// Button B
				if (keyIdA != keyIdB) {
					// Button B
					rect.width = widthB;
					CellRenderer.Draw_9Slice(buttonCode, rect, border.Left, border.Right, border.Down, border.Up);
					// Button Label B
					CellRenderer.Draw(keyIdB, rect.Shrink(border), KeyLabelTint);
					rect.x += rect.width + gap;
				}

				// Label
				rect.width = 1;

				HintLabel.Text = Language.Get(labelID);
				HintLabel.Tint = LabelTint;
				HintLabel.CharSize = Unify(TextSize);

				CellRendererGUI.Label(
					HintLabel, rect, out var bounds
				);
				if (bgCell != null) {
					bgCell.Y = Mathf.Min(bgCell.Y, bounds.y - BG_PADDING_Y);
					bgCell.Width = Mathf.Max(bgCell.Width, bounds.xMax - bgCell.X + BG_PADDING_X);
					bgCell.Height = Mathf.Max(bgCell.Height, bounds.yMax - bgCell.Y + BG_PADDING_Y);
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }

			CellRenderer.SetLayer(oldLayer);

		}


	}
}