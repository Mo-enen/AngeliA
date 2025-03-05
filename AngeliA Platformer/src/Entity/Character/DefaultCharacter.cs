using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Capacity(1, 0)]
[EntityAttribute.StageOrder(4096)]
public class DefaultCharacter : PoseCharacter, IPlayable {

	// VAR
	public override int Team => Const.TEAM_PLAYER;
	public override int AttackTargetTeam => Const.TEAM_ENEMY | Const.TEAM_ENVIRONMENT;
	public override CharacterInventoryType InventoryType => CharacterInventoryType.Unique;
	public override bool AllowBeingPush => true;

	// MSG
	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Adjust Fly Config
		if (Rendering is PoseCharacterRenderer pRender) {
			if (pRender.WingID != 0 && Wing.TryGetGadget(pRender.WingID, out var gadget) && gadget is Wing wing) {
				NativeMovement.FlyAvailable.True(1, 1);
				NativeMovement.GlideOnFlying.Override(!wing.IsPropeller, 1, 1);
			} else {
				NativeMovement.FlyAvailable.False(1, 1);
			}
		}
	}

}
