using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UIGadget;


namespace YayaMaker.UI {



	public class TestTileA : TileItem {
		public TestTileA (RectTransform rt, Text label, Image icon) : base(rt, label, icon) { }
		public override string GetDisplayName () => "Test A";
	}
	public class TestTileB : TileItem {
		public TestTileB (RectTransform rt, Text label, Image icon) : base(rt, label, icon) { }
		public override string GetDisplayName () => "Test B";
	}
	public class TestTileC : TileItem {
		public TestTileC (RectTransform rt, Text label, Image icon) : base(rt, label, icon) { }
		public override string GetDisplayName () => "Test C";
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
		public virtual string GetDisplayName () => "";
		public virtual Sprite GetIcon () => null;
		public virtual void OnLeftClick () { }


	}



	public class TileUI : MonoBehaviour {




		#region --- SUB ---


		[System.Serializable] public class VoidEvent : UnityEvent { }


		#endregion




		#region --- VAR ---


		// Const
		private const float OVERLAP_THICKNESS = 6f;

		// Ser
		[SerializeField] Grabber m_Template = null;
		[SerializeField] Vector2 m_GridSize = new(86, 86);
		[SerializeField] AnimationCurve m_SwipeCurve = new();
		[SerializeField] VoidEvent m_OnItemDragged = null;
		[SerializeField] VoidEvent m_OnItemSwiped = null;

		// Data
		private readonly List<TileItem> Tiles = new();
		private Coroutine MouseChecking = null;
		private bool Dragging = false;

		// Saving
		private SavingString PositionStrs = new("TileLayout.PositionStrs", "");


		#endregion




		#region --- API ---


		public void ReloadTiles () {
			transform.DestroyAllChirldrenImmediate();
			Tiles.Clear();
			foreach (var tileType in typeof(TileItem).GetAllChildClass()) {
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
					SwipeTileToBottom(tile, trigger);
				});
				var drag = grab.Grab<DragToMove>();
				drag.Begin.AddListener(() => OnTileDragBegin(tile));
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
			// Pos
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
				var rt = tile.RectTransform;
				var uiPos = TilePos_to_UIPos(tile.Position, rt.pivot, rt.rect.size, out var anchor);
				rt.anchorMin = rt.anchorMax = anchor;
				rt.anchoredPosition3D = uiPos;
			}
		}


		#endregion




		#region --- LGC ---


		// Mouse Drag
		private void OnTileDragBegin (TileItem tile) {
			if (MouseChecking != null) {
				StopCoroutine(MouseChecking);
			}
			Dragging = true;
			MouseChecking = StartCoroutine(DraggingTile());
			tile.RectTransform.SetAsLastSibling();
			IEnumerator DraggingTile () {
				var rt = tile.RectTransform;
				var pRT = rt.parent as RectTransform;
				var prevPos = rt.anchoredPosition + rt.anchorMin * pRT.rect.size;
				while (Input.GetMouseButton(0)) {
					var pos = rt.anchoredPosition + rt.anchorMin * pRT.rect.size;
					float deltaX = pos.x - prevPos.x;
					float deltaY = pos.y - prevPos.y;
					if (Mathf.Abs(deltaX) > rt.rect.width || Mathf.Abs(deltaY) > rt.rect.height) {
						rt.localScale = Vector3.one * 1.2f;
						m_OnItemDragged?.Invoke();
					}
					prevPos = pos;
					rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one, Time.deltaTime * 20f);
					yield return new WaitForEndOfFrame();
				}
				rt.localScale = Vector3.one;
				Dragging = false;
			}
		}


		private void OnTileDrag (TileItem tile) => FixAnchor(tile);


		private void OnTileDragEnd (TileItem tile) {
			FixAnchor(tile);
			var rt = tile.RectTransform;
			tile.Position = UIPos_to_TilePos(tile.RectTransform.anchoredPosition, rt.pivot, rt.rect.size);
			Dragging = false;
			SaveTilePositions();
		}


		// Logic
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


		private void SwipeTileToBottom (TileItem tile, TriggerUI trigger) {

			if (!trigger.enabled) { return; }

			foreach (var item in Tiles) {
				if (item != tile && item.Position == tile.Position) {
					m_OnItemSwiped?.Invoke();
					StartCoroutine(Swipe());
					return;
				}
			}

			trigger.transform.SetAsFirstSibling();

			// Func
			IEnumerator Swipe () {

				trigger.enabled = false;
				var rt = trigger.transform as RectTransform;
				var oldPos = rt.anchoredPosition;
				float duration = m_SwipeCurve.Duration();
				float startTime = m_SwipeCurve[0].time;
				float rotMulti = Random.Range(-0.3f, 0.3f);

				// Up
				for (float time = 0; time < duration / 2f; time += Time.deltaTime) {
					if (Input.GetMouseButton(0)) { break; }
					float delta = m_SwipeCurve.Evaluate(startTime + time);
					rt.anchoredPosition = oldPos + Vector2.up * delta;
					rt.localScale = Vector3.one * (1f + delta * 0.0016f);
					rt.localRotation = Quaternion.Euler(0, 0, delta * rotMulti);
					yield return new WaitForEndOfFrame();
				}

				// Swipe
				trigger.transform.SetAsFirstSibling();
				rt.localScale = Vector3.one;

				// Down
				for (float time = 0; time < duration / 2f; time += Time.deltaTime) {
					if (Input.GetMouseButton(0)) { break; }
					float delta = m_SwipeCurve.Evaluate(startTime + duration / 2f + time);
					rt.anchoredPosition = oldPos + Vector2.up * delta;
					rt.localRotation = Quaternion.Euler(0, 0, delta * rotMulti);
					yield return new WaitForEndOfFrame();
				}

				rt.anchoredPosition = oldPos;
				rt.localRotation = Quaternion.identity;
				trigger.enabled = true;
			}
		}


		// Position
		private Vector2 TilePos_to_UIPos (Vector2Int tilePos, Vector2 pivot, Vector2 size, out Vector2 anchor) {
			anchor = new Vector2(
				tilePos.x >= 0 ? 0f : 1f,
				tilePos.y >= 0 ? 0f : 1f
			);
			var pivotSize = pivot * size;
			return new Vector2(
				tilePos.x * m_GridSize.x + pivotSize.x,
				tilePos.y * m_GridSize.y + pivotSize.y
			);
		}


		private Vector2Int UIPos_to_TilePos (Vector2 uiPos, Vector2 pivot, Vector2 size) {
			var pos = uiPos - pivot * size;
			return new Vector2Int(
				Mathf.RoundToInt(pos.x / m_GridSize.x),
				Mathf.RoundToInt(pos.y / m_GridSize.y)
			);
		}


		#endregion




	}
}



#if UNITY_EDITOR
namespace YayaMaker.Editor {
	using UnityEditor;
	using global::YayaMaker.UI;
	[CustomEditor(typeof(TileUI))]
	public class TileUI_Inspector : Editor {
		public override void OnInspectorGUI () {
			bool oldW = EditorGUIUtility.wideMode;
			EditorGUIUtility.wideMode = true;
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
			EditorGUIUtility.wideMode = oldW;
		}
	}
}
#endif
