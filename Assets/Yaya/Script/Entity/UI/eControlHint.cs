using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Yaya {
	public class eControlHint : ScreenUI, IInitialize {


		// Const
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
		private int PositionY = 0;
		private Int4 Border_Keyboard = default;
		private Int4 Border_Gamepad = default;


		// MSG
		public static void Initialize () {
			TypeHintMap.Clear();
			foreach (var type in typeof(IActionEntity).AllClassImplemented()) {
				int id = $"ActionHint.{type.Name}".AngeHash();
				if (Language.Has(id)) {
					TypeHintMap.TryAdd(type.AngeHash(), id);
				}
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

			if (!Game.IsPausing) {
				// Game Playing
				switch (Player.CharacterState) {
					case eCharacter.State.General:
						if (Player.Action.CurrentTarget is Entity target) {
							// Action
							if (TypeHintMap.TryGetValue(target.TypeID, out int code)) {
								DrawKey(GameKey.Action, code);
							} else {
								DrawKey(GameKey.Action, HINT_USE_CODE);
							}
							if (target is eOpenableFurniture open && open.Open) {
								DrawKey(GameKey.Jump, HINT_CANCEL_CODE);
							}
						} else {
							// General
							DrawKey(GameKey.Left, GameKey.Right, HINT_MOVE_CODE);
							DrawKey(GameKey.Action, HINT_ATTACK_CODE);
							DrawKey(GameKey.Jump, HINT_JUMP_CODE);
						}
						break;
					case eCharacter.State.Sleep:
						DrawKey(GameKey.Action, HINT_WAKE_CODE);
						break;
				}

			} else {
				// Pausing
				DrawKey(GameKey.Start, HINT_UNPAUSE_CODE);
			}

		}


		private void DrawKey (GameKey key, int labelID) => DrawKey(key, key, labelID);
		private void DrawKey (GameKey keyA, GameKey keyB, int labelID) {

			int buttonCode = FrameInput.UsingGamepad ? GAMEPAD_BUTTON_CODE : KEYBOARD_BUTTON_CODE;
			var border = FrameInput.UsingGamepad ? Border_Gamepad : Border_Keyboard;
			var rect = new RectInt(X + CellRenderer.CameraRect.x, PositionY, KeySize, KeySize);
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

			// Button
			rect.width = widthA;
			CellRenderer.Draw_9Slice(buttonCode, rect, border.Left, border.Right, border.Down, border.Up);

			// Button Label
			CellRenderer.Draw(keyIdA, rect.Shrink(border));
			rect.x += rect.width + Gap;

			// Button B
			if (keyA != keyB) {
				// Button
				rect.width = widthB;
				CellRenderer.Draw_9Slice(buttonCode, rect, border.Left, border.Right, border.Down, border.Up);
				// Button Label
				CellRenderer.Draw(keyIdB, rect.Shrink(border));
				rect.x += rect.width + Gap;
			}

			// Label
			rect.width = 1;
			CellRenderer.DrawLabel(
				Language.Get(labelID),
				rect, Tint, TextSize, 0, 0, false, Alignment.MidLeft
			);

			PositionY += KeySize + Gap;
		}


	}
}