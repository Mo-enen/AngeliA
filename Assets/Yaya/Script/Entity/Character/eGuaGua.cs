using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.EntityCapacity(1)]
	public class eGuaGua : eYayaRigidbody {




		#region --- VAR ---


		// Const
		private static readonly int IDLE_CODE = "_aGuaGua.Idle".AngeHash();
		private static readonly int FLY_CODE = "_aGuaGua.Fly".AngeHash();
		private static readonly int SLEEP_CODE = "_aGuaGua.Sleep".AngeHash();
		private const int FLY_SHIFT_Y = Const.CELL_SIZE * 2;
		private const int TARGET_DIS_FAR = Const.CELL_SIZE * 8;
		private const int TARGET_DIS_NEAR = Const.CELL_SIZE * 4;
		private const int FLY_CLOSE_DURATION = 60;

		// Api
		public override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override int CollisionMask => YayaConst.MASK_MAP;
		public bool Fed { get; private set; } = false;

		// Short
		private eYaya Yaya {
			get {
				if (_Yaya == null) {
					_Yaya ??= Game.Current.PeekEntityInPool<eYaya>();
					_Yaya ??= Game.Current.GetEntityInStage<eYaya>();
				}
				return _Yaya;
			}
		}

		// Data
		private Movement Movement = null;
		private Attackness Attackness = null;
		private eYaya _Yaya = null;
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
			Attackness = Game.Current.LoadMeta<Attackness>(typeName, "Attackness") ?? new();
			Fed = false;
		}


		public override void OnActived () {
			base.OnActived();
			TargetX = X;
			TargetY = Y;
			TargetFlyY = Y + FLY_SHIFT_Y;
			Flying = false;
			CloseYayaFrame = 0;
			Movement.OnActived(this);
			Attackness.OnActived(this);
		}


		public override void FillPhysics () {
			if (!Flying) base.FillPhysics();
		}


		// Physics
		public override void PhysicsUpdate () {

			if (!Flying) base.PhysicsUpdate();

			// Update
			switch (Yaya.CharacterState) {
				case CharacterState.GamePlay:
				case CharacterState.Passout:
					if (Yaya.Active && Fed) {
						Update_FollowYaya();
					} else {
						Update_FreeMove();
					}
					break;
				case CharacterState.Sleep:
					// Try Goto Basket
					if (Game.Current.TryGetEntityNearby<eBasket>(new(X, Y), out var basket)) {
						X = basket.X;
						Y = basket.Y + basket.Height;
					}
					Fed = false;
					break;
			}

		}


		private void Update_FollowYaya () {

			bool tooFar = Util.SqrtDistance(Yaya.X, Yaya.Y, X, Y) > TARGET_DIS_FAR * TARGET_DIS_FAR;
			bool tooClose = Util.SqrtDistance(Yaya.X, Yaya.Y, X, Y) < TARGET_DIS_NEAR * TARGET_DIS_NEAR;
			bool isGrounded = !Flying && IsGrounded;

			CloseYayaFrame = (tooClose ? CloseYayaFrame + 1 : CloseYayaFrame - 1).Clamp(0, FLY_CLOSE_DURATION);

			// Refresh Flying State 
			bool oldFlying = Flying;
			if (!oldFlying) {
				// Start Fly when Outside Camera
				if (!Game.Current.ViewRect.Overlaps(Rect)) {
					Flying = true;
				}
			} else {
				// Stop Fly when Too Close to Yaya and Have Block to Land
				if (
					CloseYayaFrame >= FLY_CLOSE_DURATION &&
					YayaCellPhysics.TryGetMapBlockUnder(X, Y, out var block) &&
					block.Rect.y >= Yaya.Y - Const.CELL_SIZE
				) {
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


		private void Update_FreeMove () {



		}


		// Render
		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Yaya.CharacterState != CharacterState.Sleep) {
				if (!Flying) {
					// Run
					CellRenderer.Draw_Animation(
						IDLE_CODE, X + Width / 2, Y, 500, 0, 0,
						Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
						Const.ORIGINAL_SIZE,
						Game.GlobalFrame, 0
					);
				} else {
					// Fly
					CellRenderer.Draw_Animation(
						FLY_CODE, X + Width / 2, Y, 500, 0, 0,
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




		#region --- API ---


		public void Feed () {
			Fed = true;



		}


		#endregion




	}
}
