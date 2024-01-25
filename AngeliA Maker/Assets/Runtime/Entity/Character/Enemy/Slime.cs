using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;



namespace AngeliaMaker {
	public class SlimeRed : Slime {

	}

	public abstract class Slime : SheetCharacter, IDamageReceiver {

		int IDamageReceiver.Team => Const.TEAM_ENEMY;
		bool IDamageReceiver.TakeDamageFromLevel => false;


	}
}