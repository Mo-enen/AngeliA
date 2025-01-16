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
	protected string InventoryName { get; private set; } = "";
	protected abstract int InventoryColumn { get; }
	protected abstract int InventoryRow { get; }
	protected virtual bool UnlockItemInside => true;

	private static readonly Dictionary<int, InventoryPartnerUI> UiPool = [];
	private static readonly Dictionary<int, int> InventoryFurniturePool = [];
	private static readonly Dictionary<Int4, (string name, int id)> InventoryIdPool = [];


	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		UiPool.Clear();
		UiPool.Add(InventoryPartnerUI.TYPE_ID, InventoryPartnerUI.Instance);
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
			var squad = WorldSquad.Stream;
			var pos = invPos;
			for (int safe = 0; safe < 2048; safe++) {
				int id = squad.GetBlockAt(pos.x, pos.y, pos.z, BlockType.Entity);
				if (id != TypeID) break;
				invPos = pos;
				pos.x += deltaX;
				pos.y += deltaY;
			}
		}

		// Init Inventory
		if (TryGetInventoryNameAndID(invPos, TypeID, out string invName, out int invID)) {
			InventoryName = invName;
			InventoryID = invID;
			if (InventoryID != 0 && !string.IsNullOrEmpty(InventoryName)) {
				Inventory.InitializeInventoryData(InventoryID, InventoryName, InventoryColumn * InventoryRow);
			}
		}

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


	public static bool TryGetInventoryNameAndID (Int3 unitPos, int typeID, out string invName, out int invID) {
		var key = new Int4(unitPos.x, unitPos.y, unitPos.z, typeID);
		if (InventoryIdPool.TryGetValue(key, out var pair)) {
			(invName, invID) = pair;
			return true;
		} else if (Stage.GetEntityType(typeID) is System.Type entityType) {
			invName = Inventory.GetPositionBasedInventoryName(entityType.AngeName(), unitPos);
			invID = invName.AngeHash();
			InventoryIdPool.Add(key, (invName, invID));
			return true;
		} else {
			invName = "";
			invID = 0;
			return false;
		}
	}


	public static bool IsInventoryFurniture (int typeID, out int capacity) => InventoryFurniturePool.TryGetValue(typeID, out capacity);


}
