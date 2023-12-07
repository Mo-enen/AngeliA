using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;


namespace AngeliaFramework {
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


		#endregion




		#region --- VAR ---


		// Const
		private const string UNLOCK_NAME = "UnlockedItem";
		public const string COMBINATION_FILE_NAME = "Item Combination.txt";
		private static readonly int[] ITEM_TYPE_ICONS = {
			"ItemIcon.Weapon".AngeHash(),
			"ItemIcon.Armor".AngeHash(),
			"ItemIcon.Helmet".AngeHash(),
			"ItemIcon.Shoes".AngeHash(),
			"ItemIcon.Gloves".AngeHash(),
			"ItemIcon.Jewelry".AngeHash(),
			"ItemIcon.Food".AngeHash(),
			"ItemIcon.Item".AngeHash(),
		};

		// Data
		private static readonly Dictionary<int, ItemData> ItemPool = new();
		private static readonly Dictionary<Vector4Int, Vector2Int> CombinationPool = new();
		private static bool IsUnlockDirty = false;


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-128)]
		internal static void BeforeGameInitialize () {
			ItemPool.Clear();
			foreach (var type in typeof(Item).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not Item item) continue;
				string angeName = type.AngeName();
				ItemPool.TryAdd(type.AngeHash(), new(
					item,
					$"iName.{angeName}".AngeHash(),
					$"iDes.{angeName}".AngeHash(),
					angeName,
					item.MaxStackCount.Clamp(1, 256)
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


		[OnSlotChanged(256)]
		internal static void OnSlotChanged () {
			LoadUnlockDataFromFile();
			CombinationPool.Clear();
			LoadCombinationFromFile();
			LoadCombinationFromCode();
		}


		[OnSlotCreated]
		public static void SlotCreated () {
			// Create Combination File
			string combineFilePath = Util.CombinePaths(AngePath.ItemSaveDataRoot, COMBINATION_FILE_NAME);
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

			// Create Item Name Helper
			var builder = new StringBuilder();
			foreach (var type in typeof(Item).AllChildClass()) {
				builder.AppendLine(type.AngeName());
			}
			Util.TextToFile(builder.ToString(), Util.CombinePaths(AngePath.ItemSaveDataRoot, "Item Name Helper.txt"));

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
		public static void DrawItemShortInfo (int itemID, RectInt panelRect, int z, int armorIcon, int armorEmptyIcon) {

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
							var rect = new RectInt(panelRect.x, panelRect.y, panelRect.height, panelRect.height);
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
		public static void AddCombination (int item0, int item1, int item2, int item3, int result, int resultCount) {
			if (result == 0 || resultCount <= 0) {
#if UNITY_EDITOR
				if (result == 0) Debug.LogWarning("Result of combination should not be zero.");
				if (resultCount == 0) Debug.LogWarning("ResultCount of combination should not be zero.");
#endif
				return;
			}
			var from = GetSortedCombination(item0, item1, item2, item3);
#if UNITY_EDITOR
			if (CombinationPool.ContainsKey(from) && UnityEditor.EditorApplication.isPlaying) {
				Debug.LogError(
					$"Combination already exists. ({GetItem(CombinationPool[from].x).GetType().Name}) & ({GetItem(result).GetType().Name})"
				);
			}
#endif
			CombinationPool[from] = new Vector2Int(result, resultCount);
		}


		public static bool TryGetCombination (int item0, int item1, int item2, int item3, out int result, out int resultCount) {
			result = 0;
			resultCount = 0;
			var from = GetSortedCombination(item0, item1, item2, item3);
			if (CombinationPool.TryGetValue(from, out var resultValue)) {
				result = resultValue.x;
				resultCount = resultValue.y;
				return true;
			}
			return false;
		}


		public static void ClearCombination () => CombinationPool.Clear();


		public static void GetRelatedCombinations (Vector4Int combination, List<Vector4Int> output) {
			if (combination.IsZero) return;
			bool includeResult = combination.Count(0) == 3;
			foreach (var (craft, result) in CombinationPool) {
				if (includeResult && combination.Contains(result.x)) {
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


		public static Vector4Int GetSortedCombination (int a, int b, int c, int d) {

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

			return new Vector4Int(a, b, c, d);
		}


		public static IEnumerator<KeyValuePair<Vector4Int, Vector2Int>> GetCombinationPoolEnumerator () => CombinationPool.GetEnumerator();


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
		public static bool GiveItemToPlayer (int itemID, int count = 1) {
			var player = Player.Selecting;
			if (player == null) return false;
			count -= Inventory.CollectItem(player.TypeID, itemID, count);
			if (count > 0) {
				SpawnItemAtPlayer(itemID, count);
			}
			return true;
		}


		public static void SpawnItemAtPlayer (int itemID, int count = 1) {
			int x = Player.Selecting != null ? Player.Selecting.Rect.x - Const.CEL : CellRenderer.CameraRect.CenterX();
			int y = Player.Selecting != null ? Player.Selecting.Y : CellRenderer.CameraRect.CenterY();
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
			string unlockPath = Util.CombinePaths(AngePath.PlayerDataRoot, UNLOCK_NAME);
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
			string unlockPath = Util.CombinePaths(AngePath.PlayerDataRoot, UNLOCK_NAME);
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
		private static void LoadCombinationFromFile () {
			string filePath = Util.CombinePaths(AngePath.ItemSaveDataRoot, COMBINATION_FILE_NAME);
			if (!Util.FileExists(filePath)) return;
			var builder = new StringBuilder();
			foreach (string line in Util.ForAllLines(filePath)) {
				if (string.IsNullOrEmpty(line)) continue;
				if (line.StartsWith('#')) continue;
				builder.Clear();
				var com = Vector4Int.zero;
				int appendingComIndex = 0;
				bool appendingResultCount = false;
				int resultID = 0;
				int resultCount = 1;
				foreach (var c in line) {
					if (c == ' ') continue;
					if (c == '+' || c == '=') {
						if (builder.Length > 0 && appendingComIndex < 4) {
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
				if (com != Vector4Int.zero && resultCount >= 1 && resultID != 0) {
					AddCombination(com.x, com.y, com.z, com.w, resultID, resultCount);
				}

			}
		}


		private static void LoadCombinationFromCode () {

			// Get Ignore
			var ignore = new HashSet<int>();
			foreach (var (_, result) in CombinationPool) {
				ignore.TryAdd(result.x);
			}

			// Fill Pool from Attribute
			foreach (var type in typeof(Item).AllChildClass()) {
				int resultID = type.AngeHash();
				if (ignore.Contains(resultID)) continue;
				var iComs = type.GetCustomAttributes<EntityAttribute.ItemCombinationAttribute>(false);
				if (iComs == null) continue;
				foreach (var com in iComs) {
					if (com.Count <= 0) continue;
					if (com.ItemA == 0 && com.ItemB == 0 && com.ItemC == 0 && com.ItemD == 0) continue;
					AddCombination(com.ItemA, com.ItemB, com.ItemC, com.ItemD, resultID, com.Count);
				}
			}

		}


		#endregion




	}
}