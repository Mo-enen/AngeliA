using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {

	public class CommonBurnerLeft : Burner<CommonFire> {
		protected override Direction4 Direction => Direction4.Left;
	}

	public class CommonBurnerRight : Burner<CommonFire> {
		protected override Direction4 Direction => Direction4.Right;
	}

	public class CommonBurnerDown : Burner<CommonFire> {
		protected override Direction4 Direction => Direction4.Down;
	}

	public class CommonBurnerUp : Burner<CommonFire> {
		protected override Direction4 Direction => Direction4.Up;
	}



	public abstract class Burner<F> : EnvironmentEntity where F : Fire {




		#region --- VAR ---


		// Api
		protected virtual int FireFrequency => 480;
		protected virtual int FireDuration => FireFrequency / 4;
		protected virtual Direction4 Direction => Direction4.Up;

		// Data
		private int FireTypeID = 0;
		private int FireFrameOffset = 0;
		private int NextFireSpawnedFrame = int.MinValue;
		private bool Burning = false;
		private Color32 FireTint = Const.WHITE;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {

			base.OnActivated();

			FireTypeID = typeof(F).AngeHash();
			NextFireSpawnedFrame = int.MinValue;
			Burning = false;

			if (CellRenderer.TryGetSprite(FireTypeID, out var fSprite)) {
				FireTint = fSprite.SummaryTint;
			} else {
				FireTint = Const.WHITE;
			}
			FireTint.a = 96;

			// Fire Offset
			FireFrameOffset = 0;
			var normal = Direction.Normal();
			if (WorldSquad.FrontBlockSquad.TryGetSingleSystemNumber(
				(X + 1).ToUnit() + normal.x, (Y + 1).ToUnit() + normal.y, Stage.ViewZ, out int fireOffset
			)) {
				FireFrameOffset = FireFrequency * fireOffset.Clamp(0, 9) / 10;
			} else if (WorldSquad.FrontBlockSquad.TryGetSingleSystemNumber(
				(X + 1).ToUnit() - normal.x, (Y + 1).ToUnit() - normal.y, Stage.ViewZ, out fireOffset
			)) {
				FireFrameOffset = FireFrequency * fireOffset.Clamp(0, 9) / 10;
			}
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			int localFrame = (Game.SettleFrame - FireFrameOffset).UMod(FireFrequency);
			Burning = localFrame < FireDuration;
			// Spawn Fire
			if (
				localFrame < FireDuration &&
				Game.GlobalFrame >= NextFireSpawnedFrame &&
				Stage.TrySpawnEntity(FireTypeID, X, Y, out var entity) &&
				entity is Fire fire
			) {
				fire.Setup(FireDuration - localFrame, Direction, Width, Height);
				fire.X =
					Direction == Direction4.Left ? X - Const.CEL :
					Direction == Direction4.Right ? X + Const.CEL :
					X;
				fire.Y =
					Direction == Direction4.Up ? Y + Const.CEL :
					Direction == Direction4.Down ? Y - Const.CEL :
					Y;
				NextFireSpawnedFrame = Game.GlobalFrame + FireFrequency - localFrame;
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, Rect);
			if (Burning && Game.GlobalFrame % 6 < 3) {
				CellRenderer.SetLayerToAdditive();
				CellRenderer.Draw(TypeID, Rect, FireTint);
				CellRenderer.SetLayerToDefault();
			}
		}


		#endregion




	}
}
