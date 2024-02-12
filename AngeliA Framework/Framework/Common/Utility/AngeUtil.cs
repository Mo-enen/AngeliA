using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace AngeliaFramework {
	public static class AngeUtil {


		// Const
		private const string RULE_TILE_ERROR = "ERROR---";
		private static readonly System.Random GlobalRandom = new(2334768);


		// API
		public static string GetBlockRealName (string name) {
			int hashIndex = name.IndexOf('#');
			if (hashIndex >= 0) {
				name = name[..hashIndex];
			}
			return name.TrimEnd(' ');
		}


		public static int GetOnewayTag (Direction4 gateDirection) =>
		gateDirection switch {
			Direction4.Down => SpriteTag.ONEWAY_DOWN_TAG,
			Direction4.Up => SpriteTag.ONEWAY_UP_TAG,
			Direction4.Left => SpriteTag.ONEWAY_LEFT_TAG,
			Direction4.Right => SpriteTag.ONEWAY_RIGHT_TAG,
			_ => SpriteTag.ONEWAY_UP_TAG,
		};


		public static bool IsOnewayTag (int tag) => tag == SpriteTag.ONEWAY_UP_TAG || tag == SpriteTag.ONEWAY_DOWN_TAG || tag == SpriteTag.ONEWAY_LEFT_TAG || tag == SpriteTag.ONEWAY_RIGHT_TAG;


		public static bool TryGetOnewayDirection (int tag, out Direction4 direction) {
			if (tag == SpriteTag.ONEWAY_LEFT_TAG) {
				direction = Direction4.Left;
				return true;
			}
			if (tag == SpriteTag.ONEWAY_RIGHT_TAG) {
				direction = Direction4.Right;
				return true;
			}
			if (tag == SpriteTag.ONEWAY_DOWN_TAG) {
				direction = Direction4.Down;
				return true;
			}
			if (tag == SpriteTag.ONEWAY_UP_TAG) {
				direction = Direction4.Up;
				return true;
			}
			direction = default;
			return false;
		}


		public static void TryCompileDialogueFiles (string workspace, string exportRoot, bool forceCompile) {

			var ignoreDelete = new HashSet<string>();

			// For all Editable Conversation Files
			foreach (var path in Util.EnumerateFiles(workspace, false, $"*.{AngePath.EDITABLE_CONVERSATION_FILE_EXT}")) {

				string globalName = Util.GetNameWithoutExtension(path);
				string conFolderPath = Util.CombinePaths(exportRoot, globalName);
				ignoreDelete.TryAdd(globalName);

				// Check Dirty
				long modTime = Util.GetFileModifyDate(path);
				long creationTime = Util.GetFileCreationDate(path);
				if (!forceCompile && modTime == creationTime && Util.FolderExists(conFolderPath)) continue;
				Util.SetFileModifyDate(path, creationTime);

				// Delete Existing for All Languages
				Util.DeleteFolder(conFolderPath);
				Util.CreateFolder(conFolderPath);

				// Compile
				var builder = new StringBuilder();
				string currentIso = "en";
				bool contentFlag0 = false;
				bool contentFlag1 = false;
				foreach (string line in Util.ForAllLines(path, Encoding.UTF8)) {

					string trimedLine = line.TrimStart(' ', '\t');

					// Empty
					if (string.IsNullOrWhiteSpace(trimedLine)) {
						if (contentFlag1) {
							contentFlag0 = contentFlag1;
							contentFlag1 = false;
						}
						continue;
					}

					if (trimedLine[0] == '>') {
						// Switch Language
						string iso = trimedLine[1..];
						if (trimedLine.Length > 1 && Util.IsSupportedLanguageISO(iso) && currentIso != iso) {
							// Make File
							string targetPath = Util.CombinePaths(
								conFolderPath,
								$"{currentIso}.{AngePath.CONVERSATION_FILE_EXT}"
							);
							Util.TextToFile(builder.ToString(), targetPath, Encoding.UTF8);
							builder.Clear();
							currentIso = iso;
						}
						contentFlag0 = false;
						contentFlag1 = false;
					} else {
						// Line
						if (trimedLine.StartsWith("\\>")) {
							trimedLine = trimedLine[1..];
						}
						if (trimedLine.Length > 0) {
							// Add Gap
							if (trimedLine[0] == '@') {
								contentFlag0 = false;
								contentFlag1 = false;
							} else {
								if (contentFlag0 && !contentFlag1) {
									builder.Append('\n');
								}
								contentFlag0 = contentFlag1;
								contentFlag1 = true;
							}
							// Append Line
							if (builder.Length != 0) builder.Append('\n');
							builder.Append(trimedLine);
						}
					}
				}

				// Make File for Last Language 
				if (builder.Length != 0) {
					string targetPath = Util.CombinePaths(
						conFolderPath,
						$"{currentIso}.{AngePath.CONVERSATION_FILE_EXT}"
					);
					Util.TextToFile(builder.ToString(), targetPath, Encoding.UTF8);
					builder.Clear();
				}

			}

			// Delete Useless Old Files
			if (ignoreDelete != null) {
				List<string> deleteList = null;
				foreach (var path in Util.EnumerateFolders(exportRoot, true, "*")) {
					if (ignoreDelete.Contains(Util.GetNameWithoutExtension(path))) continue;
					deleteList ??= new List<string>();
					deleteList.Add(path);
				}
				if (deleteList != null) {
					foreach (var path in deleteList) {
						Util.DeleteFolder(path);
						Util.DeleteFile(path + ".meta");
					}
				}
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
					CellRenderer.Draw(isLeft ? heartLeftCode : heartRightCode, rect, 0);
				} else {
					// Empty Heart
					CellRenderer.Draw(isLeft ? emptyHeartLeftCode : emptyHeartRightCode, rect, 0);
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


		public static Int2 GetFlyingFormation (Int2 pos, int column, int instanceIndex) {

			int sign = instanceIndex % 2 == 0 ? -1 : 1;
			int _row = instanceIndex / 2 / column;
			int _column = (instanceIndex / 2 % column + 1) * sign;
			int rowSign = (_row % 2 == 0) == (sign == 1) ? 1 : -1;

			int instanceOffsetX = _column * Const.CEL * 3 / 2 + rowSign * Const.HALF / 2;
			int instanceOffsetY = _row * Const.CEL + Const.CEL - _column.Abs() * Const.HALF / 3;

			return new(pos.x + instanceOffsetX, pos.y + instanceOffsetY);
		}


		public static IEnumerable<KeyValuePair<Int4, string>> ForEachPlayerCustomizeSpritePattern (string[] patterns, string suffix0, string suffix1 = "", string suffix2 = "", string suffix3 = "") {
			if (patterns == null || patterns.Length == 0) yield break;
			foreach (string pat in patterns) {
				if (string.IsNullOrEmpty(pat)) {
					yield return new(new Int4(0, 0, 0, 0), "");
				} else if (pat[0] == '#') {
					yield return new(new Int4(0, 0, 0, 0), pat);
				} else {
					yield return new(new Int4(
						$"{pat}{suffix0}".AngeHash(),
						string.IsNullOrEmpty(suffix1) ? 0 : $"{pat}{suffix1}".AngeHash(),
						string.IsNullOrEmpty(suffix2) ? 0 : $"{pat}{suffix2}".AngeHash(),
						string.IsNullOrEmpty(suffix3) ? 0 : $"{pat}{suffix3}".AngeHash()
					), pat);
				}
			}
		}


		public static void GetSpriteInfoFromName (string name, out string realName, out string groupName, out int groupIndex, out GroupType groupType, out bool isTrigger, out string tag, out bool loopStart, out string rule, out bool noCollider, out int offsetZ, out int? pivotX, out int? pivotY) {
			isTrigger = false;
			tag = "";
			rule = "";
			loopStart = false;
			noCollider = false;
			offsetZ = 0;
			pivotX = null;
			pivotY = null;
			groupType = GroupType.General;
			const System.StringComparison OIC = System.StringComparison.OrdinalIgnoreCase;
			int hashIndex = name.IndexOf('#');
			if (hashIndex >= 0) {
				var hashs = name[hashIndex..].Replace(" ", "").Split('#');
				foreach (var hashTag in hashs) {

					if (string.IsNullOrWhiteSpace(hashTag)) continue;

					// Bool
					if (hashTag.Equals("isTrigger", OIC)) {
						isTrigger = true;
						continue;
					}

					// Tag
					bool tagFinded = false;
					foreach (string _tag in SpriteTag.ALL_TAGS_STRING) {
						if (hashTag.Equals(_tag, OIC)) {
							tag = _tag;
							tagFinded = true;
							break;
						}
					}
					if (tagFinded) continue;

					if (hashTag.Equals("loopStart", OIC)) {
						loopStart = true;
						continue;
					}

					if (hashTag.Equals("noCollider", OIC) || hashTag.Equals("ignoreCollider", OIC)) {
						noCollider = true;
						continue;
					}

					// Bool-Group
					if (hashTag.Equals("animated", OIC) || hashTag.Equals("ani", OIC)) {
						groupType = GroupType.Animated;
						continue;
					}
					if (hashTag.Equals("random", OIC) || hashTag.Equals("ran", OIC)) {
						groupType = GroupType.Random;
						continue;
					}

					// Int
					if (hashTag.StartsWith("tag=", OIC)) {
						tag = hashTag[4..];
						continue;
					}

					if (hashTag.StartsWith("rule=", OIC)) {
						rule = hashTag[5..];
						groupType = GroupType.Rule;
						continue;
					}

					if (hashTag.StartsWith("z=", OIC)) {
						if (int.TryParse(hashTag[2..], out int _offsetZ)) {
							offsetZ = _offsetZ;
						}
						continue;
					}

					if (hashTag.StartsWith("pivot", OIC)) {

						switch (hashTag) {
							case var _ when hashTag.StartsWith("pivotX=", OIC):
								if (int.TryParse(hashTag[7..], out int _px)) pivotX = _px;
								continue;
							case var _ when hashTag.StartsWith("pivotY=", OIC):
								if (int.TryParse(hashTag[7..], out int _py)) pivotY = _py;
								continue;
							case var _ when hashTag.StartsWith("pivot=bottomLeft", OIC):
								pivotX = 0;
								pivotY = 0;
								continue;
							case var _ when hashTag.StartsWith("pivot=bottomRight", OIC):
								pivotX = 1000;
								pivotY = 0;
								continue;
							case var _ when hashTag.StartsWith("pivot=bottom", OIC):
								pivotX = 500;
								pivotY = 0;
								continue;
							case var _ when hashTag.StartsWith("pivot=topLeft", OIC):
								pivotX = 0;
								pivotY = 1000;
								continue;
							case var _ when hashTag.StartsWith("pivot=topRight", OIC):
								pivotX = 1000;
								pivotY = 1000;
								continue;
							case var _ when hashTag.StartsWith("pivot=top", OIC):
								pivotX = 500;
								pivotY = 1000;
								continue;
							case var _ when hashTag.StartsWith("pivot=left", OIC):
								pivotX = 0;
								pivotY = 500;
								continue;
							case var _ when hashTag.StartsWith("pivot=right", OIC):
								pivotX = 1000;
								pivotY = 500;
								continue;
							case var _ when hashTag.StartsWith("pivot=center", OIC):
							case var _ when hashTag.StartsWith("pivot=mid", OIC):
							case var _ when hashTag.StartsWith("pivot=middle", OIC):
								pivotX = 500;
								pivotY = 500;
								continue;
						}
					}

					Game.LogWarning($"Unknown hash \"{hashTag}\" for {name}");

				}
				// Trim Name
				name = name[..hashIndex];
			}

			// Always Trigger Check
			isTrigger = isTrigger ||
				IsOnewayTag(tag.AngeHash()) ||
				tag == SpriteTag.WATER_STRING ||
				tag == SpriteTag.QUICKSAND_STRING;

			// Name and Group
			realName = name.TrimEnd(' ');
			groupName = realName.TrimEnd_NumbersEmpty();
			groupIndex = -1;
			if (!string.IsNullOrEmpty(realName) && realName[^1] >= '0' && realName[^1] <= '9') {
				string key = realName;
				int endIndex = key.Length - 1;
				while (endIndex >= 0) {
					char c = key[endIndex];
					if (c < '0' || c > '9') break;
					endIndex--;
				}
				groupIndex = endIndex < realName.Length - 1 ? int.Parse(realName[(endIndex + 1)..]) : 0;
			}

		}


		public static float GetScaledAudioVolume (int volume, int scale = 1000) {
			float fVolume = volume / 1000f;
			if (scale != 1000) fVolume *= scale / 1000f;
			return fVolume * fVolume;
		}


		public static byte[] Pixels_to_Bytes (this Byte4[] pixels, int width, int height) {
			var bytes = new byte[pixels.Length * 4];
			int index = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					int i = y * width + x;
					var p = pixels[i];
					bytes[index * 4 + 0] = p.r;
					bytes[index * 4 + 1] = p.g;
					bytes[index * 4 + 2] = p.b;
					bytes[index * 4 + 3] = p.a;
					index++;
				}
			}
			return bytes;
		}


		public static Byte4[] Bytes_to_Pixels (this byte[] bytes, int width, int height) {
			var pixels = new Byte4[width * height];
			int index = 0;
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					pixels[y * width + x] = new Byte4(
						bytes[index + 0],
						bytes[index + 1],
						bytes[index + 2],
						bytes[index + 3]
					);
					index += 4;
				}
			}
			return pixels;
		}


		// Requirement
		public static IEnumerable<KeyValuePair<string, string>> ForAllSpriteNameRequirements () {
			foreach (var pair in RequireSpriteFromField.ForAllRequirement()) {
				yield return pair;
			}
			foreach (var pair in RequireGlobalSpriteAttribute.ForAllRequirement()) {
				yield return pair;
			}
			foreach (var pair in RequireSpriteAttribute.ForAllRequirement()) {
				yield return pair;
			}
		}


		public static IEnumerable<string> ForAllLanguageKeyRequirements () {
			foreach (var name in RequireLanguageFromField.ForAllRequirement()) {
				yield return name;
			}
			foreach (var name in RequireGlobalLanguageAttribute.ForAllRequirement()) {
				yield return name;
			}
			foreach (var name in RequireLanguageAttribute.ForAllRequirement()) {
				yield return name;
			}
		}


		// Random
		public static int RandomInt (int min = int.MinValue, int max = int.MaxValue) => GlobalRandom.Next(min, max);
		public static float RandomFloat01 () => (float)GlobalRandom.NextDouble();
		public static double RandomDouble01 () => GlobalRandom.NextDouble();
		public static Byte4 RandomColor (int minH = 0, int maxH = 360, int minS = 0, int maxS = 100, int minV = 0, int maxV = 100, int minA = 0, int maxA = 255) {
			var result = Util.HsvToRgb(
				RandomInt(minH, maxH) / 360f,
				RandomInt(minS, maxS) / 100f,
				RandomInt(minV, maxV) / 100f
			);
			result.a = (byte)RandomInt(minA, maxA);
			return result;
		}


		// Pose Animation
		public static void LimbRotate (
			ref int targetX, ref int targetY, ref int targetPivotX, ref int targetPivotY, ref int targetRotation, ref int targetWidth, ref int targetHeight,
			int rotation, bool useFlip, int grow
		) {
			targetPivotY = 1000;
			targetWidth = targetWidth.Abs();
			bool bigRot = rotation.Abs() > 90;

			// Rotate
			targetRotation = rotation;
			int newPivotX = rotation > 0 != bigRot ? 1000 : 0;
			if (newPivotX != targetPivotX) {
				targetPivotX = newPivotX;
				targetX += newPivotX == 1000 ? targetWidth : -targetWidth;
			}

			// Flip
			if (targetPivotX > 500) {
				targetPivotX = 1000 - targetPivotX;
				if (bigRot || useFlip) {
					targetWidth = -targetWidth;
				} else {
					targetX -= (int)(Util.Cos(targetRotation * Util.Deg2Rad) * targetWidth);
					targetY += (int)(Util.Sin(targetRotation * Util.Deg2Rad) * targetWidth);
				}
			}

			// Fix for Big Rot
			if (bigRot) {
				targetWidth = -targetWidth;
				targetHeight -= targetWidth.Abs();
			}

			// Grow
			if (rotation != 0 && grow != 0) {
				targetHeight += rotation.Abs() * grow * targetWidth.Abs() / 96000;
			}

		}


		public static void LimbRotate (
			ref int targetX, ref int targetY, ref int targetPivotX, ref int targetPivotY, ref int targetRotation, ref int targetWidth, ref int targetHeight,
			int parentX, int parentY, int parentRotation, int parentWidth, int parentHeight,
			int rotation, bool useFlip, int grow
		) {
			targetPivotY = 1000;
			targetWidth = targetWidth.Abs();
			bool bigRot = rotation.Abs() > 90;

			// Rotate
			targetRotation = parentRotation + rotation;
			targetX = parentX - (int)(Util.Sin(parentRotation * Util.Deg2Rad) * parentHeight);
			targetY = parentY - (int)(Util.Cos(parentRotation * Util.Deg2Rad) * parentHeight);
			targetPivotX = rotation > 0 != bigRot ? 1000 : 0;
			if (parentWidth < 0 != targetPivotX > 500) {
				int pWidth = parentWidth.Abs();
				int sign = targetPivotX > 500 ? -1 : 1;
				targetX -= (int)(Util.Cos(parentRotation * Util.Deg2Rad) * pWidth) * sign;
				targetY += (int)(Util.Sin(parentRotation * Util.Deg2Rad) * pWidth) * sign;
			}

			// Flip
			if (targetPivotX > 500) {
				targetPivotX = 1000 - targetPivotX;
				if (bigRot || useFlip) {
					targetWidth = -targetWidth;
				} else {
					targetX -= (int)(Util.Cos(targetRotation * Util.Deg2Rad) * targetWidth);
					targetY += (int)(Util.Sin(targetRotation * Util.Deg2Rad) * targetWidth);
				}
			}

			// Fix for Big Rot
			if (bigRot) {
				targetWidth = -targetWidth;
				targetHeight -= parentWidth.Abs();
			}

			// Grow
			if (rotation != 0 && grow != 0) {
				targetHeight += rotation.Abs() * grow * targetWidth.Abs() / 96000;
			}

		}


		public static bool DrawPoseCharacterAsUI (IRect rect, PoseCharacter character, int animationFrame, int z, out IRect globalRect, out IRect uiRect) {

			globalRect = default;
			uiRect = default;

			// Draw Player
			int oldLayerIndex = CellRenderer.CurrentLayerIndex;
			CellRenderer.SetLayerToUI();
			int layerIndex = CellRenderer.CurrentLayerIndex;
			int cellIndexStart = CellRenderer.GetUsedCellCount();
			int oldAniFrame = character.CurrentAnimationFrame;
			character.CurrentAnimationFrame = animationFrame;
			character.FrameUpdate();
			character.CurrentAnimationFrame = oldAniFrame;
			int cellIndexEnd = CellRenderer.GetUsedCellCount();
			CellRenderer.SetLayer(oldLayerIndex);
			if (cellIndexStart == cellIndexEnd) return false;
			if (!CellRenderer.GetCells(layerIndex, out var cells, out int count)) return false;

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
					cell.Z = z + cell.Z - minZ;
				}
			}

			return true;

		}


		// Drawing
		public static Cell DrawEnvironmentShadow (Cell source, int offsetX = -Const.HALF / 2, int offsetY = 0, byte alpha = 64, int z = -64 * 1024 + 16) {
			var result = CellRenderer.Draw(Const.PIXEL, default);
			result.CopyFrom(source);
			result.X += offsetX;
			result.Y += offsetY;
			result.Z = z;
			result.Color = new Byte4(0, 0, 0, alpha);
			return result;
		}


		public static void DrawGlitchEffect (Cell cell, int frame) {

			if (frame.UMod(0096) == 0) DrawGlitch(cell, RandomInt(-012, 012), RandomInt(-007, 007), RandomInt(0900, 1100), RandomInt(0500, 1100), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0061) == 0) DrawGlitch(cell, RandomInt(-002, 041), RandomInt(-019, 072), RandomInt(0500, 1200), RandomInt(0500, 1200), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0121) == 0) DrawGlitch(cell, RandomInt(-016, 016), RandomInt(-007, 007), RandomInt(0400, 1100), RandomInt(0600, 1100), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0127) == 0) DrawGlitch(cell, RandomInt(-016, 016), RandomInt(-007, 007), RandomInt(0900, 1100), RandomInt(0500, 1300), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0185) == 0) DrawGlitch(cell, RandomInt(-011, 021), RandomInt(-018, 018), RandomInt(0900, 1300), RandomInt(0900, 1300), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0187) == 0) DrawGlitch(cell, RandomInt(-034, 042), RandomInt(-012, 008), RandomInt(0800, 1100), RandomInt(0900, 1400), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0193) == 0) DrawGlitch(cell, RandomInt(-052, 002), RandomInt(-091, 077), RandomInt(0700, 1400), RandomInt(0800, 1100), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0274) == 0) DrawGlitch(cell, RandomInt(-033, 072), RandomInt(-031, 079), RandomInt(0800, 1100), RandomInt(0900, 1800), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(1846) == 0) DrawGlitch(cell, RandomInt(-094, 012), RandomInt(-077, 112), RandomInt(0900, 1500), RandomInt(0900, 1300), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(3379) == 0) DrawGlitch(cell, RandomInt(-194, 112), RandomInt(-177, 212), RandomInt(0900, 1600), RandomInt(0700, 1900), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(9379) == 0) DrawGlitch(cell, RandomInt(-293, 211), RandomInt(-079, 011), RandomInt(0900, 1700), RandomInt(0900, 1100), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));

			// Func
			static void DrawGlitch (Cell cell, int offsetX, int offsetY, int scaleX, int scaleY, Byte4 color) {

				var cursedCell = CellRenderer.Draw(Const.PIXEL, default, 0);
				cursedCell.Sprite = cell.Sprite;
				cursedCell.TextSprite = cell.TextSprite;
				cursedCell.X = cell.X;
				cursedCell.Y = cell.Y;
				cursedCell.Z = cell.Z + 1;
				cursedCell.Rotation = cell.Rotation;
				cursedCell.Width = cell.Width * scaleX / 1000;
				cursedCell.Height = cell.Height * scaleY / 1000;
				cursedCell.PivotX = cell.PivotX;
				cursedCell.PivotY = cell.PivotY;
				cursedCell.Shift = cell.Shift;

				cursedCell.Color = color;
				cursedCell.X += offsetX;
				cursedCell.Y += offsetY;

			}
		}


		public static void GetSlicedUvBorder (AngeSprite sprite, Alignment alignment, out Float2 bl, out Float2 br, out Float2 tl, out Float2 tr) {

			bl = new(0f, 0f);
			br = new(1f, 0f);
			tl = new(0f, 1f);
			tr = new(1f, 1f);

			// Y
			switch (alignment) {
				case Alignment.TopLeft:
				case Alignment.TopMid:
				case Alignment.TopRight:
					bl.y = br.y = (sprite.GlobalHeight - sprite.GlobalBorder.up) / (float)sprite.GlobalHeight;
					break;
				case Alignment.MidLeft:
				case Alignment.MidMid:
				case Alignment.MidRight:
					tl.y = tr.y = (sprite.GlobalHeight - sprite.GlobalBorder.up) / (float)sprite.GlobalHeight;
					bl.y = br.y = sprite.GlobalBorder.down / (float)sprite.GlobalHeight;
					break;
				case Alignment.BottomLeft:
				case Alignment.BottomMid:
				case Alignment.BottomRight:
					tl.y = tr.y = sprite.GlobalBorder.down / (float)sprite.GlobalHeight;
					break;
			}
			// X
			switch (alignment) {
				case Alignment.TopLeft:
				case Alignment.MidLeft:
				case Alignment.BottomLeft:
					br.x = tr.x = sprite.GlobalBorder.left / (float)sprite.GlobalWidth;
					break;
				case Alignment.TopMid:
				case Alignment.MidMid:
				case Alignment.BottomMid:
					br.x = tr.x = (sprite.GlobalWidth - sprite.GlobalBorder.right) / (float)sprite.GlobalWidth;
					bl.x = tl.x = sprite.GlobalBorder.left / (float)sprite.GlobalWidth;
					break;
				case Alignment.TopRight:
				case Alignment.MidRight:
				case Alignment.BottomRight:
					bl.x = tl.x = (sprite.GlobalWidth - sprite.GlobalBorder.right) / (float)sprite.GlobalWidth;
					break;
			}
		}


		// Extension
		public static int ToUnit (this int globalPos) => globalPos.UDivide(Const.CEL);
		public static int ToGlobal (this int unitPos) => unitPos * Const.CEL;
		public static int ToUnifyGlobal (this int globalPos) => globalPos.UDivide(Const.CEL) * Const.CEL;

		public static Int2 ToUnit (this Int2 globalPos) => globalPos.UDivide(Const.CEL);
		public static Int2 ToGlobal (this Int2 unitPos) => unitPos * Const.CEL;
		public static Int2 ToUnifyGlobal (this Int2 globalPos) => globalPos.ToUnit().ToGlobal();

		public static Int3 ToUnit (this Int3 globalPos) => new(
			globalPos.x.UDivide(Const.CEL),
			globalPos.y.UDivide(Const.CEL),
			globalPos.z
		);
		public static Int3 ToGlobal (this Int3 unitPos) => new(
			unitPos.x * Const.CEL,
			unitPos.y * Const.CEL,
			unitPos.z
		);
		public static Int3 ToUnifyGlobal (this Int3 globalPos) => new(
			globalPos.x.ToUnit().ToGlobal(),
			globalPos.y.ToUnit().ToGlobal(),
			globalPos.z
		);


		public static IRect ToUnit (this IRect global) => global.UDivide(Const.CEL);
		public static IRect ToGlobal (this IRect unit) => new(
			unit.x * Const.CEL,
			unit.y * Const.CEL,
			unit.width * Const.CEL,
			unit.height * Const.CEL
		);
		public static IRect ToUnifyGlobal (this IRect global) => global.ToUnit().ToGlobal();


		// Map Editor
		public static string GetTileRuleString (int groupID) {
			if (!CellRenderer.HasSpriteGroup(groupID, out int groupLength)) return "";
			var builder = new StringBuilder();
			for (int i = 0; i < groupLength; i++) {
				if (CellRenderer.TryGetSpriteFromGroup(groupID, i, out var sp, false, true)) {
					int ruleDigit = sp.Rule;
					builder.Append(RuleDigitToString(ruleDigit));
				} else {
					builder.Append(RULE_TILE_ERROR);
				}
			}
			return builder.ToString();
		}


		public static string RuleDigitToString (int digit) {
			// ↖↑↗←→↙↓↘,... 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty NaN=Error
			//              000        001        010           011       100
			// eg: "02022020,11111111,01022020,02012020,..."
			// digit: int32 10000000 000 000 000 000 000 000 000 000
			//              01234567 890 123 456 789 012 345 678 901
			//              000000000001 111 111 111 222 222 222 233
			if (!digit.GetBit(0)) return RULE_TILE_ERROR;
			var builder = new StringBuilder();
			for (int i = 0; i < 8; i++) {
				int tileStrNumber = 0;
				tileStrNumber += digit.GetBit(8 + i * 3 + 0) ? 4 : 0;
				tileStrNumber += digit.GetBit(8 + i * 3 + 1) ? 2 : 0;
				tileStrNumber += digit.GetBit(8 + i * 3 + 2) ? 1 : 0;
				builder.Append(tileStrNumber);
			}
			return builder.ToString();
		}


		public static int RuleStringToDigit (string str) {
			// ↖↑↗←→↙↓↘,... 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty NaN=Error
			//              000        001        010           011       100
			// eg: "02022020,11111111,01022020,02012020,..."
			// digit: int32 10000000 000 000 000 000 000 000 000 000
			//              01234567 890 123 456 789 012 345 678 901
			//              000000000001 111 111 111 222 222 222 233
			if (string.IsNullOrEmpty(str) || str.Length < 8) return 0;
			int digit = 0;
			digit.SetBit(0, true);
			for (int i = 0; i < 8; i++) {
				char c = str[i];
				if (c < '0' || c > '9') continue;
				int strNumber = str[i] - '0';
				digit.SetBit(8 + i * 3 + 0, (strNumber / 4) % 2 == 1);
				digit.SetBit(8 + i * 3 + 1, (strNumber / 2) % 2 == 1);
				digit.SetBit(8 + i * 3 + 2, (strNumber / 1) % 2 == 1);
			}
			return digit;
		}


		public static int GetRuleIndex (
			string rule, int ruleID,
			int tl0, int tm0, int tr0, int ml0, int mr0, int bl0, int bm0, int br0,
			int tl1, int tm1, int tr1, int ml1, int mr1, int bl1, int bm1, int br1
		) {
			// 0=Whatever 1=SameTile 2=NotSameTile 3=NotEmpty 4=Empty
			int count = rule.Length / 8;
			for (int i = 0; i < count; i++) {
				char first = rule[i * 8 + 0];
				if (first < '0' || first > '9') continue;
				int _tl = rule[i * 8 + 0] - '0';
				int _tm = rule[i * 8 + 1] - '0';
				int _tr = rule[i * 8 + 2] - '0';
				int _ml = rule[i * 8 + 3] - '0';
				int _mr = rule[i * 8 + 4] - '0';
				int _bl = rule[i * 8 + 5] - '0';
				int _bm = rule[i * 8 + 6] - '0';
				int _br = rule[i * 8 + 7] - '0';
				if (!CheckForTile(tl0, tl1, _tl)) continue;
				if (!CheckForTile(tm0, tm1, _tm)) continue;
				if (!CheckForTile(tr0, tr1, _tr)) continue;
				if (!CheckForTile(ml0, ml1, _ml)) continue;
				if (!CheckForTile(mr0, mr1, _mr)) continue;
				if (!CheckForTile(bl0, bl1, _bl)) continue;
				if (!CheckForTile(bm0, bm1, _bm)) continue;
				if (!CheckForTile(br0, br1, _br)) continue;
				return TryRandom(i);
			}
			return -1;
			// Func
			bool CheckForTile (int _targetID0, int _targetID1, int _targetRule) => _targetRule switch {
				1 => _targetID0 == ruleID || _targetID1 == ruleID,
				2 => _targetID0 != ruleID && _targetID1 != ruleID,
				3 => _targetID0 != 0,
				4 => _targetID0 == 0,
				_ => true,
			};
			int TryRandom (int resultIndex) {
				int lastIndex = resultIndex;
				bool jumpOut = false;
				for (int i = resultIndex + 1; i < count; i++) {
					for (int j = 0; j < 8; j++) {
						if (rule[i * 8 + j] != rule[resultIndex * 8 + j]) {
							jumpOut = true;
							break;
						}
					}
					if (jumpOut) break;
					lastIndex = i;
				}
				if (resultIndex != lastIndex) resultIndex = AngeUtil.RandomInt(resultIndex, lastIndex + 1);
				return resultIndex;
			}
		}


		public static void DeleteAllEmptyMaps (string mapRoot) {
			var world = new World();
			foreach (var path in Util.EnumerateFiles(mapRoot, false, $"*.{AngePath.MAP_FILE_EXT}")) {
				try {
					if (!world.LoadFromDisk(path)) continue;
					if (world.EmptyCheck()) {
						Util.DeleteFile(path);
					}
				} catch (System.Exception ex) { Game.LogException(ex); }
			}
		}


		// AngeliA Hash Code
		public static string AngeName (this System.Type type) {
			string name = type.Name;
			if (char.IsLower(name[0])) name = name[1..];
			return name;
		}


		public static int[] GetAngeHashs (System.Type[] types) {
			if (types == null || types.Length == 0) return new int[0];
			var results = new int[types.Length];
			for (int i = 0; i < results.Length; i++) {
				var _type = types[i];
				results[i] = _type != null ? _type.AngeHash() : 0;
			}
			return results;
		}


		public static int AngeHash (this System.Type type) => type.AngeName().AngeHash();


		public static int AngeHash (this string str, int avoid = 0) {
			const int p = 31;
			const int m = 1837465129;
			int hash_value = 0;
			int p_pow = 1;
			for (int i = 0; i < str.Length; i++) {
				char c = str[i];
				hash_value = (hash_value + (c - 'a' + 1) * p_pow) % m;
				p_pow = (p_pow * p) % m;
			}
			return hash_value == avoid ? avoid + 1 : hash_value;
		}


	}
}