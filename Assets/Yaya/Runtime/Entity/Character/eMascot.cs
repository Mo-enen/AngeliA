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
		private readonly Vector2Int[] PosChain = new Vector2Int[6];
		private int PosChainStartIndex = -1;
		private int PrevFollowSwapeFrame = 0;
		private int PrevHp = 0;


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
				DrawHealthBar();
			}
			PrevHp = Owner.HealthPoint;
		}


		private void Update_FollowOwner () {

			if (!Owner.Active) return;

			LockFacingRight(Owner.X >= X);
			MoveState = MovementState.Fly;

			int targetX = Owner.X + (Owner.FacingRight ? -Const.CEL * 3 / 2 : Const.CEL * 3 / 2);
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
			X = X.LerpTo(pos.x, 50);
			Y = Y.LerpTo(pos.y, 50);

		}


		// Override
		protected virtual void Update_FreeMove () { }
		protected virtual void DrawHealthBar () { }


		#endregion




		#region --- API ---


		public void Summon () {
			if (!Active) Game.Current.TrySpawnEntity(TypeID, X, Y, out _);
			var spawnRect = Game.Current.SpawnRect;
			X = X.Clamp(spawnRect.xMin + Const.CEL, spawnRect.xMax - Const.CEL);
			Y = Y.Clamp(spawnRect.yMin + Const.CEL, spawnRect.yMax - Const.CEL);
		}


		protected void DrawHealthBar_Segment (
			int heartLeftCode, int heartRightCode, int emptyHeartLeftCode, int emptyHeartRightCode
		) {

			const int SIZE = Const.CEL / 2;
			const int COLUMN = 4;
			const int MAX = 8;

			int hp = Owner.HealthPoint;
			int maxHp = Mathf.Min(Owner.MaxHP, MAX);
			int left = X - SIZE * COLUMN / 4;

			// Draw Hearts
			var rect = new RectInt(0, 0, SIZE / 2, SIZE);
			bool isLeft = true;
			for (int i = 0; i < maxHp; i++) {
				rect.x = left + (i % COLUMN) * SIZE / 2;
				rect.y = Y - (i / COLUMN + 1) * SIZE;
				if (i < hp) {
					// Heart
					CellRenderer.Draw(isLeft ? heartLeftCode : heartRightCode, rect).Z = 0;
				} else {
					// Empty Heart
					CellRenderer.Draw(isLeft ? emptyHeartLeftCode : emptyHeartRightCode, rect).Z = 0;
					// Spawn Drop Particle
					if (i < PrevHp) {
						eYayaDroppingHeart heart;
						if (isLeft) {
							heart = Game.Current.SpawnEntity<eYayaDroppingHeartLeft>(rect.x, rect.y);
						} else {
							heart = Game.Current.SpawnEntity<eYayaDroppingHeartRight>(rect.x, rect.y);
						}
						if (heart != null) {
							heart.Width = rect.width + 8;
							heart.Height = rect.height + 16;
						}
					}
				}
				isLeft = !isLeft;
			}
			PrevHp = hp;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}