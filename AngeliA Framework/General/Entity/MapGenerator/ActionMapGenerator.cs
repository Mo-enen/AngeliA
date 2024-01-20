using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[RequireLanguageFromField]
	public abstract class ActionMapGenerator : MapGenerator, IActionTarget {




		#region --- VAR ---


		// Const
		private static readonly LanguageCode HINT_ENTER = "CtrlHint.MapGenerator.Enter";
		private static readonly LanguageCode HINT_GENERATE = "CtrlHint.MapGenerator.Generate";
		private static readonly LanguageCode HINT_GENERATING = "CtrlHint.MapGenerator.Generating";
		private static readonly LanguageCode HINT_NOTIFY = "Notify.MapGeneratedNotify";

		// Api
		protected virtual bool ShowGeneratingHint => true;

		// Data
		private readonly CellContent HintContent = new() { Alignment = Alignment.MidMid, BackgroundTint = Const.BLACK, BackgroundPadding = 6, Wrap = false, Clip = false, };


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
					ControlHintUI.DrawGlobalHint(
						X, Y + Const.CEL * 2 + Const.HALF, Gamekey.Action,
						HasMapInDisk ? HINT_ENTER.Get("Enter Level") : HINT_GENERATE.Get("Generate Level"),
						true
					);
				}
			} else {
				// Generating
				if (ShowGeneratingHint) {
					CellRendererGUI.Label(
						HintContent.SetText(HINT_GENERATING.Get("Generating")),
						new IRect(X - Const.CEL, Y + Const.CEL * 2, Const.CEL * 3, Const.CEL)
					);
				}
			}
		}


		protected override void AfterMapGenerate () {
			base.AfterMapGenerate();
			NotificationUI.SpawnNotification(HINT_NOTIFY.Get("Map Generated"), TypeID);
		}


		protected virtual Cell DrawArtwork () => CellRenderer.Draw(TypeID, Rect);


		void IActionTarget.Invoke () {
			if (!HasMapInDisk) {
				StartGenerateAsync();
			} else {
				Enter();
			}
		}


		bool IActionTarget.AllowInvoke () => !IsGenerating;


		private void Enter () {
			if (FrameTask.HasTask()) return;
			TeleportTask.Teleport(
				X + Width / 2,
				Y + Height / 2,
				0, 0, 0,
				waitDuration: 30,
				duration: 60,
				useVignette: true,
				newChannel: MapChannel.Procedure,
				channelName: GetType().AngeName()
			);
		}


		#endregion




	}
}