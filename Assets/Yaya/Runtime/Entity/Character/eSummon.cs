using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.MapEditorGroup("Summon")]
	[EntityAttribute.DontDestroyOutOfRange]
	public abstract class eSummon : eCharacter {




		#region --- VAR ---


		// Data
		private eCharacter Owner = null;


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


		#endregion




		#region --- API ---


		public static void Summon<T> (eCharacter owner, int x, int y) where T : eSummon {
			var summon = Game.Current.SpawnEntity<T>(x, y);
			if (summon == null) return;
			summon.Owner = owner;
		}


		public static void Summon (eCharacter owner, int typeID, int x, int y) {
			if (Game.Current.SpawnEntity(typeID, x, y) is eSummon summon) {
				summon.Owner = owner;
			}
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}