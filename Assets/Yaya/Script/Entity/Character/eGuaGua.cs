using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.ForceUpdate]
	public class eGuaGua : eYayaRigidbody {




		#region --- VAR ---


		// Const
		private const int FLY_SHIFT_Y = Const.CELL_SIZE * 2;
		private const int TARGET_DIS_FAR = Const.CELL_SIZE * 8;
		private const int TARGET_DIS_NEAR = Const.CELL_SIZE * 4;
		private const int FLY_CLOSE_DURATION = 60;
		private static readonly int IDLE_CODE = "_aGuaGua.Idle".AngeHash();
		private static readonly int FLY_CODE = "_aGuaGua.Fly".AngeHash();
		private static readonly int SLEEP_CODE = "_aGuaGua.Sleep".AngeHash();

		// Api
		public override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override int CollisionMask => YayaConst.MASK_MAP;

		// Short
		private bool TooCloseToYaya => Util.SqrtDistance(Yaya.X, Yaya.Y, X, Y) < TARGET_DIS_NEAR * TARGET_DIS_NEAR;
		private bool TooFarToYaya => Util.SqrtDistance(Yaya.X, Yaya.Y, X, Y) > TARGET_DIS_FAR * TARGET_DIS_FAR;

		// Data
		private Movement Movement = null;
		private eYaya Yaya = null;
		private bool Flying = false;
		private int TargetX = 0;
		private int TargetY = 0;
		private int TargetFlyY = 0;
		private int CloseYayaFrame = 0;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {
			base.OnInitialize();
			string typeName = GetType().Name;
			if (typeName.StartsWith("e")) typeName = typeName[1..];
			Movement = Game.Current.LoadMeta<Movement>(typeName, "Movement") ?? new();
		}


		public override void OnActived () {
			base.OnActived();
			TargetX = X;
			TargetY = Y;
			TargetFlyY = Y + FLY_SHIFT_Y;
			Flying = false;
			CloseYayaFrame = 0;
			Movement.OnActived(this);
		}


		public override void FillPhysics () {
			if (!Flying) base.FillPhysics();
		}


		public override void PhysicsUpdate () {
			if (!Flying) base.PhysicsUpdate();

			// Find Yaya
			if (Yaya == null || !Yaya.Active) {
				Game.Current.TryGetEntityInStage(out Yaya);
			}
			if (Yaya == null || !Yaya.Active) {
				Active = false;
				return;
			}

			// Update
			switch (Yaya.CharacterState) {
				case CharacterState.GamePlay:
				case CharacterState.Passout:
					Update_Gameplay();
					break;
				case CharacterState.Sleep:
					// Try Goto Basket
					if (Game.Current.TryGetEntityNearby<eBasket>(new(X, Y), out var basket)) {
						X = basket.X;
						Y = basket.Y + basket.Height;
					}

					break;
			}

		}


		private void Update_Gameplay () {

			bool tooFar = TooFarToYaya;
			bool tooClose = TooCloseToYaya;
			bool isGrounded = GroundedCheck(Rect);

			CloseYayaFrame = (tooClose ? CloseYayaFrame + 1 : CloseYayaFrame - 1).Clamp(0, FLY_CLOSE_DURATION);

			// Refresh Flying State 
			bool oldFlying = Flying;
			if (!oldFlying) {
				// Start Fly when Outside Camera
				if (!Game.Current.ViewRect.Contains(X, Y)) {
					Flying = true;
				}
				// Start Fly when Too Far to Yaya and Not Grounded
				if (isGrounded && tooFar) {
					Flying = true;
				}
			} else {
				// Stop Fly when Too Close to Yaya and Have Block to Land
				if (CloseYayaFrame >= FLY_CLOSE_DURATION && YayaCellPhysics.HasMapBlockUnder(X, Y)) {
					Flying = false;
				}
			}




			//////////////////////////////////////////////////
			//Flying = true;
			//////////////////////////////////////////////////




			// Get Target Position
			if (Yaya.IsGrounded && Util.SqrtDistance(Yaya.X, Yaya.Y, TargetX, TargetY) > TARGET_DIS_FAR * TARGET_DIS_FAR) {
				TargetX = Yaya.X;
				TargetY = Yaya.Y;
				TargetFlyY = Yaya.Y + FLY_SHIFT_Y;
			}

			// Path Finding and Move
			if (!Flying) {
				// Run



				Movement.Update();
			} else {
				// Fly
				const int FLY_SPEED_X = 16;
				const int FLY_SPEED_Y = 12;
				int speedX = FLY_SPEED_X;
				int speedY = FLY_SPEED_Y;




				X = X.MoveTowards(TargetX, speedX);
				Y = Y.MoveTowards(TargetFlyY, speedY);
			}

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Yaya.CharacterState != CharacterState.Sleep) {
				if (!Flying) {
					// Run
					CellRenderer.Draw_Animation(
						IDLE_CODE, X, Y, 500, 0, 0,
						Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
						Const.ORIGINAL_SIZE,
						Game.GlobalFrame, 0
					);
				} else {
					// Fly
					CellRenderer.Draw_Animation(
						FLY_CODE, X, Y, 500, 0, 0,
						Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
						Const.ORIGINAL_SIZE,
						Game.GlobalFrame, 0
					);
				}
			} else {
				// Sleep
				CellRenderer.Draw_Animation(
					SLEEP_CODE, X, Y,
					Const.ORIGINAL_SIZE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame, 0
				);
			}
		}


		#endregion




		#region --- LGC ---






		#endregion




	}
}
