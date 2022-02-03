using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;


namespace AngeliaFramework.Entities {
	public abstract class eRigidbody : Entity {


		public RectInt Rect => new(X + OffsetX, Y + OffsetY, Width, Height);

		public virtual int PushLevel { get; } = 0;

		public PhysicsLayer Layer = PhysicsLayer.Character;
		public int VelocityX = 0;
		public int VelocityY = 0;
		public int Gravity = 0;
		public int Width = 0;
		public int Height = 0;
		public int OffsetX = 0;
		public int OffsetY = 0;
		public int SpeedScale = 1000;
		public bool CollideWithLevel = true;
		public bool CollideWithEnvironment = true;
		public bool CollideWithItem = false;
		public bool CollideWithCharacter = true;


		// MSG
		public override void FillPhysics (int frame) => CellPhysics.Fill(
			Layer, new(X + OffsetX, Y + OffsetY, Width, Height), this
		);


		public override void PhysicsUpdate (int frame) {

			var pos = new Vector2Int(X + OffsetX, Y + OffsetY);
			var newPos = new Vector2Int(
				pos.x + VelocityX * SpeedScale / 1000,
				pos.y + VelocityY * SpeedScale / 1000
			);

			bool hitted = false;
			var _pos = newPos;
			var _dir = Direction4.Up;

			// Level
			for (int i = 0; i < Const.PHYSICS_LAYER_COUNT && !hitted; i++) {
				if (!CollideCheck((PhysicsLayer)i)) continue;
				hitted = CellPhysics.Move(
				   (PhysicsLayer)i, pos,
				   newPos, new(Width, Height), this,
				   out _pos, out _dir
			   );
			}

			X = _pos.x - OffsetX;
			Y = _pos.y - OffsetY;

			if (hitted) {
				OnHitted(_dir);
			}

		}


		// LGC
		protected virtual void OnHitted (Direction4 hitDirection) { }


		private bool CollideCheck (PhysicsLayer layer) => layer switch {
			PhysicsLayer.Level => CollideWithLevel,
			PhysicsLayer.Environment => CollideWithEnvironment,
			PhysicsLayer.Item => CollideWithItem,
			PhysicsLayer.Character => CollideWithCharacter,
			_ => false,
		};


	}
}