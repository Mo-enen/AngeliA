using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.StageOrder(4096)]
[EntityAttribute.SpawnWithCheatCode]
[EntityAttribute.UpdateOutOfRange]
public abstract class PlayableCharacter : Character, IPlayable, IActionTarget {


	private static readonly AudioCode PASSOUT_AC = "PlayerPassout";
	private static readonly AudioCode HIT_AC = "PlayerGetHit";
	private static readonly AudioCode JUMP_AC = "JumpSmall";
	private static readonly AudioCode JUMP_BIG_AC = "JumpBig";

	public override int Team => Const.TEAM_PLAYER;
	public override int AttackTargetTeam => Const.TEAM_ENEMY | Const.TEAM_NEUTRAL;
	public int CurrentStepCombo { get; set; } = 0;
	public override int FinalCharacterHeight => Rendering is PoseCharacterRenderer rendering ?
		base.FinalCharacterHeight * rendering.CharacterHeight / 160 :
		base.FinalCharacterHeight;

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
		// SoundFX
		if (Game.GlobalFrame == Movement.LastJumpFrame) {
			FrameworkUtil.PlaySoundAtPosition(Health.HP >= 2 ? JUMP_BIG_AC : JUMP_AC, XY);
		}
	}

	bool IActionTarget.Invoke () {
		if (PlayerSystem.Selecting == this) return false;
		PlayerSystem.SetCharacterAsPlayer(this);
		return true;
	}

	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != this;

	public override void OnDamaged (Damage damage) {
		if (Health.HP == 0) return;
		base.OnDamaged(damage);
		if (damage.Amount > 0) {
			bool zeroHP = Health.HP == 0;
			FrameworkUtil.PlaySoundAtPosition(zeroHP ? PASSOUT_AC : HIT_AC, XY);
			if (zeroHP) {
				Game.PauseMusic();
			}
		}
	}

	protected override CharacterRenderer CreateNativeRenderer () => new PoseCharacterRenderer(this);

}
