using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace AngeliA;

public static class Inventory {




	#region --- SUB ---


	[System.Serializable]
	private class InventoryData : IJsonSerializationCallback {

		public int[] Items;
		public int[] Counts;
		[JsonIgnore] public string Name;
		[JsonIgnore] public bool UnlockItemInside;
		[JsonIgnore] public bool IsDirty;

		public void Valid () {
			Items ??= new int[0];
			Counts ??= new int[Items.Length];
			if (Counts.Length != Items.Length) {
				var newCounts = new int[Items.Length];
				for (int i = 0; i < newCounts.Length; i++) {
					newCounts[i] = i < Counts.Length ? Counts[i] : 0;
				}
				Counts = newCounts;
			}
		}
		void IJsonSerializationCallback.OnAfterLoadedFromDisk () => Valid();
		void IJsonSerializationCallback.OnBeforeSaveToDisk () => Valid();

	}


	[System.Serializable]
	private class CharacterInventoryData : InventoryData {
		public int Helmet = 0;
		public int BodySuit = 0;
		public int Shoes = 0;
		public int Weapon = 0;
		public int Gloves = 0;
		public int Jewelry = 0;
	}



	#endregion




	#region --- VAR ---


	// Const
	private const string INV_EXT = "inv";
	private const string CHAR_INV_EXT = "chr";

	// Data
	private static readonly Dictionary<int, InventoryData> Pool = new();
	private static bool IsPoolDirty = false;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () => LoadAllFromDisk();


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		foreach (var (_, data) in Pool) {
			UpdateItemUnlocked(data);
		}
	}


	[OnGameUpdate]
	internal static void OnGameUpdate () {
		if (IsPoolDirty) {
			SaveAllToDisk(false);
		}
	}


	#endregion




	#region --- API ---


	public static void AddNewInventoryData (string inventoryName, int itemCount, bool unlockItemInside = false) {
		if (itemCount <= 0) return;
		int inventoryID = inventoryName.AngeHash();
		if (Pool.ContainsKey(inventoryID)) return;
		Pool.Add(inventoryID, new InventoryData() {
			Items = new int[itemCount],
			Counts = new int[itemCount],
			IsDirty = true,
			Name = inventoryName,
			UnlockItemInside = unlockItemInside,
		});
		IsPoolDirty = true;
	}


	public static void AddNewCharacterInventoryData (string inventoryName, int itemCount, bool unlockItemInside = false) {
		if (itemCount <= 0) return;
		int inventoryID = inventoryName.AngeHash();
		if (Pool.ContainsKey(inventoryID)) return;
		Pool.Add(inventoryID, new CharacterInventoryData() {
			Items = new int[itemCount],
			Counts = new int[itemCount],
			IsDirty = true,
			Name = inventoryName,
			UnlockItemInside = unlockItemInside,
		});
		IsPoolDirty = true;
	}


	public static void ResizeItems (int inventoryID, int newSize) {
		if (!Pool.TryGetValue(inventoryID, out var data)) return;
		var items = data.Items;
		if (newSize <= 0 || items.Length == newSize) return;
		var newItems = new int[newSize];
		for (int i = 0; i < newSize; i++) {
			newItems[i] = i < items.Length ? items[i] : 0;
		}
		data.Items = newItems;
		data.IsDirty = true;
		IsPoolDirty = true;
	}


	public static bool HasInventory (int inventoryID) => Pool.ContainsKey(inventoryID);


	public static int GetInventoryCapacity (int inventoryID) => Pool.TryGetValue(inventoryID, out var data) ? data.Items.Length : 0;


	public static void SetUnlockItemsInside (int inventoryID, bool newUnlokInside) {
		if (Pool.TryGetValue(inventoryID, out var data)) {
			data.UnlockItemInside = newUnlokInside;
		}
	}


	// Items
	public static int GetItemAt (int inventoryID, int itemIndex) => GetItemAt(inventoryID, itemIndex, out _);
	public static int GetItemAt (int inventoryID, int itemIndex, out int count) {
		if (Pool.TryGetValue(inventoryID, out var data)) {
			count = itemIndex >= 0 && itemIndex < data.Counts.Length ? data.Counts[itemIndex] : 0;
			return itemIndex >= 0 && itemIndex < data.Items.Length ? data.Items[itemIndex] : 0;
		} else {
			count = 0;
			return 0;
		}
	}


	public static int GetItemCount (int inventoryID, int itemIndex) => Pool.TryGetValue(inventoryID, out var data) && itemIndex >= 0 && itemIndex < data.Counts.Length && data.Items[itemIndex] != 0 ? data.Counts[itemIndex] : 0;


	public static void SetItemAt (int inventoryID, int itemIndex, int newItem, int newCount) {
		if (!Pool.TryGetValue(inventoryID, out var data) || itemIndex < 0 || itemIndex >= data.Items.Length) return;
		data.Items[itemIndex] = newCount > 0 ? newItem : 0;
		data.Counts[itemIndex] = newCount;
		data.IsDirty = true;
		IsPoolDirty = true;
	}


	public static int IndexOfItem (int inventoryID, int itemID) {
		if (!Pool.TryGetValue(inventoryID, out var data)) return -1;
		for (int i = 0; i < data.Items.Length; i++) {
			if (data.Items[i] == itemID) return i;
		}
		return -1;
	}


	/// <returns>How many items has been added. Return 0 means no item added. Return "count" means all items added.</returns>
	public static int AddItemAt (int inventoryID, int itemIndex, int count = 1) {
		if (
			count <= 0 ||
			itemIndex < 0 ||
			!Pool.TryGetValue(inventoryID, out var data) ||
			itemIndex >= data.Items.Length
		) return 0;
		int itemID = data.Items[itemIndex];
		if (itemID == 0) return 0;
		int _count = data.Counts[itemIndex];
		int delta = Util.Min(count, ItemSystem.GetItemMaxStackCount(itemID) - _count);
		data.Counts[itemIndex] = _count + delta;
		if (delta != 0) {
			data.IsDirty = true;
			IsPoolDirty = true;
		}
		return delta;
	}


	/// <returns>How many items has been added. Return 0 means no item added. Return "count" means all items added.</returns>
	public static int FindAndAddItem (int inventoryID, int targetItemID, int count = 1) {
		if (targetItemID == 0 || count <= 0 || !Pool.TryGetValue(inventoryID, out var data)) return 0;
		int oldCount = count;
		int maxCount = ItemSystem.GetItemMaxStackCount(targetItemID);
		for (int i = 0; i < data.Items.Length; i++) {
			if (data.Items[i] != targetItemID) continue;
			int _count = data.Counts[i];
			int delta = Util.Min(count, maxCount - _count);
			data.Counts[i] = _count + delta;
			count -= delta;
			if (count <= 0) break;
		}
		int result = oldCount - count;
		if (result != 0) {
			data.IsDirty = true;
			IsPoolDirty = true;
		}
		return result;
	}


	/// <returns>How many items has been taken. Return 0 means no item taken. Return "count" means all items taken.</returns>
	public static int TakeItemAt (int inventoryID, int itemIndex, int count = 1) {
		if (
			count <= 0 ||
			itemIndex < 0 ||
			!Pool.TryGetValue(inventoryID, out var data) ||
			itemIndex >= data.Items.Length ||
			data.Items[itemIndex] == 0
		) return 0;
		int _count = data.Counts[itemIndex];
		int delta = Util.Min(_count, count).GreaterOrEquelThanZero();
		if (delta == 0) return delta;
		int newCount = _count - delta;
		data.Counts[itemIndex] = newCount;
		if (newCount <= 0) {
			data.Items[itemIndex] = 0;
		}
		if (delta != 0) {
			data.IsDirty = true;
			IsPoolDirty = true;
		}
		return delta;
	}


	/// <returns>How many items has been taken. Return 0 means no item taken. Return "count" means all items taken.</returns>
	public static int FindAndTakeItem (int inventoryID, int targetItemID, int count = 1) {
		if (targetItemID == 0 || count <= 0 || !Pool.TryGetValue(inventoryID, out var data)) return 0;
		int oldCount = count;
		for (int i = 0; i < data.Items.Length; i++) {
			if (data.Items[i] != targetItemID) continue;
			int _count = data.Counts[i];
			int delta = Util.Min(_count, count);
			_count -= delta;
			count -= delta;
			data.Counts[i] = _count;
			if (_count <= 0) data.Items[i] = 0;
			if (count <= 0) break;
		}
		int result = oldCount - count;
		if (result != 0) {
			data.IsDirty = true;
			IsPoolDirty = true;
		}
		return result;
	}


	/// <returns>How many items has been collected. Return 0 means no item collected. Return "count" means all items collected.</returns>
	public static int CollectItem (int inventoryID, int item, int count = 1) => CollectItem(inventoryID, item, out _, count);
	public static int CollectItem (int inventoryID, int item, out int collectIndex, int count = 1) {
		collectIndex = -1;
		if (item == 0 || count <= 0 || !Pool.TryGetValue(inventoryID, out var data)) return 0;
		int oldCount = count;
		int maxStackCount = ItemSystem.GetItemMaxStackCount(item);

		// Try Append to Exists
		for (int i = 0; i < data.Items.Length; i++) {
			int _item = data.Items[i];
			if (_item != item) continue;
			int _count = data.Counts[i];
			if (_count < maxStackCount) {
				// Append Item
				int delta = Util.Min(count, maxStackCount - _count);
				count -= delta;
				_count += delta;
				data.Counts[i] = _count;
				collectIndex = i;
			}
			if (count <= 0) break;
		}
		if (count <= 0) return oldCount - count;

		// Try Add to New Slot
		for (int i = 0; i < data.Items.Length; i++) {
			int _item = data.Items[i];
			if (_item != 0) continue;
			int delta = Util.Min(count, maxStackCount);
			count -= delta;
			data.Items[i] = item;
			data.Counts[i] = delta;
			collectIndex = i;
			if (count <= 0) break;
		}
		int result = oldCount - count;
		if (result != 0) {
			data.IsDirty = true;
			IsPoolDirty = true;
		}
		return result;
	}


	public static int ItemTotalCount (int inventoryID, int itemID, bool ignoreStack = false) {
		int result = 0;
		if (Pool.TryGetValue(inventoryID, out var data)) {
			int len = Util.Min(data.Items.Length, data.Counts.Length);
			for (int i = 0; i < len; i++) {
				if (data.Items[i] == itemID) {
					result += ignoreStack ? 1 : data.Counts[i];
				}
			}
		}
		return result;
	}


	// Equipment
	public static int GetEquipment (int inventoryID, EquipmentType type) => Pool.TryGetValue(inventoryID, out var data) && data is CharacterInventoryData pData ? type switch {
		EquipmentType.Weapon => pData.Weapon,
		EquipmentType.BodyArmor => pData.BodySuit,
		EquipmentType.Helmet => pData.Helmet,
		EquipmentType.Shoes => pData.Shoes,
		EquipmentType.Gloves => pData.Gloves,
		EquipmentType.Jewelry => pData.Jewelry,
		_ => 0,
	} : 0;


	public static bool SetEquipment (int inventoryID, EquipmentType type, int equipmentID) {

		if (
			!Pool.TryGetValue(inventoryID, out var data) ||
			data is not CharacterInventoryData pData
		) return false;

		if (
			equipmentID != 0 &&
			(!ItemSystem.IsEquipment(equipmentID, out var newEquipmentType) || newEquipmentType != type)
		) return false;

		switch (type) {
			case EquipmentType.Weapon:
				pData.Weapon = equipmentID;
				break;
			case EquipmentType.BodyArmor:
				pData.BodySuit = equipmentID;
				break;
			case EquipmentType.Helmet:
				pData.Helmet = equipmentID;
				break;
			case EquipmentType.Shoes:
				pData.Shoes = equipmentID;
				break;
			case EquipmentType.Gloves:
				pData.Gloves = equipmentID;
				break;
			case EquipmentType.Jewelry:
				pData.Jewelry = equipmentID;
				break;
		}
		data.IsDirty = true;
		IsPoolDirty = true;
		return true;
	}


	#endregion




	#region --- LGC ---


	private static void LoadAllFromDisk () {
		IsPoolDirty = false;
		Pool.Clear();
		string root = Util.CombinePaths(Universe.BuiltIn.SavingMetaRoot, "Inventory");
		if (!Util.FolderExists(root)) return;
		foreach (var path in Util.EnumerateFiles(root, true, $"*.{INV_EXT}", $"*.{CHAR_INV_EXT}")) {
			try {
				string name = Util.GetNameWithoutExtension(path);
				int id = name.AngeHash();
				if (Pool.ContainsKey(id)) continue;
				InventoryData data;
				if (path.EndsWith(INV_EXT)) {
					data = JsonUtil.LoadOrCreateJsonFromPath<InventoryData>(path);
				} else {
					data = JsonUtil.LoadOrCreateJsonFromPath<CharacterInventoryData>(path);
				}
				if (data == null) continue;
				data.IsDirty = false;
				data.Name = name;
				Pool.TryAdd(id, data);
				// Valid Item Count
				for (int i = 0; i < data.Items.Length; i++) {
					int iCount = data.Counts[i];
					if (iCount <= 0) {
						data.Counts[i] = 0;
						data.Items[i] = 0;
					}
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
	}


	private static void SaveAllToDisk (bool forceSave) {
		IsPoolDirty = false;
		string root = Util.CombinePaths(Universe.BuiltIn.SavingMetaRoot, "Inventory");
		foreach (var (_, data) in Pool) {
			if (!forceSave && !data.IsDirty) continue;
			data.IsDirty = false;
			// Save Inventory
			string path = Util.CombinePaths(root, $"{data.Name}.{(data is CharacterInventoryData ? CHAR_INV_EXT : INV_EXT)}");
			JsonUtil.SaveJsonToPath(data, path, false);
			// Update Item Unlocked
			UpdateItemUnlocked(data);
		}
	}


	private static void UpdateItemUnlocked (InventoryData data) {
		if (data == null || !data.UnlockItemInside) return;
		for (int i = 0; i < data.Items.Length; i++) {
			int itemID = data.Items[i];
			int itemCount = data.Counts[i];
			if (itemID == 0 || itemCount <= 0) continue;
			ItemSystem.SetItemUnlocked(itemID, true);
		}
	}


	#endregion




}