using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class PoseCharacter : Character {

	protected override CharacterRenderer CreateNativeRenderer () => new PoseCharacterRenderer(this);

	public override void OnActivated () {
		base.OnActivated();
		if (Rendering is PoseCharacterRenderer rendering) {
			Movement.FinalCharacterHeight = Movement.MovementHeight * rendering.CharacterHeight / 160;
		}
	}

}
