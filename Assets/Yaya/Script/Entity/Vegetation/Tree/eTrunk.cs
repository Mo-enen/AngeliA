using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class eTrunkPalm : eTrunk {
		protected override string TrunkCode => "Trunk Palm";
		protected override Direction3 GetDirection () {
			base.GetDirection();
			return Direction3.Vertical;
		}
	}


	public class eTrunkDark : eTrunk {
		protected override string TrunkCode => "Trunk Dark";
	}


	public class eTrunkNormal : eTrunk {
		protected override string TrunkCode => "Trunk";
	}


	[EntityBounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE)]
	[MapEditorGroup("Vegetation")]
	[EntityCapacity(256)]
	[DrawBehind]
	public abstract class eTrunk : Entity {


		protected abstract string TrunkCode { get; }

		protected int TrunkArtworkCode { get; private set; } = 0;
		protected bool HasTrunkOnLeft { get; private set; } = false;
		protected bool HasTrunkOnRight { get; private set; } = false;
		protected bool HasTrunkOnBottom { get; private set; } = false;
		protected bool HasTrunkOnTop { get; private set; } = false;
		protected Direction3 Direction { get; private set; } = Direction3.None;



		public override void OnActived () {
			base.OnActived();
			HasTrunkOnLeft = false;
			HasTrunkOnRight = false;
			HasTrunkOnBottom = false;
			HasTrunkOnTop = false;
			Direction = Direction3.None;
			TrunkArtworkCode = CellRenderer.TryGetSpriteFromGroup(
				TrunkCode.AngeHash(), (X * 3 + Y * 11) / Const.CELL_SIZE, out var tSprite
			) ? tSprite.GlobalID : 0;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (Direction == Direction3.None) Direction = GetDirection();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			bool vertical = Direction == Direction3.Vertical;
			CellRenderer.Draw(
				TrunkArtworkCode,
				X, vertical ? Y : Y + Const.CELL_SIZE, 0, 0,
				vertical ? 0 : 90, Width, Height
			);
		}


		protected virtual Direction3 GetDirection () {
			HasTrunkOnLeft = false;
			HasTrunkOnRight = false;
			HasTrunkOnBottom = false;
			HasTrunkOnTop = false;
			int h = 0, v = 0;
			if (HasTrunkOnLeft = CellPhysics.HasEntity<eTrunk>(
				new(X - Const.CELL_SIZE / 2, Y + Const.CELL_SIZE / 2, 1, 1),
				YayaConst.MASK_ENVIRONMENT, this, OperationMode.ColliderOnly
			)) h++;
			if (HasTrunkOnRight = CellPhysics.HasEntity<eTrunk>(
				new(X + Const.CELL_SIZE + Const.CELL_SIZE / 2, Y + Const.CELL_SIZE / 2, 1, 1),
				YayaConst.MASK_ENVIRONMENT, this, OperationMode.ColliderOnly
			)) h++;
			if (HasTrunkOnBottom = CellPhysics.HasEntity<eTrunk>(
				new(X + Const.CELL_SIZE / 2, Y - Const.CELL_SIZE / 2, 1, 1),
				YayaConst.MASK_ENVIRONMENT, this, OperationMode.ColliderOnly
			)) v++;
			if (HasTrunkOnTop = CellPhysics.HasEntity<eTrunk>(
				new(X + Const.CELL_SIZE / 2, Y + Const.CELL_SIZE + Const.CELL_SIZE / 2, 1, 1),
				YayaConst.MASK_ENVIRONMENT, this, OperationMode.ColliderOnly
			)) v++;
			if (!HasTrunkOnLeft && !HasTrunkOnRight && !HasTrunkOnBottom && !HasTrunkOnTop) return Direction3.Vertical;
			return h >= v ? Direction3.Horizontal : Direction3.Vertical;
		}


	}
}
