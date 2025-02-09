using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[System.Serializable]
public class PlayableAttacknessConfig {

	public string Name = "";
	public int CombatLevel = 0;

	public void LoadConfigIntoCharacter (PlayableCharacter character) {
		if (character.Attackness is not PlayableCharacterAttackness att) return;
		att.CombatLevel = CombatLevel;
	}

	public void SaveConfigFromCharacter (PlayableCharacter character) {
		if (character.Attackness is not PlayableCharacterAttackness att) return;
		CombatLevel = att.CombatLevel;
	}

}
