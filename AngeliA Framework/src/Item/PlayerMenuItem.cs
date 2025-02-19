using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class PlayerMenuItem<UI> : Item where UI : PlayerMenuPartnerUI {

	public override int MaxStackCount => 1;
	private static readonly Dictionary<int, PlayerMenuPartnerUI> Pool = [];
	private readonly int InventoryID;
	protected abstract int Row { get; }
	protected abstract int Column { get; }

	[OnGameInitialize]
	internal static void OnGameInitialize () {
		foreach (var type in typeof(PlayerMenuItem<>).AllChildClass()) {
			var gParams = type.BaseType.GenericTypeArguments;
			if (gParams == null || gParams.Length == 0) continue;
			if (System.Activator.CreateInstance(gParams[0]) is not PlayerMenuPartnerUI ui) continue;
			Pool.TryAdd(type.AngeHash(), ui);
		}
		Pool.TrimExcess();
	}

	public PlayerMenuItem () => InventoryID = Inventory.InitializeInventoryData(GetType().AngeName(), Row * Column, hasEquipment: false);

	protected virtual void OnPanelOpened (UI panelUI) { }

	public override bool Use (Character character, int inventoryID, int itemIndex, out bool consume) {
		consume = false;
		if (character != PlayerSystem.Selecting) return false;
		if (InventoryID == 0) return false;
		if (!PlayerMenuItem<PlayerMenuPartnerUI>.Pool.TryGetValue(TypeID, out var ui)) return false;
		if (PlayerMenuUI.OpenMenuWithPartner(ui, InventoryID)) {
			if (ui is UI) OnPanelOpened(ui as UI);
			if (ui is InventoryPartnerUI invUI) {
				invUI._Column = Column;
				invUI._Row = Row;
			}
			return true;
		}
		return false;
	}

	public override bool CanUse (Character character) => character == PlayerSystem.Selecting;

}
