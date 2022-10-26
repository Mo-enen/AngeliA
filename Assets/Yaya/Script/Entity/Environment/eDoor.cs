using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {



	public class eWoodStoneDoorFront : eDoor {
		protected override bool IsFrontDoor => true;
		private static readonly int ART_CODE = "WoodStoneDoor".AngeHash();
		private static readonly int OPEN_CODE = "WoodStoneDoor Open".AngeHash();
		protected override int ArtworkCode => ART_CODE;
		protected override int ArtworkCode_Open => OPEN_CODE;
	}
	public class eWoodStoneDoorBack : eDoor {
		protected override bool IsFrontDoor => false;
		private static readonly int ART_CODE = "WoodStoneDoor".AngeHash();
		private static readonly int OPEN_CODE = "WoodStoneDoor Open".AngeHash();
		protected override int ArtworkCode => ART_CODE;
		protected override int ArtworkCode_Open => OPEN_CODE;
	}



	public class eWoodDoorFront : eDoor {
		protected override bool IsFrontDoor => true;
		private static readonly int ART_CODE = "WoodDoor".AngeHash();
		private static readonly int OPEN_CODE = "WoodDoor Open".AngeHash();
		protected override int ArtworkCode => ART_CODE;
		protected override int ArtworkCode_Open => OPEN_CODE;
	}
	public class eWoodDoorBack : eDoor {
		protected override bool IsFrontDoor => false;
		private static readonly int ART_CODE = "WoodDoor".AngeHash();
		private static readonly int OPEN_CODE = "WoodDoor Open".AngeHash();
		protected override int ArtworkCode => ART_CODE;
		protected override int ArtworkCode_Open => OPEN_CODE;
	}



	[EntityAttribute.DrawBehind]
	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	public abstract class eDoor : Entity, IActionEntity {


		// Api
		public GameKey InvokeKey => GameKey.Up;
		public int HighlightFrame { get; set; } = int.MinValue;
		public int HighlightStartFrame { get; set; } = int.MinValue;
		protected virtual int ArtworkCode => TypeID;
		protected virtual int ArtworkCode_Open => TypeID;
		protected virtual bool IsFrontDoor => false;

		// Data
		private bool Open = false;


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
			// Draw
			var player = Yaya.Current.CurrentPlayer;
			int artCode = Open ? ArtworkCode_Open : ArtworkCode;
			if (CellRenderer.TryGetSprite(artCode, out var sprite)) {
				var cell = CellRenderer.Draw(
					sprite.GlobalID, X + Width / 2, Y, 500, 0, 0,
					Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
				);
				// Highlight
				var iAct = this as IActionEntity;
				if (!Open && player != null && player.Action.CurrentTarget == this && iAct.IsHighlighted) {
					IActionEntity.HighlightBlink(cell, iAct);
				}
			}
		}


		// API
		public bool Invoke (Entity target) {
			if (target is not eCharacter ch) return false;
			if (FrameStep.HasStep(YayaConst.STEP_ROUTE)) return false;
			ch.X = X + (Width - ch.Width) / 2 - ch.OffsetX;
			ch.Y = Y;
			ch.Movement.Stop();
			Yaya.Current.SetViewZDelay(IsFrontDoor ? Game.Current.ViewZ - 1 : Game.Current.ViewZ + 1);
			Open = true;
			return true;
		}


		public void CancelInvoke (Entity target) { }


		public bool AllowInvoke (Entity target) => !FrameStep.HasStep(YayaConst.STEP_ROUTE) && target is eCharacter ch && ch.IsGrounded && ch.Rect.y >= Y && !ch.Movement.IsSquating && !ch.Movement.IsClimbing;


	}
}