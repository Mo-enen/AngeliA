using System;
using System.Collections.Generic;

namespace AngeliA;

public static partial class FrameworkUtil {


	// VAR
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


	// API
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


}