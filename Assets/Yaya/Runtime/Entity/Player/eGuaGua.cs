using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.Capacity(1)]
	public class eGuaGua : eMascot {


		// Const
		private static readonly int HEART_L_CODE = "Heart Left".AngeHash();
		private static readonly int HEART_R_CODE = "Heart Right".AngeHash();
		private static readonly int HEART_EMPTY_L_CODE = "Heart Empty Left".AngeHash();
		private static readonly int HEART_EMPTY_R_CODE = "Heart Empty Right".AngeHash();

		// Api
		public static eGuaGua Current { get; private set; } = null;
		public override ePlayer Owner => eYaya.CurrentYaya;

		// Data
		private int PrevHp = 0;


		// MSG
		public override void OnInitialize () {
			base.OnInitialize();
			Current = this;

			// Config
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
			FlySpeed.Value = 32;
			MaxHP.Value = 1;

		}


		protected override void Update_FreeMove () {
			base.Update_FreeMove();






		}


		protected override void DrawHpBar () {
			base.DrawHpBar();

			const int SIZE = Const.CEL / 2;
			const int COLUMN = 4;
			const int MAX = 9;

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
					CellRenderer.Draw(isLeft ? HEART_L_CODE : HEART_R_CODE, rect).Z = 0;
				} else {
					// Empty Heart
					CellRenderer.Draw(isLeft ? HEART_EMPTY_L_CODE : HEART_EMPTY_R_CODE, rect).Z = 0;
					// Spawn Drop Particle
					if (i < PrevHp) {
						eYayaDroppingHeart heart;
						if (isLeft) {
							heart = Game.Current.AddEntity<eYayaDroppingHeartLeft>(rect.x, rect.y);
						} else {
							heart = Game.Current.AddEntity<eYayaDroppingHeartRight>(rect.x, rect.y);
						}
						if (heart != null) {
							heart.Width = rect.width + 8;
							heart.Height = rect.height + 16;
						}
					}
				}
				isLeft = !isLeft;
			}
			// Overflow
			if (Owner.MaxHP > MAX) {

			}

			PrevHp = hp;
		}


	}
}