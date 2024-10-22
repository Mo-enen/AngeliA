namespace AngeliA;

public sealed class InventoryPartnerUI : PlayerMenuPartnerUI {
	public static readonly InventoryPartnerUI Instance = new();
	public override int Column => _Column;
	public override int Row => _Row;
	public int AvatarID { get; set; } = 0;
	public int _Column { get; set; }
	public int _Row { get; set; }
	public override void DrawPanel (IRect panelRect) {
		base.DrawPanel(panelRect);
		PlayerMenuUI.DrawTopInventory(InventoryID, Column, Row, AvatarID);
	}
}
