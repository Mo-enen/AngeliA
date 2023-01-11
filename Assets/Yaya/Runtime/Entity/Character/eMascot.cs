using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.Capacity(1, 1)]
	public abstract class eMascot : eCharacter {




		#region --- VAR ---


		// Api
		public bool FollowOwner { get; set; } = false;
		public abstract ePlayer Owner { get; }

		// Data
		private Vector2Int PrevPosition = default;
		private readonly Vector2Int[] PosChain = new Vector2Int[6];
		private int PosChainStartIndex = -1;
		private int PrevFollowSwapeFrame = 0;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			PosChainStartIndex = -1;
			PrevFollowSwapeFrame = Game.GlobalFrame;
		}


		public override void FillPhysics () {
			if (CharacterState == CharacterState.GamePlay && !FollowOwner) {
				base.FillPhysics();
			}
		}


		public override void PhysicsUpdate () {

			SetCharacterState(Owner.CharacterState);
			SetHealth(Owner.IsEmptyHealth ? 0 : MaxHP);

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
					if (Game.Current.TryGetEntityNearby<eBasket>(new(X, Y), out var basket)) {
						int offsetY = 0;
						if (CellRenderer.TryGetSprite(eBasket.TYPE_ID, out var basketSprite)) {
							offsetY = basketSprite.GlobalBorder.Down;
						}
						X = basket.X + basket.Width / 2;
						Y = basket.Y + basket.Height - offsetY;
					}
					SleepFrame = Owner.SleepFrame;
					if (SleepAmount >= 1000) FollowOwner = false;
					break;


				case CharacterState.Passout:
					VelocityX = 0;
					base.PhysicsUpdate();
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

			FacingRight = Owner.X >= X;
			MoveState = MovementState.Fly;

			int targetX = Owner.X + (Owner.FacingRight ? -Const.CEL : Const.CEL);
			int targetY = Owner.Y + Const.CEL * 3 / 2;

			// Chain
			if (PosChainStartIndex < 0) {
				for (int i = 0; i < PosChain.Length; i++) {
					PosChain[i] = new(targetX, targetY);
				}
				PosChainStartIndex = 0;
			}
			if (Game.GlobalFrame > PrevFollowSwapeFrame + 1) {
				PrevFollowSwapeFrame = Game.GlobalFrame;
				PosChainStartIndex = (PosChainStartIndex + 1) % PosChain.Length;
				PosChain[PosChainStartIndex] = new(targetX, targetY);
			}

			// Move
			var pos = PosChain[(PosChainStartIndex + 1).UMod(PosChain.Length)];
			X = X.LerpTo(pos.x, 100);
			Y = Y.LerpTo(pos.y, 100);

		}


		// Override
		protected virtual void Update_FreeMove () { }
		protected virtual void DrawHpBar () { }


		#endregion




		#region --- API ---


		public void Summon () {
			if (!Active) Game.Current.TrySpawnEntity(TypeID, X, Y, out _);
			X = PrevPosition.x;
			Y = PrevPosition.y;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}