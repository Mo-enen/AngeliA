using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


public abstract class Chair : ActionFurniture {

	// VAR
	protected sealed override Direction3 ModuleType => Direction3.None;
	protected virtual int SitPoseAnimationID => PoseAnimation_Sit.TYPE_ID;
	private bool? DockedToRight = null;
	private Entity SittingTarget = null;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		DockedToRight = null;
		SittingTarget = null;
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Character Sit Check
		if (SittingTarget != null && !SittingTarget.Active) {
			SittingTarget = null;
		}
		if (SittingTarget != null) {
			SittingTarget.X = X + Width / 2;
			SittingTarget.Y = Y + Height - ColliderBorder.up;
			if (SittingTarget is Character character) {
				character.IgnorePhysics.True(1);
				character.IgnoreGravity.True(1);
				character.Movement.Stop();
				character.Movement.LockFacingRight(DockedToRight.HasValue && DockedToRight.Value);
				if (character.Rendering is PoseCharacterRenderer pRen) {
					pRen.ManualPoseAnimate(SitPoseAnimationID, 1);
				}
			}
			if (SittingTarget == PlayerSystem.Selecting) {
				PlayerSystem.IgnoreAction(1);
				PlayerSystem.IgnoreInput(1);
				if (Input.GameKeyDown(Gamekey.Action) || Input.GameKeyDown(Gamekey.Jump) || Input.GameKeyDown(Gamekey.Select)) {
					Input.UseGameKey(Gamekey.Action);
					Input.UseGameKey(Gamekey.Jump);
					Input.UseGameKey(Gamekey.Select);
					SittingTarget = null;
				}
				ControlHintUI.AddHint(Gamekey.Action, BuiltInText.UI_QUIT, priority: 1);
			}
		}
	}

	public override void LateUpdate () {
		if (!DockedToRight.HasValue) {
			DockedToRight = !Physics.HasEntity<Table>(
				Rect.Expand(ColliderBorder).Shift(-Const.CEL, 0).Shrink(1),
				PhysicsMask.ENVIRONMENT, this, OperationMode.ColliderAndTrigger
			);
		}
		// Render
		if (Renderer.TryGetSpriteFromGroup(TypeID, 0, out var sprite, false, true)) {
			var rect = Rect.Expand(ColliderBorder);
			Cell cell;
			if (DockedToRight.HasValue && DockedToRight.Value) {
				cell = Renderer.Draw(sprite, rect);
			} else {
				cell = Renderer.Draw(sprite, rect.CenterX(), rect.y, 500, 0, 0, -rect.width, rect.height);
			}
			IActionTarget.MakeCellAsActionTarget(this, cell);
		}
	}

	public override bool Invoke () {
		var player = PlayerSystem.Selecting;
		if (player == null) return false;
		MakeTargetSit(player);
		return true;
	}

	public override bool AllowInvoke () => base.AllowInvoke() && SittingTarget != PlayerSystem.Selecting;

	public void MakeTargetSit (Entity target) {
		if (SittingTarget != null) return;
		SittingTarget = target;
	}

}
