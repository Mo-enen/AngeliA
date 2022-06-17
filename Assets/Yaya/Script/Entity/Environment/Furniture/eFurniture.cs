using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[MapEditorGroup("Furniture")]
	[EntityCapacity(32)]
	public abstract class eFurniture : Entity {


		protected enum FurniturePose {
			Unknown = 0,
			Left = 1,
			Down = 1,
			Mid = 2,
			Right = 3,
			Up = 3,
			Single = 4,
		}


		// VAR
		protected abstract Direction3 ModuleType { get; }
		protected abstract int ArtworkCode_LeftDown { get; }
		protected abstract int ArtworkCode_Mid { get; }
		protected abstract int ArtworkCode_RightUp { get; }
		protected abstract int ArtworkCode_Single { get; }
		protected virtual bool LoopArtworkIndex { get; } = false;
		protected bool HasSameFurnitureOnLeftOrDown { get; private set; } = false;
		protected bool HasSameFurnitureOnRightOrUp { get; private set; } = false;

		// Data
		protected FurniturePose Pose = FurniturePose.Unknown;
		protected int ArtworkIndex = 0;
		protected RectInt RenderingRect = default;


		// MSG
		public override void OnActived () {
			base.OnActived();
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;
			Pose = FurniturePose.Unknown;
			RenderingRect = Rect;
			HasSameFurnitureOnLeftOrDown = false;
			HasSameFurnitureOnRightOrUp = false;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			Update_Pose();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Pose == FurniturePose.Unknown) return;
			if (TryGetSprite(Pose, out var sprite)) {
				CellRenderer.Draw(sprite.GlobalID, RenderingRect);
			}
		}


		// API
		protected void DrawClockHands (RectInt rect, int handCode, int thickness, int thicknessSecond) {
			var now = System.DateTime.Now;
			// Sec
			CellRenderer.Draw(
				handCode, rect.x + rect.width / 2, rect.y + rect.height / 2,
				500, 0, now.Second * 360 / 60,
				thicknessSecond, rect.height * 900 / 2000
			);
			// Min
			CellRenderer.Draw(
				handCode, rect.x + rect.width / 2, rect.y + rect.height / 2,
				500, 0, now.Minute * 360 / 60,
				thickness, rect.height * 800 / 2000
			);
			// Hour
			CellRenderer.Draw(
				handCode, rect.x + rect.width / 2, rect.y + rect.height / 2,
				500, 0, (now.Hour * 360 / 12) + (now.Minute * 360 / 12 / 60),
				thickness, rect.height * 400 / 2000
			);
		}


		protected void DrawClockPendulum (int artCodeLeg, int artCodeHead, int x, int y, int length, int thickness, int headSize, int maxRot, int deltaX = 0) {
			float t11 = Mathf.Sin(Game.GlobalFrame * 6 * Mathf.Deg2Rad);
			int rot = (t11 * maxRot).RoundToInt();
			int dX = -(t11 * deltaX).RoundToInt();
			// Leg
			CellRenderer.Draw(artCodeLeg, x + dX, y, 500, 1000, rot, thickness, length);
			// Head
			CellRenderer.Draw(
				artCodeHead, x + dX, y, 500,
				500 * (headSize / 2 + length) / (headSize / 2),
				rot, headSize, headSize
			);
		}


		// LGC
		private void Update_Pose () {
			if (Pose != FurniturePose.Unknown) return;

			if (ModuleType == Direction3.Horizontal) {
				var rect = Rect;
				rect.x = Rect.x - Const.CELL_SIZE;
				bool hasLeft = CellPhysics.HasEntity(GetType(), rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly);
				rect.x = Rect.xMax;
				bool hasRight = CellPhysics.HasEntity(GetType(), rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly);
				Pose =
					hasLeft && hasRight ? FurniturePose.Mid :
					!hasLeft && !hasRight ? FurniturePose.Single :
					!hasLeft && hasRight ? FurniturePose.Left :
					FurniturePose.Right;
				HasSameFurnitureOnLeftOrDown = hasLeft;
				HasSameFurnitureOnRightOrUp = hasRight;
			} else if (ModuleType == Direction3.Vertical) {
				var rect = Rect;
				rect.y = Rect.y - Const.CELL_SIZE;
				bool hasDown = CellPhysics.HasEntity(GetType(), rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly);
				rect.y = Rect.yMax;
				bool hasUp = CellPhysics.HasEntity(GetType(), rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly);
				Pose =
					hasDown && hasUp ? FurniturePose.Mid :
					!hasDown && !hasUp ? FurniturePose.Single :
					!hasDown && hasUp ? FurniturePose.Down :
					FurniturePose.Up;
				HasSameFurnitureOnLeftOrDown = hasDown;
				HasSameFurnitureOnRightOrUp = hasUp;
			} else {
				Pose = FurniturePose.Single;
			}


			if (TryGetSprite(Pose, out var sp)) {
				var rect = Rect.Shrink(sp.GlobalBorder.Left, sp.GlobalBorder.Right, sp.GlobalBorder.Down, sp.GlobalBorder.Up);
				X = rect.x;
				Y = rect.y;
				Width = rect.width;
				Height = rect.height;
			}
		}


		private bool TryGetSprite (FurniturePose pose, out AngeSprite sprite) =>
			CellRenderer.TryGetSpriteFromGroup(
			pose switch {
				FurniturePose.Left => ArtworkCode_LeftDown,
				FurniturePose.Mid => ArtworkCode_Mid,
				FurniturePose.Right => ArtworkCode_RightUp,
				FurniturePose.Single => ArtworkCode_Single,
				_ => 0,
			}, ArtworkIndex, out sprite, LoopArtworkIndex);


	}
}