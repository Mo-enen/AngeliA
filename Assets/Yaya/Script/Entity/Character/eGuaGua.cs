using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.EntityCapacity(1)]
	public class eGuaGua : eCharacter {




		#region --- VAR ---


		// Api
		public override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override int CollisionMask => YayaConst.MASK_MAP;
		public bool Fed { get; private set; } = false;

		// Data
		private eYaya Yaya = null;


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
					break;
				case CharacterState.Passout:
					VelocityX = 0;
					break;
			}
		}


		private void Update_FollowYaya () {
			if (!Yaya.Active) return;




			Movement.FacingRight = Yaya.X >= X;
			MovementState = MovementState.Fly;
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


		#endregion




	}
}
