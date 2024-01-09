using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class SpinningBlade : EnvironmentEntity {


		public override void FillPhysics () => CellPhysics.FillBlock(PhysicsLayer.DAMAGE, TypeID, Rect.Expand(1), true, 1);


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(
				TypeID,
				X + Width / 2, Y + Height / 2,
				500, 500, (Game.SettleFrame * 12).UMod(360),
				Const.CEL, Const.CEL
			);
		}


	}
}