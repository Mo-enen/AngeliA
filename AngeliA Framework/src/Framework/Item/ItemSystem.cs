using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;


namespace AngeliA.Framework; 
public static class ItemSystem {




	#region --- SUB ---


	private class ItemData {
		public Item Item;
		public int NameID;
		public int DescriptionID;
		public string DefaultName;
		public int MaxStackCount;
		public bool Unlocked;
		public ItemData (Item item, int nameID, int descriptionID, string defaultName, int maxStackCount) {
			Item = item;
			NameID = nameID;
			DescriptionID = descriptionID;
			DefaultName = defaultName;
			MaxStackCount = maxStackCount;
			Unlocked = false;
		}
	}


	private class CombinationData {
		public int Result;
		public int ResultCount;
		public int IgnoreConsume0;
		public int IgnoreConsume1;
		public int IgnoreConsume2;
		public int IgnoreConsume3;
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

	// Data
	private static readonly Dictionary<int, ItemData> ItemPool = new();
	private static readonly Dictionary<Int4, CombinationData> CombinationPool = new();
	private static bool IsUnlockDirty = false;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static void OnGameInitialize () {
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
	}


	[OnUniverseOpen(31)]
	public static void OnUniverseOpen () {
		CreateItemCombinationHelperFiles(UniverseSystem.CurrentUniverse.SavingRoot);
		CreateCombinationFileFromCode(UniverseSystem.CurrentUniverse.UniverseRoot, false);
		CombinationPool.Clear();
		LoadCombinationFromFile(Util.CombinePaths(UniverseSystem.CurrentUniverse.ItemCustomizationRoot, AngePath.COMBINATION_FILE_NAME));
		LoadCombinationFromFile(Util.CombinePaths(UniverseSystem.CurrentUniverse.UniverseMetaRoot, AngePath.COMBINATION_FILE_NAME));
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
	public static string GetItemName (int id) => ItemPool.TryGetValue(id, out var item) ? Language.Get(item.NameID, item.DefaultName) : "";
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
	public static void DrawItemShortInfo (int itemID, IRect panelRect, int z, int armorIcon, int armorEmptyIcon) {

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
							CellRenderer.Draw(i < progress ? armorIcon : armorEmptyIcon, rect, z);
							rect.x += rect.width;
						}
					}
					break;
			}
		}


	}


	// Combination
	public static void CreateItemCombinationHelperFiles (string savingFolder) {

		string itemCusRoot = AngePath.GetItemCustomizationRoot(savingFolder);

		// Create User Combination Template
		string combineFilePath = Util.CombinePaths(itemCusRoot, AngePath.COMBINATION_FILE_NAME);
		if (!Util.FileExists(combineFilePath)) {
			Util.TextToFile(@"
#
# Custom Item Combination Formula
# 
#
# Remove '#' for the lines below will change
# 'TreeTrunk' to 'ItemCoin' for making chess pieces
# 
# Item names can be found in the helper file next to
# this file
#
# Example:
#
# ItemCoin + RuneWater + RuneFire = ChessPawn
# ItemCoin + RuneFire + RuneLightning = ChessKnight
# ItemCoin + RunePoison + RuneFire = ChessBishop
# ItemCoin + RuneWater + RuneLightning = ChessRook
# ItemCoin + RuneWater + RunePoison = ChessQueen
# ItemCoin + RunePoison + RuneLightning = ChessKing
#
#
#", combineFilePath);
		}

		// Create Item Name Helper
		string helperPath = Util.CombinePaths(itemCusRoot, "Item Name Helper.txt");
		if (!Util.FileExists(helperPath)) {
			var builder = new StringBuilder();
			foreach (var type in typeof(Item).AllChildClass()) {
				builder.AppendLine(type.AngeName());
			}
			Util.TextToFile(builder.ToString(), helperPath);
		}

	}


	public static void CreateCombinationFileFromCode (string universeFolder, bool forceCreate) {

		string metaRoot = AngePath.GetUniverseMetaRoot(universeFolder);

		string builtInPath = Util.CombinePaths(metaRoot, AngePath.COMBINATION_FILE_NAME);
		if (!forceCreate && Util.FileExists(builtInPath)) return;

		var builder = new StringBuilder();
		foreach (var type in typeof(Item).AllChildClass()) {
			string result = type.AngeName();
			var iComs = type.GetCustomAttributes<ItemCombinationAttribute>(false);
			if (iComs == null) continue;
			foreach (var com in iComs) {
				if (com.Count <= 0) continue;
				if (
					com.ItemA == null && com.ItemB == null &&
					com.ItemC == null && com.ItemD == null
				) continue;
				if (com.ItemA != null) {
					if (!com.ConsumeA) builder.Append('^');
					builder.Append(com.ItemA.AngeName());
				}
				if (com.ItemB != null) {
					builder.Append(' ');
					builder.Append('+');
					builder.Append(' ');
					if (!com.ConsumeB) builder.Append('^');
					builder.Append(com.ItemB.AngeName());
				}
				if (com.ItemC != null) {
					builder.Append(' ');
					builder.Append('+');
					builder.Append(' ');
					if (!com.ConsumeC) builder.Append('^');
					builder.Append(com.ItemC.AngeName());
				}
				if (com.ItemD != null) {
					builder.Append(' ');
					builder.Append('+');
					builder.Append(' ');
					if (!com.ConsumeD) builder.Append('^');
					builder.Append(com.ItemD.AngeName());
				}
				builder.Append(' ');
				builder.Append('=');
				if (com.Count > 1) {
					builder.Append(' ');
					builder.Append(com.Count);
				}
				builder.Append(' ');
				builder.Append(result);
				builder.Append('\n');
			}
		}
		Util.TextToFile(builder.ToString(), builtInPath);
	}


	public static void AddCombination (
		int item0, int item1, int item2, int item3,
		int result, int resultCount,
		bool consumeA, bool consumeB, bool consumeC, bool consumeD
	) {

		if (result == 0 || resultCount <= 0) {
			if (Game.IsEdittime) {
				if (result == 0) Util.LogWarning("Result of combination should not be zero.");
				if (resultCount == 0) Util.LogWarning("ResultCount of combination should not be zero.");
			}
			return;
		}

		var from = GetSortedCombination(item0, item1, item2, item3);
		if (CombinationPool.ContainsKey(from)) {
			if (Game.IsEdittime) {
				Util.LogError($"Combination already exists. ({GetItem(CombinationPool[from].Result).GetType().Name}) & ({GetItem(result).GetType().Name})");
			}
			return;
		}

		CombinationPool[from] = new CombinationData() {
			Result = result,
			ResultCount = resultCount,
			IgnoreConsume0 = consumeA ? 0 : item0,
			IgnoreConsume1 = consumeB ? 0 : item1,
			IgnoreConsume2 = consumeC ? 0 : item2,
			IgnoreConsume3 = consumeD ? 0 : item3,
		};
	}


	public static bool TryGetCombination (
		int item0, int item1, int item2, int item3,
		out int result, out int resultCount,
		out int ignoreConsume0, out int ignoreConsume1, out int ignoreConsume2, out int ignoreConsume3
	) {
		var from = GetSortedCombination(item0, item1, item2, item3);
		if (CombinationPool.TryGetValue(from, out var resultValue)) {
			result = resultValue.Result;
			resultCount = resultValue.ResultCount;
			ignoreConsume0 = resultValue.IgnoreConsume0;
			ignoreConsume1 = resultValue.IgnoreConsume1;
			ignoreConsume2 = resultValue.IgnoreConsume2;
			ignoreConsume3 = resultValue.IgnoreConsume3;
			return true;
		}
		result = 0;
		resultCount = 0;
		ignoreConsume0 = ignoreConsume1 = ignoreConsume2 = ignoreConsume3 = 0;
		return false;
	}


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
			var cameraRect = CellRenderer.CameraRect;
			SpawnItem(itemID, cameraRect.CenterX(), cameraRect.CenterY(), count);
		}
		return true;
	}


	public static void SpawnItemAtTarget (Entity target, int itemID, int count = 1) {
		int x = target != null ? target.Rect.x - Const.CEL : CellRenderer.CameraRect.CenterX();
		int y = target != null ? target.Y : CellRenderer.CameraRect.CenterY();
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
		string unlockPath = Util.CombinePaths(UniverseSystem.CurrentUniverse.SavingMetaRoot, UNLOCK_NAME);
		if (!Util.FileExists(unlockPath)) return;
		foreach (var (_, data) in ItemPool) data.Unlocked = false;
		var bytes = Util.FileToByte(unlockPath);
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
		string unlockPath = Util.CombinePaths(UniverseSystem.CurrentUniverse.SavingMetaRoot, UNLOCK_NAME);
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
	private static void LoadCombinationFromFile (string filePath) {
		if (!Util.FileExists(filePath)) return;
		var builder = new StringBuilder();
		foreach (string _line in Util.ForAllLines(filePath)) {
			if (string.IsNullOrEmpty(_line)) continue;
			string line = _line.TrimWhiteForStartAndEnd();
			if (line.StartsWith('#')) continue;
			builder.Clear();
			var com = Int4.zero;
			var consume = Int4.zero;
			int appendingComIndex = 0;
			bool appendingResultCount = false;
			int resultID = 0;
			int resultCount = 1;
			foreach (var c in line) {
				if (c == ' ') continue;
				if (c == '+' || c == '=') {
					if (builder.Length > 0 && appendingComIndex < 4) {
						if (builder[0] == '^') {
							builder.Remove(0, 1);
							consume[appendingComIndex] = 1;
						}
						com[appendingComIndex] = builder.ToString().AngeHash();
						appendingComIndex++;
					}
					if (c == '=') {
						appendingResultCount = true;
					}
					builder.Clear();
				} else {
					if (appendingResultCount && !char.IsDigit(c)) {
						appendingResultCount = false;
						if (builder.Length > 0 && int.TryParse(builder.ToString(), out int _resultCount)) {
							resultCount = _resultCount;
						}
						builder.Clear();
					}
					if (c != ' ') builder.Append(c);
				}
			}

			// Result
			if (builder.Length > 0) {
				resultID = builder.ToString().AngeHash();
			}

			// Add to Pool
			if (com != Int4.zero && resultCount >= 1 && resultID != 0) {
				AddCombination(com.x, com.y, com.z, com.w, resultID, resultCount, consume[0] == 0, consume[1] == 0, consume[2] == 0, consume[3] == 0);
			}

		}
	}


	#endregion




}