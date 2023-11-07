using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class WoodStoneDoorFront : Door {
		protected override bool IsFrontDoor => true;
	}
	public class WoodStoneDoorBack : Door {
		protected override bool IsFrontDoor => false;
	}
	public class WoodDoorFront : Door {
		protected override bool IsFrontDoor => true;
	}
	public class WoodDoorBack : Door {
		protected override bool IsFrontDoor => false;
	}


	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	public abstract class Door : Entity {


		// Const
		private static readonly int HINT_ENTER = "CtrlHint.EnterDoor".AngeHash();

		// Api
		protected virtual bool IsFrontDoor => false;
		protected virtual bool TouchToEnter => false;
		protected virtual bool UsePortalEffect => false;
		protected virtual Vector3Int TargetGlobalPosition => new(
			X + Width / 2, Y, IsFrontDoor ? Stage.ViewZ - 1 : Stage.ViewZ + 1
		);

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
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			const int OVERLAP_SHRINK = Const.CEL / 8;
			var player = Player.Selecting;
			PlayerOverlaps =
				player != null &&
				!player.EnteringDoor &&
				(TouchToEnter || player.IsGrounded) &&
				player.Rect.Overlaps(Rect.Shrink(OVERLAP_SHRINK, OVERLAP_SHRINK, 0, 0));

			// Invoke
			if (!InputLock && !player.LockingInput && PlayerOverlaps) {
				if (TouchToEnter || FrameInput.GameKeyHolding(Gamekey.Up)) {
					if (TouchToEnter) {
						Stage.MarkAsGlobalAntiSpawn(this);
					}
					Invoke(player);
				}
				ControlHintUI.AddHint(Gamekey.Up, Language.Get(HINT_ENTER, "Entre"));
			}
			if (InputLock && !FrameInput.GameKeyHolding(Gamekey.Up)) {
				InputLock = false;
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();

			int artIndex = Open || PlayerOverlaps ? 1 : 0;
			if (!CellRenderer.TryGetSpriteFromGroup(TypeID, artIndex, out var sprite)) return;

			// Draw
			var cell = CellRenderer.Draw(
				sprite.GlobalID, X + Width / 2, Y, 500, 0, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
			);
			AngeUtil.DrawShadow(sprite.GlobalID, cell);

			// Z Fix
			if (IsFrontDoor != FrameTask.IsTasking<TeleportTask>()) {
				cell.Z = -cell.Z;
			}
		}


		// API
		public virtual bool Invoke (Player player) {
			if (player == null || FrameTask.HasTask()) return false;
			int fromX = X + (Width - player.Width) / 2 - player.OffsetX;
			if (!UsePortalEffect) {
				player.X = fromX;
				player.Y = Y;
			}
			player.Stop();
			var task = TeleportTask.Teleport(
				fromX, Y + player.Height / 2,
				TargetGlobalPosition.x, TargetGlobalPosition.y, TargetGlobalPosition.z
			);
			if (task != null) {
				task.TeleportEntity = player;
				task.UsePortalEffect = UsePortalEffect;
				if (UsePortalEffect) {
					task.WaitDuration = 30;
					task.Duration = 60;
					task.UseVignette = true;
				}
			}
			player.EnterDoor(task.Duration, IsFrontDoor);
			Open = true;
			InputLock = true;
			return true;
		}


		public virtual bool AllowInvoke (Entity target) =>
			!FrameTask.HasTask() && target is Character ch &&
			ch.IsGrounded && ch.Rect.y >= Y && !ch.IsSquatting && !ch.IsClimbing;


	}
}