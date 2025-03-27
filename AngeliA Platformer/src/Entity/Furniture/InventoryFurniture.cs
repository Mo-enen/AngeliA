using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


public abstract class InventoryFurniture<UI> : InventoryFurniture where UI : InventoryPartnerUI {
	public InventoryFurniture () => PartnerID = typeof(UI).AngeHash();
}


public abstract class InventoryFurniture : OpenableFurniture, IActionTarget {


	// VAR
	protected int PartnerID { get; init; }
	protected int InventoryID { get; private set; } = 0;
	protected abstract int InventoryColumn { get; }
	protected abstract int InventoryRow { get; }
	protected virtual bool UnlockItemInside => true;

	private static readonly Dictionary<int, InventoryPartnerUI> UiPool = [];
	private static readonly Dictionary<int, int> InventoryFurniturePool = [];
	private readonly string TypeName;


	// MSG
	public InventoryFurniture () => TypeName = GetType().AngeName();


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		UiPool.Clear();
		var invUI = new InventoryPartnerUI();
		UiPool.Add(invUI.GetType().AngeHash(), invUI);
		foreach (var type in typeof(InventoryPartnerUI).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not InventoryPartnerUI ui) continue;
			UiPool.TryAdd(type.AngeHash(), ui);
		}
		UiPool.TrimExcess();
		// Inv Set
		InventoryFurniturePool.Clear();
		foreach (var type in typeof(InventoryFurniture).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not InventoryFurniture invF) continue;
			InventoryFurniturePool.TryAdd(type.AngeHash(), invF.InventoryRow * invF.InventoryColumn);
		}
		InventoryFurniturePool.TrimExcess();
	}


	public override void OnActivated () {
		base.OnActivated();

		// Get Inv Pos
		var invPos = new Int3((X + 1).ToUnit(), (Y + 1).ToUnit(), Stage.ViewZ);
		if (ModuleType != Direction3.None) {
			var (deltaX, deltaY) = ModuleType == Direction3.Horizontal ? (-1, 0) : (0, -1);
			var squad = WorldSquad.Front;
			var pos = invPos;
			for (int safe = 0; safe < 2048; safe++) {
				int id = squad.GetBlockAt(pos.x, pos.y, pos.z, BlockType.Entity);
				if (id != TypeID) break;
				invPos = pos;
				pos.x += deltaX;
				pos.y += deltaY;
			}
		}

		// Init Inv
		InventoryID = Inventory.InitializeInventoryData(
			TypeID, TypeName, InventoryColumn * InventoryRow, invPos, hasEquipment: false
		);

	}


	public override void LateUpdate () {
		base.LateUpdate();
		// UI Close Check
		if (Open && !PlayerMenuUI.ShowingUI) {
			SetOpen(false);
		}
	}


	// API
	public override bool Invoke () {
		if (InventoryID == 0) return false;
		if (!Open) SetOpen(true);
		// Spawn UI Entity
		if (PlayerSystem.Selecting == null) return false;
		if (!UiPool.TryGetValue(PartnerID, out var ins)) return false;
		if (!PlayerMenuUI.OpenMenuWithPartner(ins, InventoryID)) {
			return false;
		}
		ins.AvatarID = TypeID;
		ins._Column = InventoryColumn;
		ins._Row = InventoryRow;
		if (UnlockItemInside) {
			Inventory.UnlockAllItemsInside(InventoryID);
		}
		return true;
	}


	public override bool AllowInvoke () => InventoryID != 0 && base.AllowInvoke();


	protected override void SetOpen (bool open) {
		if (Open && !open) PlayerMenuUI.CloseMenu();
		base.SetOpen(open);
	}


	protected bool TryGetInventoryUI (int typeID, out InventoryPartnerUI result) => UiPool.TryGetValue(typeID, out result);


	public static bool IsInventoryFurniture (int typeID, out int capacity) => InventoryFurniturePool.TryGetValue(typeID, out capacity);


}
