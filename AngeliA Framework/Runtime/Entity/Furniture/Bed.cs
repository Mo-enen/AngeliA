using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Bed : Furniture, IActionTarget {

		protected override Direction3 ModuleType => Direction3.Horizontal;

		public void GetTargetOnBed (Character target) {

			if (target == null) return;
			target.SetCharacterState(CharacterState.Sleep);

			// Get Bed Left and Right
			int xMin = X;
			int xMax = X + Const.CEL;
			if (WorldSquad.Front.FindBlock(
				TypeID, (X - Const.HALF).UDivide(Const.CEL), (Y + Const.HALF).UDivide(Const.CEL),
				Direction4.Left, BlockType.Entity, out int leftX, out _
			)) {
				xMin = leftX * Const.CEL;
			}
			if (WorldSquad.Front.FindBlock(
				TypeID, (X + Const.CEL + Const.HALF).UDivide(Const.CEL), (Y + Const.HALF).UDivide(Const.CEL),
				Direction4.Right, BlockType.Entity, out int rightX, out _
			)) {
				xMax = rightX * Const.CEL + Const.CEL;
			}

			// Get Offset Y
			int offsetY = 0;
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, 0, out var sprite, false, true)) {
				offsetY += sprite.GlobalHeight - sprite.GlobalBorder.Up;
			}

			// Set Character Pos
			target.X = (xMin + xMax) / 2;
			target.Y = Y + offsetY + 2;
		}

		void IActionTarget.Invoke () {
			if (Player.Selecting == null) return;
			GetTargetOnBed(Player.Selecting);
			Player.Selecting.RestartOnFullAsleep = true;
		}

		bool IActionTarget.AllowInvoke () => Player.Selecting != null;

	}
}