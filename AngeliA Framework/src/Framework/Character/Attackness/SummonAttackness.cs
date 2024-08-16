using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class SummonAttackness : CharacterAttackness {
	public override int AttackTargetTeam => Owner != null ? Owner.Attackness.AttackTargetTeam : 0;
	public Character Owner { get; set; }
	public SummonAttackness (Character character) : base(character) { }
}
