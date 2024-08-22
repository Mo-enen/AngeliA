using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

public static class FrameworkUtil {


	private static readonly System.Type BLOCK_ENTITY_TYPE = typeof(IBlockEntity);


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


	public static bool PickEntityBlock (Entity target, bool dropItemAfterPick = true) {
		if (!target.MapUnitPos.HasValue) return false;
		var pos = target.MapUnitPos.Value;
		return PickBlockAt(pos.x, pos.y, true, false, false, dropItemAfterPick, false);
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
					ItemSystem.SpawnItem(e.TypeID, e.X, e.Y, jump: false);
					// Dust
					GlobalEvent.InvokePowderSpawn(e.TypeID, e.Rect);
				} else {
					// Break
					GlobalEvent.InvokeObjectBreak(e.TypeID, new IRect(e.X, e.Y, Const.CEL, Const.CEL));
				}
				if (!allowMultiplePick) {
					return true;
				}
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
				if (dropItemAfterPicked && ItemSystem.HasItem(blockID)) {
					// Drop Item
					ItemSystem.SpawnItem(blockID, unitX.ToGlobal(), unitY.ToGlobal(), jump: false);
					// Dust
					GlobalEvent.InvokePowderSpawn(blockID, blockRect);
				} else {
					// Break
					GlobalEvent.InvokeObjectBreak(realBlockID, blockRect);
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

				if (Renderer.TryGetSprite(blockID, out var sprite, true) && sprite.Group != null) {
					blockID = sprite.Group.ID;
					// Rule
					if (sprite.Group.WithRule) {
						RedirectForRule(
							WorldSquad.Stream, new IRect(unitX - 1, unitY - 1, 3, 3), Stage.ViewZ
						);
					}
				}

				if (dropItemAfterPicked && ItemSystem.HasItem(blockID)) {
					// Drop Item
					ItemSystem.SpawnItem(blockID, unitX.ToGlobal(), unitY.ToGlobal(), jump: false);
					// Dust
					GlobalEvent.InvokePowderSpawn(realBlockID, blockRect);
				} else {
					// Break
					GlobalEvent.InvokeObjectBreak(realBlockID, blockRect);
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


	public static void PutBlockTo (int blockID, BlockType blockType, Character pHolder, int targetUnitX, int targetUnitY) {

		// Set Block to Map
		if (
			Renderer.TryGetSprite(blockID, out var sprite, true) ||
			Renderer.TryGetSpriteFromGroup(blockID, 0, out sprite)
		) {
			WorldSquad.Front.SetBlockAt(targetUnitX, targetUnitY, blockType, sprite.ID);
			// Rule
			if (sprite.Group != null && sprite.Group.WithRule) {
				RedirectForRule(
					WorldSquad.Stream, new IRect(targetUnitX - 1, targetUnitY - 1, 3, 3), Stage.ViewZ
				);
			}
		} else {
			WorldSquad.Front.SetBlockAt(targetUnitX, targetUnitY, blockType, blockID);
		}

		// Spawn Block Entity
		if (
			blockType == BlockType.Entity &&
			BLOCK_ENTITY_TYPE.IsAssignableFrom(Stage.GetEntityType(blockID)) &&
			Stage.SpawnEntityFromWorld(blockID, targetUnitX, targetUnitY, Stage.ViewZ, forceSpawn: true) is IBlockEntity bEntity
		) {
			// Event
			bEntity.OnEntityPut();
		}

		// Reduce Block Count by 1
		int eqID = Inventory.GetEquipment(pHolder.TypeID, EquipmentType.Weapon, out int eqCount);
		if (eqID != 0) {
			int newEqCount = (eqCount - 1).GreaterOrEquelThanZero();
			if (newEqCount == 0) eqID = 0;
			Inventory.SetEquipment(pHolder.TypeID, EquipmentType.Weapon, eqID, newEqCount);
		}
	}


	public static bool TryGetEmptyPlaceNearby (
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
			return id != 0 && (!Renderer.TryGetSprite(id, out var sp) || !sp.IsTrigger);
		}
	}


}