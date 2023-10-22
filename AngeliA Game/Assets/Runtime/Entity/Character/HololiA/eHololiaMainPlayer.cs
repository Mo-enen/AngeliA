using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	[EntityAttribute.DefaultSelectPlayer(1)]
	public class eHololiaMainPlayer : Player, IConfigurableCharacter {


		// Api
		IConfigurableCharacter.CharacterConfig IConfigurableCharacter.Config { get; set; } = new();
		int IConfigurableCharacter.LoadedSlot { get; set; } = -1;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			(this as IConfigurableCharacter).ReloadConfig();
		}



	}
}