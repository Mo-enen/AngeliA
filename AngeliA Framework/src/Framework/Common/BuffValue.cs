using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class BuffInt {

	public int FinalValue => Game.PauselessFrame <= OverrideFrame ? OverrideValue : BaseValue;
	public int BaseValue = 0;
	public int OverrideFrame = -1;
	public int OverrideValue;

	public BuffInt (int value = 0) {
		BaseValue = value;
		OverrideFrame = -1;
	}

	public void Override (int value, int duration = 0) {
		OverrideFrame = Game.PauselessFrame + duration;
		OverrideValue = value;
	}
	public void Override (int? value, int duration = 0) {
		if (!value.HasValue) {
			OverrideFrame = -1;
			return;
		}
		OverrideFrame = Game.PauselessFrame + duration;
		OverrideValue = value.Value;
	}
	public void ClearOverride () => OverrideFrame = -1;

	public static implicit operator int (BuffInt bInt) => bInt.FinalValue;

}

public class BuffBool {

	public bool FinalValue => Game.PauselessFrame <= OverrideFrame ? OverrideValue : BaseValue;
	public bool BaseValue = true;
	public int OverrideFrame = -1;
	public bool OverrideValue;

	public BuffBool (bool value = true) {
		BaseValue = value;
		OverrideFrame = -1;
	}

	public void Override (bool value, int duration = 0) {
		OverrideFrame = Game.PauselessFrame + duration;
		OverrideValue = value;
	}
	public void Override (bool? value, int duration = 0) {
		if (!value.HasValue) {
			OverrideFrame = -1;
			return;
		}
		OverrideFrame = Game.PauselessFrame + duration;
		OverrideValue = value.Value;
	}
	public void ClearOverride () => OverrideFrame = -1;

	public static implicit operator bool (BuffBool bBool) => bBool.FinalValue;

}