using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public enum Sound : byte {
	StepOnEnemy = 0,
}

public static class MarioUtil {

	public static int CurrentScore { get; private set; } = 0;
	public static int CurrentMajorLevel { get; private set; } = 1;
	public static int CurrentMinorLevel { get; private set; } = 1;
	private static readonly SpriteCode SPIN_COIN_SP = "CoinSpin";
	private static readonly Dictionary<Sound, int> SoundPool = new(Util.AllEnumIdPairs<Sound>());

	// API
	public static int GetEmbedItemID (IRect sourceRect, int failbackID = 0) {
		int id = WorldSquad.Front.GetBlockAt(
			(sourceRect.x + 1).ToUnit(), (sourceRect.y + 1).ToUnit(), Stage.ViewZ, BlockType.Element
		);
		if (id == 0 || (!Stage.IsValidEntityID(id) && !ItemSystem.HasItem(id))) {
			id = failbackID;
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
				entity.FirstUpdate();
				IBlockEntity.RefreshBlockEntitiesNearby(
					new Int2((entity.Rect.x + 1).ToUnit(), (entity.Rect.y + 1).ToUnit()),
					entity
				);
			}
		}
		return entity;
	}

	public static void GiveScore (int score) => CurrentScore += score;

	public static void ResetScore () => CurrentScore = 0;

	public static void SetLevelCount (int major, int minor) => (CurrentMajorLevel, CurrentMinorLevel) = (major, minor);

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

	public static void UpdateForBumpToSpawnItem (IBumpable source, int itemInside, int spawnItemStartFrame) {

		const int RISE_DUR = 24;
		if (spawnItemStartFrame < 0 || Game.GlobalFrame > spawnItemStartFrame + RISE_DUR) return;
		if (source is not Entity sourceEntity) return;

		var sourceRect = sourceEntity.Rect;

		if (itemInside == Coin.TYPE_ID) {
			// For Coin
			// Bounce Animation
			if (Renderer.TryGetSprite(SPIN_COIN_SP, out var iconSp, ignoreAnimation: false)) {
				int offsetY = Util.RemapUnclamped(
					0, RISE_DUR / 2,
					0, sourceRect.height * 3,
					(Game.GlobalFrame - spawnItemStartFrame).PingPong(RISE_DUR * 2 / 3)
				);
				var coinRect = sourceRect.Shift(0, offsetY);
				var cell = Renderer.Draw(iconSp, coinRect);
				coinRect.yMin = sourceRect.yMax;
				cell.Clamp(coinRect);
			}
			// Collect Coin
			if (itemInside != 0) {
				if (Game.GlobalFrame == spawnItemStartFrame + 1) {
					Coin.Collect(1);
					WorldSquad.Front.SetBlockAt(
						(sourceRect.x + 1).ToUnit(), (sourceRect.y + 1).ToUnit(), Stage.ViewZ, BlockType.Element, 0
					);
				} else if (Game.GlobalFrame == spawnItemStartFrame + RISE_DUR) {
					itemInside = 0;
				}
			}
		} else {
			// For Entity
			bool fastSpawn = Stage.GetEntityType(itemInside).IsSubclassOf(typeof(Enemy));
			var spawnToDir = source.LastBumpFrom.Opposite();
			if (fastSpawn) {
				// Fast Spawn
				if (itemInside != 0 && Game.GlobalFrame == spawnItemStartFrame + 1) {
					var spawnedEntity = MarioUtil.SpawnEmbedItem(itemInside, sourceRect, spawnToDir);
					if (spawnedEntity is Rigidbody rig) {
						rig.VelocityY = 42;
					}
					itemInside = 0;
				}
			} else {
				// Rise Animation
				if (Renderer.TryGetSpriteForGizmos(itemInside, out var iconSp)) {
					int shift = Util.RemapUnclamped(
						spawnItemStartFrame, spawnItemStartFrame + RISE_DUR,
						0, sourceRect.height,
						Game.GlobalFrame
					);
					Renderer.Draw(iconSp, sourceRect.Shift(spawnToDir.Normal() * shift), z: int.MinValue + 1);
				}
				// Spawn
				if (itemInside != 0 && Game.GlobalFrame == spawnItemStartFrame + RISE_DUR) {
					MarioUtil.SpawnEmbedItem(itemInside, sourceRect, spawnToDir);
					itemInside = 0;
				}
			}
		}
	}

	public static void PlayMarioAudio (Sound sound, Int2 pos, float volume = 1f, float pitch = 1f) {
		if (!SoundPool.TryGetValue(sound, out int id)) return;
		Game.PlaySoundAtPosition(id, pos, volume, pitch);
	}

}
