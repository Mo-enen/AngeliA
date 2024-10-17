using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace AngeliA;

public static class ItemSystem {




	#region --- SUB ---


	public class ItemData (Item item, int nameID, int descriptionID, string typeName, int maxStackCount) {
		public Item Item = item;
		public int NameID = nameID;
		public int DescriptionID = descriptionID;
		public string TypeName = typeName;
		public int MaxStackCount = maxStackCount;
		public bool Unlocked = false;
	}


	private struct ItemDropData {
		public int ItemID;
		public int Count;
		public int Chance;
	}


	private class CombinationData {
		public int Result;
		public int ResultCount;
		public int Keep0;
		public int Keep1;
		public int Keep2;
		public int Keep3;
		public bool Keep (int id) => Keep0 == id || Keep1 == id || Keep2 == id || Keep3 == id;
	}


	#endregion




	#region --- VAR ---


	// Const
	private const string UNLOCK_NAME = "UnlockedItem";

	// Api
	public static bool ItemPoolReady { get; private set; } = false;
	public static bool ItemUnlockReady { get; private set; } = false;

	// Data
	private static readonly Dictionary<Int4, CombinationData> CombinationPool = [];
	private static readonly Dictionary<int, ItemDropData> ItemDropPool = [];
	private static readonly Dictionary<int, ItemData> ItemPool = [];
	private static bool IsUnlockDirty = false;
	private static bool BlockItemLoadedBefore = false;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static void OnGameInitialize () {

		if (Game.IsToolApplication) return;

		// Init Item Pool from Code
		var BLOCK_ITEM = typeof(BlockBuilder);
		foreach (var type in typeof(Item).AllChildClass()) {
			if (type == BLOCK_ITEM) continue;
			if (System.Activator.CreateInstance(type) is not Item item) continue;
			string angeName = type.AngeName();
			int id = angeName.AngeHash();
			ItemPool.TryAdd(id, new ItemData(
				item,
				$"iName.{angeName}".AngeHash(),
				$"iDes.{angeName}".AngeHash(),
				angeName,
				item.MaxStackCount.GreaterOrEquel(1)
			));
		}

		// Add Block Entity
		foreach (var type in typeof(IBlockEntity).AllClassImplemented()) {
			string angeName = type.AngeName();
			int id = angeName.AngeHash();
			var blockItem = new BlockBuilder(id, angeName, BlockType.Entity);
			ItemPool.TryAdd(id, new ItemData(
				blockItem,
				$"iName.{angeName}".AngeHash(),
				$"iDes.{angeName}".AngeHash(),
				angeName,
				blockItem.MaxStackCount.GreaterOrEquel(1)
			));
		}

		ItemPoolReady = true;

		// Init Drop Pool from Code
		ItemDropPool.Clear();
		foreach (var (type, att) in Util.AllClassWithAttribute<ItemDropAttribute>()) {
			ItemDropPool.TryAdd(type.AngeHash(), new() {
				ItemID = att.ItemTypeID,
				Chance = att.DropChance,
				Count = att.DropCount,
			});
		}

		// Load Combination from Code
		LoadCombinationPoolFromCode(CombinationPool);

		// Load Unlock from File
		LoadUnlockDataFromFile();

		// Final
		ItemUnlockReady = true;
	}


	[BeforeSavingSlotChanged]
	internal static void BeforeSavingSlotChanged () => ItemUnlockReady = false;


	[OnSavingSlotChanged]
	internal static void OnSavingSlotChanged () {
		LoadUnlockDataFromFile();
		ItemUnlockReady = true;
	}


	[OnMainSheetReload]
	internal static void AddBlockItemsFromSheet () {

		var sheet = Renderer.MainSheet;
		if (sheet == null) return;

		// Clear Prev Block Items
		if (BlockItemLoadedBefore) {
			foreach (var (id, itemData) in ItemPool) {
				if (itemData.Item is BlockBuilder bItem && bItem.BlockType != BlockType.Entity) {
					ItemPool.Remove(id);
				}
			}
		}
		BlockItemLoadedBefore = true;

		// Add Block Items
		var span = sheet.Sprites.GetSpan();
		int len = span.Length;
		for (int i = 0; i < len; i++) {
			var sprite = span[i];
			if (sprite.Group != null && ItemPool.ContainsKey(sprite.Group.ID)) continue;
			var bType = sprite.Atlas.Type;
			if (bType == AtlasType.General) continue;
			int itemID = sprite.Group != null ? sprite.Group.ID : sprite.ID;
			string itemName = sprite.Group != null ? sprite.Group.Name : sprite.RealName;
			var blockItem = new BlockBuilder(
				itemID, itemName,
				bType == AtlasType.Level ? BlockType.Level : BlockType.Background
			);
			ItemPool.TryAdd(itemID, new ItemData(
				blockItem,
				$"iName.{itemName}".AngeHash(),
				$"iDes.{itemName}".AngeHash(),
				itemName,
				blockItem.MaxStackCount.GreaterOrEquel(1)
			));
		}
	}


	[OnGameUpdate]
	internal static void Update () {
		// Save Unlock Data
		if (IsUnlockDirty) {
			IsUnlockDirty = false;
			SaveUnlockDataToFile();
		}
	}


	#endregion




	#region --- API ---


	// Item
	public static Item GetItem (int id) => ItemPool.TryGetValue(id, out var item) ? item.Item : null;
	public static string GetItemDisplayName (int id) => ItemPool.TryGetValue(id, out var item) ? Language.Get(item.NameID, item.TypeName) : "";
	public static string GetItemDescription (int id) => ItemPool.TryGetValue(id, out var item) ? Language.Get(item.DescriptionID) : "";
	public static int GetItemMaxStackCount (int id) => ItemPool.TryGetValue(id, out var item) ? item.MaxStackCount : 0;


	public static bool HasItem (int id) => ItemPool.ContainsKey(id);


	public static bool CanUseItem (int id, Entity target) {
		var item = GetItem(id);
		return item != null && item.CanUse(target);
	}


	// Combination
	public static bool TryGetCombination (
		int item0, int item1, int item2, int item3,
		out int result, out int resultCount,
		out int keep0, out int keep1, out int keep2, out int keep3
	) => TryGetCombinationFromPool(
		CombinationPool, item0, item1, item2, item3, out result, out resultCount,
		out keep0, out keep1, out keep2, out keep3
	);


	public static void ClearCombination () => CombinationPool.Clear();


	public static void GetRelatedCombinations (Int4 combination, List<Int4> output) {
		if (combination.IsZero) return;
		bool includeResult = combination.Count(0) == 3;
		foreach (var (craft, result) in CombinationPool) {
			if (includeResult && combination.Contains(result.Result)) {
				output.Add(craft);
				continue;
			}
			var _craft = craft;
			if (combination.x != 0 && !_craft.Swap(combination.x, 0)) continue;
			if (combination.y != 0 && !_craft.Swap(combination.y, 0)) continue;
			if (combination.z != 0 && !_craft.Swap(combination.z, 0)) continue;
			if (combination.w != 0 && !_craft.Swap(combination.w, 0)) continue;
			output.Add(craft);
		}
	}


	public static Int4 GetSortedCombination (int a, int b, int c, int d) {

		// Sort for Zero
		if (a == 0 && b != 0) (a, b) = (b, a);
		if (b == 0 && c != 0) (b, c) = (c, b);
		if (c == 0 && d != 0) (c, d) = (d, c);
		if (a == 0 && b != 0) (a, b) = (b, a);
		if (b == 0 && c != 0) (b, c) = (c, b);
		if (a == 0 && b != 0) (a, b) = (b, a);

		// Sort for Size
		if (a != 0 && b != 0 && a > b) (a, b) = (b, a);
		if (b != 0 && c != 0 && b > c) (b, c) = (c, b);
		if (c != 0 && d != 0 && c > d) (c, d) = (d, c);
		if (a != 0 && b != 0 && a > b) (a, b) = (b, a);
		if (b != 0 && c != 0 && b > c) (b, c) = (c, b);
		if (a != 0 && b != 0 && a > b) (a, b) = (b, a);

		return new Int4(a, b, c, d);
	}


	// Equipment
	public static bool IsEquipment (int itemID) => ItemPool.TryGetValue(itemID, out var data) && data.Item is Equipment;
	public static bool IsEquipment (int itemID, out EquipmentType type) {
		if (ItemPool.TryGetValue(itemID, out var data) && data.Item is Equipment equip) {
			type = equip.EquipmentType;
			return true;
		}
		type = default;
		return false;
	}


	// Unlock
	public static bool IsItemUnlocked (int itemID) => ItemPool.TryGetValue(itemID, out var data) && data.Unlocked;


	public static void SetItemUnlocked (int itemID, bool unlocked) {
		if (!ItemPool.TryGetValue(itemID, out var data) || data.Unlocked == unlocked) return;
		data.Unlocked = unlocked;
		IsUnlockDirty = true;
	}


	// Spawn 
	public static ItemHolder SpawnItem (int itemID, int x, int y, int count = 1, bool jump = true) {
		if (!HasItem(itemID)) return null;
		if (Stage.SpawnEntity(ItemHolder.TYPE_ID, x, y) is not ItemHolder holder) return null;
		holder.ItemID = itemID;
		holder.ItemCount = count;
		if (jump) {
			holder.Jump();
		}
		return holder;
	}


	// Drop
	public static bool DropItemFor (Entity entity) => DropItemFor(entity.TypeID, entity.X, entity.Y);
	public static bool DropItemFor (int sourceID, int x, int y) {
		if (!ItemDropPool.TryGetValue(sourceID, out var data)) return false;
		if (data.Chance < 1000 && Util.QuickRandom(0, 1000) >= data.Chance) return false;
		var result = SpawnItem(data.ItemID, x, y, data.Count);
		return result != null && result.ItemID != 0 && result.ItemCount > 0;
	}


	#endregion




	#region --- LGC ---


	// Unlock
	private static void LoadUnlockDataFromFile () {
		string unlockPath = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, UNLOCK_NAME);
		if (!Util.FileExists(unlockPath)) return;
		foreach (var (_, data) in ItemPool) data.Unlocked = false;
		var bytes = Util.FileToBytes(unlockPath);
		for (int i = 0; i < bytes.Length - 3; i += 4) {
			int id =
				(bytes[i + 0]) |
				(bytes[i + 1] << 8) |
				(bytes[i + 2] << 16) |
				(bytes[i + 3] << 24);
			if (ItemPool.TryGetValue(id, out var data)) {
				data.Unlocked = true;
			}
		}
	}


	private static void SaveUnlockDataToFile () {
		string unlockPath = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, UNLOCK_NAME);
		Util.CreateFolder(Util.GetParentPath(unlockPath));
		var fs = new FileStream(unlockPath, FileMode.Create, FileAccess.Write);
		foreach (var (id, data) in ItemPool) {
			if (!data.Unlocked) continue;
			fs.WriteByte((byte)id);
			fs.WriteByte((byte)(id >> 8));
			fs.WriteByte((byte)(id >> 16));
			fs.WriteByte((byte)(id >> 24));
		}
		fs.Close();
		fs.Dispose();
	}


	// Combination
	private static void LoadCombinationPoolFromCode (Dictionary<Int4, CombinationData> pool) {
		pool.Clear();
		foreach (var type in typeof(Item).AllChildClass()) {
			var iComs = type.GetCustomAttributes<ItemCombinationAttribute>(false);
			if (iComs == null) continue;
			foreach (var com in iComs) {
				if (com.Count <= 0) continue;
				if (
					com.ItemA == null && com.ItemB == null &&
					com.ItemC == null && com.ItemD == null
				) continue;
				int idA = com.ItemA != null ? com.ItemA.AngeHash() : 0;
				int idB = com.ItemB != null ? com.ItemB.AngeHash() : 0;
				int idC = com.ItemC != null ? com.ItemC.AngeHash() : 0;
				int idD = com.ItemD != null ? com.ItemD.AngeHash() : 0;
				var key = GetSortedCombination(idA, idB, idC, idD);
				if (pool.ContainsKey(key)) {
#if DEBUG
					Debug.Log($"Item Combination Collistion: \"{type.Name}\" & \"{pool[key].Result}\"");
#endif
					continue;
				}
				pool.Add(key, new CombinationData() {
					Result = type.AngeHash(),
					ResultCount = com.Count,
					Keep0 = com.ConsumeA ? 0 : idA,
					Keep1 = com.ConsumeB ? 0 : idB,
					Keep2 = com.ConsumeC ? 0 : idC,
					Keep3 = com.ConsumeD ? 0 : idD,
				});
			}
		}
	}


	private static bool TryGetCombinationFromPool (
		Dictionary<Int4, CombinationData> pool, int item0, int item1, int item2, int item3,
		out int result, out int resultCount,
		out int keep0, out int keep1, out int keep2, out int keep3
	) {
		var from = GetSortedCombination(item0, item1, item2, item3);
		if (pool.TryGetValue(from, out var resultValue)) {
			result = resultValue.Result;
			resultCount = resultValue.ResultCount;
			keep0 = resultValue.Keep0;
			keep1 = resultValue.Keep1;
			keep2 = resultValue.Keep2;
			keep3 = resultValue.Keep3;
			return true;
		}
		result = 0;
		resultCount = 0;
		keep0 = keep1 = keep2 = keep3 = 0;
		return false;
	}


	#endregion




}