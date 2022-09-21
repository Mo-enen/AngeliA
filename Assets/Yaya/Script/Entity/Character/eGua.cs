using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	public class eGua : eCharacter {


		// Const
		private const int TARGET_DISTANCE_FAR = Const.CELL_SIZE * 3;
		private const int TARGET_DISTANCE_NEAR = Const.CELL_SIZE;

		// Api
		public bool Picking { get; set; } = true;
		public override bool FacingFront => Yaya == null || Yaya.FacingFront;
		public override bool FacingRight => Yaya == null || Yaya.FacingRight;

		// Data
		private eYaya Yaya = null;
		private int TargetX = 0;
		private int TargetY = 0;


		// MSG
		public override void OnActived () {
			base.OnActived();
			TargetX = X;
			TargetY = Y;
		}


		public override void FillPhysics () {
			if (!Picking) base.FillPhysics();
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();

			if (Yaya == null || !Yaya.Active) {
				Game.Current.TryGetEntityInStage(out Yaya);
			}
			if (Yaya == null || !Yaya.Active) {
				Active = false;
				return;
			}

			Update_Sync();

			if (CharacterState == CharacterState.GamePlay) {
				if (Picking) {
					Update_Picking();
				} else {
					Update_Movement();
				}
			}
		}


		private void Update_Sync () {
			// Size
			ArtworkScale = Picking ? 900 : 1000;
			// HP
			if (Yaya.HealthPoint != HealthPoint) {
				InvokeSetHealth(Yaya.HealthPoint);
			}
			// State
			if (CharacterState != CharacterState.Animate) {
				SetCharacterState(CharacterState.Animate);
			}
		}


		private void Update_Picking () {
			ArtworkOffsetZ = FacingFront ? 1 : -1;
			// Picking
			X = Yaya.X;
			Y = Yaya.Y + Const.CELL_SIZE / 4;
			// Position Lerp
			if (CellRenderer.TryGetMeta(Yaya.BodyArtworkID, out var meta)) {
				var hand = FacingRight ? meta.HandLeft : meta.HandRight;
				if (hand != null && hand.IsVailed) {
					int lerpX = Yaya.IsAttacking ? 900 : 500;
					int lerpY = Yaya.IsAttacking ? 300 : 500;
					var bounds = Yaya.GlobalBounds;
					int localX = hand.X + hand.Width / 2;
					X = X.LerpTo(FacingRight ? bounds.x + localX : bounds.xMax - localX, lerpX);
					Y = Y.LerpTo(bounds.y + hand.Y, lerpY);
					var mState = Yaya.MovementState;
					if (mState == MovementState.Dash || mState == MovementState.Roll || mState == MovementState.Pound) {
						ArtworkOffsetZ = hand.Front ? 1 : -1;
					}
				}
			}
		}


		private void Update_Movement () {
			// Free Move 
			ArtworkOffsetZ = -1;
			if (Yaya.IsGrounded && Util.SqrtDistance(Yaya.X, Yaya.Y, TargetX, TargetY) > TARGET_DISTANCE_FAR * TARGET_DISTANCE_FAR) {
				// Recalculate Target Position
				int deltaX = Yaya.X - X;
				int deltaY = Yaya.Y - Y;
				int dis = Util.BabyloniansSqrt(deltaX * deltaX + deltaY * deltaY);
				TargetX = Yaya.X - deltaX * TARGET_DISTANCE_NEAR / dis;
				TargetY = Yaya.Y - deltaY * TARGET_DISTANCE_NEAR / dis;
			}
			// Move to Target Position




		}


	}
}
