using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public class ActiveTitleUI : TitleUI {
		protected override TitleUITriggerMode TriggerMode => TitleUITriggerMode.Active;
	}


	public class PassTitleUI : TitleUI {
		protected override TitleUITriggerMode TriggerMode => TitleUITriggerMode.Pass;
	}


	[EntityAttribute.MapEditorGroup("System")]
	public abstract class TitleUI : Entity {


		// SUB
		protected enum TitleUITriggerMode { Active, Pass, }

		// Api
		protected abstract TitleUITriggerMode TriggerMode { get; }

		// Data
		private string Title = "";
		private string SubTitle = "";
		private int TriggeredFrame = int.MinValue;
		private int PrevCameraX = 0;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			TriggeredFrame = int.MinValue;
			PrevCameraX = CellRenderer.CameraRect.CenterX();
			if (WorldSquad.FrontBlockSquad.ReadSystemNumber(X.ToUnit(), Y.ToUnit() + 1, Stage.ViewZ, Direction4.Down, out int titleIndex)) {
				Title = Language.Get($"UI.Title.{titleIndex}".AngeHash(), "");
				SubTitle = Language.Get($"UI.SubTitle.{titleIndex}".AngeHash(), "");
			} else {
				Title = "";
				SubTitle = "";
				Active = false;
				return;
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (!Active || string.IsNullOrEmpty(Title)) return;
			if (TriggeredFrame < 0) {
				TriggerCheck();
			} else {
				var cRect = CellRenderer.CameraRect;
				X = cRect.CenterX();
				Y = cRect.CenterY();
				const int DURATION = 180;
				int localFrame = Game.GlobalFrame - TriggeredFrame;
				if (localFrame > DURATION) {
					Active = false;
					return;
				}

				// Title
				CellRendererGUI.Label(
					CellContent.Get(Title, Const.WHITE, 96),
					cRect.Shrink(0, 0, cRect.height / 2, 0),
					out var bound
				);

				// Sub Title
				CellRendererGUI.Label(
					CellContent.Get(SubTitle, Const.WHITE, 32, Alignment.TopMid),
					new RectInt(
						cRect.x,
						bound.y - cRect.height,
						cRect.width,
						cRect.height - CellRendererGUI.Unify(32)
					)
				);

			}
		}


		// LGC
		private void TriggerCheck () {
			if (TriggerMode == TitleUITriggerMode.Active) {
				TriggeredFrame = Game.GlobalFrame;
			} else {
				int x = CellRenderer.CameraRect.CenterX();
				if (Player.Selecting != null) {
					x = Player.Selecting.Rect.CenterX();
				}
				if (x > X != PrevCameraX > X) {
					TriggeredFrame = Game.GlobalFrame;
				}
				PrevCameraX = x;
			}
			if (TriggeredFrame >= 0) Stage.MarkAsGlobalAntiSpawn(this);
		}


	}
}