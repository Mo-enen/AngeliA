using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public abstract class InventoryFurniture<UI> : OpenableFurniture, IActionTarget where UI : InventoryPartnerUI {


	// VAR
	protected int InventoryID { get; private set; } = 0;
	protected string InventoryName { get; private set; } = "";
	protected abstract int InventoryColumn { get; }
	protected abstract int InventoryRow { get; }
	protected virtual bool UnlockItemInside => false;

	private static readonly Dictionary<int, InventoryPartnerUI> UiPool = [];
	private static readonly Dictionary<Int4, (string name, int id)> InventoryIdPool = [];
	private int PartnerID { get; init; }


	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		var pool = InventoryFurniture<InventoryPartnerUI>.UiPool;
		pool.Clear();
		pool.Add(InventoryPartnerUI.TYPE_ID, InventoryPartnerUI.Instance);
		foreach (var type in typeof(InventoryPartnerUI).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not InventoryPartnerUI ui) continue;
			pool.TryAdd(type.AngeHash(), ui);
		}
	}


	public InventoryFurniture () => PartnerID = typeof(UI).AngeHash();


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

		// Get Inv ID
		var key = new Int4(invPos.x, invPos.y, invPos.z, TypeID);
		if (InventoryFurniture<InventoryPartnerUI>.InventoryIdPool.TryGetValue(key, out var pair)) {
			(InventoryName, InventoryID) = pair;
		} else {
			InventoryName = Inventory.GetPositionBasedInventoryName(GetType().AngeName(), invPos);
			InventoryID = InventoryName.AngeHash();
			InventoryIdPool.Add(key, (InventoryName, InventoryID));
		}

		// Init Inventory
		if (InventoryID != 0 && !string.IsNullOrEmpty(InventoryName)) {
			Inventory.InitializeInventoryData(InventoryName, InventoryColumn * InventoryRow);
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
		if (!InventoryFurniture<InventoryPartnerUI>.UiPool.TryGetValue(PartnerID, out var ins)) return false;
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
		if (Open && !open) {
			PlayerMenuUI.CloseMenu();
		}
		base.SetOpen(open);
	}


}
