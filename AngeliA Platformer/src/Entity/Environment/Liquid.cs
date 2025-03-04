using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.Layer(EntityLayer.WATER)]
public abstract class Liquid : Entity {




	#region --- SUB ---


	private enum BlockType { Empty, Block, Water, }


	#endregion




	#region --- VAR ---


	// Api
	public int Volume { get; private set; } = 1000;
	protected virtual int ArtworkID => TypeID;
	protected virtual int ProduceID => TypeID;
	protected virtual int ReproduceVolume => 200;
	protected virtual int ReproduceFrequency => 5;
	protected virtual int VanishSpeed => 100;

	// Data
	private int GlobalX;
	private int GlobalY;
	private Liquid Source;
	private bool Vanishing;
	private BlockType BlockL;
	private BlockType BlockR;
	private BlockType BlockD;
	private BlockType BlockU;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		GlobalX = X = X.ToUnifyGlobal();
		GlobalY = Y = Y.ToUnifyGlobal();
		BlockL = BlockType.Empty;
		BlockR = BlockType.Empty;
		BlockD = BlockType.Empty;
		BlockU = BlockType.Empty;
		Volume = 1000;
		Source = null;
		Vanishing = false;
	}


	// Physics 
	public override void FirstUpdate () {
		base.FirstUpdate();
		X = GlobalX;
		Y = GlobalY;
		Height = BlockU != BlockType.Water ? Volume * Const.CEL / 1000 : Const.CEL;
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Tag.Water);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		if (!Vanishing) {
			// Vanish Check
			if (RequireVanishCheck()) {
				Vanishing = true;
			}
			// Blocks
			BlockL = GetBlockAt(X - Const.CEL, Y);
			BlockR = GetBlockAt(X + Const.CEL, Y);
			BlockD = GetBlockAt(X, Y - Const.CEL);
			BlockU = GetBlockAt(X, Y + Const.CEL);
			// Reproduce
			if (Game.SettleFrame % ReproduceFrequency == 0 && Game.GlobalFrame != SpawnFrame) {
				IterateForReproduce();
			}
		} else {
			// Vanishing
			Volume = (Volume - VanishSpeed).GreaterOrEquelThanZero();
			if (Volume <= 0) {
				Active = false;
			}
		}
	}


	protected virtual bool RequireVanishCheck () {
		if (FromWorld) return false;
		if (Source == null || !Source.Active) return true;
		int sourceUnitX = Source.X.ToUnit();
		int sourceUnitY = Source.Y.ToUnit();
		int unitX = X.ToUnit();
		int unitY = Y.ToUnit();
		if ((unitX - sourceUnitX).Abs() > 1 || (unitY - sourceUnitY).Abs() > 1) return true;
		return false;
	}


	// Rendering
	public override void LateUpdate () {

		base.LateUpdate();

		if (!Active) return;
		if (!Renderer.TryGetSpriteGroup(ArtworkID, out var group)) return;
		if (!Renderer.CurrentSheet.TryGetSpriteFromAnimationFrame(group, Game.GlobalFrame, out var sprite)) return;

		int spMidWidth = sprite.GlobalWidth - sprite.GlobalBorder.horizontal;
		int spMidHeight = sprite.GlobalHeight - sprite.GlobalBorder.vertical;
		int leftBorderShift = sprite.GlobalBorder.left * Const.CEL / spMidWidth;
		int rightBorderShift = sprite.GlobalBorder.right * Const.CEL / spMidWidth;
		int downBorderShift = sprite.GlobalBorder.down * Const.CEL / spMidHeight;
		int upBorderShift = sprite.GlobalBorder.up * Const.CEL / spMidHeight;
		var expandRect = new IRect(X, Y, Const.CEL, Const.CEL).Expand(leftBorderShift, rightBorderShift, downBorderShift, upBorderShift);
		int sideShrinkL = leftBorderShift;
		int sideShrinkR = rightBorderShift;

		// Draw Mid
		var cell = Renderer.Draw(sprite, expandRect);
		cell.Shift.left += leftBorderShift;
		cell.Shift.right += rightBorderShift;
		cell.Shift.down += downBorderShift;
		cell.Shift.up += upBorderShift;

		// Draw Top
		Cell cellTop = null;
		if (BlockU == BlockType.Empty || (BlockU == BlockType.Block && Volume < 1000)) {
			cellTop = Renderer.Draw(sprite, expandRect);
			cellTop.Shift.left += leftBorderShift;
			cellTop.Shift.right += rightBorderShift;
			cellTop.Shift.down = downBorderShift + Const.CEL;
			cellTop.Y -= upBorderShift;
			cell.Height -= upBorderShift;
		}

		// Draw Bottom
		if (BlockD == BlockType.Empty) {
			cell.Shift.down = 0;
		}

		// Draw Side L
		if (BlockL == BlockType.Empty) {
			cell.Shift.left = 0;
			cell.X += sideShrinkL;
			cell.Width -= sideShrinkL;
			if (cellTop != null) {
				cellTop.Shift.left = 0;
				cellTop.X += sideShrinkL;
				cellTop.Width -= sideShrinkL;
			}
		}

		// Draw Side R
		if (BlockR == BlockType.Empty) {
			cell.Shift.right = 0;
			cell.Width -= sideShrinkR;
			if (cellTop != null) {
				cellTop.Shift.right = 0;
				cellTop.Width -= sideShrinkR;
			}
		}

		// Shift for Volume
		if (BlockU != BlockType.Water) {
			int shift = (1000 - Volume) * Const.CEL / 1000;
			cell.Shift.up += shift;
			if (cellTop != null) {
				cellTop.Y -= shift;
			}
			// Over-Shifted
			if (cell.Shift.vertical > cell.Height) {
				cell.Color = Color32.CLEAR;
				cellTop.Shift.down += cell.Shift.vertical - cell.Height;
			}
		}
	}


	#endregion




	#region --- LGC ---


	private BlockType GetBlockAt (int x, int y) {
		const int PADDING = 1;
		const int PADDING_2 = PADDING * 2;
		var rect = new IRect(x + PADDING, y + PADDING, Const.CEL - PADDING_2, Const.CEL - PADDING_2);
		return
			Physics.Overlap(PhysicsMask.LEVEL, rect, this, OperationMode.ColliderOnly) ? BlockType.Block :
			Physics.GetEntity(TypeID, rect, PhysicsMask.MAP, this, OperationMode.TriggerOnly, Tag.Water) != null ? BlockType.Water : 
			BlockType.Empty;
	}


	private void IterateForReproduce () {
		if (BlockD == BlockType.Empty) {
			// Reproduce Down
			if (Stage.SpawnEntity(ProduceID, X, Y - Const.CEL) is Liquid water) {
				BlockD = BlockType.Water;
				water.Volume = 1000;
				water.FirstUpdate();
				water.Source = this;
			}
		} else if (BlockD == BlockType.Block) {
			// Reproduce Side
			if (Volume > ReproduceVolume) {
				if (BlockL == BlockType.Empty && Stage.SpawnEntity(ProduceID, X - Const.CEL, Y) is Liquid waterL) {
					BlockL = BlockType.Water;
					waterL.Volume = Volume / 2;
					waterL.FirstUpdate();
					waterL.Source = this;
				}
				if (BlockR == BlockType.Empty && Stage.SpawnEntity(ProduceID, X + Const.CEL, Y) is Liquid waterR) {
					BlockR = BlockType.Water;
					waterR.Volume = Volume / 2;
					waterR.FirstUpdate();
					waterR.Source = this;
				}
			}
		}
	}


	#endregion



}