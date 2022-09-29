using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eWoodStoneDoorFront : eDoor {
		protected override bool IsFrontDoor => true;
	}


	public class eWoodStoneDoorBack : eDoor {
		protected override bool IsFrontDoor => false;
	}


	public class eWoodDoorFront : eDoor {
		protected override bool IsFrontDoor => true;
	}


	public class eWoodDoorBack : eDoor {
		protected override bool IsFrontDoor => false;
	}


	public abstract class eDoor : Entity {


		protected abstract bool IsFrontDoor { get; }


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (CellRenderer.TryGetSprite(TrimedTypeID, out var sprite)) {
				var cell = CellRenderer.Draw(
					sprite.GlobalID, X + Width / 2, Y, 500, 0, 0,
					Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
				);
				cell.Z = IsFrontDoor ? cell.Z.Abs() : -cell.Z.Abs();
			}
		}

	}
}