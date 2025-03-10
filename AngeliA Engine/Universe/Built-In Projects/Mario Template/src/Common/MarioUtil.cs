using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public static class MarioUtil {

	public static int CurrentScore { get; private set; } = 0;
	public static int CurrentMajorLevel { get; private set; } = 1;
	public static int CurrentMinorLevel { get; private set; } = 1;
	public static string CurrentLevelLabel { get; private set; } = "1-1";

	// API
	public static int GetEmbedItemID (IRect sourceRect) {
		int id = WorldSquad.Front.GetBlockAt(
			(sourceRect.x + 1).ToUnit(), (sourceRect.y + 1).ToUnit(), Stage.ViewZ, BlockType.Element
		);
		if (!Stage.IsValidEntityID(id) && !ItemSystem.HasItem(id)) {
			id = 0;
		}
		return id;
	}

	public static Entity SpawnEmbedItem (int id, IRect sourceRect, Direction4 spawnDirection) {
		Entity entity = null;
		if (Stage.IsValidEntityID(id)) {
			entity = Stage.SpawnEntity(id, sourceRect.x, sourceRect.y);
		} else if (ItemSystem.HasItem(id)) {
			entity = ItemSystem.SpawnItem(id, sourceRect.x, sourceRect.y, 1, jump: true);
		}
		if (entity != null) {
			var eRect = entity.Rect;
			switch (spawnDirection) {
				case Direction4.Left:
					entity.X += sourceRect.x - eRect.xMax;
					entity.Y += sourceRect.CenterY() - eRect.CenterY();
					break;
				case Direction4.Right:
					entity.X += sourceRect.xMax - eRect.x;
					entity.Y += sourceRect.CenterY() - eRect.CenterY();
					break;
				case Direction4.Down:
					entity.X += sourceRect.CenterX() - eRect.CenterX();
					entity.Y += sourceRect.y - eRect.yMax;
					break;
				case Direction4.Up:
					entity.X += sourceRect.CenterX() - eRect.CenterX();
					entity.Y += sourceRect.yMax - eRect.y;
					break;
			}
			if (entity is IBlockEntity) {
				IBlockEntity.RefreshBlockEntitiesNearby(new Int2((sourceRect.x + 1).ToUnit(), (sourceRect.yMax + 1).ToUnit()), entity);
			}
		}
		WorldSquad.Front.SetBlockAt(
			(sourceRect.x + 1).ToUnit(), (sourceRect.y + 1).ToUnit(), Stage.ViewZ, BlockType.Element, 0
		);
		return entity;
	}

	public static void GiveScore (int score) => CurrentScore += score;

	public static void ResetScore () => CurrentScore = 0;

	public static void SetLevelCount (int major, int minor) {
		(CurrentMajorLevel, CurrentMinorLevel) = (major, minor);
		CurrentLevelLabel = $"{major}-{minor}";
	}

	public static void InitMovementForMarioGame (CharacterMovement movement) {

		movement.RunAvailable.BaseValue = true;
		movement.RunSpeed.BaseValue = 48;
		movement.RunAcceleration.BaseValue = 1;
		movement.RunDeceleration.BaseValue = 2;
		movement.RunBrakeAcceleration.BaseValue = 3;

		movement.JumpCount.BaseValue = 1;
		movement.JumpSpeed.BaseValue = 80;
		movement.JumpReleaseSpeedRate.BaseValue = 700;
		movement.JumpRiseGravityRate.BaseValue = 600;
		movement.FirstJumpWithRoll.BaseValue = false;
		movement.AllowSquatJump.BaseValue = true;

		movement.SquatAvailable.BaseValue = true;
		movement.SquatHeightAmount.BaseValue = 521;
		movement.SquatMoveSpeed.BaseValue = 0;
		movement.SquatAcceleration.BaseValue = 1;
		movement.SquatDeceleration.BaseValue = 1;

		movement.SlipAvailable.BaseValue = true;
		movement.SlipAcceleration.BaseValue = 1;
		movement.SlipAcceleration.BaseValue = 1;

		movement.SwimAvailable.BaseValue = true;
		movement.InWaterSpeedRate.BaseValue = 500;
		movement.SwimWidthAmount.BaseValue = 1333;
		movement.SwimHeightAmount.BaseValue = 1000;
		movement.SwimSpeed.BaseValue = 42;
		movement.SwimJumpSpeed.BaseValue = 128;
		movement.SwimAcceleration.BaseValue = 2;
		movement.SwimDeceleration.BaseValue = 2;

		movement.ClimbAvailable.BaseValue = true;
		movement.AllowJumpWhenClimbing.BaseValue = true;
		movement.ClimbSpeedX.BaseValue = 12;
		movement.ClimbSpeedY.BaseValue = 18;

		movement.WalkAvailable.BaseValue = false;
		movement.DashAvailable.BaseValue = false;
		movement.RushAvailable.BaseValue = false;
		movement.PoundAvailable.BaseValue = false;
		movement.FlyAvailable.BaseValue = false;
		movement.SlideAvailable.BaseValue = false;
		movement.GrabTopAvailable.BaseValue = false;
		movement.GrabSideAvailable.BaseValue = false;
		movement.CrashAvailable.BaseValue = false;
		movement.PushAvailable.BaseValue = false;

	}

}
