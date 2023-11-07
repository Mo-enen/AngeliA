using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public abstract class Enemy : Character, IDamageReceiver {

		int IDamageReceiver.Team => Const.TEAM_ENEMY;
		public override int AttackTargetTeam => Const.TEAM_PLAYER | Const.TEAM_ENVIRONMENT;


	}
}