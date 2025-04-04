using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// A type of furniture that player can put items inside
/// </summary>
/// <typeparam name="UI">Which type of UI does this furniture shows</typeparam>
public abstract class InventoryFurniture<UI> : InventoryFurniture where UI : InventoryPartnerUI {
	public InventoryFurniture () => PartnerID = typeof(UI).AngeHash();
}


/// <summary>
/// A type of furniture that player can put items inside
/// </summary>
public abstract class InventoryFurniture : OpenableFurniture, IActionTarget {


	// VAR
	/// <summary>
	/// Type ID of the UI entity
	/// </summary>
	protected int PartnerID { get; init; }
	/// <summary>
	/// ID for inventory system
	/// </summary>
	protected int InventoryID { get; private set; } = 0;
	/// <summary>
	/// Column count of the inventory UI
	/// </summary>
	protected abstract int InventoryColumn { get; }
	/// <summary>
	/// Row count of the inventory UI
	/// </summary>
	protected abstract int InventoryRow { get; }
	/// <summary>
	/// True if items inside become unlocked for the player
	/// </summary>
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
	/// <summary>
	/// Open the inventory UI
	/// </summary>
	/// <returns>True if the UI successfuly opens</returns>
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


	/// <summary>
	/// True if the player can open the inventory UI
	/// </summary>
	public override bool AllowInvoke () => InventoryID != 0 && base.AllowInvoke();


	protected override void SetOpen (bool open) {
		if (Open && !open) PlayerMenuUI.CloseMenu();
		base.SetOpen(open);
	}


	/// <summary>
	/// Get the instance of the inventory UI from given furniture ID
	/// </summary>
	/// <param name="typeID">Type ID of the furniture</param>
	/// <param name="result"></param>
	/// <returns>True if the result is founded</returns>
	protected bool TryGetInventoryUI (int typeID, out InventoryPartnerUI result) => UiPool.TryGetValue(typeID, out result);


	/// <summary>
	/// True if the given type ID refers to a valid inventory furniture
	/// </summary>
	/// <param name="typeID"></param>
	/// <param name="capacity">Inventory size limit</param>
	public static bool IsInventoryFurniture (int typeID, out int capacity) => InventoryFurniturePool.TryGetValue(typeID, out capacity);


}
