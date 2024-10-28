using System.Collections;
using System.Collections.Generic;


using AngeliA;
namespace AngeliA.Platformer;


[EntityAttribute.MapEditorGroup(nameof(Platform))]
[EntityAttribute.Capacity(128)]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Platform : Entity, IBlockEntity {


	// Api
	public abstract bool OneWay { get; }

	// Short
	protected bool TouchedByPlayer { get; private set; } = false;
	protected bool TouchedByCharacter { get; private set; } = false;
	protected bool TouchedByRigidbody { get; private set; } = false;
	protected FittingPose Pose { get; private set; } = FittingPose.Unknown;

	// Data
	private int LastMoveFrame = -2;
	private int PrevX = 0;
	private int PrevY = 0;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		TouchedByPlayer = false;
		TouchedByCharacter = false;
		TouchedByRigidbody = false;
		PrevX = X;
		PrevY = Y;
		LastMoveFrame = -1;
		Pose = FrameworkUtil.GetEntityPose(TypeID, X.ToUnit(), Y.ToUnit(), true);
	}


	public override void FirstUpdate () {
		if (OneWay) {
			Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Tag.OnewayUp);
		} else {
			Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
		}
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		PrevX = X;
		PrevY = Y;
		Move();
		if (Game.GlobalFrame != LastMoveFrame + 1) {
			PrevX = X;
			PrevY = Y;
		}
		ICarrier.CarryTargetsOnTopHorizontally(this, X - PrevX, OperationMode.ColliderAndTrigger);
		ICarrier.CarryTargetsOnTopVertically(this, Y - PrevY, OneWay, OperationMode.ColliderAndTrigger);
		LastMoveFrame = Game.GlobalFrame;
		Update_PushX();
	}


	public override void Update () {
		base.Update();
		Update_Touch();
	}


	public override void LateUpdate () {
		base.LateUpdate();
		RenderPlatformBlock(TypeID);
	}


	private void Update_Touch () {
		if (TouchedByRigidbody && TouchedByCharacter && TouchedByPlayer) return;
		var hits = Physics.OverlapAll(PhysicsMask.ENTITY, Rect.Expand(1), out int count, this);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Rect.y < Y + Height) continue;
			if (hit.Entity is not Rigidbody rig) continue;
			if (!TouchedByRigidbody) OnRigidbodyTouched(rig);
			TouchedByRigidbody = true;
			if (hit.Entity is not Character ch) continue;
			if (!TouchedByCharacter) OnCharacterTouched(ch);
			TouchedByCharacter = true;
			if (hit.Entity != PlayerSystem.Selecting) continue;
			if (!TouchedByPlayer) OnPlayerTouched(PlayerSystem.Selecting);
			TouchedByPlayer = true;
			break;
		}
	}


	private void Update_PushX () {

		if (OneWay || X == PrevX) return;

		bool pushLeft = false;
		bool pushRight = false;

		if (X < PrevX) {
			// Move Left
			if (Pose == FittingPose.Single || Pose == FittingPose.Left) {
				pushLeft = true;
			}
		} else {
			// Move Right
			if (Pose == FittingPose.Single || Pose == FittingPose.Right) {
				pushRight = true;
			}
		}
		if (!pushLeft && !pushRight) return;

		var rect = Rect;
		var prevRect = rect;
		prevRect.x = PrevX;
		var hits = Physics.OverlapAll(PhysicsMask.DYNAMIC, rect, out int count, this);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Rigidbody rig) continue;
			var rRect = rig.Rect;
			if (pushLeft) {
				// Left
				if (rig.VelocityX < X - PrevX || rRect.xMin > X) continue;
				rig.PerformMove(X - rRect.xMax, 0);
			} else {
				// Right
				if (rig.VelocityX > X - PrevX || rRect.xMax < X + Width) continue;
				rig.PerformMove(X + Width - rRect.xMin, 0);
			}
		}
	}


	// ABS
	protected abstract void Move ();


	protected virtual void OnRigidbodyTouched (Rigidbody rig) { }
	protected virtual void OnCharacterTouched (Character character) { }
	protected virtual void OnPlayerTouched (Character player) { }


	// API
	public void SetTouch (bool rigidbody = true, bool character = true, bool player = true) {
		TouchedByRigidbody = rigidbody;
		TouchedByCharacter = character;
		TouchedByPlayer = player;
	}


	protected virtual void RenderPlatformBlock (int artworkID) {
		int index = Pose switch {
			FittingPose.Single => 0,
			FittingPose.Left => 1,
			FittingPose.Mid => 2,
			FittingPose.Right => 3,
			_ => 0,
		};
		if (Renderer.TryGetSpriteFromGroup(
			artworkID, index, out var sprite,
			loopIndex: false,
			clampIndex: true
		) || Renderer.TryGetSprite(artworkID, out sprite, true)) {
			Renderer.Draw(sprite, Rect);
		}
	}


}