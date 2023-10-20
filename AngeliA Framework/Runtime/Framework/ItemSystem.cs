using System.Collections;
using System.Collections.Generic;
using System.IO;
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

		// Api
		public static string CombinationFilePath => Util.CombinePaths(AngePath.MetaRoot, "Item Combination.txt");

		// Data
		private static readonly Dictionary<int, ItemData> ItemPool = new();
		private static readonly Dictionary<Vector4Int, Vector2Int> CombinationPool = new();
		private static bool IsUnlockDirty = false;


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-128)]
		public static void BeforeGameInitialize () {
			InitializeItemPool(true);
			LoadUnlockDataFromFile();
			FillCombinationFromFile(CombinationPool, CombinationFilePath);
		}


		[OnGameUpdate]
		internal static void Update () {
			// Save Unlock Data
			if (IsUnlockDirty) {
				IsUnlockDirty = false;
				SaveUnlockDataToFile();
			}
		}


		[OnSlotChanged]
		public static void OnSlotChanged () => LoadUnlockDataFromFile();


		#endregion




		#region --- API ---


		// Pool
		public static void InitializeItemPool (bool forceInitialize = false) {
			if (!forceInitialize && ItemPool.Count > 0) return;
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


		// Combination
		public static void FillCombinationFromFile (Dictionary<Vector4Int, Vector2Int> pool, string filePath) {
			pool.Clear();
			var builder = new StringBuilder();
			foreach (string line in Util.ForAllLines(filePath)) {
				if (string.IsNullOrEmpty(line)) continue;
				builder.Clear();
				var com = Vector4Int.Zero;
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
						builder.Append(c);
					}
				}

				// Result
				if (builder.Length > 0) {
					resultID = builder.ToString().AngeHash();
				}

				// Add to Pool
				if (com != Vector4Int.Zero && resultCount >= 1 && resultID != 0) {
					AddCombination(pool, com.x, com.y, com.z, com.w, resultID, resultCount);
				}

			}

			// Func
			static void AddCombination (Dictionary<Vector4Int, Vector2Int> pool, int item0, int item1, int item2, int item3, int result, int resultCount = 1) {
				if (result == 0 || resultCount <= 0) {
#if UNITY_EDITOR
					if (result == 0) Debug.LogWarning("Result of combination should not be zero.");
					if (resultCount == 0) Debug.LogWarning("ResultCount of combination should not be zero.");
#endif
					return;
				}
				var from = GetSortedCombination(item0, item1, item2, item3);
#if UNITY_EDITOR
				if (pool.ContainsKey(from) && UnityEditor.EditorApplication.isPlaying) {
					Debug.LogError(
						$"Combination already exists. ({GetItem(pool[from].x).GetType().Name}) & ({GetItem(result).GetType().Name})"
					);
				}
#endif
				pool[from] = new Vector2Int(result, resultCount);
			}
		}


		public static void SaveCombinationToFile (Dictionary<Vector4Int, Vector2Int> pool, string filePath) {
			InitializeItemPool(false);
			var builder = new StringBuilder();
			foreach (var (com, result) in pool) {
				if (
					result.x == 0 || result.y <= 0 ||
					!ItemPool.TryGetValue(result.x, out var resultData)
				) continue;
				// Com
				bool hasAppendItem = false;
				for (int i = 0; i < 4; i++) {
					int id = com[i];
					if (id == 0 || !ItemPool.TryGetValue(id, out var itemData)) continue;
					if (hasAppendItem) builder.Append(" + ");
					builder.Append(itemData.Item.GetType().AngeName());
					hasAppendItem = true;
				}
				// Result
				builder.Append(" = ");
				if (result.y > 1) builder.Append(result.y);
				builder.Append(resultData.Item.GetType().AngeName());
				// Final
				builder.Append('\n');
			}
			Util.TextToFile(builder.ToString(), filePath);
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


		public static void FillAllRelatedCombinations (Vector4Int combination, List<Vector4Int> output) {
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
			string unlockPath = Util.CombinePaths(AngePath.PlayerDataRoot, UNLOCK_NAME);
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


		#endregion




	}
}