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

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Game.GlobalFrame <= CameraUpdateFrame) return;
			if (!TargetLeft.HasValue && !TargetRight.HasValue && !TargetDown.HasValue && !TargetUp.HasValue) return;
			var player = Yaya.Current.CurrentPlayer;
			if (player == null || !player.Active) return;
			const int HALF = Const.CEL / 2;
			// Check if Entity Surround Player
			bool playerLeft = player.X < X + HALF;
			bool playerDown = player.Y < Y + HALF;
			var entityH = playerLeft ? TargetLeft : TargetRight;
			var entityV = playerDown ? TargetDown : TargetUp;
			if (!entityH.HasValue || !entityV.HasValue) return;
			if (playerLeft ? player.X < entityH.Value.x + HALF : player.X > entityH.Value.x + HALF) return;
			if (playerDown ? player.Y < entityV.Value.y + HALF : player.Y > entityV.Value.y + HALF) return;
			// Get Rect
			var targetRect = new RectInt();
			targetRect.SetMinMax(
				Mathf.Min(X + HALF, entityH.Value.x + HALF),
				Mathf.Max(X + HALF, entityH.Value.x + HALF),
				Mathf.Min(Y + HALF, entityV.Value.y + HALF),
				Mathf.Max(Y + HALF, entityV.Value.y + HALF)
			);
			// Clamp Camera
			var yayaGame = Yaya.Current;
			var cameraRect = CellRenderer.CameraRect;
			int viewOffsetX = yayaGame.ViewRect.x - CellRenderer.CameraRect.x;
			cameraRect.x = yayaGame.AimViewX - viewOffsetX;
			cameraRect.y = yayaGame.AimViewY;
			cameraRect.x = cameraRect.x.Clamp(targetRect.xMax - cameraRect.width, targetRect.xMin);
			cameraRect.y = cameraRect.y.Clamp(targetRect.yMax - cameraRect.height, targetRect.yMin);
			Game.Current.SetViewPositionDely(
				cameraRect.x + viewOffsetX,
				cameraRect.y,
				YayaConst.PLAYER_VIEW_LERP_RATE,
				YayaConst.VIEW_PRIORITY_SYSTEM
			);
			CameraUpdateFrame = Game.GlobalFrame;
		}


		#endregion




	}
}