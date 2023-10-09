using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.Capacity(1, 1)]
	[EntityAttribute.MapEditorGroup("System")]
	public sealed class MainPlayer : Player, IConfigurableCharacter {




		#region --- VAR ---


		// Api
		IConfigurableCharacter.CharacterConfig IConfigurableCharacter.Config { get; set; } = new();
		Int3? IConfigurableCharacter.HomeUnitPosition { get; set; } = null;


		#endregion




		#region --- MSG ---


		public MainPlayer () => (this as IConfigurableCharacter).LoadConfigFromFile();


		public override void OnActivated () {
			base.OnActivated();
			(this as IConfigurableCharacter).LoadCharacterFromConfig();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();

			// Sleep
			if (SleepFrame == FULL_SLEEP_DURATION) {
				var configCharacter = this as IConfigurableCharacter;
				configCharacter.HomeUnitPosition = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
				configCharacter.SaveConfigToFile();
			}

		}


		#endregion




	}
}