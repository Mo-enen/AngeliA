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
		Vector3Int? IConfigurableCharacter.HomeUnitPosition { get; set; } = null;


		#endregion




		#region --- MSG ---


		public MainPlayer () => (this as IConfigurableCharacter).LoadConfigFromFile();


		public override void OnActivated () {
			base.OnActivated();
			(this as IConfigurableCharacter).LoadCharacterFromConfig();

			/////////////////// TEMP //////////////////

			WalkToRunAccumulation.Value = 40;

			StopMoveOnAttack.Value = false;
			AttackInAir.Value = true;
			AttackInWater.Value = true;
			AttackWhenMoving.Value = true;
			AttackWhenClimbing.Value = true;
			AttackWhenFlying.Value = true;
			AttackWhenRolling.Value = true;
			AttackWhenSquatting.Value = true;
			AttackWhenDashing.Value = true;
			AttackWhenSliding.Value = true;
			AttackWhenGrabbing.Value = true;
			AttackWhenRush.Value = true;

			/////////////////// TEMP //////////////////

		}


		public override void FrameUpdate () {
			base.FrameUpdate();

			// Sleep
			if (SleepFrame == FULL_SLEEP_DURATION) {
				var configCharacter = this as IConfigurableCharacter;
				configCharacter.HomeUnitPosition = new Vector3Int(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
				configCharacter.SaveConfigToFile();
			}

		}


		#endregion




	}
}