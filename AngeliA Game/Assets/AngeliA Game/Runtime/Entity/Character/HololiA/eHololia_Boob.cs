using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	// Boob
	public class PetanBoob : AutoSpriteBoob {
		public override bool SuitAvailable => false;
		protected override void DrawBoob (Character character) {
			if (SpriteID == 0 || !character.Body.FrontSide) return;
			var boobPos = GetBoobPosition(character, false);
			if (CellRenderer.TryGetSprite(SpriteID, out var sprite)) {
				CellRenderer.Draw(
					SpriteID, new RectInt(boobPos.x, boobPos.y - sprite.GlobalHeight, boobPos.z, sprite.GlobalHeight), character.SkinColor, character.Body.Z + 1
				);
			}
		}
	}

	public class NormalBoob : AutoSpriteBoob { }

	public class MoleBoob : AutoSpriteBoob {
		protected override void DrawBoob (Character character) {
			base.DrawBoob(character);
			// Mole
			var boobPos = GetBoobPosition(character);
			const int MOLE_SIZE = 16;
			CellRenderer.Draw(
				Const.PIXEL,
				boobPos.x + boobPos.z * 618 / 1000,
				boobPos.y - MOLE_SIZE,
				500, 500, 0,
				MOLE_SIZE, MOLE_SIZE,
				Const.BLACK, character.Body.Z + 2
			);
		}
	}


}
