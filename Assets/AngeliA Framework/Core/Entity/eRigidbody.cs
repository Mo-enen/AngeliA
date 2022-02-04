using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	public abstract class eRigidbody : Entity {


		public PhysicsLayer Layer = PhysicsLayer.Character;
		public PhysicsMask CollisionMask = PhysicsMask.Level | PhysicsMask.Environment | PhysicsMask.Character;
		public int VelocityX = 0;
		public int VelocityY = 0;
		public int Gravity = 5;
		public int MaxGravitySpeed = 64;
		public int Width = 0;
		public int Height = 0;
		public int OffsetX = 0;
		public int OffsetY = 0;
		public int SpeedScale = 1000;


		// MSG
		public override void FillPhysics (int frame) => CellPhysics.Fill(
			Layer, new(X + OffsetX, Y + OffsetY, Width, Height), this
		);


		public override void PhysicsUpdate (int frame) {

			if (Gravity != 0) {
				VelocityY = Mathf.Clamp(VelocityY - Gravity, -MaxGravitySpeed, int.MaxValue);
			}

			var pos = new Vector2Int(X + OffsetX, Y + OffsetY);
			var newPos = new Vector2Int(
				pos.x + VelocityX * SpeedScale / 1000,
				pos.y + VelocityY * SpeedScale / 1000
			);

			var _pos = CellPhysics.Move(
			   CollisionMask, pos,
			   newPos, new(Width, Height), this,
			   out var hitted, out var _dir
			);

			X = _pos.x - OffsetX;
			Y = _pos.y - OffsetY;

			if (hitted) {
				if (_dir == Direction4.Left || _dir == Direction4.Right) {
					VelocityX = 0;
				} else {
					VelocityY = 0;
				}
			}

		}


	}
}