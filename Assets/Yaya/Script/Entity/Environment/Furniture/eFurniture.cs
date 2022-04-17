using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[MapEditorGroup("Furniture")]
	public abstract class eFurniture : Entity {


		// VAR
		protected abstract Direction3 Direction { get; }
		protected abstract int[] ArtworkCodes_LeftDown { get; }
		protected abstract int[] ArtworkCodes_Mid { get; }
		protected abstract int[] ArtworkCodes_RightUp { get; }
		protected abstract int[] ArtworkCodes_Single { get; }
		public override int Capacity => 32;

		// Data
		protected FurniturePose Pose = FurniturePose.Unknown;
		protected int ArtworkIndex = 0;
		protected RectInt RenderingRect = default;


		// MSG
		public override void OnActived (int frame) {
			base.OnActived(frame);
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;
			Pose = FurniturePose.Unknown;
			RenderingRect = Rect;
		}


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillEntity((int)PhysicsLayer.Environment, this, true, Const.ONEWAY_UP_TAG);
		}


		public override void PhysicsUpdate (int frame) {
			base.PhysicsUpdate(frame);
			Update_Pose();
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			if (Pose != FurniturePose.Unknown) CellRenderer.Draw(GetArtworkCode(Pose), RenderingRect);
		}


		private void Update_Pose () {
			if (Pose != FurniturePose.Unknown) return;

			if (Direction == Direction3.Horizontal) {
				var rect = Rect;
				rect.x = Rect.x - Const.CELL_SIZE;
				bool hasLeft = CellPhysics.HasEntity(GetType(), rect, (int)PhysicsMask.Environment, this, OperationMode.TriggerOnly);
				rect.x = Rect.xMax;
				bool hasRight = CellPhysics.HasEntity(GetType(), rect, (int)PhysicsMask.Environment, this, OperationMode.TriggerOnly);
				Pose =
					hasLeft && hasRight ? FurniturePose.Mid :
					!hasLeft && !hasRight ? FurniturePose.Single :
					!hasLeft && hasRight ? FurniturePose.Left :
					FurniturePose.Right;
			} else if (Direction == Direction3.Vertical) {
				var rect = Rect;
				rect.y = Rect.y - Const.CELL_SIZE;
				bool hasDown = CellPhysics.HasEntity(GetType(), rect, (int)PhysicsMask.Environment, this, OperationMode.TriggerOnly);
				rect.y = Rect.yMax;
				bool hasUp = CellPhysics.HasEntity(GetType(), rect, (int)PhysicsMask.Environment, this, OperationMode.TriggerOnly);
				Pose =
					hasDown && hasUp ? FurniturePose.Mid :
					!hasDown && !hasUp ? FurniturePose.Single :
					!hasDown && hasUp ? FurniturePose.Down :
					FurniturePose.Up;
			} else {
				Pose = FurniturePose.Single;
			}

			int code = GetArtworkCode(Pose);
			if (CellRenderer.TryGetSprite(code, out var sp)) {
				var rect = Rect.Shrink(sp.GlobalBorder.Left, sp.GlobalBorder.Right, sp.GlobalBorder.Down, sp.GlobalBorder.Up);
				X = rect.x;
				Y = rect.y;
				Width = rect.width;
				Height = rect.height;
			}
		}


		private int GetArtworkCode (FurniturePose pose) => pose switch {
			FurniturePose.Left => ArtworkCodes_LeftDown[ArtworkIndex % ArtworkCodes_LeftDown.Length],
			FurniturePose.Mid => ArtworkCodes_Mid[ArtworkIndex % ArtworkCodes_Mid.Length],
			FurniturePose.Right => ArtworkCodes_RightUp[ArtworkIndex % ArtworkCodes_RightUp.Length],
			FurniturePose.Single => ArtworkCodes_Single[ArtworkIndex % ArtworkCodes_Single.Length],
			_ => 0,
		};


	}
}