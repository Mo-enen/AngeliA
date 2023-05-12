using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	[RenderWithSheet]
	public class eGuaGua : Summon {


		// MSG
		public eGuaGua () {
			MovementWidth.Value = 150;
			MovementHeight.Value = 150;
			SquatHeight.Value = 150;
			DashDuration.Value = 20;
			WalkToRunAccumulation.Value = 48;
			JumpSpeed.Value = 69;
			FirstJumpWithRoll.Value = false;
			JumpCount.Value = 1;
			FlyAvailable.Value = false;
			FlyRiseSpeed.Value = 32;
			MaxHP.Value = 1;
			FlyAcceleration.Value = 1;
			FlyDeceleration.Value = 1;
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

			// Sleep in Basket
			if (CharacterState == CharacterState.Sleep) {
				if (Game.Current.TryGetEntityNearby<eBasket>(new(X, Y), out var basket)) {
					int offsetY = 0;
					if (CellRenderer.TryGetSprite(basket.TypeID, out var basketSprite)) {
						offsetY = basketSprite.GlobalHeight - basketSprite.GlobalBorder.Up;
					}
					X = basket.X + basket.Width / 2;
					Y = basket.Y + offsetY;
				}
			}
		}


	}
}