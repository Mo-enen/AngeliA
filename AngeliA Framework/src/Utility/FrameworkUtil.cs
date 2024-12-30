using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AngeliA;


public static class FrameworkUtil {


	public const int RUN_CODE_ANALYSIS_SETTING_ID = 895245367;
	public const int RUN_CODE_ANALYSIS_SETTING_SILENTLY_ID = 895245368;

	private const int SEARCHLIGHT_DENSITY = 32;
	private static readonly Type BLOCK_ENTITY_TYPE = typeof(IBlockEntity);
	private static readonly List<PhysicsCell[,,]> CellPhysicsCells = [];
	private static readonly Color32[] COLLIDER_TINTS = [
		Color32.RED_BETTER,
		Color32.ORANGE_BETTER,
		Color32.YELLOW,
		Color32.GREEN,
		Color32.CYAN,
		Color32.BLUE,
		Color32.GREY_128,
	];
	private static readonly int[] ITEM_TYPE_ICONS = [
		BuiltInSprite.ITEM_ICON_HAND_TOOL,
		BuiltInSprite.ITEM_ICON_ARMOR,
		BuiltInSprite.ITEM_ICON_HELMET,
		BuiltInSprite.ITEM_ICON_SHOES,
		BuiltInSprite.ITEM_ICON_GLOVES,
		BuiltInSprite.ITEM_ICON_JEWELRY,
		BuiltInSprite.ITEM_ICON_WEAPON,
		BuiltInSprite.ITEM_ICON_FOOD,
		BuiltInSprite.ITEM_ICON_ITEM,
	];
	private static readonly Dictionary<int, int> SYSTEM_NUMBER_POOL = new(10) {
		{ NumberZero.TYPE_ID, 0 },
		{ NumberOne.TYPE_ID, 1 },
		{ NumberTwo.TYPE_ID, 2 },
		{ NumberThree.TYPE_ID, 3 },
		{ NumberFour.TYPE_ID, 4 },
		{ NumberFive.TYPE_ID, 5 },
		{ NumberSix.TYPE_ID, 6 },
		{ NumberSeven.TYPE_ID, 7 },
		{ NumberEight.TYPE_ID, 8 },
		{ NumberNine.TYPE_ID, 9 },
	};
	private static readonly Dictionary<int, int> SYSTEM_NUMBER_POOL_ALT = new(10) {
		{ 0, NumberZero.TYPE_ID},
		{ 1, NumberOne.TYPE_ID},
		{ 2, NumberTwo.TYPE_ID},
		{ 3, NumberThree.TYPE_ID},
		{ 4, NumberFour.TYPE_ID},
		{ 5, NumberFive.TYPE_ID},
		{ 6, NumberSix.TYPE_ID},
		{ 7, NumberSeven.TYPE_ID},
		{ 8, NumberEight.TYPE_ID},
		{ 9, NumberNine.TYPE_ID},
	};
	private static readonly Int3[] WorldPosInViewCache = new Int3[256];
	private static readonly PhysicsCell[] BlockOperationCache = new PhysicsCell[32];

	// Event
	[OnObjectBreak] internal static Action<int, IRect> OnObjectBreak;
	[OnObjectFreeFall] internal static Action<int, Int4, Int4> OnObjectFreeFall;
	[OnBlockPicked] internal static Action<int, IRect> OnBlockPicked;
	[OnFallIntoWater] internal static Action<Rigidbody> OnFallIntoWater;
	[OnCameOutOfWater] internal static Action<Rigidbody> OnCameOutOfWater;
	[OnItemCollected] internal static Action<Entity, int, int> OnItemCollected;
	[OnItemLost] internal static Action<Character, int> OnItemLost;
	[OnItemError] internal static Action<Character, int> OnItemErrorHint;
	[OnItemDamage] internal static Action<Character, int, int> OnItemDamage;
	[OnItemUnlocked] internal static Action<int> OnItemUnlocked;
	[OnCheatPerformed] internal static Action<string> OnCheatPerformed;
	[OnFootStepped] internal static Action<int, int, int> OnFootStepped;


	// Drawing
	public static Cell DrawEnvironmentShadow (Cell source, int offsetX = -Const.HALF / 2, int offsetY = 0, byte alpha = 64, int z = -64 * 1024 + 16) {
		int oldLayer = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(RenderLayer.SHADOW);
		var result = Renderer.DrawPixel(default);
		result.CopyFrom(source);
		result.X += offsetX;
		result.Y += offsetY;
		result.Z = z;
		result.Color = new Color32(0, 0, 0, alpha);
		Renderer.SetLayer(oldLayer);
		return result;
	}


	public static void DrawGlitchEffect (Cell cell, int frame, int speedAmount = 1000, int shiftAmount = 1000, int scaleAmount = 1000) {

		if (speedAmount <= 0 || shiftAmount <= 0 || scaleAmount <= 0) return;

		if (frame.UMod(0096000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-012, 012), Util.QuickRandom(-007, 007), Util.QuickRandom(0900, 1100), Util.QuickRandom(0500, 1100), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0061000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-002, 041), Util.QuickRandom(-019, 072), Util.QuickRandom(0500, 1200), Util.QuickRandom(0500, 1200), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0121000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-016, 016), Util.QuickRandom(-007, 007), Util.QuickRandom(0400, 1100), Util.QuickRandom(0600, 1100), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0127000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-016, 016), Util.QuickRandom(-007, 007), Util.QuickRandom(0900, 1100), Util.QuickRandom(0500, 1300), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0185000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-011, 021), Util.QuickRandom(-018, 018), Util.QuickRandom(0900, 1300), Util.QuickRandom(0900, 1300), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0187000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-034, 042), Util.QuickRandom(-012, 008), Util.QuickRandom(0800, 1100), Util.QuickRandom(0900, 1400), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0193000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-052, 002), Util.QuickRandom(-091, 077), Util.QuickRandom(0700, 1400), Util.QuickRandom(0800, 1100), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(0274000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-033, 072), Util.QuickRandom(-031, 079), Util.QuickRandom(0800, 1100), Util.QuickRandom(0900, 1800), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(1846000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-094, 012), Util.QuickRandom(-077, 112), Util.QuickRandom(0900, 1500), Util.QuickRandom(0900, 1300), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(3379000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-194, 112), Util.QuickRandom(-177, 212), Util.QuickRandom(0900, 1600), Util.QuickRandom(0700, 1900), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));
		if (frame.UMod(9379000 / speedAmount) == 0) DrawGlitch(cell, shiftAmount, scaleAmount, Util.QuickRandom(-293, 211), Util.QuickRandom(-079, 011), Util.QuickRandom(0900, 1700), Util.QuickRandom(0900, 1100), Util.QuickRandomColor(0, 360, 100, 100, 100, 100, 128, 255));

		// Func
		static void DrawGlitch (Cell cell, int shiftAmount, int scaleAmount, int offsetX, int offsetY, int scaleX, int scaleY, Color32 color) {

			var cursedCell = Renderer.DrawPixel(default, 0);
			cursedCell.Sprite = cell.Sprite;
			cursedCell.TextSprite = cell.TextSprite;
			cursedCell.X = cell.X;
			cursedCell.Y = cell.Y;
			cursedCell.Z = cell.Z + 1;
			cursedCell.Rotation1000 = cell.Rotation1000;
			cursedCell.Width = cell.Width * scaleX / 1000 * scaleAmount / 1000;
			cursedCell.Height = cell.Height * scaleY / 1000 * scaleAmount / 1000;
			cursedCell.PivotX = cell.PivotX;
			cursedCell.PivotY = cell.PivotY;
			cursedCell.Shift = cell.Shift;

			cursedCell.Color = color;
			cursedCell.X += offsetX * shiftAmount / 1000;
			cursedCell.Y += offsetY * shiftAmount / 1000;

		}
	}


	public static void DrawAfterimageEffect (Cell source, int speedX, int speedY, Color32 tintStart, Color32 tintEnd, int rotateSpeed = 0, int count = 3, int frameStep = 2, int scaleStart = 1000, int scaleEnd = 1000) {
		for (int i = 1; i <= count; i++) {
			int index = i * frameStep;
			float lerp01 = (i - 1f) / (count - 1);
			var cell = Renderer.Draw(Const.PIXEL, default);
			cell.CopyFrom(source);
			cell.X -= index * speedX;
			cell.Y -= index * speedY;
			cell.Z -= index;
			cell.Rotation -= index * rotateSpeed;
			cell.Color *= Color32.Lerp(tintStart, tintEnd, lerp01);
			cell.ScaleFrom(
				Util.LerpUnclamped(scaleStart, scaleEnd, lerp01).RoundToInt(),
				cell.X, cell.Y
			);
		}
	}


	public static void DrawSegmentHealthBar (int x, int y, int heartLeftCode, int heartRightCode, int emptyHeartLeftCode, int emptyHeartRightCode, int dropParticleID, int hp, int maxHP, int prevHP = int.MinValue) {

		const int SIZE = Const.HALF;
		const int COLUMN = 4;
		const int MAX = 8;

		int maxHp = Util.Min(maxHP, MAX);
		int left = x - SIZE * COLUMN / 4;

		// Draw Hearts
		var rect = new IRect(0, 0, SIZE / 2, SIZE);
		bool isLeft = true;
		for (int i = 0; i < maxHp; i++) {
			rect.x = left + (i % COLUMN) * SIZE / 2;
			rect.y = y - (i / COLUMN + 1) * SIZE;
			if (i < hp) {
				// Heart
				Renderer.Draw(isLeft ? heartLeftCode : heartRightCode, rect, 0);
			} else {
				// Empty Heart
				Renderer.Draw(isLeft ? emptyHeartLeftCode : emptyHeartRightCode, rect, 0);
				// Spawn Drop Particle
				if (i < prevHP) {
					Entity heart;
					if (isLeft) {
						heart = Stage.SpawnEntity(dropParticleID, rect.x, rect.y);
					} else {
						heart = Stage.SpawnEntity(dropParticleID, rect.x, rect.y);
					}
					if (heart != null) {
						heart.Width = rect.width + 8;
						heart.Height = rect.height + 16;
					}
				}
			}
			isLeft = !isLeft;
		}
	}


	public static bool DrawPoseCharacterAsUI (IRect rect, PoseCharacterRenderer renderer, int animationFrame) => DrawPoseCharacterAsUI(rect, renderer, animationFrame, out _, out _);
	public static bool DrawPoseCharacterAsUI (IRect rect, PoseCharacterRenderer renderer, int animationFrame, out IRect globalRect, out IRect uiRect) {

		globalRect = default;
		uiRect = default;
		if (renderer == null) return false;
		var target = renderer.TargetCharacter;

		// Draw Player
		int cellIndexStart;
		int cellIndexEnd;
		using (new UILayerScope(ignoreSorting: true)) {
			cellIndexStart = Renderer.GetUsedCellCount();
			int oldAniFrame = renderer.CurrentAnimationFrame;
			renderer.CurrentAnimationFrame = animationFrame;
			bool oldActive = target.Active;
			target.Active = true;
			target.UpdateHandToolCache();
			renderer.LateUpdate();
			target.Active = oldActive;
			renderer.CurrentAnimationFrame = oldAniFrame;
			cellIndexEnd = Renderer.GetUsedCellCount();
		}
		if (cellIndexStart == cellIndexEnd) return false;

		if (!Renderer.GetCells(RenderLayer.UI, out var cells, out int count)) return false;

		// Get Min Max
		bool flying = target.AnimationType == CharacterAnimationType.Fly;
		int originalMinX = target.X - Const.HALF - 16;
		int originalMinY = target.Y - 16 + (flying ? renderer.PoseRootY / 2 : 0);
		int originalMaxX = target.X + Const.HALF + 16;
		int originalMaxY = target.Y + Const.CEL * 2 + 16;
		if (flying) {
			originalMinY -= Const.HALF;
			originalMaxY -= Const.HALF;
		}
		globalRect.SetMinMax(originalMinX, originalMaxX, originalMinY, originalMaxY);

		// Move Cells
		RemapCells(
			cells, cellIndexStart, Util.Min(count, cellIndexEnd),
			IRect.MinMaxRect(originalMinX, originalMinY, originalMaxX, originalMaxY),
			rect, out int minZ,
			500, 0, round: false
		);
		uiRect = rect.Fit(originalMaxX - originalMinX, originalMaxY - originalMinY, 500, 0);

		// Fix Z
		if (minZ != int.MaxValue) {
			for (int i = cellIndexStart; i < count && i < cellIndexEnd; i++) {
				var cell = cells[i];
				cell.Z -= minZ;
			}
		}

		// Sort
		Util.QuickSort(cells, cellIndexStart, Util.Min(cellIndexEnd - 1, count - 1), Renderer.CellComparer.Instance);

		return true;

	}


	public static void RemapCells (Cell[] cells, int cellIndexStart, int cellIndexEnd, IRect from, IRect to, int fitPivotX = 500, int fitPivotY = 500, bool round = false, bool fit = true) => RemapCells(cells, cellIndexStart, cellIndexEnd, from, to, out _, fitPivotX, fitPivotY, round, fit);
	public static void RemapCells (Span<Cell> cells, int cellIndexStart, int cellIndexEnd, IRect from, IRect to, int fitPivotX = 500, int fitPivotY = 500, bool round = false, bool fit = true) => RemapCells(cells, cellIndexStart, cellIndexEnd, from, to, out _, fitPivotX, fitPivotY, round, fit);
	public static void RemapCells (Cell[] cells, int cellIndexStart, int cellIndexEnd, IRect from, IRect to, out int minZ, int fitPivotX = 500, int fitPivotY = 500, bool round = false, bool fit = true) => RemapCells(cells.GetSpan(), cellIndexStart, cellIndexEnd, from, to, out minZ, fitPivotX, fitPivotY, round, fit);
	public static void RemapCells (Span<Cell> cells, int cellIndexStart, int cellIndexEnd, IRect from, IRect to, out int minZ, int fitPivotX = 500, int fitPivotY = 500, bool round = false, bool fit = true) {
		int originalWidth = from.width;
		int originalHeight = from.height;
		var targetRect = fit ?
			to.Fit(originalWidth, originalHeight, fitPivotX, fitPivotY) :
			to.Envelope(originalWidth, originalHeight);
		minZ = int.MaxValue;
		for (int i = cellIndexStart; i < cellIndexEnd; i++) {
			var cell = cells[i];
			minZ = Util.Min(minZ, cell.Z);
			if (round) {
				cell.X = (targetRect.x + (cell.X - from.x) * targetRect.width / (float)originalWidth).FloorToInt();
				cell.Y = (targetRect.y + (cell.Y - from.y) * targetRect.height / (float)originalHeight).FloorToInt();
				cell.Width = (cell.Width * targetRect.width / (float)originalWidth).CeilToInt();
				cell.Height = (cell.Height * targetRect.height / (float)originalHeight).CeilToInt();
			} else {
				cell.X = targetRect.x + (cell.X - from.x) * targetRect.width / originalWidth;
				cell.Y = targetRect.y + (cell.Y - from.y) * targetRect.height / originalHeight;
				cell.Width = cell.Width * targetRect.width / originalWidth;
				cell.Height = cell.Height * targetRect.height / originalHeight;
			}
			if (!cell.Shift.IsZero) {
				cell.Shift = new Int4(
					cell.Shift.left * targetRect.width / originalWidth,
					cell.Shift.right * targetRect.width / originalWidth,
					cell.Shift.down * targetRect.height / originalHeight,
					cell.Shift.up * targetRect.height / originalHeight
				);
			}
		}
	}


	public static void DrawMagicEncircleAurora (int spriteID, int count, int centerX, int centerY, int localFrame, Color32 tint, int scale = 1000, int rotateSpeed = 16, int swingDuration = 20, int swingAmout = 240, int growDuration = 10, int z = int.MinValue) {
		if (!Renderer.TryGetSprite(spriteID, out var sprite, false)) return;
		// Swing
		int pivotSwing = localFrame.PingPong(swingDuration) * swingAmout / swingDuration - swingAmout / 2;
		// Grow
		if (localFrame < growDuration) {
			float lerp = 1f - (float)localFrame / growDuration;
			scale = scale.LerpTo(0, lerp);
			pivotSwing = pivotSwing.LerpTo(0, lerp);
		}
		// Draw
		for (int i = 0; i < count; i++) {
			Renderer.Draw(
				sprite,
				centerX, centerY,
				sprite.PivotX - pivotSwing,
				sprite.PivotY - pivotSwing,
				localFrame * rotateSpeed + i * 360 / count,
				sprite.GlobalWidth * scale / 1000, sprite.GlobalHeight * scale / 1000,
				tint, z
			);
		}
	}


	public static void DrawExplosionRing (int spriteID, int centerX, int centerY, int radius, int localFrame, int duration, Color32 tint, int z = int.MaxValue - 1) {

		if (!Renderer.TryGetSprite(spriteID, out var ring, true)) return;

		float ease01Ex = Ease.OutCubic((float)localFrame / duration);
		tint.a = (byte)Util.LerpUnclamped(tint.a, 0, ease01Ex);
		int ringRadius = radius * 9 / 10;
		Renderer.DrawSlice(
			ring, centerX, centerY, 500, 500,
			(int)(ease01Ex * 720), ringRadius,
			ringRadius, tint, z - 1
		);

	}


	public static void DrawItemUsageBar (IRect rect, int usage, int maxUsage) {
		rect = rect.Shrink(rect.height / 6);
		int border = rect.height / 10;
		// BG
		Renderer.DrawPixel(rect, Color32.BLACK);
		// Frame
		Renderer.DrawSlice(BuiltInSprite.FRAME_16, rect, border, border, border, border, Color32.GREY_128);
		// Bar
		Renderer.DrawPixel(
			new IRect(rect.x, rect.y, rect.width * usage / maxUsage, rect.height).Shrink(border),
			Color32.Lerp(Color32.RED, Color32.GREEN, (float)usage / maxUsage)
		);
	}


	public static void DrawAllCollidersAsGizmos (
		int physicsMask = PhysicsMask.ALL,
		Int2 offset = default,
		float brightness = 1f,
		bool ignoreNonOnewayTrigger = false,
		bool ignoreOnewayTrigger = false,
		bool useTechEffect = false,
		Color32[] layerTints = null
	) {

		layerTints ??= COLLIDER_TINTS;

		// Init Cells
		if (CellPhysicsCells.Count == 0) {
			try {
				var layers = Util.GetStaticFieldValue(typeof(Physics), Physics.LayersName) as System.Array;
				for (int layerIndex = 0; layerIndex < PhysicsLayer.COUNT; layerIndex++) {
					var layerObj = layers.GetValue(layerIndex);
					CellPhysicsCells.Add(Util.GetFieldValue(layerObj, Physics.CellsName) as PhysicsCell[,,]);
				}
			} catch (Exception ex) { Debug.LogException(ex); }
			if (CellPhysicsCells.Count == 0) CellPhysicsCells.Add(null);
		}

		// Draw Cells
		if (CellPhysicsCells.Count > 0 && CellPhysicsCells[0] != null) {
			int thick = GUI.Unify(1);
			var cameraRect = Renderer.CameraRect;
			float framePingPong01 = Ease.InOutQuad(Game.GlobalFrame.PingPong(120) / 120f);
			framePingPong01 /= 4f;
			for (int layer = 0; layer < CellPhysicsCells.Count; layer++) {
				try {
					if ((physicsMask & (1 << layer)) == 0) continue;
					var tint = layerTints[layer.Clamp(0, layerTints.Length - 1)];
					tint = Color32.LerpUnclamped(Color32.BLACK, tint, brightness);
					var cells = CellPhysicsCells[layer];
					int cellWidth = cells.GetLength(0);
					int cellHeight = cells.GetLength(1);
					int celDepth = cells.GetLength(2);
					for (int y = 0; y < cellHeight; y++) {
						for (int x = 0; x < cellWidth; x++) {
							for (int d = 0; d < celDepth; d++) {
								var cell = cells[x, y, d];
								if (cell.Frame != Physics.CurrentFrame) break;
								if (cell.IsTrigger) {
									bool isNonOnewayTrigger = !Util.HasOnewayTag(cell.Tag) && !cell.Tag.HasAny(Tag.Climb | Tag.ClimbStable);
									if (ignoreNonOnewayTrigger && isNonOnewayTrigger) continue;
									if (ignoreOnewayTrigger && !isNonOnewayTrigger) continue;
								}
								if (!cell.Rect.Overlaps(cameraRect)) continue;
								// Effect
								var rect = cell.Rect.Shift(offset);
								var _tint = tint;
								if (useTechEffect) {
									const int RANDOM_SHAKE = 5;
									_tint.a = (byte)(_tint.a - framePingPong01 * 255).Clamp(0, 255);
									rect.x += Util.QuickRandom(-RANDOM_SHAKE, RANDOM_SHAKE + 1);
									rect.y += Util.QuickRandom(-RANDOM_SHAKE, RANDOM_SHAKE + 1);
								}
								// Frame
								Game.DrawGizmosRect(rect.Edge(Direction4.Down, thick), _tint);
								Game.DrawGizmosRect(rect.Edge(Direction4.Up, thick), _tint);
								Game.DrawGizmosRect(rect.Edge(Direction4.Left, thick), _tint);
								Game.DrawGizmosRect(rect.Edge(Direction4.Right, thick), _tint);
								// Cross
								if (!cell.IsTrigger) {
									Game.DrawGizmosLine(rect.x, rect.y, rect.xMax, rect.yMax, thick, _tint);
									Game.DrawGizmosLine(rect.xMax, rect.y, rect.x, rect.yMax, thick, _tint);
								}
							}
						}
					}
				} catch (Exception ex) { Debug.LogException(ex); }
			}
		}
	}


	public static void DrawBullet (Bullet bullet, int artworkID, bool facingRight, int rotation, int scale, int z = int.MaxValue - 16) {
		if (!Renderer.TryGetSprite(artworkID, out var sprite, false)) return;
		int facingSign = facingRight ? 1 : -1;
		int x = bullet.X + bullet.Width / 2;
		int y = bullet.Y + bullet.Height / 2;
		if (Renderer.TryGetAnimationGroup(artworkID, out var aniGroup)) {
			Renderer.DrawAnimation(
				aniGroup,
				x, y,
				sprite.PivotX,
				sprite.PivotY,
				rotation,
				facingSign * sprite.GlobalWidth * scale / 1000,
				sprite.GlobalHeight * scale / 1000,
				Game.GlobalFrame - bullet.SpawnFrame,
				z
			);
		} else {
			Renderer.Draw(
				artworkID,
				x, y,
				sprite.PivotX,
				sprite.PivotY,
				rotation,
				facingSign * sprite.GlobalWidth * scale / 1000,
				sprite.GlobalHeight * scale / 1000,
				z
			);
		}
	}


	public static int GetItemTypeIcon (int itemID) {
		int typeIcon;
		var item = ItemSystem.GetItem(itemID);
		if (item is Equipment eq) {
			// Equipment
			if (item is Weapon) {
				typeIcon = ITEM_TYPE_ICONS[^3];
			} else {
				typeIcon = ITEM_TYPE_ICONS[(int)eq.EquipmentType];
			}
		} else if (item is Food) {
			// Food
			typeIcon = ITEM_TYPE_ICONS[^2];
		} else {
			// General
			typeIcon = ITEM_TYPE_ICONS[^1];
		}
		return typeIcon;
	}


	public static void DrawClockHands (IRect rect, int handCode, int thickness, int thicknessSecond, Color32 tint) => DrawClockHands(rect.CenterX(), rect.CenterY(), rect.height, handCode, thickness, thicknessSecond, tint);
	public static void DrawClockHands (int centerX, int centerY, int radius, int handCode, int thickness, int thicknessSecond, Color32 tint, int z = int.MinValue) {

		Time01_to_TimeDigit(Sky.InGameDaytime01, out int hour, out int min, out int second);

		// Sec
		Renderer.Draw(
			handCode, centerX, centerY,
			500, 0, second * 360 / 60,
			thicknessSecond, radius * 900 / 2000, tint, z
		);
		// Min
		Renderer.Draw(
			handCode, centerX, centerY,
			500, 0, min * 360 / 60,
			thickness, radius * 800 / 2000, tint, z
		);
		// Hour
		Renderer.Draw(
			handCode, centerX, centerY,
			500, 0, (hour * 360 / 12) + (min * 360 / 12 / 60),
			thickness, radius * 400 / 2000, tint, z
		);
	}


	public static void DrawClockPendulum (int artCodeLeg, int artCodeHead, int x, int y, int length, int thickness, int headSize, int maxRot, int deltaX = 0) {
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


	public static void Time01_to_TimeDigit (float time01, out int hour, out int minute, out int second) {
		hour = (int)(time01 * 24f).UMod(24f);
		minute = (int)(hour * 24f * 60f).UMod(60f);
		second = (int)(hour * 24f * 60f * 60f).UMod(60f);
	}


	public static float TimeDigit_to_Time01 (int hour, int minute, int second) => ((hour + (minute + second / 60f) / 60f) / 24f).UMod(1f);


	public static void SpiralSpinningCellEffect (int localFrame, int pointX, int pointY, int duration, int cellIndexStart, bool reverseSpin = false) {
		if (!Renderer.GetCells(out var cells, out int count)) return;
		for (int i = cellIndexStart; i < count; i++) {
			float lerp01 = localFrame.PingPong(duration / 2) / (duration / 2f);
			int offsetX = (int)((1f - lerp01) * Const.CEL * Util.Sin(lerp01 * 720f * Util.Deg2Rad));
			int offsetY = (int)((1f - lerp01) * Const.CEL * Util.Cos(lerp01 * 720f * Util.Deg2Rad));
			var cell = cells[i];
			cell.X += offsetX;
			cell.Y += offsetY;
			int spinSpeed = reverseSpin ? -720 : 720;
			cell.RotateAround(localFrame * spinSpeed / duration, pointX + offsetX, pointY + offsetY);
			cell.ScaleFrom(
				Util.RemapUnclamped(0, duration / 2, 1000, 0, localFrame.PingPong(duration / 2)),
				pointX + offsetX, pointY + offsetY
			);
		}
	}


	public static void DrawLoopingActivatedHighlight (IRect targetRect, Color32 tint, int lineCount = 4, int duration = 22) {
		int localFrame = Game.GlobalFrame % duration;
		var rect = targetRect;
		Renderer.SetLayerToAdditive();
		for (int i = 0; i < lineCount; i++) {
			tint.a = (byte)(i == lineCount - 1 ? Util.RemapUnclamped(0, duration, 64, 0, localFrame) : 64);
			rect.y = targetRect.y;
			rect.height = i * targetRect.height / lineCount;
			rect.height += Util.RemapUnclamped(0, duration, 0, targetRect.height / lineCount, localFrame);
			Renderer.Draw(BuiltInSprite.SOFT_LINE_H, rect, tint);
		}
		Renderer.SetLayerToDefault();
	}


	// FrameBasedValue Load/Save
	public static bool NameAndIntFile_to_List (List<(string name, int value)> list, string path) {
		if (!Util.FileExists(path)) return false;
		list.AddRange(Util.ForAllNameAndIntInFile(path));
		return true;
	}


	public static bool List_to_FrameBasedFields (List<(string name, int value)> list, object target) {
		if (target == null || list == null) return false;
		var targetType = target.GetType();
		foreach (var (name, value) in list) {
			try {
				if (string.IsNullOrWhiteSpace(name)) continue;
				if (Util.GetField(targetType, name) is not FieldInfo field) continue;
				var valueObj = field.GetValue(target);
				if (valueObj is FrameBasedInt fbInt) {
					fbInt.BaseValue = value;
				} else if (valueObj is FrameBasedBool fbBool) {
					fbBool.BaseValue = value == 1;
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
		return true;
	}


	public static void FrameBasedFields_to_List (object target, List<(string name, int value)> list) {
		if (target == null || list == null || list.Count == 0) return;
		foreach (var (field, value) in target.ForAllFields<FrameBasedValue>(BindingFlags.Public | BindingFlags.Instance)) {
			if (value == null) continue;
			try {
				if (value is FrameBasedInt iValue) {
					list.Add((field.Name, iValue.BaseValue));
				} else if (value is FrameBasedBool bValue) {
					list.Add((field.Name, bValue.BaseValue ? 1 : 0));
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
	}


	public static void Pairs_to_NameAndIntFile (IEnumerable<KeyValuePair<string, int>> list, string path) {
		if (list == null) return;
		Util.CreateFolder(Util.GetParentPath(path));
		using var fs = new FileStream(path, FileMode.Create);
		using var sw = new StreamWriter(fs);
		foreach (var (name, value) in list) {
			try {
				sw.Write(name);
				sw.Write(':');
				sw.Write(value);
				sw.Write('\n');
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
	}


	// Input
	internal static readonly Dictionary<GamepadKey, int> GAMEPAD_CODE = new() {
		{ GamepadKey.DpadLeft, BuiltInSprite.GAMEPAD_LEFT},
		{ GamepadKey.DpadRight, BuiltInSprite.GAMEPAD_RIGHT},
		{ GamepadKey.DpadUp, BuiltInSprite.GAMEPAD_UP},
		{ GamepadKey.DpadDown, BuiltInSprite.GAMEPAD_DOWN },
		{ GamepadKey.South, BuiltInSprite.GAMEPAD_SOUTH},
		{ GamepadKey.North, BuiltInSprite.GAMEPAD_NORTH},
		{ GamepadKey.East, BuiltInSprite.GAMEPAD_EAST},
		{ GamepadKey.West, BuiltInSprite.GAMEPAD_WEST},
		{ GamepadKey.Select, BuiltInSprite.GAMEPAD_SELECT},
		{ GamepadKey.Start, BuiltInSprite.GAMEPAD_START},
		{ GamepadKey.LeftTrigger, BuiltInSprite.GAMEPAD_LEFT_TRIGGER},
		{ GamepadKey.RightTrigger, BuiltInSprite.GAMEPAD_RIGHT_TRIGGER},
		{ GamepadKey.LeftShoulder, BuiltInSprite.GAMEPAD_LEFT_SHOULDER},
		{ GamepadKey.RightShoulder, BuiltInSprite.GAMEPAD_RIGHT_SHOULDER},
	};


	public static bool MouseInside (this IRect rect) => Game.CursorInScreen && rect.Contains(Input.MouseGlobalPosition);


	// Map
	public static void RedirectForRule (WorldStream stream, IRect unitRange, int z) {
		unitRange = unitRange.Expand(1);
		for (int i = unitRange.xMin; i < unitRange.xMax; i++) {
			for (int j = unitRange.yMin; j < unitRange.yMax; j++) {
				RedirectForRule(stream, i, j, z, BlockType.Level);
				RedirectForRule(stream, i, j, z, BlockType.Background);
			}
		}
	}
	public static void RedirectForRule (WorldStream stream, int i, int j, int z, BlockType type) {
		int id = stream.GetBlockAt(i, j, z, type);
		if (id == 0) return;
		int oldID = id;
		if (!Renderer.TryGetSprite(id, out var sprite) || sprite.Group == null || !sprite.Group.WithRule) return;
		var group = sprite.Group;
		int ruleIndex = GetRuleIndex(
			group.Sprites, group.ID,
			stream.GetBlockAt(i - 1, j + 1, z, type),
			stream.GetBlockAt(i + 0, j + 1, z, type),
			stream.GetBlockAt(i + 1, j + 1, z, type),
			stream.GetBlockAt(i - 1, j + 0, z, type),
			stream.GetBlockAt(i + 1, j + 0, z, type),
			stream.GetBlockAt(i - 1, j - 1, z, type),
			stream.GetBlockAt(i + 0, j - 1, z, type),
			stream.GetBlockAt(i + 1, j - 1, z, type)
		);
		if (ruleIndex < 0 || ruleIndex >= group.Count) return;
		int newID = group.Sprites[ruleIndex].ID;
		if (newID == oldID) return;
		stream.SetBlockAt(i, j, z, type, newID);
	}


	public static int GetRuleIndex (IList<AngeSprite> sprites, int ruleID, int tl, int tm, int tr, int ml, int mr, int bl, int bm, int br) {
		int count = sprites.Count;
		for (int i = 0; i < count; i++) {
			var sprite = sprites[i];
			var rule = sprite.Rule;
			if (!CheckForTile(ruleID, tl, rule.RuleTL)) continue;
			if (!CheckForTile(ruleID, tm, rule.RuleT)) continue;
			if (!CheckForTile(ruleID, tr, rule.RuleTR)) continue;
			if (!CheckForTile(ruleID, ml, rule.RuleL)) continue;
			if (!CheckForTile(ruleID, mr, rule.RuleR)) continue;
			if (!CheckForTile(ruleID, bl, rule.RuleBL)) continue;
			if (!CheckForTile(ruleID, bm, rule.RuleB)) continue;
			if (!CheckForTile(ruleID, br, rule.RuleBR)) continue;
			return TryRandom(sprites, i, count);
		}
		return -1;
		// Func
		static bool CheckForTile (int ruleID, int targetID, Rule targetRule) => targetRule switch {
			Rule.SameTile => GpID(targetID) == ruleID,
			Rule.NotSameTile => GpID(targetID) != ruleID,
			Rule.AnyTile => targetID != 0,
			Rule.Empty => targetID == 0,
			_ => true,
		};
		static int TryRandom (IList<AngeSprite> sprites, int resultIndex, int count) {
			int lastIndex = resultIndex;
			bool jumpOut = false;
			var resultRule = sprites[resultIndex].Rule;
			for (int i = resultIndex + 1; i < count; i++) {
				var rule = sprites[i].Rule;
				for (int j = 0; j < 8; j++) {
					if (rule[j] != resultRule[j]) {
						jumpOut = true;
						break;
					}
				}
				if (jumpOut) break;
				lastIndex = i;
			}
			if (resultIndex != lastIndex) resultIndex = Util.QuickRandom(resultIndex, lastIndex + 1);
			return resultIndex;
		}
		static int GpID (int spriteID) => Renderer.TryGetSprite(spriteID, out var sprite) && sprite.Group != null ? sprite.Group.ID : 0;
	}


	public static bool BreakEntityBlock (Entity target, bool dropItemAfterPick = true) {

		if (!target.MapUnitPos.HasValue || target is not IBlockEntity eBlock) {
			InvokeObjectBreak(target.TypeID, target.Rect);
			return false;
		}

		target.Active = false;
		var mapPos = target.MapUnitPos.Value;

		// Remove from Map
		WorldSquad.Front.SetBlockAt(mapPos.x, mapPos.y, BlockType.Entity, 0);

		// Event
		eBlock.OnEntityPicked();
		if (dropItemAfterPick && ItemSystem.HasItem(target.TypeID)) {
			// Drop Item
			ItemSystem.SpawnItem(target.TypeID, target.X, target.Y, jump: false);
		}

		InvokeObjectBreak(target.TypeID, target.Rect);
		return true;
	}


	public static bool PickBlockAt (int unitX, int unitY, bool allowPickBlockEntity = true, bool allowPickLevelBlock = true, bool allowPickBackgroundBlock = true, bool dropItemAfterPicked = true, bool allowMultiplePick = false) {

		bool result = false;

		// Try Pick Block Entity
		if (allowPickBlockEntity) {
			var hits = Physics.OverlapAll(
				PhysicsMask.MAP,
				new IRect(unitX.ToGlobal() + 1, unitY.ToGlobal() + 1, Const.CEL - 2, Const.CEL - 2),
				out int count, null, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				var e = hits[i].Entity;
				if (e is not IBlockEntity eBlock) continue;
				e.Active = false;
				e.IgnoreReposition = true;
				var mapPos = e.MapUnitPos;

				// Remove from Map
				if (mapPos.HasValue) {
					WorldSquad.Front.SetBlockAt(mapPos.Value.x, mapPos.Value.y, BlockType.Entity, 0);
					result = true;
				}

				// Event
				eBlock.OnEntityPicked();
				if (dropItemAfterPicked && ItemSystem.HasItem(e.TypeID)) {
					// Drop Item
					var rect = e.Rect;
					ItemSystem.SpawnItem(e.TypeID, rect.CenterX(), rect.CenterY(), jump: false);
					InvokeBlockPicked(e.TypeID, rect);
				} else {
					// Break
					InvokeObjectBreak(e.TypeID, new IRect(e.X, e.Y, Const.CEL, Const.CEL));
				}

				// Refresh Nearby
				int nearByCount = Physics.OverlapAll(
					BlockOperationCache, PhysicsMask.MAP,
					new IRect((e.X + 1).ToUnifyGlobal(), (e.Y + 1).ToUnifyGlobal(), Const.CEL, Const.CEL).Expand(Const.CEL - 1),
					e, OperationMode.ColliderAndTrigger
				);
				for (int j = 0; j < nearByCount; j++) {
					var nearByHit = BlockOperationCache[j];
					if (nearByHit.Entity is not IBlockEntity nearByEntity) continue;
					nearByEntity.OnEntityRefresh();
				}

				// Mul Gate
				if (!allowMultiplePick) return true;

			}
		}

		// Try Pick Level Block
		if (allowPickLevelBlock) {
			int blockID = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Level);
			if (blockID != 0) {
				int realBlockID = blockID;
				var blockRect = new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL);

				// Remove from Map
				WorldSquad.Front.SetBlockAt(unitX, unitY, BlockType.Level, 0);
				result = true;

				// Event
				if (Renderer.TryGetSprite(blockID, out var sprite, true) && sprite.Group != null) {
					blockID = sprite.Group.ID;
					// Rule
					if (sprite.Group.WithRule) {
						RedirectForRule(
							WorldSquad.Stream, new IRect(unitX - 1, unitY - 1, 3, 3), Stage.ViewZ
						);
					}
				}

				// Spawn Embedded Item
				int ele = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element);
				if (ele != 0 && ItemSystem.GetItem(ele) is Item _ele && _ele.EmbedIntoLevel) {
					ItemSystem.SpawnItem(ele, unitX.ToGlobal(), unitY.ToGlobal(), 1, false);
					WorldSquad.Front.SetBlockAt(unitX, unitY, BlockType.Element, 0);
				}

				// Final
				if (dropItemAfterPicked && ItemSystem.HasItem(blockID)) {
					// Drop Item
					ItemSystem.SpawnItem(blockID, unitX.ToGlobal() + Const.HALF, unitY.ToGlobal() + Const.HALF, jump: false);
					InvokeBlockPicked(blockID, blockRect);
				} else {
					// Break
					InvokeBlockPicked(realBlockID, blockRect);
				}

				if (!allowMultiplePick) {
					return true;
				}
			}
		}

		// Try Pick BG Block
		if (allowPickBackgroundBlock) {
			int blockID = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Background);
			if (blockID != 0) {
				int realBlockID = blockID;
				var blockRect = new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL);

				// Remove from Map
				WorldSquad.Front.SetBlockAt(unitX, unitY, BlockType.Background, 0);
				result = true;

				// Rule
				if (Renderer.TryGetSprite(blockID, out var sprite, true) && sprite.Group != null) {
					blockID = sprite.Group.ID;
					if (sprite.Group.WithRule) {
						RedirectForRule(
							WorldSquad.Stream, new IRect(unitX - 1, unitY - 1, 3, 3), Stage.ViewZ
						);
					}
				}

				// Spawn Embedded Item
				int ele = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element);
				if (ele != 0 && ItemSystem.GetItem(ele) is Item _ele && _ele.EmbedIntoLevel) {
					ItemSystem.SpawnItem(ele, unitX.ToGlobal() + Const.HALF, unitY.ToGlobal() + Const.HALF, 1, false);
					WorldSquad.Front.SetBlockAt(unitX, unitY, BlockType.Element, 0);
				}

				// Final
				if (dropItemAfterPicked && ItemSystem.HasItem(blockID)) {
					// Drop Item
					ItemSystem.SpawnItem(blockID, unitX.ToGlobal(), unitY.ToGlobal(), jump: false);
					InvokeBlockPicked(realBlockID, blockRect);
				} else {
					// Break
					InvokeBlockPicked(realBlockID, blockRect);
				}

				if (!allowMultiplePick) {
					return true;
				}
			}
		}

		return result;
	}


	public static bool HasPickableBlockAt (int unitX, int unitY, bool allowPickBlockEntity = true, bool allowPickLevelBlock = true, bool allowPickBackgroundBlock = true) {
		// Check for Block Entity
		if (allowPickBlockEntity) {
			var hits = Physics.OverlapAll(
				PhysicsMask.MAP,
				new IRect(unitX.ToGlobal() + 1, unitY.ToGlobal() + 1, Const.CEL - 2, Const.CEL - 2),
				out int count, null, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				if (hits[i].Entity is IBlockEntity) return true;
			}
		}

		// Check for Level Block
		if (allowPickLevelBlock && WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Level) != 0) {
			return true;
		}

		// Check for BG Block
		if (allowPickBackgroundBlock && WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Background) != 0) {
			return true;
		}

		return false;
	}


	public static bool PutBlockTo (int blockID, BlockType blockType, Character pHolder, int targetUnitX, int targetUnitY) {

		bool success = false;
		var squad = WorldSquad.Stream;
		int z = Stage.ViewZ;

		switch (blockType) {
			case BlockType.Level:
			case BlockType.Background:
				// Set Block to Map
				int finalID = 0;
				if (Renderer.TryGetSprite(blockID, out var sprite, false)) {
					finalID = blockID;
				} else if (Renderer.TryGetSpriteFromGroup(blockID, 0, out sprite)) {
					finalID = sprite.ID;
				}
				if (finalID != 0) {
					squad.SetBlockAt(targetUnitX, targetUnitY, z, blockType, finalID);
					// Rule
					if (sprite.Group != null && sprite.Group.WithRule) {
						RedirectForRule(
							squad, new IRect(targetUnitX - 1, targetUnitY - 1, 3, 3), Stage.ViewZ
						);
					}
				} else {
					squad.SetBlockAt(targetUnitX, targetUnitY, z, blockType, blockID);
				}
				success = true;
				break;
			case BlockType.Entity:

				if (!BLOCK_ENTITY_TYPE.IsAssignableFrom(Stage.GetEntityType(blockID))) break;

				// Set to Map & Spawn
				squad.SetBlockAt(targetUnitX, targetUnitY, z, BlockType.Entity, blockID);
				var e = Stage.SpawnEntityFromWorld(blockID, targetUnitX.ToGlobal(), targetUnitY.ToGlobal(), Stage.ViewZ, forceSpawn: true);

				if (e is IBlockEntity bEntity) {
					bEntity.OnEntityPut();
					success = true;

					// Refresh Nearby
					int nearByCount = Physics.OverlapAll(
						BlockOperationCache, PhysicsMask.MAP,
						new IRect((e.X + 1).ToUnifyGlobal(), (e.Y + 1).ToUnifyGlobal(), Const.CEL, Const.CEL).Expand(Const.CEL - 1),
						e, OperationMode.ColliderAndTrigger
					);
					for (int j = 0; j < nearByCount; j++) {
						var nearByHit = BlockOperationCache[j];
						if (nearByHit.Entity is not IBlockEntity nearbyEntity) continue;
						nearbyEntity.OnEntityRefresh();
					}

				}
				break;
		}

		// Reduce Block Count by 1
		if (success) {
			int eqID = Inventory.GetEquipment(pHolder.InventoryID, EquipmentType.HandTool, out int eqCount);
			if (eqID != 0) {
				int newEqCount = (eqCount - 1).GreaterOrEquelThanZero();
				if (newEqCount == 0) eqID = 0;
				Inventory.SetEquipment(pHolder.InventoryID, EquipmentType.HandTool, eqID, newEqCount);
			}
		}

		return success;
	}


	public static bool TryGetEmptyPlaceNearbyForEntity (
		int unitX, int unitY, int z, out int resultUnitX, out int resultUnitY,
		int maxRange = 6, bool preferNoSolidLevel = true
	) {

		var squad = WorldSquad.Front;
		resultUnitX = int.MinValue;
		resultUnitY = int.MinValue;

		// Center Check
		if (squad.GetBlockAt(unitX, unitY, z, BlockType.Entity) == 0) {
			resultUnitX = unitX;
			resultUnitY = unitY;
			if (!preferNoSolidLevel || !IsSolidLevel(unitX, unitY)) {
				return true;
			}
		}

		// Range Check
		for (int range = 1; range <= maxRange; range++) {
			int len = range * 2;
			int l = unitX - range;
			int r = unitX + range;
			int d = unitY - range;
			int u = unitY + range;
			for (int i = 0; i < len; i++) {
				if (Check(l, d + i, ref resultUnitX, ref resultUnitY)) return true;
				if (Check(r, u - i, ref resultUnitX, ref resultUnitY)) return true;
				if (Check(l + i, d, ref resultUnitX, ref resultUnitY)) return true;
				if (Check(r - i, u, ref resultUnitX, ref resultUnitY)) return true;
			}
		}

		// Final
		return resultUnitX != int.MinValue;

		// Func
		bool Check (int x, int y, ref int _resultUnitX, ref int _resultUnitY) {

			if (squad.GetBlockAt(x, y, z, BlockType.Entity) != 0) return false;

			if (!preferNoSolidLevel || !IsSolidLevel(x, y)) {
				_resultUnitX = x;
				_resultUnitY = y;
				return true;
			} else {
				if (_resultUnitX == int.MinValue) {
					_resultUnitX = x;
					_resultUnitY = y;
				}
				return false;
			}

		}

		bool IsSolidLevel (int x, int y) {
			int id = squad.GetBlockAt(x, y, z, BlockType.Level);
			return id != 0 && (!Renderer.TryGetSprite(id, out var sp, false) || !sp.IsTrigger);
		}
	}


	public static void RemoveFromWorldMemory (Entity entity) {
		var mapPos = entity.MapUnitPos;
		if (mapPos.HasValue) {
			WorldSquad.Front.SetBlockAt(mapPos.Value.x, mapPos.Value.y, BlockType.Entity, 0);
		}
	}


	public static bool SearchlightBlockCheck (IBlockSquad squad, Int3 startUnitPoint, Direction8? direction, int unitDistance = Const.MAP, bool reverse = false) {
		(int deltaX, int deltaY) = direction.HasValue ? direction.Value.Normal() : Int2.zero;
		int z = startUnitPoint.z;
		if (deltaX == 0 && deltaY == 0) {
			// No Direction
			int len = unitDistance * 14142 / 10000 / SEARCHLIGHT_DENSITY;
			if (!reverse && ContentCheck(squad, startUnitPoint.x, startUnitPoint.y, z)) {
				return true;
			}
			for (int i = 1; i < len; i++) {
				int index = reverse ? len - 1 - i : i;
				int desIndex = index * SEARCHLIGHT_DENSITY;
				int pL = startUnitPoint.x - desIndex;
				int pR = startUnitPoint.x + desIndex;
				int pD = startUnitPoint.y - desIndex;
				int pU = startUnitPoint.y + desIndex;
				for (int j = 0; j < index; j++) {
					int desJ = j * SEARCHLIGHT_DENSITY;
					if (
						ContentCheck(squad, pL + desJ, startUnitPoint.y + desJ, z) ||
						ContentCheck(squad, startUnitPoint.x + desJ, pU - desJ, z) ||
						ContentCheck(squad, pR - desJ, startUnitPoint.y - desJ, z) ||
						ContentCheck(squad, startUnitPoint.x - desJ, pD + desJ, z)
					) {
						return true;
					}
				}
			}
			if (reverse && ContentCheck(squad, startUnitPoint.x, startUnitPoint.y, z)) {
				return true;
			}
		} else if (deltaX != 0 && deltaY != 0) {
			// Tilt
			int len = unitDistance * 14142 / 10000 / SEARCHLIGHT_DENSITY;
			int desDeltaX = SEARCHLIGHT_DENSITY * deltaX;
			int desDeltaY = SEARCHLIGHT_DENSITY * deltaY;
			for (int i = 0; i < len; i++) {
				int index = reverse ? len - 1 - i : i;
				int wide = index + 1;
				int pointX = startUnitPoint.x + deltaX * index * SEARCHLIGHT_DENSITY;
				int pointY = startUnitPoint.y;
				for (int j = 0; j < wide; j++) {
					if (ContentCheck(squad, pointX, pointY, z)) {
						return true;
					}
					pointX -= desDeltaX;
					pointY += desDeltaY;
				}
			}
		} else {
			// Straight
			int len = unitDistance / SEARCHLIGHT_DENSITY;
			int desDeltaX = SEARCHLIGHT_DENSITY * deltaX;
			int desDeltaY = SEARCHLIGHT_DENSITY * deltaY;
			int desDeltaAltX = SEARCHLIGHT_DENSITY * (1 - deltaX.Abs());
			int desDeltaAltY = SEARCHLIGHT_DENSITY * (1 - deltaY.Abs());
			for (int i = 0; i < len; i++) {
				int index = reverse ? len - 1 - i : i;
				int wide = index + 1;
				int pX = startUnitPoint.x + index * desDeltaX;
				int pY = startUnitPoint.y + index * desDeltaY;
				for (int j = 0; j < wide; j++) {
					if (
						ContentCheck(squad, pX + desDeltaAltX * j, pY + desDeltaAltY * j, z) ||
						ContentCheck(squad, pX - desDeltaAltX * j, pY - desDeltaAltY * j, z)
					) {
						return true;
					}
				}
			}
		}
		return false;
		// Func
		static bool ContentCheck (IBlockSquad squad, int unitX, int unitY, int z) {
			var (level, bg, entity, _) = squad.GetAllBlocksAt(unitX, unitY, z);
			return level != 0 || bg != 0 || entity != 0;
		}
	}


	public static FittingPose GetEntityPose (int typeID, int unitX, int unitY, bool horizontal) {
		bool n = WorldSquad.Front.GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
		bool p = WorldSquad.Front.GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
		return
			n && p ? FittingPose.Mid :
			!n && p ? FittingPose.Left :
			n && !p ? FittingPose.Right :
			FittingPose.Single;
	}


	public static FittingPose GetEntityPose (Entity entity, bool horizontal, int mask, out Entity left_down, out Entity right_up, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) {
		left_down = null;
		right_up = null;
		int unitX = entity.X.ToUnit();
		int unitY = entity.Y.ToUnit();
		int typeID = entity.TypeID;
		bool n = WorldSquad.Front.GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
		bool p = WorldSquad.Front.GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
		if (n) {
			left_down = Physics.GetEntity(typeID, entity.Rect.EdgeOutside(horizontal ? Direction4.Left : Direction4.Down), mask, entity, mode, tag);
		}
		if (p) {
			right_up = Physics.GetEntity(typeID, entity.Rect.EdgeOutside(horizontal ? Direction4.Right : Direction4.Up), mask, entity, mode, tag);
		}
		return
			n && p ? FittingPose.Mid :
			!n && p ? FittingPose.Left :
			n && !p ? FittingPose.Right :
			FittingPose.Single;
	}


	public static Int3[] ForAllWorldInRange (IRect overlapUnitRange, int z, out int count) {
		int left = overlapUnitRange.xMin.UDivide(Const.MAP);
		int right = (overlapUnitRange.xMax + 1).UDivide(Const.MAP);
		int down = overlapUnitRange.yMin.UDivide(Const.MAP);
		int up = (overlapUnitRange.yMax + 1).UDivide(Const.MAP);
		int index = 0;
		int maxCount = WorldPosInViewCache.Length;
		for (int i = left; i <= right; i++) {
			for (int j = down; j <= up; j++) {
				WorldPosInViewCache[index] = new Int3(i, j, z);
				index++;
				if (index >= maxCount) {
					goto _END_;
				}
			}
		}
		_END_:;
		count = index;
		return WorldPosInViewCache;
	}


	public static Int3[] ForAllExistsWorldInRange (IBlockSquad squad, IRect overlapUnitRange, int z, out int count) {
		int left = overlapUnitRange.xMin.UDivide(Const.MAP);
		int right = (overlapUnitRange.xMax + 1).UDivide(Const.MAP);
		int down = overlapUnitRange.yMin.UDivide(Const.MAP);
		int up = (overlapUnitRange.yMax + 1).UDivide(Const.MAP);
		int index = 0;
		int maxCount = WorldPosInViewCache.Length;
		for (int i = left; i <= right; i++) {
			for (int j = down; j <= up; j++) {
				var worldPos = new Int3(i, j, z);
				if (!squad.WorldExists(worldPos)) continue;
				WorldPosInViewCache[index] = worldPos;
				index++;
				if (index >= maxCount) {
					goto _END_;
				}
			}
		}
		_END_:;
		count = index;
		return WorldPosInViewCache;
	}


	public static void PaintBlock (int unitX, int unitY, int blockColorID, bool overlapExistingElement = false) {
		var squad = WorldSquad.Front;
		var (lv, bg, _, ele) = squad.GetAllBlocksAt(unitX, unitY, Stage.ViewZ);
		if (lv == 0 && bg == 0) return;
		if (!overlapExistingElement && !BlockColoringSystem.TryGetColor(ele, out _) && ele != 0) return;
		squad.SetBlockAt(unitX, unitY, BlockType.Element, blockColorID);
	}


	// System Number
	public static int SystemNumberID_to_Number (int id) => SYSTEM_NUMBER_POOL.TryGetValue(id, out int number) ? number : -1;


	public static int Number_to_SystemNumberID (int number) => SYSTEM_NUMBER_POOL_ALT.TryGetValue(number, out int id) ? id : 0;


	public static bool ReadSystemNumber (IBlockSquad squad, int unitX, int unitY, int z, out int number) {
		number = 0;
		bool hasH = ReadSystemNumber(squad, unitX, unitY, z, Direction4.Right, out int numberH);
		bool hasV = ReadSystemNumber(squad, unitX, unitY, z, Direction4.Down, out int numberV);
		if (!hasH && !hasV) return false;
		if (hasH == hasV) {
			number = Util.Max(numberH, numberV);
			return true;
		} else {
			number = hasH ? numberH : numberV;
			return true;
		}
	}


	public static bool ReadSystemNumber (IBlockSquad squad, int unitX, int unitY, int z, Direction4 direction, out int number) {

		number = 0;
		int digitCount = 0;
		int x = unitX;
		int y = unitY;
		var delta = direction.Normal();

		// Find Start
		int left = int.MinValue;
		int down = int.MinValue;
		while (HasSystemNumber(squad, x, y, z)) {
			left = x;
			down = y;
			x -= delta.x;
			y -= delta.y;
		}
		if (left == int.MinValue) return false;

		// Get Number
		x = left;
		y = down;
		while (digitCount < 9 && TryGetSingleSystemNumber(squad, x, y, z, out int digit)) {
			number *= 10;
			number += digit;
			digitCount++;
			x += delta.x;
			y += delta.y;
		}
		return digitCount > 0;
	}


	public static bool HasSystemNumber (IBlockSquad squad, int unitX, int unitY, int z) {
		int id = squad.GetBlockAt(unitX, unitY, z, BlockType.Element);
		return id != 0 && SystemNumberID_to_Number(id) != -1;
	}


	public static bool TryGetSingleSystemNumber (IBlockSquad squad, int unitX, int unitY, int z, out int digitValue) {
		int id = squad.GetBlockAt(unitX, unitY, z, BlockType.Element);
		digitValue = SystemNumberID_to_Number(id);
		return digitValue >= 0;
	}


	// Block Aiming
	public static bool GetAimingBuilderPositionFromMouse (Character holder, int unitRange, BlockType blockType, out int targetUnitX, out int targetUnitY, out bool inRange) {

		var mouseUnitPos = Input.MouseGlobalPosition.ToUnit();
		targetUnitX = mouseUnitPos.x;
		targetUnitY = mouseUnitPos.y;

		// Range Check
		int holderUnitX = holder.Rect.CenterX().ToUnit();
		int holderUnitY = (holder.Rect.y + Const.HALF).ToUnit();
		if (
			!targetUnitX.InRangeInclude(holderUnitX - unitRange, holderUnitX + unitRange) ||
			!targetUnitY.InRangeInclude(holderUnitY - unitRange, holderUnitY + unitRange)
		) {
			inRange = false;
			return false;
		}
		inRange = true;

		// Overlap with Holder Check
		var mouseRect = new IRect(targetUnitX.ToGlobal(), targetUnitY.ToGlobal(), Const.CEL, Const.CEL);
		if (holder.Rect.Overlaps(mouseRect)) {
			return false;
		}

		// Block Empty Check
		return ValidForPutBlock(targetUnitX, targetUnitY, blockType);

	}


	public static bool GetAimingBuilderPositionFromKey (Character holder, BlockType blockType, out int targetUnitX, out int targetUnitY, bool ignoreValid = false) {

		bool valid;
		var aim = holder.Attackness.AimingDirection;
		var aimNormal = aim.Normal();
		if (!holder.Movement.IsClimbing) {
			// Normal
			int pointX = holder.Rect.CenterX();
			int pointY = aim.IsTop() ? holder.Rect.yMax - Const.HALF / 2 : holder.Rect.y + Const.HALF;
			targetUnitX = pointX.ToUnit() + aimNormal.x;
			targetUnitY = pointY.ToUnit() + aimNormal.y;
		} else {
			// Climbing
			int pointX = holder.Rect.CenterX();
			int pointY = holder.Rect.yMax - Const.HALF / 2;
			targetUnitX = holder.Movement.FacingRight ? pointX.ToUnit() + 1 : pointX.ToUnit() - 1;
			targetUnitY = pointY.ToUnit() + aimNormal.y;
		}

		valid = ignoreValid || ValidForPutBlock(targetUnitX, targetUnitY, blockType);

		// Redirect
		if (!valid) {
			int oldTargetX = targetUnitX;
			int oldTargetY = targetUnitX;
			if (aim.IsBottom()) {
				if (aim == Direction8.Bottom) {
					targetUnitX += holder.Movement.FacingRight ? 1 : -1;
				}
			} else if (aim.IsTop()) {
				if (aim == Direction8.Top) {
					targetUnitX += holder.Movement.FacingRight ? 1 : -1;
				}
			} else {
				targetUnitY++;
			}
			if (oldTargetX != targetUnitX || oldTargetY != targetUnitY) {
				valid = ignoreValid || ValidForPutBlock(targetUnitX, targetUnitY, blockType);
			}
		}

		return valid;
	}


	private static bool ValidForPutBlock (int unitX, int unitY, BlockType blockType) {
		// Non Entity
		if (blockType != BlockType.Entity) {
			return WorldSquad.Front.GetBlockAt(unitX, unitY, blockType) == 0;
		}
		// Entity
		var rect = new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL);
		return !Physics.Overlap(
			PhysicsMask.LEVEL, rect, null, OperationMode.ColliderOnly
		) && !Physics.Overlap(
			PhysicsMask.ENTITY, rect, null, OperationMode.ColliderAndTrigger
		);
	}


	public static bool GetAimingPickerPositionFromMouse (
		Character holder, int unitRange, out int targetUnitX, out int targetUnitY, out bool inRange,
		bool allowPickBlockEntity = true, bool allowPickLevelBlock = true, bool allowPickBackgroundBlock = true
	) {

		var mouseUnitPos = Input.MouseGlobalPosition.ToUnit();
		targetUnitX = mouseUnitPos.x;
		targetUnitY = mouseUnitPos.y;

		// Range Check
		int holderUnitX = holder.Rect.CenterX().ToUnit();
		int holderUnitY = (holder.Rect.y + Const.HALF).ToUnit();
		inRange = targetUnitX.InRangeInclude(holderUnitX - unitRange, holderUnitX + unitRange) &&
				targetUnitY.InRangeInclude(holderUnitY - unitRange, holderUnitY + unitRange);

		if (!inRange) return false;

		// Pickable Check
		return HasPickableBlockAt(
			targetUnitX, targetUnitY, allowPickBlockEntity, allowPickLevelBlock, allowPickBackgroundBlock
		);
	}


	public static bool GetAimingPickerPositionFromKey (
		Character pHolder, out int targetUnitX, out int targetUnitY,
		bool allowPickBlockEntity = true, bool allowPickLevelBlock = true, bool allowPickBackgroundBlock = true
	) {

		var aim = pHolder.Attackness.AimingDirection;
		var aimNormal = aim.Normal();
		int pointX = aim.IsTop() ? pHolder.Rect.CenterX() : pHolder.Movement.FacingRight ? pHolder.Rect.xMax - 16 : pHolder.Rect.xMin + 16;
		int pointY = pHolder.Rect.yMax - 16;
		targetUnitX = pointX.ToUnit() + aimNormal.x;
		targetUnitY = pointY.ToUnit() + aimNormal.y;
		bool hasTraget = HasPickableBlockAt(
			targetUnitX, targetUnitY,
			allowPickBlockEntity, allowPickLevelBlock, allowPickBackgroundBlock
		);

		// Redirect
		if (!hasTraget) {
			int oldTargetX = targetUnitX;
			int oldTargetY = targetUnitX;
			if (aim.IsBottom()) {
				if (aim == Direction8.Bottom) {
					targetUnitX += pointX.UMod(Const.CEL) < Const.HALF ? -1 : 1;
				}
			} else if (aim.IsTop()) {
				if (aim == Direction8.Top) {
					targetUnitX += pHolder.Movement.FacingRight ? 1 : -1;
				}
			} else {
				targetUnitY--;
			}
			if (oldTargetX != targetUnitX || oldTargetY != targetUnitY) {
				hasTraget = HasPickableBlockAt(
					targetUnitX, targetUnitY,
					allowPickBlockEntity, allowPickLevelBlock, allowPickBackgroundBlock
				);
			}
		}

		return hasTraget;

	}


	// Item
	public static void DrawItemShortInfo (int itemID, IRect panelRect, int z, int armorIcon, int armorEmptyIcon, Color32 tint) {

		var item = ItemSystem.GetItem(itemID);
		if (item == null) return;

		// Equipment
		if (item is Equipment equipment) {
			switch (equipment.EquipmentType) {
				case EquipmentType.HandTool:
					break;
				case EquipmentType.Jewelry:
				case EquipmentType.BodyArmor:
				case EquipmentType.Helmet:
				case EquipmentType.Shoes:
				case EquipmentType.Gloves:
					if (equipment is IProgressiveItem progItem) {
						int progress = progItem.Progress;
						int totalProgress = progItem.TotalProgress;
						var rect = new IRect(panelRect.x, panelRect.y, panelRect.height, panelRect.height);
						for (int i = 0; i < totalProgress - 1; i++) {
							Renderer.Draw(i < progress ? armorIcon : armorEmptyIcon, rect, tint, z);
							rect.x += rect.width;
						}
					}
					break;
			}
		}


	}


	public static void SpawnItemFromMap (IBlockSquad squad, int unitX, int unitY, int z, int maxDeltaX = 1024, int maxDeltaY = 1024, int placeHolderID = 0) {
		for (int y = 1; y < maxDeltaY; y++) {
			int currentUnitY = unitY - y;
			int right = -1;
			for (int x = 0; x < maxDeltaX; x++) {
				int id = squad.GetBlockAt(unitX + x, currentUnitY, z, BlockType.Element);
				if (id == 0 || !ItemSystem.HasItem(id)) break;
				right = x;
			}
			if (right == -1) break;
			int itemLocalIndex = Util.QuickRandom(0, right + 1);
			int itemID = squad.GetBlockAt(unitX + itemLocalIndex, currentUnitY, z, BlockType.Element);
			if (ItemSystem.SpawnItem(itemID, unitX.ToGlobal(), unitY.ToGlobal(), 1, true) != null) {
				// Replace with Placeholder
				if (placeHolderID != 0) {
					squad.SetBlockAt(unitX + itemLocalIndex, currentUnitY, z, BlockType.Element, placeHolderID);
				}
			}
		}
	}


	// Buff
	public static void BroadcastBuff (IRect range, int buffID, int duration = 1) {
		var hits = Physics.OverlapAll(PhysicsMask.CHARACTER, range, out int count);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Character character) continue;
			character.Buff.GiveBuff(buffID, duration);
		}
	}


	public static void BroadcastBuff (int x, int y, int radius, int buffID, int duration = 1) {
		var range = new IRect(x - radius, y - radius, radius * 2, radius * 2);
		var hits = Physics.OverlapAll(PhysicsMask.CHARACTER, range, out int count);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Character character) continue;
			var hitRect = hit.Rect;
			if (!Util.OverlapRectCircle(radius, x, y, hitRect.x, hitRect.y, hitRect.xMax, hitRect.yMax)) continue;
			character.Buff.GiveBuff(buffID, duration);
		}
	}


	// Event
	public static void InvokeObjectBreak (int spriteID, IRect rect) => OnObjectBreak?.Invoke(spriteID, rect);
	public static void InvokeObjectFreeFall (int spriteID, int x, int y, int speedX = 0, int speedY = 0, int rotation = int.MinValue, int rotationSpeed = 0, int gravity = 5, bool flipX = false) => OnObjectFreeFall?.Invoke(spriteID, new(x, y, rotation, flipX ? 1 : 0), new(speedX, speedY, rotationSpeed, gravity));
	public static void InvokeBlockPicked (int spriteID, IRect rect) => OnBlockPicked?.Invoke(spriteID, rect);
	public static void InvokeFallIntoWater (Rigidbody rig) => OnFallIntoWater?.Invoke(rig);
	public static void InvokeCameOutOfWater (Rigidbody rig) => OnCameOutOfWater?.Invoke(rig);
	public static void InvokeItemCollected (Entity collector, int id, int count) => OnItemCollected?.Invoke(collector, id, count);
	public static void InvokeItemLost (Character holder, int id) => OnItemLost?.Invoke(holder, id);
	public static void InvokeItemErrorHint (Character holder, int id) => OnItemErrorHint?.Invoke(holder, id);
	public static void InvokeItemDamage (Character holder, int fromID, int toID) => OnItemDamage?.Invoke(holder, fromID, toID);
	public static void InvokeItemUnlocked (int itemID) => OnItemUnlocked?.Invoke(itemID);
	public static void InvokeCheatPerformed (string cheatCode) => OnCheatPerformed?.Invoke(cheatCode);
	public static void InvokeOnFootStepped (int x, int y, int groundedID) => OnFootStepped?.Invoke(x, y, groundedID);


	// Analysys
	public static void RunBuiltInSpriteAnalysys (bool onlyLogWhenWarningFounded = false) {
		bool anyWarning = false;
		var sheet = new Sheet();
		bool loaded = sheet.LoadFromDisk(Universe.BuiltIn.BuiltInSheetPath);
		if (!loaded) return;
		foreach (var field in typeof(BuiltInSprite).ForAllFields<SpriteCode>(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
			if (field.GetValue(null) is not SpriteCode sp) continue;
			if (!sheet.SpritePool.ContainsKey(sp.ID)) {
				Debug.LogWarning($"Built-in sprite: {sp.Name} not found in engine artwork sheet.");
				anyWarning = true;
			}
		}
		if (!anyWarning && !onlyLogWhenWarningFounded) {
			Debug.Log("[✓] Built-in Sprites are matched with artwork.");
		}
	}


	public static void RunEmptyScriptFileAnalysis (string rootPath, bool onlyLogWhenWarningFounded = false) {
		bool anyWarning = false;
		foreach (string path in Util.EnumerateFiles(rootPath, false, "*.cs")) {
			bool empty = true;
			foreach (string line in Util.ForAllLinesInFile(path)) {
				if (!string.IsNullOrWhiteSpace(line)) {
					empty = false;
					break;
				}
			}
			if (empty) {
				anyWarning = true;
				Debug.LogWarning($"Empty script: {path}");
			}
		}
		if (!anyWarning && !onlyLogWhenWarningFounded) {
			Debug.Log("[✓] No empty script file founded.");
		}
	}


	public static void RunAngeliaCodeAnalysis (bool onlyLogWhenWarningFounded = false, bool fixScriptFileName = false) {

		if (!onlyLogWhenWarningFounded) {
			Debug.Log("-------- AngeliA Project Analysis --------");
		}

		// Check for Empty Script File
		RunEmptyScriptFileAnalysis(Util.GetParentPath(Universe.BuiltIn.UniverseRoot), onlyLogWhenWarningFounded);

		// Sheet
		if (!Util.FileExists(Universe.BuiltIn.GameSheetPath)) {
			Debug.LogWarning($"Artwork sheet file missing. ({Universe.BuiltIn.GameSheetPath})");
		}

		// Check for Hash Collision
		{
			bool anyWarning = false;
			var idPool = new Dictionary<int, string>();
			var sheet = Renderer.MainSheet;
			foreach (var type in typeof(object).AllChildClass(includeAbstract: true, includeInterface: true)) {

				string typeName = type.AngeName();
				int typeID = typeName.AngeHash();

				// Class vs Class
				if (!idPool.TryAdd(typeID, typeName)) {
					string poolName = idPool[typeID];
					if (typeName != poolName) {
						anyWarning = true;
						Debug.LogWarning($"Hash collision between two class names. \"{typeName}\" & \"{poolName}\" (AngeHash = {typeID})");
					}
				}

				// Class vs Sprite
				if (sheet.SpritePool.TryGetValue(typeID, out var sprite)) {
					if (sprite.RealName != typeName && typeID == sprite.ID) {
						anyWarning = true;
						Debug.LogWarning($"Hash collision between Class name and Sprite name. \"{typeName}\" & \"{sprite.RealName}\" (AngeHash = {typeID})");
					}
				}

			}
			if (!anyWarning && !onlyLogWhenWarningFounded) {
				Debug.Log("[✓] No hash collision founded.");
			}
		}

		// Fix Script File Name
		if (fixScriptFileName) {
			bool anyWarning = false;
			foreach (string path in Util.EnumerateFiles(Util.GetParentPath(Universe.BuiltIn.UniverseRoot), false, "*.cs")) {
				string name = Util.GetNameWithExtension(path);
				if (!char.IsLower(name[0]) || name.Length <= 1) continue;
				string oldName = name;
				if (char.IsLower(name[1])) {
					// Turn First Char into Upper Case
					name = $"{char.ToUpper(name[0])}{name[1..]}";
				} else {
					// Remove First Char
					name = name[1..];
				}
				string newPath = Util.CombinePaths(Util.GetParentPath(path), name);
				if (Util.FileExists(newPath)) continue;
				Util.MoveFile(path, newPath);
				anyWarning = true;
				Debug.LogWarning($"Fix first char for script file: {oldName} >> {name}");
			}
			if (!anyWarning && !onlyLogWhenWarningFounded) {
				Debug.Log("[✓] No first char of file name need to fix.");
			}
		}

		// Check for Item Class Name
		if (fixScriptFileName) {
			bool anyWarning = false;
			var eqType = typeof(Equipment);
			foreach (var type in typeof(Item).AllChildClass()) {
				if (type.IsSubclassOf(eqType)) continue;
				string name = type.Name;
				if (name[0] == 'i') continue;
				anyWarning = true;
				Debug.LogWarning($"Item class \"{name}\" is not start with \"i\"");
			}
			if (!anyWarning && !onlyLogWhenWarningFounded) {
				Debug.Log("[✓] No item class name need to fix.");
			}
		}

	}


	// Misc
	public static void DeleteAllEmptyMaps (string mapRoot) {
		foreach (var path in Util.EnumerateFiles(mapRoot, false, AngePath.MAP_SEARCH_PATTERN)) {
			try {
				if (Util.IsExistingFileEmpty(path)) Util.DeleteFile(path);
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
	}


	public static void ResetShoulderAndUpperArmPos (PoseCharacterRenderer rendering, bool resetLeft = true, bool resetRight = true) {

		const int A2G = Const.CEL / Const.ART_CEL;

		var Body = rendering.Body;
		var Hip = rendering.Hip;
		var Head = rendering.Head;
		var UpperLegL = rendering.UpperLegL;
		var LowerLegL = rendering.LowerLegL;
		var FootL = rendering.FootL;

		var FacingRight = Body.Width > 0;


		int bodyHipSizeY = Body.SizeY + Hip.SizeY;
		int targetUnitHeight = rendering.CharacterHeight * A2G / PoseCharacterRenderer.CM_PER_PX - Head.SizeY;
		int legRootSize = UpperLegL.SizeY + LowerLegL.SizeY + FootL.SizeY;
		int defaultCharHeight = bodyHipSizeY + legRootSize;

		int bodyBorderU = Body.Border.up * targetUnitHeight / defaultCharHeight * Body.Height.Abs() / Body.SizeY;
		int bodyBorderL = (FacingRight ? Body.Border.left : Body.Border.right) * Body.Width.Abs() / Body.SizeX;
		int bodyBorderR = (FacingRight ? Body.Border.right : Body.Border.left) * Body.Width.Abs() / Body.SizeX;

		if (resetLeft) {

			var ShoulderL = rendering.ShoulderL;
			var UpperArmL = rendering.UpperArmL;

			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderL.Y = Body.Y + Body.Height - bodyBorderU;
			ShoulderL.Width = ShoulderL.SizeX;
			ShoulderL.Height = ShoulderL.SizeY;
			ShoulderL.PivotX = 1000;
			ShoulderL.PivotY = 1000;

			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmL.Width = UpperArmL.SizeX;
			UpperArmL.Height = UpperArmL.FlexableSizeY;
			UpperArmL.PivotX = 1000;
			UpperArmL.PivotY = 1000;

		}

		if (resetRight) {

			var ShoulderR = rendering.ShoulderR;
			var UpperArmR = rendering.UpperArmR;

			ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
			ShoulderR.Y = Body.Y + Body.Height - bodyBorderU;
			ShoulderR.Width = -ShoulderR.SizeX;
			ShoulderR.Height = ShoulderR.SizeY;
			ShoulderR.PivotX = 1000;
			ShoulderR.PivotY = 1000;

			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.Width = UpperArmR.SizeX;
			UpperArmR.Height = UpperArmR.FlexableSizeY;
			UpperArmR.PivotX = 0;
			UpperArmR.PivotY = 1000;
		}
	}


	public static void HighlightBlink (Cell cell, float pivotX = 0.5f, float pivotY = 0f, bool horizontal = true, bool vertical = true) {
		if (Game.GlobalFrame % 30 > 15) return;
		const int OFFSET = Const.CEL / 20;
		cell.ReturnPivots(pivotX, pivotY);
		if (horizontal) cell.Width += OFFSET * 2;
		if (vertical) cell.Height += OFFSET * 2;
	}


	public static void TryEjectOutsideGround (Rigidbody rig, int collisionMask = 0, int unitRange = 2, int speed = 32) {
		var centerPos = rig.Rect.CenterInt();
		var targetDir = Int2.zero;
		collisionMask = collisionMask == 0 ? rig.CollisionMask : collisionMask;
		if (CheckForDir(centerPos, Int2.up, unitRange, collisionMask)) {
			// Up
			targetDir = Int2.up;
		} else if (CheckForDir(centerPos, Int2.right, unitRange, collisionMask)) {
			// Right
			targetDir = Int2.right;
		} else if (CheckForDir(centerPos, Int2.left, unitRange, collisionMask)) {
			// Left
			targetDir = Int2.left;
		} else if (CheckForDir(centerPos, Int2.down, unitRange, collisionMask)) {
			// Down
			targetDir = Int2.down;
		}
		// Perform Eject
		if (targetDir != Int2.zero) {
			rig.PerformMove(targetDir.x * speed, targetDir.y * speed, ignoreCarry: true);
		}
		// Func
		static bool CheckForDir (Int2 center, Int2 dir, int unitRange, int mask) {
			for (int i = 1; i <= unitRange; i++) {
				int delta = i * Const.CEL;
				if (!Physics.Overlap(mask, IRect.Point(center.x + dir.x * delta, center.y + dir.y * delta))) {
					return true;
				}
			}
			return false;
		}
	}


}