using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Capacity(1, 0)]
public abstract class PlayableCharacter : Character, IActionTarget {

	// VAR
	public override int Team => Const.TEAM_PLAYER;
	public override int AttackTargetTeam => Const.TEAM_ENEMY | Const.TEAM_ENVIRONMENT;
	public override CharacterInventoryType InventoryType => CharacterInventoryType.Unique;
	public override int FinalCharacterHeight => Rendering is PoseCharacterRenderer rendering ?
		base.FinalCharacterHeight * rendering.CharacterHeight / 160 :
		base.FinalCharacterHeight;

	// MSG
	public virtual bool Invoke () {
		PlayerSystem.SetCharacterAsPlayer(this);
		return PlayerSystem.Selecting == this;
	}

	public virtual bool AllowInvoke () => PlayerSystem.Selecting != this && !Health.IsInvincible;

	// API
	protected override CharacterRenderer CreateNativeRenderer () => new PoseCharacterRenderer(this);

}
