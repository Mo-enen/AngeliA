using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AngeliA;

/// <summary>
/// Utility class for the AngeliA framework
/// </summary>
public static partial class FrameworkUtil {


	// VAR
	/// <summary>
	/// For remote setting between engine and rigged game
	/// </summary>
	public const int RUN_CODE_ANALYSIS_SETTING_ID = 895245367;
	/// <summary>
	/// For remote setting between engine and rigged game
	/// </summary>
	public const int RUN_CODE_ANALYSIS_SETTING_SILENTLY_ID = 895245368;
	/// <summary>
	/// ID of artwork sprite for gamepad button icons
	/// </summary>
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
	/// <summary>
	/// Get name of a map block without hashtags
	/// </summary>
	public static string GetBlockRealName (string name) {
		int hashIndex = name.IndexOf('#');
		if (hashIndex >= 0) {
			name = name[..hashIndex];
		}
		return name.TrimEnd(' ');
	}

	/// <summary>
	/// Get tag for oneway gate that facing to given direction
	/// </summary>
	public static Tag GetOnewayTag (Direction4 gateDirection) => gateDirection switch {
		Direction4.Down => Tag.OnewayDown,
		Direction4.Up => Tag.OnewayUp,
		Direction4.Left => Tag.OnewayLeft,
		Direction4.Right => Tag.OnewayRight,
		_ => Tag.OnewayUp,
	};

	/// <summary>
	/// True if the given tag contains oneway gate tag.
	/// </summary>
	public static bool HasOnewayTag (Tag tag) => tag.HasAny(Tag.OnewayUp | Tag.OnewayDown | Tag.OnewayLeft | Tag.OnewayRight);

	/// <summary>
	/// Get direction from a single oneway gate tag. The tag value can only be a single oneway gate tag.
	/// </summary>
	public static bool TryGetOnewayDirection (Tag tag, out Direction4 direction) {
		switch (tag) {
			case Tag.OnewayUp:
				direction = Direction4.Up;
				return true;
			case Tag.OnewayDown:
				direction = Direction4.Down;
				return true;
			case Tag.OnewayLeft:
				direction = Direction4.Left;
				return true;
			case Tag.OnewayRight:
				direction = Direction4.Right;
				return true;
		}
		direction = default;
		return false;
	}

	/// <summary>
	/// Get position for flying entities
	/// </summary>
	/// <param name="pos">Center position</param>
	/// <param name="column">Column of the current element</param>
	/// <param name="instanceIndex">Local index of the current element</param>
	/// <returns>Final position of this element</returns>
	public static Int2 GetFlyingFormation (Int2 pos, int column, int instanceIndex) {

		int sign = instanceIndex % 2 == 0 ? -1 : 1;
		int _row = instanceIndex / 2 / column;
		int _column = (instanceIndex / 2 % column + 1) * sign;
		int rowSign = (_row % 2 == 0) == (sign == 1) ? 1 : -1;

		int instanceOffsetX = _column * Const.CEL * 3 / 2 + rowSign * Const.HALF / 2;
		int instanceOffsetY = _row * Const.CEL + Const.CEL - _column.Abs() * Const.HALF / 3;

		return new(pos.x + instanceOffsetX, pos.y + instanceOffsetY);
	}

	/// <summary>
	/// Get infomation from naming tag of an artwork sprite
	/// </summary>
	/// <param name="name">Full name of the artwork sprite</param>
	/// <param name="realName">Name without hashtag</param>
	/// <param name="isTrigger">True if this sprite is trigger</param>
	/// <param name="tag">Tag value of this sprite</param>
	/// <param name="rule">Rule for auto tiling in map editor</param>
	/// <param name="noCollider">True if this sprite ignore collider</param>
	/// <param name="offsetZ">Z value for sort rendering cells</param>
	/// <param name="aniDuration">Duration in frame for animation</param>
	/// <param name="pivotX"></param>
	/// <param name="pivotY"></param>
	public static void GetSpriteInfoFromName (string name, out string realName, out bool isTrigger, out Tag tag, out BlockRule rule, out bool noCollider, out int offsetZ, out int aniDuration, out int? pivotX, out int? pivotY) {
		tag = Tag.None;
		isTrigger = false;
		rule = BlockRule.EMPTY;
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
				for (int i = 1; i < TagUtil.ALL_TAG_NAMES.Length; i++) {
					string _tag = TagUtil.ALL_TAG_NAMES[i];
					if (hashTag.Equals(_tag, OIC)) {
						tag |= (Tag)(1 << (i - 1));
						tagFinded = true;
						break;
					}
				}
				if (tagFinded) continue;

				// Bool-Group
				if (hashTag.Equals("loopStart", OIC)) {
					tag |= Tag.LoopStart;
					continue;
				}

				if (hashTag.Equals("noCollider", OIC) || hashTag.Equals("ignoreCollider", OIC)) {
					noCollider = true;
					continue;
				}

				if (hashTag.Equals("random", OIC) || hashTag.Equals("ran", OIC)) {
					tag |= Tag.Random;
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
					string _tagStr = hashTag[4..];
					int _tagIndex = System.Array.FindIndex(TagUtil.ALL_TAG_NAMES, _t => _t.Equals(_tagStr, System.StringComparison.OrdinalIgnoreCase));
					if (_tagIndex >= 0) {
						tag |= TagUtil.GetTagAt(_tagIndex);
					}
					continue;
				}

				if (hashTag.StartsWith("rule=", OIC)) {
					string ruleStr = hashTag[5..];
					for (int i = 0; i < 8; i++) {
						rule[i] = (Rule)(ruleStr[i] - '0');
					}
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
HasOnewayTag(tag) ||
			tag.HasAll(Tag.Water);

		// Name
		realName = name.TrimEnd(' ');

	}

	/// <summary>
	/// Get sprite group infomation from the name of artwork sprite
	/// </summary>
	/// <param name="realName">Name without hashtags</param>
	/// <param name="groupName">Name without index</param>
	/// <param name="groupIndex">Index in group</param>
	/// <returns>True if the data successfuly calculated</returns>
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

	/// <summary>
	/// Scale the given audio volume
	/// </summary>
	/// <param name="volume"></param>
	/// <param name="scale">0 means 0%, 1000 means 100%</param>
	public static float GetScaledAudioVolume (int volume, int scale = 1000) {
		float fVolume = volume / 1000f;
		if (scale != 1000) fVolume *= scale / 1000f;
		return fVolume * fVolume;
	}

	/// <summary>
	/// Reset the shoulder and upper arm position for given pose-styled character
	/// </summary>
	/// <param name="rendering">Target character</param>
	/// <param name="resetLeft"></param>
	/// <param name="resetRight"></param>
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

	/// <summary>
	/// Blink the given cell for action target highlighting
	/// </summary>
	/// <param name="cell"></param>
	/// <param name="pivotX"></param>
	/// <param name="pivotY"></param>
	/// <param name="horizontal">True if the target attached with nearby entity in left or right</param>
	/// <param name="vertical">True if the target attached with nearby entity in up or down</param>
	public static void HighlightBlink (Cell cell, float pivotX = 0.5f, float pivotY = 0f, bool horizontal = true, bool vertical = true) {
		if (Game.GlobalFrame % 30 > 15) return;
		const int OFFSET = Const.CEL / 20;
		cell.ReturnPivots(pivotX, pivotY);
		if (horizontal) {
			cell.Width = cell.Width > 0 ? cell.Width + OFFSET * 2 : cell.Width - OFFSET * 2;
		}
		if (vertical) {
			cell.Height = cell.Height > 0 ? cell.Height + OFFSET * 2 : cell.Height - OFFSET * 2;
		}
	}

	/// <summary>
	/// Convert a float value into time
	/// </summary>
	/// <param name="time01">0 means 0:00, 0.5 means 12:00. 1 means 24:00</param>
	/// <param name="hour"></param>
	/// <param name="minute"></param>
	/// <param name="second"></param>
	public static void Time01_to_TimeDigit (float time01, out int hour, out int minute, out int second) {
		hour = (int)(time01 * 24f).UMod(24f);
		minute = (int)(hour * 24f * 60f).UMod(60f);
		second = (int)(hour * 24f * 60f * 60f).UMod(60f);
	}

	/// <summary>
	/// Convert time into a float value
	/// </summary>
	/// <param name="hour"></param>
	/// <param name="minute"></param>
	/// <param name="second"></param>
	/// <returns>0 means 0:00, 0.5 means 12:00. 1 means 24:00</returns>
	public static float TimeDigit_to_Time01 (int hour, int minute, int second) => ((hour + (minute + second / 60f) / 60f) / 24f).UMod(1f);

	/// <summary>
	/// Bounce entities on stage for once
	/// </summary>
	/// <param name="springEntity">Entity that exists as the spring</param>
	/// <param name="direction">Direction that the spring power goes</param>
	/// <param name="power">Initial speed for entities get bounced</param>
	/// <param name="powerSide">Initial speed on side direction for entities get bounced</param>
	/// <returns>True if any entity get bounced</returns>
	public static bool PerformSpringBounce (Entity springEntity, Direction4 direction, int power, int powerSide = 0) {
		bool bounced = false;
		var globalRect = springEntity.Rect.EdgeOutside(direction, 16);
		for (int safe = 0; safe < 2048; safe++) {
			var hits = Physics.OverlapAll(PhysicsMask.DYNAMIC, globalRect, out int count, springEntity, OperationMode.ColliderAndTrigger);
			if (count == 0) break;
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not Rigidbody rig) continue;
				bounced = true;
				var hitRect = hit.Entity.Rect;
				if (direction.IsHorizontal()) {
					globalRect.y = hitRect.y;
					if (direction == Direction4.Left) {
						globalRect.x = Util.Min(globalRect.x, hitRect.x - globalRect.width);
					} else {
						globalRect.x = Util.Max(globalRect.x, hitRect.xMax);
					}
				} else {
					globalRect.x = hitRect.x;
					globalRect.y = Util.Max(globalRect.y, hitRect.yMax);
				}
				PerformSpringBounce(rig, springEntity, direction, power, powerSide);
				springEntity = hit.Entity;
				break;
			}
		}
		return bounced;
	}

	/// <summary>
	/// Bounce the given target for once
	/// </summary>
	/// <param name="target">Target to get bounce</param>
	/// <param name="spring">Entity that exists as the spring</param>
	/// <param name="direction">Direction that the spring power goes</param>
	/// <param name="power">Initial speed for entities get bounced</param>
	/// <param name="powerSide">Initial speed on side direction for entities get bounced</param>
	public static void PerformSpringBounce (Rigidbody target, Entity spring, Direction4 direction, int power, int powerSide = 0) {
		if (target == null) return;
		if (direction.IsHorizontal()) {
			// Horizontal
			if (direction == Direction4.Left) {
				if (target.VelocityX > -power) {
					//target.VelocityX = -power;
					target.MomentumX = (-power, 1);
					if (target is IWithCharacterMovement wMov) {
						wMov.CurrentMovement.FacingRight = false;
					}
				}
			} else {
				if (target.VelocityX < power) {
					//target.VelocityX = power;
					target.MomentumX = (power, 1);
					if (target is IWithCharacterMovement wMov) {
						wMov.CurrentMovement.FacingRight = true;
					}
				}
			}
			// Side
			if (target.VelocityY.Abs() < powerSide) {
				target.VelocityY = powerSide * (target.Rect.CenterY() > spring.Rect.CenterY() ? 1 : -1);
			}
		} else {
			// Vertical
			if (target.VelocityY < power) target.VelocityY = power;
			target.MakeGrounded(6);
			// Side
			if (target.VelocityX.Abs() < powerSide) {
				//target.VelocityX = powerSide * (target.Rect.CenterX() > spring.Rect.CenterX() ? 1 : -1);
				target.MomentumX = (powerSide * (target.Rect.CenterX() > spring.Rect.CenterX() ? 1 : -1), 1);
			}
		}
	}

	/// <summary>
	/// Try get buff from map element at given unit position
	/// </summary>
	public static void GiveBuffFromMap (IWithCharacterBuff wBuff, int unitX = int.MinValue, int unitY = int.MinValue, int unitZ = int.MinValue, int duration = -1) {
		if (wBuff is not Entity entity) return;
		unitX = unitX == int.MinValue ? (entity.X + 1).ToUnit() : unitX;
		unitY = unitY == int.MinValue ? (entity.Y + 1).ToUnit() : unitY;
		unitZ = unitZ == int.MinValue ? Stage.ViewZ : unitZ;
		int id = WorldSquad.Front.GetBlockAt(unitX, unitY, unitZ, BlockType.Element);
		if (id == 0) return;
		wBuff.CurrentBuff.GiveBuff(id, duration);
	}


	// FrameBasedValue Load/Save
	internal static bool NameAndIntFile_to_List (List<(string name, int value)> list, string path) {
		if (!Util.FileExists(path)) return false;
		list.AddRange(Util.ForAllNameAndIntInFile(path));
		return true;
	}


	internal static bool List_to_FrameBasedFields (List<(string name, int value)> list, object target) {
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


	internal static void FrameBasedFields_to_List (object target, List<(string name, int value)> list) {
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


	internal static void Pairs_to_NameAndIntFile (IEnumerable<KeyValuePair<string, int>> list, string path) {
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


	// Item
	/// <summary>
	/// Spawn item based on items list from map. This is used for the map chest.
	/// </summary>
	/// <param name="squad">Source of the map blocks</param>
	/// <param name="unitX">Position X in unit space</param>
	/// <param name="unitY">Position Y in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="maxDeltaX">Limitation on horizontal checking distance</param>
	/// <param name="maxDeltaY">Limitation on verticle checking distance</param>
	/// <param name="placeHolderID">Set spawned item into this ID</param>
	/// <param name="spawnEntity">True if spawn the entity that paint as map element</param>
	public static void SpawnItemFromMap (
		IBlockSquad squad, int unitX, int unitY, int z,
		int maxDeltaX = 1024, int maxDeltaY = 1024, int placeHolderID = 0, bool spawnEntity = true
	) {
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
			int blockID = squad.GetBlockAt(unitX + itemLocalIndex, currentUnitY, z, BlockType.Element);
			bool requirePlaceHolding = false;
			if (spawnEntity && Stage.IsValidEntityID(blockID)) {
				// As Entity
				var entity = Stage.SpawnEntity(blockID, unitX.ToGlobal(), unitY.ToGlobal());
				if (entity != null) {
					requirePlaceHolding = true;
				}
			} else if (ItemSystem.HasItem(blockID)) {
				// As Item
				if (ItemSystem.SpawnItem(blockID, unitX.ToGlobal(), unitY.ToGlobal(), 1, true) != null) {
					requirePlaceHolding = true;
				}
			}
			// Placeholding
			if (requirePlaceHolding && placeHolderID != 0) {
				squad.SetBlockAt(unitX + itemLocalIndex, currentUnitY, z, BlockType.Element, placeHolderID);
			}
		}
	}


	/// <summary>
	/// Get global single instance of the handtool that player currently equipping
	/// </summary>
	/// <returns></returns>
	public static HandTool GetPlayerHoldingHandTool () {
		if (PlayerSystem.Selecting == null) return null;
		int id = Inventory.GetEquipment(PlayerSystem.Selecting.InventoryID, EquipmentType.HandTool, out _);
		if (id == 0) return null;
		return ItemSystem.GetItem(id) as HandTool;
	}


	// Buff
	/// <summary>
	/// Give buff for all buff holder in given rectangle range
	/// </summary>
	public static void BroadcastBuff (IRect range, int buffID, int duration = 1) {
		var hits = Physics.OverlapAll(PhysicsMask.CHARACTER, range, out int count);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not Character character) continue;
			character.Buff.GiveBuff(buffID, duration);
		}
	}

	/// <summary>
	/// Give buff for all buff holder in given circle range
	/// </summary>
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
	/// <summary>
	/// Perform checking logic for checking built-in sprite sync with artwork sheet or not
	/// </summary>
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

	/// <summary>
	/// Check for empty script file in given project root
	/// </summary>
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

	/// <summary>
	/// Perform analyses for current AngeliA project and log the report
	/// </summary>
	public static void RunAngeliaCodeAnalysis (bool onlyLogWhenWarningFounded = false, bool fixScriptFileName = false, bool checkNoItemCombination = true) {

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
				Debug.Log("✓ No hash collision founded.");
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
				Debug.Log("✓ No first char of file name need to fix.");
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
				Debug.Log("✓ No item class name need to fix.");
			}
		}

		// Check for Item No Combination
		if (checkNoItemCombination) {
			bool anyWarning = false;
			var resultHash = new HashSet<int>();
			foreach (var (_, result) in ItemSystem.ForAllCombinations()) {
				resultHash.Add(result);
			}
			var typeList = new List<(int id, Type type)>();
			foreach (var type in typeof(Item).AllChildClass()) {
				typeList.Add((type.AngeHash(), type));
			}
			foreach (var type in typeof(IBlockEntity).AllClassImplemented()) {
				typeList.Add((type.AngeHash(), type));
			}
			foreach (var (id, type) in typeList) {
				if (resultHash.Contains(id)) continue;
				var nc = type.GetCustomAttributes<NoItemCombinationAttribute>(true);
				if (nc == null || !nc.Any()) {
					Debug.LogWarning($"Item \"{type.AngeName()}\" have no valid combination. Consider add attribute \"[NoItemCombination]\" to the item class.");
					anyWarning = true;
				}
			}
			if (!anyWarning && !onlyLogWhenWarningFounded) {
				Debug.Log("✓ Item combinations are properly labeled.");
			}
		}

	}


}