using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {
	[EntityAttribute.Capacity(8)]
	public abstract class NPC : PoseCharacter, IDamageReceiver {




		#region --- VAR ---


		// Api
		int IDamageReceiver.Team => Const.TEAM_NEUTRAL;
		public override int AttackTargetTeam => Const.TEAM_ENEMY | Const.TEAM_ENVIRONMENT;


		#endregion




		#region --- MSG ---



		#endregion




		#region --- API ---



		#endregion




		#region --- LGC ---



		#endregion




	}
}