using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


/// <summary>
/// A data that can be override for specified frames
/// </summary>
public abstract class FrameBasedValue {
	public bool Overrided => Game.GlobalFrame <= OverrideFrame;
	public int OverrideFrame = -1;
	public int OverridePriority = int.MinValue;
	public void ClearOverride () => OverrideFrame = -1;
}


/// <inheritdoc cref="FrameBasedValue"/>
public abstract class FrameBasedValue<T> : FrameBasedValue {
	public T FinalValue => Game.GlobalFrame <= OverrideFrame ? OverrideValue : BaseValue;
	public T BaseValue;
	public T OverrideValue;
	public FrameBasedValue () {
		BaseValue = default;
		OverrideFrame = -1;
	}
	public FrameBasedValue (T value) {
		BaseValue = value;
		OverrideFrame = -1;
	}
	public void Override (T value, int duration = 0, int priority = 0) {
		if (Game.GlobalFrame <= OverrideFrame && priority < OverridePriority) return;
		OverrideFrame = Game.GlobalFrame + duration;
		OverrideValue = value;
		OverridePriority = priority;
	}
	public override string ToString () => FinalValue.ToString();
}


/// <inheritdoc cref="FrameBasedValue"/>
public class FrameBasedInt (int value) : FrameBasedValue<int>(value) {
	public void Add (int delta, int duration = 0, int priority = 0) {
		if (Overrided) {
			if (priority < OverridePriority) return;
			OverrideFrame = Util.Max(Game.GlobalFrame + duration, OverrideFrame);
			OverrideValue += delta;
		} else {
			OverrideFrame = Game.GlobalFrame + duration;
			OverrideValue = BaseValue + delta;
		}
		OverridePriority = priority;
	}
	public void Multiply (int scale, int duration = 0, int priority = 0) {
		if (Overrided) {
			if (priority < OverridePriority) return;
			OverrideFrame = Util.Max(Game.GlobalFrame + duration, OverrideFrame);
			OverrideValue = OverrideValue * scale / 1000;
		} else {
			OverrideFrame = Game.GlobalFrame + duration;
			OverrideValue = BaseValue * scale / 1000;
		}
		OverridePriority = priority;
	}
	public void Min (int value, int duration = 0, int priority = 0) {
		if (Overrided) {
			if (priority < OverridePriority) return;
			if (value > OverrideValue) return;
			OverrideFrame = Util.Max(Game.GlobalFrame + duration, OverrideFrame);
			OverrideValue = Util.Min(OverrideValue, value);
		} else if (value <= BaseValue) {
			OverrideFrame = Game.GlobalFrame + duration;
			OverrideValue = Util.Min(BaseValue, value);
		}
		OverridePriority = priority;
	}
	public void Max (int value, int duration = 0, int priority = 0) {
		if (Overrided) {
			if (priority < OverridePriority) return;
			if (value < OverrideValue) return;
			OverrideFrame = Util.Max(Game.GlobalFrame + duration, OverrideFrame);
			OverrideValue = Util.Max(OverrideValue, value);
		} else if (value >= BaseValue) {
			OverrideFrame = Game.GlobalFrame + duration;
			OverrideValue = Util.Max(BaseValue, value);
		}
		OverridePriority = priority;
	}
	public FrameBasedInt () : this(0) { }
	public static implicit operator int (FrameBasedInt bInt) => bInt.FinalValue;
}


/// <inheritdoc cref="FrameBasedValue"/>
public class FrameBasedBool (bool value) : FrameBasedValue<bool>(value) {
	public void Or (bool value, int duration = 0, int priority = 0) {
		if (Overrided) {
			if (priority < OverridePriority) return;
			OverrideFrame = Util.Max(Game.GlobalFrame + duration, OverrideFrame);
			OverrideValue = value || OverrideValue;
		} else {
			OverrideFrame = Game.GlobalFrame + duration;
			OverrideValue = value || BaseValue;
		}
		OverridePriority = priority;
	}
	public void And (bool value, int duration = 0, int priority = 0) {
		if (Overrided) {
			if (priority < OverridePriority) return;
			OverrideFrame = Util.Max(Game.GlobalFrame + duration, OverrideFrame);
			OverrideValue = value && OverrideValue;
		} else {
			OverrideFrame = Game.GlobalFrame + duration;
			OverrideValue = value && BaseValue;
		}
		OverridePriority = priority;
	}
	public void True (int duration = 0, int priority = 0) => Override(true, duration, priority);
	public void False (int duration = 0, int priority = 0) => Override(false, duration, priority);
	public FrameBasedBool () : this(true) { }
	public static implicit operator bool (FrameBasedBool bBool) => bBool.FinalValue;
}


/// <inheritdoc cref="FrameBasedValue"/>
public class FrameBasedColor (Color32 value) : FrameBasedValue<Color32>(value) {
	public void Tint (Color32 tint, int duration = 0, int priority = 0) {
		if (Overrided) {
			if (priority < OverridePriority) return;
			OverrideFrame = Util.Max(Game.GlobalFrame + duration, OverrideFrame);
			OverrideValue = tint * BaseValue;
		} else {
			OverrideFrame = Game.GlobalFrame + duration;
			OverrideValue = tint * BaseValue;
		}
		OverridePriority = priority;
	}
	public FrameBasedColor () : this(Color32.WHITE) { }
	public static implicit operator Color32 (FrameBasedColor bColor) => bColor.FinalValue;
}

