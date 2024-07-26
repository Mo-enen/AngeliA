using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public enum Rule : byte {
	Whatever = 0,
	SameTile = 1,
	NotSameTile = 2,
	AnyTile = 3,
	Empty = 4,
}

public struct BlockRule {
	public static readonly BlockRule EMPTY = new();
	public Rule this[int i] {
		readonly get => i switch {
			0 => RuleTL,
			1 => RuleT,
			2 => RuleTR,
			3 => RuleL,
			4 => RuleR,
			5 => RuleBL,
			6 => RuleB,
			7 => RuleBR,
			_ => Rule.Whatever,
		};
		set {
			switch (i) {
				case 0: RuleTL = value; break;
				case 1: RuleT = value; break;
				case 2: RuleTR = value; break;
				case 3: RuleL = value; break;
				case 4: RuleR = value; break;
				case 5: RuleBL = value; break;
				case 6: RuleB = value; break;
				case 7: RuleBR = value; break;
			}
		}
	}
	public Rule RuleTL;
	public Rule RuleT;
	public Rule RuleTR;
	public Rule RuleL;
	public Rule RuleR;
	public Rule RuleBL;
	public Rule RuleB;
	public Rule RuleBR;
	public BlockRule (Rule ruleTL, Rule ruleT, Rule ruleTR, Rule ruleL, Rule ruleR, Rule ruleBL, Rule ruleB, Rule ruleBR) {
		RuleTL = ruleTL;
		RuleT = ruleT;
		RuleTR = ruleTR;
		RuleL = ruleL;
		RuleR = ruleR;
		RuleBL = ruleBL;
		RuleB = ruleB;
		RuleBR = ruleBR;
	}
}
