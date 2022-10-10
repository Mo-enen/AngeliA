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
		public Navigation Navigation { get; private set; } = null;

		// Data
		private eYaya Yaya = null;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {
			base.OnInitialize();
			Yaya = Game.Current.PeekOrGetEntity<eYaya>();
			Navigation = Game.Current.LoadOrCreateMeta<Navigation>("GuaGua", "Navigation");
			Navigation.OnInitialize(this);
		}


		public override void OnActived () {
			base.OnActived();
			Navigation.OnActived();
		}


		public override void PhysicsUpdate () {
			bool stateChanged = CharacterState != Yaya.CharacterState;
			if (stateChanged) SetCharacterState(Yaya.CharacterState);
			Health.SetHealth(Yaya.Health.HealthPoint);
			switch (CharacterState) {
				case CharacterState.GamePlay:
					if (Fed) {
						Update_FollowYaya();
					} else {
						Update_FreeMove();
					}
					break;
				case CharacterState.Sleep:
					if (stateChanged && Game.Current.TryGetEntityNearby<eBasket>(new(X, Y), out var basket)) {
						X = basket.X + basket.Width / 2;
						Y = basket.Y + basket.Height - OffsetY;
					}
					if (SleepAmount >= 1000) Fed = false;
					break;
			}
			base.PhysicsUpdate();
		}


		private void Update_FollowYaya () {
			// Yaya.Active




		}


		private void Update_FreeMove () {
			// Find Target



			//Navigation.SetTargetPosition(,);
			// Navigate
			Navigation.Navigate();
		}


		#endregion




		#region --- API ---


		public void Feed () {
			Fed = true;



		}


		#endregion




	}
}
