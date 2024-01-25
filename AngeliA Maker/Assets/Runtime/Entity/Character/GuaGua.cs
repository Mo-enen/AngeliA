using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaMaker {
	[EntityAttribute.Capacity(1, 0)]
	public class GuaGua : Summon {


		public override bool FlyAvailable => true;


		public GuaGua () {
			MaxHP.BaseValue = 1;
			MovementWidth.BaseValue = 150;
			MovementHeight.BaseValue = 150;
			SquatHeightAmount.BaseValue = 1000;
			DashDuration.BaseValue = 20;
			WalkToRunAccumulation.BaseValue = 48;
			JumpSpeed.BaseValue = 69;
			FirstJumpWithRoll.BaseValue = false;
			JumpCount.BaseValue = 1;
			FlyRiseSpeed.BaseValue = 32;
			FlyAcceleration.BaseValue = 1;
			FlyDeceleration.BaseValue = 1;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			FrameUpdate_Sleep();
		}


		private void FrameUpdate_Sleep () {
			if (Owner == null || !Owner.Active) return;

			// Sleep when Owner Sleep
			if (Owner.CharacterState == CharacterState.Sleep && CharacterState != CharacterState.Sleep) {
				SetCharacterState(CharacterState.Sleep);
			}

			// Wake when Owner Awake
			if (CharacterState == CharacterState.Sleep && Owner.CharacterState != CharacterState.Sleep) {
				SetCharacterState(CharacterState.GamePlay);
			}

		}


		public override void SetCharacterState (CharacterState state) {
			base.SetCharacterState(state);
			// Sleep in Basket
			if (state == CharacterState.Sleep) {
				if (Stage.TryGetEntityNearby<Basket>(new(X, Y), out var basket)) {
					int offsetY = 0;
					if (CellRenderer.TryGetSprite(basket.TypeID, out var basketSprite)) {
						offsetY = basketSprite.GlobalHeight - basketSprite.GlobalBorder.up;
					}
					X = basket.X + basket.Width / 2;
					Y = basket.Y + offsetY;
				}
			}
		}


	}
}