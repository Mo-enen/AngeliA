using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	public class eRigidbody : Entity {


		public int VelocityX { get; set; } = 0;
		public int VelocityY { get; set; } = 0;
		public int Gravity { get; set; } = 0;
		public int Width { get; set; } = 0;
		public int Height { get; set; } = 0;
		public int OffsetX { get; set; } = 0;
		public int OffsetY { get; set; } = 0;
		public int SpeedScale { get; set; } = 1000;


		public override void FillPhysics (int frame) => CellPhysics.Fill(
			PhysicsLayer.Character, new(X + OffsetX, Y + OffsetY, Width, Height), this
		);


		public override void FrameUpdate (int frame) {
			Update_ApplyPhysics();
		}


		private void Update_ApplyPhysics () {
			var pos = new Vector2Int(X + OffsetX, Y + OffsetY);
			var newPos = new Vector2Int(
				pos.x + VelocityX * SpeedScale / 1000,
				pos.y + VelocityY * SpeedScale / 1000
			);
			bool hitted = CellPhysics.Move(
				PhysicsLayer.Level, pos,
				newPos, new(Width, Height), this,
				out var _pos, out var _dir
			) || CellPhysics.Move(
				PhysicsLayer.Object, pos,
				newPos, new(Width, Height), this,
				out _pos, out _dir
			);
			X = _pos.x - OffsetX;
			Y = _pos.y - OffsetY;
			if (hitted) {
				if (_dir == Direction2.Horizontal) {
					VelocityX = 0;
				} else {
					VelocityY = 0;
				}
			}
		}


	}
}