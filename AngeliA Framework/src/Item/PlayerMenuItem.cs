using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class PlayerMenuItem<UI> : Item where UI : PlayerMenuPartnerUI {

	public override int MaxStackCount => 1;
	private static readonly Dictionary<int, PlayerMenuPartnerUI> Pool = [];

	[OnGameInitialize]
	internal static void OnGameInitialize () {
		foreach (var type in typeof(PlayerMenuItem<>).AllChildClass()) {
			var gParams = type.BaseType.GenericTypeArguments;
			if (gParams == null || gParams.Length == 0) continue;
			if (System.Activator.CreateInstance(gParams[0]) is not PlayerMenuPartnerUI ui) continue;
			Pool.TryAdd(type.AngeHash(), ui);
		}
	}

	protected virtual void OnPanelOpened (UI panelUI) { }

	public override bool Use (Character character, int inventoryID, int itemIndex, out bool consume) {
		consume = false;
		if (character != PlayerSystem.Selecting) return false;
		if (!PlayerMenuItem<PlayerMenuPartnerUI>.Pool.TryGetValue(TypeID, out var ui)) return false;
		if (PlayerMenuUI.OpenMenuWithPartner(ui, TypeID)) {
			if (ui is UI) OnPanelOpened(ui as UI);
			return true;
		}
		return false;
	}

	public override bool CanUse (Character character) => character == PlayerSystem.Selecting;

}
