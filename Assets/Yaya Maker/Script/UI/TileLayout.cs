using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UIGadget;


namespace YayaMaker.UI {




	public class TestA : TileItem {
		public TestA (RectTransform rt, Text label, Image icon) : base(rt, label, icon) { }

		public override string GetDisplayName () {
			return "AAA";
		}

		public override Sprite GetIcon () {
			return null;
		}
		public override void OnLeftClick () {
			Debug.Log("Left A");
		}
		public override void OnRightClick () {
			Debug.Log("Right A");
		}
	}

	public abstract class TestC : TileItem {
		public TestC (RectTransform rt, Text label, Image icon) : base(rt, label, icon) { }

		public override string GetDisplayName () {
			return "CCC";
		}

		public override Sprite GetIcon () {
			return null;
		}
		public override void OnLeftClick () {
			Debug.Log("Left C");
		}
	}

	public class TestB : TileItem {
		public TestB (RectTransform rt, Text label, Image icon) : base(rt, label, icon) { }

		public class TestBB : TileItem {
			public override string GetDisplayName () {
				return "BBBBBB";
			}
			public TestBB (RectTransform rt, Text label, Image icon) : base(rt, label, icon) { }

			public override Sprite GetIcon () {
				return null;
			}
			public override void OnLeftClick () {
				Debug.Log("Left BB");
			}
		}

		public override string GetDisplayName () {
			return "BBB";
		}

		public override Sprite GetIcon () {
			return null;
		}
		public override void OnLeftClick () {
			Debug.Log("Left B");
		}
	}




	public abstract class TileItem {


		// Const
		private const float TILE_GAP = 6f;

		// Api
		public Vector2Int Position = default;
		public RectTransform RectTransform => RT;

		// Data
		private RectTransform RT = null;
		private Text Label = null;
		private Image Icon = null;

		// API
		public TileItem (RectTransform rt, Text label, Image icon) {
			RT = rt;
			Label = label;
			Icon = icon;
		}
		public void RefreshLabel () => Label.text = GetDisplayName();
		public void RefreshIcon () => Icon.sprite = GetIcon();
		public void RefreshUI () {
			var rt = RT;
			rt.anchorMin = rt.anchorMax = new Vector2(1f, Position.y >= 0 ? 0f : 1f);
			rt.pivot = new Vector2(1f, 0f);
			var size = rt.rect.size;
			rt.anchoredPosition3D = new Vector2(
				Position.x * (size.x + TILE_GAP),
				Position.y * (size.y + TILE_GAP)
			);
		}
		public abstract string GetDisplayName ();
		public abstract Sprite GetIcon ();
		public virtual void OnLeftClick () { }
		public virtual void OnRightClick () { }
		public virtual void OnDoubleClick () { }


	}




	public class TileLayout : MonoBehaviour {




		#region --- VAR ---


		// Ser
		[SerializeField] Grabber m_Template = null;

		// Data
		private readonly List<TileItem> Tiles = new List<TileItem>();

		// Saving
		private SavingString PositionStrs = new SavingString("TileLayout.PositionStrs", "");


		#endregion




		#region --- API ---


		public void ReloadTiles () {
			transform.DestroyAllChirldrenImmediate();
			Tiles.Clear();
			var tiles = Assembly
			   .GetAssembly(typeof(TileItem))
			   .GetTypes()
			   .Where(t => t.IsSubclassOf(typeof(TileItem)) && !t.IsAbstract && !t.IsInterface);
			foreach (var tileType in tiles) {
				var grab = Util.SpawnItemUI(m_Template, transform as RectTransform, tileType.Name);
				var tile = System.Activator.CreateInstance(tileType, new object[] {
					grab.transform as RectTransform,
					grab.Grab<Text>("Label"),
					grab.Grab<Image>("Icon")
				}) as TileItem;
				Tiles.Add(tile);
				var trigger = grab.Grab<TriggerUI>();
				trigger.CallbackLeft.AddListener(tile.OnLeftClick);
				trigger.CallbackRight.AddListener(tile.OnRightClick);
				trigger.CallbackDoubleClick.AddListener(tile.OnDoubleClick);
				grab.Grab<DragToMove>().End.AddListener(() => OnTileDragEnd(tile));
				tile.RefreshLabel();
				tile.RefreshIcon();
			}
			LoadTilePositions();
		}


		public void LoadTilePositions () {
			using var strReader = new StringReader(PositionStrs.Value);
			string line;
			var posMap = new Dictionary<string, Vector2Int>();
			while ((line = strReader.ReadLine()) != null) {
				var strs = line.Split('#');
				if (
					strs.Length >= 3 &&
					!string.IsNullOrEmpty(strs[0]) &&
					int.TryParse(strs[1], out int x) &&
					int.TryParse(strs[2], out int y)
				) {
					posMap.TryAdd(strs[0], new Vector2Int(x, y));
				}
			}
			foreach (var tile in Tiles) {
				string name = tile.GetType().Name;
				if (posMap.ContainsKey(name)) {
					tile.Position = posMap[name];
				}
			}
			RefreshUI();
		}


		public void SaveTilePositions () {
			var builder = new StringBuilder();
			foreach (var tile in Tiles) {
				builder.AppendLine($"{tile.GetType().Name}#{tile.Position.x}#{tile.Position.y}");
			}
			PositionStrs.Value = builder.ToString();
		}


		public void RefreshUI () {
			foreach (var tile in Tiles) {
				tile.RefreshUI();
			}
		}


		#endregion




		#region --- LGC ---


		private void OnTileDragEnd (TileItem tile) {



			SaveTilePositions();
		}


		#endregion




	}
}
