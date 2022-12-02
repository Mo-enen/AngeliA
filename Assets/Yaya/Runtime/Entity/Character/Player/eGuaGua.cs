using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.Capacity(1)]
	public class eGuaGua : eMascot {


		// Const
		private static readonly int HEART_CODE = "HpHeart".AngeHash();
		private static readonly int HEART_EMPTY_CODE = "HpHeart Empty".AngeHash();

		// Api
		public static eGuaGua Current { get; private set; } = null;
		public override int OwnerTypeID => typeof(eYaya).AngeHash();
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

			const int SIZE = Const.CEL / 3;
			const int GAP = 0;
			const int COLUMN = 3;
			const int MAX = 9;

			int hp = Owner.Health.HealthPoint;
			int maxHp = Mathf.Min(Owner.Health.MaxHP, MAX);
			int left = X - ((SIZE + GAP) * COLUMN - GAP) / 2;

			// Draw Hearts
			var rect = new RectInt(0, 0, SIZE, SIZE);
			for (int i = 0; i < maxHp; i++) {
				rect.x = left + (i % COLUMN) * (SIZE + GAP);
				rect.y = Y - (i / COLUMN + 1) * (SIZE + GAP);
				if (i < hp) {
					// Heart
					CellRenderer.Draw(HEART_CODE, rect).Z = 0;
				} else {
					// Empty Heart
					CellRenderer.Draw(HEART_EMPTY_CODE, rect).Z = 0;
				}
			}
			// Overflow
			if (Owner.Health.MaxHP > MAX) {

			}
		}


	}
}