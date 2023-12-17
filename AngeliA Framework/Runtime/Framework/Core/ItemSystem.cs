using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;


namespace AngeliaFramework {
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
	public class ItemCombinationAttribute : System.Attribute {
		public System.Type ItemA = null;
		public System.Type ItemB = null;
		public System.Type ItemC = null;
		public System.Type ItemD = null;
		public int Count = 1;
		public bool ConsumeA = true;
		public bool ConsumeB = true;
		public bool ConsumeC = true;
		public bool ConsumeD = true;
		public ItemCombinationAttribute (System.Type itemA, int count = 1, bool consumeA = true) : this(itemA, null, null, null, count, consumeA, true, true, true) { }
		public ItemCombinationAttribute (System.Type itemA, System.Type itemB, int count = 1, bool consumeA = true, bool consumeB = true) : this(itemA, itemB, null, null, count, consumeA, consumeB, true, true) { }
		public ItemCombinationAttribute (System.Type itemA, System.Type itemB, System.Type itemC, int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true) : this(itemA, itemB, itemC, null, count, consumeA, consumeB, consumeC, true) { }
		public ItemCombinationAttribute (System.Type itemA, System.Type itemB, System.Type itemC, System.Type itemD, int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true) {
			ItemA = itemA;
			ItemB = itemB;
			ItemC = itemC;
			ItemD = itemD;
			Count = count;
			ConsumeA = consumeA;
			ConsumeB = consumeB;
			ConsumeC = consumeC;
			ConsumeD = consumeD;
		}
	}


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
		private static readonly Dictionary<Int4, CombinationData> CombinationPool = new();
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
			// Combination
			CombinationPool.Clear();
			LoadCombinationFromFile(Util.CombinePaths(AngePath.ItemSaveDataRoot, AngePath.COMBINATION_FILE_NAME));
			LoadCombinationFromFile(Util.CombinePaths(AngePath.MetaRoot, AngePath.COMBINATION_FILE_NAME));
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
		internal static void OnSlotChanged () => LoadUnlockDataFromFile();


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
		public static void AddCombination (
			int item0, int item1, int item2, int item3,
			int result, int resultCount,
			bool consumeA, bool consumeB, bool consumeC, bool consumeD
		) {

			if (result == 0 || resultCount <= 0) {
#if UNITY_EDITOR
				if (result == 0) Debug.LogWarning("Result of combination should not be zero.");
				if (resultCount == 0) Debug.LogWarning("ResultCount of combination should not be zero.");
#endif
				return;
			}

			var from = GetSortedCombination(item0, item1, item2, item3);
			if (CombinationPool.ContainsKey(from)) {
#if UNITY_EDITOR
				Debug.LogError($"Combination already exists. ({GetItem(CombinationPool[from].Result).GetType().Name}) & ({GetItem(result).GetType().Name})");
#endif
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
			string unlockPath = Util.CombinePaths(AngePath.UserDataRoot, UNLOCK_NAME);
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
			string unlockPath = Util.CombinePaths(AngePath.UserDataRoot, UNLOCK_NAME);
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
}