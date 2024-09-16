using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class PoseAnimation_Ride : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAnimation_Ride).AngeHash();

	public override void Animate (PoseCharacter character) {
		base.Animate(character);




		character.PoseRootY = 0;



	}

}
