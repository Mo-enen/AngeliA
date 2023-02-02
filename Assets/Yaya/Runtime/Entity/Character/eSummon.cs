using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.MapEditorGroup("Summon")]
	[EntityAttribute.Capacity(1)]
	public abstract class eSummon : eCharacter {




		#region --- VAR ---


		// Api
		public eCharacter Owner { get; set; } = null;


		#endregion




		#region --- MSG ---


		public override void OnInactived () {
			base.OnInactived();
			Owner = null;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (Owner != null) {
				SetCharacterState(Owner.CharacterState);
			}
			if (CharacterState == CharacterState.GamePlay) {
				Update_GamePlay();
			}
		}


		private void Update_GamePlay () {





		}


		public void OnSummoned (bool create) {



			RenderBounce();
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}