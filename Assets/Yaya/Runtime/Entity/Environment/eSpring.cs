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
		private static readonly int[] BOUNCE_ANI = new int[] { 0, 1, 2, 3, 3, 3, 3, 2, 2, 1, 1, 0, };
		private static readonly Color RED = new(1f, 0.25f, 0.1f);
		private const int BOUNCE_DELAY = 0;
		private const int BOUNCE_COOLDOWN = 1;
		private const int RED_LINE_MIN = 196;
		private const int RED_LINE_MAX = 512;

		// Api
		public override int PhysicsLayer => YayaConst.LAYER_ENVIRONMENT;
		protected abstract bool Horizontal { get; }
		protected abstract int Power { get; }

		// Short
		private RectInt FullRect => new(X, Y, Const.CEL, Const.CEL);

		// Data
		private int LastBounceFrame = int.MinValue;
		private int CurrentArtworkFrame = 0;
		private bool RequireBouncePerform = false;
		private Direction4 BounceSide = default;


		// MSG
		public override void OnActived () {
			base.OnActived();
			Width = Horizontal ? Const.CEL - 64 : Const.CEL;
			Height = !Horizontal ? Const.CEL - 32 : Const.CEL;
			if (Horizontal) OffsetX = (Const.CEL - Width) / 2;
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
					if (CellPhysics.Overlap(
						YayaConst.MASK_RIGIDBODY,
						new(X - 1, Y, Const.CEL / 2, Const.CEL),
						this
					)) {
						StartBounce(frame, Direction4.Left);
					} else if (CellPhysics.Overlap(
						YayaConst.MASK_RIGIDBODY,
						new(X + Const.CEL / 2, Y, Const.CEL / 2 + 1, Const.CEL),
						this
					)) {
						StartBounce(frame, Direction4.Right);
					}
				} else {
					// Vert
					if (CellPhysics.Overlap(
						YayaConst.MASK_RIGIDBODY,
						new(X, Y + Const.CEL / 2, Const.CEL, Const.CEL / 2 + 1),
						this
					)) {
						StartBounce(frame, Direction4.Up);
					}
				}
			} else if (frame > LastBounceFrame + BOUNCE_DELAY && RequireBouncePerform) {
				// Try Perform Bounce
				var hit = CellPhysics.TouchTransfer<eYayaRigidbody>(
					YayaConst.MASK_RIGIDBODY,
					FullRect.Expand(Horizontal ? 1 : 0, Horizontal ? 1 : 0, Horizontal ? 0 : 1, Horizontal ? 0 : 1),
					this, BounceSide, 16
				);
				if (hit != null) PerformBounce(hit.Entity as eYayaRigidbody);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			var tint = (Color32)Color.Lerp(Const.WHITE, RED, Mathf.InverseLerp(RED_LINE_MIN, RED_LINE_MAX, Power));
			if (Game.GlobalFrame < LastBounceFrame + BOUNCE_ANI.Length) {
				CurrentArtworkFrame++;
			} else {
				CurrentArtworkFrame = 0;
			}
			int frame = CurrentArtworkFrame.UMod(BOUNCE_ANI.Length);
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, BOUNCE_ANI[frame], out var sprite, false, true)) {
				CellRenderer.Draw(
					sprite.GlobalID,
					X + Const.CEL / 2, Y,
					500, 0, 0,
					Const.CEL, Const.CEL, tint
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
