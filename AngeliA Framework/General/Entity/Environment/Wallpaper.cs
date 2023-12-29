using System.Collections;
using System.Collections.Generic;


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


		[OnGameUpdate]
		public static void OnGameUpdate () {
			if (Current != null && !Current.Active) Current = null;
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			if (Current != null && Current.Active && Current.TypeID == TypeID && Current.SpawnFrame < SpawnFrame) {
				Active = false;
			}
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (!Active) return;
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


		protected abstract void DrawBackground (IRect backgroundRect);


		#endregion




		#region --- API ---


		protected Byte4 GetSkyTint (int y) => Byte4.LerpUnclamped(
			Game.SkyTintBottomColor, Game.SkyTintTopColor,
			Util.InverseLerp(CellRenderer.CameraRect.yMin, CellRenderer.CameraRect.yMax, y)
		);


		#endregion




		#region --- LGC ---





		#endregion




	}
}