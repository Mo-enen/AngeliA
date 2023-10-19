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
		int IConfigurableCharacter.LoadedSlot { get; set; } = 0;


		#endregion




		#region --- MSG ---


		public MainPlayer () => (this as IConfigurableCharacter).LoadConfigFromFile();


		public override void OnActivated () {

			base.OnActivated();

			(this as IConfigurableCharacter).ReloadConfig();

			/////////////////// TEMP //////////////////

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


		#endregion




	}
}