using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eSpringWoodHorizontal : eSpring {
		protected override bool Horizontal => true;
		protected override int Power => 96;
	}
	public class eSpringWoodVertical : eSpring {
		protected override bool Horizontal => false;
		protected override int Power => 96;
	}
	public class eSpringMetalHorizontal : eSpring {
		protected override bool Horizontal => true;
		protected override int Power => 164;
	}
	public class eSpringMetalVertical : eSpring {
		protected override bool Horizontal => false;
		protected override int Power => 164;
	}
	public abstract class eSpring : eRigidbody {


		// Const
		private static readonly int[] METAL_CODE = new int[] { "Spring Metal 0".AngeHash(), "Spring Metal 1".AngeHash(), "Spring Metal 2".AngeHash(), "Spring Metal 3".AngeHash(), };
		private static readonly int[] WOOD_CODE = new int[] { "Spring Wood 0".AngeHash(), "Spring Wood 1".AngeHash(), "Spring Wood 2".AngeHash(), "Spring Wood 3".AngeHash(), };
		private static readonly int[] BOUNCE_ANI = new int[] { 0, 1, 2, 3, 3, 3, 3, 3, 2, 2, 2, 1, 1, 0, };
		private static readonly Color RED = new(1f, 0.25f, 0.1f);
		private const int BOUNCE_DELY = 0;
		private const int BOUNCE_COOLDOWN = 1;
		private const int METAL_LINE = 128;
		private const int RED_LINE_MIN = 196;
		private const int RED_LINE_MAX = 512;

		// Api
		public override int PhysicsLayer => YayaConst.ENVIRONMENT;
		protected abstract bool Horizontal { get; }
		protected abstract int Power { get; }

		// Short
		private bool IsMetal => Power >= METAL_LINE;
		private RectInt FullRect => new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE);

		// Data
		private int LastBounceFrame = -BOUNCE_COOLDOWN;
		private bool RequireBouncePerform = false;
		private Direction4 BounceSide = default;


		// MSG
		public override void OnActived () {
			base.OnActived();
			Width = Horizontal ? Const.CELL_SIZE / 2 : Const.CELL_SIZE;
			Height = !Horizontal ? Const.CELL_SIZE / 2 : Const.CELL_SIZE;
			if (Horizontal) OffsetX = Const.CELL_SIZE / 4;
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			int frame = Game.GlobalFrame;
			if (frame > LastBounceFrame + BOUNCE_COOLDOWN) {
				// Check for Bounce
				RequireBouncePerform = false;
				if (Horizontal) {
					// Hori
					if (CellPhysics.Overlap(
						YayaConst.MASK_RIGIDBODY,
						new(X - 1, Y, Const.CELL_SIZE / 2, Const.CELL_SIZE),
						this
					)) {
						StartBounce(frame, Direction4.Left);
					} else if (CellPhysics.Overlap(
						YayaConst.MASK_RIGIDBODY,
						new(X + Const.CELL_SIZE / 2, Y, Const.CELL_SIZE / 2 + 1, Const.CELL_SIZE),
						this
					)) {
						StartBounce(frame, Direction4.Right);
					}
				} else {
					// Vert
					if (CellPhysics.Overlap(
						YayaConst.MASK_RIGIDBODY,
						new(X, Y + Const.CELL_SIZE / 2, Const.CELL_SIZE, Const.CELL_SIZE / 2 + 1),
						this
					)) {
						StartBounce(frame, Direction4.Up);
					}
				}
			} else if (frame > LastBounceFrame + BOUNCE_DELY && RequireBouncePerform) {
				// Try Perform Bounce
				var hit = CellPhysics.GetLastTouched<eRigidbody>(
					YayaConst.MASK_RIGIDBODY,
					FullRect.Expand(Horizontal ? 1 : 0, Horizontal ? 1 : 0, Horizontal ? 0 : 1, Horizontal ? 0 : 1),
					this, BounceSide, 16
				);
				if (hit != null) PerformBounce(hit.Entity as eRigidbody);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			int frame = Game.GlobalFrame;
			var codes = IsMetal ? METAL_CODE : WOOD_CODE;
			var tint = IsMetal ?
				(Color32)Color.Lerp(Const.WHITE, RED, Mathf.InverseLerp(RED_LINE_MIN, RED_LINE_MAX, Power)) :
				Const.WHITE;
			if (Horizontal) {
				// Hori
				int rot = 90;
				if (frame < LastBounceFrame + BOUNCE_COOLDOWN) {
					rot = BounceSide == Direction4.Left ? -90 : 90;
				}
				CellRenderer.Draw(
					codes[BOUNCE_ANI[Mathf.Clamp(frame - LastBounceFrame, 0, BOUNCE_ANI.Length - 1)]],
					X + Const.CELL_SIZE / 2,
					Y + Const.CELL_SIZE / 2,
					500, 500, rot,
					-Const.CELL_SIZE, Const.CELL_SIZE, tint
				);
			} else {
				// Vert
				CellRenderer.Draw(
					codes[BOUNCE_ANI[Mathf.Clamp(frame - LastBounceFrame, 0, BOUNCE_ANI.Length - 1)]],
					X + Const.CELL_SIZE / 2, Y,
					500, 0, 0,
					Const.CELL_SIZE, Const.CELL_SIZE, tint
				);
			}
		}


		private void StartBounce (int frame, Direction4 side) {
			LastBounceFrame = frame;
			BounceSide = side;
			RequireBouncePerform = true;
		}


		private void PerformBounce (eRigidbody target) {
			RequireBouncePerform = false;
			if (Horizontal) {
				if (BounceSide == Direction4.Left) {
					if (target.VelocityX > -Power) target.VelocityX = -Power;
				} else {
					if (target.VelocityX < Power) target.VelocityX = Power;
				}
			} else {
				if (target.VelocityY < Power) target.VelocityY = Power;
			}
		}



	}
}
