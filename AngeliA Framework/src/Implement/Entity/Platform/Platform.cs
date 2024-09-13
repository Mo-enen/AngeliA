using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


[EntityAttribute.MapEditorGroup(nameof(Platform))]
[EntityAttribute.Capacity(128)]
public abstract class Platform : EnvironmentEntity {


	// Api
	public abstract bool OneWay { get; }

	// Short
	protected bool TouchedByPlayer { get; private set; } = false;
	protected bool TouchedByCharacter { get; private set; } = false;
	protected bool TouchedByRigidbody { get; private set; } = false;
	protected FittingPose Pose { get; private set; } = FittingPose.Unknown;

	// Data
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
		Pose = GetEntityPose(TypeID, X.ToUnit(), Y.ToUnit(), true);
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
		Update_CarryX();
		Update_CarryY();
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
			if (hit.Entity != Player.Selecting) continue;
			if (!TouchedByPlayer) OnPlayerTouched(Player.Selecting);
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


	private void Update_CarryX () {

		if (X == PrevX) return;

		var rect = Rect;
		var prevRect = rect;
		prevRect.x = PrevX;
		int left = X;
		int right = X + Width;
		if (Pose == FittingPose.Single || Pose == FittingPose.Left) {
			left = int.MinValue;
		}
		if (Pose == FittingPose.Single || Pose == FittingPose.Right) {
			right = int.MaxValue;
		}

		var hits = Physics.OverlapAll(
			PhysicsMask.DYNAMIC, rect.EdgeOutside(Direction4.Up, 32).Shift(0, -16), out int count,
			this, OperationMode.ColliderAndTrigger
		);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Rigidbody rig) continue;
			if (rig.X < left || rig.X >= right) continue;
			if (rig.Rect.y < rect.yMax - 32) continue;
			if (!hit.IsTrigger) {
				// For General Rig
				rig.PerformMove(X - PrevX, rect.yMax - rig.Y);
			} else {
				// For Nav Character
				if (hit.Entity is not Character ch || ch.Movement.IsFlying) continue;
				rig.X += X - PrevX;
				rig.Y = rect.yMax;
			}
			rig.MakeGrounded(1, TypeID);
		}
	}


	private void Update_CarryY () {

		if (Y == PrevY) return;

		var rect = Rect;
		var prevRect = rect;
		prevRect.y = PrevY;
		int left = X;
		int right = X + Width;
		if (Pose == FittingPose.Single || Pose == FittingPose.Left) {
			left = int.MinValue;
		}
		if (Pose == FittingPose.Single || Pose == FittingPose.Right) {
			right = int.MaxValue;
		}

		if (Y > PrevY) {
			// Moving Up
			prevRect.height -= Height / 3;
			rect.y = PrevY + prevRect.height;
			rect.height = Y + Height - rect.y;
			var hits = Physics.OverlapAll(
				PhysicsMask.DYNAMIC, rect, out int count,
				this, OperationMode.ColliderOnly
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody rig) continue;
				if (rig.X < left || rig.X >= right) continue;
				if (rig.VelocityY > Y - PrevY) continue;
				if (!rig.Rect.Overlaps(prevRect)) {
					rig.PerformMove(0, rect.yMax - rig.Rect.y);
					rig.MakeGrounded(1, TypeID);
				}
			}
			// For Nav Character
			hits = Physics.OverlapAll(
				PhysicsMask.DYNAMIC, rect, out count,
				this, OperationMode.TriggerOnly
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Character ch || ch.Movement.IsFlying) continue;
				if (ch.X < left || ch.X >= right) continue;
				if (ch.VelocityY > Y - PrevY) continue;
				if (!ch.Rect.Overlaps(prevRect)) {
					ch.Y.MoveTowards(rect.yMax - ch.OffsetY, 64);
					ch.MakeGrounded(1, TypeID);
				}
			}

		} else {
			// Moving Down
			prevRect.height += PrevY - Y + 1;
			var hits = Physics.OverlapAll(
				PhysicsMask.DYNAMIC, prevRect, out int count,
				this, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody rig) continue;
				if (rig.X < left || rig.X >= right) continue;
				if (rig.VelocityY > 0) continue;
				if (rig.Rect.yMin < rect.yMax - Const.CEL / 3) continue;
				if (hit.IsTrigger && (hit.Entity is not Character ch || ch.Movement.IsFlying)) continue;
				rig.PerformMove(0, rect.yMax - rig.OffsetY - rig.Y);
				if (!hit.IsTrigger) rig.VelocityY = 0;
				rig.MakeGrounded(1, TypeID);
			}
		}
	}


	// ABS
	protected abstract void Move ();


	protected virtual void OnRigidbodyTouched (Rigidbody rig) { }
	protected virtual void OnCharacterTouched (Character character) { }
	protected virtual void OnPlayerTouched (Player player) { }


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