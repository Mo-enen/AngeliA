using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.MapEditorGroup("Furniture")]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Furniture : Entity, IBlockEntity {




	#region --- VAR ---


	// Api
	protected virtual Direction3 ModuleType => Direction3.None;
	public Furniture FurnitureLeftOrDown { get; private set; } = null;
	public Furniture FurnitureRightOrUp { get; private set; } = null;
	protected FittingPose Pose { get; private set; } = FittingPose.Unknown;

	// Data
	protected Int4 ColliderBorder = Int4.Zero;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Pose = FittingPose.Unknown;
		FurnitureLeftOrDown = null;
		FurnitureRightOrUp = null;
	}


	void IBlockEntity.OnEntityRefresh () {
		X = Rect.CenterX().ToUnifyGlobal();
		Y = Rect.CenterY().ToUnifyGlobal();
		Width = Const.CEL;
		Height = Const.CEL;
		Pose = FittingPose.Unknown;
		FurnitureLeftOrDown = null;
		FurnitureRightOrUp = null;
	}


	public override void FirstUpdate () {
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		Physics.FillBlock(PhysicsLayer.ENVIRONMENT, TypeID, Rect.Shrink(ColliderBorder), true, Tag.OnewayUp);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Update Pose
		if (Pose == FittingPose.Unknown) {
			Pose = FittingPose.Single;

			if (ModuleType != Direction3.None) {
				Pose = FrameworkUtil.GetEntityPose(
					this, ModuleType == Direction3.Horizontal, PhysicsMask.ENVIRONMENT,
					out var ld, out var ru, OperationMode.ColliderAndTrigger
				);
				FurnitureLeftOrDown = ld as Furniture;
				FurnitureRightOrUp = ru as Furniture;
			}

			// Shrink Rect
			var sprite = GetSpriteFromPose();
			if (sprite != null) {
				ColliderBorder = sprite.GlobalBorder;
				X = X.ToUnifyGlobal() - (sprite.GlobalWidth - Const.CEL) / 2;
				Y = Y.ToUnifyGlobal();
				Width = sprite.GlobalWidth;
				Height = sprite.GlobalHeight;
			}
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (Pose == FittingPose.Unknown) return;
		var sprite = GetSpriteFromPose();
		Renderer.Draw(sprite, Rect);
	}


	#endregion




	#region --- API ---


	protected bool GetIsHighlighted () {
		if (PlayerSystem.Selecting == null || PlayerSystem.TargetActionEntity == null) return false;
		var target = PlayerSystem.TargetActionEntity;
		if (target == this) return true;
		for (var f = FurnitureLeftOrDown; f != null; f = f.FurnitureLeftOrDown) {
			if (f == target) return true;
		}
		for (var f = FurnitureRightOrUp; f != null; f = f.FurnitureRightOrUp) {
			if (f == target) return true;
		}
		return false;
	}


	#endregion




	#region --- LGC ---


	protected AngeSprite GetSpriteFromPose () {
		if (Renderer.TryGetSpriteFromGroup(TypeID, Pose switch {
			FittingPose.Left => 1,
			FittingPose.Mid => 2,
			FittingPose.Right => 3,
			FittingPose.Single => 0,
			_ => 0,
		}, out var sprite, false, true) ||
			Renderer.TryGetSprite(TypeID, out sprite)
		) return sprite;
		return null;
	}


	#endregion




}