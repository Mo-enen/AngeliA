using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class PoseCharacter : Character {

	protected override CharacterRenderer CreateNativeRenderer () => new PoseCharacterRenderer(this);
	public override int FinalCharacterHeight => Rendering is PoseCharacterRenderer rendering ?
		base.FinalCharacterHeight * rendering.CharacterHeight / 160 :
		base.FinalCharacterHeight;

}
