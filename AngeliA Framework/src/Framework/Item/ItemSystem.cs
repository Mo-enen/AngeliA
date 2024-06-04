using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

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
	public static readonly Dictionary<Int4, CombinationData> CombinationPool = new();
	public static readonly Dictionary<int, ItemData> ItemPool = new();

	// Data
	private static bool IsUnlockDirty = false;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static void OnGameInitialize () {
		if (Game.IsToolApplication) return;
		// Init Pool
		ItemPool.Clear();
		foreach (var type in typeof(Item).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not Item item) continue;
			string angeName = type.AngeName();
			ItemPool.TryAdd(type.AngeHash(), new ItemData(
				item,
				$"iName.{angeName}".AngeHash(),
				$"iDes.{angeName}".AngeHash(),
				angeName,
				item.MaxStackCount.Clamp(1, 256)
			));
		}
		// Load Combination from Code
		ItemCombination.LoadCombinationPoolFromCode(CombinationPool);
		// Unlock
		LoadUnlockDataFromFile();
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
	) => ItemCombination.TryGetCombinationFromPool(
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


	public static void SpawnItem (int itemID, int x, int y, int count = 1) {
		if (Stage.SpawnEntity(ItemHolder.TYPE_ID, x, y) is not ItemHolder holder) return;
		holder.ItemID = itemID;
		holder.ItemCount = count;
		holder.Jump();
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


	#endregion




}