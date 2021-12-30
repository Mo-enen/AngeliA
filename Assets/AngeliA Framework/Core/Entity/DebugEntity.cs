using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class DebugEntity : Entity {


		public PhysicsLayer Layer = PhysicsLayer.Item;
		public int Width = Const.CELL_SIZE;
		public int Height = Const.CELL_SIZE;
		public Color32 Color = new(255, 255, 255, 255);
		public string SpriteName = "Pixel";
		public bool PhysicsCheck = false;
		private static int PIXEL_CODE = "Pixel".GetAngeliaHashCode();


		public override void FillPhysics () {
			if (Width < Const.CELL_SIZE && Height < Const.CELL_SIZE) {
				CellPhysics.Fill(
					Layer, new RectInt(X, Y, Width, Height), this
				);
			}
		}


		public override void FrameUpdate () {
			CellRenderer.Draw(
				SpriteName.GetAngeliaHashCode(),
				X, Y, 0, 0,
				0, Width, Height, Color
			);
			if (PhysicsCheck) {
				CellPhysics.ForAllOverlaps(Layer, new RectInt(X, Y, Width, Height), (_rect, _entity) => {
					if (_entity != this && _entity is DebugEntity dEntity) {
						CellRenderer.Draw(
							PIXEL_CODE,
							dEntity.X, dEntity.Y, 0, 0,
							0, dEntity.Width, dEntity.Height, new Color(0, 1, 0, 1f)
						);
					}
					return true;
				});
			}
		}


	}


	public class DebugChar : Entity {

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
				"Pixel".GetAngeliaHashCode(),
				X, Y, 0, 0,
				0, Width, Height, BGColor
			);
			CellRenderer.DrawLabel(
				Content,
				new RectInt(X, Y, Width, Height),
				Color, CharSize, CharSpace, LineSpace, Wrap
			);
		}


	}


}
