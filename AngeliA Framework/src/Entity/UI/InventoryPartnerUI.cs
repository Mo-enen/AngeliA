namespace AngeliA;

/// <summary>
/// A partner UI display on top of the player menu that display and manage an inventory
/// </summary>
public class InventoryPartnerUI : PlayerMenuPartnerUI {

	public override int Column => _Column;
	public override int Row => _Row;
	/// <summary>
	/// Artwork sprite ID of the avatar icon
	/// </summary>
	public int AvatarID { get; set; } = 0;
	/// <summary>
	/// Column count of the inventory
	/// </summary>
	public int _Column { get; set; }
	/// <summary>
	/// Row count of the inventory
	/// </summary>
	public int _Row { get; set; }
	public override void DrawPanel (IRect panelRect) {
		base.DrawPanel(panelRect);
		PlayerMenuUI.DrawTopInventory(InventoryID, Column, Row, AvatarID);
	}
}
