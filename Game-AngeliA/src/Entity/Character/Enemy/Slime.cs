using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;




namespace AngeliaGame; 
public class SlimeRed : Slime {

}

public abstract class Slime : SheetCharacter, IDamageReceiver {

	int IDamageReceiver.Team => Const.TEAM_ENEMY;
	bool IDamageReceiver.TakeDamageFromLevel => false;


}