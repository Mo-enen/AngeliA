using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AngeliA;

public static partial class FrameworkUtil {


	// VAR
	public const int RUN_CODE_ANALYSIS_SETTING_ID = 895245367;
	public const int RUN_CODE_ANALYSIS_SETTING_SILENTLY_ID = 895245368;
	public static readonly Dictionary<GamepadKey, int> GAMEPAD_CODE = new() {
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


	// API
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


	public static void Time01_to_TimeDigit (float time01, out int hour, out int minute, out int second) {
		hour = (int)(time01 * 24f).UMod(24f);
		minute = (int)(hour * 24f * 60f).UMod(60f);
		second = (int)(hour * 24f * 60f * 60f).UMod(60f);
	}


	public static float TimeDigit_to_Time01 (int hour, int minute, int second) => ((hour + (minute + second / 60f) / 60f) / 24f).UMod(1f);


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
	public static bool MouseInside (this IRect rect) => Game.CursorInScreen && rect.Contains(Input.MouseGlobalPosition);


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


	public static HandTool GetPlayerHoldingHandTool () {
		if (PlayerSystem.Selecting == null) return null;
		int id = Inventory.GetEquipment(PlayerSystem.Selecting.InventoryID, EquipmentType.HandTool, out _);
		if (id == 0) return null;
		return ItemSystem.GetItem(id) as HandTool;
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


}