using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class WoodStoneDoorFront : Door {
		public override bool IsFrontDoor => true;
	}
	public class WoodStoneDoorBack : Door {
		public override bool IsFrontDoor => false;
	}
	public class WoodDoorFront : Door {
		public override bool IsFrontDoor => true;
	}
	public class WoodDoorBack : Door {
		public override bool IsFrontDoor => false;
	}


	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	[RequireLanguageFromField]
	public abstract class Door : EnvironmentEntity {


		// Const
		private static readonly LanguageCode HINT_ENTER = ("CtrlHint.EnterDoor", "Enter");

		// Api
		public virtual bool IsFrontDoor => false;

		// Data
		private static bool InputLock = false;
		private bool Open = false;
		private bool PlayerOverlaps = false;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			Open = false;
			PlayerOverlaps = false;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			const int OVERLAP_SHRINK = Const.CEL / 8;
			var player = Player.Selecting;
			if (player != null) {
				PlayerOverlaps =
					player != null &&
					!player.Teleporting &&
					player.IsGrounded &&
					player.Rect.Overlaps(Rect.Shrink(OVERLAP_SHRINK, OVERLAP_SHRINK, 0, 0));

				// Invoke
				if (!InputLock && !player.LockingInput && PlayerOverlaps) {
					if (FrameInput.GameKeyHolding(Gamekey.Up)) {
						Invoke(player);
					}
					ControlHintUI.AddHint(Gamekey.Up, HINT_ENTER);
				}
				if (InputLock && !FrameInput.GameKeyHolding(Gamekey.Up)) {
					InputLock = false;
				}
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();

			int artIndex = Open || PlayerOverlaps ? 1 : 0;
			if (!CellRenderer.TryGetSpriteFromGroup(TypeID, artIndex, out var sprite)) return;

			// Draw
			var cell = CellRenderer.Draw(
				sprite, X + Width / 2, Y, 500, 0, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
			);

			// Z Fix
			if (IsFrontDoor != FrameTask.IsTasking<TeleportTask>()) {
				cell.Z = -cell.Z;
			}
		}


		// API
		public virtual bool Invoke (Player player) {
			if (player == null || FrameTask.HasTask()) return false;
			TeleportTask.Teleport(
				X + Width / 2, Y + Height / 2, X + Width / 2, Y,
				IsFrontDoor ? Stage.ViewZ - 1 : Stage.ViewZ + 1
			);
			player.X = X + Width / 2;
			player.Y = Y;
			Open = true;
			InputLock = true;
			return true;
		}


		public virtual bool AllowInvoke (Entity target) =>
			!FrameTask.HasTask() && target is Character ch &&
			ch.IsGrounded && ch.Rect.y >= Y && !ch.IsSquatting && !ch.IsClimbing;


	}
}