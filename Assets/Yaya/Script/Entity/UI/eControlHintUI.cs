using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	public class eControlHintUI : eYayaScreenUI, IInitialize {


		// Const
		private static readonly int KEYBOARD_BUTTON_ANI_CODE = "_aKeyboard Button".AngeHash();
		private static readonly int GAMEPAD_BUTTON_ANI_CODE = "_aGamepad Button".AngeHash();
		private static readonly int KEYBOARD_BUTTON_CODE = "Keyboard Button".AngeHash();
		private static readonly int GAMEPAD_BUTTON_CODE = "Gamepad Button".AngeHash();
		private static readonly int HINT_MOVE_CODE = "CtrlHint.Move".AngeHash();
		private static readonly int HINT_JUMP_CODE = "CtrlHint.Jump".AngeHash();
		private static readonly int HINT_ATTACK_CODE = "CtrlHint.Attack".AngeHash();
		private static readonly int HINT_USE_CODE = "CtrlHint.Use".AngeHash();
		private static readonly int HINT_WAKE_CODE = "CtrlHint.WakeUp".AngeHash();
		private static readonly int HINT_CANCEL_CODE = "CtrlHint.Cancel".AngeHash();
		private static readonly int HINT_UNPAUSE_CODE = "CtrlHint.UnPause".AngeHash();

		// Api
		public ePlayer Player { get; set; } = null;
		public int KeySize { get; set; } = 142;
		public int Gap { get; set; } = 32;
		public int TextSize { get; set; } = 100;
		public Color32 Tint { get; set; } = Const.WHITE;

		// Data
		private static readonly Dictionary<int, int> TypeHintMap = new();
		private static readonly List<eDialogUI> Dialogs = new();
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
				int id = $"ActionHint.{_type.Name}".AngeHash();
				while (!Language.Has(id) && _type != entityType && _type != objType) {
					_type = _type.BaseType;
					id = $"ActionHint.{_type.Name}".AngeHash();
				}
				if (Language.Has(id)) {
					TypeHintMap.TryAdd(type.AngeHash(), id);
				}
			}
			// Dialogs
			Dialogs.Clear();
			foreach (var type in typeof(eDialogUI).AllChildClass()) {
				int id = type.AngeHash();
				var dialog = Game.Current.PeekOrGetEntity(id);
				Dialogs.Add(dialog as eDialogUI);
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


		protected override void UpdateForUI () {

			if (Player == null || !Player.Active) return;
			if (FrameStep.HasStep<sOpening>() || FrameStep.HasStep<sFadeOut>()) return;

			PositionY = Y + CellRenderer.CameraRect.y;

			// Dialog Hint
			foreach (var dialog in Dialogs) {
				if (dialog.Active) {
					DrawKey(GameKey.Left, GameKey.Right, HINT_MOVE_CODE);
					DrawKey(GameKey.Action, YayaConst.UI_OK);
					DrawKey(GameKey.Start, YayaConst.UI_CANCEL);
					return;
				}
			}

			// Pausing
			if (Game.Current.IsPausing) {
				DrawKey(GameKey.Up, GameKey.Down, HINT_MOVE_CODE);
				DrawKey(GameKey.Action, YayaConst.UI_OK);
				DrawKey(GameKey.Start, HINT_UNPAUSE_CODE);
				return;
			}

			// Game Playing
			switch (Player.CharacterState) {

				case CharacterState.GamePlay: {
					// Move
					DrawKey(GameKey.Left, GameKey.Right, HINT_MOVE_CODE);
					// Action & Jump
					if (Player.Action.CurrentTarget is Entity target && target is IActionEntity iAct) {
						// Action Target
						if (target is eOpenableFurniture open && open.Open) {
							DrawKey(iAct.InvokeKey, YayaConst.UI_OK);
							DrawKey(GameKey.Jump, HINT_CANCEL_CODE);
						} else {
							if (TypeHintMap.TryGetValue(target.TypeID, out int code)) {
								DrawKey(iAct.InvokeKey, code);
							} else {
								DrawKey(iAct.InvokeKey, HINT_USE_CODE);
							}
						}
					} else {
						// General
						DrawKey(GameKey.Action, HINT_ATTACK_CODE);
						DrawKey(GameKey.Jump, HINT_JUMP_CODE);
					}
					break;
				}

				case CharacterState.Sleep: {
					int x = Player.X;
					int y = Player.GlobalBounds.yMax;
					DrawKey(x, y, GameKey.Action, HINT_WAKE_CODE, true, true);
					DrawKey(GameKey.Action, HINT_WAKE_CODE);
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

			const int BG_PADDING_X = 32;
			const int BG_PADDING_Y = 32;
			Cell bgCell = null;
			int buttonCode = animated ?
				FrameInput.UsingGamepad ? GAMEPAD_BUTTON_ANI_CODE : KEYBOARD_BUTTON_ANI_CODE :
				FrameInput.UsingGamepad ? GAMEPAD_BUTTON_CODE : KEYBOARD_BUTTON_CODE;
			var border = FrameInput.UsingGamepad ? Border_Gamepad : Border_Keyboard;
			var rect = new RectInt(x, y, KeySize, KeySize);
			int keyIdA = FrameInput.GetGameKeyLabelID(keyA);
			int keyIdB = FrameInput.GetGameKeyLabelID(keyB);
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
			CellRenderer.Draw(keyIdA, rect.Shrink(border));
			rect.x += rect.width + Gap;

			// Button B
			if (keyA != keyB) {
				// Button B
				rect.width = widthB;
				CellRenderer.Draw_9Slice(buttonCode, rect, border.Left, border.Right, border.Down, border.Up);
				// Button Label B
				CellRenderer.Draw(keyIdB, rect.Shrink(border));
				rect.x += rect.width + Gap;
			}

			// Label
			rect.width = 1;
			CellRenderer.DrawLabel(
				Language.Get(labelID),
				rect, Tint, TextSize, out var bounds, 0, 0, false, Alignment.MidLeft
			);
			if (bgCell != null) {
				bgCell.Y = Mathf.Min(bgCell.Y, bounds.y - BG_PADDING_Y);
				bgCell.Width = Mathf.Max(bgCell.Width, bounds.xMax - bgCell.X + BG_PADDING_X);
				bgCell.Height = Mathf.Max(bgCell.Height, bounds.yMax - bgCell.Y + BG_PADDING_Y);
			}

		}


	}
}