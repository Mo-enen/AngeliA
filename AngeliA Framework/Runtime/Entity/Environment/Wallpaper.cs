using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDrawBehind]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.MapEditorGroup("Wallpaper")]
	public abstract class Wallpaper : Entity {




		#region --- VAR ---


		// Api
		protected int Amount { get; private set; } = 0;

		// Data
		private static Wallpaper Current = null;
		private int? PrevTargetX = null;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			Amount = 0;
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			if (Current != null && !Current.Active) Current = null;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			// Switch Current
			if (Current != this) {
				if (Current == null) {
					Current = this;
				} else if (Player.Selecting != null && PrevTargetX.HasValue) {
					// Cross to Trigger
					if ((Player.Selecting.X - X).Sign() != (PrevTargetX.Value - X).Sign()) {
						Current = this;
					}
				}
			}
			PrevTargetX = Player.Selecting?.X;
			// Update Amount
			const int DELTA = 6;
			if (Current == this) {
				Amount = (Amount + DELTA).Clamp(0, 1000);
			} else {
				Amount = (Amount - DELTA).Clamp(0, 1000);
			}
		}


		public sealed override void FrameUpdate () {
			if (!Active) return;
			if (Current != this && Amount == 0) {
				if (!FromWorld || InstanceID.z != Stage.ViewZ || !Rect.Overlaps(Stage.SpawnRect)) {
					Active = false;
				}
				return;
			}
			base.FrameUpdate();
			int oldLayer = CellRenderer.CurrentLayerIndex;
			CellRenderer.SetLayerToWallpaper();
			DrawBackground(CellRenderer.CameraRect);
			CellRenderer.SetLayer(oldLayer);
		}


		protected abstract void DrawBackground (RectInt backgroundRect);


		#endregion




		#region --- API ---


		protected Color32 GetSkyTint (int y) {
			var cameraRect = CellRenderer.CameraRect;
			return Color32.LerpUnclamped(
				CellRenderer.SkyTintBottom, CellRenderer.SkyTintTop,
				Mathf.InverseLerp(cameraRect.yMin, cameraRect.yMax, y)
			);
		}


		#endregion




		#region --- LGC ---





		#endregion




	}
}