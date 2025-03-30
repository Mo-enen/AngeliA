using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


/// <summary>
/// Single checking rule for auto tiling map blocks
/// </summary>
public enum Rule : byte {
	
	/// <summary>
	/// Always true, do not check
	/// </summary>
	Whatever = 0,
	
	/// <summary>
	/// True if the target block is the same with source
	/// </summary>
	SameTile = 1,
	
	/// <summary>
	/// True if the target block is the different with source
	/// </summary>
	NotSameTile = 2,
	
	/// <summary>
	/// True if the target block is not 0
	/// </summary>
	AnyTile = 3,
	
	/// <summary>
	/// True if the target block is 0
	/// </summary>
	Empty = 4,
}


/// <summary>
/// Checking rules for auto tiling map blocks for a source block
/// </summary>
/// <param name="ruleTL">Rule apply to the top-left block</param>
/// <param name="ruleT">Rule apply to the top block</param>
/// <param name="ruleTR">Rule apply to the top-right block</param>
/// <param name="ruleL">Rule apply to the left block</param>
/// <param name="ruleR">Rule apply to the right block</param>
/// <param name="ruleBL">Rule apply to the bottom-left block</param>
/// <param name="ruleB">Rule apply to the bottom block</param>
/// <param name="ruleBR">Rule apply to the bottom-right block</param>
public struct BlockRule (Rule ruleTL, Rule ruleT, Rule ruleTR, Rule ruleL, Rule ruleR, Rule ruleBL, Rule ruleB, Rule ruleBR) {

	/// <summary>
	/// Empty rule what don't perform any rule check
	/// </summary>
	public static readonly BlockRule EMPTY = new();

	/// <summary>
	/// True if the rule is all set to whatever
	/// </summary>
	public readonly bool IsEmpty => RuleTL == Rule.Whatever && RuleT == Rule.Whatever && RuleTR == Rule.Whatever && RuleL == Rule.Whatever && RuleR == Rule.Whatever && RuleBL == Rule.Whatever && RuleB == Rule.Whatever && RuleBR == Rule.Whatever;

	/// <summary>
	/// Get rule at given index. (↖ ↑ ↗ ← → ↙ ↓ ↘)
	/// </summary>
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

	/// <summary>
	/// Rule apply to the top-left block 
	/// </summary>
	public Rule RuleTL = ruleTL;

	/// <summary>
	/// Rule apply to the top block 
	/// </summary>
	public Rule RuleT = ruleT;

	/// <summary>
	/// Rule apply to the top-right block 
	/// </summary>
	public Rule RuleTR = ruleTR;

	/// <summary>
	/// Rule apply to the left block 
	/// </summary>
	public Rule RuleL = ruleL;

	/// <summary>
	/// Rule apply to the right block 
	/// </summary>
	public Rule RuleR = ruleR;

	/// <summary>
	/// Rule apply to the bottom-left block 
	/// </summary>
	public Rule RuleBL = ruleBL;

	/// <summary>
	/// Rule apply to the bottom block 
	/// </summary>
	public Rule RuleB = ruleB;

	/// <summary>
	/// Rule apply to the bottom-right block 
	/// </summary>
	public Rule RuleBR = ruleBR;

	/// <summary>
	/// True if the rule is same with given rule
	/// </summary>
	public readonly bool IsSameWith (BlockRule other) => RuleTL == other.RuleTL && RuleT == other.RuleT && RuleTR == other.RuleTR && RuleL == other.RuleL && RuleR == other.RuleR && RuleBL == other.RuleBL && RuleB == other.RuleB && RuleBR == other.RuleBR;

}
