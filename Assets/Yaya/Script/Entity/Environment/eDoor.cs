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
	[EntityAttribute.EntityBounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	public abstract class eDoor : Entity, IActionEntity {


		// Api
		public GameKey InvokeKey => GameKey.Up;
		public int HighlightFrame { get; set; } = int.MinValue;
		public int HighlightStartFrame { get; set; } = int.MinValue;
		protected virtual int ArtworkCode => TypeID;
		protected virtual int ArtworkCode_Open => TypeID;

		// Data
		protected abstract bool IsFrontDoor { get; }


		// MSG
		public override void OnActived () {
			base.OnActived();
			Height = Const.CELL_SIZE * 2;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// Draw
			var player = CellPhysics.GetEntity<ePlayer>(Rect, YayaConst.MASK_CHARACTER, this);
			int artCode = player != null && player.IsGrounded ? ArtworkCode_Open : ArtworkCode;
			if (CellRenderer.TryGetSprite(artCode, out var sprite)) {
				var cell = CellRenderer.Draw(
					sprite.GlobalID, X + Width / 2, Y, 500, 0, 0,
					Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
				);
				cell.Z = IsFrontDoor ? cell.Z.Abs() : -cell.Z.Abs();
				// Highlight
				var iAct = this as IActionEntity;
				if (iAct.IsHighlighted) {
					IActionEntity.HighlightBlink(cell, iAct);
				}
			}
		}


		// API
		public bool Invoke (Entity target) {
			if (
				target is not eCharacter ch ||
				!ch.IsGrounded ||
				ch.Movement.IsSquating ||
				ch.Movement.IsClimbing
			) return false;
			ch.X = X + (Width - ch.Width) / 2 - ch.OffsetX;
			ch.Y = Y;
			Game.Current.SetViewZ(IsFrontDoor ? Game.Current.ViewZ - 1 : Game.Current.ViewZ + 1);
			return true;
		}


		public bool CancelInvoke (Entity target) => false;


	}
}