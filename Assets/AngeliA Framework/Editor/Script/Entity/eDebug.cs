using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class eDebug : Entity {


		public PhysicsLayer Layer = PhysicsLayer.Item;
		public int Width = Const.CELL_SIZE;
		public int Height = Const.CELL_SIZE;
		public Color32 Color = new(255, 255, 255, 255);
		public string SpriteName = "Pixel";
		public string Tag = "";
		public bool PhysicsCheck = false;
		public bool IsTrigger = false;

		private static readonly int PIXEL_CODE = "Pixel".ACode();
		private Color32? PhysicsCheckTint = null;


		public override bool Despawnable => false;


		public override void FillPhysics (int frame) {
			if (Width <= Const.CELL_SIZE && Height <= Const.CELL_SIZE) {
				CellPhysics.Fill(
					Layer, new RectInt(X, Y, Width, Height), this, 
					IsTrigger, string.IsNullOrEmpty(Tag) ? 0 : Tag.ACode()
				);
			}
		}


		public override void FrameUpdate (int frame) {
			CellRenderer.Draw(
				SpriteName.ACode(),
				X, Y, 0, 0,
				0, Width, Height, Color
			);
			if (PhysicsCheck) {
				bool success = false;
				CellPhysics.ForAllOverlaps(Layer, new RectInt(X, Y, Width, Height), (info) => {
					if (info.Entity != this && info.Entity is eDebug dEntity) {
						if (!PhysicsCheckTint.HasValue) {
							PhysicsCheckTint = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
						}
						CellRenderer.Draw(
							PIXEL_CODE,
							dEntity.X, dEntity.Y, 0, 0,
							0, dEntity.Width, dEntity.Height, PhysicsCheckTint.Value
						);
						success = true;
					}
					return true;
				});
				if (!success) {
					PhysicsCheckTint = null;
				}
			}
		}


	}


	public class eDebugChar : Entity {

		public int Width = Const.CELL_SIZE * 16;
		public int Height = Const.CELL_SIZE * 4;
		public int CharSize = 256;
		public int CharSpace = 8;
		public int LineSpace = 8;
		public Color32 BGColor = new(64, 64, 64, 255);
		public Color32 AreaColor = new(255, 255, 255, 64);
		public Color32 Color = new(255, 255, 255, 255);
		public string Content = "笨笨丫南瓜瓜YaYaGuaGua";


		public override void FrameUpdate (int frame) {
			CellRenderer.Draw(
				"Pixel".ACode(),
				X, Y, 0, 0,
				0, Width, Height, BGColor
			);
			CellGUI.DrawLabel(
				Content,
				new RectInt(X, Y, Width, Height),
				Color, CharSize, CharSpace, LineSpace, true
			);
		}


	}


}
