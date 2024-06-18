using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public class SpinningBlade : EnvironmentEntity {


	public override void FirstUpdate () => Physics.FillBlock(PhysicsLayer.DAMAGE, TypeID, Rect.Expand(1), true, Tag.GeneralDamage);


	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(
			TypeID,
			X + Width / 2, Y + Height / 2,
			500, 500, (Game.SettleFrame * 12).UMod(360),
			Const.CEL, Const.CEL
		);
	}


}