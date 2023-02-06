using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {



	public class eWoodStoneDoorFront : eDoor {
		protected override bool IsFrontDoor => true;
		private static readonly int ART_CODE = "WoodStoneDoorFront".AngeHash();
		private static readonly int OPEN_CODE = "WoodStoneDoorFront Open".AngeHash();
		protected override int ArtworkCode => ART_CODE;
		protected override int ArtworkCode_Open => OPEN_CODE;
	}


	public class eWoodStoneDoorBack : eDoor {
		protected override bool IsFrontDoor => false;
		private static readonly int ART_CODE = "WoodStoneDoorBack".AngeHash();
		private static readonly int OPEN_CODE = "WoodStoneDoorBack Open".AngeHash();
		protected override int ArtworkCode => ART_CODE;
		protected override int ArtworkCode_Open => OPEN_CODE;
	}



	public class eWoodDoorFront : eDoor {
		protected override bool IsFrontDoor => true;
		private static readonly int ART_CODE = "WoodDoorFront".AngeHash();
		private static readonly int OPEN_CODE = "WoodDoorFront Open".AngeHash();
		protected override int ArtworkCode => ART_CODE;
		protected override int ArtworkCode_Open => OPEN_CODE;
	}
	public class eWoodDoorBack : eDoor {
		protected override bool IsFrontDoor => false;
		private static readonly int ART_CODE = "WoodDoorBack".AngeHash();
		private static readonly int OPEN_CODE = "WoodDoorBack Open".AngeHash();
		protected override int ArtworkCode => ART_CODE;
		protected override int ArtworkCode_Open => OPEN_CODE;
	}



	[EntityAttribute.DrawBehind]
	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	public abstract class eDoor : Entity {


		// Api
		protected virtual int ArtworkCode => TypeID;
		protected virtual int ArtworkCode_Open => TypeID;
		protected virtual bool IsFrontDoor => false;

		// Data
		private bool Open = false;
		private static bool InputLock = false;


		// MSG
		public override void OnActived () {
			base.OnActived();
			Height = Const.CEL * 2;
			Open = false;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {

			base.FrameUpdate();

			const int OVERLAP_SHRINK = Const.CEL / 8;
			var player = ePlayer.Selecting;
			bool playerOverlaps =
				player != null &&
				player.IsGrounded &&
				player.Rect.Overlaps(Rect.Shrink(OVERLAP_SHRINK, OVERLAP_SHRINK, 0, 0));

			int artCode = Open || playerOverlaps ? ArtworkCode_Open : ArtworkCode;
			if (!CellRenderer.TryGetSprite(artCode, out var sprite)) return;

			// Draw
			var cell = CellRenderer.Draw(
				sprite.GlobalID, X + Width / 2, Y, 500, 0, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
			);

			// Z Fix
			if (IsFrontDoor != FrameTask.IsTasking<TeleportTask>()) {
				cell.Z = -cell.Z;
			}

			// Invoke
			if (!InputLock && playerOverlaps && FrameInput.GameKeyHolding(Gamekey.Up)) {
				Invoke(player);
			}
			if (InputLock && !FrameInput.GameKeyHolding(Gamekey.Up)) {
				InputLock = false;
			}
		}


		// API
		public bool Invoke (ePlayer player) {
			if (player == null || FrameTask.HasTask(YayaConst.TASK_ROUTE)) return false;
			var game = Game.Current;
			player.X = X + (Width - player.Width) / 2 - player.OffsetX;
			player.Y = Y;
			player.Stop();
			game.Teleport(
				player.X, player.Y + player.Height / 2,
				player.X, player.Y + player.Height / 2,
				IsFrontDoor ? game.ViewZ - 1 : game.ViewZ + 1,
				YayaConst.TASK_ROUTE
			);
			player.RenderEnterDoor(game.WorldConfig.SquadTransitionDuration, IsFrontDoor);
			Open = true;
			InputLock = true;
			return true;
		}


		public bool AllowInvoke (Entity target) =>
			!FrameTask.HasTask(YayaConst.TASK_ROUTE) && target is eCharacter ch &&
			ch.IsGrounded && ch.Rect.y >= Y && !ch.IsSquating && !ch.IsClimbing;


	}
}