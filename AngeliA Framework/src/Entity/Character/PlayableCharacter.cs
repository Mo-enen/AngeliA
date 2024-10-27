using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class PlayableCharacter : Character, IActionTarget {

	protected override CharacterRenderer CreateNativeRenderer () => new PoseCharacterRenderer(this);
	public override CharacterInventoryType InventoryType => CharacterInventoryType.Unique;

	public override void OnActivated () {
		base.OnActivated();
		if (Rendering is PoseCharacterRenderer rendering) {
			Rendering.SpinOnGroundPound = Wing.IsPropellerWing(rendering.WingID);
			Movement.FinalCharacterHeight = Movement.FinalCharacterHeight * rendering.CharacterHeight / 160;
		}
	}

	public virtual bool Invoke () {
		PlayerSystem.SetCharacterAsPlayer(this);
		return PlayerSystem.Selecting == this;
	}

	public virtual bool AllowInvoke () => PlayerSystem.Selecting != this && !Health.IsInvincible;

}
