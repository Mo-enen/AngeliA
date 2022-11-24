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
		private readonly Vector2Int[] PosChain = new Vector2Int[6];
		private int PosChainStartIndex = -1;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {
			base.OnInitialize();
			Yaya = Game.Current.PeekOrGetEntity<eYaya>();
		}


		public override void OnActived () {
			base.OnActived();
			PosChainStartIndex = -1;
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
					base.PhysicsUpdate();
					break;
				case CharacterState.Passout:
					VelocityX = 0;
					break;
			}
			if (!Fed || CharacterState != CharacterState.GamePlay) {
				PosChainStartIndex = -1;
			}
			PrevPosition.x = X;
			PrevPosition.y = Y;
		}


		private void Update_FollowYaya () {

			if (!Yaya.Active) return;

			Movement.FacingRight = Yaya.X >= X;
			MovementState = MovementState.Fly;

			int targetX = Yaya.X;
			int targetY = Yaya.Y + Yaya.Height + Const.CEL / 3;

			// Chain
			const int SEG_DIS = Const.CEL / 4;
			if (PosChainStartIndex < 0) {
				for (int i = 0; i < PosChain.Length; i++) {
					PosChain[i] = new(targetX, targetY);
				}
				PosChainStartIndex = 0;
			}
			var currentPos = PosChain[PosChainStartIndex];
			if (Util.SqrtDistance(targetX, targetY, currentPos.x, currentPos.y) > SEG_DIS * SEG_DIS) {
				PosChainStartIndex = (PosChainStartIndex + 1) % PosChain.Length;
				PosChain[PosChainStartIndex] = new(targetX, targetY);
			}

			// Move
			var pos = PosChain[(PosChainStartIndex + 1).UMod(PosChain.Length)];
			X = X.LerpTo(pos.x, 200);
			Y = Y.LerpTo(pos.y, 200);

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
