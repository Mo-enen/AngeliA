using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.StageOrder(4096)]
[EntityAttribute.SpawnWithCheatCode]
[EntityAttribute.UpdateOutOfRange]
public abstract class PlayableCharacter : PoseCharacter, IPlayable, IActionTarget {

	public override int Team => Const.TEAM_PLAYER;
	public override int AttackTargetTeam => Const.TEAM_ENEMY | Const.TEAM_NEUTRAL;
	protected virtual bool UseMarioStyleMovement => true;

	public override void OnActivated () {
		base.OnActivated();
		if (UseMarioStyleMovement) {
			MarioUtil.InitMovementForMarioGame(NativeMovement);
		}
	}

	bool IActionTarget.Invoke () {
		if (PlayerSystem.Selecting == this) return false;
		PlayerSystem.SetCharacterAsPlayer(this);
		return true;
	}

	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != this;

}
