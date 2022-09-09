using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class eSpringWoodHorizontal : eSpring {
		protected override bool Horizontal => true;
		protected override int Power => 64;
	}


	public class eSpringWoodVertical : eSpring {
		protected override bool Horizontal => false;
		protected override int Power => 64;
	}


	public class eSpringMetalHorizontal : eSpring {
		protected override bool Horizontal => true;
		protected override int Power => 128;
	}


	public class eSpringMetalVertical : eSpring {
		protected override bool Horizontal => false;
		protected override int Power => 128;
	}


	public abstract class eSpring : eYayaRigidbody {


		// Const
		private static readonly int[] METAL_CODE = new int[] { "Spring Metal 0".AngeHash(), "Spring Metal 1".AngeHash(), "Spring Metal 2".AngeHash(), "Spring Metal 3".AngeHash(), };
		private static readonly int[] WOOD_CODE = new int[] { "Spring Wood 0".AngeHash(), "Spring Wood 1".AngeHash(), "Spring Wood 2".AngeHash(), "Spring Wood 3".AngeHash(), };
		private static readonly int[] BOUNCE_ANI = new int[] { 0, 1, 2, 3, 3, 3, 3, 2, 2, 1, 1, 0, };
		private static readonly Color RED = new(1f, 0.25f, 0.1f);
		private const int BOUNCE_DELY = 0;
		private const int BOUNCE_COOLDOWN = 1;
		private const int METAL_LINE = 96;
		private const int RED_LINE_MIN = 196;
		private const int RED_LINE_MAX = 512;

		// Api
		public override int PhysicsLayer => YayaConst.LAYER_ENVIRONMENT;
		protected abstract bool Horizontal { get; }
		protected abstract int Power { get; }

		// Short
		private bool IsMetal => Power >= METAL_LINE;
		private RectInt FullRect => new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE);

		// Data
		private int LastBounceFrame = int.MinValue;
		private int CurrentArtworkFrame = 0;
		private bool RequireBouncePerform = false;
		private Direction4 BounceSide = default;


		// MSG
		public override void OnActived () {
			base.OnActived();
			Width = Horizontal ? Const.CELL_SIZE - 64 : Const.CELL_SIZE;
			Height = !Horizontal ? Const.CELL_SIZE - 32 : Const.CELL_SIZE;
			if (Horizontal) OffsetX = (Const.CELL_SIZE - Width) / 2;
			LastBounceFrame = int.MinValue;
			RequireBouncePerform = false;
			BounceSide = default;
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			int frame = Game.GlobalFrame;
			if (frame > LastBounceFrame + BOUNCE_COOLDOWN) {
				// Check for Bounce
				RequireBouncePerform = false;
				if (Horizontal) {
					// Hori
					if (Physics.Overlap(
						YayaConst.MASK_RIGIDBODY,
						new(X - 1, Y, Const.CELL_SIZE / 2, Const.CELL_SIZE),
						this
					)) {
						StartBounce(frame, Direction4.Left);
					} else if (Physics.Overlap(
						YayaConst.MASK_RIGIDBODY,
						new(X + Const.CELL_SIZE / 2, Y, Const.CELL_SIZE / 2 + 1, Const.CELL_SIZE),
						this
					)) {
						StartBounce(frame, Direction4.Right);
					}
				} else {
					// Vert
					if (Physics.Overlap(
						YayaConst.MASK_RIGIDBODY,
						new(X, Y + Const.CELL_SIZE / 2, Const.CELL_SIZE, Const.CELL_SIZE / 2 + 1),
						this
					)) {
						StartBounce(frame, Direction4.Up);
					}
				}
			} else if (frame > LastBounceFrame + BOUNCE_DELY && RequireBouncePerform) {
				// Try Perform Bounce
				var hit = Physics.GetLastTouched<eYayaRigidbody>(
					YayaConst.MASK_RIGIDBODY,
					FullRect.Expand(Horizontal ? 1 : 0, Horizontal ? 1 : 0, Horizontal ? 0 : 1, Horizontal ? 0 : 1),
					this, BounceSide, 16
				);
				if (hit != null) PerformBounce(hit.Entity as eYayaRigidbody);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			var codes = IsMetal ? METAL_CODE : WOOD_CODE;
			var tint = IsMetal ?
				(Color32)Color.Lerp(Const.WHITE, RED, Mathf.InverseLerp(RED_LINE_MIN, RED_LINE_MAX, Power)) :
				Const.WHITE;
			if (Game.GlobalFrame < LastBounceFrame + BOUNCE_ANI.Length) {
				CurrentArtworkFrame++;
			} else {
				CurrentArtworkFrame = 0;
			}
			int frame = CurrentArtworkFrame.UMod(BOUNCE_ANI.Length);
			if (Horizontal) {
                // Hori
                AngeliaFramework.Renderer.Draw(
					globalID: codes[BOUNCE_ANI[frame]],
					x: X + Const.CELL_SIZE / 2,
					y: Y + Const.CELL_SIZE / 2,
					pivotX: 500,
					pivotY: 500,
					rotation: BounceSide == Direction4.Left ? -90 : 90,
					width: -Const.CELL_SIZE, height: Const.CELL_SIZE, color: tint
				);
			} else {
                // Vert
                AngeliaFramework.Renderer.Draw(
					codes[BOUNCE_ANI[frame]],
                    X + Const.CELL_SIZE / 2, Y,
					500, 0, 0,
                    Const.CELL_SIZE, Const.CELL_SIZE, tint
				);
			}
		}


		// LGC
		private void StartBounce (int frame, Direction4 side) {
			LastBounceFrame = frame;
			BounceSide = side;
			RequireBouncePerform = true;
		}


		private void PerformBounce (eYayaRigidbody target) {
			RequireBouncePerform = false;
			if (Horizontal) {
				// Horizontal
				if (BounceSide == Direction4.Left) {
					if (target.VelocityX > -Power) target.VelocityX = -Power;
				} else {
					if (target.VelocityX < Power) target.VelocityX = Power;
				}
			} else {
				// Vertical
				if (target.VelocityY < Power) target.VelocityY = Power;
				target.MakeGrounded(6);
			}
		}



	}
}
