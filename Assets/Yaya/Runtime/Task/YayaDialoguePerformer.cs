using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class YayaDialoguePerformer : ScreenDialoguePerformer {

		private static readonly int CHOICE_HINT_ID = "UI.ChoiceHint".AngeHash();
		public override string ChoiceHintLabel => Language.Get(CHOICE_HINT_ID);

	}
}
