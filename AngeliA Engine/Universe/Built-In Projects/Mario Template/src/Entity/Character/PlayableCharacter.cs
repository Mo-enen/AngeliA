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

	public override void OnActivated () {
		base.OnActivated();
		MarioUtil.InitMovementForMarioGame(NativeMovement);
	}

	bool IActionTarget.Invoke () {
		if (PlayerSystem.Selecting == this) return false;
		PlayerSystem.SetCharacterAsPlayer(this);
		return true;
	}

	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != this;

}
