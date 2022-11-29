using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eMascot : eCharacter {




		#region --- VAR ---


		// Api
		public bool FollowOwner { get; set; } = false;
		public abstract int OwnerTypeID { get; }

		// Data
		private ePlayer Owner = null;
		private Vector2Int PrevPosition = default;
		private readonly Vector2Int[] PosChain = new Vector2Int[6];
		private int PosChainStartIndex = -1;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {
			base.OnInitialize();
			Owner = Game.Current.PeekOrGetEntity(OwnerTypeID) as ePlayer;
		}


		public override void OnActived () {
			base.OnActived();
			PosChainStartIndex = -1;
		}


		public override void FillPhysics () {
			if (CharacterState == CharacterState.GamePlay && !FollowOwner) {
				base.FillPhysics();
			}
		}


		public override void PhysicsUpdate () {
			bool stateChanged = CharacterState != Owner.CharacterState;
			if (stateChanged) SetCharacterState(Owner.CharacterState);
			Health.SetHealth(Owner.Health.EmptyHealth ? 0 : Health.MaxHP);
			switch (CharacterState) {
				case CharacterState.GamePlay:
					if (FollowOwner) {
						Update_FollowOwner();
					} else {
						Update_FreeMove();
						base.PhysicsUpdate();
					}
					PrevPosition.x = X;
					PrevPosition.y = Y;
					break;
				case CharacterState.Sleep:
					if (stateChanged && Game.Current.TryGetEntityNearby<eBasket>(new(X, Y), out var basket)) {
						X = basket.X + basket.Width / 2;
						Y = basket.Y + basket.Height - OffsetY;
					}
					if (SleepAmount >= 1000) FollowOwner = false;
					base.PhysicsUpdate();
					break;
				case CharacterState.Passout:
					VelocityX = 0;
					break;
			}
			if (!FollowOwner || CharacterState != CharacterState.GamePlay) {
				PosChainStartIndex = -1;
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (
				Owner != null && Owner.Active && FollowOwner &&
				Owner.CharacterState == CharacterState.GamePlay
			) {
				DrawHpBar();
			}
		}


		private void Update_FollowOwner () {

			if (!Owner.Active) return;

			Movement.FacingRight = Owner.X >= X;
			MovementState = MovementState.Fly;

			int targetX = Owner.X;
			int targetY = Owner.Y + Const.CEL * 3 / 2;

			// Chain
			const int SEG_DIS = Const.CEL / 8;
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


		// Override
		protected virtual void Update_FreeMove () { }
		protected virtual void DrawHpBar () { }


		#endregion




		#region --- API ---


		public void Summon () {
			if (!Active) Game.Current.TryAddEntity(TypeID, X, Y, out _);
			X = PrevPosition.x;
			Y = PrevPosition.y;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}