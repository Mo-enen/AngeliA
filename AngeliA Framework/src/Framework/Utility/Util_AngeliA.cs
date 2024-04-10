using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace AngeliA;
public static partial class Util {


	// Const
	private static readonly byte[] SINGLE_RULE_CACHE = new byte[8];
	private static readonly Cell[] SLICE_RESULT = new Cell[9];
	public const string RULE_TILE_ERROR = "ERROR---";
	private static readonly System.Random GlobalRandom = new(2334768);
	public delegate Cell DrawHandler (AngeSprite sprite, int x, int y, int pivotX, int pivotY, int rotation, int width, int height, Color32 color, int z);
	private static readonly StringBuilder CacheRuleBuilder = new();


	// API
	public static void ClampCells (Cell[] cells, IRect rect, int startIndex, int endIndex) {
		for (int i = startIndex; i < endIndex; i++) {
			ClampCell(cells[i], rect);
		}
	}

	public static void ClampCell (Cell cell, IRect rect) {
		var cellRect = new IRect();
		cellRect.x = cell.X - (int)(cell.Width * cell.PivotX);
		cellRect.y = cell.Y - (int)(cell.Height * cell.PivotY);
		cellRect.width = cell.Width;
		cellRect.height = cell.Height;
		cellRect.FlipNegative();
		if (!cellRect.Overlaps(rect)) {
			cell.Width = 0;
			return;
		}
		// Clamp
		int cellL = cellRect.x;
		int cellR = cellRect.x + cellRect.width;
		int cellD = cellRect.y;
		int cellU = cellRect.y + cellRect.height;
		if (cellL < rect.x) {
			if (cell.Width > 0) {
				cell.Shift.left = rect.x - cellL;
			} else {
				cell.Shift.right = rect.x - cellL;
			}
		}
		if (cellR > rect.x + rect.width) {
			if (cell.Width > 0) {
				cell.Shift.right = cellR - rect.x - rect.width;
			} else {
				cell.Shift.left = cellR - rect.x - rect.width;
			}
		}
		if (cellD < rect.y) {
			if (cell.Height > 0) {
				cell.Shift.down = rect.y - cellD;
			} else {
				cell.Shift.up = rect.y - cellD;
			}
		}
		if (cellU > rect.y + rect.height) {
			if (cell.Height > 0) {
				cell.Shift.up = cellU - rect.y - rect.height;
			} else {
				cell.Shift.down = cellU - rect.y - rect.height;
			}
		}
	}

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
		foreach (var path in EnumerateFiles(workspace, false, $"*.{AngePath.EDITABLE_CONVERSATION_FILE_EXT}")) {

			string globalName = GetNameWithoutExtension(path);
			string conFolderPath = CombinePaths(exportRoot, globalName);
			ignoreDelete.TryAdd(globalName);

			// Check Dirty
			long modTime = GetFileModifyDate(path);
			long creationTime = GetFileCreationDate(path);
			if (!forceCompile && modTime == creationTime && FolderExists(conFolderPath)) continue;
			SetFileModifyDate(path, creationTime);

			// Delete Existing for All Languages
			DeleteFolder(conFolderPath);
			CreateFolder(conFolderPath);

			// Compile
			var builder = new StringBuilder();
			string currentIso = "en";
			bool contentFlag0 = false;
			bool contentFlag1 = false;
			foreach (string line in ForAllLines(path, Encoding.UTF8)) {

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
					if (trimedLine.Length > 1 && IsSupportedLanguageISO(iso) && currentIso != iso) {
						// Make File
						string targetPath = CombinePaths(
							conFolderPath,
							$"{currentIso}.{AngePath.CONVERSATION_FILE_EXT}"
						);
						TextToFile(builder.ToString(), targetPath, Encoding.UTF8);
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
				string targetPath = CombinePaths(
					conFolderPath,
					$"{currentIso}.{AngePath.CONVERSATION_FILE_EXT}"
				);
				TextToFile(builder.ToString(), targetPath, Encoding.UTF8);
				builder.Clear();
			}

		}

		// Delete Useless Old Files
		if (ignoreDelete != null) {
			List<string> deleteList = null;
			foreach (var path in EnumerateFolders(exportRoot, true, "*")) {
				if (ignoreDelete.Contains(GetNameWithoutExtension(path))) continue;
				deleteList ??= new List<string>();
				deleteList.Add(path);
			}
			if (deleteList != null) {
				foreach (var path in deleteList) {
					DeleteFolder(path);
					DeleteFile(path + ".meta");
				}
			}
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

	public static void GetSpriteInfoFromName (string name, out string realName, out bool isTrigger, out string tag, out string rule, out bool noCollider, out int offsetZ, out int aniDuration, out int? pivotX, out int? pivotY) {
		isTrigger = false;
		tag = "";
		rule = "";
		noCollider = false;
		offsetZ = 0;
		pivotX = null;
		pivotY = null;
		aniDuration = 0;
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

				// Bool-Group
				if (hashTag.Equals("loopStart", OIC)) {
					tag = SpriteTag.LOOP_START_STRING;
					continue;
				}

				if (hashTag.Equals("noCollider", OIC) || hashTag.Equals("ignoreCollider", OIC)) {
					noCollider = true;
					continue;
				}

				if (hashTag.Equals("random", OIC) || hashTag.Equals("ran", OIC)) {
					tag = SpriteTag.RANDOM_STRING;
					continue;
				}

				// Int
				if (hashTag.StartsWith("ani=", OIC)) {
					if (int.TryParse(hashTag[4..], out int _aniD)) {
						aniDuration = _aniD;
					}
					continue;
				}
				if (hashTag.StartsWith("animated=", OIC)) {
					if (int.TryParse(hashTag[9..], out int _aniD)) {
						aniDuration = _aniD;
					}
					continue;
				}

				if (hashTag.StartsWith("tag=", OIC)) {
					tag = hashTag[4..];
					continue;
				}

				if (hashTag.StartsWith("rule=", OIC)) {
					rule = hashTag[5..];
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

			}
			// Trim Name
			name = name[..hashIndex];
		}

		// Always Trigger Check
		isTrigger = isTrigger ||
			IsOnewayTag(tag.AngeHash()) ||
			tag == SpriteTag.WATER_STRING ||
			tag == SpriteTag.QUICKSAND_STRING;

		// Name
		realName = name.TrimEnd(' ');

	}

	public static bool GetGroupInfoFromSpriteRealName (string realName, out string groupName, out int groupIndex) {
		groupName = realName;
		groupIndex = -1;
		if (!string.IsNullOrEmpty(realName) && char.IsNumber(realName[^1])) {
			string key = realName;
			int endIndex = key.Length - 1;
			while (endIndex >= 0) {
				char c = key[endIndex];
				if (!char.IsNumber(c)) break;
				endIndex--;
			}
			groupIndex = endIndex < realName.Length - 1 ? int.Parse(realName[(endIndex + 1)..]) : 0;
			groupName = realName.TrimEnd_NumbersEmpty();
			return true;
		}
		return false;
	}

	public static float GetScaledAudioVolume (int volume, int scale = 1000) {
		float fVolume = volume / 1000f;
		if (scale != 1000) fVolume *= scale / 1000f;
		return fVolume * fVolume;
	}


	// Random
	public static int RandomInt (int min = int.MinValue, int max = int.MaxValue) => GlobalRandom.Next(min, max);
	public static float RandomFloat01 () => (float)GlobalRandom.NextDouble();
	public static double RandomDouble01 () => GlobalRandom.NextDouble();
	public static Color32 RandomColor (int minH = 0, int maxH = 360, int minS = 0, int maxS = 100, int minV = 0, int maxV = 100, int minA = 0, int maxA = 255) {
		var result = HsvToRgb(
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
				targetX -= (int)(Cos(targetRotation * Deg2Rad) * targetWidth);
				targetY += (int)(Sin(targetRotation * Deg2Rad) * targetWidth);
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
		targetX = parentX - (int)(Sin(parentRotation * Deg2Rad) * parentHeight);
		targetY = parentY - (int)(Cos(parentRotation * Deg2Rad) * parentHeight);
		targetPivotX = rotation > 0 != bigRot ? 1000 : 0;
		if (parentWidth < 0 != targetPivotX > 500) {
			int pWidth = parentWidth.Abs();
			int sign = targetPivotX > 500 ? -1 : 1;
			targetX -= (int)(Cos(parentRotation * Deg2Rad) * pWidth) * sign;
			targetY += (int)(Sin(parentRotation * Deg2Rad) * pWidth) * sign;
		}

		// Flip
		if (targetPivotX > 500) {
			targetPivotX = 1000 - targetPivotX;
			if (bigRot || useFlip) {
				targetWidth = -targetWidth;
			} else {
				targetX -= (int)(Cos(targetRotation * Deg2Rad) * targetWidth);
				targetY += (int)(Sin(targetRotation * Deg2Rad) * targetWidth);
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


	// Drawing
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

	public static Cell[] NineSlice (
		DrawHandler draw,
		AngeSprite sprite, int x, int y, int pivotX, int pivotY,
		int rotation, int width, int height,
		int borderL, int borderR, int borderD, int borderU,
		bool[] partIgnore, Color32 color, int z
	) {

		SLICE_RESULT[0] = SLICE_RESULT[1] = SLICE_RESULT[2] = Cell.EMPTY;
		SLICE_RESULT[3] = SLICE_RESULT[4] = SLICE_RESULT[5] = Cell.EMPTY;
		SLICE_RESULT[6] = SLICE_RESULT[7] = SLICE_RESULT[8] = Cell.EMPTY;

		// Original Size
		if (sprite != null && width != 0 && height != 0) {
			if (width == Const.ORIGINAL_SIZE) {
				width = sprite.GlobalWidth;
			} else if (width == Const.ORIGINAL_SIZE_NEGATAVE) {
				width = -sprite.GlobalWidth;
			}
			if (height == Const.ORIGINAL_SIZE) {
				height = sprite.GlobalHeight;
			} else if (height == Const.ORIGINAL_SIZE_NEGATAVE) {
				height = -sprite.GlobalHeight;
			}
		} else return SLICE_RESULT;

		bool flipX = width < 0;
		bool flipY = height < 0;
		width = width.Abs();
		height = height.Abs();

		borderL = borderL.Clamp(0, width);
		borderR = borderR.Clamp(0, width);
		borderD = borderD.Clamp(0, height);
		borderU = borderU.Clamp(0, height);

		bool hasL = borderL > 0;
		bool hasM = borderL + borderR < width;
		bool hasR = borderR > 0;
		int mWidth = width - borderL - borderR;
		int mHeight = height - borderD - borderU;

		float _px0 = width * pivotX / 1000f / borderL;
		float _px1 = (width * pivotX / 1000f - borderL) / mWidth;
		float _px2 = (borderR - width * (1000 - pivotX) / 1000f) / borderR;

		if (borderU > 0) {
			float _py = (borderU - height * (1000 - pivotY) / 1000f) / borderU;
			// TL
			if (hasL && !partIgnore[0]) {
				var cell = draw(
					sprite, x, y, 0, 0,
					rotation, borderL, borderU, color, z
				);
				cell.BorderSide = Alignment.TopLeft;
				cell.PivotX = _px0;
				cell.PivotY = _py;
				SLICE_RESULT[0] = cell;
			}
			// TM
			if (hasM && !partIgnore[1]) {
				var cell = draw(
					sprite, x, y, 0, 0,
					rotation, mWidth, borderU, color, z
				);
				cell.BorderSide = Alignment.TopMid;
				cell.PivotX = _px1;
				cell.PivotY = _py;
				SLICE_RESULT[1] = cell;
			}
			// TR
			if (hasR && !partIgnore[2]) {
				var cell = draw(
					sprite, x, y, 0, 0,
					rotation, borderR, borderU, color, z
				);
				cell.PivotX = _px2;
				cell.PivotY = _py;
				cell.BorderSide = Alignment.TopRight;
				SLICE_RESULT[2] = cell;
			}
		}
		if (borderD + borderU < height) {
			float _py = (height * pivotY / 1000f - borderD) / mHeight;
			// ML
			if (hasL && !partIgnore[3]) {
				var cell = draw(
					sprite, x, y, 0, 0,
					rotation, borderL, mHeight, color, z
				);
				cell.BorderSide = Alignment.MidLeft;
				cell.PivotX = _px0;
				cell.PivotY = _py;
				SLICE_RESULT[3] = cell;
			}
			// MM
			if (hasM && !partIgnore[4]) {
				var cell = draw(
					sprite, x, y, 0, 0,
					rotation, mWidth, mHeight, color, z
				);
				cell.BorderSide = Alignment.MidMid;
				cell.PivotX = _px1;
				cell.PivotY = _py;
				SLICE_RESULT[4] = cell;
			}
			// MR
			if (hasR && !partIgnore[5]) {
				var cell = draw(
					sprite, x, y, 0, 0,
					rotation, borderR, mHeight, color, z
				);
				cell.BorderSide = Alignment.MidRight;
				cell.PivotX = _px2;
				cell.PivotY = _py;
				SLICE_RESULT[5] = cell;
			}
		}
		if (borderD > 0) {
			float _py = height * pivotY / 1000f / borderD;
			// DL
			if (hasL && !partIgnore[6]) {
				var cell = draw(
					sprite, x, y, 0, 0,
					rotation, borderL, borderD, color, z
				);
				cell.BorderSide = Alignment.BottomLeft;
				cell.PivotX = _px0;
				cell.PivotY = _py;
				SLICE_RESULT[6] = cell;
			}
			// DM
			if (hasM && !partIgnore[7]) {
				var cell = draw(
					sprite, x, y, 0, 0,
					rotation, mWidth, borderD, color, z
				);
				cell.BorderSide = Alignment.BottomMid;
				cell.PivotX = _px1;
				cell.PivotY = _py;
				SLICE_RESULT[7] = cell;
			}
			// DR
			if (hasR && !partIgnore[8]) {
				var cell = draw(
					sprite, x, y, 0, 0,
					rotation, borderR, borderD, color, z
				);
				cell.BorderSide = Alignment.BottomRight;
				cell.PivotX = _px2;
				cell.PivotY = _py;
				SLICE_RESULT[8] = cell;
			}
		}

		// Flip for Negative Size
		if (flipX) {
			foreach (var cell in SLICE_RESULT) cell.Width = -cell.Width;
		}
		if (flipY) {
			foreach (var cell in SLICE_RESULT) cell.Height = -cell.Height;
		}

		return SLICE_RESULT;

	}


	// Rule
	public static string DigitToRuleString (int digit) {
		// ↖↑↗←→↙↓↘,... 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty NaN=Error
		//              000        001        010           011       100
		// eg: "02022020,11111111,01022020,02012020,..."
		// digit: int32 10000000 000 000 000 000 000 000 000 000
		//              01234567 890 123 456 789 012 345 678 901
		//              000000000001 111 111 111 222 222 222 233
		if (!digit.GetBit(0)) return RULE_TILE_ERROR;

		CacheRuleBuilder.Clear();
		for (int i = 0; i < 8; i++) {
			int tileStrNumber = 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 0) ? 4 : 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 1) ? 2 : 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 2) ? 1 : 0;
			CacheRuleBuilder.Append(tileStrNumber);
		}
		return CacheRuleBuilder.ToString();
	}

	public static void DigitToRuleByte (int digit, byte[] bytes) {
		if (!digit.GetBit(0)) {
			System.Array.Clear(bytes);
			return;
		}
		for (int i = 0; i < 8; i++) {
			int tileStrNumber = 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 0) ? 4 : 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 1) ? 2 : 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 2) ? 1 : 0;
			bytes[i] = (byte)tileStrNumber;
		}
	}

	public static int RuleStringToDigit (string ruleStr) {
		// ↖↑↗←→↙↓↘,... 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty NaN=Error
		//              000        001        010           011       100
		// eg: "02022020,11111111,01022020,02012020,..."
		// digit: int32 10000000 000 000 000 000 000 000 000 000
		//              01234567 890 123 456 789 012 345 678 901
		//              000000000001 111 111 111 222 222 222 233
		if (string.IsNullOrEmpty(ruleStr) || ruleStr.Length < 8) return 0;
		for (int i = 0; i < 8; i++) {
			char c = ruleStr[i];
			SINGLE_RULE_CACHE[i] = (byte)(c >= '0' && c <= '9' ? c - '0' : 255);
		}
		return RuleByteToDigit(SINGLE_RULE_CACHE);
	}

	public static int RuleByteToDigit (byte[] singleRule) {
		// ↖↑↗←→↙↓↘ 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty 255=Error
		//              000        001        010           011       100
		// eg: "02022020,11111111,01022020,02012020,..."
		// digit: int32 10000000 000 000 000 000 000 000 000 000
		//              01234567 890 123 456 789 012 345 678 901
		//              000000000001 111 111 111 222 222 222 233
		int digit = 0;
		digit.SetBit(0, true);
		for (int i = 0; i < 8; i++) {
			byte b = singleRule[i];
			digit.SetBit(8 + i * 3 + 0, (b / 4) % 2 == 1);
			digit.SetBit(8 + i * 3 + 1, (b / 2) % 2 == 1);
			digit.SetBit(8 + i * 3 + 2, (b / 1) % 2 == 1);
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
			if (resultIndex != lastIndex) resultIndex = RandomInt(resultIndex, lastIndex + 1);
			return resultIndex;
		}
	}


}