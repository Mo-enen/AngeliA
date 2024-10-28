using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;

public abstract class InventoryChest : OpenableFurniture, IActionTarget {


	// VAR
	protected virtual int InventoryColumn => 10;
	protected virtual int InventoryRow => 4;
	protected virtual bool UnlockItemInside => false;
	public int InventoryID { get; private set; } = 0;

	private static readonly Dictionary<Int3, (string name, int id)> InventoryIdPool = [];
	private static readonly int[] LockingCheckCache = new int[64];
	private string InventoryName = "";


	// API
	public override void OnActivated () {
		base.OnActivated();

		// Get Inv ID
		if (MapUnitPos.HasValue) {
			if (InventoryIdPool.TryGetValue(MapUnitPos.Value, out var pair)) {
				(InventoryName, InventoryID) = pair;
			} else {
				InventoryName = Inventory.GetPositionBasedInventoryName(nameof(InventoryChest), MapUnitPos.Value);
				InventoryID = InventoryName.AngeHash();
				pair = (InventoryName, InventoryID);
				InventoryIdPool.Add(MapUnitPos.Value, pair);
			}
		} else {
			InventoryName = "";
			InventoryID = 0;
		}

		// Init Inventory
		if (InventoryID != 0 && !string.IsNullOrEmpty(InventoryName)) {
			int targetCount = InventoryColumn * InventoryRow;
			if (Inventory.HasInventory(InventoryID)) {
				int iCount = Inventory.GetInventoryCapacity(InventoryID);
				// Resize
				if (iCount != targetCount) {
					Inventory.ResizeInventory(InventoryID, targetCount);
				}
			} else {
				// Create New Items
				Inventory.AddNewInventoryData(InventoryName, targetCount);
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

	public virtual bool Invoke () {
		if (!Open) SetOpen(true);
		// Spawn UI Entity
		if (PlayerSystem.Selecting == null) return false;
		if (!PlayerMenuUI.OpenMenuWithPartner(InventoryPartnerUI.Instance, InventoryID)) {
			return false;
		}
		var ins = InventoryPartnerUI.Instance;
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

	public static bool UnlockableCheck (int lockInvID, int keyInvID, int lockId0, int lockId1, int lockId2, int keyId0, int keyId1, int keyId2) {

		// Get Lock Pattern
		int patternLen = 0;
		int len = Inventory.GetInventoryCapacity(lockInvID);
		for (int i = 0; i < len; i++) {
			int itemID = Inventory.GetItemAt(lockInvID, i, out int iCount);
			if (itemID == 0 || iCount <= 0) continue;
			if (itemID == lockId0) {
				LockingCheckCache[patternLen] = 0;
			} else if (itemID == lockId1) {
				LockingCheckCache[patternLen] = 1;
			} else if (itemID == lockId2) {
				LockingCheckCache[patternLen] = 2;
			} else {
				continue;
			}
			patternLen++;
			if (patternLen >= LockingCheckCache.Length) break;
		}

		// Not Locked
		if (patternLen == 0) return true;

		// Check Key Pattern
		int patternIndex = 0;
		int matchCount = 0;
		len = Inventory.GetInventoryCapacity(keyInvID);
		for (int i = 0; i < len; i++) {
			int itemID = Inventory.GetItemAt(keyInvID, i, out int iCount);
			if (itemID == 0 || iCount <= 0) continue;
			if (itemID == keyId0) {
				if (LockingCheckCache[patternIndex] == 0) {
					matchCount++;
				} else {
					return false;
				}
			} else if (itemID == keyId1) {
				if (LockingCheckCache[patternIndex] == 1) {
					matchCount++;
				} else {
					return false;
				}
			} else if (itemID == keyId2) {
				if (LockingCheckCache[patternIndex] == 2) {
					matchCount++;
				} else {
					return false;
				}
			} else {
				continue;
			}
			patternIndex++;
			if (patternIndex >= patternLen) break;
		}

		// Final
		return matchCount >= patternLen;
	}

}