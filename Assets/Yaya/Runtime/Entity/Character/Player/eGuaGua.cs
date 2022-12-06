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
		public override CharacterIdentity Identity => CharacterIdentity.Player;


		// MSG
		public override void OnInitialize () {
			base.OnInitialize();
			Current = this;
		}


		protected override void Update_FreeMove () {
			base.Update_FreeMove();


		}


		protected override void DrawHpBar () {
			base.DrawHpBar();

			const int SIZE = Const.CEL / 2;
			const int COLUMN = 4;
			const int MAX = 9;

			int hp = Owner.Health.HealthPoint;
			int maxHp = Mathf.Min(Owner.Health.MaxHP, MAX);
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
				}
				isLeft = !isLeft;
			}
			// Overflow
			if (Owner.Health.MaxHP > MAX) {

			}
		}


	}
}