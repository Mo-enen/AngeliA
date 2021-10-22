#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Moenen.Standard;


namespace AngeliaFramework.Editor {
	public class CellPhysicsDebuger : EditorWindow {




		#region --- VAR ---





		#endregion




		#region --- MSG ---


		[MenuItem("Tools/Entity Debuger")]
		private static void OpenWindow () {
			var window = GetWindow<CellPhysicsDebuger>(false, "Entity Debuger", true);
			window.minSize = new Vector2(275, 480);
			window.maxSize = new Vector2(1024, 1024);
		}


		private void OnGUI () {
			if (CellRenderer.DebugLayer == null) { InitLayer(); }

			var cell = CellRenderer.DebugLayer.Cells[0];
			cell.ID = 0;
			cell.X = 256;
			cell.Y = 256;
			cell.Width = 256;
			cell.Height = 256;
			cell.PivotX = 0;
			cell.PivotY = 0;
			cell.Color = Color.green;
			CellRenderer.DebugLayer.Cells[0] = cell;
			CellRenderer.DebugLayer.Cells[1].ID = -1;

		}


		#endregion




		#region --- API ---


		public static void Test () {




		}


		#endregion




		#region --- LGC ---


		private void InitLayer () {
			CellRenderer.DebugLayer = new CellRenderer.Layer() {
				Cells = new CellRenderer.Cell[1024],
				CellCount = 1024,
				Material = new Material(Shader.Find("Cell")) { mainTexture = Texture2D.whiteTexture },
				UVs = new Rect[1] { new Rect(0, 0, 1, 1) },
				UVCount = 1,
			};
			CellRenderer.DebugLayer.Cells[0].ID = -1;
		}


		#endregion




	}
}
#endif