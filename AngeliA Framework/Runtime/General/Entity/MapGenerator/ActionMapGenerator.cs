using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class ActionMapGenerator : MapGenerator, IActionTarget {




		#region --- VAR ---


		// Const
		private static readonly int HINT_GENERATE = "CtrlHint.MapGenerator.Generate".AngeHash();
		private static readonly int HINT_GENERATING = "CtrlHint.MapGenerator.Generating".AngeHash();
		private static readonly int HINT_NOTIFY = "Notify.MapGeneratedNotify".AngeHash();

		// Api
		protected virtual bool ShowGeneratingHint => true;

		// Data
		private readonly CellContent HintContent = new() { Alignment = Alignment.MidMid, BackgroundTint = Const.BLACK, TightBackground = true, Wrap = false, Clip = false, };


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();

		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();

			var cell = DrawArtwork();
			if (!IsGenerating) {
				// Normal
				if ((this as IActionTarget).IsHighlighted) {
					IActionTarget.HighlightBlink(cell, 0.5f, 0.5f);
					// Hint
					ControlHintUI.DrawGlobalHint(X, Y + Const.CEL * 2 + Const.HALF, Gamekey.Action, Language.Get(HINT_GENERATE, "Generate Map"), true);
				}
			} else {
				// Generating
				if (ShowGeneratingHint) {
					CellRendererGUI.Label(
						HintContent.SetText(Language.Get(HINT_GENERATING, "Generating")),
						new RectInt(X - Const.CEL, Y + Const.CEL * 2, Const.CEL * 3, Const.CEL)
					);
				}
			}
		}


		protected override void AfterMapGenerate () {
			base.AfterMapGenerate();
			RemoveEntrancePortal();
			SpawnEntrancePortal();
			NotificationUI.SpawnNotification(Language.Get(HINT_NOTIFY, "Map Generated"), TypeID);
		}


		protected virtual Cell DrawArtwork () => CellRenderer.Draw(TypeID, Rect);


		protected virtual void RemoveEntrancePortal () {
			var cells = CellPhysics.OverlapAll(
				PhysicsMask.ENVIRONMENT, Rect.Shift(0, Const.CEL * 2), out int count, this, OperationMode.TriggerOnly
			);
			for (int i = 0; i < count; i++) {
				if (cells[i].Entity is Portal portal) portal.Active = false;
			}
		}


		protected virtual void SpawnEntrancePortal () => Stage.SpawnEntity<PortalBack>(X, Y + Const.CEL * 3);


		void IActionTarget.Invoke () => GenerateAsync();


		bool IActionTarget.AllowInvoke () => !IsGenerating && !HasMapInDisk;


		#endregion




	}
}