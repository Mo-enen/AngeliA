using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eModularFurniture : Entity {



		// VAR
		protected virtual Direction2 Direction { get; } = Direction2.Horizontal;
		protected FurniturePose Pose = FurniturePose.Unknown;


		// MSG
		public override void OnActived (int frame) {
			base.OnActived(frame);
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;
			Pose = FurniturePose.Unknown;
		}


		public override void PhysicsUpdate (int frame) {
			base.PhysicsUpdate(frame);
			if (Pose == FurniturePose.Unknown) {
				if (Direction == Direction2.Horizontal) {
					var rect = new RectInt(Rect.x - Const.CELL_SIZE / 2, Rect.y + Const.CELL_SIZE / 2, 1, 1);
					bool hasLeft = CellPhysics.HasEntity(GetType(), rect, (int)PhysicsMask.Environment, this, OperationMode.TriggerOnly);
					rect.x = Rect.xMax + Const.CELL_SIZE / 2;
					bool hasRight = CellPhysics.HasEntity(GetType(), rect, (int)PhysicsMask.Environment, this, OperationMode.TriggerOnly);
					Pose =
						hasLeft && hasRight ? FurniturePose.Mid :
						!hasLeft && !hasRight ? FurniturePose.Single :
						!hasLeft && hasRight ? FurniturePose.Left :
						FurniturePose.Right;
				} else {
					var rect = new RectInt(Rect.x + Const.CELL_SIZE / 2, Rect.y - Const.CELL_SIZE / 2, 1, 1);
					bool hasDown = CellPhysics.HasEntity(GetType(), rect, (int)PhysicsMask.Environment, this, OperationMode.TriggerOnly);
					rect.y = Rect.yMax + Const.CELL_SIZE / 2;
					bool hasUp = CellPhysics.HasEntity(GetType(), rect, (int)PhysicsMask.Environment, this, OperationMode.TriggerOnly);
					Pose =
						hasDown && hasUp ? FurniturePose.Mid :
						!hasDown && !hasUp ? FurniturePose.Single :
						!hasDown && hasUp ? FurniturePose.Down :
						FurniturePose.Up;
				}
			}
		}


	}
}