using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.Capacity(1)]
	public class eGuaGua : eCharacter {




		#region --- VAR ---


		// Api
		public override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override int CollisionMask => YayaConst.MASK_MAP;
		public bool Fed { get; private set; } = true;

		// Data
		private eYaya Yaya = null;
		private Vector2Int PrevPosition = default;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {
			base.OnInitialize();
			Yaya = Game.Current.PeekOrGetEntity<eYaya>();
		}


		public override void FillPhysics () {
			if (CharacterState == CharacterState.GamePlay) {
				base.FillPhysics();
			}
		}


		public override void PhysicsUpdate () {
			PrevPosition.x = X;
			PrevPosition.y = Y;
			bool stateChanged = CharacterState != Yaya.CharacterState;
			if (stateChanged) SetCharacterState(Yaya.CharacterState);
			Health.SetHealth(Yaya.Health.EmptyHealth ? 0 : Health.MaxHP);
			switch (CharacterState) {
				case CharacterState.GamePlay:
					if (Fed) {
						Update_FollowYaya();
					} else {
						Update_FreeMove();
						base.PhysicsUpdate();
					}
					break;
				case CharacterState.Sleep:
					if (stateChanged && Game.Current.TryGetEntityNearby<eBasket>(new(X, Y), out var basket)) {
						X = basket.X + basket.Width / 2;
						Y = basket.Y + basket.Height - OffsetY;
					}
					if (SleepAmount >= 1000) Fed = false;
					base.PhysicsUpdate();
					break;
				case CharacterState.Passout:
					VelocityX = 0;
					break;
			}
		}


		private void Update_FollowYaya () {

			if (!Yaya.Active) return;

			int TARGET_OFFSET = Width / 2;

			Movement.FacingRight = Yaya.X >= X;
			MovementState = MovementState.Fly;
			var yayaRect = Yaya.Rect;
			int targetX = Yaya.Movement.FacingRight ? yayaRect.xMin - TARGET_OFFSET : yayaRect.xMax + TARGET_OFFSET;
			int targetY = yayaRect.yMax + Const.CEL / 3;

			// Move
			const int SLOW_DOWN_RANGE_X = Const.CEL * 2;
			const int SLOW_DOWN_RANGE_Y = Const.CEL * 1;
			int disX = Mathf.Abs(targetX - X);
			int disY = Mathf.Abs(targetY - Y);
			if (disX > SLOW_DOWN_RANGE_X || disY > SLOW_DOWN_RANGE_Y) {
				// Move to Target
				const int FORCE = 3;
				const int AIR = 1;
				int maxSpeedX = Util.Remap(
					SLOW_DOWN_RANGE_X,
					SLOW_DOWN_RANGE_X * 2,
					Yaya.Movement.RunSpeed,
					Yaya.Movement.RunSpeed + 24,
					disX
				);
				const int MAX_SPEED_Y = 42;
				int forceX = targetX - X;
				int forceY = targetY - Y;
				int len = Util.DistanceInt(forceX, forceY, 0, 0);
				forceX = forceX * FORCE / len;
				forceY = forceY * FORCE / len;
				forceX -= VelocityX.Clamp(-AIR, AIR);
				forceY -= VelocityY.Clamp(-AIR, AIR);
				VelocityX = (VelocityX + forceX).Clamp(-maxSpeedX, maxSpeedX);
				VelocityY = (VelocityY + forceY).Clamp(-MAX_SPEED_Y, MAX_SPEED_Y);
			} else {
				// Slow Down
				VelocityX = VelocityX.MoveTowards(0, 2);
				VelocityY = VelocityY.MoveTowards((targetY - Y).Clamp(-7, 7), 2);
			}

			// Apply
			X += VelocityX;
			Y += VelocityY;
		}


		private void Update_FreeMove () {
			// Find Target


			// Move/Jump to Target


		}


		#endregion




		#region --- API ---


		public void Feed () {
			Fed = true;



		}


		public void ReturnToPrevPos () {
			X = PrevPosition.x;
			Y = PrevPosition.y;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}
