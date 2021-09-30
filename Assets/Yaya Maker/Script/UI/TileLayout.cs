using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UIGadget;
using System.Reflection;

namespace YayaMaker.UI {


	public abstract class TileItem {

		public Text Label { get; set; }
		public Image Icon { get; set; }

		public void RefreshLabel () => Label.text = GetDisplayName();
		public void RefreshIcon () => Icon.sprite = GetIcon();

		public abstract string GetDisplayName ();
		public abstract Sprite GetIcon ();
		public virtual void OnLeftClick () { }
		public virtual void OnRightClick () { }
		public virtual void OnDoubleClick () { }

	}


	public class TestA : TileItem {
		public override string GetDisplayName () {
			throw new System.NotImplementedException();
		}

		public override Sprite GetIcon () {
			throw new System.NotImplementedException();
		}
		public override void OnLeftClick () {
			Debug.Log("Left A");
		}
		public override void OnRightClick () {
			Debug.Log("Right A");
		}
	}

	public abstract class TestC : TileItem {
		public override string GetDisplayName () {
			throw new System.NotImplementedException();
		}

		public override Sprite GetIcon () {
			throw new System.NotImplementedException();
		}
		public override void OnLeftClick () {
			Debug.Log("Left C");
		}
	}

	public class TestB : TileItem {

		public class TestBB : TileItem {
			public override string GetDisplayName () {
				throw new System.NotImplementedException();
			}

			public override Sprite GetIcon () {
				throw new System.NotImplementedException();
			}
			public override void OnLeftClick () {
				Debug.Log("Left BB");
			}
		}

		public override string GetDisplayName () {
			throw new System.NotImplementedException();
		}

		public override Sprite GetIcon () {
			throw new System.NotImplementedException();
		}
		public override void OnLeftClick () {
			Debug.Log("Left B");
		}
	}


	public class TileLayout : MonoBehaviour {




		#region --- SUB ---





		#endregion




		#region --- VAR ---


		// Ser
		[SerializeField] Grabber m_Template = null;

		// Data
		private readonly List<TileItem> Tiles = new List<TileItem>();


		#endregion




		#region --- MSG ---




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
				var tile = System.Activator.CreateInstance(tileType) as TileItem;
				Tiles.Add(tile);
				var grab = Util.SpawnItemUI(m_Template, transform as RectTransform, tileType.Name);

				var trigger = grab.Grab<TriggerUI>();
				trigger.CallbackLeft.AddListener(tile.OnLeftClick);
				trigger.CallbackRight.AddListener(tile.OnRightClick);
				trigger.CallbackDoubleClick.AddListener(tile.OnDoubleClick);

				tile.Label = grab.Grab<Text>("Label");
				tile.Icon = grab.Grab<Image>("Icon");

				tile.RefreshLabel();
				tile.RefreshIcon();

			}
			RefreshTilePosition();
		}


		public void RefreshTilePosition () {



		}




		#endregion




		#region --- LGC ---




		#endregion




		#region --- UTL ---




		#endregion




	}
}
