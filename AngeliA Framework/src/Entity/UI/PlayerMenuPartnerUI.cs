namespace AngeliA;

public abstract class PlayerMenuPartnerUI : IWindowEntityUI {

	public int InventoryID { get; set; } = 0;
	public virtual int Column => 1;
	public virtual int Row => 1;
	public virtual int ItemFieldSize => PlayerMenuUI.ITEM_SIZE;
	public bool MouseInPanel { get; set; } = false;
	public IRect BackgroundRect { get; protected set; } = default;

	public virtual void EnablePanel () { }

	public virtual void DrawPanel (IRect panelRect) => BackgroundRect = panelRect;

	protected static int Unify (int value) => GUI.Unify(value);

}
