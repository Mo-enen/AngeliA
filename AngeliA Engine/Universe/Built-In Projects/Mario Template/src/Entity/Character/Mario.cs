using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[CharacterAttribute.DefaultSelectedPlayer]
public class Mario : PlayableCharacter {
	public override int DefaultCharacterHeight => 155;
}
