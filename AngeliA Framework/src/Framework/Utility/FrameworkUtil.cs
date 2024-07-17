using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

public static class FrameworkUtil {


	// Drawing
	public static Cell DrawEnvironmentShadow (Cell source, int offsetX = -Const.HALF / 2, int offsetY = 0, byte alpha = 64, int z = -64 * 1024 + 16) {
		var result = Renderer.DrawPixel(default);
		result.CopyFrom(source);
		result.X += offsetX;
		result.Y += offsetY;
		result.Z = z;
		result.Color = new Color32(0, 0, 0, alpha);
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


	public static bool DrawPoseCharacterAsUI (IRect rect, PoseCharacter character, int animationFrame) => DrawPoseCharacterAsUI(rect, character, animationFrame, out _, out _, null);
	public static bool DrawPoseCharacterAsUI (IRect rect, PoseCharacter character, int animationFrame, out IRect globalRect, out IRect uiRect, System.Action<PoseCharacter> onRenderCharacter) {

		globalRect = default;
		uiRect = default;

		// Draw Player
		int cellIndexStart;
		int cellIndexEnd;
		using (new UILayerScope(ignoreSorting: true)) {
			cellIndexStart = Renderer.GetUsedCellCount();
			int oldAniFrame = character.CurrentAnimationFrame;
			character.CurrentAnimationFrame = animationFrame;
			character.LateUpdate();
			onRenderCharacter?.Invoke(character);
			character.CurrentAnimationFrame = oldAniFrame;
			cellIndexEnd = Renderer.GetUsedCellCount();
		}
		if (cellIndexStart == cellIndexEnd) return false;

		if (!Renderer.GetCells(RenderLayer.UI, out var cells, out int count)) return false;

		// Get Min Max
		bool flying = character.AnimationType == CharacterAnimationType.Fly;
		int originalMinX = character.X - Const.HALF - 16;
		int originalMinY = character.Y - 16 + (flying ? character.PoseRootY / 2 : 0);
		int originalMaxX = character.X + Const.HALF + 16;
		int originalMaxY = character.Y + Const.CEL * 2 + 16;
		if (flying) {
			originalMinY -= Const.HALF;
			originalMaxY -= Const.HALF;
		}
		globalRect.SetMinMax(originalMinX, originalMaxX, originalMinY, originalMaxY);

		// Move Cells
		int originalWidth = originalMaxX - originalMinX;
		int originalHeight = originalMaxY - originalMinY;
		var targetRect = uiRect = rect.Fit(originalWidth, originalHeight, 500, 0);
		int minZ = int.MaxValue;
		for (int i = cellIndexStart; i < count && i < cellIndexEnd; i++) {
			var cell = cells[i];
			minZ = Util.Min(minZ, cell.Z);
			cell.X = targetRect.x + (cell.X - originalMinX) * targetRect.width / originalWidth;
			cell.Y = targetRect.y + (cell.Y - originalMinY) * targetRect.height / originalHeight;
			cell.Width = cell.Width * targetRect.width / originalWidth;
			cell.Height = cell.Height * targetRect.height / originalHeight;
			if (!cell.Shift.IsZero) {
				cell.Shift = new Int4(
					cell.Shift.left * targetRect.width / originalWidth,
					cell.Shift.right * targetRect.width / originalWidth,
					cell.Shift.down * targetRect.height / originalHeight,
					cell.Shift.up * targetRect.height / originalHeight
				);
			}
		}

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


	public static void DrawMagicEncircleAurora (int spriteID, int count, int centerX, int centerY, int localFrame, Color32 tint, int scale = 1000, int rotateSpeed = 16, int swingDuration = 20, int swingAmout = 240, int growDuration = 10, int z = int.MinValue) {
		if (!Renderer.TryGetSprite(spriteID, out var sprite)) return;
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


	// Misc
	public static void DeleteAllEmptyMaps (string mapRoot) {
		foreach (var path in Util.EnumerateFiles(mapRoot, false, $"*.{AngePath.MAP_FILE_EXT}")) {
			try {
				if (Util.IsExistingFileEmpty(path)) Util.DeleteFile(path);
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
	}


	public static void RunAngeliaCodeAnalysis (bool onlyLogWhenWarningFounded = false) {

		bool anyWarning = false;

		// Check for Empty Script File
		foreach (string path in Util.EnumerateFiles(Util.GetParentPath(Universe.BuiltIn.UniverseRoot), false, "*.cs")) {
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

		// Sheet
		if (!Util.FileExists(Universe.BuiltIn.SheetPath)) {
			anyWarning = true;
			Debug.LogWarning("Artwork sheet file not found.");
		}

		// Check for Hash Collision
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



		// End
		if (!anyWarning && !onlyLogWhenWarningFounded) {
			Debug.Log("Everything is fine.");
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


}