using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.MapEditorGroup("Furniture")]
	[EntityAttribute.Capacity(32)]
	public abstract class eFurniture : eActionEntity {




		#region --- VAR ---


		// Over
		protected virtual Direction3 ModuleType => Direction3.None;
		protected virtual int ArtworkCode_LeftDown => TypeID;
		protected virtual int ArtworkCode_Mid => TypeID;
		protected virtual int ArtworkCode_RightUp => TypeID;
		protected virtual int ArtworkCode_Single => TypeID;
		protected virtual bool LoopArtworkIndex => false;
		protected virtual RectInt RenderingRect => Rect.Expand(ColliderBorder);

		// Api
		public eFurniture FurnitureLeftOrDown { get; private set; } = null;
		public eFurniture FurnitureRightOrUp { get; private set; } = null;
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


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			// Update Pose
			if (Pose == FittingPose.Unknown) {
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
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Pose == FittingPose.Unknown) return;
			if (TryGetSprite(Pose, out var sprite)) {
				var cell = CellRenderer.Draw(sprite.GlobalID, RenderingRect);
				HighlightBlink(cell, ModuleType, Pose);
			}
		}


		#endregion




		#region --- API ---


		public override bool Invoke (Entity target) => true;


		public override bool AllowInvoke (Entity target) => false;


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
		public override void Highlight () {
			base.Highlight();
			HighlightAllNeighbors();
		}


		public void HighlightAllNeighbors () {
			for (eFurniture i = FurnitureLeftOrDown; i != null; i = i.FurnitureLeftOrDown) {
				i.HighlightFrame = Game.GlobalFrame;
				i.HighlightBlinkOffset = HighlightBlinkOffset;
			}
			for (eFurniture i = FurnitureRightOrUp; i != null; i = i.FurnitureRightOrUp) {
				i.HighlightFrame = Game.GlobalFrame;
				i.HighlightBlinkOffset = HighlightBlinkOffset;
			}
		}


		#endregion




	}
}