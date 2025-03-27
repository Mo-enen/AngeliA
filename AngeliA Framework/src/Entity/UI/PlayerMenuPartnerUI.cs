namespace AngeliA;

/// <summary>
/// Class for customize partner ui for player menu ui
/// </summary>
public abstract class PlayerMenuPartnerUI : IWindowEntityUI {

	/// <summary>
	/// ID for inventory system
	/// </summary>
	public int InventoryID { get; set; } = 0;
	/// <summary>
	/// Inventory column count
	/// </summary>
	public virtual int Column => 1;
	/// <summary>
	/// Inventory row count
	/// </summary>
	public virtual int Row => 1;
	/// <summary>
	/// Unified size of a single item field
	/// </summary>
	public virtual int ItemFieldSize => 42;
	/// <summary>
	/// Trie if mouse cursor is currently inside this panel
	/// </summary>
	public bool MouseInPanel { get; set; } = false;
	/// <summary>
	/// Rect position of the background range in global space
	/// </summary>
	public IRect BackgroundRect { get; protected set; } = default;

	/// <summary>
	/// This function is called when the panel start to display
	/// </summary>
	public virtual void EnablePanel () { }

	/// <summary>
	/// This function is called every frame for displaying this panel
	/// </summary>
	/// <param name="panelRect">Rect position for the range of this panel in global space</param>
	public virtual void DrawPanel (IRect panelRect) => BackgroundRect = panelRect;

	/// <inheritdoc cref="GUI.Unify"/>
	protected static int Unify (int value) => GUI.Unify(value);

}
