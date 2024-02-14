using System.Collections;
using System.Collections.Generic;



namespace AngeliA.Framework {
	[EntityAttribute.MapEditorGroup("Furniture")]
	[EntityAttribute.Capacity(32)]
	public abstract class Furniture : EnvironmentEntity, IActionTarget {




		#region --- VAR ---


		// Api
		protected virtual Direction3 ModuleType => Direction3.None;
		protected virtual IRect RenderingRect => Rect.Expand(ColliderBorder);
		public Furniture FurnitureLeftOrDown { get; private set; } = null;
		public Furniture FurnitureRightOrUp { get; private set; } = null;
		protected FittingPose Pose { get; private set; } = FittingPose.Unknown;
		bool IActionTarget.IsHighlighted => GetIsHighlighted();

		// Data
		protected Int4 ColliderBorder = Int4.zero;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			Pose = FittingPose.Unknown;
			FurnitureLeftOrDown = null;
			FurnitureRightOrUp = null;
		}


		public override void FillPhysics () {
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, SpriteTag.ONEWAY_UP_TAG);
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			// Update Pose
			if (Pose == FittingPose.Unknown) {
				Pose = FittingPose.Single;

				if (ModuleType != Direction3.None) {
					Pose = GetEntityPose(
						this, ModuleType == Direction3.Horizontal, PhysicsMask.ENVIRONMENT,
						out var ld, out var ru, OperationMode.ColliderAndTrigger
					);
					FurnitureLeftOrDown = ld as Furniture;
					FurnitureRightOrUp = ru as Furniture;
				}

				// Shrink Rect
				var sprite = GetSpriteFromPose();
				if (sprite != null) {
					X -= (sprite.GlobalWidth - Width) / 2;
					Width = sprite.GlobalWidth;
					Height = sprite.GlobalHeight;
					ColliderBorder.left = sprite.GlobalBorder.left;
					ColliderBorder.right = sprite.GlobalBorder.right;
					ColliderBorder.down = sprite.GlobalBorder.down;
					ColliderBorder.up = sprite.GlobalBorder.up;
					X += ColliderBorder.left;
					Y += ColliderBorder.down;
					Width -= ColliderBorder.horizontal;
					Height -= ColliderBorder.vertical;
				}
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Pose == FittingPose.Unknown) return;
			var sprite = GetSpriteFromPose();
			if (sprite != null) {
				var cell = CellRenderer.Draw(sprite, RenderingRect);
				if ((this as IActionTarget).IsHighlighted) {
					BlinkCellAsFurniture(cell);
				}
			}
		}


		#endregion




		#region --- API ---


		void IActionTarget.Invoke () { }


		bool IActionTarget.AllowInvoke () => false;


		protected void DrawClockHands (IRect rect, int handCode, int thickness, int thicknessSecond) {
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
			float t11 = Util.Sin(Game.GlobalFrame * 6 * Util.Deg2Rad);
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


		protected AngeSprite GetSpriteFromPose () {
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, Pose switch {
				FittingPose.Left => 1,
				FittingPose.Mid => 2,
				FittingPose.Right => 3,
				FittingPose.Single => 0,
				_ => 0,
			}, out var sprite, false, true) ||
				CellRenderer.TryGetSprite(TypeID, out sprite)
			) return sprite;
			return null;
		}


		protected bool GetIsHighlighted () {
			if (Player.Selecting == null || Player.Selecting.TargetActionEntity == null) return false;
			var target = Player.Selecting.TargetActionEntity;
			if (target == this) return true;
			for (var f = FurnitureLeftOrDown; f != null; f = f.FurnitureLeftOrDown) {
				if (f == target) return true;
			}
			for (var f = FurnitureRightOrUp; f != null; f = f.FurnitureRightOrUp) {
				if (f == target) return true;
			}
			return false;
		}


		protected void BlinkCellAsFurniture (Cell cell) {
			float pivotX = 0.5f;
			if (ModuleType == Direction3.Horizontal) {
				if (Pose == FittingPose.Left) {
					pivotX = 1f;
				} else if (Pose == FittingPose.Right) {
					pivotX = 0f;
				}
			}
			bool useHorizontal = ModuleType != Direction3.Horizontal || Pose != FittingPose.Mid;
			bool useVertical = ModuleType != Direction3.Vertical || Pose == FittingPose.Up;
			IActionTarget.HighlightBlink(cell, pivotX, 0f, useHorizontal, useVertical);
		}


		#endregion




	}
}