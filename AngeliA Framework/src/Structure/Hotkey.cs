namespace AngeliA;

/// <summary>
/// Data for a keyboard hotkey configuration
/// </summary>
public readonly struct Hotkey {

	private const uint CTRL_MARK = 0b10000000000000000000000000000000;
	private const uint ALT_MARK = 0b01000000000000000000000000000000;
	private const uint SHIFT_MARK = 0b00100000000000000000000000000000;

	/// <summary>
	/// The target keyboard key
	/// </summary>
	public readonly KeyboardKey Key;

	/// <summary>
	/// True if this hotkey require ctrl to be holding
	/// </summary>
	public readonly bool Ctrl;

	/// <summary>
	/// True if this hotkey require shift to be holding
	/// </summary>
	public readonly bool Shift;

	/// <summary>
	/// True if this hotkey require alt to be holding
	/// </summary>
	public readonly bool Alt;

	public Hotkey (KeyboardKey key, bool ctrl = false, bool shift = false, bool alt = false) {
		Key = key;
		Ctrl = ctrl;
		Shift = shift;
		Alt = alt;
	}

	/// <summary>
	/// Create a hotkey config from string data. Get this string using hotkey.GetStringData();
	/// </summary>
	/// <param name="data"></param>
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

	/// <summary>
	/// True if the hotkey is currently holding
	/// </summary>
	public readonly bool Holding () {
		if (Ctrl != Input.HoldingCtrl) return false;
		if (Shift != Input.HoldingShift) return false;
		if (Alt != Input.HoldingAlt) return false;
		return Input.KeyboardHolding(Key);
	}

	/// <summary>
	/// True if the hotkey is pressed for the current frame
	/// </summary>
	public readonly bool Down () {
		if (Ctrl != Input.HoldingCtrl) return false;
		if (Shift != Input.HoldingShift) return false;
		if (Alt != Input.HoldingAlt) return false;
		return Input.KeyboardDown(Key);
	}

	/// <summary>
	/// True if the hotkey is triggered repeatedly by holding
	/// </summary>
	public readonly bool DownGUI () {
		if (Ctrl != Input.HoldingCtrl) return false;
		if (Shift != Input.HoldingShift) return false;
		if (Alt != Input.HoldingAlt) return false;
		return Input.KeyboardDownGUI(Key);
	}

	/// <summary>
	/// Get a string that saves the infomation of this hotkey
	/// </summary>
	public readonly string GetStringData () => ((uint)Key | (Ctrl ? CTRL_MARK : 0) | (Shift ? SHIFT_MARK : 0) | (Alt ? ALT_MARK : 0)).ToString();

	public override bool Equals (object obj) => obj is Hotkey h && h.Key == Key && h.Ctrl == Ctrl && h.Shift == Shift && h.Alt == Alt;
	public override int GetHashCode () => System.HashCode.Combine(Key, Ctrl, Shift, Alt);
	public override string ToString () => base.ToString();

}