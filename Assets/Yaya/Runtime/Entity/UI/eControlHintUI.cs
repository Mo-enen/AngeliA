using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;


namespace Yaya {
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public class eControlHintUI : UI, IInitialize {


		// Const
		private static readonly int KEYBOARD_BUTTON_ANI_CODE = "Keyboard Button".AngeHash();
		private static readonly int GAMEPAD_BUTTON_ANI_CODE = "Gamepad Button".AngeHash();
		private static readonly int KEYBOARD_BUTTON_CODE = "Keyboard Button".AngeHash();
		private static readonly int GAMEPAD_BUTTON_CODE = "Gamepad Button".AngeHash();

		// Api
		public ePlayer Player { get; set; } = null;
		public int KeySize { get; set; } = 142;
		public int Gap { get; set; } = 32;
		public int TextSize { get; set; } = 100;
		public Color32 LabelTint { get; set; } = Const.WHITE;
		public Color32 KeyLabelTint { get; set; } = new Color32(44, 49, 54, 255);

		// Data
		private static readonly Dictionary<int, int> TypeHintMap = new();
		private static readonly List<Entity> Listening = new();
		private static readonly Dictionary<Key, int> KeyNameIdMap = new();
		private int PositionY = 0;
		private Int4 Border_Keyboard = default;
		private Int4 Border_Gamepad = default;


		// MSG
		public static void Initialize () {
			TypeHintMap.Clear();
			var objType = typeof(object);
			var entityType = typeof(Entity);
			foreach (var type in typeof(IActionEntity).AllClassImplemented()) {
				var _type = type;
				string name = _type.Name;
				if (name[0] == 'e') name = name[1..];
				int id = $"ActionHint.{name}".AngeHash();
				while (!Language.Has(id) && _type != entityType && _type != objType) {
					_type = _type.BaseType;
					name = _type.Name;
					if (name[0] == 'e') name = name[1..];
					id = $"ActionHint.{name}".AngeHash();
				}
				if (Language.Has(id)) {
					TypeHintMap.TryAdd(type.AngeHash(), id);
				}
			}
			// Listening
			Listening.Clear();
			foreach (var type in typeof(MenuUI).AllChildClass()) {
				int id = type.AngeHash();
				var e = Game.Current.PeekOrGetEntity(id);
				if (e == null) continue;
				Listening.Add(e);
			}
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


			PositionY = Y + CellRenderer.CameraRect.y;

			// Cutscene
			if (Game.Current.State == GameState.Cutscene) {
				if (Cutscene.IsPlayingTask || Game.GlobalFrame > Cutscene.StartFrame + Const.CUTSCENE_FADEOUT_DURATION) {
					DrawKey(GameKey.Start, WORD.HINT_SKIP_CODE);
				}
				return;
			}

			// Listening Hint
			foreach (var e in Listening) {
				if (!e.Active) continue;
				switch (e) {
					case MenuUI menu:
						DrawKey(GameKey.Down, GameKey.Up, WORD.HINT_MOVE_CODE);
						if (menu.SelectionAdjustable) {
							DrawKey(GameKey.Left, GameKey.Right, WORD.HINT_VALUE_CODE);
						} else {
							DrawKey(GameKey.Action, WORD.HINT_USE_CODE);
						}
						break;
				}
			}

			if (Player == null || !Player.Active) return;
			if (Game.Current.State != GameState.Play) return;
			if (FrameTask.IsTasking(YayaConst.TASK_ROUTE)) return;

			// Game Playing
			switch (Player.CharacterState) {

				case CharacterState.GamePlay: {
					// Move
					DrawKey(GameKey.Left, GameKey.Right, WORD.HINT_MOVE_CODE);
					// Action & Jump
					if (Player.Action.CurrentTarget is Entity target && target is IActionEntity iAct) {
						// Action Target
						if (target is eOpenableFurniture open && open.Open) {
							DrawKey(iAct.InvokeKey, WORD.UI_OK);
							DrawKey(GameKey.Jump, WORD.UI_CANCEL);
						} else {
							if (TypeHintMap.TryGetValue(target.TypeID, out int code)) {
								DrawKey(iAct.InvokeKey, code);
							} else {
								DrawKey(iAct.InvokeKey, WORD.HINT_USE_CODE);
							}
						}
					} else {
						// General
						DrawKey(GameKey.Action, WORD.HINT_ATTACK_CODE);
						DrawKey(GameKey.Jump, WORD.HINT_JUMP_CODE);
					}
					break;
				}

				case CharacterState.Sleep: {
					int x = Player.GlobalBounds.xMin;
					int y = Player.GlobalBounds.yMax;
					DrawKey(x, y, GameKey.Action, WORD.HINT_WAKE_CODE, true, true);
					DrawKey(GameKey.Action, WORD.HINT_WAKE_CODE);
					break;
				}

				case CharacterState.Passout: {
					if (Game.GlobalFrame < Player.PassoutFrame + YayaConst.PASSOUT_WAIT) break;
					int x = Player.GlobalBounds.xMin;
					int y = Player.GlobalBounds.yMax;
					DrawKey(x, y, GameKey.Action, WORD.UI_CONTINUE, true, true);
					DrawKey(GameKey.Action, WORD.UI_CONTINUE);
					break;
				}

			}

		}


		private void DrawKey (GameKey key, int labelID) => DrawKey(key, key, labelID);
		private void DrawKey (GameKey keyA, GameKey keyB, int labelID) {
			int x = X + CellRenderer.CameraRect.x;
			int y = PositionY;
			DrawKey(x, y, keyA, keyB, labelID);
			PositionY += KeySize + Gap;
		}
		private void DrawKey (int x, int y, GameKey key, int labelID, bool background = false, bool animated = false) => DrawKey(x, y, key, key, labelID, background, animated);
		private void DrawKey (int x, int y, GameKey keyA, GameKey keyB, int labelID, bool background = false, bool animated = false) {

			var keyboardA = FrameInput.GetKeyboardMap(keyA);
			var keyboardB = FrameInput.GetKeyboardMap(keyB);
			var gamepadA = FrameInput.GetGamepadMap(keyA);
			var gamepadB = FrameInput.GetGamepadMap(keyB);

			const int BG_PADDING_X = 32;
			const int BG_PADDING_Y = 32;
			Cell bgCell = null;
			int buttonCode = animated ?
				FrameInput.UsingGamepad ? GAMEPAD_BUTTON_ANI_CODE : KEYBOARD_BUTTON_ANI_CODE :
				FrameInput.UsingGamepad ? GAMEPAD_BUTTON_CODE : KEYBOARD_BUTTON_CODE;
			var border = FrameInput.UsingGamepad ? Border_Gamepad : Border_Keyboard;
			var rect = new RectInt(x, y, KeySize, KeySize);
			int keyIdA = FrameInput.UsingGamepad ? (YayaConst.GAMEPAD_CODE.TryGetValue(gamepadA, out var _value0) ? _value0 : 0) : KeyNameIdMap[keyboardA];
			int keyIdB = FrameInput.UsingGamepad ? (YayaConst.GAMEPAD_CODE.TryGetValue(gamepadB, out var _value1) ? _value1 : 0) : KeyNameIdMap[keyboardB];
			int widthA = KeySize;
			int widthB = KeySize;

			// Fix Width for A
			if (CellRenderer.TryGetSprite(keyIdA, out var spriteA) && spriteA.GlobalWidth > spriteA.GlobalHeight) {
				widthA = ((KeySize - border.Vertical) * ((float)spriteA.GlobalWidth / spriteA.GlobalHeight)).RoundToInt();
				widthA += border.Horizontal;
			}
			if (CellRenderer.TryGetSprite(keyIdB, out var spriteB) && spriteB.GlobalWidth > spriteB.GlobalHeight) {
				widthB = ((KeySize - border.Vertical) * ((float)spriteB.GlobalWidth / spriteB.GlobalHeight)).RoundToInt();
				widthB += border.Horizontal;
			}

			rect.width = widthA;
			if (background) {
				bgCell = CellRenderer.Draw(Const.PIXEL, rect.Expand(BG_PADDING_X), new(12, 12, 12, 255));
				bgCell.Z = 0;
			}
			// Button A
			CellRenderer.Draw_9Slice(buttonCode, rect, border.Left, border.Right, border.Down, border.Up);

			// Button Label A
			CellRenderer.Draw(keyIdA, rect.Shrink(border), KeyLabelTint);
			rect.x += rect.width + Gap;

			// Button B
			if (keyA != keyB) {
				// Button B
				rect.width = widthB;
				CellRenderer.Draw_9Slice(buttonCode, rect, border.Left, border.Right, border.Down, border.Up);
				// Button Label B
				CellRenderer.Draw(keyIdB, rect.Shrink(border), KeyLabelTint);
				rect.x += rect.width + Gap;
			}

			// Label
			rect.width = 1;
			CellRenderer.DrawLabel(
				new CellLabel() {
					Text = Language.Get(labelID),
					Tint = LabelTint,
					CharSize = TextSize,
					Alignment = Alignment.MidLeft,
				}, rect, out var bounds
			);
			if (bgCell != null) {
				bgCell.Y = Mathf.Min(bgCell.Y, bounds.y - BG_PADDING_Y);
				bgCell.Width = Mathf.Max(bgCell.Width, bounds.xMax - bgCell.X + BG_PADDING_X);
				bgCell.Height = Mathf.Max(bgCell.Height, bounds.yMax - bgCell.Y + BG_PADDING_Y);
			}

		}


	}
}