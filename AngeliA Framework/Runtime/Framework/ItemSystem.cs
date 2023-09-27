using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		// Data
		private static readonly Dictionary<int, ItemData> ItemPool = new();
		private static readonly Dictionary<Int4, Int2> CombinationPool = new();
		private static bool IsUnlockDirty = false;


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-128)]
		public static void BeforeGameInitialize () {
			// Pool
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
			// Load Unlock Data
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


		public static Item GetItem (int id) => ItemPool.TryGetValue(id, out var item) ? item.Item : null;
		public static string GetItemName (int id) => ItemPool.TryGetValue(id, out var item) ? Language.Get(item.NameID, item.DefaultName) : "";
		public static string GetItemDescription (int id) => ItemPool.TryGetValue(id, out var item) ? Language.Get(item.DescriptionID) : "";
		public static int GetItemMaxStackCount (int id) => ItemPool.TryGetValue(id, out var item) ? item.MaxStackCount : 0;


		public static bool HasItem (int id) => ItemPool.ContainsKey(id);


		public static bool CanUseItem (int id, Entity target) {
			var item = GetItem(id);
			return item != null && item.CanUse(target);
		}


		// Combination
		public static void AddCombination (int item0, int item1, int item2, int item3, int result, int resultCount = 1) {
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
					$"Combination already exists. ({GetItem(CombinationPool[from].A).GetType().Name}) & ({GetItem(result).GetType().Name})"
				);
			}
#endif
			CombinationPool[from] = new Int2(result, resultCount);
		}


		public static bool TryGetCombination (int item0, int item1, int item2, int item3, out int result, out int resultCount) {
			result = 0;
			resultCount = 0;
			var from = GetSortedCombination(item0, item1, item2, item3);
			if (CombinationPool.TryGetValue(from, out var resultValue)) {
				result = resultValue.A;
				resultCount = resultValue.B;
				return true;
			}
			return false;
		}


		public static void FillAllRelatedCombinations (Int4 items, List<Int4> output) {
			if (items.IsZero) return;
			bool includeResult = items.Count(0) == 3;
			foreach (var (craft, result) in CombinationPool) {
				if (includeResult && items.Contains(result.A)) {
					output.Add(craft);
					continue;
				}
				var _craft = craft;
				if (items.A != 0 && !_craft.Swap(items.A, 0)) continue;
				if (items.B != 0 && !_craft.Swap(items.B, 0)) continue;
				if (items.C != 0 && !_craft.Swap(items.C, 0)) continue;
				if (items.D != 0 && !_craft.Swap(items.D, 0)) continue;
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


		public static void SetItemUnloced (int itemID, bool unlocked) {
			if (!ItemPool.TryGetValue(itemID, out var data) || data.Unlocked == unlocked) return;
			data.Unlocked = unlocked;
			IsUnlockDirty = true;
		}


		// Spawn 
		public static void ItemSpawnItemAtPlayer (int itemID, int count = 1) {
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
			string unlockPath = Util.CombinePaths(Const.PlayerDataRoot, UNLOCK_NAME);
			if (Util.FileExists(unlockPath)) {
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
		}


		private static void SaveUnlockDataToFile () {
			string unlockPath = Util.CombinePaths(Const.PlayerDataRoot, UNLOCK_NAME);
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
}