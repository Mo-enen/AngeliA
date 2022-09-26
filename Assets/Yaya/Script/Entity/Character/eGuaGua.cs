using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.ForceUpdate]
	public class eGuaGua : eCharacter {




		#region --- VAR ---


		// Api
		public bool Picking { get; set; } = false;

		// Data
		private eYaya Yaya = null;
		private int TargetX = 0;
		private int TargetY = 0;
		private bool Flying = false;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			TargetX = X;
			TargetY = Y;
			Flying = false;
		}


		public override void FillPhysics () {
			if (!Picking) base.FillPhysics();
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();

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
			int targetSize = 1000;
			switch (Yaya.CharacterState) {
				case CharacterState.GamePlay:
					if (Picking) {
						SetCharacterState(CharacterState.Animate);
						Update_Picking();
						targetSize = 850;
					} else {
						Update_Moving();
					}
					break;
				case CharacterState.Animate:
					SetCharacterState(CharacterState.Animate);
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
					SetCharacterState(CharacterState.Passout);
					break;
			}

			Renderer.ArtworkScale = Renderer.ArtworkScale.LerpTo(targetSize, 50);

		}


		private void Update_Picking () {

			Renderer.ArtworkOffsetZ = Yaya.Movement.FacingFront ? 1 : -1;

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
						Renderer.ArtworkOffsetZ = hand.Front ? 1 : -1;
					}
				}
			}

			// Animation
			Renderer.UpdateAnimation(TrimedTypeID, Game.GlobalFrame, 0, !Yaya.Movement.FacingRight);

		}


		private void Update_Moving () {

			const int TARGET_DIS_FAR = Const.CELL_SIZE * 4;
			const int TARGET_DIS_NEAR = Const.CELL_SIZE * 2;

			Renderer.ArtworkOffsetZ = 1;
			SetCharacterState(CharacterState.Animate);

			// Fly Check
			bool oldFlying = Flying;
			Flying = FlyCheck(Flying);
			if (Flying && !oldFlying) {
				// Take Off

				Renderer.Bounce();
			}

			// Get Target Position
			if (Yaya.IsGrounded && Util.SqrtDistance(Yaya.X, Yaya.Y, TargetX, TargetY) > TARGET_DIS_FAR * TARGET_DIS_FAR) {
				TargetX = Yaya.X;
				TargetY = Yaya.Y;
			}

			// Too Close Check
			bool tooClose = Util.SqrtDistance(Yaya.X, Yaya.Y, X, Y) < TARGET_DIS_NEAR * TARGET_DIS_NEAR;





		}


		#endregion




		#region --- LGC ---


		private bool FlyCheck (bool isFlying) {
			if (!isFlying) {
				// Fly Start Check
				// Fly when Outside Camera
				var cRect = CellRenderer.CameraRect;
				if (!cRect.Contains(X, Y)) return true;


			} else {
				// Fly Stop Check



			}
			return isFlying;
		}


		#endregion




	}
}
