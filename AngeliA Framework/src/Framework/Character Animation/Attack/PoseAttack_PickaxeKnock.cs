using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseAttack_PickaxeKnock : PoseAnimation {


	public static readonly int TYPE_ID = typeof(PoseAttack_PickaxeKnock).AngeHash();


	public override void Animate (PoseCharacter character) {
		base.Animate(character);
		KnockBlock();
	}


	public void KnockBlock () {






	}


}
