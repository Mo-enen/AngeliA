using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	// Boob
	public class PetanBoob : AutoSpriteBoob {
		protected override void DrawBoob (Character character) {
			if (SpriteID == 0 || !character.Body.FrontSide) return;
			CellRenderer.Draw(
				SpriteID, GetBoobRect(character, 300, false), character.SkinColor, character.Body.Z + 1
			);
		}
	}

	public class NormalBoob : AutoSpriteBoob { }

	public class MoleBoob : AutoSpriteBoob {
		protected override void DrawBoob (Character character) {
			base.DrawBoob(character);
			// Mole
			var boobRect = GetBoobRect(character);
			const int MOLE_SIZE = 16;
			CellRenderer.Draw(
				Const.PIXEL,
				boobRect.x + boobRect.width * 618 / 1000,
				boobRect.yMax - MOLE_SIZE,
				500, 500, 0,
				MOLE_SIZE, MOLE_SIZE,
				Const.BLACK, character.Body.Z + 2
			);
		}
	}


}
