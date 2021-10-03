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
		public void RefreshUI (Vector2 gridSize) {
			var rt = RT;
			rt.anchorMin = rt.anchorMax = new Vector2(
				Position.x >= 0 ? 0f : 1f,
				Position.y >= 0 ? 0f : 1f
			);
			rt.pivot = Vector2.zero;
			rt.anchoredPosition3D = new Vector2(
				Position.x * gridSize.x,
				Position.y * gridSize.y
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
		[SerializeField] Vector2 m_GridSize = new Vector2(86, 86);

		// Data
		private readonly List<TileItem> Tiles = new List<TileItem>();
		private Coroutine MouseChecking = null;
		private bool Dragging = false;

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
				trigger.CallbackLeft.AddListener(() => {
					if (Dragging) { return; }
					tile.OnLeftClick();
				});
				trigger.CallbackRight.AddListener(() => {
					if (Dragging) { return; }
					tile.OnRightClick();
				});
				trigger.CallbackDoubleClick.AddListener(() => {
					if (Dragging) { return; }
					tile.OnDoubleClick();
				});
				var drag = grab.Grab<DragToMove>();
				drag.Begin.AddListener(OnTileDragBegin);
				drag.Drag.AddListener(() => OnTileDrag(tile));
				drag.End.AddListener(() => OnTileDragEnd(tile));
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
				tile.RefreshUI(m_GridSize);
			}
		}


		#endregion




		#region --- LGC ---


		private void OnTileDragBegin () {
			if (MouseChecking != null) {
				StopCoroutine(MouseChecking);
			}
			Dragging = true;
			MouseChecking = StartCoroutine(MouseCheck());
			IEnumerator MouseCheck () {
				yield return new WaitUntil(() => !Input.GetMouseButton(0));
				Dragging = false;
			}
		}


		private void OnTileDrag (TileItem tile) => FixAnchor(tile);


		private void OnTileDragEnd (TileItem tile) {
			FixAnchor(tile);





			Dragging = false;
			SaveTilePositions();
		}


		private void FixAnchor (TileItem tile) {
			var rt = tile.RectTransform;
			var tilePos = rt.localPosition;
			var newAnchor = new Vector2(
				tilePos.x > 0f ? 1f : 0f,
				tilePos.y > 0f ? 1f : 0f
			);
			if (rt.anchorMin.NotAlmost(newAnchor)) {
				rt.anchorMin = newAnchor;
			}
			if (rt.anchorMax.NotAlmost(newAnchor)) {
				rt.anchorMax = newAnchor;
			}
		}


		#endregion




	}
}
