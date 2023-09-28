using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	// Boob
	public class PetanBoob : AutoSpriteBoob {
		public override bool SuitAvailable => false;
		public override int Size => 1000;
		protected override void DrawBoob (Character character) {
			if (SpriteID == 0 || !character.Body.FrontSide) return;
			var boobPos = GetBoobPosition(character, false);
			if (CellRenderer.TryGetSprite(SpriteID, out var sprite)) {
				CellRenderer.Draw(
					SpriteID,
					boobPos.x, boobPos.y, 500, 1000, 0,
					sprite.GlobalWidth, sprite.GlobalHeight,
					character.SkinColor, character.Body.Z + 1
				);
			}
		}
	}

	public class NormalBoob : AutoSpriteBoob {
		public override int Size => 1200;
	}

	public class MoleBoob : AutoSpriteBoob {
		public override int Size => 1200;
		protected override void DrawBoob (Character character) {
			base.DrawBoob(character);
			// Mole
			var boobPos = GetBoobPosition(character);
			const int MOLE_SIZE = 16;
			CellRenderer.Draw(
				Const.PIXEL,
				boobPos.x + character.Body.Width.Abs() * 118 / 1000,
				boobPos.y - MOLE_SIZE,
				500, 500, 0,
				MOLE_SIZE, MOLE_SIZE,
				Const.BLACK, character.Body.Z + 2
			);
		}
	}


}
