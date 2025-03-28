using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// A type of item that spawns a player partner menu when use
/// </summary>
/// <typeparam name="UI">What type of menu does it spawns</typeparam>
public abstract class PlayerMenuItem<UI> : Item where UI : PlayerMenuPartnerUI {

	public override int MaxStackCount => 1;
	private static readonly Dictionary<int, PlayerMenuPartnerUI> Pool = [];
	private readonly int InventoryID;

	/// <summary>
	/// Inventory row count of the menu
	/// </summary>
	protected abstract int Row { get; }
	/// <summary>
	/// Inventory column count of the menu
	/// </summary>
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

	/// <summary>
	/// This function is called when the menu is spawned
	/// </summary>
	/// <param name="panelUI">Instance of the menu</param>
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
