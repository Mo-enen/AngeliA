using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class ActionMapGenerator : MapGenerator, IActionTarget {




		#region --- VAR ---


		// Const
		private static readonly int HINT_GENERATE = "CtrlHint.MapGenerator.Generate".AngeHash();
		private static readonly int HINT_REGENERATE = "CtrlHint.MapGenerator.Regenerate".AngeHash();
		private static readonly int HINT_GENERATING = "CtrlHint.MapGenerator.Generating".AngeHash();

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
					IActionTarget.HighlightBlink(cell, Direction3.None, FittingPose.Single);
					// Hint
					ControlHintUI.DrawGlobalHint(
						X, Y + Const.CEL * 2 + Const.HALF, Gamekey.Action,
						HasMapInDisk ? Language.Get(HINT_REGENERATE, "Regenerate Map") : Language.Get(HINT_GENERATE, "Generate Map"), true
					);
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


		protected virtual Cell DrawArtwork () => CellRenderer.Draw(TypeID, Rect);


		void IActionTarget.Invoke () => GenerateAsync();


		bool IActionTarget.AllowInvoke () => !IsGenerating && !HasMapInDisk;


		#endregion




	}
}