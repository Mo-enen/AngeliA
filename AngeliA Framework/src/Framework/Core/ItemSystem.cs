using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace AngeliA;

public static class ItemSystem {




	#region --- SUB ---


	public class ItemData {
		public Item Item;
		public int NameID;
		public int DescriptionID;
		public string TypeName;
		public int MaxStackCount;
		public bool Unlocked;
		public ItemData (Item item, int nameID, int descriptionID, string typeName, int maxStackCount) {
			Item = item;
			NameID = nameID;
			DescriptionID = descriptionID;
			TypeName = typeName;
			MaxStackCount = maxStackCount;
			Unlocked = false;
		}
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
	private static readonly int[] ITEM_TYPE_ICONS = {
		BuiltInSprite.ITEM_ICON_WEAPON,
		BuiltInSprite.ITEM_ICON_ARMOR,
		BuiltInSprite.ITEM_ICON_HELMET,
		BuiltInSprite.ITEM_ICON_SHOES,
		BuiltInSprite.ITEM_ICON_GLOVES,
		BuiltInSprite.ITEM_ICON_JEWELRY,
		BuiltInSprite.ITEM_ICON_FOOD,
		BuiltInSprite.ITEM_ICON_ITEM,
	};

	// Api
	public static bool ItemPoolReady { get; private set; } = false;
	public static bool ItemUnlockReady { get; private set; } = false;

	// Data
	private static readonly Dictionary<Int4, CombinationData> CombinationPool = new();
	private static readonly Dictionary<int, ItemDropData> ItemDropPool = new();
	private static readonly Dictionary<int, ItemData> ItemPool = new();
	private static bool IsUnlockDirty = false;
	private static bool BlockItemLoadedBefore = false;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static void OnGameInitialize () {

		if (Game.IsToolApplication) return;

		// Init Item Pool from Code
		var BLOCK_ITEM = typeof(BlockItem);
		foreach (var type in typeof(Item).AllChildClass()) {
			if (type == BLOCK_ITEM) continue;
			if (System.Activator.CreateInstance(type) is not Item item) continue;
			string angeName = type.AngeName();
			ItemPool.TryAdd(type.AngeHash(), new ItemData(
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
			var blockItem = new BlockItem(id, angeName, BlockType.Entity);
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


	[OnMainSheetReload]
	internal static void AddBlockItemsFromSheet () {

		var sheet = Renderer.MainSheet;
		if (sheet == null) return;

		// Clear Prev Block Items
		if (BlockItemLoadedBefore) {
			foreach (var (id, itemData) in ItemPool) {
				if (itemData.Item is BlockItem bItem && bItem.BlockType != BlockType.Entity) {
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
			var blockItem = new BlockItem(
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


	public static int GetItemTypeIcon (int itemID) {
		int typeIcon = ITEM_TYPE_ICONS[^1];
		if (IsEquipment(itemID, out var equipmentType)) {
			// Equipment
			typeIcon = ITEM_TYPE_ICONS[(int)equipmentType];
		} else if (IsFood(itemID)) {
			// Food
			typeIcon = ITEM_TYPE_ICONS[^2];
		}
		return typeIcon;
	}


	// UI
	public static void DrawItemShortInfo (int itemID, IRect panelRect, int z, int armorIcon, int armorEmptyIcon, Color32 tint) {

		if (!ItemPool.TryGetValue(itemID, out var itemData)) return;
		var item = itemData.Item;

		// Equipment
		if (item is Equipment equipment) {
			switch (equipment.EquipmentType) {
				case EquipmentType.Weapon:
					break;
				case EquipmentType.Jewelry:
					break;
				case EquipmentType.BodyArmor:
				case EquipmentType.Helmet:
				case EquipmentType.Shoes:
				case EquipmentType.Gloves:
					if (equipment is IProgressiveItem progItem) {
						int progress = progItem.Progress;
						int totalProgress = progItem.TotalProgress;
						var rect = new IRect(panelRect.x, panelRect.y, panelRect.height, panelRect.height);
						for (int i = 0; i < totalProgress - 1; i++) {
							Renderer.Draw(i < progress ? armorIcon : armorEmptyIcon, rect, tint, z);
							rect.x += rect.width;
						}
					}
					break;
			}
		}


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


	// Food
	public static bool IsFood (int itemID) => ItemPool.TryGetValue(itemID, out var data) && data.Item is Food;


	// Unlock
	public static bool IsItemUnlocked (int itemID) => ItemPool.TryGetValue(itemID, out var data) && data.Unlocked;


	public static void SetItemUnlocked (int itemID, bool unlocked) {
		if (!ItemPool.TryGetValue(itemID, out var data) || data.Unlocked == unlocked) return;
		data.Unlocked = unlocked;
		IsUnlockDirty = true;
	}


	// Spawn 
	public static bool GiveItemTo (int inventoryID, int itemID, int count = 1) {
		count -= Inventory.CollectItem(inventoryID, itemID, count);
		if (count > 0) {
			var cameraRect = Renderer.CameraRect;
			SpawnItem(itemID, cameraRect.CenterX(), cameraRect.CenterY(), count);
		}
		return true;
	}


	public static void SpawnItemAtTarget (Entity target, int itemID, int count = 1) {
		int x = target != null ? target.Rect.x - Const.CEL : Renderer.CameraRect.CenterX();
		int y = target != null ? target.Y : Renderer.CameraRect.CenterY();
		SpawnItem(itemID, x, y, count);
	}


	public static ItemHolder SpawnItem (int itemID, int x, int y, int count = 1, bool jump = true) {
		if (Stage.SpawnEntity(ItemHolder.TYPE_ID, x, y) is not ItemHolder holder) return null;
		holder.ItemID = itemID;
		holder.ItemCount = count;
		if (jump) {
			holder.Jump();
		}
		return holder;
	}


	public static void SpawnItemFromMap (int unitX, int unitY, int z, IBlockSquad squad = null) {
		squad ??= WorldSquad.Front;
		for (int y = 1; y < 256; y++) {
			int currentUnitY = unitY - y;
			int right = -1;
			for (int x = 0; x < 256; x++) {
				int id = squad.GetBlockAt(unitX + x, currentUnitY, z, BlockType.Element);
				if (id == 0 || !HasItem(id)) break;
				right = x;
			}
			if (right == -1) break;
			int itemLocalIndex = Util.QuickRandom(0, right + 1);
			int itemID = squad.GetBlockAt(unitX + itemLocalIndex, currentUnitY, z, BlockType.Element);
			// Spawn Item
			if (HasItem(itemID) && Stage.SpawnEntity(ItemHolder.TYPE_ID, unitX.ToGlobal(), unitY.ToGlobal()) is ItemHolder holder) {
				holder.ItemID = itemID;
				holder.ItemCount = 1;
				holder.Jump();
			}
		}
	}


	// Drop
	public static void DropItemFor (Entity entity) => DropItemFor(entity.TypeID, entity.X, entity.Y);
	public static void DropItemFor (int sourceID, int x, int y) {
		if (!ItemDropPool.TryGetValue(sourceID, out var data)) return;
		if (data.Chance < 1000 && Util.QuickRandom(0, 1000) >= data.Chance) return;
		SpawnItem(data.ItemID, x, y, data.Count);
	}


	#endregion




	#region --- LGC ---


	// Unlock
	private static void LoadUnlockDataFromFile () {
		string unlockPath = Util.CombinePaths(Universe.BuiltIn.SavingMetaRoot, UNLOCK_NAME);
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
		string unlockPath = Util.CombinePaths(Universe.BuiltIn.SavingMetaRoot, UNLOCK_NAME);
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
				if (pool.ContainsKey(key)) continue;
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