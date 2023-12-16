using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class ActionMapGenerator : MapGenerator, IActionTarget {




		#region --- VAR ---


		// Const
		private static readonly int HINT_ENTER = "CtrlHint.MapGenerator.Enter".AngeHash();
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
					ControlHintUI.DrawGlobalHint(
						X, Y + Const.CEL * 2 + Const.HALF, Gamekey.Action,
						HasMapInDisk ? Language.Get(HINT_ENTER, "Enter Level") : Language.Get(HINT_GENERATE, "Generate Level"),
						true
					);
				}
			} else {
				// Generating
				if (ShowGeneratingHint) {
					CellRendererGUI.Label(
						HintContent.SetText(Language.Get(HINT_GENERATING, "Generating")),
						new IRect(X - Const.CEL, Y + Const.CEL * 2, Const.CEL * 3, Const.CEL)
					);
				}
			}
		}


		protected override void AfterMapGenerate () {
			base.AfterMapGenerate();
			NotificationUI.SpawnNotification(Language.Get(HINT_NOTIFY, "Map Generated"), TypeID);
		}


		protected virtual Cell DrawArtwork () => CellRenderer.Draw(TypeID, Rect);


		void IActionTarget.Invoke () {
			if (!HasMapInDisk) {
				GenerateAsync();
			} else {
				Enter();
			}
		}


		bool IActionTarget.AllowInvoke () => !IsGenerating;


		private void Enter () {
			var player = Player.Selecting;
			if (player == null || FrameTask.HasTask()) return;
			int z = Stage.ViewZ + 1;
			player.Stop();
			player.VelocityX = 0;
			player.VelocityY = 0;
			var task = TeleportTask.Teleport(
				X + (Width - player.Width) / 2 - player.OffsetX,
				Y + player.Height / 2,
				X, Y, z
			);
			if (task != null) {
				task.TeleportEntity = player;
				task.WaitDuration = 30;
				task.Duration = 60;
				task.UseVignette = true;
				player.EnterTeleportState(task.Duration, Stage.ViewZ > z, false);
			}
		}


		#endregion




	}
}