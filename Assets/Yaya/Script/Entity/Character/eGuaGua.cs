using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.EntityCapacity(1)]
	public class eGuaGua : eCharacter {




		#region --- VAR ---


		// Const
		private static readonly int IDLE_CODE = "_aGuaGua.Idle".AngeHash();
		private static readonly int FLY_CODE = "_aGuaGua.Fly".AngeHash();
		private static readonly int SLEEP_CODE = "_aGuaGua.Sleep".AngeHash();

		// Api
		public override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override int CollisionMask => YayaConst.MASK_MAP;
		public bool Fed { get; private set; } = false;
		public Navigation Navigation { get; private set; } = null;

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
		private eYaya _Yaya = null;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {
			base.OnInitialize();
			var game = Game.Current;
			Navigation = game.LoadMeta<Navigation>("GuaGua", "Movement") ?? new();
		}


		public override void OnActived () {
			base.OnActived();
			Navigation.OnActived(this);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			switch (Yaya.CharacterState) {
				case CharacterState.GamePlay:
				case CharacterState.Passout:
					SetCharacterState(CharacterState.GamePlay);
					if (Fed) {
						Update_FollowYaya();
					} else {
						Update_FreeMove();
					}
					break;
				case CharacterState.Sleep:
					SetCharacterState(CharacterState.Sleep);
					if (Game.Current.TryGetEntityNearby<eBasket>(new(X, Y), out var basket)) {
						X = basket.X;
						Y = basket.Y + basket.Height;
					}
					Fed = false;
					break;
			}
		}


		private void Update_FollowYaya () {
			// Yaya.Active




		}


		private void Update_FreeMove () {



		}


		#endregion




		#region --- API ---


		public void Feed () {
			Fed = true;



		}


		#endregion




	}
}
