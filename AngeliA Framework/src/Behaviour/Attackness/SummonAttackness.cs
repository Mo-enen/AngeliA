using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class SummonAttackness (Character character) : CharacterAttackness(character) {
	public override AttackStyleMode AttackStyle => AttackStyleMode.Manually;
}
