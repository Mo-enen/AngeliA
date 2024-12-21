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

		// Get Inv ID
		var mPos = MapUnitPos ?? new Int3(int.MinValue, int.MinValue, int.MinValue);
		var key = new Int4(mPos.x, mPos.y, mPos.z, TypeID);
		if (InventoryFurniture<InventoryPartnerUI>.InventoryIdPool.TryGetValue(key, out var pair)) {
			(InventoryName, InventoryID) = pair;
		} else {
			(InventoryID, InventoryName) = GetInventoryIdAndName();
			pair = (InventoryName, InventoryID);
			InventoryIdPool.Add(key, pair);
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
	public virtual bool Invoke () {
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


	bool IActionTarget.AllowInvoke () => PlayerSystem.Selecting != null;


	protected override void SetOpen (bool open) {
		if (Open && !open) {
			PlayerMenuUI.CloseMenu();
		}
		base.SetOpen(open);
	}


	// LGC
	protected virtual (int id, string name) GetInventoryIdAndName () {
		if (MapUnitPos.HasValue) {
			string name = Inventory.GetPositionBasedInventoryName(GetType().AngeName(), MapUnitPos.Value);
			int id = name.AngeHash();
			return (id, name);
		} else {
			return (TypeID, GetType().AngeName());
		}
	}


}
