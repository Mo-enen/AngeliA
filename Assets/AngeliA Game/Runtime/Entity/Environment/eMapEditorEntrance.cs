using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	[EntityAttribute.Capacity(1, 0)]
	public class eMapEditorEntrance : Entity, IActionTarget {


		// Const
		private static readonly int HINT_START = "CtrlHint.EditMap".AngeHash();
		private static readonly int HINT_QUIT = "CtrlHint.QuitEditing".AngeHash();


		// MSG
		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (CellRenderer.TryGetSprite(TypeID, out var sprite)) {
				var cell = CellRenderer.Draw(TypeID, X + Width / 2, Y, 500, 0, 0, sprite.GlobalWidth, sprite.GlobalHeight);
				if ((this as IActionTarget).IsHighlighted) {
					IActionTarget.HighlightBlink(cell);
					// Draw Hint
					bool editing = MapEditor.Instance != null && MapEditor.Instance.Active;
					ControlHintUI.DrawGlobalHint(
						X, Y + Height + Const.CEL * 2, Gamekey.Action,
						editing ? Language.Get(HINT_QUIT, "Quit Editing Map") : Language.Get(HINT_START, "Edit Map"), true
					);
				}
				AngeUtil.DrawShadow(TypeID, cell);
			}
		}


		void IActionTarget.Invoke () {
			if (FrameTask.HasTask()) return;
			bool editing = MapEditor.Instance != null && MapEditor.Instance.Active;
			if (editing) {
				MapEditor.CloseMapEditorSmoothly();
			} else {
				MapEditor.OpenMapEditorSmoothly();
			}
		}


		bool IActionTarget.AllowInvoke () => !FrameTask.HasTask();


	}
}