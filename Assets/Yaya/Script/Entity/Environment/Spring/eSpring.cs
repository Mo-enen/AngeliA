using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eSpring : eRigidbody {


		// Const
		private static readonly int[] METAL_CODE = new int[] { "Spring Metal 0".AngeHash(), "Spring Metal 1".AngeHash(), "Spring Metal 2".AngeHash(), "Spring Metal 3".AngeHash(), };
		private static readonly int[] WOOD_CODE = new int[] { "Spring Wood 0".AngeHash(), "Spring Wood 1".AngeHash(), "Spring Wood 2".AngeHash(), "Spring Wood 3".AngeHash(), };
		private static readonly int[] BOUNCE_ANI = new int[] { 0, 1, 2, 3, 3, 3, 3, 3, 2, 2, 2, 1, 1, 0, };
		private static readonly Color RED = new(1f, 0.25f, 0.1f);
		private const int BOUNCE_DELY = 1;
		private const int BOUNCE_COOLDOWN = 9;
		private const int METAL_LINE = 128;
		private const int RED_LINE_MIN = 196;
		private const int RED_LINE_MAX = 512;

		// Api
		public override int Layer => (int)EntityLayer.Environment;
		public override int CollisionLayer => (int)PhysicsLayer.Environment;
		protected abstract bool Horizontal { get; }
		public override int PushLevel => Horizontal ? int.MaxValue - 1 : 1;
		public override RectInt Bounds => new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE);

		// Short
		private int Power => Data;
		private bool IsMetal => Power >= METAL_LINE;
		private RectInt FullRect => new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE);

		// Data
		private readonly HitInfo[] c_PerformBounce = new HitInfo[64];
		private int LastBounceFrame = -BOUNCE_COOLDOWN;
		private bool RequireBouncePerform = false;
		private Direction4 BounceSide = default;


		// MSG
		public override void OnCreate (int frame) {
			base.OnCreate(frame);
			Width = Horizontal ? Const.CELL_SIZE / 2 : Const.CELL_SIZE;
			Height = !Horizontal ? Const.CELL_SIZE / 2 : Const.CELL_SIZE;
			if (Horizontal) OffsetX = Const.CELL_SIZE / 4;
		}


		public override void PhysicsUpdate (int frame) {
			base.PhysicsUpdate(frame);
			if (frame > LastBounceFrame + BOUNCE_COOLDOWN) {
				// Check for Bounce
				RequireBouncePerform = false;
				if (Horizontal) {
					// Hori
					if (CellPhysics.Overlap((int)PhysicsMask.Rigidbody, new RectInt(X, Y, 1, Const.CELL_SIZE), this)) {
						StartBounce(frame, Direction4.Left);
					} else if (CellPhysics.Overlap((int)PhysicsMask.Rigidbody, new RectInt(X + Const.CELL_SIZE - 1, Y, 1, Const.CELL_SIZE), this)) {
						StartBounce(frame, Direction4.Right);
					}
				} else {
					// Vert
					if (CellPhysics.Overlap((int)PhysicsMask.Rigidbody, new(X, Y + Const.CELL_SIZE - 1, Const.CELL_SIZE, 1), this)) {
						StartBounce(frame, Direction4.Up);
					}
				}
			} else if (frame > LastBounceFrame + BOUNCE_DELY && RequireBouncePerform) {
				// Try Perform Bounce
				int count = CellPhysics.ForAllTouched<eRigidbody>(
					c_PerformBounce, (int)PhysicsMask.Rigidbody, FullRect, this, BounceSide
				);
				for (int i = count - 1; i >= 0; i--) {
					if (c_PerformBounce[i].Entity is eRigidbody rig) {
						if (!Horizontal) {
							rig.PerformMove(0, i * (Power - Const.GRAVITY), true, true);
						}
						PerformBounce(rig);
					}
				}
				c_PerformBounce.Dispose();
			}
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
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
