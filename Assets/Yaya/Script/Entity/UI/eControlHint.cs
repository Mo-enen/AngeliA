using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Yaya {
	public class eControlHint : ScreenUI, IInitialize {


		// Const
		private static readonly int KEY_BUTTON_CODE = "Keyboard Button".AngeHash();
		private static readonly int HINT_MOVE_CODE = "CtrlHint.Move".AngeHash();
		private static readonly int HINT_JUMP_CODE = "CtrlHint.Jump".AngeHash();
		private static readonly int HINT_ATTACK_CODE = "CtrlHint.Attack".AngeHash();
		private static readonly int HINT_USE_CODE = "CtrlHint.Use".AngeHash();
		private static readonly int HINT_WAKE_CODE = "CtrlHint.WakeUp".AngeHash();
		private static readonly int HINT_CANCEL_CODE = "CtrlHint.Cancel".AngeHash();

		// Api
		public ePlayer Player { get; set; } = null;
		public int KeySize { get; set; } = 128;
		public int Gap { get; set; } = 32;
		public int TextSize { get; set; } = 100;
		public Color32 Tint { get; set; } = Const.WHITE;

		// Data
		private static readonly Dictionary<int, int> TypeHintMap = new();
		private int PositionY = 0;
		private Int4 Border = default;


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
			if (CellRenderer.TryGetSprite(KEY_BUTTON_CODE, out var sprite)) {
				Border.Left = (int)(sprite.GlobalBorder.Left * ((float)KeySize / sprite.GlobalWidth));
				Border.Right = (int)(sprite.GlobalBorder.Right * ((float)KeySize / sprite.GlobalWidth));
				Border.Down = (int)(sprite.GlobalBorder.Down * ((float)KeySize / sprite.GlobalHeight));
				Border.Up = (int)(sprite.GlobalBorder.Up * ((float)KeySize / sprite.GlobalHeight));
			}
		}


		protected override void UpdateForUI () {
			if (Player == null || !Player.Active) return;
			if (FrameStep.HasStep<sOpening>() || FrameStep.HasStep<sFadeOut>()) return;
			PositionY = Y + CellRenderer.CameraRect.y;
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
						DrawKey(GameKey.Jump, HINT_JUMP_CODE);
						DrawKey(GameKey.Action, HINT_ATTACK_CODE);
					}
					break;
				case eCharacter.State.Sleep:
					DrawKey(GameKey.Action, HINT_WAKE_CODE);
					break;
			}
		}


		private void DrawKey (GameKey key, int labelID) => DrawKey(key, key, labelID);
		private void DrawKey (GameKey keyA, GameKey keyB, int labelID) {

			int x = X + CellRenderer.CameraRect.x;

			// Back
			CellRenderer.Draw(KEY_BUTTON_CODE, new(x, PositionY, KeySize, KeySize));

			// Key Label
			int keyID = FrameInput.GetGameKeyLabelID(keyA);
			CellRenderer.Draw(keyID, new RectInt(x, PositionY, KeySize, KeySize).Shrink(Border));
			x += KeySize + Gap;

			// Key Label B
			if (keyA != keyB) {
				CellRenderer.Draw(KEY_BUTTON_CODE, new(x, PositionY, KeySize, KeySize));
				keyID = FrameInput.GetGameKeyLabelID(keyB);
				CellRenderer.Draw(keyID, new RectInt(x, PositionY, KeySize, KeySize).Shrink(Border));
				x += KeySize + Gap;
			}

			// Label
			string content = Language.Get(labelID);
			CellRenderer.DrawLabel(content, new(x, PositionY, 1, KeySize), Tint, TextSize, 0, 0, false, Alignment.MidLeft);

			PositionY += KeySize + Gap;
		}


	}
}