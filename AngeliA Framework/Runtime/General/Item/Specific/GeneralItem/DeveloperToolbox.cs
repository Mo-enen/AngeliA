using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class iDeveloperToolbox : Item {




		#region --- SUB ---


		private class BarData {
			public IntToChars I2C;
			public int Value;
			public int Capacity;
		}


		#endregion




		#region --- VAR ---


		private static readonly BarData[] RenderingUsages = new BarData[RenderLayer.COUNT];
		private static readonly BarData[] EntityUsages = new BarData[EntityLayer.COUNT];
		private static BarData[] TextUsages = new BarData[0];
		private static int RequireDataFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		[OnGameInitializeLater(1024)]
		public static void OnGameInitialize () {
			TextUsages = new BarData[CellRenderer.TextLayerCount];
			for (int i = 0; i < RenderingUsages.Length; i++) {
				int capa = CellRenderer.GetLayerCapacity(i);
				RenderingUsages[i] = new BarData() {
					Capacity = capa,
					I2C = new IntToChars($"{CellRenderer.GetLayerName(i)}  ", $" / {capa}"),
				};
			}
			for (int i = 0; i < TextUsages.Length; i++) {
				int capa = CellRenderer.GetTextLayerCapacity(i);
				TextUsages[i] = new BarData() {
					Capacity = capa,
					I2C = new IntToChars($"{CellRenderer.GetTextLayerName(i)}  ", $" / {capa}"),
				};
			}
			for (int i = 0; i < EntityUsages.Length; i++) {
				int capa = Stage.Entities[i].Length;
				EntityUsages[i] = new BarData() {
					Capacity = capa,
					I2C = new IntToChars($"{EntityLayer.LAYER_NAMES[i]}  ", $" / {capa}"),
				};
			}
		}


		[OnGameUpdateLater]
		public static void OnGameUpdateLater () {
			// Collect Data
			if (Game.GlobalFrame <= RequireDataFrame + 1) {
				for (int i = 0; i < RenderLayer.COUNT; i++) {
					RenderingUsages[i].Value = CellRenderer.GetUsedCellCount(i);
				}
				for (int i = 0; i < CellRenderer.TextLayerCount; i++) {
					TextUsages[i].Value = CellRenderer.GetTextUsedCellCount(i);
				}
				for (int i = 0; i < EntityLayer.COUNT; i++) {
					EntityUsages[i].Value = Stage.EntityCounts[i];
				}
			}
		}


		public override void OnItemUpdate_FromInventory (Entity holder) {
			base.OnItemUpdate_FromInventory(holder);
			DrawProfiler();

		}


		private void DrawProfiler () {
			RequireDataFrame = Game.GlobalFrame;
			int panelWidth = CellRendererGUI.Unify(256);
			int barHeight = CellRendererGUI.Unify(24);
			int barPadding = CellRendererGUI.Unify(4);
			int panelHeight = barHeight * (EntityUsages.Length + TextUsages.Length + RenderingUsages.Length);
			var panelRect = new IRect(CellRenderer.CameraRect.xMax - panelWidth, CellRenderer.CameraRect.yMax - panelHeight, panelWidth, panelHeight);
			var rect = new IRect(panelRect.x, panelRect.yMax - barHeight, panelRect.width, barHeight);
			int oldLayer = CellRenderer.CurrentLayerIndex;
			CellRenderer.SetLayerToUI();
			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK, int.MaxValue);
			// Entity
			for (int i = 0; i < EntityUsages.Length; i++) {
				DrawBar(rect.Shrink(barPadding), EntityUsages[i]);
				rect.y -= rect.height;
			}
			// Rendering
			for (int i = 0; i < RenderingUsages.Length; i++) {
				DrawBar(rect.Shrink(barPadding), RenderingUsages[i]);
				rect.y -= rect.height;
			}
			// Text
			for (int i = 0; i < TextUsages.Length; i++) {
				DrawBar(rect.Shrink(barPadding), TextUsages[i]);
				rect.y -= rect.height;
			}
			CellRenderer.SetLayer(oldLayer);
			// Func
			static void DrawBar (IRect rect, BarData data) {
				int width = Util.RemapUnclamped(0, data.Capacity, 0, rect.width, data.Value);
				var tint = Byte4.LerpUnclamped(Const.GREEN, Const.RED_BETTER, (float)data.Value / data.Capacity);
				CellRenderer.Draw(Const.PIXEL, new IRect(rect.x, rect.y, width, rect.height), tint, int.MaxValue);
				// Label
				int startIndex = CellRenderer.GetTextUsedCellCount();
				CellRendererGUI.Label(
					CellContent.Get(data.I2C.GetChars(data.Value), Const.GREY_230, 14, Alignment.MidMid), rect
				);
				if (CellRenderer.GetTextCells(out var cells, out int count)) {
					for (int i = startIndex; i < count && i < startIndex + data.I2C.Prefix.Length; i++) {
						cells[i].Color = new Byte4(96, 96, 96, 255);
					}
				}
			}
		}


		#endregion




	}
}