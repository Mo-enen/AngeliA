using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System;

namespace Yaya {
	[EntityAttribute.UpdateOutOfRange]
	public class eGuaGua : eSummon {


		// Const
		//private static readonly int HEART_L_CODE = "Heart Left".AngeHash();
		//private static readonly int HEART_R_CODE = "Heart Right".AngeHash();
		//private static readonly int HEART_EMPTY_L_CODE = "Heart Empty Left".AngeHash();
		//private static readonly int HEART_EMPTY_R_CODE = "Heart Empty Right".AngeHash();


		// MSG
		public eGuaGua () {
			MovementWidth.Value = 150;
			MovementHeight.Value = 150;
			SquatHeight.Value = 150;
			DashDuration.Value = 20;
			RunAccumulation.Value = 48;
			JumpSpeed.Value = 69;
			SwimInFreeStyle.Value = false;
			JumpWithRoll.Value = false;
			JumpCount.Value = 1;
			FlyAvailable.Value = false;
			FlyRiseSpeed.Value = 32;
			MaxHP.Value = 1;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			// Sleep in Basket
			if (CharacterState == CharacterState.Sleep) {
				if (Game.Current.TryGetEntityNearby<eBasket>(new(X, Y), out var basket)) {
					int offsetY = 0;
					if (CellRenderer.TryGetSprite(eBasket.TYPE_ID, out var basketSprite)) {
						offsetY = basketSprite.GlobalBorder.Down;
					}
					X = basket.X + basket.Width / 2;
					Y = basket.Y + basket.Height - offsetY;
				}
			}
		}


	}
}