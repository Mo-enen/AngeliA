using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AngeliaFramework;



namespace AngeliaFramework.Editor {
	public class CraftingEditor : UtilWindow {



		// SUB
		private class WindowStyle {

		}

		private class ItemData {
			public int ID;
			public Item Item;
			public Rect UvRect;
		}


		// Data 
		private readonly Dictionary<Vector4Int, Vector2Int> ComPool = new();
		private readonly List<ItemData> ItemList = new();
		private Trie<int> SearchTrie = null;
		private Texture2D Texture = null;
		private WindowStyle Style = null;
		private bool Loaded = false;
		private bool IsDirty = false;


		// MSG
		[MenuItem("AngeliA/Crafting Editor", false, 27)]
		public static void OpenWindow () {
			var window = GetWindow<CraftingEditor>(true, "Crafting", true);
			window.minSize = new Vector2(512, 256);
			window.maxSize = new Vector2(1600, 1024);
		}


		private void OnEnable () {
			try {
				ItemList.Clear();
				ItemSystem.InitializeItemPool(true);
				// Load Com Pool
				Loaded = Load();
				if (Loaded) {
					// Load Texture
					var game = FindAnyObjectByType<Game>(FindObjectsInactive.Include);
					if (game != null) {
						Texture = game.Editor_GetSheetTexture();
					}
					// Load UvRect from Sheet
					int textureWidth = Texture != null ? Texture.width : 1;
					int textureHeight = Texture != null ? Texture.height : 1;
					var uvPool = new Dictionary<int, Rect>();
					var sheet = AngeUtil.LoadJson<SpriteSheet>(AngePath.SheetRoot);
					if (sheet != null) {
						foreach (var sprite in sheet.Sprites) {
							uvPool[sprite.GlobalID] = sprite.GetTextureRect(textureWidth, textureHeight).ToRect();
						}
					}
					// Item Pool
					SearchTrie = new();
					foreach (var itemType in typeof(Item).AllChildClass()) {
						int itemID = itemType.AngeHash();
						var item = ItemSystem.GetItem(itemID);
						if (item == null) continue;
						ItemList.Add(new ItemData() {
							ID = itemID,
							Item = item,
							UvRect = uvPool.TryGetValue(itemID, out var _uvRect) ? _uvRect : default,
						});
						SearchTrie.AddForSearching(Util.GetDisplayName(itemType.AngeName()), itemID);
					}
				}
			} catch (System.Exception ex) {
				Loaded = false;
				Debug.LogException(ex);
			}
		}


		protected override void OnLostFocus () { }


		protected override void BeforeWindowGUI () {
			base.BeforeWindowGUI();
			if (!Loaded) {
				Close();
				return;
			}
			Style ??= new();
			BarGUI();
		}


		protected override void OnWindowGUI () {
			if (!Loaded) {
				Close();
				return;
			}
			ContentGUI();
		}


		private void BarGUI () {
			using var _ = new GUILayout.HorizontalScope(EditorStyles.toolbar);
			const int HEIGHT = 20;





		}


		private void ContentGUI () {

			if (ItemList.Count == 0) {
				EditorGUILayout.HelpBox("No Item class in this project.", MessageType.Info, true);
				return;
			}

			// Items




		}


		private void OnDestroy () {
			if (Loaded && IsDirty) {
				Save();
			}
		}


		// LGC
		private bool Load () {
			IsDirty = false;
			ComPool.Clear();
			ItemSystem.FillCombinationFromFile(ComPool, ItemSystem.CombinationFilePath);
			return ComPool.Count > 0;
		}


		private void Save () {
			IsDirty = false;
			ItemSystem.SaveCombinationToFile(ComPool, ItemSystem.CombinationFilePath);
		}


		private void DrawItem (Rect rect, ItemData itemData, int count = 1) {

			// BG
			GUI.Box(rect, GUIContent.none);

			if (Texture == null) return;

			// Icon
			GUI.DrawTextureWithTexCoords(rect, Texture, itemData.UvRect);

			// Number
			if (count > 1) {
				var numberRect = new Rect(
					rect.xMax - rect.width / 3,
					rect.yMax - rect.height / 3,
					rect.width / 3,
					rect.height / 3
				);
				EditorGUI.DrawRect(numberRect, Color.black);
				GUI.Label(numberRect, count.ToString(), MGUI.CenteredLabel);
			}

		}


	}
}