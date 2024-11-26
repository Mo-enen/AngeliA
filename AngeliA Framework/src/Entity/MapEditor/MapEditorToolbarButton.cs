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


public abstract class MapEditorToolbarButton {
	public abstract SpriteCode Icon { get; }
	public abstract LanguageCode Tip { get; }
	public abstract System.Action Func { get; }
	public virtual System.Func<bool> Enable => null;
	public virtual System.Func<bool> Active => null;
	public virtual int Order => 0;
	public virtual void ButtonGUI (IRect rect) {
		if (GUI.DarkButton(rect, Icon)) {
			Func?.Invoke();
		}
	}
}

