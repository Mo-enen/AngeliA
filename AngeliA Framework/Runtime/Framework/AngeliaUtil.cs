using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;


namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaFramework {


	public enum FittingPose {
		Unknown = 0,
		Left = 1,
		Down = 1,
		Mid = 2,
		Right = 3,
		Up = 3,
		Single = 4,
	}


	public static class AngeUtil {


		// Const
		private const string RULE_TILE_ERROR = "ERROR---";
		private static readonly int[] ITEM_TYPE_ICONS = {
			"ItemIcon.Weapon".AngeHash(),
			"ItemIcon.Armor".AngeHash(),
			"ItemIcon.Helmet".AngeHash(),
			"ItemIcon.Shoes".AngeHash(),
			"ItemIcon.Gloves".AngeHash(),
			"ItemIcon.Jewelry".AngeHash(),
			"ItemIcon.Food".AngeHash(),
			"ItemIcon.Item".AngeHash(),
		};
		private static readonly System.Random GlobalRandom = new(2334768);


		// Universe
		public static void CreateAngeFolders () {
			Util.CreateFolder(Const.UniverseRoot);
			Util.CreateFolder(Const.LanguageRoot);
			Util.CreateFolder(Const.BuiltInMapRoot);
			Util.CreateFolder(Const.UserMapRoot);
			Util.CreateFolder(Const.DownloadMapRoot);
			Util.CreateFolder(Const.PlayerDataRoot);
			Util.CreateFolder(Const.SheetRoot);
		}


		public static int LoadUniverseVersionFromManifest (string manifestPath) {
			if (!Util.FileExists(manifestPath)) return -1;
			foreach (string line in Util.ForAllLines(manifestPath)) {
				if (!string.IsNullOrWhiteSpace(line)) {
					if (int.TryParse(line, out int diskVersion)) {
						return diskVersion;
					}
				}
				return -1;
			}
			return -1;
		}


		public static IEnumerator SyncUniverseFolder (int gameUniverseVersion, System.Action callback) {

			UnityEngine.Networking.UnityWebRequest request = null;
			bool isAndroid = Application.platform == RuntimePlatform.Android;
			string streamingRoot = Util.CombinePaths(Application.streamingAssetsPath, Const.UNIVERSE_NAME);
			string persistentRoot = Util.CombinePaths(Application.persistentDataPath, Const.UNIVERSE_NAME);

			// Check Version
			string uniManifestPath = Util.CombinePaths(persistentRoot, Const.MANIFEST_NAME);
			int diskVersion = LoadUniverseVersionFromManifest(uniManifestPath);
			if (gameUniverseVersion == diskVersion) {
				// No Need to Sync
				callback?.Invoke();
				yield break;
			}

			// Load Manifest
			try {
				string manifestUrl = Util.CombinePaths(streamingRoot, Const.MANIFEST_NAME);
				if (!isAndroid) manifestUrl = Util.GetUrl(manifestUrl);
				request = UnityEngine.Networking.UnityWebRequest.Get(manifestUrl);
			} catch (System.Exception ex) {
				Debug.LogException(ex);
				callback?.Invoke();
				yield break;
			}

			yield return request.SendWebRequest();

			// Get Paths
			string[] pathList = null;
			try {
				var handler = request.downloadHandler;
				if (handler == null) {
					callback?.Invoke();
					yield break;
				}
				pathList = handler.text.Split('\n');
			} catch (System.Exception ex) {
				Debug.LogException(ex);
				callback?.Invoke();
				yield break;
			}

			// Delete Old
			Util.DeleteFolder(persistentRoot);

			// Copy All Files
			for (int i = 1; i < pathList.Length; i++) {

				string path = pathList[i];
				if (string.IsNullOrWhiteSpace(path)) continue;

				try {
					string url = Util.CombinePaths(streamingRoot, path);
					if (!isAndroid) url = Util.GetUrl(url);
					request = UnityEngine.Networking.UnityWebRequest.Get(url);
				} catch {
					continue;
				}

				yield return request.SendWebRequest();

				try {
					var handler = request.downloadHandler;
					if (handler == null) continue;
					string newPath = Util.CombinePaths(persistentRoot, path);
					Util.ByteToFile(handler.nativeData.ToArray(), newPath);
				} catch {
					continue;
				}
			}

			// End
			callback?.Invoke();
		}


		// API
		public static int GetOnewayTag (Direction4 gateDirection) =>
		gateDirection switch {
			Direction4.Down => Const.ONEWAY_DOWN_TAG,
			Direction4.Up => Const.ONEWAY_UP_TAG,
			Direction4.Left => Const.ONEWAY_LEFT_TAG,
			Direction4.Right => Const.ONEWAY_RIGHT_TAG,
			_ => Const.ONEWAY_UP_TAG,
		};


		public static bool IsOnewayTag (int tag) => tag == Const.ONEWAY_UP_TAG || tag == Const.ONEWAY_DOWN_TAG || tag == Const.ONEWAY_LEFT_TAG || tag == Const.ONEWAY_RIGHT_TAG;


		public static void DrawSegmentHealthBar (int x, int y, int heartLeftCode, int heartRightCode, int emptyHeartLeftCode, int emptyHeartRightCode, int dropParticleID, int hp, int maxHP, int prevHP = int.MinValue) {

			const int SIZE = Const.HALF;
			const int COLUMN = 4;
			const int MAX = 8;

			int maxHp = Mathf.Min(maxHP, MAX);
			int left = x - SIZE * COLUMN / 4;

			// Draw Hearts
			var rect = new RectInt(0, 0, SIZE / 2, SIZE);
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


		public static Vector2Int GetFlyingFormation (Vector2Int pos, int column, int instanceIndex) {

			int sign = instanceIndex % 2 == 0 ? -1 : 1;
			int _row = instanceIndex / 2 / column;
			int _column = (instanceIndex / 2 % column + 1) * sign;
			int rowSign = (_row % 2 == 0) == (sign == 1) ? 1 : -1;

			int instanceOffsetX = _column * Const.CEL * 3 / 2 + rowSign * Const.HALF / 2;
			int instanceOffsetY = _row * Const.CEL + Const.CEL - _column.Abs() * Const.HALF / 3;

			return new(pos.x + instanceOffsetX, pos.y + instanceOffsetY);
		}


		public static Cell DrawShadow (int id, RectInt rect) => CellRenderer.Draw(
			id, rect.Shift(-Const.HALF / 2, 0),
			new Color32(0, 0, 0, 64), -64 * 1024 + 16
		);
		public static Cell DrawShadow (int id, Cell positionCell) => CellRenderer.Draw(
			id, positionCell.X - Const.HALF / 2, positionCell.Y,
			(int)(positionCell.PivotX * 1000),
			(int)(positionCell.PivotY * 1000), 0,
			positionCell.Width, positionCell.Height,
			new Color32(0, 0, 0, 64), -64 * 1024 + 16
		);


		public static int GetItemTypeIcon (int itemID) {
			int typeIcon = ITEM_TYPE_ICONS[^1];
			if (ItemSystem.IsEquipment(itemID, out var equipmentType)) {
				// Equipment
				typeIcon = ITEM_TYPE_ICONS[(int)equipmentType];
			} else if (ItemSystem.IsFood(itemID)) {
				// Food
				typeIcon = ITEM_TYPE_ICONS[^2];
			}
			return typeIcon;
		}


		public static string GetGameKeyDisplayName (Gamekey key) => Util.GetKeyDisplayName(FrameInput.GetKeyboardMap(key));


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


		public static int RandomInt (int min = int.MinValue, int max = int.MaxValue) => GlobalRandom.Next(min, max);


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
					targetX -= (int)(Mathf.Cos(targetRotation * Mathf.Deg2Rad) * targetWidth);
					targetY += (int)(Mathf.Sin(targetRotation * Mathf.Deg2Rad) * targetWidth);
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
			targetX = parentX - (int)(Mathf.Sin(parentRotation * Mathf.Deg2Rad) * parentHeight);
			targetY = parentY - (int)(Mathf.Cos(parentRotation * Mathf.Deg2Rad) * parentHeight);
			targetPivotX = rotation > 0 != bigRot ? 1000 : 0;
			if (parentWidth < 0 != targetPivotX > 500) {
				int pWidth = parentWidth.Abs();
				int sign = targetPivotX > 500 ? -1 : 1;
				targetX -= (int)(Mathf.Cos(parentRotation * Mathf.Deg2Rad) * pWidth) * sign;
				targetY += (int)(Mathf.Sin(parentRotation * Mathf.Deg2Rad) * pWidth) * sign;
			}

			// Flip
			if (targetPivotX > 500) {
				targetPivotX = 1000 - targetPivotX;
				if (bigRot || useFlip) {
					targetWidth = -targetWidth;
				} else {
					targetX -= (int)(Mathf.Cos(targetRotation * Mathf.Deg2Rad) * targetWidth);
					targetY += (int)(Mathf.Sin(targetRotation * Mathf.Deg2Rad) * targetWidth);
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


		public static void DrawPoseCharacterAsUI (RectInt rect, Character character, int animationFrame) {

			// Draw Player
			int oldLayerIndex = CellRenderer.CurrentLayerIndex;
			CellRenderer.SetLayerToUI();
			int layerIndex = CellRenderer.CurrentLayerIndex;
			int cellIndexStart = CellRenderer.GetUsedCellCount();
			character.CurrentAnimationFrame = animationFrame;
			character.FrameUpdate();
			int cellIndexEnd = CellRenderer.GetUsedCellCount();
			CellRenderer.SetLayer(oldLayerIndex);
			if (cellIndexStart == cellIndexEnd) return;
			if (!CellRenderer.GetCells(layerIndex, out var cells, out int count)) return;

			// Get Min Max
			bool flying = character.AnimatedPoseType == CharacterPoseAnimationType.Fly;
			int originalMinX = character.X - Const.HALF - 16;
			int originalMinY = character.Y - 16 + (flying ? character.PoseRootY / 2 : 0);
			int originalMaxX = character.X + Const.HALF + 16;
			int originalMaxY = character.Y + Const.CEL * 2 + 16;
			if (flying) {
				originalMinY -= Const.HALF;
				originalMaxY -= Const.HALF;
			}

			// Move Cells
			int originalWidth = originalMaxX - originalMinX;
			int originalHeight = originalMaxY - originalMinY;
			var targetRect = rect.Fit(originalWidth, originalHeight, 500, 0);
			for (int i = cellIndexStart; i < count && i < cellIndexEnd; i++) {
				var cell = cells[i];
				cell.X = targetRect.x + (cell.X - originalMinX) * targetRect.width / originalWidth;
				cell.Y = targetRect.y + (cell.Y - originalMinY) * targetRect.height / originalHeight;
				cell.Width = cell.Width * targetRect.width / originalWidth;
				cell.Height = cell.Height * targetRect.height / originalHeight;
				if (!cell.Shift.IsZero) {
					cell.Shift = new Int4(
						cell.Shift.Left * targetRect.width / originalWidth,
						cell.Shift.Right * targetRect.width / originalWidth,
						cell.Shift.Down * targetRect.height / originalHeight,
						cell.Shift.Up * targetRect.height / originalHeight
					);
				}
			}

		}


		// Json Meta
		public static T LoadOrCreateJson<T> (string rootPath, string name = "") where T : class, new() {
			var result = LoadJson<T>(rootPath, name);
			if (result == null) {
				result = new T();
				if (result is ISerializationCallbackReceiver ser) {
					ser.OnAfterDeserialize();
				}
			}
			return result;
		}


		public static bool OverrideJson<T> (string rootPath, T target, string name = "") where T : class {
			if (target == null) return false;
			try {
				string jsonPath = GetJsonMetaPath<T>(rootPath, name);
				if (Util.FileExists(jsonPath)) {
					var data = Util.FileToText(jsonPath, Encoding.UTF8);
					JsonUtility.FromJsonOverwrite(data, target);
					return true;
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
			return false;
		}


		public static T LoadJson<T> (string rootPath, string name = "") where T : class {
			try {
				string jsonPath = GetJsonMetaPath<T>(rootPath, name);
				if (Util.FileExists(jsonPath)) {
					var data = Util.FileToText(jsonPath, Encoding.UTF8);
					var target = JsonUtility.FromJson<T>(data);
					if (target != null) return target;
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
			return null;
		}


		public static void SaveJson<T> (T meta, string rootPath, string name = "", bool prettyPrint = false) {
			string jsonPath = GetJsonMetaPath<T>(rootPath, name);
			string data = JsonUtility.ToJson(meta, prettyPrint);
			Util.TextToFile(data, jsonPath, Encoding.UTF8);
		}


		public static string GetJsonMetaPath<T> (string rootPath, string name = "") => Util.CombinePaths(rootPath, $"{(string.IsNullOrEmpty(name) ? typeof(T).Name : name)}.json");


		// Extension
		public static bool IsHorizontal (this Direction4 dir) => dir == Direction4.Left || dir == Direction4.Right;
		public static bool IsVertical (this Direction4 dir) => dir == Direction4.Down || dir == Direction4.Up;


		public static Direction4 Opposite (this Direction4 dir) => dir switch {
			Direction4.Down => Direction4.Up,
			Direction4.Up => Direction4.Down,
			Direction4.Left => Direction4.Right,
			Direction4.Right => Direction4.Left,
			_ => throw new System.NotImplementedException(),
		};


		public static Direction3 Opposite (this Direction3 dir) => dir switch {
			Direction3.Down => Direction3.Up,
			Direction3.Up => Direction3.Down,
			Direction3.None => Direction3.None,
			_ => throw new System.NotImplementedException(),
		};


		public static Direction4 Clockwise (this Direction4 dir) => dir switch {
			Direction4.Down => Direction4.Left,
			Direction4.Left => Direction4.Up,
			Direction4.Up => Direction4.Right,
			Direction4.Right => Direction4.Down,
			_ => throw new System.NotImplementedException(),
		};


		public static Direction4 AntiClockwise (this Direction4 dir) => dir switch {
			Direction4.Down => Direction4.Right,
			Direction4.Right => Direction4.Up,
			Direction4.Up => Direction4.Left,
			Direction4.Left => Direction4.Down,
			_ => throw new System.NotImplementedException(),
		};


		public static Vector2Int Normal (this Direction4 dir) => dir switch {
			Direction4.Down => new(0, -1),
			Direction4.Up => new(0, 1),
			Direction4.Left => new(-1, 0),
			Direction4.Right => new(1, 0),
			_ => throw new System.NotImplementedException(),
		};


		public static RectInt Edge (this RectInt rect, Direction4 edge, int thickness = 1) => edge switch {
			Direction4.Up => rect.Shrink(0, 0, rect.height, -thickness),
			Direction4.Down => rect.Shrink(0, 0, -thickness, rect.height),
			Direction4.Left => rect.Shrink(-thickness, rect.width, 0, 0),
			Direction4.Right => rect.Shrink(rect.width, -thickness, 0, 0),
			_ => throw new System.NotImplementedException(),
		};
		public static Rect Edge (this Rect rect, Direction4 edge, float thickness = 1f) => edge switch {
			Direction4.Up => rect.Shrink(0, 0, rect.height, -thickness),
			Direction4.Down => rect.Shrink(0, 0, -thickness, rect.height),
			Direction4.Left => rect.Shrink(-thickness, rect.width, 0, 0),
			Direction4.Right => rect.Shrink(rect.width, -thickness, 0, 0),
			_ => throw new System.NotImplementedException(),
		};


		public static int GetRotation (this Direction4 dir) => dir switch {
			Direction4.Up => 0,
			Direction4.Down => 180,
			Direction4.Left => -90,
			Direction4.Right => 90,
			_ => 0,
		};


		public static bool AnyButtonPressed (this Gamepad pad) {
			if (pad[GamepadButton.DpadUp].isPressed) return true;
			if (pad[GamepadButton.DpadDown].isPressed) return true;
			if (pad[GamepadButton.DpadLeft].isPressed) return true;
			if (pad[GamepadButton.DpadRight].isPressed) return true;
			if (pad[GamepadButton.North].isPressed) return true;
			if (pad[GamepadButton.East].isPressed) return true;
			if (pad[GamepadButton.South].isPressed) return true;
			if (pad[GamepadButton.West].isPressed) return true;
			if (pad[GamepadButton.LeftStick].isPressed) return true;
			if (pad[GamepadButton.RightStick].isPressed) return true;
			if (pad[GamepadButton.LeftShoulder].isPressed) return true;
			if (pad[GamepadButton.RightShoulder].isPressed) return true;
			if (pad[GamepadButton.Start].isPressed) return true;
			if (pad[GamepadButton.Select].isPressed) return true;
			if (pad[GamepadButton.LeftTrigger].isPressed) return true;
			if (pad[GamepadButton.RightTrigger].isPressed) return true;
			var stickL = pad.leftStick;
			var stickR = pad.rightStick;
			return
				stickL.left.isPressed || stickR.left.isPressed ||
				stickL.right.isPressed || stickR.right.isPressed ||
				stickL.down.isPressed || stickR.down.isPressed ||
				stickL.up.isPressed || stickR.up.isPressed;
		}
		public static bool AnyButtonHolding (this Gamepad pad, out GamepadButton button) {
			button = GamepadButton.DpadUp;
			for (int i = 0; i < 16; i++) {
				if (i == 14) i = 0x20;
				if (i == 15) i = 33;
				var btn = (GamepadButton)i;
				if (pad[btn].isPressed) {
					button = btn;
					return true;
				}
			}
			var stickL = pad.leftStick;
			var stickR = pad.rightStick;
			if (stickL.left.isPressed || stickR.left.isPressed) {
				button = GamepadButton.DpadLeft;
				return true;
			}
			if (stickL.right.isPressed || stickR.right.isPressed) {
				button = GamepadButton.DpadRight;
				return true;
			}
			if (stickL.down.isPressed || stickR.down.isPressed) {
				button = GamepadButton.DpadDown;
				return true;
			}
			if (stickL.up.isPressed || stickR.up.isPressed) {
				button = GamepadButton.DpadUp;
				return true;
			}
			return false;
		}
		public static bool AnyKeyPressed (this Keyboard keyboard) => keyboard.anyKey.isPressed;
		public static bool AnyKeyHolding (this Keyboard keyboard, out Key key) {
			key = Key.None;
			if (keyboard.anyKey.isPressed) {
				var allKeys = keyboard.allKeys;
				for (int i = 0; i < allKeys.Count; i++) {
					var k = allKeys[i];
					if (k.isPressed) {
						key = k.keyCode;
						break;
					}
				}
				return true;
			}
			return false;
		}


		public static int ToUnit (this int globalPos) => globalPos.UDivide(Const.CEL);
		public static int ToGlobal (this int unitPos) => unitPos * Const.CEL;
		public static int ToUnifyGlobal (this int globalPos) => globalPos.UDivide(Const.CEL) * Const.CEL;


		public static Vector2Int ToUnit (this Vector2Int globalPos) => globalPos.UDivide(Const.CEL);
		public static Vector2Int ToGlobal (this Vector2Int unitPos) => unitPos * Const.CEL;
		public static Vector2Int ToUnifyGlobal (this Vector2Int globalPos) => globalPos.ToUnit().ToGlobal();

		public static Vector3Int ToUnit (this Vector3Int globalPos) => new(
			globalPos.x.UDivide(Const.CEL),
			globalPos.y.UDivide(Const.CEL),
			globalPos.z
		);
		public static Vector3Int ToGlobal (this Vector3Int unitPos) => new(
			unitPos.x * Const.CEL,
			unitPos.y * Const.CEL,
			unitPos.z
		);
		public static Vector3Int ToUnifyGlobal (this Vector3Int globalPos) => new(
			globalPos.x.ToUnit().ToGlobal(),
			globalPos.y.ToUnit().ToGlobal(),
			globalPos.z
		);


		public static RectInt ToUnit (this RectInt global) => global.UDivide(Const.CEL);
		public static RectInt ToGlobal (this RectInt unit) => new(
			unit.x * Const.CEL,
			unit.y * Const.CEL,
			unit.width * Const.CEL,
			unit.height * Const.CEL
		);
		public static RectInt ToUnifyGlobal (this RectInt global) => global.ToUnit().ToGlobal();


		// Map Editor
		public static string GetTileRuleString (int groupID) {
			if (!CellRenderer.HasSpriteGroup(groupID, out int groupLength)) return "";
			var builder = new StringBuilder();
			for (int i = 0; i < groupLength; i++) {
				if (CellRenderer.TryGetSpriteFromGroup(groupID, i, out var sp, false, true)) {
					if (CellRenderer.TryGetMeta(sp.GlobalID, out var meta)) {
						int ruleDigit = meta.Rule;
						builder.Append(RuleDigitToString(ruleDigit));
					} else {
						builder.Append(RuleDigitToString(0));
					}
				} else {
					builder.Append(RULE_TILE_ERROR);
				}
			}
			return builder.ToString();
		}


		public static string RuleDigitToString (int digit) {
			// ↖↑↗←→↙↓↘,... 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty NaN=Error
			// eg: "02022020,11111111,01022020,02012020,..."
			// digit: int32 10000000 000 000 000 000 000 000 000 000
			//              01234567 890 123 456 789 012 345 678 901
			//                         1             2            3
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
			//                       000        001        010           011       100
			// eg: "02022020,11111111,01022020,02012020,..."
			// digit: int32 10000000 000 000 000 000 000 000 000 000
			//              01234567 890 123 456 789 012 345 678 901
			//                         1             2            3
			if (string.IsNullOrEmpty(str) || str.Length < 8) return 0;
			int digit = 0;
			digit.SetBitValue(0, true);
			for (int i = 0; i < 8; i++) {
				char c = str[i];
				if (c < '0' || c > '9') continue;
				int strNumber = str[i] - '0';
				digit.SetBitValue(8 + i * 3 + 0, (strNumber / 4) % 2 == 1);
				digit.SetBitValue(8 + i * 3 + 1, (strNumber / 2) % 2 == 1);
				digit.SetBitValue(8 + i * 3 + 2, (strNumber / 1) % 2 == 1);
			}
			return digit;
		}


		public static int GetRuleIndex (
			string rule, int ruleID,
			int tl0, int tm0, int tr0, int ml0, int mr0, int bl0, int bm0, int br0,
			int tl1, int tm1, int tr1, int ml1, int mr1, int bl1, int bm1, int br1
		) {
			// 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty
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
				if (resultIndex != lastIndex) resultIndex = Random.Range(resultIndex, lastIndex + 1);
				return resultIndex;
			}
		}


		public static void DeleteAllEmptyMaps (string mapRoot) {
			var world = new World();
			foreach (var path in Util.EnumerateFiles(mapRoot, false, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					if (!world.LoadFromDisk(path)) continue;
					if (world.EmptyCheck()) {
						Util.DeleteFile(path);
						Util.DeleteFile(path + ".meta");
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
		}


		public static string AngeName (this System.Type type) {
			string name = type.Name;
			if (char.IsLower(name[0])) name = name[1..];
			return name;
		}


		// AngeliA Hash Code
		public static int AngeHash (this System.Type type) => type.AngeName().AngeHash();


		public static int AngeHash (this string str, int avoid = 0) {
			const int p = 31;
			const int m = 1837465129;
			int hash_value = 0;
			int p_pow = 1;
			foreach (var c in str) {
				hash_value = (hash_value + (c - 'a' + 1) * p_pow) % m;
				p_pow = (p_pow * p) % m;
			}
			return hash_value == avoid ? avoid + 1 : hash_value;
		}


	}
}