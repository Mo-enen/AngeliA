using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.Capacity(16)]
	[EntityAttribute.MapEditorGroup("Camera")]
	public sealed class eCameraTarget : Entity {




		#region --- VAR ---


		// Data
		private static int CameraUpdateFrame = int.MinValue;
		private Vector2Int? TargetLeft = null;
		private Vector2Int? TargetRight = null;
		private Vector2Int? TargetDown = null;
		private Vector2Int? TargetUp = null;
		private eCameraTarget LeftEntity = null;
		private eCameraTarget RightEntity = null;
		private eCameraTarget DownEntity = null;
		private eCameraTarget UpEntity = null;


		#endregion




		#region --- MSG ---


		public override void OnActived () {

			base.OnActived();

			TargetLeft = null;
			TargetRight = null;
			TargetDown = null;
			TargetUp = null;

			if (Game.Current.WorldSquad.TryFindBlockFrom(
				TypeID, X.UDivide(Const.CEL) - 1, Y.UDivide(Const.CEL), Direction4.Left, BlockType.Entity,
				out int unitX, out int unitY
			)) TargetLeft = new Vector2Int(unitX * Const.CEL + Const.CEL / 2, unitY * Const.CEL + Const.CEL / 2);

			if (Game.Current.WorldSquad.TryFindBlockFrom(
				TypeID, X.UDivide(Const.CEL) + 1, Y.UDivide(Const.CEL), Direction4.Right, BlockType.Entity,
				out unitX, out unitY
			)) TargetRight = new Vector2Int(unitX * Const.CEL + Const.CEL / 2, unitY * Const.CEL + Const.CEL / 2);

			if (Game.Current.WorldSquad.TryFindBlockFrom(
				TypeID, X.UDivide(Const.CEL), Y.UDivide(Const.CEL) - 1, Direction4.Down, BlockType.Entity,
				out unitX, out unitY
			)) TargetDown = new Vector2Int(unitX * Const.CEL + Const.CEL / 2, unitY * Const.CEL + Const.CEL / 2);

			if (Game.Current.WorldSquad.TryFindBlockFrom(
				TypeID, X.UDivide(Const.CEL), Y.UDivide(Const.CEL) + 1, Direction4.Up, BlockType.Entity,
				out unitX, out unitY
			)) TargetUp = new Vector2Int(unitX * Const.CEL + Const.CEL / 2, unitY * Const.CEL + Const.CEL / 2);

			LeftEntity = null;
			RightEntity = null;
			DownEntity = null;
			UpEntity = null;

		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (TargetLeft.HasValue) {
				LeftEntity = CellPhysics.GetEntity<eCameraTarget>(new RectInt(TargetLeft.Value.x, TargetLeft.Value.y, 1, 1), YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly);
				if (LeftEntity != null) LeftEntity.RightEntity = this;
				TargetLeft = null;
			}
			if (TargetRight.HasValue) {
				RightEntity = CellPhysics.GetEntity<eCameraTarget>(new RectInt(TargetRight.Value.x, TargetRight.Value.y, 1, 1), YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly);
				if (RightEntity != null) RightEntity.LeftEntity = this;
				TargetRight = null;
			}
			if (TargetDown.HasValue) {
				DownEntity = CellPhysics.GetEntity<eCameraTarget>(new RectInt(TargetDown.Value.x, TargetDown.Value.y, 1, 1), YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly);
				if (DownEntity != null) DownEntity.UpEntity = this;
				TargetDown = null;
			}
			if (TargetUp.HasValue) {
				UpEntity = CellPhysics.GetEntity<eCameraTarget>(new RectInt(TargetUp.Value.x, TargetUp.Value.y, 1, 1), YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly);
				if (UpEntity != null) UpEntity.DownEntity = this;
				TargetUp = null;
			}

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Game.GlobalFrame <= CameraUpdateFrame) return;
			if (LeftEntity == null && RightEntity == null && DownEntity == null && UpEntity == null) return;
			var player = Yaya.Current.CurrentPlayer;
			if (player == null || !player.Active) return;
			const int HALF = Const.CEL / 2;
			// Check if Entity Surround Player
			bool playerLeft = player.X < X + HALF;
			bool playerDown = player.Y < Y + HALF;
			var entityH = playerLeft ? LeftEntity : RightEntity;
			var entityV = playerDown ? DownEntity : UpEntity;
			if (entityH == null || entityV == null) return;
			if (playerLeft ? player.X < entityH.X + HALF : player.X > entityH.X + HALF) return;
			if (playerDown ? player.Y < entityV.Y + HALF : player.Y > entityV.Y + HALF) return;
			var entityCorner = playerLeft ? entityV.LeftEntity : entityV.RightEntity;
			var entityCornerAlt = playerDown ? entityH.DownEntity : entityH.UpEntity;
			if (entityCorner != entityCornerAlt || entityCorner == null) return;
			// Get Rect
			int targetL = Mathf.Min(X + HALF, entityH.X + HALF);
			int targetR = Mathf.Max(X + HALF, entityH.X + HALF);
			int targetD = Mathf.Min(Y + HALF, entityV.Y + HALF);
			int targetU = Mathf.Max(Y + HALF, entityV.Y + HALF);
			var targetRect = new RectInt(targetL, targetD, targetR - targetL, targetU - targetD);
			// Clamp Camera



			


			CameraUpdateFrame = Game.GlobalFrame;
		}



		#endregion




		#region --- API ---



		#endregion




		#region --- LGC ---



		#endregion




	}
}