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
		public bool PhysicsCheck = false;
		private static int PIXEL_CODE = "Pixel".ACode();
		private Color32? PhysicsCheckTint = null;


		public override void FillPhysics () {
			if (Width <= Const.CELL_SIZE && Height <= Const.CELL_SIZE) {
				CellPhysics.Fill(
					Layer, new RectInt(X, Y, Width, Height), this
				);
			}
		}


		public override void FrameUpdate () {
			CellRenderer.Draw(
				SpriteName.ACode(),
				X, Y, 0, 0,
				0, Width, Height, Color
			);
			if (PhysicsCheck) {
				bool success = false;
				CellPhysics.ForAllOverlaps(Layer, new RectInt(X, Y, Width, Height), (_rect, _entity) => {
					if (_entity != this && _entity is eDebug dEntity) {
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
		bool Wrap = true;
		public Color32 BGColor = new(64, 64, 64, 255);
		public Color32 AreaColor = new(255, 255, 255, 64);
		public Color32 Color = new(255, 255, 255, 255);
		public string Content = "±¿±¿Ñ¾ÄÏ¹Ï¹ÏYaYaGuaGua";


		public override void FrameUpdate () {
			CellRenderer.Draw(
				"Pixel".ACode(),
				X, Y, 0, 0,
				0, Width, Height, BGColor
			);
			CellGUI.DrawLabel(
				Content,
				new RectInt(X, Y, Width, Height),
				Color, CharSize, CharSpace, LineSpace, Wrap
			);
		}


	}


}
