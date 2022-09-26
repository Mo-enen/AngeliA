using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	public class eGuaGua : eCharacter {


		// Const
		private const int TARGET_DISTANCE_FAR = Const.CELL_SIZE * 3;
		private const int TARGET_DISTANCE_NEAR = Const.CELL_SIZE;

		// Api
		public bool Picking { get; set; } = false;

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

			int targetSize = 1000;

			// Find Yaya
			if (Yaya == null || !Yaya.Active) {
				Game.Current.TryGetEntityInStage(out Yaya);
			}
			if (Yaya == null || !Yaya.Active) {
				Active = false;
				return;
			}

			// HP
			if (Yaya.Health.HealthPoint != Health.HealthPoint) {
				Health.SetHealth(Yaya.Health.HealthPoint);
			}

			// Update
			switch (Yaya.CharacterState) {
				case CharacterState.GamePlay:
					if (Picking) {
						Update_Picking();
						targetSize = 850;
					} else {
						Update_Movement();
					}
					break;
				case CharacterState.Animate:
					if (CharacterState != CharacterState.Animate) {
						SetCharacterState(CharacterState.Animate);
					}
					break;
				case CharacterState.Sleep:
					if (CharacterState != CharacterState.Sleep) {
						SetCharacterState(CharacterState.Sleep);
						// Goto Basket
						if (Game.Current.TryGetEntityNearby<eBasket>(new(X, Y), out var basket)) {
							X = basket.X;
							Y = basket.Y;
						}
					}
					break;
				case CharacterState.Passout:
					if (CharacterState != CharacterState.Passout) {
						SetCharacterState(CharacterState.Passout);
					}
					break;
			}

			ArtworkScale = ArtworkScale.LerpTo(targetSize, 50);

		}


		private void Update_Picking () {

			ArtworkOffsetZ = Yaya.Movement.FacingFront ? 1 : -1;

			// State
			if (CharacterState != CharacterState.Animate) {
				SetCharacterState(CharacterState.Animate);
			}

			// Picking
			X = Yaya.X;
			Y = Yaya.Y + Const.CELL_SIZE / 4;

			// Position Lerp
			if (CellRenderer.TryGetMeta(Yaya.Renderer.BodyArtworkID, out var meta)) {
				var hand = Yaya.Movement.FacingRight ? meta.HandLeft : meta.HandRight;
				if (hand != null && hand.IsVailed) {
					int lerpX = Yaya.Attackness.IsAttacking ? 900 : 500;
					int lerpY = Yaya.Attackness.IsAttacking ? 300 : 500;
					var bounds = Yaya.GlobalBounds;
					int localX = hand.X + hand.Width / 2;
					X = X.LerpTo(Yaya.Movement.FacingRight ? bounds.x + localX : bounds.xMax - localX, lerpX);
					Y = Y.LerpTo(bounds.y + hand.Y, lerpY);
					var mState = Yaya.MovementState;
					if (mState == MovementState.Dash || mState == MovementState.Roll || mState == MovementState.Pound) {
						ArtworkOffsetZ = hand.Front ? 1 : -1;
					}
				}
			}

			// Animation
			Renderer.UpdateAnimation(TrimedTypeID, Game.GlobalFrame, 0, !Yaya.Movement.FacingRight);

		}


		private void Update_Movement () {

			// State
			if (CharacterState != CharacterState.GamePlay) {
				SetCharacterState(CharacterState.GamePlay);
			}

			// Free Move 
			ArtworkOffsetZ = -1;
			if (Yaya.IsGrounded && Util.SqrtDistance(Yaya.X, Yaya.Y, TargetX, TargetY) > TARGET_DISTANCE_FAR * TARGET_DISTANCE_FAR) {
				// Recalculate Target Position
				int deltaX = Yaya.X - X;
				int deltaY = Yaya.Y - Y;
				if (deltaX != 0 || deltaY != 0) {
					int dis = Util.BabyloniansSqrt(deltaX * deltaX + deltaY * deltaY);
					TargetX = Yaya.X - deltaX * TARGET_DISTANCE_NEAR / dis;
					TargetY = Yaya.Y - deltaY * TARGET_DISTANCE_NEAR / dis;
				} else {
					TargetX = Yaya.X;
					TargetY = Yaya.Y;
				}
			}

			// Move to Target Position




		}


	}
}
