using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;

[EntityAttribute.MapEditorGroup("Furniture")]
[EntityAttribute.Capacity(32)]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Furniture : Entity, IBlockEntity, IActionTarget {




	#region --- VAR ---


	// Api
	protected virtual Direction3 ModuleType => Direction3.None;
	protected virtual IRect RenderingRect => Rect.Expand(ColliderBorder);
	public Furniture FurnitureLeftOrDown { get; private set; } = null;
	public Furniture FurnitureRightOrUp { get; private set; } = null;
	protected FittingPose Pose { get; private set; } = FittingPose.Unknown;
	bool IActionTarget.IsHighlighted => GetIsHighlighted();

	// Data
	protected Int4 ColliderBorder = Int4.zero;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Pose = FittingPose.Unknown;
		FurnitureLeftOrDown = null;
		FurnitureRightOrUp = null;
	}


	public override void FirstUpdate () {
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Tag.OnewayUp);
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
				if (ColliderBorder != Int4.zero) {
					int rWidth = sprite.GlobalWidth;
					int rHeight = sprite.GlobalHeight;
					var rect = new IRect(X - (rWidth - Const.CEL) / 2, Y, rWidth, rHeight).Shrink(ColliderBorder);
					X = rect.x;
					Y = rect.y;
					Width = rect.width;
					Height = rect.height;
				}
			}
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (Pose == FittingPose.Unknown) return;
		var sprite = GetSpriteFromPose();
		if (sprite != null) {
			var cell = Renderer.Draw(sprite, RenderingRect);
			if ((this as IActionTarget).IsHighlighted) {
				BlinkCellAsFurniture(cell);
			}
		}
	}


	#endregion




	#region --- API ---


	bool IActionTarget.Invoke () => false;


	bool IActionTarget.AllowInvoke () => false;


	protected void DrawClockHands (IRect rect, int handCode, int thickness, int thicknessSecond) {
		var now = System.DateTime.Now;
		// Sec
		Renderer.Draw(
			handCode, rect.x + rect.width / 2, rect.y + rect.height / 2,
			500, 0, now.Second * 360 / 60,
			thicknessSecond, rect.height * 900 / 2000
		);
		// Min
		Renderer.Draw(
			handCode, rect.x + rect.width / 2, rect.y + rect.height / 2,
			500, 0, now.Minute * 360 / 60,
			thickness, rect.height * 800 / 2000
		);
		// Hour
		Renderer.Draw(
			handCode, rect.x + rect.width / 2, rect.y + rect.height / 2,
			500, 0, (now.Hour * 360 / 12) + (now.Minute * 360 / 12 / 60),
			thickness, rect.height * 400 / 2000
		);
	}


	protected void DrawClockPendulum (int artCodeLeg, int artCodeHead, int x, int y, int length, int thickness, int headSize, int maxRot, int deltaX = 0) {
		float t11 = Util.Sin(Game.GlobalFrame * 6 * Util.Deg2Rad);
		int rot = (t11 * maxRot).RoundToInt();
		int dX = -(t11 * deltaX).RoundToInt();
		// Leg
		Renderer.Draw(artCodeLeg, x + dX, y, 500, 1000, rot, thickness, length);
		// Head
		Renderer.Draw(
			artCodeHead, x + dX, y, 500,
			500 * (headSize / 2 + length) / (headSize / 2),
			rot, headSize, headSize
		);
	}


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


	protected void BlinkCellAsFurniture (Cell cell) {
		float pivotX = 0.5f;
		if (ModuleType == Direction3.Horizontal) {
			if (Pose == FittingPose.Left) {
				pivotX = 1f;
			} else if (Pose == FittingPose.Right) {
				pivotX = 0f;
			}
		}
		bool useHorizontal = ModuleType != Direction3.Horizontal || Pose != FittingPose.Mid;
		bool useVertical = ModuleType != Direction3.Vertical || Pose == FittingPose.Up;
		(this as IActionTarget).BlinkIfHighlight(cell, pivotX, 0f, useHorizontal, useVertical);
	}


	#endregion




}