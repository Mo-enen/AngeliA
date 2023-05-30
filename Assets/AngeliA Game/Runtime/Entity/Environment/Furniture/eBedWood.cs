using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eBedWoodA : eBed, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eBedWoodB : eBed, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public abstract class eBed : Furniture, IActionEntity {

		protected override Direction3 ModuleType => Direction3.Horizontal;

		void IActionEntity.Invoke (Entity target) {

			if (target is not Character ch) return;

			ch.SetCharacterState(CharacterState.Sleep);

			// Get Bed Left and Right
			int xMin = X;
			int xMax = X + Const.CEL;
			if (Game.Current.WorldSquad.FindBlock(
				TypeID, (X - Const.HALF).UDivide(Const.CEL), (Y + Const.HALF).UDivide(Const.CEL),
				Direction4.Left, BlockType.Entity, out int leftX, out _
			)) {
				xMin = leftX * Const.CEL;
			}
			if (Game.Current.WorldSquad.FindBlock(
				TypeID, (X + Const.CEL + Const.HALF).UDivide(Const.CEL), (Y + Const.HALF).UDivide(Const.CEL),
				Direction4.Right, BlockType.Entity, out int rightX, out _
			)) {
				xMax = rightX * Const.CEL + Const.CEL;
			}

			// Get Offset Y
			int offsetY = 0;
			if (CellRenderer.TryGetSprite(TypeID, out var sprite)) {
				offsetY += sprite.GlobalHeight - sprite.GlobalBorder.Up;
			}

			// Set Character Pos
			ch.X = (xMin + xMax) / 2;
			ch.Y = Y + offsetY + 2;
		}

		bool IActionEntity.AllowInvoke (Entity target) => true;

		public override void FrameUpdate () {
			base.FrameUpdate();
			// Make Player Sleep at Opening
			if (
				FrameTask.GetCurrentTask() is OpeningTask oTask &&
				(!oTask.IsFadingOut || Player.Selecting.CharacterState == CharacterState.GamePlay) &&
				Player.Selecting != null &&
				Player.Selecting.Active &&
				Player.Selecting.CharacterState != CharacterState.Sleep
			) {
				(this as IActionEntity).Invoke(Player.Selecting);
				Player.Selecting.Heal(Player.Selecting.MaxHP);
				Player.Selecting.SetAsFullSleep();
			}
		}

	}
}
