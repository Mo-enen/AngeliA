namespace AngeliA;

public abstract class PlayerMenuPartnerUI : IWindowEntityUI {

	public int InventoryID { get; private set; } = 0;
	public int Column { get; private set; } = 1;
	public int Row { get; private set; } = 1;
	public int ItemSize { get; private set; } = PlayerMenuUI.ITEM_SIZE;
	public bool MouseInPanel { get; set; } = false;
	public IRect BackgroundRect { get; protected set; } = default;
	
	public virtual void EnablePanel (int inventoryID, int column, int row, int itemSize = PlayerMenuUI.ITEM_SIZE) {
		InventoryID = inventoryID;
		Column = column;
		Row = row;
		ItemSize = itemSize;
	}

	public virtual void DrawPanel (IRect panelRect) => BackgroundRect = panelRect;

	protected static int Unify (int value) => GUI.Unify(value);

}
