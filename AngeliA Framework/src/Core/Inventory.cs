using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;


namespace AngeliA;


public static class Inventory {




	#region --- SUB ---


	private class InventoryData : IJsonSerializationCallback {

		public int[] Items;
		public int[] Counts;
		[JsonIgnore] public string Name;
		[JsonIgnore] public bool IsDirty;

		public void Valid () {
			Items ??= [];
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


	private class EquipmentInventoryData : InventoryData {
		public int HandTool = 0;
		public int Helmet = 0;
		public int BodySuit = 0;
		public int Shoes = 0;
		public int Gloves = 0;
		public int Jewelry = 0;
		public int HandToolCount = 0;
		public int HelmetCount = 0;
		public int BodySuitCount = 0;
		public int ShoesCount = 0;
		public int GlovesCount = 0;
		public int JewelryCount = 0;
	}



	#endregion




	#region --- VAR ---


	// Api
	public static bool PoolReady { get; private set; } = false;

	// Data
	private static readonly Dictionary<int, InventoryData> Pool = [];
	private static bool IsPoolDirty = false;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	[OnSavingSlotChanged]
	internal static void OnGameInitialize () {
		LoadInventoryPoolFromDisk();
		PoolReady = true;
	}


	[OnGameInitializeLater]
	internal static TaskResult OnGameInitializeLater () {

		if (!ItemSystem.ItemPoolReady) return TaskResult.Continue;

		// Init Cheat Code
		var giveItemCheatInfo = typeof(Inventory).GetMethod(
			nameof(GiveItemCheat),
			BindingFlags.NonPublic | BindingFlags.Static
		);

		// Cheat from Code
		var BLOCK_ITEM = typeof(BlockBuilder);
		foreach (var type in typeof(Item).AllChildClass()) {
			if (type == BLOCK_ITEM) continue;
			if (System.Activator.CreateInstance(type) is not Item item) continue;
			string angeName = type.AngeName();
			int id = angeName.AngeHash();
			CheatSystem.TryAddCheatAction($"Give{angeName}", giveItemCheatInfo, id);
		}

		// Cheat from Block Entity
		foreach (var type in typeof(IBlockEntity).AllClassImplemented()) {
			string angeName = type.AngeName();
			int id = angeName.AngeHash();
			var blockItem = new BlockBuilder(id, angeName, BlockType.Entity);
			CheatSystem.TryAddCheatAction($"Give{angeName}", giveItemCheatInfo, id);
		}

		return TaskResult.End;
	}


	[BeforeSavingSlotChanged]
	internal static void BeforeSavingSlotChanged () => PoolReady = false;


	[OnMainSheetReload]
	internal static void OnMainSheetReload () {

		if (Game.IsToolApplication) return;
		var sheet = Renderer.MainSheet;
		if (sheet == null) return;

		// Add Block Items
		var giveItemCheatInfo = typeof(Inventory).GetMethod(
			nameof(GiveItemCheat),
			BindingFlags.NonPublic | BindingFlags.Static
		);
		var span = sheet.Sprites.GetSpan();
		int len = span.Length;
		for (int i = 0; i < len; i++) {
			var sprite = span[i];
			var bType = sprite.Atlas.Type;
			if (bType == AtlasType.General) continue;
			int itemID = sprite.Group != null ? sprite.Group.ID : sprite.ID;
			string itemName = sprite.Group != null ? sprite.Group.Name : sprite.RealName;
			CheatSystem.TryAddCheatAction($"Give{itemName.Replace(" ", "")}", giveItemCheatInfo, itemID);
		}
	}


	[OnGameUpdate]
	internal static void OnGameUpdate () {
		if (IsPoolDirty && Game.GlobalFrame % 300 == 0) {
			SaveAllToDisk(forceSave: false);
		}
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () => SaveAllToDisk(forceSave: true);


	#endregion




	#region --- API ---


	// Inventory Data
	public static void AddNewInventoryData (string inventoryName, int itemCount) {
		if (itemCount <= 0) return;
		int inventoryID = inventoryName.AngeHash();
		if (Pool.ContainsKey(inventoryID)) return;
		Pool.Add(inventoryID, new InventoryData() {
			Items = new int[itemCount],
			Counts = new int[itemCount],
			IsDirty = true,
			Name = inventoryName,
		});
		IsPoolDirty = true;
	}


	public static void AddNewEquipmentInventoryData (string inventoryName, int itemCount) {
		if (itemCount <= 0) return;
		int inventoryID = inventoryName.AngeHash();
		if (Pool.ContainsKey(inventoryID)) return;
		Pool.Add(inventoryID, new EquipmentInventoryData() {
			Items = new int[itemCount],
			Counts = new int[itemCount],
			IsDirty = true,
			Name = inventoryName,
		});
		IsPoolDirty = true;
	}


	public static string GetPositionBasedInventoryName (string baseName, Int3 unitPosition) => $"{baseName}.{unitPosition.x}.{unitPosition.y}.{unitPosition.z}";


	public static void ResizeInventory (int inventoryID, int newSize) {
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


	public static void RenameEquipInventory (string currentName, string newName) {

		// Change Data in Pool
		int id = currentName.AngeHash();
		int newID = newName.AngeHash();
		if (Pool.Remove(id, out var invData)) {
			invData.Name = newName;
			Pool[newID] = invData;
		}

		// Move File
		string root = Universe.BuiltIn.SlotInventoryRoot;
		string from = Util.CombinePaths(root, $"{currentName}.{AngePath.EQ_INVENTORY_FILE_EXT}");
		string to = Util.CombinePaths(root, $"{newName}.{AngePath.EQ_INVENTORY_FILE_EXT}");
		Util.MoveFile(from, to);

	}


	public static void UnlockAllItemsInside (int inventoryID) {
		if (!Pool.TryGetValue(inventoryID, out var data)) return;
		for (int i = 0; i < data.Items.Length; i++) {
			int id = data.Items[i];
			if (data.Counts[i] <= 0 || id == 0) continue;
			ItemSystem.SetItemUnlocked(id, true);
		}
		if (data is EquipmentInventoryData eqData) {
			if (eqData.HelmetCount > 0 && eqData.Helmet != 0) {
				ItemSystem.SetItemUnlocked(eqData.Helmet, true);
			}
			if (eqData.BodySuitCount > 0 && eqData.BodySuit != 0) {
				ItemSystem.SetItemUnlocked(eqData.BodySuit, true);
			}
			if (eqData.GlovesCount > 0 && eqData.Gloves != 0) {
				ItemSystem.SetItemUnlocked(eqData.Gloves, true);
			}
			if (eqData.HandToolCount > 0 && eqData.HandTool != 0) {
				ItemSystem.SetItemUnlocked(eqData.HandTool, true);
			}
			if (eqData.ShoesCount > 0 && eqData.Shoes != 0) {
				ItemSystem.SetItemUnlocked(eqData.Shoes, true);
			}
			if (eqData.JewelryCount > 0 && eqData.Jewelry != 0) {
				ItemSystem.SetItemUnlocked(eqData.Jewelry, true);
			}
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
	public static int CollectItem (int inventoryID, int item, int count = 1, bool ignoreEquipment = true) => CollectItem(inventoryID, item, out _, count, ignoreEquipment);
	public static int CollectItem (int inventoryID, int item, out int collectIndex, int count = 1, bool ignoreEquipment = true) {

		collectIndex = -1;
		if (item == 0 || count <= 0 || !Pool.TryGetValue(inventoryID, out var data)) return 0;
		int oldCount = count;
		int maxStackCount = ItemSystem.GetItemMaxStackCount(item);

		// Try Append to Equipment
		if (
			!ignoreEquipment &&
			data is EquipmentInventoryData eData &&
			ItemSystem.IsEquipment(item, out var eqType)
		) {
			switch (eqType) {

				case EquipmentType.HandTool:
					// Hand
					bool emptyH = eData.HandTool == 0;
					if (emptyH || (eData.HandTool == item && eData.HandToolCount < maxStackCount)) {
						int delta = Util.Min(count, maxStackCount - eData.HandToolCount);
						count -= delta;
						eData.HandTool = item;
						eData.HandToolCount += delta;
						if (emptyH) {
							FillEquipmentFromInventoryLogic(eData, eqType, item, eData.HandToolCount, maxStackCount);
						}
					}
					if (count <= 0) return oldCount - count;
					break;

				case EquipmentType.BodyArmor:
					// Body
					bool emptyB = eData.BodySuit == 0;
					if (emptyB || (eData.BodySuit == item && eData.BodySuitCount < maxStackCount)) {
						int delta = Util.Min(count, maxStackCount - eData.BodySuitCount);
						count -= delta;
						eData.BodySuitCount += delta;
						if (emptyB) {
							FillEquipmentFromInventoryLogic(eData, eqType, item, eData.BodySuitCount, maxStackCount);
						}
					}
					if (count <= 0) return oldCount - count;
					break;

				case EquipmentType.Helmet:
					// Helmet
					bool emptyHl = eData.Helmet == 0;
					if (emptyHl || (eData.Helmet == item && eData.HelmetCount < maxStackCount)) {
						int delta = Util.Min(count, maxStackCount - eData.HelmetCount);
						count -= delta;
						eData.HelmetCount += delta;
						if (emptyHl) {
							FillEquipmentFromInventoryLogic(eData, eqType, item, eData.HelmetCount, maxStackCount);
						}
					}
					if (count <= 0) return oldCount - count;
					break;

				case EquipmentType.Shoes:
					// Shoes
					bool emptyS = eData.Shoes == 0;
					if (emptyS || (eData.Shoes == item && eData.ShoesCount < maxStackCount)) {
						int delta = Util.Min(count, maxStackCount - eData.ShoesCount);
						count -= delta;
						eData.ShoesCount += delta;
						if (emptyS) {
							FillEquipmentFromInventoryLogic(eData, eqType, item, eData.ShoesCount, maxStackCount);
						}
					}
					if (count <= 0) return oldCount - count;
					break;

				case EquipmentType.Gloves:
					// Gloves
					bool emptyG = eData.Gloves == 0;
					if (emptyG || (eData.Gloves == item && eData.GlovesCount < maxStackCount)) {
						int delta = Util.Min(count, maxStackCount - eData.GlovesCount);
						count -= delta;
						eData.GlovesCount += delta;
						if (emptyG) {
							FillEquipmentFromInventoryLogic(eData, eqType, item, eData.GlovesCount, maxStackCount);
						}
					}
					if (count <= 0) return oldCount - count;
					break;

				case EquipmentType.Jewelry:
					// Jewelry
					bool emptyJ = eData.Jewelry == 0;
					if (emptyJ || (eData.Jewelry == item && eData.JewelryCount < maxStackCount)) {
						int delta = Util.Min(count, maxStackCount - eData.JewelryCount);
						count -= delta;
						eData.JewelryCount += delta;
						if (emptyJ) {
							FillEquipmentFromInventoryLogic(eData, eqType, item, eData.JewelryCount, maxStackCount);
						}
					}
					if (count <= 0) return oldCount - count;
					break;

			}

		}

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


	// Give
	public static bool GiveItemToTarget (Entity target, int itemID, int count = 1, bool spawnWhenInventoryFull = true) {
		if (target == null) {
			return
				spawnWhenInventoryFull &&
				ItemSystem.SpawnItem(itemID, Renderer.CameraRect.CenterX(), Renderer.CameraRect.CenterY(), count) != null;
		} else {
			count -= CollectItem(target is Character cTarget ? cTarget.InventoryID : target.TypeID, itemID, count, ignoreEquipment: false);
			return count <= 0 || (spawnWhenInventoryFull && ItemSystem.SpawnItem(itemID, target.Rect.x - Const.CEL, target.Y, count) != null);
		}
	}


	internal static void GiveItemCheat () {
		var player = PlayerSystem.Selecting;
		if (player == null) return;
		if (CheatSystem.CurrentParam is not int id) return;
		ItemSystem.SetItemUnlocked(id, true);
		GiveItemToTarget(player, id, 1);
	}


	// Equipment
	public static int GetEquipment (int inventoryID, EquipmentType type, out int equipmentCount) {
		if (Pool.TryGetValue(inventoryID, out var data) && data is EquipmentInventoryData pData) {
			(int resultID, equipmentCount) = type switch {
				EquipmentType.HandTool => (pData.HandTool, pData.HandToolCount),
				EquipmentType.BodyArmor => (pData.BodySuit, pData.BodySuitCount),
				EquipmentType.Helmet => (pData.Helmet, pData.HelmetCount),
				EquipmentType.Shoes => (pData.Shoes, pData.ShoesCount),
				EquipmentType.Gloves => (pData.Gloves, pData.GlovesCount),
				EquipmentType.Jewelry => (pData.Jewelry, pData.JewelryCount),
				_ => (0, 0),
			};
			if (resultID == 0) equipmentCount = 0;
			return resultID;
		} else {
			equipmentCount = 0;
			return 0;
		}
	}


	public static bool SetEquipment (int inventoryID, EquipmentType type, int equipmentID, int equipmentCount) {

		if (
			!Pool.TryGetValue(inventoryID, out var data) ||
			data is not EquipmentInventoryData pData
		) return false;

		if (
			equipmentID != 0 &&
			(!ItemSystem.IsEquipment(equipmentID, out var newEquipmentType) || newEquipmentType != type)
		) return false;

		if (equipmentID == 0) equipmentCount = 0;

		switch (type) {
			case EquipmentType.HandTool:
				pData.HandTool = equipmentID;
				pData.HandToolCount = equipmentCount;
				break;
			case EquipmentType.BodyArmor:
				pData.BodySuit = equipmentID;
				pData.BodySuitCount = equipmentCount;
				break;
			case EquipmentType.Helmet:
				pData.Helmet = equipmentID;
				pData.HelmetCount = equipmentCount;
				break;
			case EquipmentType.Shoes:
				pData.Shoes = equipmentID;
				pData.ShoesCount = equipmentCount;
				break;
			case EquipmentType.Gloves:
				pData.Gloves = equipmentID;
				pData.GlovesCount = equipmentCount;
				break;
			case EquipmentType.Jewelry:
				pData.Jewelry = equipmentID;
				pData.JewelryCount = equipmentCount;
				break;
		}

		data.IsDirty = true;
		IsPoolDirty = true;
		return true;
	}


	public static void ReduceEquipmentCount (int inventoryID, int delta, EquipmentType type) {
		int eqID = GetEquipment(inventoryID, type, out int eqCount);
		if (eqID == 0) return;
		int newEqCount = (eqCount - delta).GreaterOrEquelThanZero();
		if (newEqCount == 0) eqID = 0;
		SetEquipment(inventoryID, type, eqID, newEqCount);
	}


	public static void FillEquipmentFromInventory (int inventoryID, EquipmentType type) {
		if (!Pool.TryGetValue(inventoryID, out var data) || data is not EquipmentInventoryData pData) return;
		int itemID = GetEquipment(inventoryID, type, out int count);
		if (itemID == 0) return;
		int maxStack = ItemSystem.GetItemMaxStackCount(itemID);
		FillEquipmentFromInventoryLogic(pData, type, itemID, count, maxStack);
	}


	#endregion




	#region --- LGC ---


	private static void LoadInventoryPoolFromDisk () {
		IsPoolDirty = false;
		Pool.Clear();
		string root = Universe.BuiltIn.SlotInventoryRoot;
		if (!Util.FolderExists(root)) return;
		foreach (var path in Util.EnumerateFiles(root, true, AngePath.INVENTORY_SEARCH_PATTERN, AngePath.EQ_INVENTORY_SEARCH_PATTERN)) {
			try {
				string name = Util.GetNameWithoutExtension(path);
				int id = name.AngeHash();
				if (Pool.ContainsKey(id)) continue;
				InventoryData data;
				if (path.EndsWith(AngePath.INVENTORY_FILE_EXT)) {
					data = JsonUtil.LoadOrCreateJsonFromPath<InventoryData>(path);
				} else {
					data = JsonUtil.LoadOrCreateJsonFromPath<EquipmentInventoryData>(path);
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
		string root = Universe.BuiltIn.SlotInventoryRoot;
		foreach (var (_, data) in Pool) {
			if (!forceSave && !data.IsDirty) continue;
			data.IsDirty = false;
			// Save Inventory
			string path = Util.CombinePaths(root, $"{data.Name}.{(data is EquipmentInventoryData ? AngePath.EQ_INVENTORY_FILE_EXT : AngePath.INVENTORY_FILE_EXT)}");
			JsonUtil.SaveJsonToPath(data, path, false);
		}
	}


	private static void FillEquipmentFromInventoryLogic (EquipmentInventoryData data, EquipmentType type, int itemID, int currentCount, int maxStack) {
		if (currentCount >= maxStack) return;
		int len = data.Items.Length;
		bool changed = false;
		for (int i = 0; i < len; i++) {
			int item = data.Items[i];
			if (item != itemID) continue;
			int count = data.Counts[i];
			if (count <= 0) continue;
			int deltaCount = Util.Min(count, maxStack - currentCount);
			count -= deltaCount;
			if (count <= 0) {
				data.Items[i] = 0;
			}
			data.Counts[i] = count;
			changed = true;
			currentCount += deltaCount;
			if (currentCount >= maxStack) break;
		}
		if (changed) {
			data.IsDirty = true;
			IsPoolDirty = true;
			switch (type) {
				case EquipmentType.HandTool:
					data.HandToolCount = currentCount;
					break;
				case EquipmentType.BodyArmor:
					data.BodySuitCount = currentCount;
					break;
				case EquipmentType.Helmet:
					data.HelmetCount = currentCount;
					break;
				case EquipmentType.Shoes:
					data.ShoesCount = currentCount;
					break;
				case EquipmentType.Gloves:
					data.GlovesCount = currentCount;
					break;
				case EquipmentType.Jewelry:
					data.JewelryCount = currentCount;
					break;
			}
		}
	}


	#endregion




}