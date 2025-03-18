using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class StartArrow : Entity {

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, CenterX, Y, 500, 0, 0, Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);
	}

}
