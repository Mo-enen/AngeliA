using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;


namespace AngeliA;

/// <summary>
/// Core system to handle storage of items
/// </summary>
public static class Inventory {




	#region --- SUB ---


	private class InventoryData : IJsonSerializationCallback {

		public int[] Items;
		public int[] Counts;
		[JsonIgnore] public string BasicName;
		[JsonIgnore] public string Path;
		[JsonIgnore] public bool IsDirty;
		[JsonIgnore] public Int3 MapUnitPosition;

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
	/// <summary>
	/// True if the system is read to use
	/// </summary>
	public static bool PoolReady { get; private set; } = false;

	// Data
	private static readonly Dictionary<int, InventoryData> Pool = [];
	private static readonly Dictionary<Int4, int> BasicIdToInvIdPool = [];
	private static bool IsPoolDirty = false;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		PoolReady = false;
		LoadInventoryPoolFromDisk(false);
		PoolReady = true;
	}


	[OnGameInitializeLater]
	internal static TaskResult OnGameInitializeLater () {

		if (!ItemSystem.ItemPoolReady) return TaskResult.Continue;
		if (!Cloth.ClothSystemReady) return TaskResult.Continue;

		// Init Cheat Code
		var giveItemCheatInfo = typeof(Inventory).GetMethod(
			nameof(GiveItemCheat),
			BindingFlags.NonPublic | BindingFlags.Static
		);

		// Cheat from Code
		var BLOCK_ITEM = typeof(BlockBuilder);
		var CLOTH_ITEM = typeof(ClothItem);
		var BG_ITEM = typeof(BodyGadgetItem);
		var BS_ITEM = typeof(BodySetItem);
		foreach (var type in typeof(Item).AllChildClass()) {
			if (type == BLOCK_ITEM || type == CLOTH_ITEM || type == BG_ITEM || type == BS_ITEM) continue;
			string angeName = type.AngeName();
			int id = angeName.AngeHash();
			CheatSystem.TryAddCheatAction($"Give{angeName}", giveItemCheatInfo, id);
		}

		// Cheat from Block Entity
		foreach (var type in typeof(IBlockEntity).AllClassImplemented()) {
			string angeName = type.AngeName();
			int id = angeName.AngeHash();
			CheatSystem.TryAddCheatAction($"Give{angeName}", giveItemCheatInfo, id);
		}

		// Cheat from Cloth Entity
		foreach (var (id, cloth) in Cloth.ForAllCloth()) {
			CheatSystem.TryAddCheatAction($"Give{cloth.ClothName}", giveItemCheatInfo, id);
		}

		// Cheat from Body Gadget Entity
		foreach (var (id, gadget) in BodyGadget.ForAllGadget()) {
			CheatSystem.TryAddCheatAction($"Give{gadget.GadgetName}", giveItemCheatInfo, id);
		}

		// Cheat from BodySet Entity
		foreach (var (id, (_, name)) in BodySetItem.ForAllBodySetCharacterType()) {
			CheatSystem.TryAddCheatAction($"Give{name}.BodySet", giveItemCheatInfo, id);
		}

		return TaskResult.End;
	}


	[BeforeSavingSlotChanged]
	internal static void BeforeSavingSlotChanged () => PoolReady = false;


	[OnSavingSlotChanged]
	internal static void OnSavingSlotChanged () {
		PoolReady = false;
		LoadInventoryPoolFromDisk(true);
		PoolReady = true;
	}


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
	/// <inheritdoc cref="InitializeInventoryData(int, string, int, bool)"/>
	public static int InitializeInventoryData (string basicName, int capacity, bool hasEquipment = false) => InitializeInventoryData(basicName.AngeHash(), basicName, capacity, new Int3(int.MinValue, int.MinValue, int.MinValue), hasEquipment);
	/// <inheritdoc cref="InitializeInventoryData(int, string, int, bool)"/>
	public static int InitializeInventoryData (string basicName, int capacity, Int3 mapUnitPos, bool hasEquipment = false) => InitializeInventoryData(basicName.AngeHash(), basicName, capacity, mapUnitPos, hasEquipment);
	/// <inheritdoc cref="InitializeInventoryData(int, string, int, bool)"/>
	public static int InitializeInventoryData (int basicID, string basicName, int capacity, bool hasEquipment = false) => InitializeInventoryData(basicID, basicName, capacity, new Int3(int.MinValue, int.MinValue, int.MinValue), hasEquipment);
	/// <summary>
	/// Initialize an inventory data for the system
	/// </summary>
	/// <param name="basicID">ID of the holder</param>
	/// <param name="basicName">Name of the holder</param>
	/// <param name="capacity">Maximal item count the inventory can hold</param>
	/// <param name="mapUnitPos">Original position in unit space for the map-type inventory. (int.MinValue, int.MinValue, int.MinValue) for other type inventory.</param>
	/// <param name="hasEquipment">True if this inventory requires equipment part.</param>
	/// <returns>Inventory ID</returns>
	public static int InitializeInventoryData (int basicID, string basicName, int capacity, Int3 mapUnitPos, bool hasEquipment = false) {
		int invID = GetInventoryIdFromBasicIdAndPos(basicID, basicName, mapUnitPos);
		if (HasInventory(invID)) {
			if (GetInventoryCapacity(invID) != capacity) {
				ResizeInventory(invID, capacity);
			}
		} else {
			if (hasEquipment) {
				AddNewInventoryData<EquipmentInventoryData>(basicName, capacity, mapUnitPos);
			} else {
				AddNewInventoryData<InventoryData>(basicName, capacity, mapUnitPos);
			}
		}
		return invID;
	}


	/// <summary>
	/// Calculate inventory ID based of the given information
	/// </summary>
	/// <param name="basicID">ID of the holder</param>
	/// <param name="baseName">Name of the holder</param>
	/// <param name="mapPos">Original position in unit space for the map-type inventory. (int.MinValue, int.MinValue, int.MinValue) for other type inventory.</param>
	public static int GetInventoryIdFromBasicIdAndPos (int basicID, string baseName, Int3 mapPos) {
		var key = new Int4(mapPos.x, mapPos.y, mapPos.z, basicID);
		if (!BasicIdToInvIdPool.TryGetValue(key, out int id)) {
			id = GetPositionBasedInventoryName(baseName, mapPos).AngeHash();
			BasicIdToInvIdPool.Add(key, id);
			return id;
		}
		return id;
	}


	/// <summary>
	/// Calculate the original holder position from the given inventory file name
	/// </summary>
	public static Int3 GetInventoryMapPosFromName (string invNameWithoutExt, out string basicName) {
		basicName = invNameWithoutExt;
		var def = new Int3(int.MinValue, int.MinValue, int.MinValue);
		int dot0 = invNameWithoutExt.IndexOf('.');
		if (dot0 < 0) return def;
		basicName = invNameWithoutExt[..dot0];
		int dot1 = invNameWithoutExt.IndexOf('.', dot0 + 1);
		if (dot1 < 0) return def;
		int dot2 = invNameWithoutExt.IndexOf('.', dot1 + 1);
		if (dot2 < 0) return def;
		if (
			int.TryParse(invNameWithoutExt[(dot0 + 1)..dot1], out int x) &&
			int.TryParse(invNameWithoutExt[(dot1 + 1)..dot2], out int y) &&
			int.TryParse(invNameWithoutExt[(dot2 + 1)..], out int z)
		) {
			return new Int3(x, y, z);
		}
		return def;
	}


	/// <summary>
	/// Set capacity of the inventory without changing the content. (items will be clip out if the capacity is not enough to hold)
	/// </summary>
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


	/// <summary>
	/// True if the given ID refers to a valid inventory
	/// </summary>
	public static bool HasInventory (int inventoryID) => Pool.ContainsKey(inventoryID);


	/// <summary>
	/// Get the item count limit for target inventory
	/// </summary>
	public static int GetInventoryCapacity (int inventoryID) => Pool.TryGetValue(inventoryID, out var data) ? data.Items.Length : 0;


	/// <summary>
	/// Reset the map position for the inventory into a new value
	/// </summary>
	public static void RepositionInventory (int inventoryID, Int3 newMapUnitPosition) {

		lock (Pool) {

			if (
				!Pool.TryGetValue(inventoryID, out var invData) ||
				invData.MapUnitPosition == newMapUnitPosition
			) return;

			string oldPath = invData.Path;
			string ext = invData is EquipmentInventoryData ? AngePath.EQ_INVENTORY_FILE_EXT : AngePath.INVENTORY_FILE_EXT;
			string newName = GetPositionBasedInventoryName(invData.BasicName, newMapUnitPosition);
			string newPath = Util.CombinePaths(Universe.BuiltIn.SlotInventoryRoot, $"{newName}.{ext}");
			int newID = newName.AngeHash();

			// Change Data in Pool
			if (Pool.Remove(inventoryID)) {
				Pool[newID] = invData;
			}
			invData.Path = newPath;
			invData.MapUnitPosition = newMapUnitPosition;

			// Move File
			Util.MoveFile(oldPath, newPath);
		}
	}


	/// <summary>
	/// Unlock all items inside given inventory for player
	/// </summary>
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
	/// <inheritdoc cref="GetItemAt(int, int, out int)"/>
	public static int GetItemAt (int inventoryID, int itemIndex) => GetItemAt(inventoryID, itemIndex, out _);
	/// <summary>
	/// Get ID of the item inside given inventory
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="itemIndex">Index of the item</param>
	/// <param name="count">Count of the item</param>
	public static int GetItemAt (int inventoryID, int itemIndex, out int count) {
		if (Pool.TryGetValue(inventoryID, out var data)) {
			count = itemIndex >= 0 && itemIndex < data.Counts.Length ? data.Counts[itemIndex] : 0;
			return itemIndex >= 0 && itemIndex < data.Items.Length ? data.Items[itemIndex] : 0;
		} else {
			count = 0;
			return 0;
		}
	}


	/// <summary>
	/// Get count of the given item
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="itemIndex">Index of the item</param>
	/// <returns>Count of the item</returns>
	public static int GetItemCount (int inventoryID, int itemIndex) => Pool.TryGetValue(inventoryID, out var data) && itemIndex >= 0 && itemIndex < data.Counts.Length && data.Items[itemIndex] != 0 ? data.Counts[itemIndex] : 0;


	/// <summary>
	/// Set item ID at index of the given inventory
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="itemIndex">Index of the item</param>
	/// <param name="newItem">ID of the item</param>
	/// <param name="newCount">Count of the item</param>
	public static void SetItemAt (int inventoryID, int itemIndex, int newItem, int newCount) {
		lock (Pool) {
			if (!Pool.TryGetValue(inventoryID, out var data) || itemIndex < 0 || itemIndex >= data.Items.Length) return;
			data.Items[itemIndex] = newCount > 0 ? newItem : 0;
			data.Counts[itemIndex] = newCount;
			data.IsDirty = true;
			IsPoolDirty = true;
		}
	}


	/// <summary>
	/// Find index of the target item inside given inventory
	/// </summary>
	/// <returns>Index of the item. -1 when not found.</returns>
	public static int IndexOfItem (int inventoryID, int itemID) {
		if (!Pool.TryGetValue(inventoryID, out var data)) return -1;
		for (int i = 0; i < data.Items.Length; i++) {
			if (data.Items[i] == itemID) return i;
		}
		return -1;
	}


	/// <summary>
	/// True if the given item founded in the target inventory.
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="itemID"></param>
	/// <param name="includeEquipment">True if search inside equipment part</param>
	public static bool HasItem (int inventoryID, int itemID, bool includeEquipment = true) {
		if (!Pool.TryGetValue(inventoryID, out var data)) return false;
		for (int i = 0; i < data.Items.Length; i++) {
			if (data.Items[i] == itemID) return true;
		}
		if (includeEquipment && data is EquipmentInventoryData eqData) {
			return
				(eqData.HandTool == itemID && eqData.HandToolCount > 0) ||
				(eqData.Helmet == itemID && eqData.HelmetCount > 0) ||
				(eqData.Shoes == itemID && eqData.ShoesCount > 0) ||
				(eqData.BodySuit == itemID && eqData.BodySuitCount > 0) ||
				(eqData.Gloves == itemID && eqData.GlovesCount > 0) ||
				(eqData.Jewelry == itemID && eqData.JewelryCount > 0);
		}
		return false;
	}


	/// <summary>
	/// Add "count" to the given item
	/// </summary>
	/// <returns>How many items has been added. 0 means no item added. "count" means all items added.</returns>
	public static int AddItemAt (int inventoryID, int itemIndex, int count = 1) {
		lock (Pool) {
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
	}

	/// <summary>
	/// Find given item and add "count" to the item
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="targetItemID"></param>
	/// <param name="count"></param>
	/// <param name="ignoreEquipment">True if do not search for equipment part</param>
	/// <returns>How many items has been added. 0 means no item added. "count" means all items added.</returns>
	public static int FindAndAddItem (int inventoryID, int targetItemID, int count = 1, bool ignoreEquipment = true) {
		lock (Pool) {
			if (targetItemID == 0 || count <= 0 || !Pool.TryGetValue(inventoryID, out var data)) return 0;
			int oldCount = count;
			// Equipment
			if (!ignoreEquipment) {
				int collectedCount = CollectItem(inventoryID, targetItemID, count, false, true);
				count -= collectedCount;
				if (count <= 0) {
					return oldCount - count;
				}
			}
			// Inventory
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
	}


	/// <summary>
	/// Take "count" items at given index for target inventory
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="itemIndex"></param>
	/// <param name="count"></param>
	/// <returns>How many items has been taken. 0 means no item taken. "count" means all items taken.</returns>
	public static int TakeItemAt (int inventoryID, int itemIndex, int count = 1) {
		lock (Pool) {
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
	}


	/// <summary>
	/// Take "count" items with "targetItemID" from given inventory
	/// </summary>
	/// <returns>How many items has been taken. 0 means no item taken. "count" means all items taken.</returns>
	public static int FindAndTakeItem (int inventoryID, int targetItemID, int count = 1) {
		lock (Pool) {
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
	}


	/// <inheritdoc cref="CollectItem(int, int, out int, int, bool, bool, bool)"/>
	public static int CollectItem (int inventoryID, int item, int count = 1, bool ignoreEquipment = true, bool ignoreInventory = false, bool dontCollectIntoEmptyEquipmentSlot = false) => CollectItem(inventoryID, item, out _, count, ignoreEquipment, ignoreInventory, dontCollectIntoEmptyEquipmentSlot);
	/// <summary>
	/// Add item into given inventory to first free slot founded.
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="item">ID of the item</param>
	/// <param name="count">Count of the item</param>
	/// <param name="ignoreEquipment">True if do not add into equipment part</param>
	/// <param name="ignoreInventory">True if do not add into inventory part (non-equipment part)</param>
	/// <param name="dontCollectIntoEmptyEquipmentSlot">True if do not add item into empty slot of equipment part</param>
	/// <param name="collectIndex">Index of which slot collect the item. -1 if not collected.</param>
	/// <returns>How many items has been collected. 0 means no item collected. "count" means all items collected.</returns>
	public static int CollectItem (int inventoryID, int item, out int collectIndex, int count = 1, bool ignoreEquipment = true, bool ignoreInventory = false, bool dontCollectIntoEmptyEquipmentSlot = false) {
		lock (Pool) {
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
						bool emptyH = !dontCollectIntoEmptyEquipmentSlot && eData.HandTool == 0;
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
							eData.BodySuit = item;
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
							eData.Helmet = item;
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
							eData.Shoes = item;
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
							eData.Gloves = item;
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
							eData.Jewelry = item;
							eData.JewelryCount += delta;
							if (emptyJ) {
								FillEquipmentFromInventoryLogic(eData, eqType, item, eData.JewelryCount, maxStackCount);
							}
						}
						if (count <= 0) return oldCount - count;
						break;

				}

			}

			if (!ignoreInventory && count > 0) {

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

			return oldCount - count;
		}
	}


	/// <summary>
	/// True if the inventory have enough room to contain all the target items
	/// </summary>
	public static bool HasEnoughRoomToCollect (int inventoryID, int item, int count) {

		if (item == 0 || count <= 0 || !Pool.TryGetValue(inventoryID, out var data)) return false;
		int maxStackCount = ItemSystem.GetItemMaxStackCount(item);

		if (count > 0) {

			// Try Append to Exists
			for (int i = 0; i < data.Items.Length; i++) {
				int _item = data.Items[i];
				if (_item != item) continue;
				int _count = data.Counts[i];
				if (_count < maxStackCount) {
					// Append Item
					int delta = Util.Min(count, maxStackCount - _count);
					count -= delta;
				}
				if (count <= 0) break;
			}
			if (count <= 0) return true;

			// Try Add to New Slot
			for (int i = 0; i < data.Items.Length; i++) {
				int _item = data.Items[i];
				if (_item != 0) continue;
				int delta = Util.Min(count, maxStackCount);
				count -= delta;
				if (count <= 0) break;
			}

		}

		return count <= 0;
	}


	/// <summary>
	/// How many target items are currently inside the inventory
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="itemID"></param>
	/// <param name="ignoreStack">True if stacked items count as one</param>
	public static int ItemTotalCount (int inventoryID, int itemID, bool ignoreStack = false) => ItemTotalCount(inventoryID, itemID, -1, out _, ignoreStack);


	/// <summary>
	/// How many target items are currently inside the inventory
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="itemID"></param>
	/// <param name="targetIndex">Index of the special target</param>
	/// <param name="targetOrder">Order of the special target between all items inside the inventory with same ID</param>
	/// <param name="ignoreStack">True if stacked items count as one</param>
	public static int ItemTotalCount (int inventoryID, int itemID, int targetIndex, out int targetOrder, bool ignoreStack = false) {
		int result = 0;
		int order = 0;
		targetOrder = -1;
		if (Pool.TryGetValue(inventoryID, out var data)) {
			int len = Util.Min(data.Items.Length, data.Counts.Length);
			for (int i = 0; i < len; i++) {
				if (data.Items[i] != itemID) continue;
				result += ignoreStack ? 1 : data.Counts[i];
				if (targetIndex == i) {
					targetOrder = order;
				}
				order++;
			}
		}
		return result;
	}


	/// <summary>
	/// How many target items are currently inside the inventory
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="ignoreStack">True if stacked items count as one</param>
	public static int ItemTotalCount<I> (int inventoryID, bool ignoreStack = false) where I : Item => ItemTotalCount<I>(inventoryID, -1, out _, ignoreStack);


	/// <summary>
	/// How many target items are currently inside the inventory
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="targetIndex">Index of the special target</param>
	/// <param name="targetOrder">Order of the special target between all items inside the inventory with same ID</param>
	/// <param name="ignoreStack">True if stacked items count as one</param>
	public static int ItemTotalCount<I> (int inventoryID, int targetIndex, out int targetOrder, bool ignoreStack = false) where I : Item {
		int result = 0;
		int order = 0;
		targetOrder = -1;
		if (Pool.TryGetValue(inventoryID, out var data)) {
			int len = Util.Min(data.Items.Length, data.Counts.Length);
			for (int i = 0; i < len; i++) {
				int id = data.Items[i];
				if (id == 0) continue;
				if (ItemSystem.GetItem(id) is not I) continue;
				result += ignoreStack ? 1 : data.Counts[i];
				if (targetIndex == i) {
					targetOrder = order;
				}
				order++;
			}
		}
		return result;
	}


	// Give
	/// <summary>
	/// Add target item to inventory of target entity
	/// </summary>
	/// <param name="target"></param>
	/// <param name="itemID"></param>
	/// <param name="count"></param>
	/// <param name="spawnWhenInventoryFull">True if spawn an ItemHolder entity to hold the item</param>
	/// <returns>True if the item is given</returns>
	public static bool GiveItemToTarget (Entity target, int itemID, int count = 1, bool spawnWhenInventoryFull = true) {
		lock (Pool) {
			if (target == null) {
				return
					spawnWhenInventoryFull &&
					ItemSystem.SpawnItem(itemID, Renderer.CameraRect.CenterX(), Renderer.CameraRect.CenterY(), count) != null;
			} else {
				count -= CollectItem(target is Character cTarget ? cTarget.InventoryID : target.TypeID, itemID, count, ignoreEquipment: false);
				return count <= 0 || (spawnWhenInventoryFull && ItemSystem.SpawnItem(itemID, target.Rect.x - Const.CEL, target.Y, count) != null);
			}
		}
	}


	internal static void GiveItemCheat () {
		lock (Pool) {
			var player = PlayerSystem.Selecting;
			if (player == null) return;
			if (CheatSystem.CurrentParam is not int id) return;
			ItemSystem.SetItemUnlocked(id, true);
			int maxCount = ItemSystem.GetItemMaxStackCount(id);
			GiveItemToTarget(player, id, maxCount);
		}
	}


	// Equipment
	/// <summary>
	/// Get ID of equipment for given inventory
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="type">Type of the equipment</param>
	/// <param name="equipmentCount">Stack count of the equipment</param>
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


	/// <summary>
	/// Set ID of equipment for given inventory
	/// </summary>
	/// <param name="inventoryID"></param>
	/// <param name="type">Type of the equipment</param>
	/// <param name="equipmentID"></param>
	/// <param name="equipmentCount">Stack count of the equipment</param>
	/// <returns>True if the value successfuly setted</returns>
	public static bool SetEquipment (int inventoryID, EquipmentType type, int equipmentID, int equipmentCount) {
		lock (Pool) {
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
	}


	/// <summary>
	/// Remove equipment count by "delta" (set delta to 1 means remove 1)
	/// </summary>
	public static void ReduceEquipmentCount (int inventoryID, int delta, EquipmentType type) {
		lock (Pool) {
			int eqID = GetEquipment(inventoryID, type, out int eqCount);
			if (eqID == 0) return;
			int newEqCount = (eqCount - delta).GreaterOrEquelThanZero();
			if (newEqCount == 0) eqID = 0;
			SetEquipment(inventoryID, type, eqID, newEqCount);
		}
	}


	/// <summary>
	/// Make items with same ID inside inventory part stack onto equipment part
	/// </summary>
	public static void FillEquipmentFromInventory (int inventoryID, EquipmentType type) {
		lock (Pool) {
			if (!Pool.TryGetValue(inventoryID, out var data) || data is not EquipmentInventoryData pData) return;
			int itemID = GetEquipment(inventoryID, type, out int count);
			if (itemID == 0) return;
			int maxStack = ItemSystem.GetItemMaxStackCount(itemID);
			FillEquipmentFromInventoryLogic(pData, type, itemID, count, maxStack);
		}
	}


	#endregion




	#region --- LGC ---


	private static void LoadInventoryPoolFromDisk (bool keepOriginalCapacity) {
		lock (Pool) {
			IsPoolDirty = false;
			if (!keepOriginalCapacity) {
				Pool.Clear();
			} else {
				foreach (var (_, data) in Pool) {
					System.Array.Clear(data.Items);
					System.Array.Clear(data.Counts);
					if (data is EquipmentInventoryData eData) {
						eData.HandTool = 0;
						eData.Helmet = 0;
						eData.BodySuit = 0;
						eData.Shoes = 0;
						eData.Gloves = 0;
						eData.Jewelry = 0;
						eData.HandToolCount = 0;
						eData.HelmetCount = 0;
						eData.BodySuitCount = 0;
						eData.ShoesCount = 0;
						eData.GlovesCount = 0;
						eData.JewelryCount = 0;
					}
				}
			}
			string root = Universe.BuiltIn.SlotInventoryRoot;
			if (!Util.FolderExists(root)) return;
			foreach (var path in Util.EnumerateFiles(
				root, true, AngePath.INVENTORY_SEARCH_PATTERN, AngePath.EQ_INVENTORY_SEARCH_PATTERN
			)) {
				try {
					string name = Util.GetNameWithoutExtension(path);
					int id = name.AngeHash();
					if (!keepOriginalCapacity && Pool.ContainsKey(id)) continue;
					InventoryData data;
					if (path.EndsWith(AngePath.EQ_INVENTORY_FILE_EXT)) {
						data = JsonUtil.LoadOrCreateJsonFromPath<EquipmentInventoryData>(path);
					} else {
						data = JsonUtil.LoadOrCreateJsonFromPath<InventoryData>(path);
					}
					if (data == null) continue;
					data.IsDirty = false;
					data.Path = path;
					data.MapUnitPosition = GetInventoryMapPosFromName(name, out string basicName);
					data.BasicName = basicName;
					Pool[id] = data;
					// Valid Item Count
					for (int i = 0; i < data.Items.Length; i++) {
						int iCount = data.Counts[i];
						if (iCount <= 0 || data.Items[i] == 0) {
							data.Counts[i] = 0;
							data.Items[i] = 0;
						}
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
		}
	}


	private static void SaveAllToDisk (bool forceSave) {
		lock (Pool) {
			IsPoolDirty = false;
			string root = Universe.BuiltIn.SlotInventoryRoot;
			foreach (var (_, data) in Pool) {
				if (!forceSave && !data.IsDirty) continue;
				data.IsDirty = false;
				// Save Inventory
				JsonUtil.SaveJsonToPath(data, data.Path, false);
			}
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


	private static void AddNewInventoryData<T> (string basicName, int capacity, Int3 mapUnitPos) where T : InventoryData, new() {
		if (capacity <= 0) return;
		int inventoryID = GetPositionBasedInventoryName(basicName, mapUnitPos).AngeHash();
		if (Pool.ContainsKey(inventoryID)) return;
		var data = new T() {
			Items = new int[capacity],
			Counts = new int[capacity],
			IsDirty = true,
			MapUnitPosition = mapUnitPos,
			BasicName = basicName,
		};
		string root = Universe.BuiltIn.SlotInventoryRoot;
		string ext = data is EquipmentInventoryData ? AngePath.EQ_INVENTORY_FILE_EXT : AngePath.INVENTORY_FILE_EXT;
		data.Path = Util.CombinePaths(root, $"{GetPositionBasedInventoryName(basicName, mapUnitPos)}.{ext}");
		Pool.Add(inventoryID, data);
		IsPoolDirty = true;
	}


	private static string GetPositionBasedInventoryName (string baseName, Int3 unitPosition) => $"{baseName}.{unitPosition.x}.{unitPosition.y}.{unitPosition.z}";


	#endregion




}