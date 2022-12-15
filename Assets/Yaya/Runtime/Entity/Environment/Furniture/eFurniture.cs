using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.MapEditorGroup("Furniture")]
	[EntityAttribute.Capacity(32)]
	public abstract class eFurniture : Entity {




		#region --- VAR ---


		// Over
		protected virtual Direction3 ModuleType => Direction3.None;
		protected virtual int ArtworkCode_LeftDown => TypeID;
		protected virtual int ArtworkCode_Mid => TypeID;
		protected virtual int ArtworkCode_RightUp => TypeID;
		protected virtual int ArtworkCode_Single => TypeID;
		protected virtual bool LoopArtworkIndex => false;
		protected virtual bool UseHighlightAnimation => true;
		protected virtual RectInt RenderingRect => Rect.Expand(ColliderBorder);

		// Api
		public eFurniture FurnitureLeftOrDown { get; private set; } = null;
		public eFurniture FurnitureRightOrUp { get; private set; } = null;
		public int HighlightFrame { get; set; } = int.MinValue;
		protected RectOffset ColliderBorder { get; } = new();
		protected FittingPose Pose { get; private set; } = FittingPose.Unknown;
		protected int ArtworkIndex { get; set; } = 0;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
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
				if (this is IActionEntity iAct) {
					UpdateHighlight(cell, iAct);
				}
			}
		}


		private void UpdateHighlight (Cell cell, IActionEntity iAct) {
			// Highlight
			if (!UseHighlightAnimation || !iAct.IsHighlighted) return;
			int offset = Game.GlobalFrame % 30 > 15 ? 0 : Const.CEL / 20;
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
				}, ArtworkIndex, out sprite, LoopArtworkIndex
			);


		// Highlight
		public void Highlight () {
			if (this is not IActionEntity) return;
			HighlightFrame = Game.GlobalFrame;
			HighlightAllNeighbors();
		}


		public void HighlightAllNeighbors () {
			for (eFurniture i = FurnitureLeftOrDown; i != null; i = i.FurnitureLeftOrDown) {
				i.HighlightFrame = Game.GlobalFrame;
			}
			for (eFurniture i = FurnitureRightOrUp; i != null; i = i.FurnitureRightOrUp) {
				i.HighlightFrame = Game.GlobalFrame;
			}
		}


		#endregion




		#region --- LGC ---


		private void Update_Pose () {

			if (Pose != FittingPose.Unknown) return;
			Pose = FittingPose.Single;

			if (ModuleType != Direction3.None) {
				Pose = YayaGame.Current.WorldSquad.GetEntityPose(
					this, ModuleType == Direction3.Horizontal, YayaConst.MASK_ENVIRONMENT,
					out var ld, out var ru, OperationMode.ColliderAndTrigger
				);
				FurnitureLeftOrDown = ld as eFurniture;
				FurnitureRightOrUp = ru as eFurniture;
			}

			// Shrink Rect
			if (TryGetSprite(Pose, out var sp)) {
				ColliderBorder.left = sp.GlobalBorder.Left;
				ColliderBorder.right = sp.GlobalBorder.Right;
				ColliderBorder.bottom = sp.GlobalBorder.Down;
				ColliderBorder.top = sp.GlobalBorder.Up;
				X += ColliderBorder.left;
				Y += ColliderBorder.bottom;
				Width -= ColliderBorder.horizontal;
				Height -= ColliderBorder.vertical;
			}
		}


		#endregion




	}
}