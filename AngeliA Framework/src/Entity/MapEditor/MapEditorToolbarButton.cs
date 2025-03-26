namespace AngeliA;


internal class BuiltInMapEditorToolbarButton (SpriteCode icon, LanguageCode tip, System.Action func, System.Func<bool> enable = null) : MapEditorToolbarButton {
	public override SpriteCode Icon => _Icon;
	public override LanguageCode Tip => _Tip;
	public override System.Action Func => _Func;
	public override System.Func<bool> Enable => _Enable;
	public override int Order => -4096;
	public SpriteCode _Icon = icon;
	public LanguageCode _Tip = tip;
	public System.Action _Func = func;
	public System.Func<bool> _Enable = enable;
}


/// <summary>
/// Base class for detect toolbar button logic of the map editor
/// </summary>
public abstract class MapEditorToolbarButton {
	/// <summary>
	/// Icon artwork sprite for the button
	/// </summary>
	public abstract SpriteCode Icon { get; }
	/// <summary>
	/// Tooltip which shows when user hover mouse on it
	/// </summary>
	public abstract LanguageCode Tip { get; }
	/// <summary>
	/// This function is called when user click the button
	/// </summary>
	public abstract System.Action Func { get; }
	/// <summary>
	/// True if the button apears to be enabled
	/// </summary>
	public virtual System.Func<bool> Enable => null;
	/// <summary>
	/// True if the button display inside the toolbar
	/// </summary>
	public virtual System.Func<bool> Active => null;
	/// <summary>
	/// Order of the button inside the toolbar
	/// </summary>
	public virtual int Order => 0;
	/// <summary>
	/// Draw UI for the button
	/// </summary>
	/// <param name="rect">Rect position in global space</param>
	public virtual void ButtonGUI (IRect rect) {
		if (GUI.DarkButton(rect, Icon)) {
			Func?.Invoke();
		}
	}
}

