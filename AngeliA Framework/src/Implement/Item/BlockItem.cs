using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.ExcludeInMapEditor]
public sealed class BlockItem : Weapon {


	// VAR
	public int BlockID { get; init; }
	public override WeaponType WeaponType => WeaponType.Block;
	public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	public BlockType BlockType { get; init; }
	public override int MaxStackCount => 128;


	// MSG
	public BlockItem (int blockID, string blockName, BlockType blockType) {
		BlockID = blockID;
		BlockType = blockType;
		SpriteID = blockID;
		TypeName = blockName;
	}


	public override void PoseAnimationUpdate_FromEquipment (Entity holder) {

		if (
			holder is not PoseCharacter pHolder ||
			!pHolder.IsAttackAllowedByMovement() ||
			pHolder.CharacterState != CharacterState.GamePlay ||
			PlayerMenuUI.ShowingUI ||
			Task.HasTask() ||
			WorldSquad.Readonly
		) goto _BASE_;



		// Highlight


		// Put Block
		if (Game.GlobalFrame == pHolder.LastAttackFrame) {

		}

		// Base
		_BASE_:;
		base.PoseAnimationUpdate_FromEquipment(holder);
	}


	public override Bullet SpawnBullet (Character sender) => null;


	private void DrawTargetHighlight (int unitX, int unitY, bool allowPut) {
		int border = GUI.Unify(2);
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_HOLLOW_16,
			new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL),
			border, border, border, border,
			allowPut ? Color32.GREY_230 : Color32.WHITE_96, z: int.MaxValue
		);
		if (
			Renderer.TryGetSprite(BlockID, out var sp, true) ||
			Renderer.TryGetSpriteFromGroup(BlockID, 0, out sp)
		) {
			Renderer.Draw(
				sp,
				new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL),
				allowPut ? Color32.GREY_230 : Color32.WHITE_96, z: int.MaxValue
			);
		}
	}


}
