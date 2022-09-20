using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.MapEditorGroup("Furniture")]
	[EntityAttribute.EntityCapacity(32)]
	public abstract class eFurniture : Entity {




		#region --- VAR ---


		// Over
		protected virtual Direction3 ModuleType => Direction3.None;
		protected virtual int ArtworkCode_LeftDown => TrimedTypeID;
		protected virtual int ArtworkCode_Mid => TrimedTypeID;
		protected virtual int ArtworkCode_RightUp => TrimedTypeID;
		protected virtual int ArtworkCode_Single => TrimedTypeID;
		protected virtual bool LoopArtworkIndex => false;
		protected virtual bool UseHighlightAnimation => true;
		protected virtual RectInt RenderingRect => Rect.Expand(ColliderBorder);

		// Api
		public eFurniture FurnitureLeftOrDown { get; private set; } = null;
		public eFurniture FurnitureRightOrUp { get; private set; } = null;
		public bool IsHighlighted => Game.GlobalFrame <= HighlightFrame + 1;
		public int HighlightFrame { get; set; } = int.MinValue;
		protected RectOffset ColliderBorder { get; } = new();
		protected FittingPose Pose { get; private set; } = FittingPose.Unknown;
		protected int ArtworkIndex { get; set; } = 0;

		// Data
		private int LastUnhighlightFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;
			Pose = FittingPose.Unknown;
			FurnitureLeftOrDown = null;
			FurnitureRightOrUp = null;
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
			if (Pose == FittingPose.Unknown) return;
			if (TryGetSprite(Pose, out var sprite)) {
				var cell = CellRenderer.Draw(sprite.GlobalID, RenderingRect);
				Update_Highlight(cell);
			}
		}


		private void Update_Highlight (Cell cell) {
			// Highlight
			if (!UseHighlightAnimation || this is not IActionEntity iAct) return;
			if (!iAct.IsHighlighted) return;
			int offset =
				(Game.GlobalFrame - LastUnhighlightFrame) % 30 > 15
				? 0
				: Const.CELL_SIZE / 20;
			if (ModuleType == Direction3.Horizontal) {
				// Horizontal
				if (Pose == FittingPose.Left || Pose == FittingPose.Single) {
					cell.X -= offset;
				}
				if (Pose != FittingPose.Mid) {
					if (Pose == FittingPose.Left) {
						cell.Width += offset;
					} else {
						cell.Width += offset * 2;
					}
				}
				cell.Y -= offset;
				cell.Height += offset * 2;
			} else {
				// Vertical
				if (Pose == FittingPose.Down || Pose == FittingPose.Single) {
					cell.Y -= offset;
				}
				if (Pose != FittingPose.Mid) {
					if (Pose == FittingPose.Down) {
						cell.Height += offset;
					} else {
						cell.Height += offset * 2;
					}
				}
				cell.X -= offset;
				cell.Width += offset * 2;
			}
		}


		#endregion




		#region --- API ---


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


		protected bool TryGetSprite (FittingPose pose, out AngeSprite sprite) =>
			CellRenderer.TryGetSpriteFromGroup(
			pose switch {
				FittingPose.Left => ArtworkCode_LeftDown,
				FittingPose.Mid => ArtworkCode_Mid,
				FittingPose.Right => ArtworkCode_RightUp,
				FittingPose.Single => ArtworkCode_Single,
				_ => 0,
			}, ArtworkIndex, out sprite, LoopArtworkIndex);


		// Highlight
		public void Highlight () {
			if (this is not IActionEntity) return;
			bool oldHighlight = IsHighlighted;
			if (!oldHighlight) LastUnhighlightFrame = Game.GlobalFrame;
			HighlightFrame = Game.GlobalFrame;
			HighlightAllNeighbors(!oldHighlight);
		}


		public void HighlightAllNeighbors (bool firstFrame) {
			for (eFurniture i = FurnitureLeftOrDown; i != null; i = i.FurnitureLeftOrDown) {
				i.HighlightFrame = Game.GlobalFrame;
				if (firstFrame) i.LastUnhighlightFrame = Game.GlobalFrame;
			}
			for (eFurniture i = FurnitureRightOrUp; i != null; i = i.FurnitureRightOrUp) {
				i.HighlightFrame = Game.GlobalFrame;
				if (firstFrame) i.LastUnhighlightFrame = Game.GlobalFrame;
			}
		}


		#endregion




		#region --- LGC ---


		private void Update_Pose () {
			if (Pose != FittingPose.Unknown) return;

			if (ModuleType == Direction3.Horizontal) {
				var rect = Rect;
				rect.x = Rect.x - Const.CELL_SIZE;
				var eLeft = CellPhysics.GetEntity(
					GetType(), rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly
				) as eFurniture;
				bool hasLeft = eLeft != null;
				rect.x = Rect.xMax;
				var eRight = CellPhysics.GetEntity(
					GetType(), rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly
				) as eFurniture;
				bool hasRight = eRight != null;
				Pose =
					hasLeft && hasRight ? FittingPose.Mid :
					!hasLeft && !hasRight ? FittingPose.Single :
					!hasLeft && hasRight ? FittingPose.Left :
					FittingPose.Right;
				FurnitureLeftOrDown = eLeft;
				FurnitureRightOrUp = eRight;
			} else if (ModuleType == Direction3.Vertical) {
				var rect = Rect;
				rect.y = Rect.y - Const.CELL_SIZE;
				var eDown = CellPhysics.GetEntity(
					GetType(), rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly
				) as eFurniture;
				bool hasDown = eDown != null;
				rect.y = Rect.yMax;
				var eUp = CellPhysics.GetEntity(
					GetType(), rect, YayaConst.MASK_ENVIRONMENT, this, OperationMode.TriggerOnly
				) as eFurniture;
				bool hasUp = eUp != null;
				Pose =
					hasDown && hasUp ? FittingPose.Mid :
					!hasDown && !hasUp ? FittingPose.Single :
					!hasDown && hasUp ? FittingPose.Down :
					FittingPose.Up;
				FurnitureLeftOrDown = eDown;
				FurnitureRightOrUp = eUp;
			} else {
				Pose = FittingPose.Single;
			}

			// Shrink Rect
			if (TryGetSprite(Pose, out var sp)) {
				ColliderBorder.left = sp.GlobalBorder.Left;
				ColliderBorder.right = sp.GlobalBorder.Right;
				ColliderBorder.bottom = sp.GlobalBorder.Down;
				ColliderBorder.top = sp.GlobalBorder.Up;
				X -= ColliderBorder.left;
				Y -= ColliderBorder.bottom;
				Width -= ColliderBorder.horizontal;
				Height -= ColliderBorder.vertical;
			}
		}


		#endregion




	}
}