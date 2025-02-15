using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System;

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
		public Int4 OriginalCombinationKeys;
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


	[CheatCode("ResetItemUnlock")]
	internal static void Cheat_ResetItemUnlock () {
		foreach (var (_, data) in ItemPool) {
			data.Unlocked = false;
		}
		IsUnlockDirty = false;
		SaveUnlockDataToFile();
	}


	[OnGameInitialize(-128)]
	internal static TaskResult OnGameInitialize () {

		if (Game.IsToolApplication) return TaskResult.End;
		if (!Cloth.ClothSystemReady) return TaskResult.Continue;
		if (!BodyGadget.BodyGadgetSystemReady) return TaskResult.Continue;

		// Init Item Pool from Code
		var BLOCK_ITEM = typeof(BlockBuilder);
		var CLOTH_ITEM = typeof(ClothItem);
		var BG_ITEM = typeof(BodyGadgetItem);
		var BS_ITEM = typeof(BodySetItem);
		foreach (var type in typeof(Item).AllChildClass()) {
			if (type == BLOCK_ITEM || type == CLOTH_ITEM || type == BG_ITEM || type == BS_ITEM) continue;
			if (Activator.CreateInstance(type) is not Item item) continue;
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
			if (Activator.CreateInstance(type) is not IBlockEntity bEntity) continue;
			string angeName = type.AngeName();
			int id = angeName.AngeHash();
			var blockItem = new BlockBuilder(id, angeName, BlockType.Entity, bEntity.MaxStackCount);
			ItemPool.TryAdd(id, new ItemData(
				blockItem,
				$"iName.{angeName}".AngeHash(),
				$"iDes.{angeName}".AngeHash(),
				angeName,
				blockItem.MaxStackCount.GreaterOrEquel(1)
			));
		}

		// Add Cloth Item
		foreach (var (id, cloth) in Cloth.ForAllCloth()) {
			string angeName = cloth.ClothName;
			var clothItem = new ClothItem(id);
			cloth.GetDisplayName(out int lanID);
			ItemPool.TryAdd(id, new ItemData(
				clothItem,
				lanID, $"iDes.{angeName}".AngeHash(),
				angeName, clothItem.MaxStackCount
			));
		}

		// Add BodyGadget Item
		foreach (var (id, gadget) in BodyGadget.ForAllGadget()) {
			var gadgetItem = new BodyGadgetItem(id);
			gadget.GetDisplayName(gadget.GadgetName, out int lanID);
			ItemPool.TryAdd(id, new ItemData(
				gadgetItem,
				lanID, $"iDes.{gadget.GadgetName}".AngeHash(),
				gadget.GadgetName, gadgetItem.MaxStackCount
			));
		}

		// Add BodySet Item
		foreach (var (id, (type, name)) in BodySetItem.ForAllBodySetCharacterType()) {
			var item = type != null ? new BodySetItem(type) : new BodySetItem(name);
			item.GetDisplayName(item.TargetCharacterName, out int lanID);
			ItemPool.TryAdd(id, new ItemData(
				item,
				lanID, $"iDes.BodySet.{item.TargetCharacterName}".AngeHash(),
				item.TargetCharacterName, item.MaxStackCount
			));
		}

		ItemPoolReady = true;

		// Init Drop Pool from Code
		ItemDropPool.Clear();
		foreach (var (type, att) in Util.AllClassWithAttribute<ItemDropAttribute>(inherit: true)) {
			ItemDropPool.TryAdd(type.AngeHash(), new() {
				ItemID = att.ItemTypeID,
				Chance = att.DropChance,
				Count = att.DropCount,
			});
		}

		// Load Combination from Code
		LoadCombinationPoolFromCode();

		// Load Unlock from File
		LoadUnlockDataFromFile();

		// Final
		ItemUnlockReady = true;
		ItemPool.TrimExcess();
		ItemDropPool.TrimExcess();
		return TaskResult.End;
	}


	[BeforeSavingSlotChanged]
	internal static void BeforeSavingSlotChanged () => ItemUnlockReady = false;


	[OnSavingSlotChanged]
	internal static void OnSavingSlotChanged () {
		LoadUnlockDataFromFile();
		ItemUnlockReady = true;
	}


	[OnMainSheetReload]
	internal static void AddItemsFromSheet () {

		if (Game.IsToolApplication) return;

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
				bType == AtlasType.Level ? BlockType.Level : BlockType.Background,
				256
			);
			ItemPool.TryAdd(itemID, new ItemData(
				blockItem,
				$"iName.{itemName}".AngeHash(),
				$"iDes.{itemName}".AngeHash(),
				itemName,
				blockItem.MaxStackCount.GreaterOrEquel(1)
			));
		}

		// Reload Unlock
		if (Game.GlobalFrame > 0) {
			LoadUnlockDataFromFile();
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


	public static bool CanUseItem (int id, Character target) {
		var item = GetItem(id);
		return item != null && item.ItemConditionCheck(target) && item.CanUse(target);
	}


	// Combination
	public static bool TryGetCombination (
		int item0, int item1, int item2, int item3,
		out int result, out int resultCount,
		out int keep0, out int keep1, out int keep2, out int keep3
	) {
		var from = GetSortedCombination(item0, item1, item2, item3);
		if (CombinationPool.TryGetValue(from, out var resultValue)) {
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


	public static void ClearCombination () => CombinationPool.Clear();


	public static void GetRelatedCombinations (Int4 combination, List<Int4> output, int materialCountLimit) {
		if (combination.IsZero) return;
		bool includeResult = combination.Count(0) == 3;
		foreach (var (craft, result) in CombinationPool) {
			if (4 - craft.Count(0) > materialCountLimit) continue;
			if (includeResult && combination.Contains(result.Result)) {
				output.Add(craft);
				continue;
			}
			var _craft = craft;
			if (combination.x != 0 && !ReplaceTo(ref _craft, combination.x, 0)) continue;
			if (combination.y != 0 && !ReplaceTo(ref _craft, combination.y, 0)) continue;
			if (combination.z != 0 && !ReplaceTo(ref _craft, combination.z, 0)) continue;
			if (combination.w != 0 && !ReplaceTo(ref _craft, combination.w, 0)) continue;
			output.Add(result.OriginalCombinationKeys);
		}
		static bool ReplaceTo (ref Int4 host, int value, int newValue) {
			if (host.x == value) {
				host.x = newValue;
				return true;
			}
			if (host.y == value) {
				host.y = newValue;
				return true;
			}
			if (host.z == value) {
				host.z = newValue;
				return true;
			}
			if (host.w == value) {
				host.w = newValue;
				return true;
			}
			return false;
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
		if (unlocked) {
			FrameworkUtil.InvokeItemUnlocked(itemID);
		}
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
	private static void LoadCombinationPoolFromCode () {

#if DEBUG
		var typeList = new List<(int id, System.Type type)>();
#endif

		CombinationPool.Clear();

		// Global Combination
		foreach (var assembly in Util.AllAssemblies) {
			int prevCount = CombinationPool.Count;
			var iComs = assembly.GetCustomAttributes<BasicItemCombinationAttribute>();
			if (iComs != null) {
				LoadCombinationsIntoPool(null, iComs);
			}
		}

		// Item
		foreach (var type in typeof(Item).AllChildClass()) {
			var iComs = type.GetCustomAttributes<BasicItemCombinationAttribute>(false);
			if (iComs != null) {
				LoadCombinationsIntoPool(type, iComs);
			}
#if DEBUG
			typeList.Add((type.AngeHash(), type));
#endif
		}

		// Block Entity
		foreach (var type in typeof(IBlockEntity).AllClassImplemented()) {
			var iComs = type.GetCustomAttributes<BasicItemCombinationAttribute>(false);
			if (iComs != null) {
				LoadCombinationsIntoPool(type, iComs);
			}
#if DEBUG
			typeList.Add((type.AngeHash(), type));
#endif
		}

#if DEBUG

		var resultHash = new HashSet<int>();
		foreach (var (com, comData) in CombinationPool) {
			resultHash.Add(comData.Result);
		}
		foreach (var (id, type) in typeList) {
			if (resultHash.Contains(id)) continue;
			var nc = type.GetCustomAttributes<NoItemCombinationAttribute>(true);
			if (nc == null || !nc.Any()) {
				Debug.LogWarning($"Item \"{type.AngeName()}\" have no valid combination. Add attribute \"NoItemCombinationAttribute\" to ignore this warning.");
			}
		}
#endif

		// Func
		static void LoadCombinationsIntoPool (Type type, IEnumerable<BasicItemCombinationAttribute> iComs) {
			int typeID = type != null ? type.AngeHash() : 0;
			foreach (var com in iComs) {
				if (com.Count <= 0) continue;
				if (
					string.IsNullOrEmpty(com.ItemA) &&
					string.IsNullOrEmpty(com.ItemB) &&
					string.IsNullOrEmpty(com.ItemC) &&
					string.IsNullOrEmpty(com.ItemD)
				) continue;
				int idA = !string.IsNullOrEmpty(com.ItemA) ? com.ItemA.AngeHash() : 0;
				int idB = !string.IsNullOrEmpty(com.ItemB) ? com.ItemB.AngeHash() : 0;
				int idC = !string.IsNullOrEmpty(com.ItemC) ? com.ItemC.AngeHash() : 0;
				int idD = !string.IsNullOrEmpty(com.ItemD) ? com.ItemD.AngeHash() : 0;
				var key = GetSortedCombination(idA, idB, idC, idD);
#if DEBUG
				// Check for Invalid Name
				if (idA != 0 && GetItem(idA) == null) {
					string tName = type != null ? type.Name : com is BasicGlobalItemCombinationAttribute _gCom ? _gCom.Result : "";
					Debug.Log($"Item name \"{com.ItemA}\" is invalid for combination.({tName})");
					continue;
				}
				if (idB != 0 && GetItem(idB) == null) {
					string tName = type != null ? type.Name : com is BasicGlobalItemCombinationAttribute _gCom ? _gCom.Result : "";
					Debug.Log($"Item name \"{com.ItemB}\" is invalid for combination.({tName})");
					continue;
				}
				if (idC != 0 && GetItem(idC) == null) {
					string tName = type != null ? type.Name : com is BasicGlobalItemCombinationAttribute _gCom ? _gCom.Result : "";
					Debug.Log($"Item name \"{com.ItemC}\" is invalid for combination.({tName})");
					continue;
				}
				if (idD != 0 && GetItem(idD) == null) {
					string tName = type != null ? type.Name : com is BasicGlobalItemCombinationAttribute _gCom ? _gCom.Result : "";
					Debug.Log($"Item name \"{com.ItemD}\" is invalid for combination.({tName})");
					continue;
				}
#endif
				// Check for Duplicate Combination
				if (CombinationPool.ContainsKey(key)) {
#if DEBUG
					var resultItem = GetItem(CombinationPool[key].Result);
					if (resultItem != null) {
						string tName = type != null ? type.Name : com is BasicGlobalItemCombinationAttribute _gCom ? _gCom.Result : "";
						string rName;
						if (resultItem is BlockBuilder rBuilder && Renderer.TryGetSpriteForGizmos(rBuilder.BlockID, out var bSprite)) {
							rName = bSprite.RealName;
						} else {
							rName = resultItem.GetType().Name;
						}
						Debug.Log($"Item Combination Collistion: \"{tName}\" & \"{rName}\"");
					}
#endif
					continue;
				}

				// Check Result Item Exists
				int resultID = com is BasicGlobalItemCombinationAttribute gCom ? gCom.Result.AngeHash() : typeID;
				if (GetItem(resultID) == null) {
#if DEBUG
					if (com is BasicGlobalItemCombinationAttribute _gCom) {
						Debug.Log($"Item name \"{_gCom.Result}\" is invalid for combination.");
					}
#endif
					continue;
				}

				CombinationPool.Add(key, new CombinationData() {
					Result = resultID,
					ResultCount = com.Count,
					Keep0 = com.KeepId0,
					Keep1 = com.KeepId1,
					Keep2 = com.KeepId2,
					Keep3 = com.KeepId3,
					OriginalCombinationKeys = new(idA, idB, idC, idD),
				});
			}
		}
		CombinationPool.TrimExcess();
	}


	public static Item GetItem (object resultID) {
		throw new NotImplementedException();
	}


	#endregion




}
