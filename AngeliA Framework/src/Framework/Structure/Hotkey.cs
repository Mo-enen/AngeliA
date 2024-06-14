namespace AngeliA;

public readonly struct Hotkey {
	private const uint CTRL_MARK = 0b10000000000000000000000000000000;
	private const uint ALT_MARK = 0b01000000000000000000000000000000;
	private const uint SHIFT_MARK = 0b00100000000000000000000000000000;
	public readonly KeyboardKey Key;
	public readonly bool Ctrl;
	public readonly bool Shift;
	public readonly bool Alt;
	public Hotkey (KeyboardKey key, bool ctrl = false, bool shift = false, bool alt = false) {
		Key = key;
		Ctrl = ctrl;
		Shift = shift;
		Alt = alt;
	}
	public Hotkey (string data) {
		if (uint.TryParse(data, out uint result)) {
			Ctrl = result.GetBit(31);
			Alt = result.GetBit(30);
			Shift = result.GetBit(29);
			result.SetBit(31, false);
			result.SetBit(30, false);
			result.SetBit(29, false);
			Key = (KeyboardKey)result;
		} else {
			Key = KeyboardKey.None;
			Ctrl = Shift = Alt = false;
		}
	}
	public readonly bool Holding () {
		if (Ctrl != Input.KeyboardHolding(KeyboardKey.LeftCtrl)) return false;
		if (Shift != Input.KeyboardHolding(KeyboardKey.LeftShift)) return false;
		if (Alt != Input.KeyboardHolding(KeyboardKey.LeftAlt)) return false;
		return Input.KeyboardHolding(Key);
	}
	public readonly bool Down () {
		if (Ctrl != Input.KeyboardHolding(KeyboardKey.LeftCtrl)) return false;
		if (Shift != Input.KeyboardHolding(KeyboardKey.LeftShift)) return false;
		if (Alt != Input.KeyboardHolding(KeyboardKey.LeftAlt)) return false;
		return Input.KeyboardDown(Key);
	}
	public readonly bool DownGUI () {
		if (Ctrl != Input.KeyboardHolding(KeyboardKey.LeftCtrl)) return false;
		if (Shift != Input.KeyboardHolding(KeyboardKey.LeftShift)) return false;
		if (Alt != Input.KeyboardHolding(KeyboardKey.LeftAlt)) return false;
		return Input.KeyboardDownGUI(Key);
	}
	public readonly string GetStringData () => ((uint)Key | (Ctrl ? CTRL_MARK : 0) | (Shift ? SHIFT_MARK : 0) | (Alt ? ALT_MARK : 0)).ToString();
	public override bool Equals (object obj) => obj is Hotkey h && h.Key == Key && h.Ctrl == Ctrl && h.Shift == Shift && h.Alt == Alt;
	public override int GetHashCode () => System.HashCode.Combine(Key, Ctrl, Shift, Alt);
	public override string ToString () => base.ToString();
}