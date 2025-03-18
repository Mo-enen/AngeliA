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
	public int CurrentStepCombo { get; set; } = 0;

	public override void OnActivated () {
		base.OnActivated();
		MarioUtil.InitCharacterForMarioGame(this);
	}

	public override void Update () {
		base.Update();
		// Combo
		if (Game.GlobalFrame <= Movement.LastGroundingFrame + 2) {
			CurrentStepCombo = 0;
		}
		// Body Height
		var ren = Rendering as PoseCharacterRenderer;
		ren.CharacterHeight = Health.HP == 2 ? DefaultCharacterHeight : 95;
	}

	bool IActionTarget.Invoke () {
		if (PlayerSystem.Selecting == this) return false;
		PlayerSystem.SetCharacterAsPlayer(this);
		return true;
	}

	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != this;

}
