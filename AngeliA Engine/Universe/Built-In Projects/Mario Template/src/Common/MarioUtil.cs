using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public static class MarioUtil {

	public static int CurrentScore { get; private set; } = 0;
	public static int CurrentMajorLevel { get; private set; } = 1;
	public static int CurrentMinorLevel { get; private set; } = 1;
	private static readonly int FCT_FONT = "SuperMarioBros".AngeHash();
	private static readonly AudioCode SPAWN_AC = "SpawnItem";
	private static readonly SpriteCode SPIN_COIN_SP = "CoinSpin";
	private static readonly IntToChars ScoreToChar = new();

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
			entity.OnActivated();
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

	public static void GiveScore (int score, int x, int y) {
		CurrentScore += score;
		FloatingCombatText.Spawn(
			x, y,
			ScoreToChar.GetChars(score),
			fontID: FCT_FONT,
			style: GUI.Skin.SmallCenterLabel
		);
	}

	public static void ResetScore () => CurrentScore = 0;

	public static void SetLevelCount (int major, int minor) => (CurrentMajorLevel, CurrentMinorLevel) = (major, minor);

	public static void InitCharacterForMarioGame (PlayableCharacter character) {

		// Health
		var health = character.NativeHealth;
		health.MaxHP.BaseValue = 2;
		health.HP = 1;

		// Mov
		var mov = character.NativeMovement;

		mov.RunAvailable.BaseValue = true;
		mov.RunSpeed.BaseValue = 48;
		mov.RunAcceleration.BaseValue = 1;
		mov.RunDeceleration.BaseValue = 2;
		mov.RunBrakeAcceleration.BaseValue = 3;

		mov.JumpCount.BaseValue = 1;
		mov.JumpSpeed.BaseValue = 80;
		mov.JumpReleaseSpeedRate.BaseValue = 700;
		mov.JumpRiseGravityRate.BaseValue = 600;
		mov.FirstJumpWithRoll.BaseValue = false;
		mov.AllowSquatJump.BaseValue = true;

		mov.SquatAvailable.BaseValue = true;
		mov.SquatHeightAmount.BaseValue = 521;
		mov.SquatMoveSpeed.BaseValue = 0;
		mov.SquatAcceleration.BaseValue = 1;
		mov.SquatDeceleration.BaseValue = 1;

		mov.SlipAvailable.BaseValue = true;
		mov.SlipAcceleration.BaseValue = 1;
		mov.SlipAcceleration.BaseValue = 1;

		mov.SwimAvailable.BaseValue = true;
		mov.InWaterSpeedRate.BaseValue = 500;
		mov.SwimWidthAmount.BaseValue = 1333;
		mov.SwimHeightAmount.BaseValue = 1000;
		mov.SwimSpeed.BaseValue = 42;
		mov.SwimJumpSpeed.BaseValue = 128;
		mov.SwimAcceleration.BaseValue = 2;
		mov.SwimDeceleration.BaseValue = 2;

		mov.ClimbAvailable.BaseValue = true;
		mov.AllowJumpWhenClimbing.BaseValue = true;
		mov.ClimbSpeedX.BaseValue = 12;
		mov.ClimbSpeedY.BaseValue = 18;

		mov.WalkAvailable.BaseValue = true;
		mov.WalkSpeed.BaseValue = 20;
		mov.WalkAcceleration.BaseValue = 3;
		mov.WalkDeceleration.BaseValue = 4;

		mov.DashAvailable.BaseValue = false;
		mov.RushAvailable.BaseValue = false;
		mov.PoundAvailable.BaseValue = false;
		mov.FlyAvailable.BaseValue = false;
		mov.SlideAvailable.BaseValue = false;
		mov.GrabTopAvailable.BaseValue = false;
		mov.GrabSideAvailable.BaseValue = false;
		mov.CrashAvailable.BaseValue = false;
		mov.PushAvailable.BaseValue = false;

	}

	public static void UpdateForBumpToSpawnItem (Entity entity, int itemInside, int spawnItemStartFrame, Direction4 spawnDirection, int frame = -1) {

		frame = frame < 0 ? Game.GlobalFrame : frame;

		const int RISE_DUR = 24;
		if (spawnItemStartFrame < 0 || frame > spawnItemStartFrame + RISE_DUR) return;

		var sourceRect = entity.Rect;

		if (itemInside == Coin.TYPE_ID) {
			// For Coin
			// Bounce Animation
			if (Renderer.TryGetSprite(SPIN_COIN_SP, out var iconSp, ignoreAnimation: false)) {
				int offsetY = Util.RemapUnclamped(
					0, RISE_DUR / 2,
					0, sourceRect.height * 3,
					(frame - spawnItemStartFrame).PingPong(RISE_DUR * 2 / 3)
				);
				var coinRect = sourceRect.Shift(0, offsetY);
				var cell = Renderer.Draw(iconSp, coinRect);
				coinRect.yMin = sourceRect.yMax;
				cell.Clamp(coinRect);
			}
			// Collect Coin
			if (itemInside != 0) {
				if (frame == spawnItemStartFrame + 1) {
					Coin.Collect(1);
					WorldSquad.Front.SetBlockAt(
						(sourceRect.x + 1).ToUnit(), (sourceRect.y + 1).ToUnit(), Stage.ViewZ, BlockType.Element, 0
					);
				} else if (frame == spawnItemStartFrame + RISE_DUR) {
					itemInside = 0;
				}
			}
		} else {
			// For Entity
			var itemType = Stage.GetEntityType(itemInside);
			bool fastSpawn = itemType != null && itemType.IsSubclassOf(typeof(Enemy));
			if (fastSpawn) {
				// Fast Spawn
				if (itemInside != 0 && frame == spawnItemStartFrame + 1) {
					var spawnedEntity = MarioUtil.SpawnEmbedItem(itemInside, sourceRect, spawnDirection);
					if (spawnedEntity is Rigidbody rig) {
						rig.VelocityY = 42;
					}
					itemInside = 0;
					FrameworkUtil.PlaySoundAtPosition(SPAWN_AC, entity.XY);
				}
			} else {
				// Rise Animation
				if (Renderer.TryGetSpriteForGizmos(itemInside, out var iconSp)) {
					int shift = Util.RemapUnclamped(
						spawnItemStartFrame, spawnItemStartFrame + RISE_DUR,
						0, sourceRect.height,
						frame
					);
					Renderer.Draw(iconSp, sourceRect.Shift(spawnDirection.Normal() * shift), z: int.MinValue + 1);
				}
				// Spawn
				if (itemInside != 0) {
					if (frame == spawnItemStartFrame) {
						FrameworkUtil.PlaySoundAtPosition(SPAWN_AC, entity.XY);
					}
					if (frame == spawnItemStartFrame + RISE_DUR) {
						MarioUtil.SpawnEmbedItem(itemInside, sourceRect, spawnDirection);
						itemInside = 0;
					}
				}
			}
		}
	}

}
