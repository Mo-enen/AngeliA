namespace AngeliA;

public sealed class InventoryPartnerUI : PlayerMenuPartnerUI {
	public static readonly InventoryPartnerUI Instance = new();
	public int AvatarID { get; set; } = 0;
	public override void DrawPanel (IRect panelRect) {
		base.DrawPanel(panelRect);
		PlayerMenuUI.DrawTopInventory(InventoryID, Column, Row, AvatarID);
	}
}
