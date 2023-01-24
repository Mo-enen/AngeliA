using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eYayaMenuUI : MenuUI {


		protected override void DrawMenu () {
			// Ctrl Hint
			if (SelectionAdjustable) {
				eControlHintUI.DrawHint(GameKey.Left, GameKey.Right, WORD.HINT_ADJUST);
			} else {
				eControlHintUI.DrawHint(GameKey.Action, WORD.HINT_USE);
			}
			eControlHintUI.DrawHint(GameKey.Down, GameKey.Up, WORD.HINT_MOVE);
		}


	}
}